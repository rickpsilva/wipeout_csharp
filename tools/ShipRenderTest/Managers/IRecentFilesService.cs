namespace WipeoutRewrite.Tools.Managers;

/// <summary>
/// Service for managing recent files list.
/// Follows Dependency Inversion Principle - depend on abstraction not concretion.
/// </summary>
public interface IRecentFilesService
{
    /// <summary>
    /// List of recent items.
    /// </summary>
    IReadOnlyList<RecentItem> RecentItems { get; }

    /// <summary>
    /// Add a file to the recent files list.
    /// </summary>
    void AddRecentFile(string filePath, bool isFolder = false);
    /// <summary>
    /// Clear the recent files list.
    /// </summary>
    void ClearRecentFiles();
}