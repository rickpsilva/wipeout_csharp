namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Defines the interface for audio settings management.
/// Controls volume levels and audio preferences.
/// </summary>
public interface IAudioSettings
{
    /// <summary>
    /// Gets or sets the master volume level (0.0 to 1.0).
    /// </summary>
    float MasterVolume { get; set; }

    /// <summary>
    /// Gets or sets the music volume level (0.0 to 1.0).
    /// </summary>
    float MusicVolume { get; set; }

    /// <summary>
    /// Gets or sets the sound effects volume level (0.0 to 1.0).
    /// </summary>
    float SoundEffectsVolume { get; set; }

    /// <summary>
    /// Gets or sets whether audio is muted.
    /// </summary>
    bool IsMuted { get; set; }

    /// <summary>
    /// Gets or sets whether music playback is enabled.
    /// </summary>
    bool MusicEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether sound effects playback is enabled.
    /// </summary>
    bool SoundEffectsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the music playback mode (Random, Sequential, Loop).
    /// </summary>
    string MusicMode { get; set; }

    /// <summary>
    /// Resets all audio settings to defaults.
    /// </summary>
    void ResetToDefaults();

    /// <summary>
    /// Validates the current settings.
    /// </summary>
    /// <returns>True if all settings are valid; otherwise false.</returns>
    bool IsValid();
}
