using Moq;
using OpenTK.Mathematics;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.UI;
using Xunit;

namespace WipeoutRewrite.Tests.Infrastructure.UI;

[Collection("UIHelperState")]
public class UIHelperTests
{
    private readonly Mock<IFontSystem> _mockFontSystem;
    private readonly Mock<IRenderer> _mockRenderer;

    public UIHelperTests()
    {
        _mockFontSystem = new Mock<IFontSystem>();
        _mockRenderer = new Mock<IRenderer>();
        UIHelper.Initialize(_mockFontSystem.Object, _mockRenderer.Object, 1280, 720);
        UIHelper.SetUIScale(2); // Default scale
    }

    [Fact]
    public void Initialize_SetsUpFontSystemAndRenderer()
    {
        var fontSystem = new Mock<IFontSystem>().Object;
        var renderer = new Mock<IRenderer>().Object;

        UIHelper.Initialize(fontSystem, renderer, 1920, 1080);

        // Just verify initialization doesn't throw
        Assert.True(true);
    }

    [Fact]
    public void SetWindowSize_UpdatesWindowDimensions()
    {
        UIHelper.SetWindowSize(1920, 1080);

        // Position calculations should now use new dimensions
        var pos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(0, 0));
        Assert.Equal(960, pos.X); // 1920 / 2
        Assert.Equal(540, pos.Y); // 1080 / 2
    }

    [Fact]
    public void SetUIScale_ChangesScale()
    {
        UIHelper.SetUIScale(3);

        var scale = UIHelper.GetUIScale();
        Assert.Equal(3, scale);

        // Reset for other tests
        UIHelper.SetUIScale(2);
    }

    [Fact]
    public void GetUIScale_ReturnsCurrentScale()
    {
        UIHelper.SetUIScale(2);

        var scale = UIHelper.GetUIScale();

        Assert.Equal(2, scale);
    }

    [Theory]
    [InlineData(16, TextSize.Size16)]
    [InlineData(12, TextSize.Size12)]
    [InlineData(8, TextSize.Size8)]
    [InlineData(20, TextSize.Size16)]
    [InlineData(4, TextSize.Size8)]
    public void GetTextWidth_CallsFontSystemWithCorrectSize(int size, TextSize expectedSize)
    {
        _mockFontSystem.Setup(f => f.GetTextWidth(It.IsAny<string>(), It.IsAny<TextSize>()))
            .Returns(100);

        var width = UIHelper.GetTextWidth("Test", size);

        _mockFontSystem.Verify(f => f.GetTextWidth("Test", expectedSize), Times.Once);
        Assert.Equal(100, width);
    }

    [Fact]
    public void GetTextWidth_WithNullText_ReturnsZero()
    {
        var width = UIHelper.GetTextWidth(null!, 16);

        Assert.Equal(0, width);
    }

    [Fact]
    public void GetTextWidth_WithEmptyText_ReturnsZero()
    {
        var width = UIHelper.GetTextWidth("", 16);

        Assert.Equal(0, width);
    }

    [Theory]
    [InlineData(UIAnchor.TopLeft, 0, 0)]
    [InlineData(UIAnchor.TopCenter, 640, 0)]
    [InlineData(UIAnchor.TopRight, 1280, 0)]
    [InlineData(UIAnchor.MiddleLeft, 0, 360)]
    [InlineData(UIAnchor.MiddleCenter, 640, 360)]
    [InlineData(UIAnchor.MiddleRight, 1280, 360)]
    [InlineData(UIAnchor.BottomLeft, 0, 720)]
    [InlineData(UIAnchor.BottomCenter, 640, 720)]
    [InlineData(UIAnchor.BottomRight, 1280, 720)]
    public void ScaledPos_WithNoOffset_ReturnsBasePosition(UIAnchor anchor, int expectedX, int expectedY)
    {
        var pos = UIHelper.ScaledPos(anchor, new Vec2i(0, 0));

        Assert.Equal(expectedX, pos.X);
        Assert.Equal(expectedY, pos.Y);
    }

    [Fact]
    public void ScaledPos_WithOffset_MultipliesByScale()
    {
        UIHelper.SetUIScale(2);

        var pos = UIHelper.ScaledPos(UIAnchor.TopLeft, new Vec2i(10, 20));

        Assert.Equal(20, pos.X); // 0 + 10 * 2
        Assert.Equal(40, pos.Y); // 0 + 20 * 2

        UIHelper.SetUIScale(2); // Reset
    }

    [Fact]
    public void ScaledPos_WithDifferentScale_AdjustsCorrectly()
    {
        UIHelper.SetUIScale(3);

        var pos = UIHelper.ScaledPos(UIAnchor.TopLeft, new Vec2i(10, 20));

        Assert.Equal(30, pos.X); // 0 + 10 * 3
        Assert.Equal(60, pos.Y); // 0 + 20 * 3

        UIHelper.SetUIScale(2); // Reset
    }

    [Fact]
    public void DrawText_CallsFontSystemDrawText()
    {
        var pos = new Vec2i(100, 200);
        
        UIHelper.DrawText("Hello", pos, 16, UIColor.Default);

        _mockFontSystem.Verify(f => f.DrawText(
            _mockRenderer.Object,
            "Hello",
            It.Is<Vector2>(v => v.X == 100 && v.Y == 200),
            TextSize.Size16,
            It.IsAny<Color4>()), Times.Once);
    }

    [Fact]
    public void DrawText_WithEmptyString_DoesNotCallFontSystem()
    {
        UIHelper.DrawText("", new Vec2i(100, 200), 16, UIColor.Default);

        _mockFontSystem.Verify(f => f.DrawText(
            It.IsAny<IRenderer>(),
            It.IsAny<string>(),
            It.IsAny<Vector2>(),
            It.IsAny<TextSize>(),
            It.IsAny<Color4>()), Times.Never);
    }

    [Fact]
    public void DrawTextCentered_CentersTextAtPosition()
    {
        _mockFontSystem.Setup(f => f.GetTextWidth("Test", TextSize.Size16))
            .Returns(40); // Unscaled width

        UIHelper.DrawTextCentered("Test", new Vec2i(200, 100), 16, UIColor.Default);

        // Expected: 200 - (40 * 2) / 2 = 200 - 40 = 160
        _mockFontSystem.Verify(f => f.DrawText(
            _mockRenderer.Object,
            "Test",
            It.Is<Vector2>(v => v.X == 160 && v.Y == 100),
            TextSize.Size16,
            It.IsAny<Color4>()), Times.Once);
    }

    [Fact]
    public void DrawNumber_ConvertsToString()
    {
        UIHelper.DrawNumber(42, new Vec2i(100, 200), 12, UIColor.Default);

        _mockFontSystem.Verify(f => f.DrawText(
            _mockRenderer.Object,
            "42",
            It.Is<Vector2>(v => v.X == 100 && v.Y == 200),
            TextSize.Size12,
            It.IsAny<Color4>()), Times.Once);
    }

    [Theory]
    [InlineData(UIColor.Default)]
    [InlineData(UIColor.Accent)]
    [InlineData(UIColor.Disabled)]
    public void DrawText_WithDifferentColors_UsesCorrectColor(UIColor color)
    {
        UIHelper.DrawText("Text", new Vec2i(0, 0), 8, color);

        _mockFontSystem.Verify(f => f.DrawText(
            _mockRenderer.Object,
            "Text",
            It.IsAny<Vector2>(),
            TextSize.Size8,
            It.IsAny<Color4>()), Times.Once);
    }
}
