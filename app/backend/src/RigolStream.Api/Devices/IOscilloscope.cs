using RigolStream.Api.Models;

namespace RigolStream.Api.Devices;

/// <summary>
/// Hardware-agnostic contract for the oscilloscope. The HTTP layer only ever
/// talks to this interface, so the same endpoints serve a real DHO914S
/// (<see cref="ScpiOscilloscope"/>) or the built-in <see cref="SimulatedOscilloscope"/>.
/// Implementations must be safe for concurrent reads from multiple requests.
/// </summary>
public interface IOscilloscope
{
    /// <summary>Instrument identity (*IDN?).</summary>
    Task<DeviceInfo> GetDeviceInfoAsync(CancellationToken ct = default);

    /// <summary>Current run/timebase/trigger state.</summary>
    Task<AcquisitionState> GetAcquisitionStateAsync(CancellationToken ct = default);

    /// <summary>Configuration for all four channels.</summary>
    Task<IReadOnlyList<ChannelConfig>> GetChannelsAsync(CancellationToken ct = default);

    /// <summary>Apply a patch to one channel and return the resulting config.</summary>
    Task<ChannelConfig> UpdateChannelAsync(int channel, ChannelUpdate update, CancellationToken ct = default);

    /// <summary>Capture a single-shot trace for one channel.</summary>
    Task<Waveform> GetWaveformAsync(int channel, int? maxPoints = null, CancellationToken ct = default);

    /// <summary>Run the default automatic measurements for one channel.</summary>
    Task<MeasurementSet> GetMeasurementsAsync(int channel, CancellationToken ct = default);

    /// <summary>Grab the display as PNG bytes.</summary>
    Task<byte[]> GetScreenshotAsync(CancellationToken ct = default);

    /// <summary>Change the run state (run/stop/single/force).</summary>
    Task<AcquisitionState> SetRunStateAsync(RunState state, CancellationToken ct = default);

    /// <summary>Update timebase/trigger settings; null fields are left unchanged.</summary>
    Task<AcquisitionState> UpdateAcquisitionAsync(AcquisitionUpdate update, CancellationToken ct = default);
}

/// <summary>Nullable patch for timebase + trigger settings.</summary>
public sealed record AcquisitionUpdate
{
    public double? SecondsPerDivision { get; init; }
    public double? TimebaseOffset { get; init; }
    public int? TriggerSource { get; init; }
    public double? TriggerLevel { get; init; }
    public TriggerSlope? TriggerSlope { get; init; }
}
