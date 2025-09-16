using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Infrastructure;
using RigolStream.Api.Models;

namespace RigolStream.Api.Functions;

/// <summary>Run control (run/stop/single) and timebase/trigger updates.</summary>
public sealed class AcquisitionFunctions
{
    private readonly IOscilloscope _scope;
    private readonly ILogger<AcquisitionFunctions> _log;

    public AcquisitionFunctions(IOscilloscope scope, ILogger<AcquisitionFunctions> log)
    {
        _scope = scope;
        _log = log;
    }

    /// <summary><c>GET /api/acquisition</c> — current timebase/trigger/run state.</summary>
    [Function("GetAcquisition")]
    public Task<IActionResult> GetAcquisition(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "acquisition")] HttpRequest req,
        CancellationToken ct)
        => ApiResults.Execute(_log, () => _scope.GetAcquisitionStateAsync(ct));

    /// <summary><c>POST /api/acquisition/{action}</c> where action = run | stop | single.</summary>
    [Function("SetRunState")]
    public async Task<IActionResult> SetRunState(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "acquisition/{action}")] HttpRequest req,
        string action,
        CancellationToken ct)
    {
        RunState? state = action.ToLowerInvariant() switch
        {
            "run" => RunState.Running,
            "stop" => RunState.Stopped,
            "single" => RunState.Single,
            _ => null,
        };

        if (state is null)
            return ApiResults.Problem(StatusCodes.Status400BadRequest, "BadRequest",
                $"Unknown action '{action}' (expected run, stop or single)");

        return await ApiResults.Execute(_log, () => _scope.SetRunStateAsync(state.Value, ct));
    }

    /// <summary><c>PATCH /api/acquisition</c> — update timebase + trigger.</summary>
    [Function("UpdateAcquisition")]
    public async Task<IActionResult> UpdateAcquisition(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", "put", Route = "acquisition")] HttpRequest req,
        CancellationToken ct)
    {
        AcquisitionUpdate? update;
        try
        {
            update = await JsonSerializer.DeserializeAsync<AcquisitionUpdate>(req.Body, ApiJson.Options, ct);
        }
        catch (JsonException ex)
        {
            return ApiResults.Problem(StatusCodes.Status400BadRequest, "BadRequest", $"Invalid JSON: {ex.Message}");
        }

        if (update is null)
            return ApiResults.Problem(StatusCodes.Status400BadRequest, "BadRequest", "Empty body");

        return await ApiResults.Execute(_log, () => _scope.UpdateAcquisitionAsync(update, ct));
    }
}
