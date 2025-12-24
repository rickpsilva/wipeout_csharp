using ImGuiNET;
using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Manages file and folder dialogs for opening PRM files.
/// Encapsulates all dialog state and rendering logic.
/// </summary>
public class FileDialogManager
{
    // Callbacks
    public Action<string[]>? OnFilesSelected { get; set; }

    public Action<string>? OnFolderSelected { get; set; }

    #region fields
    private string _fileDialogPath = "";
    private string _folderBrowserPath = "";
    private string _folderInputPath = "";
    private readonly ILogger<FileDialogManager> _logger;
    private readonly List<string> _selectedFiles = new();

    // File dialog state
    private bool _showFileDialog = false;

    private bool _showFolderBrowser = false;

    // Folder dialog state
    private bool _showFolderInput = false;

    #endregion 

    public FileDialogManager(ILogger<FileDialogManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region methods

    /// <summary>
    /// Render all active dialogs
    /// </summary>
    public void Render()
    {
        if (_showFileDialog)
            RenderFileDialog();
        if (_showFolderInput)
            RenderFolderInputDialog();
        if (_showFolderBrowser)
            RenderFolderBrowserDialog();
    }

    /// <summary>
    /// Show file picker dialog
    /// </summary>
    public void ShowFileDialog(string initialPath)
    {
        _showFileDialog = true;
        _fileDialogPath = string.IsNullOrEmpty(initialPath) ? ModelFileDialog.GetModelDirectory() : initialPath;
        _selectedFiles.Clear();
    }

    /// <summary>
    /// Show folder input dialog
    /// </summary>
    public void ShowFolderDialog(string initialPath)
    {
        _showFolderInput = true;
        _folderInputPath = string.IsNullOrEmpty(initialPath) ? ModelFileDialog.GetModelDirectory() : initialPath;
    }

    private void RenderFileDialog()
    {
        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize * 0.5f, ImGuiCond.FirstUseEver, new System.Numerics.Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(700, 500), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Open PRM Model Files", ref _showFileDialog))
        {
            ImGui.Text("Current path:");
            ImGui.InputText("##path", ref _fileDialogPath, 256);
            ImGui.SameLine();
            if (ImGui.Button("Go"))
            {
                if (!Directory.Exists(_fileDialogPath))
                    _logger.LogWarning("[EDITOR] Path does not exist: {Path}", _fileDialogPath);
            }

            ImGui.SameLine();
            if (ImGui.Button("Home"))
                _fileDialogPath = ModelFileDialog.GetModelDirectory();

            ImGui.SameLine();
            if (ImGui.Button("Up"))
            {
                var parent = Directory.GetParent(_fileDialogPath);
                if (parent != null)
                    _fileDialogPath = parent.FullName;
            }

            ImGui.Separator();

            // List directories and PRM files
            if (Directory.Exists(_fileDialogPath))
            {
                try
                {
                    var directories = Directory.GetDirectories(_fileDialogPath).OrderBy(d => Path.GetFileName(d)).ToList();
                    var prmFiles = Directory.GetFiles(_fileDialogPath, "*.prm", SearchOption.TopDirectoryOnly)
                        .OrderBy(f => Path.GetFileName(f))
                        .ToList();

                    if (ImGui.BeginChild("FileList", new System.Numerics.Vector2(-1, -80)))
                    {
                        // Directories
                        foreach (var dir in directories)
                        {
                            var dirName = Path.GetFileName(dir);
                            ImGui.TextColored(new System.Numerics.Vector4(0.7f, 0.9f, 1.0f, 1.0f), "📁");
                            ImGui.SameLine();
                            if (ImGui.Selectable("[" + dirName + "]", false, ImGuiSelectableFlags.AllowDoubleClick))
                            {
                                if (ImGui.IsMouseDoubleClicked(0))
                                    _fileDialogPath = dir;
                            }
                        }

                        // Files
                        for (int i = 0; i < prmFiles.Count; i++)
                        {
                            var filePath = prmFiles[i];
                            var fileName = Path.GetFileName(filePath);
                            bool isSelected = _selectedFiles.Contains(filePath);

                            ImGui.TextColored(new System.Numerics.Vector4(0.9f, 0.9f, 0.7f, 1.0f), "📄");
                            ImGui.SameLine();
                            if (ImGui.Selectable(fileName, isSelected))
                            {
                                if (ImGui.GetIO().KeyCtrl)
                                {
                                    if (isSelected)
                                        _selectedFiles.Remove(filePath);
                                    else
                                        _selectedFiles.Add(filePath);
                                }
                                else
                                {
                                    _selectedFiles.Clear();
                                    _selectedFiles.Add(filePath);
                                }
                            }
                        }

                        if (directories.Count == 0 && prmFiles.Count == 0)
                            ImGui.TextDisabled("(no files or folders)");
                    }
                    ImGui.EndChild();
                }
                catch (UnauthorizedAccessException)
                {
                    ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "Access denied to this directory");
                }
            }
            else
            {
                ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "Path does not exist");
            }

            ImGui.Separator();

            if (_selectedFiles.Count > 0)
                ImGui.Text("Selected files: " + _selectedFiles.Count);

            if (ImGui.Button("Load Selected", new System.Numerics.Vector2(150, 0)))
            {
                if (_selectedFiles.Count > 0)
                {
                    OnFilesSelected?.Invoke(_selectedFiles.ToArray());
                    _showFileDialog = false;
                    _selectedFiles.Clear();
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel", new System.Numerics.Vector2(150, 0)))
            {
                _showFileDialog = false;
                _selectedFiles.Clear();
            }
        }
        ImGui.End();
    }

    private void RenderFolderBrowserDialog()
    {
        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize * 0.5f, ImGuiCond.FirstUseEver, new System.Numerics.Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(700, 500), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Browse for Folder", ref _showFolderBrowser))
        {
            ImGui.Text("Current path:");
            ImGui.InputText("##browsepath", ref _folderBrowserPath, 256);
            ImGui.SameLine();
            if (ImGui.Button("Go"))
            {
                if (!Directory.Exists(_folderBrowserPath))
                    _logger.LogWarning("[EDITOR] Path does not exist: {Path}", _folderBrowserPath);
            }

            ImGui.SameLine();
            if (ImGui.Button("Home"))
                _folderBrowserPath = ModelFileDialog.GetModelDirectory();

            ImGui.SameLine();
            if (ImGui.Button("Up"))
            {
                var parent = Directory.GetParent(_folderBrowserPath);
                if (parent != null)
                    _folderBrowserPath = parent.FullName;
            }

            ImGui.Separator();

            if (Directory.Exists(_folderBrowserPath))
            {
                try
                {
                    var directories = Directory.GetDirectories(_folderBrowserPath).OrderBy(d => Path.GetFileName(d)).ToList();
                    var prmCount = Directory.GetFiles(_folderBrowserPath, "*.prm", SearchOption.TopDirectoryOnly).Length;

                    if (ImGui.BeginChild("FolderList", new System.Numerics.Vector2(-1, -80)))
                    {
                        ImGui.TextColored(new System.Numerics.Vector4(0.7f, 0.9f, 1.0f, 1.0f), "📁");
                        ImGui.SameLine();
                        ImGui.TextColored(new System.Numerics.Vector4(1.0f, 1.0f, 0.7f, 1.0f), $"Current folder: {prmCount} PRM files");
                        ImGui.Separator();

                        foreach (var dir in directories)
                        {
                            var dirName = Path.GetFileName(dir);
                            var dirPrmCount = Directory.GetFiles(dir, "*.prm", SearchOption.TopDirectoryOnly).Length;

                            ImGui.TextColored(new System.Numerics.Vector4(0.7f, 0.9f, 1.0f, 1.0f), "📁");
                            ImGui.SameLine();
                            if (ImGui.Selectable($"{dirName} ({dirPrmCount} PRM files)", false, ImGuiSelectableFlags.AllowDoubleClick))
                            {
                                if (ImGui.IsMouseDoubleClicked(0))
                                    _folderBrowserPath = dir;
                            }
                        }

                        if (directories.Count == 0)
                            ImGui.TextDisabled("(No subdirectories)");
                    }
                    ImGui.EndChild();
                }
                catch (Exception ex)
                {
                    ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), $"Error: {ex.Message}");
                }
            }
            else
            {
                ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "Path does not exist");
            }

            ImGui.Separator();

            if (ImGui.Button("Select This Folder", new System.Numerics.Vector2(150, 0)))
            {
                if (Directory.Exists(_folderBrowserPath))
                {
                    _folderInputPath = _folderBrowserPath;
                    _showFolderBrowser = false;
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel", new System.Numerics.Vector2(120, 0)))
                _showFolderBrowser = false;
        }
        ImGui.End();
    }

    private void RenderFolderInputDialog()
    {
        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize * 0.5f, ImGuiCond.Appearing, new System.Numerics.Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 150), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Open Folder with PRM Files", ref _showFolderInput))
        {
            ImGui.TextWrapped("Enter the path to a folder containing PRM files:");
            ImGui.Spacing();

            ImGui.Text("Folder Path:");
            ImGui.SetNextItemWidth(-120);
            ImGui.InputText("##folderpath", ref _folderInputPath, 512);
            ImGui.SameLine();
            if (ImGui.Button("Browse...", new System.Numerics.Vector2(100, 0)))
            {
                _folderBrowserPath = string.IsNullOrEmpty(_folderInputPath) ? ModelFileDialog.GetModelDirectory() : _folderInputPath;
                _showFolderBrowser = true;
            }

            ImGui.Spacing();

            if (ImGui.Button("Default Models"))
                _folderInputPath = ModelFileDialog.GetModelDirectory();
            ImGui.SameLine();
            if (ImGui.Button("Home"))
                _folderInputPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button("Load Folder", new System.Numerics.Vector2(120, 0)))
            {
                if (Directory.Exists(_folderInputPath))
                {
                    OnFolderSelected?.Invoke(_folderInputPath);
                    _showFolderInput = false;
                }
                else
                {
                    _logger.LogWarning("[EDITOR] Folder does not exist: {Path}", _folderInputPath);
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel", new System.Numerics.Vector2(120, 0)))
                _showFolderInput = false;

            if (!string.IsNullOrEmpty(_folderInputPath))
            {
                if (Directory.Exists(_folderInputPath))
                {
                    var prmCount = Directory.GetFiles(_folderInputPath, "*.prm", SearchOption.TopDirectoryOnly).Length;
                    ImGui.TextColored(new System.Numerics.Vector4(0, 1, 0, 1), $"✓ Folder exists ({prmCount} PRM files)");
                }
                else
                {
                    ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "✗ Folder does not exist");
                }
            }
        }
        ImGui.End();
    }

    #endregion 
}