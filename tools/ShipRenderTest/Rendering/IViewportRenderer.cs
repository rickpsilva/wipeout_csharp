namespace WipeoutRewrite.Tools.Rendering;

/// <summary>
/// Interface for rendering 3D viewport content.
/// Follows Single Responsibility Principle - handles only 3D rendering logic.
/// </summary>
public interface IViewportRenderer
{
    /// <summary>
    /// Get the texture ID of the rendered viewport.
    /// </summary>
    int ViewportTextureId { get; }

    /// <summary>
    /// Initialize viewport framebuffer with specified dimensions.
    /// </summary>
    void InitializeFramebuffer(int width, int height);
    /// <summary>
    /// Render the 3D scene to a framebuffer.
    /// </summary>
    /// <param name="camera">Camera to use for rendering</param>
    /// <param name="width">Viewport width</param>
    /// <param name="height">Viewport height</param>
    void RenderToFramebuffer(ICamera camera, int width, int height);
    /// <summary>
    /// Update viewport size.
    /// </summary>
    void ResizeFramebuffer(int width, int height);
}