namespace RigolStream.Api.Models;

/// <summary>Run state of the acquisition engine.</summary>
public enum RunState
{
    Stopped,
    Running,
    Single,
}

/// <summary>Trigger edge slope.</summary>
public enum TriggerSlope
{
    Positive,
    Negative,
    Either,
}

/// <summary>
/// Snapshot of the timebase + trigger + run state. Mirrors the subset of scope
/// state the UI needs to draw the graticule and trigger marker correctly.
/// </summary>
public sealed record AcquisitionState
{
    public RunState RunState { get; init; } = RunState.Running;

    /// <summary>Horizontal scale in seconds per division (10 divisions across).</summary>
    public double SecondsPerDivision { get; init; } = 1e-3;

    public double TimebaseOffset { get; init; }

    public int TriggerSource { get; init; } = 1;
    public double TriggerLevel { get; init; }
    public TriggerSlope TriggerSlope { get; init; } = TriggerSlope.Positive;

    /// <summary>Reported by <c>:TRIGger:STATus?</c> — TD, WAIT, RUN, AUTO, STOP.</summary>
    public string TriggerStatus { get; init; } = "AUTO";
}
