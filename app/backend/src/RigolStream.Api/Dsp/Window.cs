namespace RigolStream.Api.Dsp;

/// <summary>Analysis windows for the FFT.</summary>
public enum WindowType
{
    Rectangular,
    Hann,
    Hamming,
    Blackman,
    FlatTop,
}

public static class Window
{
    public static WindowType Parse(string? name) => (name ?? "hann").Trim().ToLowerInvariant() switch
    {
        "rect" or "rectangular" or "none" => WindowType.Rectangular,
        "hamming" => WindowType.Hamming,
        "blackman" => WindowType.Blackman,
        "flattop" or "flat-top" => WindowType.FlatTop,
        _ => WindowType.Hann,
    };

    /// <summary>Window coefficient at sample <paramref name="n"/> of <paramref name="count"/>.</summary>
    public static double Coefficient(WindowType type, int n, int count)
    {
        if (count <= 1) return 1.0;
        double x = 2 * Math.PI * n / (count - 1);
        return type switch
        {
            WindowType.Rectangular => 1.0,
            WindowType.Hann => 0.5 - 0.5 * Math.Cos(x),
            WindowType.Hamming => 0.54 - 0.46 * Math.Cos(x),
            WindowType.Blackman => 0.42 - 0.5 * Math.Cos(x) + 0.08 * Math.Cos(2 * x),
            WindowType.FlatTop => 0.21557895 - 0.41663158 * Math.Cos(x) + 0.277263158 * Math.Cos(2 * x)
                                  - 0.083578947 * Math.Cos(3 * x) + 0.006947368 * Math.Cos(4 * x),
            _ => 1.0,
        };
    }

    /// <summary>Coherent gain of a window, used to correct amplitude after windowing.</summary>
    public static double CoherentGain(WindowType type, int count)
    {
        double sum = 0;
        for (int n = 0; n < count; n++) sum += Coefficient(type, n, count);
        return count == 0 ? 1.0 : sum / count;
    }
}
