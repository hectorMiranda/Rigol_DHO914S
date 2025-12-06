using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Infrastructure;
using RigolStream.Api.Models;

namespace RigolStream.Api.Functions;

/// <summary>Save / recall named instrument setups.</summary>
public sealed class SetupFunctions
{
    private readonly IOscilloscope _scope;
    private readonly SetupStore _store;
    private readonly ILogger<SetupFunctions> _log;

    public SetupFunctions(IOscilloscope scope, SetupStore store, ILogger<SetupFunctions> log)
    {
        _scope = scope;
        _store = store;
        _log = log;
    }

    /// <summary><c>GET /api/setups</c></summary>
    [Function("ListSetups")]
    public IActionResult List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "setups")] HttpRequest req)
        => new OkObjectResult(_store.List());

    /// <summary><c>GET /api/setups/{name}</c></summary>
    [Function("GetSetup")]
    public IActionResult Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "setups/{name}")] HttpRequest req,
        string name)
    {
        var setup = _store.Get(name);
        return setup is null
            ? ApiResults.Problem(StatusCodes.Status404NotFound, "NotFound", $"No setup named '{name}'")
            : new OkObjectResult(setup);
    }

    /// <summary><c>POST /api/setups/{name}</c> — snapshot current scope state.</summary>
    [Function("SaveSetup")]
    public Task<IActionResult> Save(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "setups/{name}")] HttpRequest req,
        string name,
        CancellationToken ct)
        => ApiResults.Execute(_log, async () =>
        {
            var setup = new Setup
            {
                Name = name,
                Channels = await _scope.GetChannelsAsync(ct),
                Acquisition = await _scope.GetAcquisitionStateAsync(ct),
                SavedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };
            return _store.Save(setup);
        });

    /// <summary><c>DELETE /api/setups/{name}</c></summary>
    [Function("DeleteSetup")]
    public IActionResult Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "setups/{name}")] HttpRequest req,
        string name)
        => _store.Delete(name)
            ? new NoContentResult()
            : ApiResults.Problem(StatusCodes.Status404NotFound, "NotFound", $"No setup named '{name}'");

    /// <summary><c>POST /api/setups/{name}/recall</c> — apply a saved setup to the scope.</summary>
    [Function("RecallSetup")]
    public Task<IActionResult> Recall(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "setups/{name}/recall")] HttpRequest req,
        string name,
        CancellationToken ct)
    {
        var setup = _store.Get(name);
        if (setup is null)
            return Task.FromResult(ApiResults.Problem(StatusCodes.Status404NotFound, "NotFound", $"No setup named '{name}'"));

        return ApiResults.Execute(_log, async () =>
        {
            foreach (var ch in setup.Channels)
            {
                await _scope.UpdateChannelAsync(ch.Channel, new ChannelUpdate
                {
                    Enabled = ch.Enabled,
                    VoltsPerDivision = ch.VoltsPerDivision,
                    OffsetVolts = ch.OffsetVolts,
                    Coupling = ch.Coupling,
                    ProbeRatio = ch.ProbeRatio,
                    Label = ch.Label,
                }, ct);
            }

            return await _scope.UpdateAcquisitionAsync(new AcquisitionUpdate
            {
                SecondsPerDivision = setup.Acquisition.SecondsPerDivision,
                TimebaseOffset = setup.Acquisition.TimebaseOffset,
                TriggerSource = setup.Acquisition.TriggerSource,
                TriggerLevel = setup.Acquisition.TriggerLevel,
                TriggerSlope = setup.Acquisition.TriggerSlope,
            }, ct);
        });
    }
}
