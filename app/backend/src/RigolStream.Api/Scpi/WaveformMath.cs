using RigolStream.Api.Models;

namespace RigolStream.Api.Scpi;

/// <summary>
/// Conversions between raw instrument data and engineering units, ported from
/// the Python <c>utils.py</c>. Kept pure/static so it is trivially unit-testable.
/// </summary>
public static class WaveformMath
{
    /// <summary>
    /// Convert raw BYTE-format ADC codes to volts using the preamble, following
    /// the Rigol convention:
    /// <c>volts = (code - YReference - YOrigin) * YIncrement</c>.
    /// </summary>
    public static double[] BytesToVoltage(ReadOnlySpan<byte> raw, WaveformPreamble p)
    {
        var volts = new double[raw.Length];
        for (int i = 0; i < raw.Length; i++)
            volts[i] = (raw[i] - p.YReference - p.YOrigin) * p.YIncrement;
        return volts;
    }

    /// <summary>
    /// Strip an IEEE 488.2 definite-length block header (<c>#&lt;n&gt;&lt;len&gt;&lt;data&gt;</c>)
    /// and return just the payload. Returns the input unchanged if no header is present.
    /// </summary>
    public static ReadOnlyMemory<byte> StripBlockHeader(ReadOnlyMemory<byte> response)
    {
        var span = response.Span;
        if (span.Length < 2 || span[0] != (byte)'#')
            return response;

        int digits = span[1] - '0';
        if (digits <= 0 || digits > 9 || span.Length < 2 + digits)
            return response;

        int headerLength = 2 + digits;
        // The length field itself is ASCII digits; we trust the trailing bytes are
        // the payload and simply skip the header (matches the Python implementation).
        return response[headerLength..];
    }

    /// <summary>Build the time axis origin/increment for a captured trace.</summary>
    public static (double origin, double increment) TimeAxis(WaveformPreamble p)
        => (p.XOrigin - p.XReference * p.XIncrement, p.XIncrement);
}
