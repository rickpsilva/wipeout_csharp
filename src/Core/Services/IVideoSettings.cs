namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Defines the interface for video settings management.
/// Controls display, graphics quality, and visual options.
/// </summary>
public interface IVideoSettings
{
    /// <summary>
    /// Gets or sets whether fullscreen mode is enabled.
    /// </summary>
    bool Fullscreen { get; set; }

    /// <summary>
    /// Gets or sets the internal roll/view angle offset.
    /// </summary>
    float InternalRoll { get; set; }

    /// <summary>
    /// Gets or sets the UI scale (1-4).
    /// </summary>
    uint UIScale { get; set; }

    /// <summary>
    /// Gets or sets whether to show frames per second overlay.
    /// </summary>
    bool ShowFPS { get; set; }

    /// <summary>
    /// Gets or sets the logical render resolution (Native/240p/480p).
    /// 0 = Native, 1 = 240p, 2 = 480p.
    /// </summary>
    ScreenResolutionType ScreenResolution { get; set; }

    /// <summary>
    /// Gets or sets the post effect (None/CRT).
    /// 0 = None, 1 = CRT.
    /// </summary>
    PostEffectType PostEffect { get; set; }

    /// <summary>
    /// Resets all video settings to defaults.
    /// </summary>
    void ResetToDefaults();

    /// <summary>
    /// Validates the current settings.
    /// </summary>
    /// <returns>True if all settings are valid; otherwise false.</returns>
    bool IsValid();
}
