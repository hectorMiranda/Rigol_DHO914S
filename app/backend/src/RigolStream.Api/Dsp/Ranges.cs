namespace RigolStream.Api.Dsp;

/// <summary>Helpers for snapping continuous values to instrument 1-2-5 steps.</summary>
public static class Ranges
{
    private static readonly double[] Mantissas = { 1, 2, 5 };

    /// <summary>Round a positive value up to the next 1-2-5 step (e.g. 0.034 → 0.05).</summary>
    public static double SnapUp125(double value)
    {
        if (value <= 0) return 0;
        double decade = Math.Pow(10, Math.Floor(Math.Log10(value)));
        foreach (var m in Mantissas)
            if (m * decade >= value - 1e-15)
                return m * decade;
        return 10 * decade;
    }
}
