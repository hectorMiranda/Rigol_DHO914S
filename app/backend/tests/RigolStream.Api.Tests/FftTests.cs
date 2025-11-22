using RigolStream.Api.Dsp;
using RigolStream.Api.Models;
using Xunit;

namespace RigolStream.Api.Tests;

public class FftTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 4)]
    [InlineData(600, 1024)]
    [InlineData(1024, 1024)]
    public void NextPow2_RoundsUp(int input, int expected)
    {
        Assert.Equal(expected, Fft.NextPow2(input));
    }

    [Fact]
    public void Compute_FindsFundamentalOfSine()
    {
        const double freq = 1000.0;
        const double dt = 1.0 / 50_000.0; // 50 kHz sample rate
        const int n = 1024;

        var v = new double[n];
        for (int i = 0; i < n; i++)
            v[i] = Math.Sin(2 * Math.PI * freq * i * dt);

        var wf = new Waveform { Channel = 1, Voltage = v, TimeIncrement = dt };
        var spectrum = Fft.Compute(wf, WindowType.Hann);

        var (peakFreq, _) = spectrum.Peak();
        // Peak should land within one bin of the true frequency.
        Assert.True(Math.Abs(peakFreq - freq) <= spectrum.FrequencyStep,
            $"peak {peakFreq} Hz not within {spectrum.FrequencyStep} Hz of {freq} Hz");
    }

    [Fact]
    public void Transform_RejectsNonPowerOfTwo()
    {
        Assert.Throws<ArgumentException>(() => Fft.Transform(new double[3], new double[3]));
    }

    [Fact]
    public void CoherentGain_HannIsAboutHalf()
    {
        double gain = Window.CoherentGain(WindowType.Hann, 1024);
        Assert.InRange(gain, 0.49, 0.51);
    }
}
