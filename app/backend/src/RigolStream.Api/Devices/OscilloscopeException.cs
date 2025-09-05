namespace RigolStream.Api.Devices;

/// <summary>
/// Raised for any instrument-level failure (connection, command, or data
/// decoding). Carries an optional <see cref="Kind"/> so the HTTP layer can map
/// it to an appropriate status code.
/// </summary>
public sealed class OscilloscopeException : Exception
{
    public OscilloscopeErrorKind Kind { get; }

    public OscilloscopeException(string message, OscilloscopeErrorKind kind = OscilloscopeErrorKind.Command, Exception? inner = null)
        : base(message, inner)
    {
        Kind = kind;
    }
}

public enum OscilloscopeErrorKind
{
    Connection,
    Command,
    Timeout,
    Data,
}
