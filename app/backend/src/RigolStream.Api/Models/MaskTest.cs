namespace RigolStream.Api.Models;

/// <summary>Request body for a pass/fail mask test.</summary>
public sealed record MaskRequest
{
    public int Channel { get; init; } = 1;
    public double LowerVolts { get; init; } = -1;
    public double UpperVolts { get; init; } = 1;
    public int? Points { get; init; }
}

/// <summary>Outcome of a mask test.</summary>
public sealed record MaskResult
{
    public required int Channel { get; init; }
    public double LowerVolts { get; init; }
    public double UpperVolts { get; init; }
    public int Total { get; init; }
    public int Violations { get; init; }
    public bool Pass => Violations == 0;
    public double ViolationRatio => Total == 0 ? 0 : (double)Violations / Total;
    public long Timestamp { get; init; }
}
