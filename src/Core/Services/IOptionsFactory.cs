namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Factory interface for creating options/settings instances.
/// </summary>
public interface IOptionsFactory
{
    /// <summary>
    /// Creates a new controls settings instance.
    /// </summary>
    IControlsSettings CreateControlsSettings();

    /// <summary>
    /// Creates a new video settings instance.
    /// </summary>
    IVideoSettings CreateVideoSettings();

    /// <summary>
    /// Creates a new audio settings instance.
    /// </summary>
    IAudioSettings CreateAudioSettings();

    /// <summary>
    /// Creates a new best times manager instance.
    /// </summary>
    IBestTimesManager CreateBestTimesManager();
}
