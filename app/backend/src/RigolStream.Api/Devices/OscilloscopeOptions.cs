namespace RigolStream.Api.Devices;

/// <summary>
/// Bound from the <c>Oscilloscope</c> configuration section. Controls whether the
/// API drives real hardware or the simulator, and how to reach the instrument.
/// </summary>
public sealed class OscilloscopeOptions
{
    public const string SectionName = "Oscilloscope";

    /// <summary>Either <c>Simulated</c> or <c>Scpi</c> (case-insensitive).</summary>
    public string Mode { get; set; } = "Simulated";

    /// <summary>VISA resource string when <see cref="Mode"/> is Scpi.</summary>
    public string? Resource { get; set; }

    /// <summary>Command timeout in milliseconds.</summary>
    public int TimeoutMs { get; set; } = 10_000;

    public bool IsSimulated =>
        !string.Equals(Mode, "Scpi", StringComparison.OrdinalIgnoreCase);
}
