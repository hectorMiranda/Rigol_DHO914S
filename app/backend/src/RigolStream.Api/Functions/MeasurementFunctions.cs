using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Infrastructure;

namespace RigolStream.Api.Functions;

/// <summary>Automatic measurement endpoint.</summary>
public sealed class MeasurementFunctions
{
    private readonly IOscilloscope _scope;
    private readonly ILogger<MeasurementFunctions> _log;

    public MeasurementFunctions(IOscilloscope scope, ILogger<MeasurementFunctions> log)
    {
        _scope = scope;
        _log = log;
    }

    /// <summary><c>GET /api/measurements/{channel}</c> — default auto-measurements.</summary>
    [Function("GetMeasurements")]
    public async Task<IActionResult> GetMeasurements(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "measurements/{channel}")] HttpRequest req,
        string channel,
        CancellationToken ct)
    {
        if (!ApiResults.TryChannel(channel, out int ch, out var error))
            return error!;

        return await ApiResults.Execute(_log, () => _scope.GetMeasurementsAsync(ch, ct));
    }
}
