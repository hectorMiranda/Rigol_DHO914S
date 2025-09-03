namespace RigolStream.Api.Models;

/// <summary>
/// A single automatic measurement result. <see cref="Value"/> is null when the
/// scope reports an out-of-range / no-signal sentinel (Rigol returns ~9.9E37).
/// </summary>
public sealed record Measurement
{
    public required string Name { get; init; }
    public required string Code { get; init; }
    public double? Value { get; init; }
    public required string Unit { get; init; }
}

/// <summary>All measurements gathered for one channel in a single sweep.</summary>
public sealed record MeasurementSet
{
    public required int Channel { get; init; }
    public required IReadOnlyList<Measurement> Items { get; init; }
    public long Timestamp { get; init; }
}
