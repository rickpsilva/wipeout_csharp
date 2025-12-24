namespace WipeoutRewrite.Tools.UI;

public interface IAssetBrowserPanel : IUIPanel
{
    /// <summary>
    /// Event triggered when "Add to Scene" is requested.
    /// </summary>
    event Action<string, int>? OnAddToSceneRequested;
}