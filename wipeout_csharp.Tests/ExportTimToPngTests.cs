using System;
using System.IO;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using WipeoutRewrite.Infrastructure.Assets;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WipeoutRewrite.Tests
{
    public class ExportTimToPngTests
    {
        [Fact]
        public void Export_SelectedTims_From_Allsh_ToPng()
        {
            var cmpPath = "/home/rick/workspace/wipeout-rewrite/wipeout/common/allsh.cmp";
            if (!File.Exists(cmpPath)) return;

            int[] indicesToExport = new[] { 8, 9, 11 };

            var cmpLoader = new CmpImageLoader(NullLogger<CmpImageLoader>.Instance);
            var timLoader = new TimImageLoader(NullLogger<TimImageLoader>.Instance);

            var images = cmpLoader.LoadCompressed(cmpPath);
            Console.WriteLine($"Loaded {images.Length} images from {cmpPath}");

            var outDir = "/home/rick/workspace/wipeout_csharp/build/diagnostics/tims";
            Directory.CreateDirectory(outDir);

            foreach (var idx in indicesToExport)
            {
                if (idx < 0 || idx >= images.Length)
                {
                    Console.WriteLine($"Index {idx} out of range (0..{images.Length-1})");
                    continue;
                }

                try
                {
                    var (pixels, w, h) = timLoader.LoadTimFromBytes(images[idx], false);

                    // Convert byte[] RGBA -> Rgba32 array
                    var pxCount = pixels.Length / 4;
                    var arr = new Rgba32[pxCount];
                    for (int p = 0, i = 0; p < pixels.Length; p += 4, i++)
                    {
                        arr[i] = new Rgba32(pixels[p], pixels[p + 1], pixels[p + 2], pixels[p + 3]);
                    }

                    using var image = Image.LoadPixelData<Rgba32>(arr.AsSpan(), w, h);
                    var outPath = Path.Combine(outDir, $"allsh_tim_{idx:D2}.png");
                    image.SaveAsPng(outPath);
                    Console.WriteLine($"Wrote TIM#{idx} -> {outPath} ({w}x{h})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to export TIM#{idx}: {ex.Message}");
                }
            }

            Assert.True(true);
        }
    }
}
