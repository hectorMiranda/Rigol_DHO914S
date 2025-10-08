using RigolStream.Api.Devices;
using RigolStream.Api.Models;
using Xunit;

namespace RigolStream.Api.Tests;

public class SimulatorTests
{
    [Fact]
    public async Task GetWaveform_ReturnsRequestedPointCount()
    {
        var scope = new SimulatedOscilloscope();
        var wf = await scope.GetWaveformAsync(1, 512);
        Assert.Equal(512, wf.SampleCount);
        Assert.Equal(1, wf.Channel);
        Assert.True(wf.TimeIncrement > 0);
    }

    [Fact]
    public async Task GetWaveform_RejectsBadChannel()
    {
        var scope = new SimulatedOscilloscope();
        await Assert.ThrowsAsync<OscilloscopeException>(() => scope.GetWaveformAsync(9));
    }

    [Fact]
    public async Task UpdateChannel_PersistsPartialChange()
    {
        var scope = new SimulatedOscilloscope();
        var updated = await scope.UpdateChannelAsync(3, new ChannelUpdate { VoltsPerDivision = 0.2, Enabled = true });
        Assert.Equal(0.2, updated.VoltsPerDivision, 6);
        Assert.True(updated.Enabled);

        var all = await scope.GetChannelsAsync();
        Assert.Equal(0.2, all.First(c => c.Channel == 3).VoltsPerDivision, 6);
    }

    [Fact]
    public async Task Screenshot_HasPngSignature()
    {
        var scope = new SimulatedOscilloscope();
        var png = await scope.GetScreenshotAsync();
        byte[] signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        Assert.True(png.Length > signature.Length);
        Assert.Equal(signature, png[..8]);
    }

    [Fact]
    public async Task Measurements_IncludeFiniteVpp()
    {
        var scope = new SimulatedOscilloscope();
        var set = await scope.GetMeasurementsAsync(1);
        var vpp = set.Items.Single(m => m.Code == "VPP");
        Assert.NotNull(vpp.Value);
        Assert.True(vpp.Value > 0);
    }

    [Fact]
    public async Task RunState_TogglesTriggerStatus()
    {
        var scope = new SimulatedOscilloscope();
        var stopped = await scope.SetRunStateAsync(RunState.Stopped);
        Assert.Equal(RunState.Stopped, stopped.RunState);
        Assert.Equal("STOP", stopped.TriggerStatus);
    }
}

public class SignalAndTransportTests
{
    [Fact]
    public void SignalGenerator_IsDeterministicForSeed()
    {
        var a = new SignalGenerator(42).Sample(SignalGenerator.DefaultFor(1), 256, 0, 1e-6);
        var b = new SignalGenerator(42).Sample(SignalGenerator.DefaultFor(1), 256, 0, 1e-6);
        Assert.Equal(a, b);
    }

    [Theory]
    [InlineData("TCPIP::192.168.1.50::INSTR", "192.168.1.50", 5555)]
    [InlineData("192.168.1.50:7777", "192.168.1.50", 7777)]
    [InlineData("10.0.0.2", "10.0.0.2", 5555)]
    public void TcpTransport_ParsesResourceStrings(string resource, string host, int port)
    {
        var (h, p) = TcpScpiTransport.ParseResource(resource);
        Assert.Equal(host, h);
        Assert.Equal(port, p);
    }
}
