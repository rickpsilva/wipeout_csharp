using System;
using System.IO;

namespace WipeoutRewrite.Infrastructure.Assets;

public class TimImageLoader
{
    private const int TIM_TYPE_PALETTED_4_BPP = 0x08;
    private const int TIM_TYPE_PALETTED_8_BPP = 0x09;
    private const int TIM_TYPE_TRUE_COLOR_16_BPP = 0x02;

    public static (byte[] pixels, int width, int height) LoadTim(string filePath, bool transparent = false)
    {
        byte[] bytes = File.ReadAllBytes(filePath);
        Console.WriteLine($"Loading TIM: {filePath}, size: {bytes.Length} bytes");
        return LoadTimFromBytes(bytes, transparent);
    }

    public static (byte[] pixels, int width, int height) LoadTimFromBytes(byte[] bytes, bool transparent = false)
    {
        int p = 0;

        int magic = GetI32LE(bytes, ref p);
        int type = GetI32LE(bytes, ref p);
        Console.WriteLine($"TIM magic: 0x{magic:X8}, type: 0x{type:X8}");
        
        byte[] palette = new byte[256 * 4]; // RGBA

        // Load palette if paletted
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

        int pixelsPer16Bit = 1;
        if (type == TIM_TYPE_PALETTED_8_BPP)
            pixelsPer16Bit = 2;
        else if (type == TIM_TYPE_PALETTED_4_BPP)
            pixelsPer16Bit = 4;

        short skipX = GetI16LE(bytes, ref p);
        short skipY = GetI16LE(bytes, ref p);
        short entriesPerRow = GetI16LE(bytes, ref p);
        short rows = GetI16LE(bytes, ref p);

        int width = entriesPerRow * pixelsPer16Bit;
        int height = rows;
        int entries = entriesPerRow * rows;
        
        Console.WriteLine($"TIM dimensions: {width}x{height}, entries: {entries}, pixelsPer16Bit: {pixelsPer16Bit}");

        byte[] pixels = new byte[width * height * 4]; // RGBA
        int pixelPos = 0;

        if (type == TIM_TYPE_TRUE_COLOR_16_BPP)
        {
            for (int i = 0; i < entries; i++)
            {
                ushort color = GetU16LE(bytes, ref p);
                var rgba = Tim16BitToRgba(color, transparent);
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
                short palettePos = GetI16LE(bytes, ref p);
                int idx1 = (palettePos >> 0) & 0xff;
                int idx2 = (palettePos >> 8) & 0xff;
                
                pixels[pixelPos++] = palette[idx1 * 4 + 0];
                pixels[pixelPos++] = palette[idx1 * 4 + 1];
                pixels[pixelPos++] = palette[idx1 * 4 + 2];
                pixels[pixelPos++] = palette[idx1 * 4 + 3];
                
                pixels[pixelPos++] = palette[idx2 * 4 + 0];
                pixels[pixelPos++] = palette[idx2 * 4 + 1];
                pixels[pixelPos++] = palette[idx2 * 4 + 2];
                pixels[pixelPos++] = palette[idx2 * 4 + 3];
            }
        }
        else if (type == TIM_TYPE_PALETTED_4_BPP)
        {
            for (int i = 0; i < entries; i++)
            {
                short palettePos = GetI16LE(bytes, ref p);
                
                for (int j = 0; j < 4; j++)
                {
                    int idx = (palettePos >> (j * 4)) & 0xf;
                    pixels[pixelPos++] = palette[idx * 4 + 0];
                    pixels[pixelPos++] = palette[idx * 4 + 1];
                    pixels[pixelPos++] = palette[idx * 4 + 2];
                    pixels[pixelPos++] = palette[idx * 4 + 3];
                }
            }
        }

        return (pixels, width, height);
    }

    private static (byte r, byte g, byte b, byte a) Tim16BitToRgba(ushort c, bool transparentBit)
    {
        byte r = (byte)(((c >> 0) & 0x1f) << 3);
        byte g = (byte)(((c >> 5) & 0x1f) << 3);
        byte b = (byte)(((c >> 10) & 0x1f) << 3);
        byte a = (c == 0) ? (byte)0x00 : 
                 (transparentBit && (c & 0x7fff) == 0) ? (byte)0x00 : (byte)0xff;
        
        return (r, g, b, a);
    }

    private static int GetI32LE(byte[] bytes, ref int p)
    {
        int value = bytes[p] | (bytes[p + 1] << 8) | (bytes[p + 2] << 16) | (bytes[p + 3] << 24);
        p += 4;
        return value;
    }

    private static short GetI16LE(byte[] bytes, ref int p)
    {
        short value = (short)(bytes[p] | (bytes[p + 1] << 8));
        p += 2;
        return value;
    }

    private static ushort GetU16LE(byte[] bytes, ref int p)
    {
        ushort value = (ushort)(bytes[p] | (bytes[p + 1] << 8));
        p += 2;
        return value;
    }
}
