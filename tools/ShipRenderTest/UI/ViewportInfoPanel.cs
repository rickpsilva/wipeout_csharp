using ImGuiNET;
using WipeoutRewrite.Tools.Core;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Panel showing viewport statistics and auto-rotate controls.
/// </summary>
public class ViewportInfoPanel : IViewportInfoPanel,IUIPanel
{
    // Event for reset camera button
    public event Action? OnResetCameraRequested;

    public bool AutoRotate { get; set; }
    public int AutoRotateAxis { get; set; }
    public bool IsVisible { get; set; } = true;

    private readonly ICamera _camera;
    private readonly IScene _scene;

    public ViewportInfoPanel(IScene scene, ICamera camera)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
    }

    public void Render()
    {
        if (!IsVisible) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 120), ImGuiCond.FirstUseEver);

        bool isVisible = IsVisible;
        if (ImGui.Begin("Viewport Info", ref isVisible))
        {
            IsVisible = isVisible;
            // Toolbar
            if (ImGui.Button("⟲ Reset Camera"))
            {
                OnResetCameraRequested?.Invoke();
            }

            bool autoRotate = AutoRotate;
            ImGui.SameLine();
            if (ImGui.Checkbox("Auto Rotate", ref autoRotate))
            {
                AutoRotate = autoRotate;
            }

            // Auto rotate axis selection
            if (AutoRotate)
            {
                ImGui.Text("Axis:");
                ImGui.SameLine();

                int axis = AutoRotateAxis;
                if (ImGui.RadioButton("X", ref axis, 0))
                    AutoRotateAxis = 0;
                ImGui.SameLine();
                if (ImGui.RadioButton("Y", ref axis, 1))
                    AutoRotateAxis = 1;
                ImGui.SameLine();
                if (ImGui.RadioButton("Z", ref axis, 2))
                    AutoRotateAxis = 2;
            }

            ImGui.Separator();

            // Stats from selected scene object
            var selectedObject = _scene.SelectedObject;
            if (selectedObject != null && selectedObject.Ship?.Model != null)
            {
                ImGui.Text($"Vertices: {selectedObject.Ship.Model.Vertices.Length}");
                ImGui.Text($"Primitives: {selectedObject.Ship.Model.Primitives.Count}");
            }
            else
            {
                ImGui.TextDisabled("No object selected");
            }

            // Camera info
            var pos = _camera?.Position ?? new OpenTK.Mathematics.Vector3(0, 0, 0);
            ImGui.Text($"Camera: ({pos.X:F1}, {pos.Y:F1}, {pos.Z:F1})");
        }
        ImGui.End();
    }
}