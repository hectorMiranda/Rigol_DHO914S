namespace RigolStream.Api.Models;

/// <summary>
/// A patch applied to a channel's configuration. Every field is nullable so a
/// PATCH request can change just one property; null means "leave unchanged".
/// </summary>
public sealed record ChannelUpdate
{
    public bool? Enabled { get; init; }
    public double? VoltsPerDivision { get; init; }
    public double? OffsetVolts { get; init; }
    public Coupling? Coupling { get; init; }
    public double? ProbeRatio { get; init; }
    public string? Label { get; init; }

    /// <summary>Apply this patch onto an existing config, returning a new record.</summary>
    public ChannelConfig ApplyTo(ChannelConfig current) => current with
    {
        Enabled = Enabled ?? current.Enabled,
        VoltsPerDivision = VoltsPerDivision ?? current.VoltsPerDivision,
        OffsetVolts = OffsetVolts ?? current.OffsetVolts,
        Coupling = Coupling ?? current.Coupling,
        ProbeRatio = ProbeRatio ?? current.ProbeRatio,
        Label = Label ?? current.Label,
    };
}
