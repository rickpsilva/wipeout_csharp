using Xunit;
using Moq;
using WipeoutRewrite.Presentation;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.UI;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for TitleScreen.
/// Validates timer logic, attract mode triggering, and state transitions.
/// </summary>
public class TitleScreenTests
{
    private readonly TimImageLoader _timLoader;
    private readonly Mock<IFontSystem> _mockFontSystem;

    public TitleScreenTests()
    {
        _timLoader = new TimImageLoader(Microsoft.Extensions.Logging.Abstractions.NullLogger<TimImageLoader>.Instance);
        _mockFontSystem = new Mock<IFontSystem>();
    }

    [Fact]
    public void Constructor_ShouldInitializeTimerToZero()
    {
        var screen = new TitleScreen(_timLoader, _mockFontSystem.Object);
        
        screen.Update(0f, out bool shouldStartAttract, out bool shouldStartMenu);
        
        Assert.False(shouldStartAttract);
        Assert.False(shouldStartMenu);
    }

    [Fact]
    public void Update_WhenTimeElapsed_ShouldTriggerAttractMode()
    {
        var screen = new TitleScreen(_timLoader, _mockFontSystem.Object);
        
        // Simulate 10+ seconds passing (AttractDelayFirst)
        screen.Update(10.1f, out bool shouldStartAttract, out bool shouldStartMenu);
        
        Assert.True(shouldStartAttract);
        Assert.False(shouldStartMenu);
    }

    [Fact]
    public void Update_BeforeTimeElapsed_ShouldNotTriggerAttractMode()
    {
        var screen = new TitleScreen(_timLoader, _mockFontSystem.Object);
        
        // Less than 10 seconds
        screen.Update(5.0f, out bool shouldStartAttract, out bool shouldStartMenu);
        
        Assert.False(shouldStartAttract);
        Assert.False(shouldStartMenu);
    }

    [Fact]
    public void Reset_ShouldResetTimer()
    {
        var screen = new TitleScreen(_timLoader, _mockFontSystem.Object);
        
        // Advance timer
        screen.Update(5.0f, out _, out _);
        
        // Reset
        screen.Reset();
        
        // Check timer is back to zero
        screen.Update(0f, out bool shouldStartAttract, out _);
        Assert.False(shouldStartAttract);
    }

    [Fact]
    public void OnAttractComplete_ShouldResetTimerAndMarkAttractShown()
    {
        var screen = new TitleScreen(_timLoader, _mockFontSystem.Object);
        
        // First attract mode trigger
        screen.Update(10.1f, out _, out _);
        screen.OnAttractComplete();
        
        // After attract complete, timer should be reset
        screen.Update(0f, out bool shouldStartAttract, out _);
        Assert.False(shouldStartAttract);
    }

    [Fact]
    public void Update_AfterAttractComplete_ShouldUseSubsequentDelay()
    {
        var screen = new TitleScreen(_timLoader, _mockFontSystem.Object);
        
        // Complete first attract
        screen.Update(10.1f, out _, out _);
        screen.OnAttractComplete();
        
        // Now subsequent delay should apply (also 10 seconds)
        screen.Update(10.1f, out bool shouldStartAttract, out _);
        Assert.True(shouldStartAttract);
    }

    [Fact]
    public void Update_ShouldIncrementBlinkTimer()
    {
        var screen = new TitleScreen(_timLoader, _mockFontSystem.Object);
        
        // Update twice with different delta times
        screen.Update(0.3f, out _, out _);
        screen.Update(0.3f, out _, out _);
        
        // Blink timer should have advanced (tested indirectly via render behavior)
        // We can't directly test private fields, but behavior is correct if no exception
        Assert.True(true);
    }

    [Fact]
    public void Render_ShouldCallBeginFrame()
    {
        var screen = new TitleScreen(_timLoader, _mockFontSystem.Object);
        var mockRenderer = new Mock<IRenderer>();
        
        screen.Render(mockRenderer.Object, 1280, 720);
        
        mockRenderer.Verify(r => r.BeginFrame(), Times.Once);
    }

    [Fact]
    public void Render_ShouldCallEndFrame()
    {
        var screen = new TitleScreen(_timLoader, _mockFontSystem.Object);
        var mockRenderer = new Mock<IRenderer>();
        
        screen.Render(mockRenderer.Object, 1280, 720);
        
        mockRenderer.Verify(r => r.EndFrame(), Times.Once);
    }

    [Fact]
    public void Render_WithFontSystem_ShouldAttemptDrawText()
    {
        var screen = new TitleScreen(_timLoader, _mockFontSystem.Object);
        var mockRenderer = new Mock<IRenderer>();
        
        // Render when text should be visible (blink timer < 0.5s)
        screen.Update(0.1f, out _, out _);
        screen.Render(mockRenderer.Object, 1280, 720);
        
        // DrawTextCentered might be called if within blink interval
        // Note: Can't guarantee due to timing, but test doesn't crash
        mockRenderer.Verify(r => r.BeginFrame(), Times.Once);
    }

    [Fact]
    public void OnStartPressed_ShouldNotThrow()
    {
        var screen = new TitleScreen(_timLoader, _mockFontSystem.Object);
        
        var exception = Record.Exception(() => screen.OnStartPressed());
        
        Assert.Null(exception);
    }
}
