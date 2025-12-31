using OpenTK.Mathematics;

namespace WipeoutRewrite.Tools.Rendering;

/// <summary>
/// Defines an interface for rendering a world grid in 3D space.
/// </summary>
/// <remarks>
/// This interface provides properties to configure grid appearance and behavior,
/// including size, position, fade distance, and visibility options for the grid
/// and coordinate axes. The <see cref="Render"/> method handles the actual rendering
/// with customizable camera parameters.
/// </remarks>
public interface IWorldGrid
{
    /// <summary>
    /// Gets or sets the distance at which the grid begins to fade out.
    /// </summary>
    float GridFadeDistance { get; set; }

    /// <summary>
    /// Gets or sets the size of individual grid cells.
    /// </summary>
    float GridSize { get; set; }

    /// <summary>
    /// Gets or sets the Y-axis position of the grid in world space.
    /// </summary>
    float GridYPosition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the coordinate axes should be displayed.
    /// </summary>
    bool ShowAxes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the grid should be displayed.
    /// </summary>
    bool ShowGrid { get; set; }

    /// <summary>
    /// Renders the world grid using the specified projection, view, and camera parameters.
    /// </summary>
    /// <param name="projection">The projection matrix for the camera.</param>
    /// <param name="view">The view matrix for the camera.</param>
    /// <param name="cameraPosition">The position of the camera in world space.</param>
    /// <param name="near">The near clipping plane distance. Defaults to 0.1f.</param>
    /// <param name="far">The far clipping plane distance. Defaults to 1000.0f.</param>
    void Render(Matrix4 projection, Matrix4 view, Vector3 cameraPosition, float near = 0.1f, float far = 1000.0f);
}