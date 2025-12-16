using ImGuiNET;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Tools.Managers;
using WipeoutRewrite.Tools.Rendering;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Panel for UI scale, viewport settings and other preferences.
/// </summary>
public class SettingsPanel : ISettingsPanel, IUIPanel
{
    public bool IsVisible { get; set; } = false;

    private float _currentUIScale = 1.0f;
    private readonly ILogger<SettingsPanel> _logger;
    private readonly ISettingsService _settingsManager;
    private readonly IWorldGrid _worldGrid;

    public SettingsPanel(
        ILogger<SettingsPanel> logger,
        ISettingsService settingsManager,
        IWorldGrid worldGrid)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        _worldGrid = worldGrid ?? throw new ArgumentNullException(nameof(worldGrid));
    }

    public void Render()
    {
        if (!IsVisible) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 300), ImGuiCond.FirstUseEver);

        bool isVisible = IsVisible;
        if (ImGui.Begin("Settings", ref isVisible))
        {
            IsVisible = isVisible;
            ImGui.SeparatorText("Display Settings");

            // UI Scale slider
            float uiScale = _currentUIScale;
            ImGui.Text("UI Scale:");
            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderFloat("##uiscale", ref uiScale, 0.5f, 3.0f, $"{uiScale:F2}x ({(int)(uiScale * 100)}%)"))
            {
                _currentUIScale = uiScale;
                ApplyUIScale(_currentUIScale);
                _settingsManager.SetUIScale(_currentUIScale);
            }

            ImGui.TextWrapped("Adjust the UI scale for better readability. Recommended: 1.5x for high-DPI displays.");

            ImGui.Spacing();

            // Quick preset buttons
            ImGui.Text("Quick Presets:");
            if (ImGui.Button("100%", new System.Numerics.Vector2(60, 0)))
            {
                _currentUIScale = 1.0f;
                ApplyUIScale(_currentUIScale);
                _settingsManager.SetUIScale(_currentUIScale);
            }
            ImGui.SameLine();
            if (ImGui.Button("125%", new System.Numerics.Vector2(60, 0)))
            {
                _currentUIScale = 1.25f;
                ApplyUIScale(_currentUIScale);
                _settingsManager.SetUIScale(_currentUIScale);
            }
            ImGui.SameLine();
            if (ImGui.Button("150%", new System.Numerics.Vector2(60, 0)))
            {
                _currentUIScale = 1.5f;
                ApplyUIScale(_currentUIScale);
                _settingsManager.SetUIScale(_currentUIScale);
            }
            ImGui.SameLine();
            if (ImGui.Button("175%", new System.Numerics.Vector2(60, 0)))
            {
                _currentUIScale = 1.75f;
                ApplyUIScale(_currentUIScale);
                _settingsManager.SetUIScale(_currentUIScale);
            }
            ImGui.SameLine();
            if (ImGui.Button("200%", new System.Numerics.Vector2(60, 0)))
            {
                _currentUIScale = 2.0f;
                ApplyUIScale(_currentUIScale);
                _settingsManager.SetUIScale(_currentUIScale);
            }

            ImGui.Separator();
            ImGui.SeparatorText("Viewport Settings");

            // Grid and axes toggles
            if (_worldGrid != null)
            {
                bool showGrid = _worldGrid.ShowGrid;
                if (ImGui.Checkbox("Show Grid", ref showGrid))
                {
                    _worldGrid.ShowGrid = showGrid;
                    _settingsManager.Settings.ShowGrid = showGrid;
                    _settingsManager.SaveSettings();
                }

                bool showAxes = _worldGrid.ShowAxes;
                if (ImGui.Checkbox("Show Coordinate Axes (XYZ)", ref showAxes))
                {
                    _worldGrid.ShowAxes = showAxes;
                    _settingsManager.Settings.ShowAxes = showAxes;
                    _settingsManager.SaveSettings();
                }

                ImGui.TextWrapped("Grid: Ground plane (XZ). Axes: X=Red, Y=Green, Z=Blue");
            }

            ImGui.Spacing();

            // View gizmo toggle
            bool showGizmo = _settingsManager.Settings.ShowGizmo;
            if (ImGui.Checkbox("Show View Gizmo", ref showGizmo))
            {
                _settingsManager.Settings.ShowGizmo = showGizmo;
                _settingsManager.SaveSettings();
            }
            ImGui.TextWrapped("Gizmo: 3D orientation widget. Click axes to snap camera view.");

            ImGui.Separator();
            ImGui.SeparatorText("Other Settings");

            ImGui.TextDisabled("More settings coming soon...");

            ImGui.Spacing();
            ImGui.Separator();

            ImGui.TextWrapped($"Settings are saved to: app_settings.json");
            ImGui.TextWrapped($"Note: UI scale changes may require restarting the application for full effect.");
        }
        ImGui.End();
    }

    public void SetUIScale(float scale)
    {
        _currentUIScale = scale;
    }

    private void ApplyUIScale(float scale)
    {
        var io = ImGui.GetIO();
        io.FontGlobalScale = scale;

        _logger.LogInformation("[UI] Applied UI scale: {Scale}x ({Percent}%)", scale, (int)(scale * 100));
    }
}