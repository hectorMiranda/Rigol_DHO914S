using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Infrastructure;

namespace RigolStream.Api.Functions;

/// <summary>Display screenshot endpoint (returns a PNG).</summary>
public sealed class ScreenshotFunctions
{
    private readonly IOscilloscope _scope;
    private readonly ILogger<ScreenshotFunctions> _log;

    public ScreenshotFunctions(IOscilloscope scope, ILogger<ScreenshotFunctions> log)
    {
        _scope = scope;
        _log = log;
    }

    /// <summary><c>GET /api/screenshot</c> — PNG of the current display.</summary>
    [Function("GetScreenshot")]
    public async Task<IActionResult> GetScreenshot(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "screenshot")] HttpRequest req,
        CancellationToken ct)
    {
        try
        {
            var png = await _scope.GetScreenshotAsync(ct);
            return new FileContentResult(png, "image/png");
        }
        catch (OscilloscopeException ex)
        {
            _log.LogWarning(ex, "Screenshot failed: {Message}", ex.Message);
            return ApiResults.Problem(StatusCodes.Status502BadGateway, ex.Kind.ToString(), ex.Message);
        }
    }
}
