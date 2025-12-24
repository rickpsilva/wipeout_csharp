using OpenTK.Mathematics;

namespace WipeoutRewrite.Tools.Rendering;

/// <summary>
/// Interface for rendering a view orientation gizmo.
/// </summary>
public interface IViewGizmo : IDisposable
{
    /// <summary>
    /// Renders the view gizmo.
    /// </summary>
    void Render(ICamera camera, Vector2 screenPosition, float snapDistance = 5.0f, float size = 80.0f);

    /// <summary>
    /// Handles input for the view gizmo.
    /// </summary>
    bool HandleInput(ICamera camera, Vector2 screenPosition, Vector2 mousePosition, float snapDistance = 5.0f, float size = 80.0f);
}