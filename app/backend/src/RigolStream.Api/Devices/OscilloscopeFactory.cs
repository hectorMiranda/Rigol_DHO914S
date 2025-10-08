using Microsoft.Extensions.Options;

namespace RigolStream.Api.Devices;

/// <summary>
/// Builds the right <see cref="IOscilloscope"/> from configuration: the simulator
/// when <c>Oscilloscope:Mode</c> is Simulated (the default), or a SCPI driver over
/// TCP when set to Scpi.
/// </summary>
public static class OscilloscopeFactory
{
    public static IOscilloscope Create(IOptions<OscilloscopeOptions> options)
    {
        var o = options.Value;
        if (o.IsSimulated)
            return new SimulatedOscilloscope();

        var transport = new TcpScpiTransport(o.Resource ?? string.Empty, o.TimeoutMs);
        return new ScpiOscilloscope(transport);
    }
}
