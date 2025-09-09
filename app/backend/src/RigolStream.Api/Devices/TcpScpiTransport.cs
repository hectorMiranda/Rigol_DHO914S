using System.Net.Sockets;
using System.Text;
using RigolStream.Api.Scpi;

namespace RigolStream.Api.Devices;

/// <summary>
/// SCPI over a raw TCP socket — the LXI "socket" channel that Rigol scopes expose
/// on port 5555. Accepts resource strings like <c>TCPIP::192.168.1.50::INSTR</c>,
/// <c>192.168.1.50</c> or <c>192.168.1.50:5555</c>. Connects lazily and serializes
/// access with a semaphore (one command in flight at a time).
/// </summary>
public sealed class TcpScpiTransport : IScpiTransport
{
    private const int DefaultPort = 5555;

    private readonly string _host;
    private readonly int _port;
    private readonly int _timeoutMs;
    private readonly SemaphoreSlim _mutex = new(1, 1);

    private TcpClient? _client;
    private NetworkStream? _stream;

    public TcpScpiTransport(string resource, int timeoutMs)
    {
        (_host, _port) = ParseResource(resource);
        _timeoutMs = timeoutMs;
    }

    internal static (string host, int port) ParseResource(string resource)
    {
        if (string.IsNullOrWhiteSpace(resource))
            throw new OscilloscopeException("No VISA resource configured", OscilloscopeErrorKind.Connection);

        var s = resource.Trim();
        // TCPIP::host::INSTR  or  TCPIP0::host::5555::SOCKET
        if (s.StartsWith("TCPIP", StringComparison.OrdinalIgnoreCase))
        {
            var parts = s.Split("::", StringSplitOptions.RemoveEmptyEntries);
            string host = parts.ElementAtOrDefault(1) ?? throw new OscilloscopeException(
                $"Malformed TCPIP resource: '{resource}'", OscilloscopeErrorKind.Connection);
            int port = int.TryParse(parts.ElementAtOrDefault(2), out var p) ? p : DefaultPort;
            return (host, port);
        }
        // host or host:port
        var hp = s.Split(':');
        return hp.Length == 2 && int.TryParse(hp[1], out var pp) ? (hp[0], pp) : (s, DefaultPort);
    }

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (_client is { Connected: true }) return;

        _client = new TcpClient { NoDelay = true, ReceiveTimeout = _timeoutMs, SendTimeout = _timeoutMs };
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_timeoutMs);
            await _client.ConnectAsync(_host, _port, cts.Token);
            _stream = _client.GetStream();
        }
        catch (Exception ex) when (ex is not OscilloscopeException)
        {
            throw new OscilloscopeException(
                $"Could not connect to {_host}:{_port}: {ex.Message}", OscilloscopeErrorKind.Connection, ex);
        }
    }

    public async Task WriteAsync(string command, CancellationToken ct = default)
    {
        await _mutex.WaitAsync(ct);
        try
        {
            await EnsureConnectedAsync(ct);
            await SendAsync(command, ct);
        }
        finally { _mutex.Release(); }
    }

    public async Task<string> QueryAsync(string command, CancellationToken ct = default)
    {
        await _mutex.WaitAsync(ct);
        try
        {
            await EnsureConnectedAsync(ct);
            await SendAsync(command, ct);
            return (await ReadLineAsync(ct)).TrimEnd('\r', '\n');
        }
        finally { _mutex.Release(); }
    }

    public async Task<byte[]> QueryBinaryAsync(string command, CancellationToken ct = default)
    {
        await _mutex.WaitAsync(ct);
        try
        {
            await EnsureConnectedAsync(ct);
            await SendAsync(command, ct);
            return await ReadBlockAsync(ct);
        }
        finally { _mutex.Release(); }
    }

    private async Task SendAsync(string command, CancellationToken ct)
    {
        var bytes = Encoding.ASCII.GetBytes(command.EndsWith('\n') ? command : command + "\n");
        await _stream!.WriteAsync(bytes, ct);
        await _stream.FlushAsync(ct);
    }

    private async Task<string> ReadLineAsync(CancellationToken ct)
    {
        var sb = new StringBuilder();
        var one = new byte[1];
        while (true)
        {
            int n = await _stream!.ReadAsync(one.AsMemory(0, 1), ct);
            if (n == 0) break;
            char c = (char)one[0];
            if (c == '\n') break;
            sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Read an IEEE 488.2 definite-length block: <c>#&lt;ndigits&gt;&lt;length&gt;&lt;payload&gt;\n</c>.
    /// Falls back to reading a single line if no block header is present.
    /// </summary>
    private async Task<byte[]> ReadBlockAsync(CancellationToken ct)
    {
        int first = _stream!.ReadByte();
        if (first != '#')
            return Encoding.ASCII.GetBytes(await ReadLineAsync(ct));

        int ndigits = _stream.ReadByte() - '0';
        if (ndigits is < 1 or > 9)
            throw new OscilloscopeException("Malformed IEEE block header", OscilloscopeErrorKind.Data);

        var lenDigits = new byte[ndigits];
        await ReadExactAsync(lenDigits, ct);
        int length = int.Parse(Encoding.ASCII.GetString(lenDigits));

        var payload = new byte[length];
        await ReadExactAsync(payload, ct);
        _stream.ReadByte(); // trailing newline
        return payload;
    }

    private async Task ReadExactAsync(byte[] buffer, CancellationToken ct)
    {
        int read = 0;
        while (read < buffer.Length)
        {
            int n = await _stream!.ReadAsync(buffer.AsMemory(read), ct);
            if (n == 0) throw new OscilloscopeException("Connection closed mid-transfer", OscilloscopeErrorKind.Data);
            read += n;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _stream?.Dispose();
        _client?.Dispose();
        _mutex.Dispose();
        await Task.CompletedTask;
    }
}
