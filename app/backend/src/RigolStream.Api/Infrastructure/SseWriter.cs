using Microsoft.AspNetCore.Http;

namespace RigolStream.Api.Infrastructure;

/// <summary>
/// Minimal Server-Sent-Events writer over an ASP.NET Core <see cref="HttpResponse"/>.
/// Sets the SSE headers once, then emits <c>data:</c> frames and <c>:</c> heartbeat
/// comments, flushing after each so the browser receives them immediately.
/// </summary>
public sealed class SseWriter
{
    private readonly HttpResponse _response;

    public SseWriter(HttpResponse response)
    {
        _response = response;
        _response.Headers.ContentType = "text/event-stream";
        _response.Headers.CacheControl = "no-cache";
        _response.Headers.Connection = "keep-alive";
        _response.Headers["X-Accel-Buffering"] = "no"; // disable proxy buffering
    }

    public async Task WriteJsonAsync(string json, CancellationToken ct)
    {
        await _response.WriteAsync($"data: {json}\n\n", ct);
        await _response.Body.FlushAsync(ct);
    }

    public async Task WriteEventAsync(string eventName, string json, CancellationToken ct)
    {
        await _response.WriteAsync($"event: {eventName}\ndata: {json}\n\n", ct);
        await _response.Body.FlushAsync(ct);
    }

    public async Task HeartbeatAsync(CancellationToken ct)
    {
        await _response.WriteAsync(": ping\n\n", ct);
        await _response.Body.FlushAsync(ct);
    }
}
