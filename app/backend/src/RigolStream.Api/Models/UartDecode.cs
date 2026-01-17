namespace RigolStream.Api.Models;

/// <summary>One decoded UART word.</summary>
public sealed record UartFrame(double Time, int Value, bool FramingError);

/// <summary>Result of decoding a channel as asynchronous serial (UART).</summary>
public sealed record UartDecodeResult
{
    public required int Channel { get; init; }
    public required int Baud { get; init; }
    public double Threshold { get; init; }
    public required IReadOnlyList<UartFrame> Frames { get; init; }
    public long Timestamp { get; init; }
}
