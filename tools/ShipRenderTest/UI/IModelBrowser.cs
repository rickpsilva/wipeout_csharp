namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Interface for browsing and selecting PRM model files.
/// Follows Interface Segregation Principle - clients depend on specific browser operations.
/// </summary>
public interface IModelBrowser
{
    /// <summary>
    /// Get the last loaded file path.
    /// </summary>
    string? LastLoadedFile { get; }
    /// <summary>
    /// Get all available PRM files.
    /// </summary>
    IReadOnlyList<PrmFileInfo> PrmFiles { get; }
    /// <summary>
    /// Get the currently selected file index.
    /// </summary>
    int SelectedFileIndex { get; }
    /// <summary>
    /// Get the currently selected object index.
    /// </summary>
    int SelectedObjectIndex { get; }

    /// <summary>
    /// Add PRM files to the model browser.
    /// </summary>
    void AddModels(params string[] filePaths);
    /// <summary>
    /// Clear all models from the browser.
    /// </summary>
    void ClearModels();
    /// <summary>
    /// Get model statistics.
    /// </summary>
    ModelStats GetModelStats(string filePath);
    /// <summary>
    /// Get the currently selected model path and object index.
    /// </summary>
    (string? path, int objectIndex) GetSelectedModel();
    /// <summary>
    /// Load PRM files from a specific folder.
    /// </summary>
    void LoadFromFolder(string folderPath);
    /// <summary>
    /// Load PRM files from a specific folder (async with progress reporting).
    /// </summary>
    Task LoadFromFolderAsync(string folderPath, IProgress<(int current, int total, string fileName)>? progress = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Load a single PRM file into the browser.
    /// </summary>
    void LoadSingleFile(string filePath);
    /// <summary>
    /// Load all CMP files from a folder into the browser.
    /// </summary>
    void LoadCmpFilesFromFolder(string folderPath);
    /// <summary>
    /// Load all TIM files from a folder into the browser.
    /// </summary>
    void LoadTimFilesFromFolder(string folderPath);
    /// <summary>
    /// Refresh the list of available PRM models and scan for objects.
    /// </summary>
    void RefreshModelList();
    /// <summary>
    /// Refresh the list of available PRM models and scan for objects (async).
    /// </summary>
    Task RefreshModelListAsync(IProgress<(int current, int total, string fileName)>? progress = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Set the selected model by file and object index.
    /// </summary>
    void SelectModel(int fileIndex, int objectIndex);
    /// <summary>
    /// Toggle expansion state of a PRM file.
    /// </summary>
    void ToggleExpanded(int fileIndex);
}