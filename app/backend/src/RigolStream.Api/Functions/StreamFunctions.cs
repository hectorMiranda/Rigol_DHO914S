using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Infrastructure;
using RigolStream.Api.Models;

namespace RigolStream.Api.Functions;

/// <summary>
/// The live waveform stream. Holds the HTTP connection open and pushes a frame
/// per channel on a fixed cadence over Server-Sent Events, which the browser
/// consumes with a plain <c>EventSource</c>.
/// </summary>
public sealed class StreamFunctions
{
    private readonly IOscilloscope _scope;
    private readonly ILogger<StreamFunctions> _log;

    public StreamFunctions(IOscilloscope scope, ILogger<StreamFunctions> log)
    {
        _scope = scope;
        _log = log;
    }

    /// <summary>
    /// <c>GET /api/stream?channels=1,2&amp;interval=100&amp;points=600</c> — SSE waveform stream.
    /// </summary>
    [Function("StreamWaveforms")]
    public async Task<IActionResult> Stream(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stream")] HttpRequest req,
        CancellationToken ct)
    {
        int[] channels = ParseChannels(req.Query["channels"]);
        int interval = Math.Clamp(ParseInt(req.Query["interval"], 100), 20, 2000);
        int points = Math.Clamp(ParseInt(req.Query["points"], 600), 16, 4096);

        var sse = new SseWriter(req.HttpContext.Response);
        _log.LogInformation("SSE stream opened: channels={Channels} interval={Interval}ms", string.Join(',', channels), interval);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var frames = new List<Waveform>(channels.Length);
                foreach (var ch in channels)
                    frames.Add(await _scope.GetWaveformAsync(ch, points, ct));

                var payload = new
                {
                    t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    frames,
                };
                await sse.WriteJsonAsync(ApiJson.Serialize(payload), ct);
                await Task.Delay(interval, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected — normal.
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "SSE stream ended with error");
        }

        return new EmptyResult();
    }

    private static int[] ParseChannels(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new[] { 1 };

        var set = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var n) ? n : -1)
            .Where(n => n is >= 1 and <= 4)
            .Distinct()
            .ToArray();

        return set.Length > 0 ? set : new[] { 1 };
    }

    private static int ParseInt(string? raw, int fallback) =>
        int.TryParse(raw, out var n) ? n : fallback;
}
