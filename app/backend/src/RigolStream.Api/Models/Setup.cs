namespace RigolStream.Api.Models;

/// <summary>
/// A named snapshot of instrument state (channels + acquisition) that can be
/// re-applied later. Equivalent to the scope's save/recall setups.
/// </summary>
public sealed record Setup
{
    public required string Name { get; init; }
    public required IReadOnlyList<ChannelConfig> Channels { get; init; }
    public required AcquisitionState Acquisition { get; init; }
    public long SavedAt { get; init; }
}

/// <summary>List entry returned by <c>GET /api/setups</c>.</summary>
public sealed record SetupSummary(string Name, long SavedAt, int ChannelCount);
