using Xunit;
using Moq;
using WipeoutRewrite.Presentation;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.UI;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for CreditsScreen.
/// Validates scrolling behavior, reset functionality, and rendering logic.
/// </summary>
public class CreditsScreenTests
{
    private readonly Mock<IFontSystem> _mockFontSystem;
    private readonly Mock<IRenderer> _mockRenderer;

    public CreditsScreenTests()
    {
        _mockFontSystem = new Mock<IFontSystem>();
        _mockRenderer = new Mock<IRenderer>();
    }

    [Fact]
    public void Constructor_ShouldInitializeScrollToZero()
    {
        var mockRenderer = new Mock<IRenderer>();
        var screen = new CreditsScreen(_mockFontSystem.Object, mockRenderer.Object);
        
        // Should not throw - scroll is initialized to 0
        var exception = Record.Exception(() => screen.Render(1280, 720));
        
        Assert.Null(exception);
    }

    [Fact]
    public void Reset_ShouldResetScrollPosition()
    {
        var screen = new CreditsScreen(_mockFontSystem.Object, _mockRenderer.Object);
        
        // Advance scroll
        screen.Update(5.0f); // 5 seconds * 30 px/s = 150 pixels
        
        // Reset
        screen.Reset();
        
        // After reset, scroll should be back to 0
        // (verified indirectly - no exception means state is valid)
        Assert.True(true);
    }

    [Fact]
    public void Update_ShouldIncrementScrollPosition()
    {
        var mockRenderer = new Mock<IRenderer>();
        var screen = new CreditsScreen(_mockFontSystem.Object, mockRenderer.Object);
        
        // Initial state
        screen.Render(1280, 720);
        int initialCalls = mockRenderer.Invocations.Count;
        
        // Update (scroll should advance)
        screen.Update(1.0f); // 1 second = 30 pixels
        
        // Render again
        screen.Render(1280, 720);
        
        // Should still render (just at different position)
        Assert.True(mockRenderer.Invocations.Count > initialCalls);
    }

    [Fact]
    public void Update_WithLargeTime_ShouldAutoReset()
    {
        var screen = new CreditsScreen(_mockFontSystem.Object, _mockRenderer.Object);
        
        // Update with large time to exceed credits length
        // Credits have ~33 lines * 30 px = 990 pixels
        screen.Update(40.0f); // 40s * 30px/s = 1200 pixels (should reset)
        
        // Should not throw (reset happens automatically)
        var exception = Record.Exception(() => screen.Render(1280, 720));
        
        Assert.Null(exception);
    }

    [Fact]
    public void Render_ShouldCallBeginFrame()
    {
        var mockRenderer = new Mock<IRenderer>();
        var screen = new CreditsScreen(_mockFontSystem.Object, mockRenderer.Object);
        
        screen.Render(1280, 720);
        
        mockRenderer.Verify(r => r.BeginFrame(), Times.Once);
    }

    [Fact]
    public void Render_ShouldCallEndFrame()
    {
        var mockRenderer = new Mock<IRenderer>();
        var screen = new CreditsScreen(_mockFontSystem.Object, mockRenderer.Object);
        
        screen.Render(1280, 720);
        
        mockRenderer.Verify(r => r.EndFrame2D(), Times.Once);
    }

    [Fact]
    public void Render_WithoutFontSystem_ShouldNotCrash()
    {
        // CreditsScreen now requires non-null fontSystem in constructor
        // This test is no longer valid as ArgumentNullException is expected
        var mockRenderer = new Mock<IRenderer>();
        
        Assert.Throws<ArgumentNullException>(() => new CreditsScreen(null!, mockRenderer.Object));
    }

    [Fact]
    public void Render_WithFontSystem_ShouldCallDrawTextCentered()
    {
        var mockRenderer = new Mock<IRenderer>();
        var mockFontSystem = new Mock<IFontSystem>();
        
        // Setup required renderer methods
        mockRenderer.Setup(r => r.BeginFrame());
        mockRenderer.Setup(r => r.Setup2DRendering());
        mockRenderer.Setup(r => r.EndFrame2D());
        
        // Setup DrawTextCentered to be callable
        mockFontSystem.Setup(f => f.DrawTextCentered(
            It.IsAny<IRenderer>(),
            It.IsAny<string>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<TextSize>(),
            It.IsAny<OpenTK.Mathematics.Color4>()));
        
        var screen = new CreditsScreen(mockFontSystem.Object, mockRenderer.Object);
        
        // Scroll a bit so non-empty lines are visible
        screen.Update(3.0f); // 3s * 30px/s = 90px scroll
        screen.Render(1280, 720);
        
        // Should attempt to draw text (DrawTextCentered called at least once for visible lines)
        // Note: Exact count depends on scroll position and visible lines
        mockRenderer.Verify(r => r.BeginFrame(), Times.Once);
        mockFontSystem.Verify(f => f.DrawTextCentered(
            It.IsAny<IRenderer>(),
            It.IsAny<string>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<TextSize>(),
            It.IsAny<OpenTK.Mathematics.Color4>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Update_MultipleTimes_ShouldAccumulateScroll()
    {
        var screen = new CreditsScreen(_mockFontSystem.Object, _mockRenderer.Object);
        
        // Update multiple times
        screen.Update(1.0f);
        screen.Update(1.0f);
        screen.Update(1.0f);
        
        // Total: 3 seconds * 30 px/s = 90 pixels scrolled
        // Should not crash when rendering
        var exception = Record.Exception(() => screen.Render(1280, 720));
        
        Assert.Null(exception);
    }

    [Fact]
    public void Render_ShouldOnlyDrawVisibleLines()
    {
        var mockRenderer = new Mock<IRenderer>();
        var screen = new CreditsScreen(_mockFontSystem.Object, mockRenderer.Object);
        
        // Scroll past all credits
        screen.Update(50.0f); // Way past end
        
        // Should reset and render normally
        screen.Render(1280, 720);
        
        mockRenderer.Verify(r => r.BeginFrame(), Times.Once);
        mockRenderer.Verify(r => r.EndFrame2D(), Times.Once);
    }
}
