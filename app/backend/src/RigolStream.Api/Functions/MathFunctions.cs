using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Infrastructure;
using RigolStream.Api.Models;

namespace RigolStream.Api.Functions;

/// <summary>
/// Math channel: sample-wise combination of two source channels. The result is
/// returned as a <see cref="Waveform"/> with <c>channel = 0</c> (MATH).
/// </summary>
public sealed class MathFunctions
{
    private readonly IOscilloscope _scope;
    private readonly ILogger<MathFunctions> _log;

    public MathFunctions(IOscilloscope scope, ILogger<MathFunctions> log)
    {
        _scope = scope;
        _log = log;
    }

    /// <summary><c>GET /api/math/{op}?a=1&amp;b=2&amp;points=600</c> where op = add | subtract | multiply.</summary>
    [Function("GetMath")]
    public async Task<IActionResult> GetMath(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "math/{op}")] HttpRequest req,
        string op,
        CancellationToken ct)
    {
        Func<double, double, double>? combine = op.ToLowerInvariant() switch
        {
            "add" => (x, y) => x + y,
            "subtract" or "sub" => (x, y) => x - y,
            "multiply" or "mul" => (x, y) => x * y,
            _ => null,
        };
        if (combine is null)
            return ApiResults.Problem(StatusCodes.Status400BadRequest, "BadRequest",
                $"Unknown op '{op}' (add, subtract, multiply)");

        if (!ApiResults.TryChannel(req.Query["a"], out int a, out var ea)) return ea!;
        if (!ApiResults.TryChannel(req.Query["b"], out int b, out var eb)) return eb!;
        int points = int.TryParse(req.Query["points"], out var p) ? Math.Clamp(p, 16, 8192) : 600;

        return await ApiResults.Execute(_log, async () =>
        {
            var wa = await _scope.GetWaveformAsync(a, points, ct);
            var wb = await _scope.GetWaveformAsync(b, points, ct);
            int n = Math.Min(wa.Voltage.Length, wb.Voltage.Length);
            var outV = new double[n];
            for (int i = 0; i < n; i++) outV[i] = combine(wa.Voltage[i], wb.Voltage[i]);

            return new Waveform
            {
                Channel = 0,
                Voltage = outV,
                TimeOrigin = wa.TimeOrigin,
                TimeIncrement = wa.TimeIncrement,
                Timestamp = wa.Timestamp,
            };
        });
    }
}
