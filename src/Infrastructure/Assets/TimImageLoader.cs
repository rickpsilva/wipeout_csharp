using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Infrastructure.Assets
{
    public class TimImageLoader : ITimImageLoader
    {
        private readonly ILogger<TimImageLoader> _logger;
        private const int TIM_TYPE_PALETTED_4_BPP = 0x08;
        private const int TIM_TYPE_PALETTED_8_BPP = 0x09;
        private const int TIM_TYPE_TRUE_COLOR_16_BPP = 0x02;

        public TimImageLoader(
            ILogger<TimImageLoader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public (byte[] pixels, int width, int height) LoadTim(string filePath, bool transparent = false)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            _logger.LogDebug("Loading TIM: {FilePath}, size: {Size} bytes", filePath, bytes.Length);
            return LoadTimFromBytes(bytes, transparent);
        }

        public (byte[] pixels, int width, int height) LoadTimFromBytes(byte[] bytes, bool transparent = false)
        {
            int p = 0;

            int magic = GetI32LE(bytes, ref p);
            int type = GetI32LE(bytes, ref p);
            _logger.LogDebug("TIM magic: 0x{Magic:X8}, type: 0x{Type:X8}", magic, type);
            
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
                    var (r, g, b, a) = Tim16BitToRgba(color, transparent);
                    palette[i * 4 + 0] = r;
                    palette[i * 4 + 1] = g;
                    palette[i * 4 + 2] = b;
                    palette[i * 4 + 3] = a;
                }
            }

            int pixelsPer16Bit = 1;
            if (type == TIM_TYPE_PALETTED_8_BPP)
                pixelsPer16Bit = 2;
            else if (type == TIM_TYPE_PALETTED_4_BPP)
                pixelsPer16Bit = 4;

            // Read pixel data block header (4 bytes for block size)
            int pixelDataLength = GetI32LE(bytes, ref p);
            
            short skipX = GetI16LE(bytes, ref p);
            short skipY = GetI16LE(bytes, ref p);
            short entriesPerRow = GetI16LE(bytes, ref p);
            short rows = GetI16LE(bytes, ref p);

            int width = entriesPerRow * pixelsPer16Bit;
            int height = rows;
            int entries = entriesPerRow * rows;
            
            _logger.LogDebug("TIM dimensions: {Width}x{Height}, entries: {Entries}, pixelsPer16Bit: {PixelsPer16Bit}", width, height, entries, pixelsPer16Bit);

            byte[] pixels = new byte[width * height * 4]; // RGBA
            int pixelPos = 0;

            if (type == TIM_TYPE_TRUE_COLOR_16_BPP)
            {
                for (int i = 0; i < entries; i++)
                {
                    ushort color = GetU16LE(bytes, ref p);
                    var (r, g, b, a) = Tim16BitToRgba(color, transparent);
                    pixels[pixelPos++] = r;
                    pixels[pixelPos++] = g;
                    pixels[pixelPos++] = b;
                    pixels[pixelPos++] = a;
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
            // PS1 TIM 16-bit format: lower 15 bits are color (5:5:5), MSB (0x8000) is the STP bit.
            // Implementation matches wipeout-rewrite/src/wipeout/image.c:tim_16bit_to_rgba()
            byte r = (byte)(((c >> 0) & 0x1f) << 3);
            byte g = (byte)(((c >> 5) & 0x1f) << 3);
            byte b = (byte)(((c >> 10) & 0x1f) << 3);
            byte a;
            
            // Match C logic exactly:
            // The C code checks: (c == 0 ? 0x00 : transparent_bit && (c & 0x7fff) == 0 ? 0x00 : 0xff)
            // This means:
            // 1. If c == 0 (pure black, all bits zero), alpha = 0x00 (transparent)
            // 2. Else if transparentBit AND only MSB is set ((c & 0x7fff) == 0), alpha = 0x00
            // 3. Else alpha = 0xff (opaque)
            // 
            // IMPORTANT: When transparentBit=false (CMP ship textures), pure black (c==0) 
            // is STILL treated as transparent by the C code.
            if (c == 0)
            {
                a = 0x00;
            }
            else if (transparentBit && (c & 0x7fff) == 0)
            {
                a = 0x00;
            }
            else
            {
                a = 0xff;
            }

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
}