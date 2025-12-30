using ImGuiNET;
using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Panel for browsing and selecting PRM model files.
/// </summary>
public class AssetBrowserPanel : IAssetBrowserPanel, IUIPanel
{
    // Event for "Add to Scene" button
    public event Action<string, int>? OnAddToSceneRequested;

    public bool IsVisible { get; set; } = true;
    public int SelectedModelIndex => _selectedModelIndex;

    private readonly ILogger<AssetBrowserPanel> _logger;
    private readonly IModelBrowser _modelBrowser;
    private string _searchFilter = "";
    private int _selectedModelIndex = -1;

    public AssetBrowserPanel(
        ILogger<AssetBrowserPanel> logger,
        IModelBrowser modelBrowser)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _modelBrowser = modelBrowser ?? throw new ArgumentNullException(nameof(modelBrowser));
    }

    public void Render()
    {
        if (!IsVisible) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 400), ImGuiCond.FirstUseEver);

        bool isVisible = IsVisible;
        if (ImGui.Begin("Asset Browser", ref isVisible))
        {
            IsVisible = isVisible;
            ImGui.Text($"Total PRM Files: {_modelBrowser.PrmFiles.Count}");

            // Add to Scene button at TOP for better visibility
            if (_selectedModelIndex >= 0 && _selectedModelIndex < _modelBrowser.PrmFiles.Count)
            {
                var selectedFile = _modelBrowser.PrmFiles[_selectedModelIndex];
                var (modelPath, objIdx) = _modelBrowser.GetSelectedModel();

                if (modelPath != null)
                {
                    // Show selected model info
                    var selectedObj = _modelBrowser.SelectedObjectIndex >= 0 && _modelBrowser.SelectedObjectIndex < selectedFile.Objects.Count
                        ? selectedFile.Objects[_modelBrowser.SelectedObjectIndex]
                        : null;

                    if (selectedObj != null)
                    {
                        ImGui.TextColored(new System.Numerics.Vector4(0.4f, 0.8f, 0.4f, 1.0f),
                            $"Selected: {selectedObj.Name}");
                    }

                    // Two buttons side by side
                    float buttonWidth = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2f;
                    
                    if (ImGui.Button("+ Add to Scene", new System.Numerics.Vector2(buttonWidth, 0)))
                    {
                        OnAddToSceneRequested?.Invoke(modelPath, objIdx);
                    }
                    
                    ImGui.SameLine();
                    
                    if (ImGui.Button("+ Add All Models", new System.Numerics.Vector2(buttonWidth, 0)))
                    {
                        // Load all objects from the selected file (async to avoid UI freeze)
                        _logger.LogInformation("[UI] Loading all {Count} models from {File}", selectedFile.Objects.Count, selectedFile.FileName);
                        
                        // Use Task.Run to avoid blocking UI thread
                        var objectsToLoad = selectedFile.Objects.ToList();
                        var filePath = selectedFile.FilePath;
                        Task.Run(() =>
                        {
                            foreach (var obj in objectsToLoad)
                            {
                                OnAddToSceneRequested?.Invoke(filePath, obj.Index);
                                // Small delay to let UI breathe
                                System.Threading.Thread.Sleep(10);
                            }
                        });
                    }
                }
            }
            else
            {
                ImGui.TextDisabled("Select a model to add to scene");
            }

            ImGui.Separator();

            // Search filter
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##search", "Search models...", ref _searchFilter, 256);

            ImGui.Separator();

            // Model list (use remaining space minus button area)
            if (ImGui.BeginChild("ModelList"))
            {
                for (int i = 0; i < _modelBrowser.PrmFiles.Count; i++)
                {
                    var file = _modelBrowser.PrmFiles[i];

                    // Apply search filter
                    if (!string.IsNullOrEmpty(_searchFilter) &&
                        !file.FileName.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Show file with collapse if it has multiple objects
                    if (file.Objects.Count > 1)
                    {
                        bool isExpanded = file.IsExpanded;
                        if (ImGui.TreeNodeEx($"[P] {file.FileName} ({file.Objects.Count} models)###{i}",
                            isExpanded ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None))
                        {
                            file.IsExpanded = true;

                            // Show objects within the file
                            for (int j = 0; j < file.Objects.Count; j++)
                            {
                                var obj = file.Objects[j];
                                bool isSelected = _selectedModelIndex == i && _modelBrowser.SelectedObjectIndex == obj.Index;

                                if (ImGui.Selectable($"  └ [{obj.Index}] {obj.Name}###{i}_{j}", isSelected))
                                {
                                    _logger.LogInformation("[UI] Selected model: {Name} (file={FileIndex}, object={ObjIndex})", obj.Name, i, obj.Index);
                                    _selectedModelIndex = i;
                                    _modelBrowser.SelectModel(i, j); // Just select, don't load
                                }

                                if (isSelected)
                                    ImGui.SetItemDefaultFocus();
                            }

                            ImGui.TreePop();
                        }
                        else
                        {
                            file.IsExpanded = false;
                        }
                    }
                    else if (file.Objects.Count == 1)
                    {
                        // Single object file - show directly
                        bool isSelected = _selectedModelIndex == i;
                        var obj = file.Objects[0];

                        if (ImGui.Selectable($"[P] {file.FileName}: {obj.Name}###{i}", isSelected))
                        {
                            _logger.LogInformation("[UI] Selected model: {Name} (index={Index})", obj.Name, i);
                            _selectedModelIndex = i;
                            _modelBrowser.SelectModel(i, 0); // Just select, don't load
                        }

                        if (isSelected)
                            ImGui.SetItemDefaultFocus();
                    }
                    else
                    {
                        // No objects - show as disabled
                        ImGui.TextDisabled($"[P] {file.FileName} (no models)");
                    }
                }
            }
            ImGui.EndChild();
        }
        ImGui.End();
    }
}