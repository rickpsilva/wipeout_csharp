namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// Helper class to browse and select PRM model files.
/// </summary>
public static class ModelFileDialog
{
    #region methods

    /// <summary>
    /// Get all available PRM files in the models directory.
    /// </summary>
    public static IEnumerable<(string path, string name)> GetAvailablePrmFiles()
    {
        var modelDir = GetModelDirectory();

        if (!Directory.Exists(modelDir))
        {
            return Enumerable.Empty<(string, string)>();
        }

        try
        {
            return Directory.GetFiles(modelDir, "*.prm", SearchOption.AllDirectories)
                .OrderBy(f => Path.GetFileName(f))
                .Select(f => (f, Path.GetFileName(f)));
        }
        catch
        {
            return Enumerable.Empty<(string, string)>();
        }
    }

    /// <summary>
    /// Get the models directory path.
    /// </summary>
    public static string GetModelDirectory()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var modelsDir = Path.Combine(baseDir, "wipeout", "common");

        if (Directory.Exists(modelsDir))
        {
            return modelsDir;
        }

        // Fallback to user home directory
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    /// <summary>
    /// Get all PRM files from a specific directory.
    /// </summary>
    public static IEnumerable<string> GetPrmFilesFromDirectory(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            return Directory.GetFiles(directoryPath, "*.prm", SearchOption.TopDirectoryOnly)
                .OrderBy(f => Path.GetFileName(f));
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Check if a file exists and is a valid PRM file.
    /// </summary>
    public static bool IsValidPrmFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        return File.Exists(filePath) &&
               Path.GetExtension(filePath).Equals(".prm", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Open a folder browser dialog (using Zenity on Linux, native dialogs on other platforms).
    /// Returns the selected folder path or null if cancelled.
    /// </summary>
    public static string? OpenFolderDialog(string title = "Select Folder")
    {
        try
        {
            // Linux: Try multiple dialog tools
            if (OperatingSystem.IsLinux())
            {
                // Try Zenity first
                var result = TryLinuxDialogTool("zenity", $"--file-selection --directory --title=\"{title}\"");
                if (result != null) return result;

                // Try kdialog (KDE)
                result = TryLinuxDialogTool("kdialog", $"--getexistingdirectory . \"{title}\"");
                if (result != null) return result;

                // Try yad (Yet Another Dialog)
                result = TryLinuxDialogTool("yad", $"--file --directory --title=\"{title}\"");
                if (result != null) return result;
            }
            // macOS: Use osascript
            else if (OperatingSystem.IsMacOS())
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "osascript",
                        Arguments = "-e 'POSIX path of (choose folder with prompt \"" + title + "\")'",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string result = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Try to execute a Linux dialog tool and return the result.
    /// </summary>
    private static string? TryLinuxDialogTool(string tool, string arguments)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tool,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (process.ExitCode == 0 && !string.IsNullOrEmpty(result))
            {
                return result;
            }
        }
        catch
        {
            // Tool not available or failed
        }

        return null;
    }

    #endregion 
}