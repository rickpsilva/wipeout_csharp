using ImGuiNET;
using WipeoutRewrite.Tools.Core;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Panel for editing directional light properties.
/// </summary>
public class LightPanel : ILightPanel
{
    public bool IsVisible { get; set; }

    private readonly IScene _scene;

    public LightPanel(IScene scene)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        IsVisible = false;
    }

    public void Render()
    {
        // Only show if a light is selected
        if (_scene.SelectedLight == null || !IsVisible) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(350, 400), ImGuiCond.FirstUseEver);

        bool isOpen = IsVisible;
        if (ImGui.Begin("Light Properties", ref isOpen))
        {
            var light = _scene.SelectedLight;

            // Light name
            ImGui.SeparatorText("Light: " + light.Name);

            // Enable/Disable
            bool isEnabled = light.IsEnabled;
            if (ImGui.Checkbox("Enabled", ref isEnabled))
            {
                light.IsEnabled = isEnabled;
            }

            ImGui.Spacing();

            // Direction - use spherical coordinates (angles) for easier editing
            ImGui.SeparatorText("Direction");

            // Calculate current angles from direction vector
            var dir = light.Direction;
            float pitch = MathF.Asin(-dir.Y) * (180f / MathF.PI); // -90 to +90 degrees
            float yaw = MathF.Atan2(dir.X, dir.Z) * (180f / MathF.PI); // -180 to +180 degrees

            bool directionChanged = false;

            ImGui.Text("Horizontal (Yaw):");
            if (ImGui.SliderFloat("##yaw", ref yaw, -180f, 180f, "%.1f°"))
            {
                directionChanged = true;
            }

            ImGui.Text("Vertical (Pitch):");
            if (ImGui.SliderFloat("##pitch", ref pitch, -90f, 90f, "%.1f°"))
            {
                directionChanged = true;
            }

            if (directionChanged)
            {
                // Convert angles back to direction vector
                float yawRad = yaw * (MathF.PI / 180f);
                float pitchRad = pitch * (MathF.PI / 180f);

                float cosPitch = MathF.Cos(pitchRad);
                light.Direction = new Vec3(
                    MathF.Sin(yawRad) * cosPitch,
                    -MathF.Sin(pitchRad),
                    MathF.Cos(yawRad) * cosPitch
                );
            }

            // Show raw XYZ values (read-only)
            ImGui.Spacing();
            ImGui.Text($"X: {dir.X:F3}  Y: {dir.Y:F3}  Z: {dir.Z:F3}");

            ImGui.Spacing();

            // Color
            ImGui.SeparatorText("Color");
            var color = light.Color;
            var colorVec3 = new System.Numerics.Vector3(color.X, color.Y, color.Z);

            if (ImGui.ColorEdit3("RGB", ref colorVec3))
            {
                light.Color = new Vec3(colorVec3.X, colorVec3.Y, colorVec3.Z);
            }

            ImGui.Spacing();

            // Intensity
            ImGui.SeparatorText("Intensity");
            float intensity = light.Intensity;
            if (ImGui.SliderFloat("##Intensity", ref intensity, 0f, 5f))
            {
                light.Intensity = intensity;
            }

            ImGui.Spacing();

            // Preset directions
            ImGui.SeparatorText("Presets");
            if (ImGui.Button("From Above"))
            {
                light.Direction = new Vec3(0, -1, 0);
            }
            ImGui.SameLine();
            if (ImGui.Button("From Front"))
            {
                light.Direction = new Vec3(0, 0, -1);
            }
            ImGui.SameLine();
            if (ImGui.Button("From Right"))
            {
                light.Direction = new Vec3(-1, 0, 0);
            }

            ImGui.Spacing();

            // Info
            ImGui.SeparatorText("Info");
            ImGui.Text($"Type: Directional Light");
            ImGui.Text($"Normalized: Yes");
        }

        if (!isOpen)
        {
            _scene.SelectedLight = null;
            IsVisible = false;
        }

        ImGui.End();
    }
}