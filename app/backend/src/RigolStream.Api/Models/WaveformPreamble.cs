namespace RigolStream.Api.Models;

/// <summary>
/// The 10-field waveform preamble returned by <c>:WAVeform:PREamble?</c>. The
/// increments/origins/references map raw ADC codes back to volts and seconds:
///
///   voltage = (code - YReference - YOrigin) * YIncrement      (Rigol convention)
///   time    = (index - XReference) * XIncrement + XOrigin
/// </summary>
public sealed record WaveformPreamble
{
    public int Format { get; init; }
    public int Type { get; init; }
    public int Points { get; init; }
    public int Count { get; init; }
    public double XIncrement { get; init; }
    public double XOrigin { get; init; }
    public double XReference { get; init; }
    public double YIncrement { get; init; }
    public double YOrigin { get; init; }
    public double YReference { get; init; }

    /// <summary>
    /// Parse the comma-separated preamble string. Throws <see cref="FormatException"/>
    /// if fewer than 10 fields are present.
    /// </summary>
    public static WaveformPreamble Parse(string preamble)
    {
        var p = (preamble ?? string.Empty).Split(',', StringSplitOptions.TrimEntries);
        if (p.Length < 10)
            throw new FormatException($"Expected 10 preamble fields, got {p.Length}: '{preamble}'");

        double D(int i) => double.Parse(p[i], System.Globalization.CultureInfo.InvariantCulture);
        int I(int i) => (int)D(i);

        return new WaveformPreamble
        {
            Format = I(0),
            Type = I(1),
            Points = I(2),
            Count = I(3),
            XIncrement = D(4),
            XOrigin = D(5),
            XReference = D(6),
            YIncrement = D(7),
            YOrigin = D(8),
            YReference = D(9),
        };
    }
}
