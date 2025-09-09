using System.Globalization;
using RigolStream.Api.Models;
using RigolStream.Api.Scpi;

namespace RigolStream.Api.Devices;

/// <summary>
/// Drives a real DHO914S over an <see cref="IScpiTransport"/>. The command surface
/// is the same dialect the Python library uses, so behaviour matches the scripted
/// tooling in this repo.
/// </summary>
public sealed class ScpiOscilloscope : IOscilloscope, IAsyncDisposable
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private readonly IScpiTransport _transport;

    public ScpiOscilloscope(IScpiTransport transport) => _transport = transport;

    public async Task<DeviceInfo> GetDeviceInfoAsync(CancellationToken ct = default)
    {
        var idn = await _transport.QueryAsync(ScpiCommands.Identity, ct);
        return DeviceInfo.Parse(idn, simulated: false);
    }

    public async Task<AcquisitionState> GetAcquisitionStateAsync(CancellationToken ct = default)
    {
        double secDiv = await QueryDoubleAsync(ScpiCommands.TimebaseScaleQuery, ct);
        double offset = await QueryDoubleAsync(ScpiCommands.TimebaseOffsetQuery, ct);
        string status = await _transport.QueryAsync(ScpiCommands.TriggerStatusQuery, ct);

        return new AcquisitionState
        {
            SecondsPerDivision = secDiv,
            TimebaseOffset = offset,
            TriggerStatus = status,
            RunState = status.Equals("STOP", StringComparison.OrdinalIgnoreCase)
                ? RunState.Stopped
                : RunState.Running,
        };
    }

    public async Task<IReadOnlyList<ChannelConfig>> GetChannelsAsync(CancellationToken ct = default)
    {
        var list = new List<ChannelConfig>(4);
        for (int ch = 1; ch <= 4; ch++)
        {
            double scale = await QueryDoubleAsync(string.Format(ScpiCommands.ChannelScaleQuery, ch), ct);
            double offset = await QueryDoubleAsync(string.Format(ScpiCommands.ChannelOffsetQuery, ch), ct);
            double probe = await QueryDoubleAsync(string.Format(ScpiCommands.ChannelProbeQuery, ch), ct);
            string coup = await _transport.QueryAsync(string.Format(ScpiCommands.ChannelCouplingQuery, ch), ct);

            list.Add(new ChannelConfig
            {
                Channel = ch,
                VoltsPerDivision = scale,
                OffsetVolts = offset,
                ProbeRatio = probe,
                Coupling = ParseCoupling(coup),
                Label = $"CH{ch}",
            });
        }
        return list;
    }

    public async Task<ChannelConfig> UpdateChannelAsync(int channel, ChannelUpdate update, CancellationToken ct = default)
    {
        if (update.Enabled is { } en)
            await _transport.WriteAsync(string.Format(ScpiCommands.ChannelEnable, channel, en ? "ON" : "OFF"), ct);
        if (update.VoltsPerDivision is { } scale)
            await _transport.WriteAsync(string.Format(Inv, ScpiCommands.ChannelScale, channel, scale), ct);
        if (update.OffsetVolts is { } off)
            await _transport.WriteAsync(string.Format(Inv, ScpiCommands.ChannelOffset, channel, off), ct);
        if (update.ProbeRatio is { } probe)
            await _transport.WriteAsync(string.Format(Inv, ScpiCommands.ChannelProbe, channel, probe), ct);
        if (update.Coupling is { } coup)
            await _transport.WriteAsync(string.Format(ScpiCommands.ChannelCoupling, channel, FormatCoupling(coup)), ct);

        var channels = await GetChannelsAsync(ct);
        return channels.First(c => c.Channel == channel);
    }

    public async Task<Waveform> GetWaveformAsync(int channel, int? maxPoints = null, CancellationToken ct = default)
    {
        await _transport.WriteAsync(string.Format(ScpiCommands.WaveformSource, $"CHAN{channel}"), ct);
        await _transport.WriteAsync(string.Format(ScpiCommands.WaveformFormat, "BYTE"), ct);
        await _transport.WriteAsync(string.Format(ScpiCommands.WaveformMode, "NORMal"), ct);
        if (maxPoints is { } pts)
            await _transport.WriteAsync(string.Format(ScpiCommands.WaveformPoints, pts), ct);

        var preamble = WaveformPreamble.Parse(await _transport.QueryAsync(ScpiCommands.WaveformPreamble, ct));
        var raw = await _transport.QueryBinaryAsync(ScpiCommands.WaveformData, ct);
        var payload = WaveformMath.StripBlockHeader(raw).Span;
        var volts = WaveformMath.BytesToVoltage(payload, preamble);
        var (origin, increment) = WaveformMath.TimeAxis(preamble);

        return new Waveform
        {
            Channel = channel,
            Voltage = volts,
            TimeOrigin = origin,
            TimeIncrement = increment,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };
    }

    public async Task<MeasurementSet> GetMeasurementsAsync(int channel, CancellationToken ct = default)
    {
        await _transport.WriteAsync(string.Format(ScpiCommands.MeasureSource, $"CHAN{channel}"), ct);
        long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var items = new List<Measurement>();

        foreach (var type in MeasurementType.Default)
        {
            string response = await _transport.QueryAsync(string.Format(ScpiCommands.MeasureItem, type.Code), ct);
            double raw = double.TryParse(response, NumberStyles.Float, Inv, out var v) ? v : double.NaN;
            items.Add(type.ToResult(raw, ts));
        }

        return new MeasurementSet { Channel = channel, Items = items, Timestamp = ts };
    }

    public Task<byte[]> GetScreenshotAsync(CancellationToken ct = default) =>
        _transport.QueryBinaryAsync(ScpiCommands.DisplayData, ct);

    public async Task<AcquisitionState> SetRunStateAsync(RunState state, CancellationToken ct = default)
    {
        string cmd = state switch
        {
            RunState.Running => ScpiCommands.Run,
            RunState.Stopped => ScpiCommands.Stop,
            RunState.Single => ScpiCommands.Single,
            _ => ScpiCommands.Run,
        };
        await _transport.WriteAsync(cmd, ct);
        return await GetAcquisitionStateAsync(ct);
    }

    public async Task<AcquisitionState> UpdateAcquisitionAsync(AcquisitionUpdate update, CancellationToken ct = default)
    {
        if (update.SecondsPerDivision is { } sd)
            await _transport.WriteAsync(string.Format(Inv, ScpiCommands.TimebaseScale, sd), ct);
        if (update.TimebaseOffset is { } to)
            await _transport.WriteAsync(string.Format(Inv, ScpiCommands.TimebaseOffset, to), ct);
        if (update.TriggerSource is { } ts)
            await _transport.WriteAsync(string.Format(ScpiCommands.TriggerSource, $"CHAN{ts}"), ct);
        if (update.TriggerLevel is { } tl)
            await _transport.WriteAsync(string.Format(Inv, ScpiCommands.TriggerLevel, tl), ct);
        if (update.TriggerSlope is { } slope)
            await _transport.WriteAsync(string.Format(ScpiCommands.TriggerSlope, FormatSlope(slope)), ct);

        return await GetAcquisitionStateAsync(ct);
    }

    private async Task<double> QueryDoubleAsync(string command, CancellationToken ct)
    {
        string r = await _transport.QueryAsync(command, ct);
        return double.TryParse(r, NumberStyles.Float, Inv, out var v) ? v : 0.0;
    }

    private static Coupling ParseCoupling(string s) => s.Trim().ToUpperInvariant() switch
    {
        "AC" => Coupling.Ac,
        "GND" => Coupling.Gnd,
        _ => Coupling.Dc,
    };

    private static string FormatCoupling(Coupling c) => c switch
    {
        Coupling.Ac => "AC",
        Coupling.Gnd => "GND",
        _ => "DC",
    };

    private static string FormatSlope(TriggerSlope s) => s switch
    {
        TriggerSlope.Negative => "NEGative",
        TriggerSlope.Either => "RFALl",
        _ => "POSitive",
    };

    public ValueTask DisposeAsync() => _transport.DisposeAsync();
}
