namespace RigolStream.Api.Dsp;

/// <summary>Evaluates a waveform against a simple horizontal-band mask.</summary>
public static class MaskEvaluator
{
    /// <summary>Count samples that fall outside the inclusive [lower, upper] band.</summary>
    public static (int violations, int total) Evaluate(ReadOnlySpan<double> voltage, double lower, double upper)
    {
        if (lower > upper) (lower, upper) = (upper, lower);
        int violations = 0;
        foreach (var v in voltage)
            if (v < lower || v > upper) violations++;
        return (violations, voltage.Length);
    }
}
