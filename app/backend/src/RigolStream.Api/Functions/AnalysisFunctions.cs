using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Dsp;
using RigolStream.Api.Infrastructure;

namespace RigolStream.Api.Functions;

/// <summary>Spectral analysis (FFT) computed from a captured trace.</summary>
public sealed class AnalysisFunctions
{
    private readonly IOscilloscope _scope;
    private readonly ILogger<AnalysisFunctions> _log;

    public AnalysisFunctions(IOscilloscope scope, ILogger<AnalysisFunctions> log)
    {
        _scope = scope;
        _log = log;
    }

    /// <summary>
    /// <c>GET /api/fft/{channel}?points=2048&amp;window=hann</c> — one-sided magnitude spectrum.
    /// </summary>
    [Function("GetFft")]
    public async Task<IActionResult> GetFft(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "fft/{channel}")] HttpRequest req,
        string channel,
        CancellationToken ct)
    {
        if (!ApiResults.TryChannel(channel, out int ch, out var error))
            return error!;

        int points = int.TryParse(req.Query["points"], out var p) ? Math.Clamp(p, 64, 16384) : 2048;
        var window = Window.Parse(req.Query["window"]);

        return await ApiResults.Execute(_log, async () =>
        {
            var waveform = await _scope.GetWaveformAsync(ch, points, ct);
            return Fft.Compute(waveform, window);
        });
    }
}
