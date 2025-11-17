using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WipeoutRewrite.Tools;

// Inline TIM loader for standalone tool
public class TimImageLoader
{
    private const int TIM_TYPE_PALETTED_4_BPP = 0x08;
    private const int TIM_TYPE_PALETTED_8_BPP = 0x09;
    private const int TIM_TYPE_TRUE_COLOR_16_BPP = 0x02;

    public static (byte[] pixels, int width, int height) LoadTim(string filePath, bool transparent = false)
    {
        byte[] bytes = File.ReadAllBytes(filePath);
        return LoadTimFromBytes(bytes, transparent);
    }

    public static (byte[] pixels, int width, int height) LoadTimFromBytes(byte[] bytes, bool transparent = false)
    {
        int p = 0;

        int magic = GetI32LE(bytes, ref p);
        int type = GetI32LE(bytes, ref p);
        
        byte[] palette = new byte[256 * 4];

        if (type == TIM_TYPE_PALETTED_4_BPP || type == TIM_TYPE_PALETTED_8_BPP)
        {
            int headerLength = GetI32LE(bytes, ref p);
            short paletteX = GetI16LE(bytes, ref p);
            short paletteY = GetI16LE(bytes, ref p);
            short paletteColors = GetI16LE(bytes, ref p);
            short palettes = GetI16LE(bytes, ref p);
            
            for (int i = 0; i < paletteColors; i++)
            {
                ushort color = GetU16LE(bytes, ref p);
                var rgba = Tim16BitToRgba(color, transparent);
                palette[i * 4 + 0] = rgba.r;
                palette[i * 4 + 1] = rgba.g;
                palette[i * 4 + 2] = rgba.b;
                palette[i * 4 + 3] = rgba.a;
            }
        }

        int dataSize = GetI32LE(bytes, ref p);
        int pixelsPer16Bit = type == TIM_TYPE_PALETTED_8_BPP ? 2 : type == TIM_TYPE_PALETTED_4_BPP ? 4 : 1;

        short skipX = GetI16LE(bytes, ref p);
        short skipY = GetI16LE(bytes, ref p);
        short entriesPerRow = GetI16LE(bytes, ref p);
        short rows = GetI16LE(bytes, ref p);

        int width = entriesPerRow * pixelsPer16Bit;
        int height = rows;
        int entries = entriesPerRow * rows;

        byte[] pixels = new byte[width * height * 4];
        int pixelPos = 0;

        if (type == TIM_TYPE_TRUE_COLOR_16_BPP)
        {
            for (int i = 0; i < entries; i++)
            {
                var rgba = Tim16BitToRgba(GetU16LE(bytes, ref p), transparent);
                pixels[pixelPos++] = rgba.r;
                pixels[pixelPos++] = rgba.g;
                pixels[pixelPos++] = rgba.b;
                pixels[pixelPos++] = rgba.a;
            }
        }
        else if (type == TIM_TYPE_PALETTED_8_BPP)
        {
            for (int i = 0; i < entries; i++)
            {
                ushort palettePos = GetU16LE(bytes, ref p);
                Array.Copy(palette, (palettePos & 0xff) * 4, pixels, pixelPos, 4); pixelPos += 4;
                Array.Copy(palette, ((palettePos >> 8) & 0xff) * 4, pixels, pixelPos, 4); pixelPos += 4;
            }
        }
        else if (type == TIM_TYPE_PALETTED_4_BPP)
        {
            for (int i = 0; i < entries; i++)
            {
                ushort palettePos = GetU16LE(bytes, ref p);
                Array.Copy(palette, (palettePos & 0xf) * 4, pixels, pixelPos, 4); pixelPos += 4;
                Array.Copy(palette, ((palettePos >> 4) & 0xf) * 4, pixels, pixelPos, 4); pixelPos += 4;
                Array.Copy(palette, ((palettePos >> 8) & 0xf) * 4, pixels, pixelPos, 4); pixelPos += 4;
                Array.Copy(palette, ((palettePos >> 12) & 0xf) * 4, pixels, pixelPos, 4); pixelPos += 4;
            }
        }

        return (pixels, width, height);
    }

    private static (byte r, byte g, byte b, byte a) Tim16BitToRgba(ushort c, bool transparentBit)
    {
        byte r = (byte)(((c >> 0) & 0x1f) << 3);
        byte g = (byte)(((c >> 5) & 0x1f) << 3);
        byte b = (byte)(((c >> 10) & 0x1f) << 3);
        byte a = (c == 0) ? (byte)0x00 : (transparentBit && (c & 0x7fff) == 0) ? (byte)0x00 : (byte)0xff;
        return (r, g, b, a);
    }

    private static int GetI32LE(byte[] bytes, ref int position)
    {
        int value = bytes[position] | (bytes[position + 1] << 8) | (bytes[position + 2] << 16) | (bytes[position + 3] << 24);
        position += 4;
        return value;
    }

    private static short GetI16LE(byte[] bytes, ref int position)
    {
        short value = (short)(bytes[position] | (bytes[position + 1] << 8));
        position += 2;
        return value;
    }

    private static ushort GetU16LE(byte[] bytes, ref int position)
    {
        ushort value = (ushort)(bytes[position] | (bytes[position + 1] << 8));
        position += 2;
        return value;
    }
}

class ExtractTimToPng
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: ExtractTimToPng <input.tim> <output.png>");
            return;
        }

        string inputPath = args[0];
        string outputPath = args[1];

        try
        {
            Console.WriteLine($"Loading TIM: {inputPath}");
            var (pixels, width, height) = TimImageLoader.LoadTim(inputPath, false);
            
            Console.WriteLine($"Image size: {width}x{height}");
            
            // Convert RGBA bytes to ImageSharp image
            using (var image = Image.LoadPixelData<Rgba32>(pixels, width, height))
            {
                image.SaveAsPng(outputPath);
                Console.WriteLine($"✓ Saved to: {outputPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
