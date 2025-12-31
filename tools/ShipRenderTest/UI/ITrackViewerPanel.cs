namespace WipeoutRewrite.Tools.UI;

public interface ITrackViewerPanel : IUIPanel
{
    /// <summary>
    /// Event triggered when a track is selected and should be loaded.
    /// Passes the track number (1-14) and the wipeout data directory.
    /// </summary>
    event Action<int, string>? OnTrackLoadRequested;

    /// <summary>
    /// Set the root wipeout data directory (containing track01, track02, etc.)
    /// </summary>
    void SetWipeoutDataDirectory(string directory);
}