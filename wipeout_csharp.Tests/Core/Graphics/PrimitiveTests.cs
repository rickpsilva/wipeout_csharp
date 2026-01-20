using WipeoutRewrite.Core.Graphics;
using Xunit;

namespace WipeoutRewrite.Tests.Core.Graphics;

// FT4 Tests - Flat textured quad
public class FT4Tests
{
    [Fact]
    public void Constructor_InitializesType()
    {
        var ft4 = new FT4();
        Assert.Equal(PrimitiveType.FT4, ft4.Type);
    }

    [Fact]
    public void Constructor_InitializesArrays()
    {
        var ft4 = new FT4();
        
        Assert.NotNull(ft4.CoordIndices);
        Assert.Equal(4, ft4.CoordIndices.Length);
        Assert.NotNull(ft4.UVs);
        Assert.Equal(4, ft4.UVs.Length);
        Assert.NotNull(ft4.UVsF);
        Assert.Equal(4, ft4.UVsF.Length);
    }

    [Fact]
    public void CoordIndices_CanBeSet()
    {
        var ft4 = new FT4 { CoordIndices = new short[] { 0, 1, 2, 3 } };
        Assert.Equal(new short[] { 0, 1, 2, 3 }, ft4.CoordIndices);
    }

    [Fact]
    public void TextureId_CanBeSet()
    {
        var ft4 = new FT4 { TextureId = 42 };
        Assert.Equal(42, ft4.TextureId);
    }

    [Fact]
    public void UVs_CanBeSet()
    {
        var ft4 = new FT4();
        ft4.UVs = new (byte u, byte v)[] { (0, 0), (255, 0), (255, 255), (0, 255) };
        
        Assert.Equal((byte)0, ft4.UVs[0].u);
        Assert.Equal((byte)255, ft4.UVs[2].u);
    }

    [Fact]
    public void Color_CanBeSet()
    {
        var ft4 = new FT4 { Color = (128, 64, 32, 255) };
        
        Assert.Equal((byte)128, ft4.Color.r);
        Assert.Equal((byte)64, ft4.Color.g);
        Assert.Equal((byte)32, ft4.Color.b);
        Assert.Equal((byte)255, ft4.Color.a);
    }

    [Fact]
    public void UVsF_CanBeSet()
    {
        var ft4 = new FT4();
        ft4.UVsF = new (float u, float v)[] { (0f, 0f), (1f, 0f), (1f, 1f), (0f, 1f) };
        
        Assert.Equal(0f, ft4.UVsF[0].u);
        Assert.Equal(1f, ft4.UVsF[2].u);
    }

    [Fact]
    public void TextureHandle_CanBeSet()
    {
        var ft4 = new FT4 { TextureHandle = 100 };
        Assert.Equal(100, ft4.TextureHandle);
    }
}

// G4 Tests - Gouraud shaded quad
public class G4Tests
{
    [Fact]
    public void Constructor_InitializesType()
    {
        var g4 = new G4();
        Assert.Equal(PrimitiveType.G4, g4.Type);
    }

    [Fact]
    public void Constructor_InitializesArrays()
    {
        var g4 = new G4();
        
        Assert.NotNull(g4.CoordIndices);
        Assert.Equal(4, g4.CoordIndices.Length);
        Assert.NotNull(g4.Colors);
        Assert.Equal(4, g4.Colors.Length);
    }

    [Fact]
    public void CoordIndices_CanBeSet()
    {
        var g4 = new G4 { CoordIndices = new short[] { 10, 20, 30, 40 } };
        Assert.Equal(new short[] { 10, 20, 30, 40 }, g4.CoordIndices);
    }

    [Fact]
    public void Colors_CanBeSet()
    {
        var g4 = new G4();
        g4.Colors = new (byte r, byte g, byte b, byte a)[]
        {
            (255, 0, 0, 255),
            (0, 255, 0, 255),
            (0, 0, 255, 255),
            (255, 255, 0, 255)
        };
        
        Assert.Equal((byte)255, g4.Colors[0].r);
        Assert.Equal((byte)255, g4.Colors[1].g);
        Assert.Equal((byte)255, g4.Colors[2].b);
    }

    [Fact]
    public void Colors_IndividualElementsAccessible()
    {
        var g4 = new G4();
        g4.Colors[0] = (128, 64, 32, 16);
        
        Assert.Equal((byte)128, g4.Colors[0].r);
        Assert.Equal((byte)64, g4.Colors[0].g);
        Assert.Equal((byte)32, g4.Colors[0].b);
        Assert.Equal((byte)16, g4.Colors[0].a);
    }
}
