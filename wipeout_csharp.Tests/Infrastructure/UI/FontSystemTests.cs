using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using OpenTK.Mathematics;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.UI;
using Xunit;

namespace WipeoutRewrite.Tests.Infrastructure.UI;

[Collection("UIHelperState")]
public class FontSystemTests
{
    private static FontSystem CreateFontSystem(out Mock<ILogger<FontSystem>> logger)
    {
        logger = new Mock<ILogger<FontSystem>>();
        var cmpLoader = new Mock<ICmpImageLoader>(MockBehavior.Strict);
        var timLoader = new Mock<ITimImageLoader>(MockBehavior.Strict);
        return new FontSystem(logger.Object, cmpLoader.Object, timLoader.Object);
    }

    private static void SetLoaded(FontSystem fontSystem)
    {
        var loadedField = typeof(FontSystem).GetField("_loaded", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(loadedField);
        loadedField!.SetValue(fontSystem, true);
    }

    private sealed class StubRenderer : IRenderer
    {
        public int WhiteTexture => 0;
        public int ScreenWidth => 1280;
        public int ScreenHeight => 720;

        public void BeginFrame() { }
        public void Cleanup() { }
        public void EndFrame() { }
        public void EndFrame2D() { }
        public void Flush() { }
        public void Init(int screenWidth, int screenHeight) { }
        public void LoadSpriteTexture(string path) { }
        public void PushSprite(float x, float y, float width, float height, Vector4 color) { }
        public void PushTri(Vector3 a, Vector2 uvA, Vector4 colorA, Vector3 b, Vector2 uvB, Vector4 colorB, Vector3 c, Vector2 uvC, Vector4 colorC) { }
        public void RenderVideoFrame(int textureId, int videoWidth, int videoHeight, int windowWidth, int windowHeight) { }
        public void RenderVideoFrame(byte[] frameData, int videoWidth, int videoHeight, int windowWidth, int windowHeight) { }
        public void SetAlphaTest(bool enabled) { }
        public void SetBlending(bool enabled) { }
        public void SetCurrentTexture(int textureId) { }
        public void SetDepthTest(bool enabled) { }
        public void SetDepthWrite(bool enabled) { }
        public void SetDirectionalLight(Vector3 direction, Vector3 color, float intensity) { }
        public void SetFaceCulling(bool enabled) { }
        public void SetLightingEnabled(bool enabled) { }
        public void SetModelMatrix(Matrix4 model) { }
        public void SetPassthroughProjection(bool enabled) { }
        public void SetProjectionMatrix(Matrix4 projection) { }
        public void SetViewMatrix(Matrix4 view) { }
        public void Setup2DRendering() { }
        public void UpdateScreenSize(int width, int height) { }
        public int CreateTexture(byte[] pixels, int width, int height) => 0;
    }

    [Fact]
    public void GetTextWidth_SumsGlyphWidths()
    {
        // Arrange
        var fontSystem = CreateFontSystem(out _);

        // Act
        int width = fontSystem.GetTextWidth("A A", TextSize.Size16);

        // Assert
        Assert.Equal(58, width); // 25 + 8 (space) + 25
    }

    [Fact]
    public void DrawText_WhenNotLoaded_LogsWarningAndSkips()
    {
        // Arrange
        var fontSystem = CreateFontSystem(out var logger);
        var renderer = new StubRenderer();

        // Act
        fontSystem.DrawText(renderer, "TEST", new Vector2(10, 20), TextSize.Size12, Color4.White);

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fonts not loaded")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void DrawText_WhenLoaded_DoesNotThrowWithStubRenderer()
    {
        // Arrange
        var fontSystem = CreateFontSystem(out _);
        SetLoaded(fontSystem);
        var renderer = new StubRenderer();

        // Act + Assert
        fontSystem.DrawText(renderer, "AB", new Vector2(0, 0), TextSize.Size8, Color4.Red);
    }

    [Fact]
    public void DrawTextCentered_WhenLoaded_AdjustsPositionAndDoesNotThrow()
    {
        // Arrange
        var fontSystem = CreateFontSystem(out _);
        SetLoaded(fontSystem);
        var renderer = new StubRenderer();

        int originalScale = UIHelper.GetUIScale();
        UIHelper.SetUIScale(1);
        try
        {
            // Act + Assert
            fontSystem.DrawTextCentered(renderer, "AB", new Vector2(100, 50), TextSize.Size8, Color4.Blue);
        }
        finally
        {
            UIHelper.SetUIScale(originalScale);
        }
    }
}
