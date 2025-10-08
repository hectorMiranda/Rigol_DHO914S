namespace RigolStream.Api.Models;

/// <summary>Vertical coupling modes supported by the DHO914S.</summary>
public enum Coupling
{
    Dc,
    Ac,
    Gnd,
}

/// <summary>
/// Mutable per-channel configuration. Volts are expressed in SI units
/// (volts, volts/division) to match the SCPI command surface.
/// </summary>
public sealed record ChannelConfig
{
    /// <summary>Channel index, 1-4.</summary>
    public required int Channel { get; init; }

    /// <summary>Whether the trace is shown / acquired.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>Vertical scale in volts per division.</summary>
    public double VoltsPerDivision { get; init; } = 1.0;

    /// <summary>Vertical offset in volts.</summary>
    public double OffsetVolts { get; init; }

    public Coupling Coupling { get; init; } = Coupling.Dc;

    /// <summary>Probe attenuation ratio (1x, 10x, ...).</summary>
    public double ProbeRatio { get; init; } = 10.0;

    /// <summary>Optional human label shown in the UI legend.</summary>
    public string? Label { get; init; }
}
