using Xunit;
using WipeoutRewrite.Infrastructure.Graphics;
using OpenTK.Mathematics;

namespace WipeoutRewrite.Tests.Infrastructure.Graphics;

/// <summary>
/// Examples of testing patterns for GLRenderer without GL context.
/// </summary>
public class RenderingPatternsExamples
{
    /// <summary>
    /// Pattern 1: Test Renderer (Fake/Stub)
    /// Creates a fake renderer that implements IRenderer and records calls.
    /// Best for: High-level rendering logic tests
    /// </summary>
    [Fact]
    public void Pattern1_TestRenderer_RecordsRenderCalls()
    {
        // Arrange
        var renderer = new TestRenderer();
        renderer.Init(1920, 1080);

        // Act
        renderer.BeginFrame();
        renderer.Setup2DRendering();
        renderer.PushSprite(100, 100, 200, 200, new(1, 1, 1, 1));
        renderer.EndFrame2D();

        // Assert
        Assert.NotEmpty(renderer.Commands);
        Assert.Contains(renderer.Commands, cmd => cmd.Operation == "Init");
        Assert.Contains(renderer.Commands, cmd => cmd.Operation == "PushSprite");
    }

    [Fact]
    public void Pattern1_TestRenderer_TracksScreenDimensions()
    {
        // Arrange
        var renderer = new TestRenderer();

        // Act
        renderer.Init(1280, 720);
        renderer.UpdateScreenSize(1920, 1080);

        // Assert
        Assert.Equal(1920, renderer.ScreenWidth);
        Assert.Equal(1080, renderer.ScreenHeight);
    }

    /// <summary>
    /// Pattern 2: Command Pattern
    /// Record rendering commands and execute them later.
    /// Best for: Deferred rendering, replay debugging
    /// </summary>
    [Fact]
    public void Pattern2_CommandQueue_RecordsAndExecutes()
    {
        // Arrange
        var queue = new RenderCommandQueue();
        var testRenderer = new TestRenderer();

        // Act - Record commands
        queue.Enqueue(new BeginFrameCommand());
        queue.Enqueue(new DrawSpriteCommand(100, 100, 200, 200, new(1, 1, 1, 1)));
        queue.Enqueue(new EndFrameCommand());

        // Assert - Verify commands were queued
        Assert.Equal(3, queue.CommandCount);

        // Execute commands on test renderer
        queue.Execute(testRenderer);
        
        // Verify execution recorded on renderer
        Assert.Contains(testRenderer.Commands, cmd => cmd.Operation == "BeginFrame");
        Assert.Contains(testRenderer.Commands, cmd => cmd.Operation == "PushSprite");
        Assert.Contains(testRenderer.Commands, cmd => cmd.Operation == "EndFrame");
    }

    /// <summary>
    /// Pattern 3: Null Object Pattern
    /// Create a no-op renderer for tests that don't care about rendering.
    /// Best for: Game logic tests that shouldn't render
    /// </summary>
    [Fact]
    public void Pattern3_NullObjectRenderer_DoesNothing()
    {
        // Arrange
        var nullRenderer = new NullRenderer();

        // Act - These should all be no-ops
        nullRenderer.Init(1920, 1080);
        nullRenderer.BeginFrame();
        nullRenderer.Setup2DRendering();
        nullRenderer.PushSprite(0, 0, 100, 100, new(1, 1, 1, 1));
        nullRenderer.EndFrame();

        // Assert - Should complete without error
        Assert.NotNull(nullRenderer);
    }
}

/// <summary>
/// Null Object Pattern: No-op renderer for tests.
/// </summary>
public class NullRenderer : IRenderer
{
    public int WhiteTexture => 0;
    public int ScreenWidth => 0;
    public int ScreenHeight => 0;

    public void Init(int width, int height) { }
    public void BeginFrame() { }
    public void EndFrame() { }
    public void Setup2DRendering() { }
    public void EndFrame2D() { }
    public void Flush() { }
    public void PushSprite(float x, float y, float w, float h, Vector4 color) { }
    public void PushTri(Vector3 a, Vector2 uvA, Vector4 colorA, Vector3 b, Vector2 uvB, Vector4 colorB, Vector3 c, Vector2 uvC, Vector4 colorC) { }
    public void RenderVideoFrame(int videoTextureId, int videoWidth, int videoHeight, int windowWidth, int windowHeight) { }
    public void RenderVideoFrame(byte[] frameData, int videoWidth, int videoHeight, int windowWidth, int windowHeight) { }
    public void LoadSpriteTexture(string path) { }
    public void SetCurrentTexture(int textureId) { }
    public int GetCurrentTexture() => 0;
    public void Cleanup() { }
    public void SetProjectionMatrix(Matrix4 projection) { }
    public void SetViewMatrix(Matrix4 view) { }
    public void SetModelMatrix(Matrix4 model) { }
    public void SetDepthTest(bool enabled) { }
    public void SetDepthWrite(bool enabled) { }
    public void SetAlphaTest(bool enabled) { }
    public void SetBlending(bool enabled) { }
    public void SetFaceCulling(bool enabled) { }
    public void SetPassthroughProjection(bool enabled) { }
    public void SetDirectionalLight(Vector3 direction, Vector3 color, float intensity) { }
    public void SetLightingEnabled(bool enabled) { }
    public void UpdateScreenSize(int width, int height) { }
    public int CreateTexture(byte[] pixels, int width, int height) => 0;
}
