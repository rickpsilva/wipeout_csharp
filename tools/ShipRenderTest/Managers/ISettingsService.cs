namespace WipeoutRewrite.Tools.Managers;

/// <summary>
/// Service for managing application settings.
/// Follows Dependency Inversion Principle - depend on abstraction not concretion.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Current application settings.
    /// </summary>
    AppSettings Settings { get; }

    /// <summary>
    /// Load settings from disk.
    /// </summary>
    void LoadSettings();
    /// <summary>
    /// Save settings to disk.
    /// </summary>
    void SaveSettings();
    /// <summary>
    /// Set UI scale and save.
    /// </summary>
    void SetUIScale(float scale);
}