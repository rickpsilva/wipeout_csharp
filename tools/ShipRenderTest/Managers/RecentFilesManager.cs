using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WipeoutRewrite.Tools.Managers;

/// <summary>
/// Manages recent files list.
/// Implements IRecentFilesService following Dependency Inversion Principle.
/// </summary>
public class RecentFilesManager : IRecentFilesService
{
    private const int MaxRecentItems = 10;
    private const string RecentFilesPath = "recent_files.json";

    public IReadOnlyList<RecentItem> RecentItems => _recentItems.AsReadOnly();

    private readonly ILogger<RecentFilesManager> _logger;
    private List<RecentItem> _recentItems;

    public RecentFilesManager(ILogger<RecentFilesManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _recentItems = new List<RecentItem>();
        LoadRecentFiles();
    }

    #region methods

    public void AddRecentFile(string filePath, bool isFolder = false)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) && !Directory.Exists(filePath))
            return;

        // Remove if already exists
        _recentItems.RemoveAll(item => item.Path.Equals(filePath, StringComparison.OrdinalIgnoreCase));

        // Add to front
        _recentItems.Insert(0, new RecentItem
        {
            Path = filePath,
            DisplayName = isFolder ? Path.GetFileName(filePath) : Path.GetFileNameWithoutExtension(filePath),
            IsFolder = isFolder,
            LastAccessed = DateTime.Now
        });

        // Keep only max items
        if (_recentItems.Count > MaxRecentItems)
        {
            _recentItems = _recentItems.Take(MaxRecentItems).ToList();
        }

        SaveRecentFiles();
    }

    public void ClearRecentFiles()
    {
        _recentItems.Clear();
        SaveRecentFiles();
    }

    private void LoadRecentFiles()
    {
        try
        {
            if (File.Exists(RecentFilesPath))
            {
                string json = File.ReadAllText(RecentFilesPath);
                _recentItems = JsonSerializer.Deserialize<List<RecentItem>>(json) ?? new List<RecentItem>();

                // Remove items that no longer exist
                _recentItems = _recentItems
                    .Where(item => File.Exists(item.Path) || Directory.Exists(item.Path))
                    .ToList();

                _logger.LogInformation("[RECENT] Loaded {Count} recent items", _recentItems.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RECENT] Failed to load recent files");
            _recentItems = new List<RecentItem>();
        }
    }

    private async Task LoadRecentFilesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(RecentFilesPath))
            {
                string json = await File.ReadAllTextAsync(RecentFilesPath, cancellationToken);
                _recentItems = JsonSerializer.Deserialize<List<RecentItem>>(json) ?? new List<RecentItem>();

                // Remove items that no longer exist
                _recentItems = _recentItems
                    .Where(item => File.Exists(item.Path) || Directory.Exists(item.Path))
                    .ToList();

                _logger.LogInformation("[RECENT] Loaded {Count} recent items", _recentItems.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RECENT] Failed to load recent files");
            _recentItems = new List<RecentItem>();
        }
    }

    private void SaveRecentFiles()
    {
        try
        {
            string json = JsonSerializer.Serialize(_recentItems, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(RecentFilesPath, json);
            _logger.LogInformation("[RECENT] Saved {Count} recent items", _recentItems.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RECENT] Failed to save recent files");
        }
    }

    private async Task SaveRecentFilesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string json = JsonSerializer.Serialize(_recentItems, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(RecentFilesPath, json, cancellationToken);
            _logger.LogInformation("[RECENT] Saved {Count} recent items", _recentItems.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RECENT] Failed to save recent files");
        }
    }

    #endregion 
}

public class RecentItem
{
    public string DisplayName { get; set; } = "";
    public bool IsFolder { get; set; }
    public DateTime LastAccessed { get; set; }
    public string Path { get; set; } = "";
}