using Xunit;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Tests.Infrastructure.Graphics;

/// <summary>
/// Unit tests for GLRenderer.
/// 
/// GLRenderer directly calls OpenGL functions, so most tests require a GL context.
/// These tests focus on:
/// 1. Properties that don't require GL initialization
/// 2. Methods that validate parameters before calling GL
/// 3. State tracking that can be verified without GL
/// 
/// Full integration tests with GL context should be done separately.
/// </summary>
public class GLRendererTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Act
        var renderer = new GLRenderer();

        // Assert
        Assert.NotNull(renderer);
    }

    [Fact]
    public void WhiteTexture_InitiallyZero_BeforeInit()
    {
        // Arrange
        var renderer = new GLRenderer();

        // Act
        var whiteTexture = renderer.WhiteTexture;

        // Assert
        // Before Init(), WhiteTexture should be 0 (not initialized)
        Assert.Equal(0, whiteTexture);
    }

    [Fact]
    public void ScreenWidth_InitiallyZero_BeforeInit()
    {
        // Arrange
        var renderer = new GLRenderer();

        // Act
        var width = renderer.ScreenWidth;

        // Assert
        Assert.Equal(0, width);
    }

    [Fact]
    public void ScreenHeight_InitiallyZero_BeforeInit()
    {
        // Arrange
        var renderer = new GLRenderer();

        // Act
        var height = renderer.ScreenHeight;

        // Assert
        Assert.Equal(0, height);
    }

    [Fact]
    public void UpdateScreenSize_UpdatesDimensions()
    {
        // Arrange
        var renderer = new GLRenderer();
        const int expectedWidth = 1920;
        const int expectedHeight = 1080;

        // Act
        renderer.UpdateScreenSize(expectedWidth, expectedHeight);

        // Assert
        Assert.Equal(expectedWidth, renderer.ScreenWidth);
        Assert.Equal(expectedHeight, renderer.ScreenHeight);
    }

    [Fact]
    public void UpdateScreenSize_WithDifferentValues_UpdatesCorrectly()
    {
        // Arrange
        var renderer = new GLRenderer();

        // Act
        renderer.UpdateScreenSize(1280, 720);
        Assert.Equal(1280, renderer.ScreenWidth);
        Assert.Equal(720, renderer.ScreenHeight);

        // Update again with different values
        renderer.UpdateScreenSize(800, 600);

        // Assert
        Assert.Equal(800, renderer.ScreenWidth);
        Assert.Equal(600, renderer.ScreenHeight);
    }

    [Fact]
    public void UpdateScreenSize_WithZero_SetsZero()
    {
        // Arrange
        var renderer = new GLRenderer();
        renderer.UpdateScreenSize(1024, 768);

        // Act
        renderer.UpdateScreenSize(0, 0);

        // Assert
        Assert.Equal(0, renderer.ScreenWidth);
        Assert.Equal(0, renderer.ScreenHeight);
    }

    [Fact]
    public void UpdateScreenSize_WithNegativeValues_SetsNegative()
    {
        // Arrange
        var renderer = new GLRenderer();

        // Act - Renderer doesn't validate negative (GL would handle)
        renderer.UpdateScreenSize(-100, -200);

        // Assert
        Assert.Equal(-100, renderer.ScreenWidth);
        Assert.Equal(-200, renderer.ScreenHeight);
    }

    [Fact]
    public void SetPassthroughProjection_DoesNotThrow()
    {
        // Arrange
        var renderer = new GLRenderer();

        // Act & Assert - Should not throw even without GL context
        // (only sets internal flag)
        renderer.SetPassthroughProjection(true);
        renderer.SetPassthroughProjection(false);
    }

    [Fact]
    public void SetAlphaTest_DoesNotThrow()
    {
        // Arrange
        var renderer = new GLRenderer();

        // Act & Assert - Should not throw even without GL context
        // (only sets internal flag)
        renderer.SetAlphaTest(true);
        renderer.SetAlphaTest(false);
    }

    [Theory]
    [InlineData(1920, 1080)]
    [InlineData(1280, 720)]
    [InlineData(800, 600)]
    [InlineData(3840, 2160)]
    public void UpdateScreenSize_WithCommonResolutions_UpdatesCorrectly(int width, int height)
    {
        // Arrange
        var renderer = new GLRenderer();

        // Act
        renderer.UpdateScreenSize(width, height);

        // Assert
        Assert.Equal(width, renderer.ScreenWidth);
        Assert.Equal(height, renderer.ScreenHeight);
    }

    [Fact]
    public void GLRenderer_ImplementsIRenderer()
    {
        // Verify GLRenderer implements the IRenderer interface
        var rendererType = typeof(GLRenderer);
        Assert.True(typeof(IRenderer).IsAssignableFrom(rendererType),
            "GLRenderer must implement IRenderer interface");
    }
}
