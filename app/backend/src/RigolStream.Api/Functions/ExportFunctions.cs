using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Infrastructure;
using RigolStream.Api.Models;

namespace RigolStream.Api.Functions;

/// <summary>Capture-to-file export (CSV), matching the Python tooling's columns.</summary>
public sealed class ExportFunctions
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private readonly IOscilloscope _scope;
    private readonly ILogger<ExportFunctions> _log;

    public ExportFunctions(IOscilloscope scope, ILogger<ExportFunctions> log)
    {
        _scope = scope;
        _log = log;
    }

    /// <summary><c>GET /api/export/{channel}.csv?points=N</c> — time,voltage CSV download.</summary>
    [Function("ExportCsv")]
    public async Task<IActionResult> ExportCsv(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "export/{channel}.csv")] HttpRequest req,
        string channel,
        CancellationToken ct)
    {
        if (!ApiResults.TryChannel(channel, out int ch, out var error))
            return error!;

        int? points = int.TryParse(req.Query["points"], out var p) ? p : null;

        try
        {
            var wf = await _scope.GetWaveformAsync(ch, points, ct);
            var csv = BuildCsv(wf);
            var bytes = Encoding.UTF8.GetBytes(csv);
            return new FileContentResult(bytes, "text/csv")
            {
                FileDownloadName = $"ch{ch}_waveform_{wf.Timestamp}.csv",
            };
        }
        catch (OscilloscopeException ex)
        {
            _log.LogWarning(ex, "Export failed: {Message}", ex.Message);
            return ApiResults.Problem(StatusCodes.Status502BadGateway, ex.Kind.ToString(), ex.Message);
        }
    }

    private static string BuildCsv(Waveform wf)
    {
        var sb = new StringBuilder(wf.Voltage.Length * 24);
        sb.Append("time_s,voltage_v\n");
        for (int i = 0; i < wf.Voltage.Length; i++)
        {
            double t = wf.TimeOrigin + i * wf.TimeIncrement;
            sb.Append(t.ToString("G9", Inv)).Append(',').Append(wf.Voltage[i].ToString("G9", Inv)).Append('\n');
        }
        return sb.ToString();
    }
}
