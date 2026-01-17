using OpenTK.Mathematics;
using WipeoutRewrite.Infrastructure.Graphics;
using System.Collections.Generic;

namespace WipeoutRewrite.Tests.Infrastructure.Graphics;

/// <summary>
/// Test renderer that implements IRenderer without requiring OpenGL context.
/// Records all rendering calls for verification in tests.
/// </summary>
public class TestRenderer : IRenderer
{
    public record RenderCommand(string Operation);
    
    private readonly List<RenderCommand> _commands = new();
    private int _screenWidth = 800;
    private int _screenHeight = 600;

    public int WhiteTexture => 1;
    public int ScreenWidth => _screenWidth;
    public int ScreenHeight => _screenHeight;
    public List<RenderCommand> Commands => _commands;

    public void Init(int width, int height)
    {
        _screenWidth = width;
        _screenHeight = height;
        RecordCommand("Init");
    }

    public void BeginFrame() => RecordCommand("BeginFrame");
    public void EndFrame() => RecordCommand("EndFrame");
    public void Setup2DRendering() => RecordCommand("Setup2D");
    public void EndFrame2D() => RecordCommand("EndFrame2D");
    public void Flush() => RecordCommand("Flush");
    public void PushSprite(float x, float y, float w, float h, Vector4 color) => RecordCommand("PushSprite");
    public void PushTri(Vector3 a, Vector2 uvA, Vector4 colorA, Vector3 b, Vector2 uvB, Vector4 colorB, Vector3 c, Vector2 uvC, Vector4 colorC) => RecordCommand("PushTri");
    public void RenderVideoFrame(int videoTextureId, int videoWidth, int videoHeight, int windowWidth, int windowHeight) => RecordCommand("RenderVideo");
    public void RenderVideoFrame(byte[] frameData, int videoWidth, int videoHeight, int windowWidth, int windowHeight) => RecordCommand("RenderVideoData");
    public void LoadSpriteTexture(string path) => RecordCommand("LoadTexture");
    public void SetCurrentTexture(int textureId) => RecordCommand("SetTexture");
    public int GetCurrentTexture() => 1;
    public void Cleanup() => RecordCommand("Cleanup");
    public void SetProjectionMatrix(Matrix4 projection) => RecordCommand("SetProjection");
    public void SetViewMatrix(Matrix4 view) => RecordCommand("SetView");
    public void SetModelMatrix(Matrix4 model) => RecordCommand("SetModel");
    public void SetDepthTest(bool enabled) => RecordCommand("SetDepthTest");
    public void SetDepthWrite(bool enabled) => RecordCommand("SetDepthWrite");
    public void SetAlphaTest(bool enabled) => RecordCommand("SetAlphaTest");
    public void SetBlending(bool enabled) => RecordCommand("SetBlending");
    public void SetFaceCulling(bool enabled) => RecordCommand("SetFaceCulling");
    public void SetPassthroughProjection(bool enabled) => RecordCommand("SetPassthrough");
    public void SetDirectionalLight(Vector3 direction, Vector3 color, float intensity) => RecordCommand("SetLight");
    public void SetLightingEnabled(bool enabled) => RecordCommand("SetLighting");

    public void UpdateScreenSize(int width, int height)
    {
        _screenWidth = width;
        _screenHeight = height;
        RecordCommand("UpdateSize");
    }

    public int CreateTexture(byte[] pixels, int width, int height) => 1;

    private void RecordCommand(string operation) => _commands.Add(new RenderCommand(operation));
}
