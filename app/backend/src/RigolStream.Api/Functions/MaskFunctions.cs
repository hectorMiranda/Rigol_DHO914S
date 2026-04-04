using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Dsp;
using RigolStream.Api.Infrastructure;
using RigolStream.Api.Models;

namespace RigolStream.Api.Functions;

/// <summary>Pass/fail mask testing against a horizontal voltage band.</summary>
public sealed class MaskFunctions
{
    private readonly IOscilloscope _scope;
    private readonly ILogger<MaskFunctions> _log;

    public MaskFunctions(IOscilloscope scope, ILogger<MaskFunctions> log)
    {
        _scope = scope;
        _log = log;
    }

    /// <summary><c>POST /api/mask/test</c> — body: { channel, lowerVolts, upperVolts, points? }.</summary>
    [Function("MaskTest")]
    public async Task<IActionResult> MaskTest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mask/test")] HttpRequest req,
        CancellationToken ct)
    {
        MaskRequest? body;
        try
        {
            body = await JsonSerializer.DeserializeAsync<MaskRequest>(req.Body, ApiJson.Options, ct);
        }
        catch (JsonException ex)
        {
            return ApiResults.Problem(StatusCodes.Status400BadRequest, "BadRequest", $"Invalid JSON: {ex.Message}");
        }

        if (body is null || body.Channel is < 1 or > 4)
            return ApiResults.Problem(StatusCodes.Status400BadRequest, "BadRequest", "channel must be 1-4");

        return await ApiResults.Execute(_log, async () =>
        {
            var wf = await _scope.GetWaveformAsync(body.Channel, body.Points, ct);
            var (violations, total) = MaskEvaluator.Evaluate(wf.Voltage, body.LowerVolts, body.UpperVolts);
            return new MaskResult
            {
                Channel = body.Channel,
                LowerVolts = body.LowerVolts,
                UpperVolts = body.UpperVolts,
                Total = total,
                Violations = violations,
                Timestamp = wf.Timestamp,
            };
        });
    }
}
