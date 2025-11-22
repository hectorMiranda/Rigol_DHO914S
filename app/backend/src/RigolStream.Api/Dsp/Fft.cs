using RigolStream.Api.Models;

namespace RigolStream.Api.Dsp;

/// <summary>
/// Radix-2 iterative Cooley–Tukey FFT plus helpers to turn a captured
/// <see cref="Waveform"/> into a one-sided magnitude spectrum. Inputs are
/// zero-padded to the next power of two.
/// </summary>
public static class Fft
{
    public static int NextPow2(int n)
    {
        int p = 1;
        while (p < n) p <<= 1;
        return p;
    }

    /// <summary>In-place complex FFT. <paramref name="re"/>/<paramref name="im"/> length must be a power of two.</summary>
    public static void Transform(double[] re, double[] im)
    {
        int n = re.Length;
        if (n <= 1) return;
        if ((n & (n - 1)) != 0)
            throw new ArgumentException("FFT length must be a power of two", nameof(re));

        // Bit-reversal permutation.
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j)
            {
                (re[i], re[j]) = (re[j], re[i]);
                (im[i], im[j]) = (im[j], im[i]);
            }
        }

        for (int len = 2; len <= n; len <<= 1)
        {
            double ang = -2 * Math.PI / len;
            double wRe = Math.Cos(ang), wIm = Math.Sin(ang);
            for (int i = 0; i < n; i += len)
            {
                double curRe = 1, curIm = 0;
                for (int k = 0; k < len / 2; k++)
                {
                    int a = i + k, b = i + k + len / 2;
                    double tRe = re[b] * curRe - im[b] * curIm;
                    double tIm = re[b] * curIm + im[b] * curRe;
                    re[b] = re[a] - tRe;
                    im[b] = im[a] - tIm;
                    re[a] += tRe;
                    im[a] += tIm;
                    double nextRe = curRe * wRe - curIm * wIm;
                    curIm = curRe * wIm + curIm * wRe;
                    curRe = nextRe;
                }
            }
        }
    }

    /// <summary>
    /// Compute a one-sided magnitude spectrum (in dBV) from a waveform.
    /// </summary>
    public static Spectrum Compute(Waveform wf, WindowType window)
    {
        double[] v = wf.Voltage;
        int n = NextPow2(v.Length);
        var re = new double[n];
        var im = new double[n];

        double gain = Window.CoherentGain(window, v.Length);
        if (gain == 0) gain = 1;

        for (int i = 0; i < v.Length; i++)
            re[i] = v[i] * Window.Coefficient(window, i, v.Length);

        Transform(re, im);

        int bins = n / 2;
        var magsDb = new double[bins];
        for (int k = 0; k < bins; k++)
        {
            double mag = Math.Sqrt(re[k] * re[k] + im[k] * im[k]) / (v.Length * gain);
            // One-sided: double everything except DC.
            if (k > 0) mag *= 2;
            magsDb[k] = 20 * Math.Log10(mag + 1e-12);
        }

        double sampleRate = wf.TimeIncrement > 0 ? 1.0 / wf.TimeIncrement : 0;
        return new Spectrum
        {
            Channel = wf.Channel,
            Window = window.ToString(),
            SampleRate = sampleRate,
            FrequencyStep = sampleRate / n,
            MagnitudesDb = magsDb,
            Timestamp = wf.Timestamp,
        };
    }
}
