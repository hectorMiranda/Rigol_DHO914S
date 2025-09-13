using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Infrastructure;

namespace RigolStream.Api.Functions;

/// <summary>Single-shot waveform capture.</summary>
public sealed class WaveformFunctions
{
    private readonly IOscilloscope _scope;
    private readonly ILogger<WaveformFunctions> _log;

    public WaveformFunctions(IOscilloscope scope, ILogger<WaveformFunctions> log)
    {
        _scope = scope;
        _log = log;
    }

    /// <summary>
    /// <c>GET /api/waveform/{channel}?points=1200</c> — capture one trace.
    /// </summary>
    [Function("GetWaveform")]
    public async Task<IActionResult> GetWaveform(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "waveform/{channel}")] HttpRequest req,
        string channel,
        CancellationToken ct)
    {
        if (!ApiResults.TryChannel(channel, out int ch, out var error))
            return error!;

        int? points = int.TryParse(req.Query["points"], out var p) ? p : null;
        return await ApiResults.Execute(_log, () => _scope.GetWaveformAsync(ch, points, ct));
    }
}
