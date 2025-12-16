namespace WipeoutRewrite.Tools.UI;

public interface IViewportInfoPanel : IUIPanel
{
    /// <summary>
    /// Event triggered when a reset camera is requested.
    /// </summary>
    event Action? OnResetCameraRequested;

    /// <summary>
    /// Gets or sets whether auto-rotate is enabled.
    /// </summary>
    bool AutoRotate { get; set; }
    /// <summary>
    /// Gets or sets the axis for auto-rotation (0 = X, 1 =
    /// Y, 2 = Z).
    /// </summary>
    int AutoRotateAxis { get; set; }
}