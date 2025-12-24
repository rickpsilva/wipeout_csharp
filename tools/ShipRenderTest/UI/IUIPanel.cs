
namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Interface for UI panels in the editor.
/// Follows Single Responsibility Principle - each panel handles its own UI rendering.
/// </summary>
public interface IUIPanel
{
    /// <summary>
    /// Whether this panel is currently visible.
    /// </summary>
    bool IsVisible { get; set; }

    /// <summary>
    /// Render the UI panel using ImGui.
    /// </summary>
    void Render();
}