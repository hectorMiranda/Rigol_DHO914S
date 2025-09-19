using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;

namespace RigolStream.Api.Infrastructure;

/// <summary>
/// Wraps endpoint handlers so instrument failures become sensible HTTP responses
/// instead of unhandled 500s. Maps <see cref="OscilloscopeErrorKind"/> to a
/// status code and returns an RFC 7807-ish problem body.
/// </summary>
public static class ApiResults
{
    public static async Task<IActionResult> Execute<T>(ILogger log, Func<Task<T>> action)
    {
        try
        {
            return new OkObjectResult(await action());
        }
        catch (OscilloscopeException ex)
        {
            int status = ex.Kind switch
            {
                OscilloscopeErrorKind.Connection => StatusCodes.Status503ServiceUnavailable,
                OscilloscopeErrorKind.Timeout => StatusCodes.Status504GatewayTimeout,
                OscilloscopeErrorKind.Data => StatusCodes.Status502BadGateway,
                _ => StatusCodes.Status400BadRequest,
            };
            log.LogWarning(ex, "Instrument error ({Kind}): {Message}", ex.Kind, ex.Message);
            return Problem(status, ex.Kind.ToString(), ex.Message);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Unhandled error: {Message}", ex.Message);
            return Problem(StatusCodes.Status500InternalServerError, "Internal", ex.Message);
        }
    }

    public static IActionResult Problem(int status, string kind, string detail) =>
        new ObjectResult(new { status, kind, detail }) { StatusCode = status };

    /// <summary>Validate a channel argument or return a 400 result via <paramref name="error"/>.</summary>
    public static bool TryChannel(string? raw, out int channel, out IActionResult? error)
    {
        if (int.TryParse(raw, out channel) && channel is >= 1 and <= 4)
        {
            error = null;
            return true;
        }
        error = Problem(StatusCodes.Status400BadRequest, "BadRequest", $"Channel '{raw}' must be 1-4");
        return false;
    }
}
