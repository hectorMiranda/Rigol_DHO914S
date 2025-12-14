using RigolStream.Api.Dsp;
using RigolStream.Api.Models;
using Xunit;

namespace RigolStream.Api.Tests;

public class UartDecoderTests
{
    private const int SamplesPerBit = 10;
    private const int Baud = 9600;

    private static Waveform SynthesizeByte(int value, int dataBits = 8)
    {
        double sampleRate = (double)SamplesPerBit * Baud;
        var bits = new List<int>();
        bits.AddRange(Enumerable.Repeat(1, SamplesPerBit * 2)); // idle high
        AppendBit(bits, 0); // start bit (low)
        for (int b = 0; b < dataBits; b++) AppendBit(bits, (value >> b) & 1); // LSB first
        AppendBit(bits, 1); // stop bit (high)
        bits.AddRange(Enumerable.Repeat(1, SamplesPerBit * 2)); // trailing idle

        var v = bits.Select(b => b == 1 ? 3.3 : 0.0).ToArray();
        return new Waveform { Channel = 1, Voltage = v, TimeIncrement = 1.0 / sampleRate };
    }

    private static void AppendBit(List<int> bits, int level)
    {
        for (int i = 0; i < SamplesPerBit; i++) bits.Add(level);
    }

    [Theory]
    [InlineData(0x41)] // 'A'
    [InlineData(0x00)]
    [InlineData(0xFF)]
    [InlineData(0x55)] // alternating
    public void Decode_RecoversByte(int value)
    {
        var wf = SynthesizeByte(value);
        var result = UartDecoder.Decode(wf, Baud);

        Assert.Single(result.Frames);
        Assert.Equal(value, result.Frames[0].Value);
        Assert.False(result.Frames[0].FramingError);
    }

    [Fact]
    public void Decode_ReturnsEmptyWhenBaudTooHighForSampleRate()
    {
        var wf = SynthesizeByte(0x41);
        var result = UartDecoder.Decode(wf, Baud * 100); // < 2 samples/bit
        Assert.Empty(result.Frames);
    }
}
