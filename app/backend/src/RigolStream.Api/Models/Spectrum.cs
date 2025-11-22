namespace RigolStream.Api.Models;

/// <summary>
/// One-sided magnitude spectrum. The frequency of bin <c>k</c> is
/// <c>k * FrequencyStep</c> Hz; magnitudes are in dBV.
/// </summary>
public sealed record Spectrum
{
    public required int Channel { get; init; }
    public required string Window { get; init; }
    public double SampleRate { get; init; }
    public double FrequencyStep { get; init; }
    public required double[] MagnitudesDb { get; init; }
    public long Timestamp { get; init; }

    public int Bins => MagnitudesDb.Length;

    /// <summary>The bin with the highest magnitude and its frequency — the fundamental.</summary>
    public (double frequency, double magnitudeDb) Peak()
    {
        if (MagnitudesDb.Length <= 1) return (0, 0);
        int peak = 1; // skip DC (bin 0) when locating the fundamental
        for (int k = 1; k < MagnitudesDb.Length; k++)
            if (MagnitudesDb[k] > MagnitudesDb[peak]) peak = k;
        return (peak * FrequencyStep, MagnitudesDb.Length > 0 ? MagnitudesDb[peak] : 0);
    }
}
