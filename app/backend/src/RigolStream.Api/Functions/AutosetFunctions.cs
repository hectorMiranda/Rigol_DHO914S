using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Dsp;
using RigolStream.Api.Infrastructure;
using RigolStream.Api.Models;

namespace RigolStream.Api.Functions;

/// <summary>
/// Auto-set: measure each channel and pick a sensible vertical scale and timebase
/// so the signal fills the screen — the scope's "AUTO" button, server-side.
/// </summary>
public sealed class AutosetFunctions
{
    private readonly IOscilloscope _scope;
    private readonly ILogger<AutosetFunctions> _log;

    public AutosetFunctions(IOscilloscope scope, ILogger<AutosetFunctions> log)
    {
        _scope = scope;
        _log = log;
    }

    /// <summary><c>POST /api/autoset</c></summary>
    [Function("Autoset")]
    public Task<IActionResult> Autoset(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "autoset")] HttpRequest req,
        CancellationToken ct)
        => ApiResults.Execute(_log, async () =>
        {
            var channels = await _scope.GetChannelsAsync(ct);
            double? bestPeriod = null;

            foreach (var ch in channels.Where(c => c.Enabled))
            {
                var measurements = await _scope.GetMeasurementsAsync(ch.Channel, ct);
                double? vpp = measurements.Items.FirstOrDefault(m => m.Code == "VPP")?.Value;
                double? period = measurements.Items.FirstOrDefault(m => m.Code == "PERiod")?.Value;

                if (vpp is > 0)
                {
                    // Aim for the peak-to-peak to span ~6 of the 8 vertical divisions.
                    double voltsPerDiv = Ranges.SnapUp125(vpp.Value / 6.0);
                    await _scope.UpdateChannelAsync(ch.Channel, new ChannelUpdate { VoltsPerDivision = voltsPerDiv }, ct);
                }

                if (period is > 0 && bestPeriod is null)
                    bestPeriod = period;
            }

            if (bestPeriod is > 0)
            {
                // Show ~3 periods across the 10 horizontal divisions.
                double secondsPerDiv = Ranges.SnapUp125(bestPeriod.Value * 3 / 10.0);
                await _scope.UpdateAcquisitionAsync(new AcquisitionUpdate { SecondsPerDivision = secondsPerDiv }, ct);
            }

            return new ScopeStatus
            {
                Device = await _scope.GetDeviceInfoAsync(ct),
                Acquisition = await _scope.GetAcquisitionStateAsync(ct),
                Channels = await _scope.GetChannelsAsync(ct),
            };
        });
}
