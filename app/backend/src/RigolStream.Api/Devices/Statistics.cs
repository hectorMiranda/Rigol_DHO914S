namespace RigolStream.Api.Devices;

/// <summary>
/// Amplitude and timing statistics computed from a sampled trace. Used by the
/// simulator to produce measurement values that actually match the rendered
/// waveform. <see cref="Lookup"/> maps a SCPI measurement code to its value.
/// </summary>
public readonly record struct Statistics(
    double Min,
    double Max,
    double PeakToPeak,
    double Rms,
    double Average,
    double Frequency,
    double Period,
    double RiseTime,
    double FallTime,
    double DutyCycle)
{
    public static Statistics From(ReadOnlySpan<double> v, double knownFrequency)
    {
        if (v.Length == 0)
            return default;

        double min = double.MaxValue, max = double.MinValue, sum = 0, sumSq = 0;
        foreach (var x in v)
        {
            if (x < min) min = x;
            if (x > max) max = x;
            sum += x;
            sumSq += x * x;
        }

        double avg = sum / v.Length;
        double rms = Math.Sqrt(sumSq / v.Length);
        double mid = (min + max) / 2;

        int above = 0;
        foreach (var x in v)
            if (x >= mid) above++;
        double duty = 100.0 * above / v.Length;

        double freq = knownFrequency > 0 ? knownFrequency : 0;
        double period = freq > 0 ? 1.0 / freq : 0;

        return new Statistics(
            Min: min,
            Max: max,
            PeakToPeak: max - min,
            Rms: rms,
            Average: avg,
            Frequency: freq,
            Period: period,
            // Edge times aren't meaningful for a pure simulator; approximate a
            // realistic slew of ~5% of the period so the panel isn't empty.
            RiseTime: period * 0.05,
            FallTime: period * 0.05,
            DutyCycle: duty);
    }

    public double? Lookup(string code) => code switch
    {
        "VPP" => PeakToPeak,
        "VMAX" => Max,
        "VMIN" => Min,
        "VRMS" => Rms,
        "VAVG" => Average,
        "FREQuency" => Frequency > 0 ? Frequency : null,
        "PERiod" => Period > 0 ? Period : null,
        "RTIMe" => RiseTime > 0 ? RiseTime : null,
        "FTIMe" => FallTime > 0 ? FallTime : null,
        "PDUTy" => DutyCycle,
        _ => null,
    };
}
