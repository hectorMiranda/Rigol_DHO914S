using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RigolStream.Api.Devices;
using RigolStream.Api.Infrastructure;
using RigolStream.Api.Models;

namespace RigolStream.Api.Functions;

/// <summary>Per-channel configuration endpoints.</summary>
public sealed class ChannelFunctions
{
    private readonly IOscilloscope _scope;
    private readonly ILogger<ChannelFunctions> _log;

    public ChannelFunctions(IOscilloscope scope, ILogger<ChannelFunctions> log)
    {
        _scope = scope;
        _log = log;
    }

    /// <summary><c>GET /api/channels</c></summary>
    [Function("GetChannels")]
    public Task<IActionResult> GetChannels(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "channels")] HttpRequest req,
        CancellationToken ct)
        => ApiResults.Execute(_log, () => _scope.GetChannelsAsync(ct));

    /// <summary><c>GET /api/channels/{channel}</c></summary>
    [Function("GetChannel")]
    public async Task<IActionResult> GetChannel(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "channels/{channel}")] HttpRequest req,
        string channel,
        CancellationToken ct)
    {
        if (!ApiResults.TryChannel(channel, out int ch, out var error))
            return error!;

        return await ApiResults.Execute(_log, async () =>
        {
            var all = await _scope.GetChannelsAsync(ct);
            return all.First(c => c.Channel == ch);
        });
    }

    /// <summary><c>PATCH /api/channels/{channel}</c> — apply a partial config update.</summary>
    [Function("UpdateChannel")]
    public async Task<IActionResult> UpdateChannel(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", "put", Route = "channels/{channel}")] HttpRequest req,
        string channel,
        CancellationToken ct)
    {
        if (!ApiResults.TryChannel(channel, out int ch, out var error))
            return error!;

        ChannelUpdate? update;
        try
        {
            update = await JsonSerializer.DeserializeAsync<ChannelUpdate>(req.Body, ApiJson.Options, ct);
        }
        catch (JsonException ex)
        {
            return ApiResults.Problem(StatusCodes.Status400BadRequest, "BadRequest", $"Invalid JSON: {ex.Message}");
        }

        if (update is null)
            return ApiResults.Problem(StatusCodes.Status400BadRequest, "BadRequest", "Empty body");

        return await ApiResults.Execute(_log, () => _scope.UpdateChannelAsync(ch, update, ct));
    }
}
