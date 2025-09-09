namespace RigolStream.Api.Devices;

/// <summary>
/// Low-level transport to the instrument. Abstracted so the SCPI driver can sit
/// on raw TCP (LXI socket), a VISA wrapper, or a fake in tests.
/// </summary>
public interface IScpiTransport : IAsyncDisposable
{
    /// <summary>Send a command, expecting no response.</summary>
    Task WriteAsync(string command, CancellationToken ct = default);

    /// <summary>Send a query and read the textual response (newline-terminated).</summary>
    Task<string> QueryAsync(string command, CancellationToken ct = default);

    /// <summary>Send a query and read an IEEE 488.2 binary block response.</summary>
    Task<byte[]> QueryBinaryAsync(string command, CancellationToken ct = default);
}
