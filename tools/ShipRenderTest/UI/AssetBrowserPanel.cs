
using ImGuiNET;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Tools.Core;
using WipeoutRewrite.Infrastructure.Graphics;

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
    private readonly ITextureManager _textureManager;
    private readonly IScene _scene;
    private readonly ILogger _mainLogger;
    private string _searchFilter = "";
    private int _selectedModelIndex = -1;
    private bool _showOnlyTracks = false;

    // Filter to show only TrackXX files

    public AssetBrowserPanel(
        ILogger<AssetBrowserPanel> logger,
        IModelBrowser modelBrowser,
        ITextureManager textureManager,
        IScene scene,
        ILogger<ShipRenderWindow> mainLogger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _modelBrowser = modelBrowser ?? throw new ArgumentNullException(nameof(modelBrowser));
        _textureManager = textureManager ?? throw new ArgumentNullException(nameof(textureManager));
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _mainLogger = mainLogger ?? throw new ArgumentNullException(nameof(mainLogger));
    }

    public void Render()
    {
        if (!IsVisible) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 400), ImGuiCond.FirstUseEver);

        bool isVisible = IsVisible;
        if (ImGui.Begin("Asset Browser", ref isVisible))
        {
            IsVisible = isVisible;

            // Count total and track files
            int totalFiles = _modelBrowser.PrmFiles.Count;
            int trackFiles = _modelBrowser.PrmFiles.Count(f => IsTrackFile(f.FilePath));

            ImGui.Text($"Total: {totalFiles} files ({trackFiles} tracks)");

            // Track filter checkbox
            ImGui.Checkbox("Show Only Tracks", ref _showOnlyTracks);
            ImGui.SameLine();
            if (ImGui.SmallButton("?"))
            {
                ImGui.SetTooltip("Show only files from TrackXX folders\n(scene.prm, sky.prm, etc.)");
            }

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

                        // Se for CMP, botão para carregar texturas
                        if (selectedFile.FileName.EndsWith(".cmp", StringComparison.OrdinalIgnoreCase))
                        {
                            // Check if there are multiple CMP files loaded (from folder)
                            int cmpCount = _modelBrowser.PrmFiles.Count(f => f.FileName.EndsWith(".cmp", StringComparison.OrdinalIgnoreCase));

                            if (cmpCount > 1)
                            {
                                // Multiple CMPs - show two buttons
                                float buttonWidth = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2f;

                                if (ImGui.Button("Load Texture", new System.Numerics.Vector2(buttonWidth, 0)))
                                {
                                    // Load only selected CMP
                                    var textures = TextureLoader.LoadTexturesFromCmpFile(_textureManager, selectedFile.FilePath);
                                    if (textures.Length > 0)
                                    {
                                        _scene.AddStandaloneTextures(selectedFile.FileName, textures);
                                        _mainLogger.LogInformation($"[TEXTURE] Loaded {textures.Length} textures from CMP: {selectedFile.FileName}");
                                    }
                                    else
                                    {
                                        _mainLogger.LogWarning($"[TEXTURE] No textures found in CMP: {selectedFile.FileName}");
                                    }
                                }

                                ImGui.SameLine();

                                if (ImGui.Button("Load All Textures", new System.Numerics.Vector2(buttonWidth, 0)))
                                {
                                    // Load all CMPs in the list
                                    int totalTextures = 0;
                                    foreach (var cmpFile in _modelBrowser.PrmFiles.Where(f => f.FileName.EndsWith(".cmp", StringComparison.OrdinalIgnoreCase)))
                                    {
                                        var textures = TextureLoader.LoadTexturesFromCmpFile(_textureManager, cmpFile.FilePath);
                                        if (textures.Length > 0)
                                        {
                                            _scene.AddStandaloneTextures(cmpFile.FileName, textures);
                                            totalTextures += textures.Length;
                                        }
                                    }
                                    _mainLogger.LogInformation($"[TEXTURE] Loaded {totalTextures} textures from {cmpCount} CMP files");
                                }
                            }
                            else
                            {
                                // Single CMP - show one button
                                if (ImGui.Button("Load Texture"))
                                {
                                    var textures = TextureLoader.LoadTexturesFromCmpFile(_textureManager, selectedFile.FilePath);
                                    if (textures.Length > 0)
                                    {
                                        _scene.AddStandaloneTextures(selectedFile.FileName, textures);
                                        _mainLogger.LogInformation($"[TEXTURE] Loaded {textures.Length} textures from CMP: {selectedFile.FileName}");
                                    }
                                    else
                                    {
                                        _mainLogger.LogWarning($"[TEXTURE] No textures found in CMP: {selectedFile.FileName}");
                                    }
                                }
                            }
                        }

                        // Se for TIM, botão para carregar textura
                        if (selectedFile.FileName.EndsWith(".tim", StringComparison.OrdinalIgnoreCase))
                        {
                            // Check if there are multiple TIM files loaded (from folder)
                            int timCount = _modelBrowser.PrmFiles.Count(f => f.FileName.EndsWith(".tim", StringComparison.OrdinalIgnoreCase));

                            if (timCount > 1)
                            {
                                // Multiple TIMs - show two buttons
                                float buttonWidth = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2f;

                                if (ImGui.Button("Load Texture", new System.Numerics.Vector2(buttonWidth, 0)))
                                {
                                    // Load only selected TIM
                                    var handle = TextureLoader.LoadTextureFromTimFile(_textureManager, selectedFile.FilePath);
                                    if (handle > 0)
                                    {
                                        _scene.AddStandaloneTextures(selectedFile.FileName, new[] { handle });
                                        _mainLogger.LogInformation($"[TEXTURE] Loaded texture from TIM: {selectedFile.FileName}");
                                    }
                                    else
                                    {
                                        _mainLogger.LogWarning($"[TEXTURE] Failed to load TIM: {selectedFile.FileName}");
                                    }
                                }

                                ImGui.SameLine();

                                if (ImGui.Button("Load All Textures", new System.Numerics.Vector2(buttonWidth, 0)))
                                {
                                    // Load all TIMs in the list
                                    int totalTextures = 0;
                                    foreach (var timFile in _modelBrowser.PrmFiles.Where(f => f.FileName.EndsWith(".tim", StringComparison.OrdinalIgnoreCase)))
                                    {
                                        var handle = TextureLoader.LoadTextureFromTimFile(_textureManager, timFile.FilePath);
                                        if (handle > 0)
                                        {
                                            _scene.AddStandaloneTextures(timFile.FileName, new[] { handle });
                                            totalTextures++;
                                        }
                                    }
                                    _mainLogger.LogInformation($"[TEXTURE] Loaded {totalTextures} textures from {timCount} TIM files");
                                }
                            }
                            else
                            {
                                // Single TIM - show one button
                                if (ImGui.Button("Load Texture"))
                                {
                                    var handle = TextureLoader.LoadTextureFromTimFile(_textureManager, selectedFile.FilePath);
                                    if (handle > 0)
                                    {
                                        _scene.AddStandaloneTextures(selectedFile.FileName, new[] { handle });
                                        _mainLogger.LogInformation($"[TEXTURE] Loaded texture from TIM: {selectedFile.FileName}");
                                    }
                                    else
                                    {
                                        _mainLogger.LogWarning($"[TEXTURE] Failed to load TIM: {selectedFile.FileName}");
                                    }
                                }
                            }
                        }
                    }

                    // Two buttons side by side (mantém para PRM)
                    if (selectedFile.FileName.EndsWith(".prm", StringComparison.OrdinalIgnoreCase))
                    {
                        float buttonWidth = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2f;

                        if (ImGui.Button("+ Add to Scene", new System.Numerics.Vector2(buttonWidth, 0)))
                        {
                            OnAddToSceneRequested?.Invoke(modelPath, objIdx);
                        }

                        ImGui.SameLine();

                        if (ImGui.Button("+ Add All Models", new System.Numerics.Vector2(buttonWidth, 0)))
                        {
                            _logger.LogWarning("[UI] *** ADD ALL MODELS BUTTON CLICKED ***");
                            _logger.LogWarning("[UI] Selected file: {File}, Objects count: {Count}", selectedFile.FileName, selectedFile.Objects.Count);

                            // Check if this is scene.prm or sky.prm (these should ALWAYS use "load all" mode)
                            bool isSceneOrSky = selectedFile.FileName.Equals("scene.prm", StringComparison.OrdinalIgnoreCase) ||
                                               selectedFile.FileName.Equals("sky.prm", StringComparison.OrdinalIgnoreCase);

                            _logger.LogWarning("[UI] IsSceneOrSky: {IsSceneOrSky}", isSceneOrSky);

                            if (isSceneOrSky)
                            {
                                // For scene/sky files, trigger with index -1 to signal "load all"
                                _logger.LogWarning("[UI] Loading ALL objects from scene/sky file: {File} (path: {Path})",
                                    selectedFile.FileName, selectedFile.FilePath);
                                OnAddToSceneRequested?.Invoke(selectedFile.FilePath, -1); // -1 = load all
                            }
                            else
                            {
                                // For regular files, load each object individually
                                _logger.LogInformation("[UI] Loading all {Count} models from {File}", selectedFile.Objects.Count, selectedFile.FileName);

                                var objectsToLoad = selectedFile.Objects.ToList();
                                var filePath = selectedFile.FilePath;
                                Task.Run(() =>
                                {
                                    foreach (var obj in objectsToLoad)
                                    {
                                        OnAddToSceneRequested?.Invoke(filePath, obj.Index);
                                        System.Threading.Thread.Sleep(10);
                                    }
                                });
                            }
                        }
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

                    // Apply track filter
                    if (_showOnlyTracks && !IsTrackFile(file.FilePath))
                        continue;

                    // Novo filtro: busca no nome do arquivo OU no nome dos objetos
                    bool matchesSearch = string.IsNullOrEmpty(_searchFilter)
                        || file.FileName.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase)
                        || file.Objects.Any(obj => obj.Name.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase));
                    if (!matchesSearch)
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
                                bool objMatchesSearch = string.IsNullOrEmpty(_searchFilter)
                                    || obj.Name.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase);
                                if (!objMatchesSearch)
                                    continue;

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
                        // Single object file - show diretamente se o nome bater
                        var obj = file.Objects[0];
                        bool objMatchesSearch = string.IsNullOrEmpty(_searchFilter)
                            || obj.Name.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase);
                        if (!objMatchesSearch)
                            continue;

                        bool isSelected = _selectedModelIndex == i;

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

    /// <summary>
    /// Check if a file path is from a TrackXX folder
    /// </summary>
    private bool IsTrackFile(string filePath)
    {
        return filePath.Contains("track", StringComparison.OrdinalIgnoreCase) &&
               System.Text.RegularExpressions.Regex.IsMatch(filePath, @"track\d{2}",
                   System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}