using OpenTK.Mathematics;
using WipeoutRewrite.Infrastructure.UI;
using Xunit;

namespace WipeoutRewrite.Tests.Infrastructure.UI;

public class GlyphTests
{
    [Fact]
    public void Constructor_InitializesWithCorrectValues()
    {
        var glyph = new Glyph(10, 20, 15);

        Assert.Equal(10, glyph.Offset.X);
        Assert.Equal(20, glyph.Offset.Y);
        Assert.Equal(15, glyph.Width);
    }

    [Fact]
    public void Constructor_WithZeroValues()
    {
        var glyph = new Glyph(0, 0, 0);

        Assert.Equal(0, glyph.Offset.X);
        Assert.Equal(0, glyph.Offset.Y);
        Assert.Equal(0, glyph.Width);
    }

    [Fact]
    public void Constructor_WithLargeValues()
    {
        var glyph = new Glyph(256, 512, 1024);

        Assert.Equal(256, glyph.Offset.X);
        Assert.Equal(512, glyph.Offset.Y);
        Assert.Equal(1024, glyph.Width);
    }

    [Fact]
    public void Constructor_WithNegativeValues()
    {
        var glyph = new Glyph(-10, -20, -15);

        Assert.Equal(-10, glyph.Offset.X);
        Assert.Equal(-20, glyph.Offset.Y);
        Assert.Equal(-15, glyph.Width);
    }

    [Fact]
    public void Offset_CanBeModified()
    {
        var glyph = new Glyph(0, 0, 10);
        glyph.Offset = new Vector2i(100, 200);

        Assert.Equal(100, glyph.Offset.X);
        Assert.Equal(200, glyph.Offset.Y);
    }

    [Fact]
    public void Width_CanBeModified()
    {
        var glyph = new Glyph(10, 20, 5);
        glyph.Width = 50;

        Assert.Equal(50, glyph.Width);
    }
}

public class CharSetTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var charset = new CharSet();

        Assert.Equal(0, charset.Texture);
        Assert.Equal(0, charset.Height);
        Assert.NotNull(charset.Glyphs);
        Assert.Equal(40, charset.Glyphs.Length);
    }

    [Fact]
    public void Texture_CanBeSet()
    {
        var charset = new CharSet
        {
            Texture = 42
        };

        Assert.Equal(42, charset.Texture);
    }

    [Fact]
    public void Height_CanBeSet()
    {
        var charset = new CharSet
        {
            Height = 16
        };

        Assert.Equal(16, charset.Height);
    }

    [Fact]
    public void Glyphs_CanBeSet()
    {
        var glyphs = new Glyph[40];
        glyphs[0] = new Glyph(0, 0, 10);
        glyphs[39] = new Glyph(100, 100, 20);

        var charset = new CharSet
        {
            Glyphs = glyphs
        };

        Assert.Equal(40, charset.Glyphs.Length);
        Assert.Equal(10, charset.Glyphs[0].Width);
        Assert.Equal(20, charset.Glyphs[39].Width);
    }

    [Fact]
    public void Glyphs_IndexAccessible()
    {
        var charset = new CharSet();
        var glyph = new Glyph(5, 10, 25);
        charset.Glyphs[10] = glyph;

        Assert.Equal(25, charset.Glyphs[10].Width);
    }

    [Fact]
    public void CharSet_CanBeConfiguredCompletely()
    {
        var charset = new CharSet
        {
            Texture = 5,
            Height = 12
        };

        // Create glyphs with sample data
        var glyphs = new Glyph[40];
        for (int i = 0; i < 40; i++)
        {
            glyphs[i] = new Glyph(i, i * 2, i * 3);
        }
        charset.Glyphs = glyphs;

        Assert.Equal(5, charset.Texture);
        Assert.Equal(12, charset.Height);
        Assert.Equal(0, charset.Glyphs[0].Offset.X);
        Assert.Equal(39, charset.Glyphs[39].Offset.X);
        Assert.Equal(117, charset.Glyphs[39].Width);
    }
}

public class TextSizeTests
{
    [Fact]
    public void TextSize_HasCorrectValues()
    {
        Assert.Equal(0, (int)TextSize.Size16);
        Assert.Equal(1, (int)TextSize.Size12);
        Assert.Equal(2, (int)TextSize.Size8);
    }

    [Fact]
    public void TextSize_CanBeComparedToInt()
    {
        TextSize size = TextSize.Size16;
        Assert.True(size == TextSize.Size16);
        Assert.False(size == TextSize.Size12);
    }

    [Fact]
    public void TextSize_CanBeConverted()
    {
        int index = (int)TextSize.Size12;
        Assert.Equal(1, index);
    }
}
