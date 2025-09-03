namespace RigolStream.Api.Models;

/// <summary>
/// Aggregate health/status payload returned by <c>GET /api/status</c>. Gives the
/// UI everything it needs to render the toolbar in a single round-trip.
/// </summary>
public sealed record ScopeStatus
{
    public required DeviceInfo Device { get; init; }
    public required AcquisitionState Acquisition { get; init; }
    public required IReadOnlyList<ChannelConfig> Channels { get; init; }
    public string? LastError { get; init; }
}
