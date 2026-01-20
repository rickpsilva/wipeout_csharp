using System.Collections.Generic;
using OpenTK.Mathematics;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.UI;
using Xunit;

namespace WipeoutRewrite.Tests.Infrastructure.UI;

[Collection("UIHelperState")]
public class MenuRendererTests
{
    [Fact]
    public void RenderMenu_VerticalLayout_DrawsTitleAndItems()
    {
        var font = new RecordingFontSystem();
        var renderer = new DummyRenderer();

        UIHelper.Initialize(font, renderer, 800, 600);
        UIHelper.SetUIScale(1);

        var page = new MenuPage
        {
            Title = "MAIN",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
            ItemsAnchor = UIAnchor.TopLeft,
            ItemsPos = new Vec2i(0, 0),
            BlockWidth = 150,
            SelectedIndex = 1
        };

        page.Items.Add(new MenuButton { Label = "START" });
        page.Items.Add(new MenuButton { Label = "OPTIONS" });

        var menuManager = new StubMenuManager(page);

        var menuRenderer = new MenuRenderer(renderer, font);

        menuRenderer.SetWindowSize(800, 600);
        menuRenderer.RenderMenu(menuManager);

        Assert.Contains("MAIN", font.DrawnText);
        Assert.Contains("START", font.DrawnText);
        Assert.Contains("OPTIONS", font.DrawnText);
    }

    [Fact]
    public void RenderMenu_WithHorizontalLayout_RendersItems()
    {
        var font = new RecordingFontSystem();
        var renderer = new DummyRenderer();

        UIHelper.Initialize(font, renderer, 640, 480);
        UIHelper.SetUIScale(1);

        var page = new MenuPage
        {
            Title = "CONFIRM\nEXIT",
            LayoutFlags = MenuLayoutFlags.Horizontal,
            ItemsAnchor = UIAnchor.MiddleCenter,
            ItemsPos = new Vec2i(0, 0),
            BlockWidth = 100,
            SelectedIndex = 0
        };

        page.Items.Add(new MenuButton { Label = "YES" });
        page.Items.Add(new MenuButton { Label = "NO" });

        var menuManager = new StubMenuManager(page);
        var menuRenderer = new MenuRenderer(renderer, font);

        menuRenderer.SetWindowSize(640, 480);
        menuRenderer.RenderMenu(menuManager);

        Assert.Contains("CONFIRM", font.DrawnText);
        Assert.Contains("EXIT", font.DrawnText);
        Assert.Contains("YES", font.DrawnText);
        Assert.Contains("NO", font.DrawnText);
    }

    private sealed class StubMenuManager : IMenuManager
    {
        public MenuPage? CurrentPage { get; private set; }

        public StubMenuManager(MenuPage page)
        {
            CurrentPage = page;
        }

        public bool HandleInput(MenuAction action) => false;
        public void PopPage() { }
        public void PushPage(MenuPage page) => CurrentPage = page;
        public bool ShouldBlink() => false;
        public void Update(float deltaTime) { }
    }

    private sealed class RecordingFontSystem : IFontSystem
    {
        public List<string> DrawnText { get; } = new();

        public void LoadFonts(string assetsPath) { }

        public void DrawText(IRenderer renderer, string text, Vector2 pos, TextSize textSize, Color4 color)
        {
            DrawnText.Add(text);
        }

        public void DrawTextCentered(IRenderer renderer, string text, Vector2 pos, TextSize textSize, Color4 color)
        {
            DrawnText.Add(text);
        }

        public int GetTextWidth(string text, TextSize textSize)
        {
            return text?.Length ?? 0;
        }
    }

    private sealed class DummyRenderer : IRenderer
    {
        public int WhiteTexture => 0;
        public int ScreenWidth => 0;
        public int ScreenHeight => 0;

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
}