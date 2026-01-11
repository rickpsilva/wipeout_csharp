namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Defines the interface for a properties panel that controls rendering options.
/// </summary>
public interface IPropertiesPanel : IUIPanel
{
    /// <summary>
    /// Gets or sets a value indicating whether the spline should be displayed.
    /// </summary>
    bool ShowSpline { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether wireframe rendering mode is enabled.
    /// </summary>
    bool WireframeMode { get; set; }
}