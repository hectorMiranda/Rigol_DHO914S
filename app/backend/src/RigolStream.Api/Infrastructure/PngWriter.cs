namespace RigolStream.Api.Infrastructure;

/// <summary>
/// A minimal, dependency-free PNG encoder. Writes 8-bit RGBA using zlib "stored"
/// (uncompressed) deflate blocks, so we get a valid PNG without System.Drawing
/// or any image package — handy for cross-platform Functions and for rendering
/// the simulated scope screenshot.
/// </summary>
public sealed class PngCanvas
{
    public int Width { get; }
    public int Height { get; }
    private readonly byte[] _rgba; // row-major, 4 bytes per pixel

    public PngCanvas(int width, int height)
    {
        Width = width;
        Height = height;
        _rgba = new byte[width * height * 4];
    }

    public void Clear(byte r, byte g, byte b, byte a = 255)
    {
        for (int i = 0; i < _rgba.Length; i += 4)
        {
            _rgba[i] = r; _rgba[i + 1] = g; _rgba[i + 2] = b; _rgba[i + 3] = a;
        }
    }

    public void SetPixel(int x, int y, byte r, byte g, byte b, byte a = 255)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height) return;
        int i = (y * Width + x) * 4;
        _rgba[i] = r; _rgba[i + 1] = g; _rgba[i + 2] = b; _rgba[i + 3] = a;
    }

    /// <summary>Draw a 1px line with Bresenham's algorithm.</summary>
    public void DrawLine(int x0, int y0, int x1, int y1, byte r, byte g, byte b, byte a = 255)
    {
        int dx = Math.Abs(x1 - x0), dy = -Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;
        while (true)
        {
            SetPixel(x0, y0, r, g, b, a);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    /// <summary>Encode the canvas as PNG bytes.</summary>
    public byte[] Encode()
    {
        using var ms = new MemoryStream();
        Span<byte> sig = stackalloc byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        ms.Write(sig);

        // IHDR
        var ihdr = new byte[13];
        WriteBE(ihdr, 0, (uint)Width);
        WriteBE(ihdr, 4, (uint)Height);
        ihdr[8] = 8;  // bit depth
        ihdr[9] = 6;  // colour type RGBA
        ihdr[10] = 0; // compression
        ihdr[11] = 0; // filter
        ihdr[12] = 0; // interlace
        WriteChunk(ms, "IHDR", ihdr);

        // IDAT: filter-0 scanlines wrapped in a zlib stream of stored blocks
        byte[] raw = BuildRawScanlines();
        byte[] zlib = ZlibStore(raw);
        WriteChunk(ms, "IDAT", zlib);

        WriteChunk(ms, "IEND", Array.Empty<byte>());
        return ms.ToArray();
    }

    private byte[] BuildRawScanlines()
    {
        int stride = Width * 4;
        var raw = new byte[Height * (stride + 1)];
        for (int y = 0; y < Height; y++)
        {
            int dst = y * (stride + 1);
            raw[dst] = 0; // filter type: none
            Array.Copy(_rgba, y * stride, raw, dst + 1, stride);
        }
        return raw;
    }

    private static byte[] ZlibStore(byte[] data)
    {
        using var ms = new MemoryStream();
        ms.WriteByte(0x78); // CMF
        ms.WriteByte(0x01); // FLG (no dict, fastest)

        const int max = 65535;
        int offset = 0;
        while (offset < data.Length || offset == 0 && data.Length == 0)
        {
            int len = Math.Min(max, data.Length - offset);
            bool last = offset + len >= data.Length;
            ms.WriteByte((byte)(last ? 1 : 0)); // BFINAL, BTYPE=00 stored
            ms.WriteByte((byte)(len & 0xFF));
            ms.WriteByte((byte)((len >> 8) & 0xFF));
            int nlen = ~len;
            ms.WriteByte((byte)(nlen & 0xFF));
            ms.WriteByte((byte)((nlen >> 8) & 0xFF));
            ms.Write(data, offset, len);
            offset += len;
            if (len == 0) break;
        }

        WriteBE(ms, Adler32(data));
        return ms.ToArray();
    }

    private static void WriteChunk(Stream s, string type, byte[] data)
    {
        Span<byte> lenBuf = stackalloc byte[4];
        WriteBE(lenBuf, 0, (uint)data.Length);
        s.Write(lenBuf);

        var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
        s.Write(typeBytes);
        s.Write(data);

        uint crc = Crc32(typeBytes, data);
        WriteBE(lenBuf, 0, crc);
        s.Write(lenBuf);
    }

    private static void WriteBE(Span<byte> buf, int offset, uint value)
    {
        buf[offset] = (byte)(value >> 24);
        buf[offset + 1] = (byte)(value >> 16);
        buf[offset + 2] = (byte)(value >> 8);
        buf[offset + 3] = (byte)value;
    }

    private static void WriteBE(Stream s, uint value)
    {
        Span<byte> b = stackalloc byte[4];
        WriteBE(b, 0, value);
        s.Write(b);
    }

    private static uint Adler32(byte[] data)
    {
        const uint mod = 65521;
        uint a = 1, b = 0;
        foreach (var d in data)
        {
            a = (a + d) % mod;
            b = (b + a) % mod;
        }
        return (b << 16) | a;
    }

    private static readonly uint[] CrcTable = BuildCrcTable();

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            uint c = n;
            for (int k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
            table[n] = c;
        }
        return table;
    }

    private static uint Crc32(byte[] type, byte[] data)
    {
        uint c = 0xFFFFFFFF;
        foreach (var b in type) c = CrcTable[(c ^ b) & 0xFF] ^ (c >> 8);
        foreach (var b in data) c = CrcTable[(c ^ b) & 0xFF] ^ (c >> 8);
        return c ^ 0xFFFFFFFF;
    }
}
