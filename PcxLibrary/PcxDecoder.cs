
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PcxLibrary;

public class PcxHeader
{
    public byte Identifier { get; set; }
    public byte Version { get; set; }
    public byte Encoding { get; set; }
    public byte BitsPerPixel { get; set; }
    public ushort XStart { get; set; }
    public ushort YStart { get; set; }
    public ushort XEnd { get; set; }
    public ushort YEnd { get; set; }
    public ushort HorizontalResolution { get; set; }
    public ushort VerticalResolution { get; set; }
    public byte[/* 48 */]? Palette { get; set; }
    public byte Reserved1 { get; set; }
    public byte NumberOfBitPlanes { get; set; }
    public ushort BytesPerScanLine { get; set; }
    public ushort PaletteType { get; set; }
    public ushort HorizontalScreenSize { get; set; }
    public ushort VerticalScreenSize { get; set; }
    public byte[/* 54 */]? Reserverd2 { get; set; }

    public int Width => XEnd - XStart + 1;
    public int Height => YEnd - YStart + 1;
}

public class DecodingProgressEventArgs : EventArgs
{
    public int Progress { get; init; }
    public Image<Rgba32>? Image { get; init; } = null;
}

public delegate void DecodingProgressEventHandler(object sender, DecodingProgressEventArgs a);

public class PcxDecoder
{
    private const int PcxIdentifier = 0x0A;
    private const int HeaderPaletteSize = 48;
    private const int HeaderReserved2Size = 54;
    private const int PcxHasPalette = 0x0C;

    public bool IsPcxFile => header.Identifier == PcxIdentifier;

    public event DecodingProgressEventHandler? DecodingProgress;
    public Image<Rgba32>? Image => image;
    public PcxHeader Header => header;

    private readonly BinaryReader reader;
    private readonly PcxHeader header;
    private Image<Rgba32>? image;


    public PcxDecoder(Stream stream)
    {
        reader = new BinaryReader(new BufferedStream(stream, 4096));
        header = new();
    }

    public void ReadHeader()
    {
        header.Identifier = reader.ReadByte();
        if (!IsPcxFile) return;
        header.Version = reader.ReadByte();
        header.Encoding = reader.ReadByte();
        header.BitsPerPixel = reader.ReadByte();

        header.XStart = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
        header.YStart = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
        header.XEnd = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
        header.YEnd = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
        header.HorizontalResolution = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
        header.VerticalResolution = BitConverter.ToUInt16(reader.ReadBytes(2), 0);

        header.Palette = reader.ReadBytes(48);
        header.Reserved1 = reader.ReadByte();
        header.NumberOfBitPlanes = reader.ReadByte();

        header.BytesPerScanLine = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
        header.PaletteType = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
        header.HorizontalScreenSize = BitConverter.ToUInt16(reader.ReadBytes(2), 0);
        header.VerticalScreenSize = BitConverter.ToUInt16(reader.ReadBytes(2), 0);

        header.Reserverd2 = reader.ReadBytes(54);
    }

    public void DecodeImageInForegroundThread()
    {
        image = new Image<Rgba32>(header.Width, header.Height);

        DecodeImageInternal();
    }

    public void DecodeImageInBackgroundThread()
    {
        image = new Image<Rgba32>(header.Width, header.Height);


        Thread workerThread = new(DecodeImageInternal)
        {
            IsBackground = true
        };
        workerThread.Start();
    }

    private void DecodeImageInternal()
    {
        var palette = ReadPalette(reader, header);

        for (int y = 0; y < header.Height; y++)
        {
            var scanLine = ReadScanLine(reader, header);
            for (int x = 0; x < header.Width; x++)
            {
                Rgba32 pixel;
                if (header.BitsPerPixel == 8 && header.NumberOfBitPlanes == 1)
                {
                    byte paletteIdx = scanLine[x];
                    pixel = palette![paletteIdx];
                }
                else
                {
                    byte r = scanLine[x];
                    byte g = scanLine[x + header.BytesPerScanLine];
                    byte b = scanLine[x + 2 * header.BytesPerScanLine];

                    pixel = new Rgba32(r, g, b, 255);
                }
                image![x, y] = pixel;
            }
            DecodingProgress?.Invoke(this, new DecodingProgressEventArgs { Image = null, Progress = (int)((double)y / header.Height * 100.0) });
        }
        DecodingProgress?.Invoke(this, new DecodingProgressEventArgs { Image = image, Progress = 100 });
    }

    private static Rgba32[]? ReadPalette(BinaryReader reader, PcxHeader header)
    {
        int colorsCount = (1 << header.BitsPerPixel) * header.NumberOfBitPlanes;

        var prevPos = reader.BaseStream.Position;
        reader.BaseStream.Seek(-3 * colorsCount - 1, SeekOrigin.End);
        var hasPalette = reader.ReadByte();
        if (hasPalette != PcxHasPalette)
        {
            reader.BaseStream.Position = prevPos;
            return null;
        }

        Rgba32[] palette = new Rgba32[colorsCount];
        for (int i = 0; i < colorsCount; i++)
        {
            byte r = reader.ReadByte();
            byte g = reader.ReadByte();
            byte b = reader.ReadByte();
            palette[i] = new Rgba32(r, g, b, 255);
        }

        reader.BaseStream.Position = prevPos;
        return palette;
    }

    private static byte[] ReadScanLine(BinaryReader reader, PcxHeader header)
    {
        var scanLineLength = header.BytesPerScanLine * header.NumberOfBitPlanes;
        var scanLineBuffer = new byte[scanLineLength];

        int idx = 0;

        while (idx < scanLineLength)
        {
            var nextByte = reader.ReadByte();

            if ((nextByte & 0xC0) == 0xC0)
            {
                var count = nextByte & 0x3F;
                var value = reader.ReadByte();

                for (int i = 0; i < count && idx < scanLineLength; i++)
                {
                    scanLineBuffer[idx++] = value;
                }
            }
            else
            {
                scanLineBuffer[idx++] = nextByte;
            }
        }
        return scanLineBuffer;

    }
}
