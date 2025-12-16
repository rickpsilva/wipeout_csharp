using System;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using WipeoutRewrite.Infrastructure.Assets;

namespace WipeoutRewrite.Tests
{
    public class TimAlphaDiagnosticsTests
    {
        [Fact]
        public void Dump_CmpTim_AlphaStats_Allsh()
        {
            var cmpPath = "/home/rick/workspace/wipeout-rewrite/wipeout/common/allsh.cmp";
            if (!System.IO.File.Exists(cmpPath)) return;

            var cmpLoader = new CmpImageLoader(NullLogger<CmpImageLoader>.Instance);
            var timLoader = new TimImageLoader(NullLogger<TimImageLoader>.Instance);

            var images = cmpLoader.LoadCompressed(cmpPath);
            Console.WriteLine($"Loaded {images.Length} images from {cmpPath}");

            for (int i = 0; i < images.Length; i++)
            {
                try
                {
                    var (pixels, w, h) = timLoader.LoadTimFromBytes(images[i], false);
                    int alphaZero = 0;
                    for (int p = 0; p < pixels.Length; p += 4)
                    {
                        if (pixels[p + 3] == 0) alphaZero++;
                    }
                    Console.WriteLine($"TIM#{i}: size={w}x{h}, pixels={pixels.Length/4}, alphaZero={alphaZero}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"TIM#{i}: failed to decode: {ex.Message}");
                }
            }

            Assert.True(true);
        }
    }
}
