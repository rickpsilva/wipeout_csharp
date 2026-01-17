using Xunit;
using WipeoutRewrite.Core.Graphics;

namespace WipeoutRewrite.Tests.Core.Graphics;

public class GT4Tests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var gt4 = new GT4();

        Assert.Equal(PrimitiveType.GT4, gt4.Type);
        Assert.Equal(4, gt4.CoordIndices.Length);
        Assert.Equal(4, gt4.UVs.Length);
        Assert.Equal(4, gt4.Colors.Length);
        Assert.Equal(4, gt4.UVsF.Length);
        Assert.Equal(0, gt4.TextureHandle);
    }

    [Fact]
    public void CoordIndices_CanBeSet()
    {
        var gt4 = new GT4
        {
            CoordIndices = new short[] { 0, 1, 2, 3 }
        };

        Assert.Equal(0, gt4.CoordIndices[0]);
        Assert.Equal(1, gt4.CoordIndices[1]);
        Assert.Equal(2, gt4.CoordIndices[2]);
        Assert.Equal(3, gt4.CoordIndices[3]);
    }

    [Fact]
    public void TextureId_CanBeSet()
    {
        var gt4 = new GT4
        {
            TextureId = 5
        };

        Assert.Equal(5, gt4.TextureId);
    }

    [Fact]
    public void UVs_CanBeSet()
    {
        var gt4 = new GT4
        {
            UVs = new (byte u, byte v)[] { (0, 0), (128, 0), (128, 128), (0, 128) }
        };

        Assert.Equal((byte)0, gt4.UVs[0].u);
        Assert.Equal((byte)128, gt4.UVs[2].u);
    }

    [Fact]
    public void Colors_CanBeSet()
    {
        var gt4 = new GT4
        {
            Colors = new (byte r, byte g, byte b, byte a)[]
            {
                (255, 0, 0, 255),
                (0, 255, 0, 255),
                (0, 0, 255, 255),
                (255, 255, 255, 255)
            }
        };

        Assert.Equal((byte)255, gt4.Colors[0].r);
        Assert.Equal((byte)255, gt4.Colors[1].g);
        Assert.Equal((byte)255, gt4.Colors[2].b);
    }

    [Fact]
    public void UVsF_CanBeSet()
    {
        var gt4 = new GT4
        {
            UVsF = new (float u, float v)[] { (0f, 0f), (1f, 0f), (1f, 1f), (0f, 1f) }
        };

        Assert.Equal(0f, gt4.UVsF[0].u);
        Assert.Equal(1f, gt4.UVsF[2].u);
    }

    [Fact]
    public void TextureHandle_CanBeSet()
    {
        var gt4 = new GT4
        {
            TextureHandle = 10
        };

        Assert.Equal(10, gt4.TextureHandle);
    }

    [Fact]
    public void Flags_CanBeSet()
    {
        var gt4 = new GT4
        {
            Flags = PrimitiveFlags.SHIP_ENGINE
        };

        Assert.Equal(PrimitiveFlags.SHIP_ENGINE, gt4.Flags);
    }
}
