namespace RigolStream.Api.Models;

/// <summary>
/// A captured trace for a single channel. <see cref="Voltage"/> is in volts and
/// is paired index-for-index with an implicit time axis described by
/// <see cref="TimeOrigin"/> and <see cref="TimeIncrement"/> (seconds). Sending
/// the axis as scalars rather than a second array roughly halves the JSON size
/// on the wire — important for the live stream.
/// </summary>
public sealed record Waveform
{
    public required int Channel { get; init; }
    public required double[] Voltage { get; init; }
    public double TimeOrigin { get; init; }
    public double TimeIncrement { get; init; }

    /// <summary>Wall-clock capture moment as a Unix epoch in milliseconds.</summary>
    public long Timestamp { get; init; }

    public int SampleCount => Voltage.Length;

    /// <summary>Total captured window in seconds.</summary>
    public double Duration => Voltage.Length * TimeIncrement;
}
