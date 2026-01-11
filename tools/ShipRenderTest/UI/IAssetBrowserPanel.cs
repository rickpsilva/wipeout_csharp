namespace WipeoutRewrite.Tools.UI;

public interface IAssetBrowserPanel : IUIPanel
{
    /// <summary>
    /// Event triggered when "Add to Scene" is requested for a single model.
    /// </summary>
    event Action<string, int>? OnAddToSceneRequested;
}