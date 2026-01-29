using Microsoft.Extensions.Logging;
using WipeoutRewrite.Infrastructure.Database;
using WipeoutRewrite.Infrastructure.Database.Entities;

namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Service to persist settings changes to database automatically.
/// Should be called whenever settings are modified.
/// </summary>
public class SettingsPersistenceService : ISettingsPersistenceService
{
    private readonly ISettingsRepository _repository;
    private readonly ILogger<SettingsPersistenceService> _logger;
    private readonly IControlsSettings _controlsSettings;
    private readonly IVideoSettings _videoSettings;
    private readonly IAudioSettings _audioSettings;

    public SettingsPersistenceService(
        ISettingsRepository repository,
        ILogger<SettingsPersistenceService> logger,
        IControlsSettings controlsSettings,
        IVideoSettings videoSettings,
        IAudioSettings audioSettings)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _controlsSettings = controlsSettings ?? throw new ArgumentNullException(nameof(controlsSettings));
        _videoSettings = videoSettings ?? throw new ArgumentNullException(nameof(videoSettings));
        _audioSettings = audioSettings ?? throw new ArgumentNullException(nameof(audioSettings));
    }

    /// <summary>
    /// Get the ControlsSettings instance that this service is managing.
    /// </summary>
    public IControlsSettings GetControlsSettings() => _controlsSettings;

    /// <summary>
    /// Get the VideoSettings instance that this service is managing.
    /// </summary>
    public IVideoSettings GetVideoSettings() => _videoSettings;

    /// <summary>
    /// Get the AudioSettings instance that this service is managing.
    /// </summary>
    public IAudioSettings GetAudioSettings() => _audioSettings;

    /// <summary>
    /// Save all settings to database.
    /// </summary>
    public void SaveAllSettings()
    {
        TryExecute(() =>
        {
            SaveControlsSettings();
            SaveVideoSettings();
            SaveAudioSettings();
            _repository.SaveChanges();
        }, "All settings saved to database", "Failed to save all settings");
    }

    /// <summary>
    /// Save only controls settings to database.
    /// </summary>
    public void SaveControlsSettings()
    {
        TryExecute(() =>
        {
            var buttons = new uint[9, 2];
            for (int i = 0; i < 9; i++)
            {
                buttons[i, 0] = _controlsSettings.GetButtonBinding((RaceAction)i, InputDevice.Keyboard);
                buttons[i, 1] = _controlsSettings.GetButtonBinding((RaceAction)i, InputDevice.Joystick);
            }

            var entity = new ControlsSettingsEntity();
            entity.FromButtonArray(buttons);

            _repository.SaveControlsSettings(entity);
            _repository.SaveChanges();
        }, "Controls settings saved to database", "Failed to save controls settings");
    }

    /// <summary>
    /// Save only video settings to database.
    /// </summary>
    public void SaveVideoSettings()
    {
        TryExecute(() =>
        {
            var entity = new VideoSettingsEntity
            {
                Fullscreen = _videoSettings.Fullscreen,
                InternalRoll = _videoSettings.InternalRoll,
                UIScale = _videoSettings.UIScale,
                ShowFPS = _videoSettings.ShowFPS,
                ScreenResolution = _videoSettings.ScreenResolution,
                PostEffect = _videoSettings.PostEffect,
                LastModified = DateTime.UtcNow
            };

            _repository.SaveVideoSettings(entity);
            _repository.SaveChanges();
        }, "Video settings saved to database", "Failed to save video settings");
    }

    /// <summary>
    /// Save only audio settings to database.
    /// </summary>
    public void SaveAudioSettings()
    {
        TryExecute(() =>
        {
            var entity = new AudioSettingsEntity
            {
                MasterVolume = _audioSettings.MasterVolume,
                MusicVolume = _audioSettings.MusicVolume,
                SoundEffectsVolume = _audioSettings.SoundEffectsVolume,
                IsMuted = _audioSettings.IsMuted,
                MusicEnabled = _audioSettings.MusicEnabled,
                SoundEffectsEnabled = _audioSettings.SoundEffectsEnabled,
                MusicMode = _audioSettings.MusicMode,
                LastModified = DateTime.UtcNow
            };

            _repository.SaveAudioSettings(entity);
            _repository.SaveChanges();
        }, "Audio settings saved to database", "Failed to save audio settings");
    }

    /// <summary>
    /// Helper method to execute save operations with consistent error handling.
    /// </summary>
    private void TryExecute(Action operation, string successMessage, string errorMessage)
    {
        try
        {
            operation();
            _logger.LogInformation("[SETTINGS] {Message}", successMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SETTINGS] {Message}", errorMessage);
        }
    }
}
