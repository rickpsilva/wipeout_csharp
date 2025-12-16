using ImGuiNET;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Factory;
using WipeoutRewrite.Tools.Core;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// UI Panel for scene hierarchy and object management.
/// Follows Single Responsibility Principle - only handles scene tree UI.
/// </summary>
public class SceneHierarchyPanel : ISceneHierarchyPanel
{
    public bool IsVisible
    {
        get => _isVisible;
        set => _isVisible = value;
    }

    #region fields
    private int _cameraCounter = 1;
    private readonly ICameraFactory _cameraFactory;
    private bool _isVisible = true;
    private int _lightCounter = 1;
    private readonly ILogger<SceneHierarchyPanel> _logger;
    private readonly IScene _scene;
    #endregion 

    public SceneHierarchyPanel(
        ILogger<SceneHierarchyPanel> logger,
        IScene scene,
        ICameraFactory cameraFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _cameraFactory = cameraFactory ?? throw new ArgumentNullException(nameof(cameraFactory));

        // Initialize counters based on existing entities
        _cameraCounter = _scene.CameraManager.Cameras.Count;
        _lightCounter = _scene.LightManager.Lights.Count;
    }

    public void Render()
    {
        if (!_isVisible) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 400), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Scene", ref _isVisible))
        {
            RenderSceneToolbar();
            ImGui.Separator();
            RenderSceneTree();
        }
        ImGui.End();
    }

    private void RenderSceneToolbar()
    {
        if (ImGui.Button("+ Camera"))
        {
            _cameraCounter++;
            string cameraName = $"Camera {_cameraCounter}";

            // Create new camera using factory
            var newCamera = _cameraFactory.CreateCamera(
                16f / 9f,  // Default aspect ratio
                73.75f);   // Default FOV

            var sceneCamera = _scene.CameraManager.AddCamera(cameraName, newCamera);
            _scene.SelectedCamera = sceneCamera;
            _logger.LogInformation("[SCENE] Added {CameraName}", cameraName);
        }
        ImGui.SameLine();
        if (ImGui.Button("+ Light"))
        {
            _lightCounter++;
            string lightName = $"Light {_lightCounter}";
            var newLight = _scene.LightManager.AddLight(lightName);
            _scene.SelectedLight = newLight;
            _logger.LogInformation("[SCENE] Added {LightName}", lightName);
        }
    }

    private void RenderSceneTree()
    {
        if (ImGui.TreeNodeEx("Cameras", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Create a copy to avoid "Collection was modified" exception when deleting
            var cameras = _scene.CameraManager.Cameras.ToList();
            foreach (var camera in cameras)
            {
                bool isSelected = _scene.SelectedCamera == camera;
                bool isActive = _scene.CameraManager.ActiveCamera == camera;
                string label = isActive ? $"[C] {camera.Name} (Active)" : $"[C] {camera.Name}";

                if (ImGui.Selectable(label, isSelected))
                {
                    _scene.SelectedCamera = camera;
                    _scene.SelectedObject = null;
                    _scene.SelectedLight = null;
                }

                if (isSelected && ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem("Set as Active"))
                    {
                        _scene.CameraManager.SetActiveCamera(camera.Name);
                    }
                    if (ImGui.MenuItem("Rename"))
                    {
                        _logger.LogDebug("[SCENE] Rename camera: {Name}", camera.Name);
                    }
                    if (_scene.CameraManager.Cameras.Count > 1 && ImGui.MenuItem("Delete"))
                    {
                        _logger.LogDebug("[SCENE] Delete camera: {Name}", camera.Name);
                    }
                    ImGui.EndPopup();
                }
            }
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Objects", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Create a copy to avoid "Collection was modified" exception when deleting
            var objects = _scene.Objects.ToList();
            foreach (var obj in objects)
            {
                bool isSelected = _scene.SelectedObject == obj;
                string icon = obj.IsVisible ? "[O]" : "[ ]";
                if (ImGui.Selectable($"{icon} {obj.Name}", isSelected))
                {
                    _scene.SelectedObject = obj;
                    _scene.SelectedCamera = null;
                    _scene.SelectedLight = null;
                }

                if (isSelected && ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem(obj.IsVisible ? "Hide" : "Show"))
                    {
                        obj.IsVisible = !obj.IsVisible;
                    }
                    if (ImGui.MenuItem("Rename"))
                    {
                        _logger.LogDebug("[SCENE] Rename object: {Name}", obj.Name);
                    }
                    if (ImGui.MenuItem("Delete"))
                    {
                        _scene.RemoveObject(obj);
                    }
                    ImGui.EndPopup();
                }
            }
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Lights", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Create a copy to avoid "Collection was modified" exception when deleting
            var lights = _scene.LightManager.Lights.ToList();
            foreach (var light in lights)
            {
                bool isSelected = _scene.SelectedLight == light;
                string icon = light.IsEnabled ? "[L]" : "[l]";
                if (ImGui.Selectable($"{icon} {light.Name}", isSelected))
                {
                    _scene.SelectedLight = light;
                    _scene.SelectedCamera = null;
                    _scene.SelectedObject = null;
                }

                if (isSelected && ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem(light.IsEnabled ? "Disable" : "Enable"))
                    {
                        light.IsEnabled = !light.IsEnabled;
                    }
                    if (ImGui.MenuItem("Rename"))
                    {
                        _logger.LogDebug("[SCENE] Rename light: {Name}", light.Name);
                    }
                    if (ImGui.MenuItem("Delete"))
                    {
                        _scene.LightManager.RemoveLight(light);
                        if (_scene.SelectedLight == light)
                        {
                            _scene.SelectedLight = null;
                        }
                    }
                    ImGui.EndPopup();
                }
            }
            ImGui.TreePop();
        }
    }
}