using ImGuiNET;
using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Panel for selecting and loading complete tracks for visualization.
/// Allows flythrough navigation of entire track scenes.
/// </summary>
public class TrackViewerPanel : ITrackViewerPanel, IUIPanel
{
    public event Action<int, string>? OnTrackLoadRequested;

    public bool IsVisible { get; set; } = true;

    private readonly ILogger<TrackViewerPanel> _logger;
    private int _selectedTrackNumber = 7;

    // Default to Altima VII - Venom
    private string _wipoutDataDir = "";

    // Track names
    private static readonly string[] TrackNames = new[]
    {
        "Terramax - Venom",
        "Altima VII - Venom",
        "Altima VII - Rapier",
        "Karbonis V - Venom",
        "Karbonis V - Rapier",
        "Terramax - Rapier",
        "Korodera - Venom",
        "Arridos IV - Venom",
        "Silverstream - Venom",
        "Firestar - Venom",
        "Arridos IV - Rapier",
        "Korodera - Rapier",
        "Silverstream - Rapier",
        "Firestar - Rapier",
    };

    public TrackViewerPanel(ILogger<TrackViewerPanel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Render()
    {
        if (!IsVisible) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(350, 400), ImGuiCond.FirstUseEver);

        bool isVisible = IsVisible;
        if (ImGui.Begin("Track Viewer", ref isVisible))
        {
            IsVisible = isVisible;

            ImGui.Text("Select a track to load:");
            ImGui.Separator();

            // Track selector
            if (ImGui.BeginCombo("##track_combo", _selectedTrackNumber > 0 && _selectedTrackNumber <= TrackNames.Length
                ? TrackNames[_selectedTrackNumber - 1]
                : "Select a track"))
            {
                for (int i = 1; i <= TrackNames.Length; i++)
                {
                    bool isSelected = _selectedTrackNumber == i;
                    if (ImGui.Selectable($"Track {i:D2} - {TrackNames[i - 1]}", isSelected))
                    {
                        _selectedTrackNumber = i;
                        _logger.LogInformation("[TRACK] Selected track: {TrackNum} - {TrackName}", i, TrackNames[i - 1]);
                    }

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            ImGui.Separator();

            // Load button
            if (ImGui.Button("Load Track", new System.Numerics.Vector2(-1, 0)))
            {
                if (!string.IsNullOrEmpty(_wipoutDataDir))
                {
                    _logger.LogWarning("[TRACK] Loading track {TrackNum}: {TrackName}",
                        _selectedTrackNumber, TrackNames[_selectedTrackNumber - 1]);
                    OnTrackLoadRequested?.Invoke(_selectedTrackNumber, _wipoutDataDir);
                }
                else
                {
                    _logger.LogWarning("[TRACK] Cannot load track: wipeout data directory not set");
                }
            }

            ImGui.Separator();

            // Info display
            if (_selectedTrackNumber > 0 && _selectedTrackNumber <= TrackNames.Length)
            {
                ImGui.TextColored(new System.Numerics.Vector4(0.4f, 0.8f, 0.4f, 1.0f),
                    $"Track {_selectedTrackNumber:D2}");
                ImGui.TextWrapped(TrackNames[_selectedTrackNumber - 1]);

                ImGui.Spacing();
                ImGui.TextDisabled("This will load:");
                ImGui.BulletText("scene.prm + scene.cmp");
                ImGui.BulletText("sky.prm + sky.cmp");
                ImGui.BulletText("library.cmp");
            }
        }
        ImGui.End();
    }

    public void SetWipeoutDataDirectory(string directory)
    {
        _wipoutDataDir = directory;
        _logger.LogInformation("[TRACK] Wipeout data directory set to: {Dir}", directory);
    }
}