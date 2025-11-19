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
}
