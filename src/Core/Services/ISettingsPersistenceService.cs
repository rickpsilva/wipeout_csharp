namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Interface for the SettingsPersistenceService.
/// </summary>
public interface ISettingsPersistenceService
{
    IControlsSettings GetControlsSettings();
    IVideoSettings GetVideoSettings();
    IAudioSettings GetAudioSettings();
    void SaveAllSettings();
    void SaveControlsSettings();
    void SaveVideoSettings();
    void SaveAudioSettings();
}
