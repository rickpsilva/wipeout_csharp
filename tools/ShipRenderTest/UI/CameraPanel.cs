using ImGuiNET;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using SysVector2 = System.Numerics.Vector2;
using SysVector3 = System.Numerics.Vector3;
using SysVector4 = System.Numerics.Vector4;
using WipeoutRewrite.Tools.Core;
using WipeoutRewrite.Tools.Managers;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// UI Panel for camera settings and control.
/// Follows Single Responsibility Principle - only handles camera UI.
/// </summary>
public class CameraPanel : ICameraPanel
{
    public bool IsVisible
    {
        get => _isVisible;
        set => _isVisible = value;
    }

    private bool _isVisible = true;
    private readonly ILogger<CameraPanel> _logger;
    private readonly IScene _scene;

    public CameraPanel(ILogger<CameraPanel> logger, IScene scene)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
    }

    public void Render()
    {
        if (!_isVisible) return;

        ImGui.SetNextWindowSize(new SysVector2(300, 300), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Camera", ref _isVisible))
        {
            // Use active camera instead of selected camera
            var activeCamera = _scene.CameraManager.ActiveCamera;
            if (activeCamera != null)
            {
                RenderCameraSettings(activeCamera);
            }
            else
            {
                ImGui.TextDisabled("No active camera");
                ImGui.Text("Create a camera from the Scene panel");
            }
        }
        ImGui.End();
    }

    private void RenderCameraSettings(SceneCamera sceneCamera)
    {
        ImGui.Text($"Camera: {sceneCamera.Name}");

        bool isActive = _scene.CameraManager.ActiveCamera == sceneCamera;
        if (isActive)
        {
            ImGui.SameLine();
            ImGui.TextColored(new SysVector4(0.3f, 1.0f, 0.3f, 1.0f), "(Active)");
        }

        ImGui.Separator();

        var camera = sceneCamera.Camera;

        // Camera position (read-only - controlled by orbit parameters)
        var pos = camera.Position;
        var posVec = new SysVector3(pos.X, pos.Y, pos.Z);
        ImGui.Text($"Position: ({pos.X:F2}, {pos.Y:F2}, {pos.Z:F2})");

        // FOV (already in degrees in Camera.cs)
        float fovDeg = camera.Fov;
        if (ImGui.SliderFloat("FOV (degrees)", ref fovDeg, 30.0f, 120.0f))
        {
            camera.Fov = fovDeg;
        }

        // Orbit camera settings
        ImGui.Separator();
        ImGui.Text("Orbit Settings:");

        float distance = camera.Distance;
        if (ImGui.DragFloat("Distance", ref distance, 0.1f, 1.0f, 200.0f))
        {
            camera.Distance = distance;
        }

        // Yaw and Pitch in degrees
        float yawDeg = MathHelper.RadiansToDegrees(camera.Yaw);
        float pitchDeg = MathHelper.RadiansToDegrees(camera.Pitch);

        if (ImGui.DragFloat("Yaw (degrees)", ref yawDeg, 1.0f))
        {
            camera.Yaw = MathHelper.DegreesToRadians(yawDeg);
        }

        if (ImGui.DragFloat("Pitch (degrees)", ref pitchDeg, 1.0f, -89.0f, 89.0f))
        {
            camera.Pitch = MathHelper.DegreesToRadians(pitchDeg);
        }

        // Look target
        var target = camera.Target;
        var targetVec = new SysVector3(target.X, target.Y, target.Z);
        if (ImGui.DragFloat3("Target", ref targetVec, 0.1f))
        {
            camera.Target = new Vector3(targetVec.X, targetVec.Y, targetVec.Z);
        }

        ImGui.Separator();

        // Quick actions
        if (ImGui.Button("Reset Camera"))
        {
            camera.Target = Vector3.Zero;
            camera.Yaw = 0;
            camera.Pitch = MathHelper.DegreesToRadians(-26.5f);
            camera.Distance = 30;
        }

        if (!isActive)
        {
            if (ImGui.Button("Set as Active Camera"))
            {
                _scene.CameraManager.SetActiveCamera(sceneCamera.Name);
                _logger.LogInformation("[CAMERA] Set {Name} as active", sceneCamera.Name);
            }
        }
    }
}