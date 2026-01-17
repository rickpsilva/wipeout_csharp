using System.Collections.Generic;
using OpenTK.Mathematics;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.UI;
using Xunit;

namespace WipeoutRewrite.Tests.Infrastructure.UI;

[Collection("UIHelperState")]
public class MenuDrawFunctionsTests
{
    [Fact]
    public void DrawControlsTable_WhenControlsNotSet_DoesNothing()
    {
        var font = new RecordingFontSystem();
        var renderer = new DummyRenderer();

        UIHelper.Initialize(font, renderer, 640, 480);
        UIHelper.SetUIScale(1);

        MenuDrawFunctions.SetControlsSettings(null);

        var page = new MenuPage
        {
            ItemsPos = new Vec2i(10, 10),
            ItemsAnchor = UIAnchor.TopLeft,
            BlockWidth = 200,
            SelectedIndex = 0
        };

        MenuDrawFunctions.DrawControlsTable(page);

        Assert.Empty(font.DrawnText);
    }

    [Fact]
    public void DrawControlsTable_RendersHeadingsAndBindings()
    {
        var font = new RecordingFontSystem();
        var renderer = new DummyRenderer();

        UIHelper.Initialize(font, renderer, 800, 600);
        UIHelper.SetUIScale(1);

        var controls = new StubControlsSettings();
        controls.SetButtonBinding(RaceAction.Up, InputDevice.Keyboard, 26);      // W
        controls.SetButtonBinding(RaceAction.Up, InputDevice.Joystick, 110);     // A
        controls.SetButtonBinding(RaceAction.Fire, InputDevice.Keyboard, 40);    // RETURN

        MenuDrawFunctions.SetControlsSettings(controls);

        var page = new MenuPage
        {
            ItemsPos = new Vec2i(100, 50),
            ItemsAnchor = UIAnchor.TopLeft,
            BlockWidth = 160,
            SelectedIndex = 0
        };

        MenuDrawFunctions.DrawControlsTable(page);

        Assert.Contains("KEYBOARD", font.DrawnText);
        Assert.Contains("JOYSTICK", font.DrawnText);
        Assert.Contains("UP", font.DrawnText);
        Assert.Contains("DOWN", font.DrawnText);
        Assert.Contains("RETURN", font.DrawnText); // keyboard binding for FIRE
        Assert.Contains("W", font.DrawnText);      // keyboard binding for UP
        Assert.Contains("A", font.DrawnText);      // joystick binding for UP
    }

    private sealed class StubControlsSettings : IControlsSettings
    {
        private readonly Dictionary<(RaceAction action, InputDevice device), uint> _bindings = new();

        public uint GetButtonBinding(RaceAction action, InputDevice device)
        {
            if (_bindings.TryGetValue((action, device), out var value))
                return value;
            return 0;
        }

        public void SetButtonBinding(RaceAction action, InputDevice device, uint button)
        {
            _bindings[(action, device)] = button;
        }

        public bool IsValid() => true;
        public void ResetToDefaults() => _bindings.Clear();
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