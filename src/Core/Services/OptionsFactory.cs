using Microsoft.Extensions.Logging;
using WipeoutRewrite.Infrastructure.Database;
using WipeoutRewrite.Infrastructure.Database.Entities;

namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Default implementation of options factory.
/// Creates and initializes settings instances with proper logging.
/// Loads settings from database if available.
/// </summary>
public class OptionsFactory : IOptionsFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ISettingsRepository _repository;

    public OptionsFactory(ILoggerFactory loggerFactory, ISettingsRepository repository)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public IControlsSettings CreateControlsSettings()
    {
        var logger = _loggerFactory.CreateLogger<ControlsSettings>();
        logger.LogInformation("[OPTIONS] Creating ControlsSettings instance");

        var settings = new ControlsSettings();
        TryLoadControlsSettings(settings, logger);
        return settings;
    }

    public IVideoSettings CreateVideoSettings()
    {
        var logger = _loggerFactory.CreateLogger<VideoSettings>();
        logger.LogInformation("[OPTIONS] Creating VideoSettings instance");

        var settings = new VideoSettings();
        TryLoadVideoSettings(settings, logger);
        return settings;
    }

    public IAudioSettings CreateAudioSettings()
    {
        var logger = _loggerFactory.CreateLogger<AudioSettings>();
        logger.LogInformation("[OPTIONS] Creating AudioSettings instance");

        var settings = new AudioSettings();
        TryLoadAudioSettings(settings, logger);
        return settings;
    }

    public IBestTimesManager CreateBestTimesManager()
    {
        var logger = _loggerFactory.CreateLogger<BestTimesManager>();
        logger.LogInformation("[OPTIONS] Creating BestTimesManager instance");
        return new BestTimesManager(logger, _repository);
    }

    private void TryLoadControlsSettings(ControlsSettings settings, ILogger<ControlsSettings> logger)
    {
        try
        {
            var entity = _repository.LoadControlsSettings();
            logger.LogInformation("[OPTIONS] Loading ControlsSettings from database");
            
            // Convert database entity to in-memory settings
            var buttons = entity.ToButtonArray();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    settings.SetButtonBinding((RaceAction)i, (InputDevice)j, buttons[i, j]);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[OPTIONS] Failed to load ControlsSettings from database, using defaults");
        }
    }

    private void TryLoadVideoSettings(VideoSettings settings, ILogger<VideoSettings> logger)
    {
        try
        {
            var entity = _repository.LoadVideoSettings();
            logger.LogInformation("[OPTIONS] Loading VideoSettings from database");
            
            // Map from entity to settings
            settings.Fullscreen = entity.Fullscreen;
            settings.InternalRoll = entity.InternalRoll;
            settings.UIScale = entity.UIScale;
            settings.ShowFPS = entity.ShowFPS;
            settings.ScreenResolution = entity.ScreenResolution;
            settings.PostEffect = entity.PostEffect;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[OPTIONS] Failed to load VideoSettings from database, using defaults");
        }
    }

    private void TryLoadAudioSettings(AudioSettings settings, ILogger<AudioSettings> logger)
    {
        try
        {
            var entity = _repository.LoadAudioSettings();
            logger.LogInformation("[OPTIONS] Loading AudioSettings from database");
            
            // Map from entity to settings
            settings.MasterVolume = entity.MasterVolume;
            settings.MusicVolume = entity.MusicVolume;
            settings.SoundEffectsVolume = entity.SoundEffectsVolume;
            settings.IsMuted = entity.IsMuted;
            settings.MusicEnabled = entity.MusicEnabled;
            settings.SoundEffectsEnabled = entity.SoundEffectsEnabled;
            settings.MusicMode = entity.MusicMode;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[OPTIONS] Failed to load AudioSettings from database, using defaults");
        }
    }
}
