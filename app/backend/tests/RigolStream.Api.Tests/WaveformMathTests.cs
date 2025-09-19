using RigolStream.Api.Models;
using RigolStream.Api.Scpi;
using Xunit;

namespace RigolStream.Api.Tests;

public class WaveformMathTests
{
    [Fact]
    public void StripBlockHeader_RemovesIeeeHeader()
    {
        // "#" + "2" (2 length digits) + "05" (len) + 5 payload bytes
        byte[] block = { (byte)'#', (byte)'2', (byte)'0', (byte)'5', 1, 2, 3, 4, 5 };
        var payload = WaveformMath.StripBlockHeader(block).ToArray();
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, payload);
    }

    [Fact]
    public void StripBlockHeader_PassesThroughWhenNoHeader()
    {
        byte[] data = { 9, 8, 7 };
        var payload = WaveformMath.StripBlockHeader(data).ToArray();
        Assert.Equal(data, payload);
    }

    [Fact]
    public void BytesToVoltage_AppliesRigolConvention()
    {
        var preamble = new WaveformPreamble
        {
            YIncrement = 0.04,
            YOrigin = 0,
            YReference = 128,
        };
        // code 128 -> 0 V; code 228 -> +4 V
        var volts = WaveformMath.BytesToVoltage(new byte[] { 128, 228 }, preamble);
        Assert.Equal(0.0, volts[0], 6);
        Assert.Equal(4.0, volts[1], 6);
    }

    [Fact]
    public void Preamble_Parse_ReadsTenFields()
    {
        var p = WaveformPreamble.Parse("0,2,1200,1,1e-6,-6e-4,0,0.04,0,128");
        Assert.Equal(1200, p.Points);
        Assert.Equal(1e-6, p.XIncrement, 12);
        Assert.Equal(128, p.YReference, 6);
    }

    [Fact]
    public void DeviceInfo_Parse_SplitsIdentity()
    {
        var info = DeviceInfo.Parse("RIGOL TECHNOLOGIES,DHO914S,DHO9A1234,00.01.03", simulated: true);
        Assert.Equal("DHO914S", info.Model);
        Assert.Equal("DHO9A1234", info.SerialNumber);
        Assert.True(info.Simulated);
    }
}
