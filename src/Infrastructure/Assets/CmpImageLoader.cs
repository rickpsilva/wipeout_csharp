using System;
using System.IO;

namespace WipeoutRewrite.Infrastructure.Assets
{
    public class CmpImageLoader
    {
        private const int LZSS_INDEX_BIT_COUNT = 13;
        private const int LZSS_LENGTH_BIT_COUNT = 4;
        private const int LZSS_WINDOW_SIZE = 1 << LZSS_INDEX_BIT_COUNT; // 8192
        private const int LZSS_BREAK_EVEN = (1 + LZSS_INDEX_BIT_COUNT + LZSS_LENGTH_BIT_COUNT) / 9; // 1
        private const int LZSS_END_OF_STREAM = 0;

        public static byte[][] LoadCompressed(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"CMP file not found: {filePath}");
                return Array.Empty<byte[]>();
            }

            byte[] compressedBytes = File.ReadAllBytes(filePath);
            Console.WriteLine($"Loading CMP: {filePath}, size: {compressedBytes.Length} bytes");

            int p = 0;
            int imageCount = GetInt32LE(compressedBytes, ref p);
            Console.WriteLine($"CMP image count: {imageCount}");

            // Calculate total decompressed size
            int[] imageSizes = new int[imageCount];
            int totalDecompressedSize = 0;
            for (int i = 0; i < imageCount; i++)
            {
                imageSizes[i] = GetInt32LE(compressedBytes, ref p);
                totalDecompressedSize += imageSizes[i];
                Console.WriteLine($"  Image {i}: {imageSizes[i]} bytes");
            }

            // Decompress all data at once
            byte[] decompressedBytes = LzssDecompress(compressedBytes, p);
            Console.WriteLine($"Decompressed {decompressedBytes.Length} bytes (expected {totalDecompressedSize})");

            // Split into individual images
            byte[][] images = new byte[imageCount][];
            int offset = 0;
            for (int i = 0; i < imageCount; i++)
            {
                images[i] = new byte[imageSizes[i]];
                Array.Copy(decompressedBytes, offset, images[i], 0, imageSizes[i]);
                offset += imageSizes[i];
            }

            return images;
        }

        private static byte[] LzssDecompress(byte[] inData, int startPos)
        {
            using (var output = new MemoryStream())
            {
                byte[] window = new byte[LZSS_WINDOW_SIZE];
                int currentPosition = 1;
                int inPos = startPos;
                byte inBfileMask = 0x80;
                byte inBfileRack = 0;

                while (true)
                {
                    if (inBfileMask == 0x80)
                    {
                        if (inPos >= inData.Length) break;
                        inBfileRack = inData[inPos++];
                    }

                    bool value = (inBfileRack & inBfileMask) != 0;
                    inBfileMask >>= 1;
                    if (inBfileMask == 0)
                    {
                        inBfileMask = 0x80;
                    }

                    if (value)
                    {
                        // Literal byte
                        int cc = ReadBits(inData, ref inPos, ref inBfileMask, ref inBfileRack, 8);
                        output.WriteByte((byte)cc);
                        window[currentPosition] = (byte)cc;
                        currentPosition = (currentPosition + 1) & (LZSS_WINDOW_SIZE - 1);
                    }
                    else
                    {
                        // Match position and length
                        int matchPosition = ReadBits(inData, ref inPos, ref inBfileMask, ref inBfileRack, LZSS_INDEX_BIT_COUNT);

                        if (matchPosition == LZSS_END_OF_STREAM)
                        {
                            break;
                        }

                        int matchLength = ReadBits(inData, ref inPos, ref inBfileMask, ref inBfileRack, LZSS_LENGTH_BIT_COUNT);
                        matchLength += LZSS_BREAK_EVEN;

                        for (int i = 0; i <= matchLength; i++)
                        {
                            byte cc = window[(matchPosition + i) & (LZSS_WINDOW_SIZE - 1)];
                            output.WriteByte(cc);
                            window[currentPosition] = cc;
                            currentPosition = (currentPosition + 1) & (LZSS_WINDOW_SIZE - 1);
                        }
                    }
                }

                return output.ToArray();
            }
        }

        private static int ReadBits(byte[] data, ref int pos, ref byte mask, ref byte rack, int bitCount)
        {
            int returnValue = 0;
            int bitMask = 1 << (bitCount - 1);

            while (bitMask != 0)
            {
                if (mask == 0x80)
                {
                    if (pos >= data.Length) break;
                    rack = data[pos++];
                }

                if ((rack & mask) != 0)
                {
                    returnValue |= bitMask;
                }

                bitMask >>= 1;
                mask >>= 1;

                if (mask == 0)
                {
                    mask = 0x80;
                }
            }

            return returnValue;
        }

        private static int GetInt32LE(byte[] bytes, ref int position)
        {
            int value = bytes[position] |
                       (bytes[position + 1] << 8) |
                       (bytes[position + 2] << 16) |
                       (bytes[position + 3] << 24);
            position += 4;
            return value;
        }
    }
}
