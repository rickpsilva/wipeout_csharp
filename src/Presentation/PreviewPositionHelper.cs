using OpenTK.Graphics.OpenGL4;
using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Presentation;

/// <summary>
/// Helper for computing preview positions and viewport areas based on layout.
/// Uses data-driven configuration for flexible, maintainable viewport management.
/// </summary>
public static class PreviewPositionHelper
{
    private const float DefaultScale = 1.0f;

    /// <summary>
    /// Optional override for GL.Viewport used in tests to avoid requiring an OpenGL context.
    /// </summary>
    public static Action<int, int, int, int>? ViewportOverride { get; set; }

    private static readonly Vec3 DefaultCameraOffset = new(0, 3, 8);

    /// <summary>
    /// Viewport configuration lookup table.
    /// Centralized configuration makes it easy to adjust layouts without code changes.
    /// </summary>
    private static readonly Dictionary<PreviewPosition, ViewportConfig> ViewportConfigs = new()
    {
        [PreviewPosition.Center] = new ViewportConfig(
            XRatio: 0.0f, YRatio: 0.0f,
            WidthRatio: 1.0f, HeightRatio: 1.0f,
            CameraOffset: DefaultCameraOffset, Scale: DefaultScale),

        [PreviewPosition.LeftBottom] = new ViewportConfig(
            XRatio: 0.0f, YRatio: 0.0f,
            WidthRatio: 1.0f / 3.0f, HeightRatio: 1.0f / 3.0f,
            CameraOffset: DefaultCameraOffset, Scale: DefaultScale),

        [PreviewPosition.RightBottom] = new ViewportConfig(
            XRatio: 2.0f / 3.0f, YRatio: 0.0f,
            WidthRatio: 1.0f / 3.0f, HeightRatio: 1.0f / 3.0f,
            CameraOffset: DefaultCameraOffset, Scale: DefaultScale),

        [PreviewPosition.TopCenter] = new ViewportConfig(
            XRatio: 0.0f, YRatio: 2.0f / 3.0f,
            WidthRatio: 1.0f, HeightRatio: 1.0f / 3.0f,
            CameraOffset: DefaultCameraOffset, Scale: DefaultScale),

        [PreviewPosition.BottomCenter] = new ViewportConfig(
            XRatio: 0.0f, YRatio: 0.0f,
            WidthRatio: 1.0f, HeightRatio: 0.2f,
            CameraOffset: DefaultCameraOffset, Scale: DefaultScale)
    };

    #region methods

    /// <summary>
    /// Applies viewport positioning for a specific preview area.
    /// This physically positions where on the screen each preview will render.
    /// </summary>
    public static void ApplyPositionLayout(PreviewPosition position, int screenWidth, int screenHeight)
    {
        var (X, Y, Width, Height) = CalculateViewport(position, screenWidth, screenHeight);
        (ViewportOverride ?? GL.Viewport)(X, Y, Width, Height);
    }

    /// <summary>
    /// Calculates viewport dimensions for a given position without applying them.
    /// Useful for testing or custom viewport management.
    /// </summary>
    public static (int X, int Y, int Width, int Height) CalculateViewport(
        PreviewPosition position, int screenWidth, int screenHeight)
    {
        var config = GetConfig(position);
        return (
            X: (int)(config.XRatio * screenWidth),
            Y: (int)(config.YRatio * screenHeight),
            Width: (int)(config.WidthRatio * screenWidth),
            Height: (int)(config.HeightRatio * screenHeight)
        );
    }

    /// <summary>
    /// Gets the camera offset for a given preview position.
    /// Used for fine-tuning the view within a viewport.
    /// </summary>
    public static Vec3 GetCameraOffsetForPosition(PreviewPosition position)
        => GetConfig(position).CameraOffset;

    /// <summary>
    /// Gets the complete configuration for a preview position.
    /// </summary>
    public static ViewportConfig GetConfig(PreviewPosition position)
    {
        if (ViewportConfigs.TryGetValue(position, out var config))
            return config;

        // Fallback to Center configuration for unknown positions
        return ViewportConfigs[PreviewPosition.Center];
    }

    /// <summary>
    /// Gets the scale factor for rendering at a specific position.
    /// </summary>
    public static float GetScaleForPosition(PreviewPosition position)
        => GetConfig(position).Scale;

    /// <summary>
    /// Allows runtime customization of viewport configurations.
    /// Useful for dynamic layouts or testing different configurations.
    /// </summary>
    public static void SetCustomConfig(PreviewPosition position, ViewportConfig config)
    {
        ViewportConfigs[position] = config;
    }

    #endregion 
}

/// <summary>
/// Configuration for viewport layout calculation
/// </summary>
public readonly record struct ViewportConfig(
    float XRatio,       // X position as ratio of screen width (0.0 to 1.0)
    float YRatio,       // Y position as ratio of screen height (0.0 to 1.0)
    float WidthRatio,   // Width as ratio of screen width (0.0 to 1.0)
    float HeightRatio,  // Height as ratio of screen height (0.0 to 1.0)
    Vec3 CameraOffset,  // Camera position offset
    float Scale         // Render scale factor
);