using RigolStream.Api.Infrastructure;
using RigolStream.Api.Models;
using RigolStream.Api.Scpi;

namespace RigolStream.Api.Devices;

/// <summary>
/// An in-memory oscilloscope used when no hardware is attached. It keeps channel
/// and acquisition state, synthesizes traces with <see cref="SignalGenerator"/>,
/// computes measurements from the samples and renders a real PNG screenshot of
/// the graticule. Registered as a singleton, so all state is shared and guarded
/// by a lock.
/// </summary>
public sealed class SimulatedOscilloscope : IOscilloscope
{
    private const int DefaultPoints = 1200;
    private const int MaxPoints = 100_000;
    private const int Divisions = 10; // horizontal divisions across the screen

    private readonly object _gate = new();
    private readonly SignalGenerator _generator = new(seed: 1337);
    private readonly Dictionary<int, ChannelConfig> _channels = new();
    private readonly Dictionary<int, SignalSpec> _signals = new();
    private AcquisitionState _acq = new();

    // Channel trace colours (Rigol-style), used for the rendered screenshot.
    private static readonly (byte r, byte g, byte b)[] ChannelColours =
    {
        (255, 220, 0),   // CH1 yellow
        (0, 200, 255),   // CH2 cyan
        (255, 64, 200),  // CH3 magenta
        (0, 220, 120),   // CH4 green
    };

    public SimulatedOscilloscope()
    {
        for (int ch = 1; ch <= 4; ch++)
        {
            _channels[ch] = new ChannelConfig
            {
                Channel = ch,
                Enabled = ch <= 2, // CH1/CH2 on by default
                VoltsPerDivision = 0.5,
                Label = $"CH{ch}",
            };
            _signals[ch] = SignalGenerator.DefaultFor(ch);
        }
    }

    public Task<DeviceInfo> GetDeviceInfoAsync(CancellationToken ct = default) =>
        Task.FromResult(DeviceInfo.Parse("RIGOL TECHNOLOGIES,DHO914S,SIM0000000001,00.01.03 (sim)", simulated: true));

    public Task<AcquisitionState> GetAcquisitionStateAsync(CancellationToken ct = default)
    {
        lock (_gate) return Task.FromResult(_acq);
    }

    public Task<IReadOnlyList<ChannelConfig>> GetChannelsAsync(CancellationToken ct = default)
    {
        lock (_gate)
            return Task.FromResult<IReadOnlyList<ChannelConfig>>(
                _channels.Values.OrderBy(c => c.Channel).ToList());
    }

    public Task<ChannelConfig> UpdateChannelAsync(int channel, ChannelUpdate update, CancellationToken ct = default)
    {
        Guard(channel);
        lock (_gate)
        {
            var updated = update.ApplyTo(_channels[channel]);
            _channels[channel] = updated;
            return Task.FromResult(updated);
        }
    }

    public Task<Waveform> GetWaveformAsync(int channel, int? maxPoints = null, CancellationToken ct = default)
    {
        Guard(channel);
        int count = Math.Clamp(maxPoints ?? DefaultPoints, 16, MaxPoints);
        lock (_gate)
        {
            var (origin, dt, drift) = TimeAxis(count);
            var volts = _generator.Sample(_signals[channel], count, origin, dt, drift);
            return Task.FromResult(new Waveform
            {
                Channel = channel,
                Voltage = volts,
                TimeOrigin = origin,
                TimeIncrement = dt,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            });
        }
    }

    public Task<MeasurementSet> GetMeasurementsAsync(int channel, CancellationToken ct = default)
    {
        Guard(channel);
        lock (_gate)
        {
            var (origin, dt, drift) = TimeAxis(DefaultPoints);
            var spec = _signals[channel];
            var v = _generator.Sample(spec, DefaultPoints, origin, dt, drift);
            var stats = Statistics.From(v, spec.FrequencyHz);
            long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var items = MeasurementType.Default
                .Select(t => new Measurement
                {
                    Name = t.Name,
                    Code = t.Code,
                    Unit = t.Unit,
                    Value = stats.Lookup(t.Code),
                })
                .ToList();

            return Task.FromResult(new MeasurementSet { Channel = channel, Items = items, Timestamp = ts });
        }
    }

    public Task<byte[]> GetScreenshotAsync(CancellationToken ct = default)
    {
        lock (_gate)
            return Task.FromResult(RenderScreenshot());
    }

    public Task<AcquisitionState> SetRunStateAsync(RunState state, CancellationToken ct = default)
    {
        lock (_gate)
        {
            _acq = _acq with
            {
                RunState = state,
                TriggerStatus = state switch
                {
                    RunState.Running => "AUTO",
                    RunState.Single => "WAIT",
                    _ => "STOP",
                },
            };
            return Task.FromResult(_acq);
        }
    }

    public Task<AcquisitionState> UpdateAcquisitionAsync(AcquisitionUpdate update, CancellationToken ct = default)
    {
        lock (_gate)
        {
            _acq = _acq with
            {
                SecondsPerDivision = Clamp(update.SecondsPerDivision, _acq.SecondsPerDivision, 1e-9, 50),
                TimebaseOffset = update.TimebaseOffset ?? _acq.TimebaseOffset,
                TriggerSource = update.TriggerSource ?? _acq.TriggerSource,
                TriggerLevel = update.TriggerLevel ?? _acq.TriggerLevel,
                TriggerSlope = update.TriggerSlope ?? _acq.TriggerSlope,
            };
            return Task.FromResult(_acq);
        }
    }

    // --- helpers -----------------------------------------------------------

    private (double origin, double dt, double drift) TimeAxis(int count)
    {
        double window = Divisions * _acq.SecondsPerDivision;
        double origin = -window / 2 + _acq.TimebaseOffset;
        double dt = window / count;
        // Gentle horizontal motion while running; frozen when stopped.
        double drift = _acq.RunState == RunState.Stopped
            ? 0
            : 2 * Math.PI * 0.25 * (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0);
        return (origin, dt, drift);
    }

    private byte[] RenderScreenshot()
    {
        const int w = 800, h = 480;
        var canvas = new PngCanvas(w, h);
        canvas.Clear(10, 10, 20);

        // Graticule: 10 x 8 divisions.
        const int xDiv = 10, yDiv = 8;
        for (int i = 0; i <= xDiv; i++)
        {
            int x = i * (w - 1) / xDiv;
            byte g = (byte)(i == xDiv / 2 ? 90 : 45);
            canvas.DrawLine(x, 0, x, h - 1, g, g, g);
        }
        for (int j = 0; j <= yDiv; j++)
        {
            int y = j * (h - 1) / yDiv;
            byte g = (byte)(j == yDiv / 2 ? 90 : 45);
            canvas.DrawLine(0, y, w - 1, y, g, g, g);
        }

        double pxPerDivY = (double)h / yDiv;
        int centerY = h / 2;

        foreach (var cfg in _channels.Values.Where(c => c.Enabled).OrderBy(c => c.Channel))
        {
            var (origin, dt, drift) = TimeAxis(w);
            var v = _generator.Sample(_signals[cfg.Channel], w, origin, dt, drift);
            var (r, g, b) = ChannelColours[(cfg.Channel - 1) % ChannelColours.Length];

            int prevY = VoltageToY(v[0], cfg, pxPerDivY, centerY);
            for (int x = 1; x < w; x++)
            {
                int y = VoltageToY(v[x], cfg, pxPerDivY, centerY);
                canvas.DrawLine(x - 1, prevY, x, y, r, g, b);
                prevY = y;
            }
        }

        return canvas.Encode();
    }

    private static int VoltageToY(double v, ChannelConfig cfg, double pxPerDivY, int centerY)
    {
        double divisions = (v + cfg.OffsetVolts) / cfg.VoltsPerDivision;
        return (int)Math.Round(centerY - divisions * pxPerDivY);
    }

    private static double Clamp(double? value, double current, double lo, double hi) =>
        value is { } v ? Math.Clamp(v, lo, hi) : current;

    private void Guard(int channel)
    {
        if (channel is < 1 or > 4)
            throw new OscilloscopeException($"Channel {channel} out of range (1-4)", OscilloscopeErrorKind.Command);
    }
}
