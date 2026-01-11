using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Graphics;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Simple model browser for selecting and loading PRM files.
/// Displays available models in a file list with expand/collapse for multi-object PRMs.
/// Follows Single Responsibility Principle - only handles model browsing.
/// </summary>
public class ModelBrowser : IModelBrowser
{
    /// <summary>
    /// Get the last loaded file path.
    /// </summary>
    public string? LastLoadedFile => _lastLoadedFile;

    /// <summary>
    /// Get all available PRM files.
    /// </summary>
    public IReadOnlyList<PrmFileInfo> PrmFiles => _prmFiles.AsReadOnly();

    /// <summary>
    /// Get the currently selected file index.
    /// </summary>
    public int SelectedFileIndex => _selectedFileIndex;

    /// <summary>
    /// Get the currently selected object index.
    /// </summary>
    public int SelectedObjectIndex => _selectedObjectIndex;

    #region fields
    private string? _lastLoadedFile;
    private readonly ILogger<ModelBrowser> _logger;
    private readonly IModelLoader _modelLoader;
    private List<PrmFileInfo> _prmFiles = new();
    private int _selectedFileIndex = -1;
    private int _selectedObjectIndex = -1;
    #endregion 

    public ModelBrowser(
        ILogger<ModelBrowser> logger,
        IModelLoader modelLoader)
    {
        _prmFiles = new List<PrmFileInfo>();
        _selectedFileIndex = -1;
        _selectedObjectIndex = -1;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _modelLoader = modelLoader ?? throw new ArgumentNullException(nameof(modelLoader));
    }

    #region methods

    /// <summary>
    /// Add PRM files to the model browser.
    /// </summary>
    public void AddModels(params string[] filePaths)
    {
        foreach (var path in filePaths)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path) && path.EndsWith(".prm", StringComparison.OrdinalIgnoreCase))
            {
                // Avoid duplicates
                if (!_prmFiles.Any(m => m.FilePath == path))
                {
                    var fileInfo = new PrmFileInfo
                    {
                        FilePath = path,
                        FileName = Path.GetFileName(path)
                    };

                    // Scan for objects
                    try
                    {
                        var objects = _modelLoader.GetObjectsInPrmFile(path);
                        foreach (var (index, objName) in objects)
                        {
                            fileInfo.Objects.Add(new PrmObjectInfo
                            {
                                Index = index,
                                Name = string.IsNullOrWhiteSpace(objName) ? $"object {index}" : objName
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to scan PRM file: {Path}", path);
                    }

                    _prmFiles.Add(fileInfo);
                }
            }
        }

        if (_selectedFileIndex < 0 && _prmFiles.Count > 0)
        {
            _selectedFileIndex = 0;
            _selectedObjectIndex = _prmFiles[0].Objects.Count > 0 ? _prmFiles[0].Objects[0].Index : 0;
        }
    }

    /// <summary>
    /// Clear all models from the browser.
    /// </summary>
    public void ClearModels()
    {
        _prmFiles.Clear();
        _selectedFileIndex = -1;
        _selectedObjectIndex = -1;
    }

    /// <summary>
    /// Get model statistics (placeholder for now).
    /// </summary>
    public ModelStats GetModelStats(string filePath)
    {
        return new ModelStats
        {
            FileName = Path.GetFileName(filePath),
            FilePath = filePath,
            FileSize = new FileInfo(filePath).Length,
            Polygons = 0,  // TODO: Parse from PRM file
            Textures = 0,  // TODO: Parse from PRM file
            Vertices = 0   // TODO: Parse from PRM file
        };
    }

    /// <summary>
    /// Get the currently selected model path and object index.
    /// </summary>
    public (string? path, int objectIndex) GetSelectedModel()
    {
        if (_selectedFileIndex >= 0 && _selectedFileIndex < _prmFiles.Count)
        {
            var file = _prmFiles[_selectedFileIndex];
            int objIndex = _selectedObjectIndex >= 0 ? _selectedObjectIndex : 0;
            return (file.FilePath, objIndex);
        }
        return (null, 0);
    }

    /// <summary>
    /// Load PRM files from a specific folder.
    /// </summary>
    public void LoadFromFolder(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            _logger.LogWarning("Invalid folder path: {Path}", folderPath);
            return;
        }

        _prmFiles.Clear();

        var prmFiles = ModelFileDialog.GetPrmFilesFromDirectory(folderPath);
        int loadedCount = 0;

        foreach (var path in prmFiles)
        {
            var fileInfo = new PrmFileInfo
            {
                FilePath = path,
                FileName = Path.GetFileName(path)
            };

            // Scan the PRM file for objects
            try
            {
                var objects = _modelLoader.GetObjectsInPrmFile(path);
                foreach (var (index, objName) in objects)
                {
                    fileInfo.Objects.Add(new PrmObjectInfo
                    {
                        Index = index,
                        Name = string.IsNullOrWhiteSpace(objName) ? $"object {index}" : objName
                    });
                }

                _prmFiles.Add(fileInfo);
                loadedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to scan PRM file: {Path}", path);
            }
        }

        _selectedFileIndex = _prmFiles.Count > 0 ? 0 : -1;
        _selectedObjectIndex = (_prmFiles.Count > 0 && _prmFiles[0].Objects.Count > 0) ? _prmFiles[0].Objects[0].Index : -1;

        _logger.LogInformation("Loaded {Count} PRM files from {Path}", loadedCount, folderPath);
    }

    /// <summary>
    /// Load PRM files from a specific folder (async with progress reporting).
    /// </summary>
    public async Task LoadFromFolderAsync(string folderPath, IProgress<(int current, int total, string fileName)>? progress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            _logger.LogWarning("Invalid folder path: {Path}", folderPath);
            return;
        }

        await Task.Run(() =>
        {
            _prmFiles.Clear();

            var prmFiles = ModelFileDialog.GetPrmFilesFromDirectory(folderPath).ToList();
            int loadedCount = 0;
            int totalFiles = prmFiles.Count;

            foreach (var path in prmFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fileName = Path.GetFileName(path);
                progress?.Report((loadedCount, totalFiles, fileName));

                var fileInfo = new PrmFileInfo
                {
                    FilePath = path,
                    FileName = fileName
                };

                // Scan the PRM file for objects
                try
                {
                    var objects = _modelLoader.GetObjectsInPrmFile(path);
                    foreach (var (index, objName) in objects)
                    {
                        fileInfo.Objects.Add(new PrmObjectInfo
                        {
                            Index = index,
                            Name = string.IsNullOrWhiteSpace(objName) ? $"object {index}" : objName
                        });
                    }

                    _prmFiles.Add(fileInfo);
                    loadedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to scan PRM file: {Path}", path);
                }
            }

            _selectedFileIndex = _prmFiles.Count > 0 ? 0 : -1;
            _selectedObjectIndex = (_prmFiles.Count > 0 && _prmFiles[0].Objects.Count > 0) ? _prmFiles[0].Objects[0].Index : -1;

            _logger.LogInformation("Loaded {Count} PRM files from {Path}", loadedCount, folderPath);
            progress?.Report((loadedCount, totalFiles, "Complete"));
        }, cancellationToken);
    }

    /// <summary>
    /// Load a single PRM file into the browser, replacing all existing files.
    /// </summary>
    public void LoadSingleFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _logger.LogWarning("Invalid file path: {Path}", filePath);
            return;
        }

        if (!filePath.EndsWith(".prm", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("File is not a PRM file: {Path}", filePath);
            return;
        }

        _prmFiles.Clear();

        var fileInfo = new PrmFileInfo
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath)
        };

        // Scan the PRM file for objects
        try
        {
            var objects = _modelLoader.GetObjectsInPrmFile(filePath);
            foreach (var (index, objName) in objects)
            {
                fileInfo.Objects.Add(new PrmObjectInfo
                {
                    Index = index,
                    Name = string.IsNullOrWhiteSpace(objName) ? $"object {index}" : objName
                });
            }

            _prmFiles.Add(fileInfo);
            _selectedFileIndex = 0;
            _selectedObjectIndex = fileInfo.Objects.Count > 0 ? fileInfo.Objects[0].Index : 0;

            _logger.LogInformation("Loaded single PRM file: {Path}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load PRM file: {Path}", filePath);
        }
    }

    /// <summary>
    /// Refresh the list of available PRM models and scan for objects.
    /// </summary>
    public void RefreshModelList()
    {
        _prmFiles.Clear();

        foreach (var (path, name) in ModelFileDialog.GetAvailablePrmFiles())
        {
            var fileInfo = new PrmFileInfo
            {
                FilePath = path,
                FileName = name
            };

            // Scan the PRM file for objects
            try
            {
                var objects = _modelLoader.GetObjectsInPrmFile(path);
                foreach (var (index, objName) in objects)
                {
                    fileInfo.Objects.Add(new PrmObjectInfo
                    {
                        Index = index,
                        Name = string.IsNullOrWhiteSpace(objName) ? $"object {index}" : objName
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to scan PRM file: {Path}", path);
            }

            _prmFiles.Add(fileInfo);
        }

        _selectedFileIndex = _prmFiles.Count > 0 ? 0 : -1;
        _selectedObjectIndex = (_prmFiles.Count > 0 && _prmFiles[0].Objects.Count > 0) ? 0 : -1;
    }

    /// <summary>
    /// Refresh the list of available PRM models and scan for objects (async).
    /// </summary>
    public async Task RefreshModelListAsync(IProgress<(int current, int total, string fileName)>? progress = null, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            _prmFiles.Clear();

            var availableFiles = ModelFileDialog.GetAvailablePrmFiles().ToList();
            int totalFiles = availableFiles.Count;
            int processedCount = 0;

            foreach (var (path, name) in availableFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report((processedCount, totalFiles, name));

                var fileInfo = new PrmFileInfo
                {
                    FilePath = path,
                    FileName = name
                };

                // Scan the PRM file for objects
                try
                {
                    var objects = _modelLoader.GetObjectsInPrmFile(path);
                    foreach (var (index, objName) in objects)
                    {
                        fileInfo.Objects.Add(new PrmObjectInfo
                        {
                            Index = index,
                            Name = string.IsNullOrWhiteSpace(objName) ? $"object {index}" : objName
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to scan PRM file: {Path}", path);
                }

                _prmFiles.Add(fileInfo);
                processedCount++;
            }

            _selectedFileIndex = _prmFiles.Count > 0 ? 0 : -1;
            _selectedObjectIndex = (_prmFiles.Count > 0 && _prmFiles[0].Objects.Count > 0) ? 0 : -1;

            progress?.Report((processedCount, totalFiles, "Complete"));
        }, cancellationToken);
    }

    /// <summary>
    /// Set the selected model by file and object index.
    /// </summary>
    public void SelectModel(int fileIndex, int objectIndex)
    {
        if (fileIndex >= 0 && fileIndex < _prmFiles.Count)
        {
            _selectedFileIndex = fileIndex;
            var file = _prmFiles[fileIndex];

            if (objectIndex >= 0 && objectIndex < file.Objects.Count)
            {
                _selectedObjectIndex = file.Objects[objectIndex].Index;
            }
            else
            {
                _selectedObjectIndex = file.Objects.Count > 0 ? file.Objects[0].Index : 0;
            }

            _lastLoadedFile = file.FilePath;
        }
    }

    /// <summary>
    /// Toggle expansion state of a PRM file.
    /// </summary>
    public void ToggleExpanded(int fileIndex)
    {
        if (fileIndex >= 0 && fileIndex < _prmFiles.Count)
        {
            _prmFiles[fileIndex].IsExpanded = !_prmFiles[fileIndex].IsExpanded;
        }
    }

    #endregion 
}

/// <summary>
/// Model statistics information.
/// </summary>
public class ModelStats
{
    #region properties
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public int Polygons { get; set; }
    public int Textures { get; set; }
    public int Vertices { get; set; }
    #endregion 
}

/// <summary>
/// Information about a PRM file and its contained objects.
/// </summary>
public class PrmFileInfo
{
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public bool IsExpanded { get; set; } = false;
    public List<PrmObjectInfo> Objects { get; set; } = new();
}

/// <summary>
/// Information about a single object within a PRM file.
/// </summary>
public class PrmObjectInfo
{
    public int Index { get; set; }
    public string Name { get; set; } = "";
}