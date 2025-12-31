using ImGuiNET;
using WipeoutRewrite.Tools.Core;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Panel showing properties of the selected object in the Scene.
/// </summary>
public class PropertiesPanel : IPropertiesPanel, IUIPanel
{
    public bool IsVisible { get; set; } = true;
    public bool ShowSpline { get; set; } = true;
    public bool WireframeMode { get; set; }
    private readonly IScene _scene;

    public PropertiesPanel(IScene scene)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));

        // Default: Enable spline visualization for easier debugging
        ShowSpline = true;
    }

    public void Render()
    {
        if (!IsVisible) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 400), ImGuiCond.FirstUseEver);

        bool isVisible = IsVisible;
        if (ImGui.Begin("Properties", ref isVisible))
        {
            IsVisible = isVisible;

            var selectedObject = _scene.SelectedObject;

            if (selectedObject != null)
            {
                // Object name
                ImGui.SeparatorText("Object");
                ImGui.Text($"Name: {selectedObject.Name}");
                ImGui.Text($"Type: {selectedObject.GetType().Name}");

                ImGui.Separator();
                ImGui.SeparatorText("Transform");
                ImGui.Text($"Position: ({selectedObject.Position.X:F2}, {selectedObject.Position.Y:F2}, {selectedObject.Position.Z:F2})");
                ImGui.Text($"Rotation: ({selectedObject.Rotation.X:F2}, {selectedObject.Rotation.Y:F2}, {selectedObject.Rotation.Z:F2})");
                ImGui.Text($"Scale: {selectedObject.Scale:F2}");

                // Geometry info if it's a model
                if (selectedObject.Ship?.Model != null)
                {
                    var model = selectedObject.Ship.Model;
                    ImGui.Separator();
                    ImGui.SeparatorText("Geometry");
                    ImGui.Text($"Vertices: {model.Vertices.Length}");
                    ImGui.Text($"Primitives: {model.Primitives.Count}");

                    // Texture info
                    if (selectedObject.Ship.Texture != null)
                    {
                        ImGui.Text($"Textures: {selectedObject.Ship.Texture.Length}");
                    }
                }

                ImGui.Separator();
                ImGui.SeparatorText("Rendering");

                bool visible = selectedObject.IsVisible;
                if (ImGui.Checkbox("Visible", ref visible))
                {
                    selectedObject.IsVisible = visible;
                }

                bool wireframe = WireframeMode;
                if (ImGui.Checkbox("Wireframe", ref wireframe))
                {
                    WireframeMode = wireframe;
                }

                bool showSpline = ShowSpline;
                if (ImGui.Checkbox("Spline (Red Debug Line)", ref showSpline))
                {
                    ShowSpline = showSpline;
                }
            }
            else
            {
                ImGui.TextDisabled("No object selected");
                ImGui.Separator();
                ImGui.TextWrapped("Select an object from the Scene panel to view its properties.");
            }
        }
        ImGui.End();
    }
}