using ImGuiNET;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using SysVector2 = System.Numerics.Vector2;
using SysVector3 = System.Numerics.Vector3;
using WipeoutRewrite.Tools.Core;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// UI Panel for object transform editing (position, rotation, scale).
/// Follows Single Responsibility Principle - only handles transform editing UI.
/// </summary>
public class TransformPanel : ITransformPanel
{
    public bool IsVisible
    {
        get => _isVisible;
        set => _isVisible = value;
    }

    private bool _isVisible = true;
    private readonly ILogger<TransformPanel> _logger;
    private readonly IScene _scene;

    public TransformPanel(ILogger<TransformPanel> logger, IScene scene)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
    }

    public void Render()
    {
        if (!_isVisible) return;

        ImGui.SetNextWindowSize(new SysVector2(300, 250), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Transform", ref _isVisible))
        {
            var selectedObj = _scene.SelectedObject;
            if (selectedObj != null)
            {
                RenderObjectTransform(selectedObj);
            }
            else
            {
                ImGui.TextDisabled("No object selected");
            }
        }
        ImGui.End();
    }

    private void RenderObjectTransform(SceneObject obj)
    {
        ImGui.Text($"Object: {obj.Name}");
        ImGui.Separator();

        // Position
        var pos = obj.Position;
        var posVec = new SysVector3(pos.X, pos.Y, pos.Z);
        if (ImGui.DragFloat3("Position", ref posVec, 0.1f))
        {
            obj.Position = new Vec3(posVec.X, posVec.Y, posVec.Z);
        }

        // Rotation (in degrees for user-friendly editing)
        var rot = obj.Rotation;
        var rotDeg = new SysVector3(
            MathHelper.RadiansToDegrees(rot.X),
            MathHelper.RadiansToDegrees(rot.Y),
            MathHelper.RadiansToDegrees(rot.Z)
        );
        if (ImGui.DragFloat3("Rotation (deg)", ref rotDeg, 1.0f))
        {
            obj.Rotation = new Vec3(
                MathHelper.DegreesToRadians(rotDeg.X),
                MathHelper.DegreesToRadians(rotDeg.Y),
                MathHelper.DegreesToRadians(rotDeg.Z)
            );
        }

        // Scale
        float scale = obj.Scale;
        if (ImGui.DragFloat("Scale", ref scale, 0.01f, 0.01f, 10.0f))
        {
            obj.Scale = scale;
        }

        ImGui.Separator();

        // Quick actions
        if (ImGui.Button("Reset Position"))
        {
            obj.Position = new Vec3(0, 0, 0);
        }
        ImGui.SameLine();
        if (ImGui.Button("Reset Rotation"))
        {
            obj.Rotation = new Vec3(0, 0, 0);
        }
        ImGui.SameLine();
        if (ImGui.Button("Reset Scale"))
        {
            obj.Scale = 0.1f;
        }
    }
}