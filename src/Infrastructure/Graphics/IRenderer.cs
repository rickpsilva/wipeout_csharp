using OpenTK.Mathematics;

namespace WipeoutRewrite.Infrastructure.Graphics;

/// <summary>
/// Interface para sistema de rendering.
/// Allows swapping implementations (OpenGL, DirectX, Vulkan) and facilitates testing.
/// </summary>
public interface IRenderer
{
    /// <summary>
    /// Initializes renderer with screen dimensions.
    /// </summary>
    void Init(int screenWidth, int screenHeight);
    
    /// <summary>
    /// Inicia um novo frame de rendering.
    /// </summary>
    void BeginFrame();
    
    /// <summary>
    /// Finaliza o frame atual.
    /// </summary>
    void EndFrame();
    
    /// <summary>
    /// Flushes the accumulated rendering batches to the GPU.
    /// Call this after rendering scene objects to ensure visibility.
    /// </summary>
    void Flush();
    
    /// <summary>
    /// Setup 2D orthographic rendering (for sprites, UI, video)
    /// </summary>
    void Setup2DRendering();
    
    /// <summary>
    /// End frame for 2D rendering (uses simple shader without ambient lighting)
    /// </summary>
    void EndFrame2D();
    
    /// <summary>
    /// Adiciona um sprite ao batch de rendering.
    /// </summary>
    void PushSprite(float x, float y, float width, float height, Vector4 color);
    
    /// <summary>
    /// Adds a triangle to rendering batch.
    /// </summary>
    void PushTri(Vector3 a, Vector2 uvA, Vector4 colorA,
                 Vector3 b, Vector2 uvB, Vector4 colorB,
                 Vector3 c, Vector2 uvC, Vector4 colorC);
    
    /// <summary>
    /// Renders a video frame in fullscreen.
    /// </summary>
    void RenderVideoFrame(int textureId, int videoWidth, int videoHeight, 
                         int windowWidth, int windowHeight);
    
    /// <summary>
    /// Renders a video frame from raw RGBA pixel data.
    /// </summary>
    void RenderVideoFrame(byte[] frameData, int videoWidth, int videoHeight,
                         int windowWidth, int windowHeight);
    
    /// <summary>
    /// Carrega textura de sprite a partir de arquivo.
    /// </summary>
    void LoadSpriteTexture(string path);
    
    /// <summary>
    /// Updates screen dimensions (when window is resized).
    /// </summary>
    void UpdateScreenSize(int width, int height);
    
    /// <summary>
    /// Libera recursos do renderer.
    /// </summary>
    void Cleanup();
    
    /// <summary>
    /// Define a textura atual para rendering.
    /// </summary>
    void SetCurrentTexture(int textureId);

    /// <summary>
    /// Enable or disable dynamic lighting calculations.
    /// When disabled, only vertex colors * 2.0 are used (like original PS1).
    /// </summary>
    void SetLightingEnabled(bool enabled);

    /// <summary>
    /// Get the white texture handle for solid color rendering.
    /// </summary>
    int WhiteTexture { get; }

    /// <summary>
    /// Current screen width in pixels.
    /// </summary>
    int ScreenWidth { get; }

    /// <summary>
    /// Current screen height in pixels.
    /// </summary>
    int ScreenHeight { get; }

    /// <summary>
    /// Enable or disable depth testing (for 3D vs 2D rendering).
    /// </summary>
    void SetDepthTest(bool enabled);

    /// <summary>
    /// Enable or disable depth *writes* (depth mask). Useful for rendering translucent
    /// geometry: keep depth testing enabled but prevent writing to the depth buffer.
    /// </summary>
    void SetDepthWrite(bool enabled);

    /// <summary>
    /// Enable or disable alpha-test mode (discard fragments whose alpha <= threshold).
    /// Useful for rendering cutout/collage textures that use binary alpha masks.
    /// </summary>
    void SetAlphaTest(bool enabled);

    /// <summary>
    /// Enable or disable blending (used for translucent geometry and overlays).
    /// ShipPreview will disable blending for opaque/cutout pass and enable it for translucent pass.
    /// </summary>
    void SetBlending(bool enabled);

    /// <summary>
    /// Enable passthrough projection (vertices already in screen space).
    /// Used for 3D meshes that have been pre-projected to screen coordinates.
    /// </summary>
    void SetPassthroughProjection(bool enabled);

    /// <summary>
    /// Enable or disable face culling (backface removal) for 3D rendering.
    /// Should be enabled for 3D ships but disabled for 2D UI elements.
    /// </summary>
    void SetFaceCulling(bool enabled);

    void SetProjectionMatrix(Matrix4 projection);

    void SetViewMatrix(Matrix4 view);

    void SetModelMatrix(Matrix4 model);

    /// <summary>
    /// Creates a texture from raw pixel data and returns its handle.
    /// Used for loading shadow textures and other runtime-loaded textures.
    /// </summary>
    int CreateTexture(byte[] pixels, int width, int height);

    /// <summary>
    /// Sets a directional light for the scene.
    /// direction: normalized direction vector pointing FROM the light source
    /// color: RGB color of the light (0-1 range)
    /// intensity: brightness multiplier
    /// </summary>
    void SetDirectionalLight(Vector3 direction, Vector3 color, float intensity);
}
