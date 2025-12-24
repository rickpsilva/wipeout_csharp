namespace WipeoutRewrite.Tools.UI;

public interface ISettingsPanel : IUIPanel
{
    /// <summary>
    /// Sets the UI scale factor.
    /// </summary>
    void SetUIScale(float scale);
}