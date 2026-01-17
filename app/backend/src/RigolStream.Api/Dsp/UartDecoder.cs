using RigolStream.Api.Models;

namespace RigolStream.Api.Dsp;

/// <summary>
/// Decodes a captured trace as 8-N-1 (configurable) asynchronous serial. Idle is
/// high; a start bit is a high→low edge; data bits are sampled LSB-first at the
/// centre of each bit period; the stop bit must be high or the frame is flagged.
/// </summary>
public static class UartDecoder
{
    public static UartDecodeResult Decode(Waveform wf, int baud, double? threshold = null, int dataBits = 8, int stopBits = 1)
    {
        double[] v = wf.Voltage;
        double sampleRate = wf.TimeIncrement > 0 ? 1.0 / wf.TimeIncrement : 0;
        double thr = threshold ?? AutoThreshold(v);
        var frames = new List<UartFrame>();

        double samplesPerBit = baud > 0 ? sampleRate / baud : 0;
        if (samplesPerBit < 2 || v.Length == 0)
        {
            // Too few samples per bit to decode reliably.
            return Build(wf, baud, thr, frames);
        }

        bool High(int i) => i >= 0 && i < v.Length && v[i] > thr;
        bool Bit(double pos) => High((int)Math.Round(pos));

        int idx = 0;
        while (idx < v.Length - 1)
        {
            // Seek a high→low edge (start bit).
            if (!(High(idx) && !High(idx + 1)))
            {
                idx++;
                continue;
            }

            double edge = idx + 1;
            // Confirm the start bit is low at its centre.
            if (Bit(edge + samplesPerBit * 0.5))
            {
                idx++;
                continue;
            }

            int value = 0;
            for (int b = 0; b < dataBits; b++)
            {
                double centre = edge + samplesPerBit * (1.5 + b);
                if (Bit(centre)) value |= 1 << b; // LSB first
            }

            double stopCentre = edge + samplesPerBit * (1.5 + dataBits);
            bool framingError = !Bit(stopCentre);

            double time = wf.TimeOrigin + edge * wf.TimeIncrement;
            frames.Add(new UartFrame(time, value, framingError));

            // Advance past this frame (start + data + stop bits).
            idx = (int)Math.Ceiling(edge + samplesPerBit * (1 + dataBits + stopBits));
        }

        return Build(wf, baud, thr, frames);
    }

    private static double AutoThreshold(double[] v)
    {
        if (v.Length == 0) return 0;
        double min = double.MaxValue, max = double.MinValue;
        foreach (var x in v)
        {
            if (x < min) min = x;
            if (x > max) max = x;
        }
        return (min + max) / 2;
    }

    private static UartDecodeResult Build(Waveform wf, int baud, double threshold, List<UartFrame> frames) => new()
    {
        Channel = wf.Channel,
        Baud = baud,
        Threshold = threshold,
        Frames = frames,
        Timestamp = wf.Timestamp,
    };
}
