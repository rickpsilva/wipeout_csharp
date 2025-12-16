using OpenTK.Mathematics;

namespace WipeoutRewrite.Tools.Rendering;

/// <summary>
/// Interface for rendering a world grid for spatial reference.
/// </summary>
public interface IWorldGrid : IDisposable
{
    float GridFadeDistance { get; set; }
    float GridSize { get; set; }
    bool ShowAxes { get; set; }
    bool ShowGrid { get; set; }

    /// <summary>
    /// Renders the world grid.
    /// </summary>
    void Render(Matrix4 projection, Matrix4 view, Vector3 cameraPosition, float near = 0.1f, float far = 1000.0f);
}