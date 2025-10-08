using RigolStream.Api.Models;

namespace RigolStream.Api.Devices;

/// <summary>The shape synthesized for a simulated channel.</summary>
public enum Waveshape
{
    Sine,
    Square,
    Triangle,
    Sawtooth,
    Noise,
}

/// <summary>Per-channel synthesis parameters for the simulator.</summary>
public sealed record SignalSpec(
    Waveshape Shape,
    double FrequencyHz,
    double AmplitudeVolts,
    double OffsetVolts,
    double PhaseRadians,
    double NoiseVolts);

/// <summary>
/// A tiny function generator. Given a time axis it synthesizes a band-limited-ish
/// trace with a little additive noise so the live view looks like a real bench
/// signal rather than a perfect textbook curve. Deterministic for a given seed.
/// </summary>
public sealed class SignalGenerator
{
    private readonly Random _rng;

    public SignalGenerator(int seed = 1) => _rng = new Random(seed);

    /// <summary>Sensible defaults: a different, recognisable signal per channel.</summary>
    public static SignalSpec DefaultFor(int channel) => channel switch
    {
        1 => new SignalSpec(Waveshape.Sine, 1_000, 1.6, 0.0, 0.0, 0.015),
        2 => new SignalSpec(Waveshape.Square, 500, 1.0, 0.0, 0.0, 0.012),
        3 => new SignalSpec(Waveshape.Triangle, 2_000, 0.8, -0.5, 0.0, 0.010),
        4 => new SignalSpec(Waveshape.Sawtooth, 250, 0.6, 0.6, 0.0, 0.010),
        _ => new SignalSpec(Waveshape.Sine, 1_000, 1.0, 0.0, 0.0, 0.01),
    };

    /// <summary>
    /// Sample <paramref name="count"/> points starting at <paramref name="tOrigin"/>
    /// seconds with spacing <paramref name="dt"/> seconds. A small wall-clock
    /// <paramref name="driftPhase"/> can be added so successive frames advance,
    /// giving the live stream gentle motion.
    /// </summary>
    public double[] Sample(SignalSpec spec, int count, double tOrigin, double dt, double driftPhase = 0)
    {
        var y = new double[count];
        double w = 2 * Math.PI * spec.FrequencyHz;
        for (int i = 0; i < count; i++)
        {
            double t = tOrigin + i * dt;
            double phase = w * t + spec.PhaseRadians + driftPhase;
            double core = spec.Shape switch
            {
                Waveshape.Sine => Math.Sin(phase),
                Waveshape.Square => Math.Sign(Math.Sin(phase)),
                Waveshape.Triangle => 2.0 / Math.PI * Math.Asin(Math.Sin(phase)),
                Waveshape.Sawtooth => 2.0 * (Frac(phase / (2 * Math.PI)) - 0.5),
                Waveshape.Noise => NextGaussian(),
                _ => 0.0,
            };
            double noise = spec.NoiseVolts > 0 ? NextGaussian() * spec.NoiseVolts : 0.0;
            y[i] = spec.AmplitudeVolts * core + spec.OffsetVolts + noise;
        }
        return y;
    }

    private static double Frac(double v) => v - Math.Floor(v);

    /// <summary>Box–Muller standard normal sample.</summary>
    private double NextGaussian()
    {
        double u1 = 1.0 - _rng.NextDouble();
        double u2 = 1.0 - _rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }
}
