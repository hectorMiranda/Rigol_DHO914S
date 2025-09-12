using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Infrastructure;
using RigolStream.Api.Models;

namespace RigolStream.Api.Functions;

/// <summary>Identity and aggregate status endpoints.</summary>
public sealed class DeviceFunctions
{
    private readonly IOscilloscope _scope;
    private readonly ILogger<DeviceFunctions> _log;

    public DeviceFunctions(IOscilloscope scope, ILogger<DeviceFunctions> log)
    {
        _scope = scope;
        _log = log;
    }

    /// <summary><c>GET /api/device</c> — instrument identity.</summary>
    [Function("GetDevice")]
    public Task<IActionResult> GetDevice(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "device")] HttpRequest req,
        CancellationToken ct)
        => ApiResults.Execute(_log, () => _scope.GetDeviceInfoAsync(ct));

    /// <summary><c>GET /api/status</c> — device + acquisition + channels in one shot.</summary>
    [Function("GetStatus")]
    public Task<IActionResult> GetStatus(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req,
        CancellationToken ct)
        => ApiResults.Execute(_log, async () =>
        {
            var device = await _scope.GetDeviceInfoAsync(ct);
            var acq = await _scope.GetAcquisitionStateAsync(ct);
            var channels = await _scope.GetChannelsAsync(ct);
            return new ScopeStatus
            {
                Device = device,
                Acquisition = acq,
                Channels = channels,
            };
        });
}
