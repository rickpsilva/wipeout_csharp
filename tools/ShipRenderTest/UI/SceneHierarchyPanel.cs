using ImGuiNET;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Factory;
using WipeoutRewrite.Tools.Core;
using WipeoutRewrite.Tools.Managers;

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
    private bool _openCameraRenamePopup = false;
    private bool _openLightRenamePopup = false;
    private string _renameBuffer = "";
    private SceneCamera? _renamingCamera = null;
    private DirectionalLight? _renamingLight = null;
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

        // Initialize counters based on highest number found in existing entities
        InitializeCounters();
    }

    #region methods

    public void Render()
    {
        if (!_isVisible) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 400), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Scene", ref _isVisible))
        {
            RenderSceneToolbar();
            ImGui.Separator();
            RenderSceneTree();

            // Render popups inside the window
            RenderCameraRenamePopup();
            RenderLightRenamePopup();
        }
        ImGui.End();
    }

    private void InitializeCounters()
    {
        // Find highest camera number
        int maxCameraNum = 0;
        foreach (var camera in _scene.CameraManager.Cameras)
        {
            if (camera.Name.StartsWith("Camera ") && int.TryParse(camera.Name.Substring(7), out var num))
            {
                maxCameraNum = Math.Max(maxCameraNum, num);
            }
        }
        _cameraCounter = maxCameraNum;

        // Find highest light number
        int maxLightNum = 0;
        foreach (var light in _scene.LightManager.Lights)
        {
            if (light.Name.StartsWith("Light ") && int.TryParse(light.Name.Substring(6), out var num))
            {
                maxLightNum = Math.Max(maxLightNum, num);
            }
        }
        _lightCounter = maxLightNum;
    }

    private void RefreshCounters()
    {
        // Always recalculate counters to handle late initialization
        InitializeCounters();
    }

    private void RenderCameraRenamePopup()
    {
        if (_openCameraRenamePopup)
        {
            ImGui.OpenPopup("RenameCameraPopup");
            _openCameraRenamePopup = false;
        }

        bool isRenameCameraOpen = true;
        if (ImGui.BeginPopupModal("RenameCameraPopup", ref isRenameCameraOpen, ImGuiWindowFlags.AlwaysAutoResize))
        {
            // Focus the input on first frame
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }

            if (ImGui.InputText("New Name##cameraRename", ref _renameBuffer, 100, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
            {
                if (!string.IsNullOrWhiteSpace(_renameBuffer) && _renamingCamera != null)
                {
                    _renamingCamera.Name = _renameBuffer;
                    _logger.LogInformation("[SCENE] Renamed camera to: {Name}", _renameBuffer);
                    ImGui.CloseCurrentPopup();
                }
            }

            if (ImGui.Button("OK##cameraRenameOK", new System.Numerics.Vector2(120, 0)))
            {
                if (!string.IsNullOrWhiteSpace(_renameBuffer) && _renamingCamera != null)
                {
                    _renamingCamera.Name = _renameBuffer;
                    _logger.LogInformation("[SCENE] Renamed camera to: {Name}", _renameBuffer);
                }
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel##cameraRenameCancel", new System.Numerics.Vector2(120, 0)))
            {
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }

    private void RenderLightRenamePopup()
    {
        if (_openLightRenamePopup)
        {
            ImGui.OpenPopup("RenameLightPopup");
            _openLightRenamePopup = false;
        }

        bool isRenameLightOpen = true;
        if (ImGui.BeginPopupModal("RenameLightPopup", ref isRenameLightOpen, ImGuiWindowFlags.AlwaysAutoResize))
        {
            // Focus the input on first frame
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }

            if (ImGui.InputText("New Name##lightRename", ref _renameBuffer, 100, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
            {
                if (!string.IsNullOrWhiteSpace(_renameBuffer) && _renamingLight != null)
                {
                    _renamingLight.Name = _renameBuffer;
                    _logger.LogInformation("[SCENE] Renamed light to: {Name}", _renameBuffer);
                    ImGui.CloseCurrentPopup();
                }
            }

            if (ImGui.Button("OK##lightRenameOK", new System.Numerics.Vector2(120, 0)))
            {
                if (!string.IsNullOrWhiteSpace(_renameBuffer) && _renamingLight != null)
                {
                    _renamingLight.Name = _renameBuffer;
                    _logger.LogInformation("[SCENE] Renamed light to: {Name}", _renameBuffer);
                }
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel##lightRenameCancel", new System.Numerics.Vector2(120, 0)))
            {
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }

    private void RenderObjectNode(SceneObject obj)
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

    private void RenderSceneToolbar()
    {
        if (ImGui.Button("+ Camera"))
        {
            // Always ensure we have the latest counter before adding
            RefreshCounters();
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
            // Always ensure we have the latest counter before adding
            RefreshCounters();
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
                    if (!isActive && ImGui.MenuItem("Set as Active"))
                    {
                        _scene.CameraManager.SetActiveCamera(camera.Name);
                        _logger.LogInformation("[SCENE] Set {Name} as active camera", camera.Name);
                    }
                    if (ImGui.MenuItem("Rename"))
                    {
                        _renamingCamera = camera;
                        _renameBuffer = camera.Name;
                        _openCameraRenamePopup = true;
                    }
                    if (_scene.CameraManager.Cameras.Count > 1 && ImGui.MenuItem("Delete"))
                    {
                        _scene.CameraManager.RemoveCamera(camera);
                        _scene.SelectedCamera = null;
                        _logger.LogInformation("[SCENE] Deleted camera: {Name}", camera.Name);
                    }
                    ImGui.EndPopup();
                }
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Objects", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Separate objects into Track and Default subgroups
            var trackObjects = _scene.Objects
                .Where(obj => !string.IsNullOrEmpty(obj.SourceFilePath) &&
                    (obj.SourceFilePath.EndsWith("scene.prm", StringComparison.OrdinalIgnoreCase) ||
                     obj.SourceFilePath.EndsWith("sky.prm", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var defaultObjects = _scene.Objects
                .Where(obj => !trackObjects.Contains(obj))
                .ToList();

            // Render Track subgroup
            if (ImGui.TreeNodeEx("Track", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (trackObjects.Count == 0)
                {
                    ImGui.TextDisabled("(No track objects loaded)");
                }
                else
                {
                    foreach (var obj in trackObjects)
                    {
                        RenderObjectNode(obj);
                    }
                }
                ImGui.TreePop();
            }

            // Render Default subgroup
            if (ImGui.TreeNodeEx("Default", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (defaultObjects.Count == 0)
                {
                    ImGui.TextDisabled("(No default objects loaded)");
                }
                else
                {
                    foreach (var obj in defaultObjects)
                    {
                        RenderObjectNode(obj);
                    }
                }
                ImGui.TreePop();
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
                        _renamingLight = light;
                        _renameBuffer = light.Name;
                        _openLightRenamePopup = true;
                    }
                    if (ImGui.MenuItem("Delete"))
                    {
                        _scene.LightManager.RemoveLight(light);
                        if (_scene.SelectedLight == light)
                        {
                            _scene.SelectedLight = null;
                        }
                        _logger.LogInformation("[SCENE] Deleted light: {Name}", light.Name);
                    }
                    ImGui.EndPopup();
                }
            }

            ImGui.TreePop();
        }
    }

    #endregion 
}