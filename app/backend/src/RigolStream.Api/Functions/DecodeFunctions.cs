using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Dsp;
using RigolStream.Api.Infrastructure;

namespace RigolStream.Api.Functions;

/// <summary>Protocol decoding over a captured channel.</summary>
public sealed class DecodeFunctions
{
    private readonly IOscilloscope _scope;
    private readonly ILogger<DecodeFunctions> _log;

    public DecodeFunctions(IOscilloscope scope, ILogger<DecodeFunctions> log)
    {
        _scope = scope;
        _log = log;
    }

    /// <summary>
    /// <c>GET /api/decode/uart/{channel}?baud=9600&amp;points=4096&amp;threshold=auto</c>.
    /// </summary>
    [Function("DecodeUart")]
    public async Task<IActionResult> DecodeUart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "decode/uart/{channel}")] HttpRequest req,
        string channel,
        CancellationToken ct)
    {
        if (!ApiResults.TryChannel(channel, out int ch, out var error))
            return error!;

        int baud = int.TryParse(req.Query["baud"], out var b) ? Math.Clamp(b, 50, 10_000_000) : 9600;
        int points = int.TryParse(req.Query["points"], out var p) ? Math.Clamp(p, 64, 16384) : 4096;
        double? threshold = double.TryParse(req.Query["threshold"], out var t) ? t : null;

        return await ApiResults.Execute(_log, async () =>
        {
            var wf = await _scope.GetWaveformAsync(ch, points, ct);
            return UartDecoder.Decode(wf, baud, threshold);
        });
    }
}
