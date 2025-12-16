using WipeoutRewrite.Tools.Core;

namespace WipeoutRewrite.Tools.Rendering;

/// <summary>
/// Service responsible for rendering the scene.
/// Follows Interface Segregation Principle - clients only depend on what they need.
/// </summary>
public interface ISceneRenderer
{
    /// <summary>
    /// Render the scene with the specified camera.
    /// </summary>
    void RenderScene(ICamera camera, Scene scene, bool wireframeMode);
    /// <summary>
    /// Render view gizmo.
    /// </summary>
    void RenderViewGizmo(ICamera camera, float x, float y, float size);
    /// <summary>
    /// Render world grid and axes.
    /// </summary>
    void RenderWorldGrid(ICamera camera);
}