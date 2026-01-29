using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Infrastructure.Database;
using WipeoutRewrite.Infrastructure.Database.Entities;

namespace WipeoutRewrite.Tests.Core.Services;

public class SettingsPersistenceServiceIntegrationTests
{
    private readonly Mock<ISettingsRepository> _mockRepository;
    private readonly Mock<ILogger<SettingsPersistenceService>> _mockLogger;
    private readonly Mock<IControlsSettings> _mockControlsSettings;
    private readonly Mock<IVideoSettings> _mockVideoSettings;
    private readonly Mock<IAudioSettings> _mockAudioSettings;
    private readonly SettingsPersistenceService _service;

    public SettingsPersistenceServiceIntegrationTests()
    {
        _mockRepository = new Mock<ISettingsRepository>();
        _mockLogger = new Mock<ILogger<SettingsPersistenceService>>();
        _mockControlsSettings = new Mock<IControlsSettings>();
        _mockVideoSettings = new Mock<IVideoSettings>();
        _mockAudioSettings = new Mock<IAudioSettings>();

        _service = new SettingsPersistenceService(
            _mockRepository.Object,
            _mockLogger.Object,
            _mockControlsSettings.Object,
            _mockVideoSettings.Object,
            _mockAudioSettings.Object
        );
    }

    [Fact]
    public void GetControlsSettings_ReturnsInstance()
    {
        var result = _service.GetControlsSettings();
        Assert.Same(_mockControlsSettings.Object, result);
    }

    [Fact]
    public void GetVideoSettings_ReturnsInstance()
    {
        var result = _service.GetVideoSettings();
        Assert.Same(_mockVideoSettings.Object, result);
    }

    [Fact]
    public void GetAudioSettings_ReturnsInstance()
    {
        var result = _service.GetAudioSettings();
        Assert.Same(_mockAudioSettings.Object, result);
    }

    [Fact]
    public void SaveControlsSettings_CallsRepository()
    {
        // Setup button binding returns
        _mockControlsSettings
            .Setup(s => s.GetButtonBinding(It.IsAny<RaceAction>(), It.IsAny<InputDevice>()))
            .Returns(100u);

        _service.SaveControlsSettings();

        _mockRepository.Verify(
            r => r.SaveControlsSettings(It.IsAny<ControlsSettingsEntity>()),
            Times.Once);
        _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
    }

    [Fact]
    public void SaveVideoSettings_CreatesCorrectEntity()
    {
        _mockVideoSettings.Setup(s => s.Fullscreen).Returns(true);
        _mockVideoSettings.Setup(s => s.InternalRoll).Returns(45.0f);
        _mockVideoSettings.Setup(s => s.UIScale).Returns(2u);
        _mockVideoSettings.Setup(s => s.ShowFPS).Returns(true);
        _mockVideoSettings.Setup(s => s.ScreenResolution).Returns(ScreenResolutionType.Res240p);
        _mockVideoSettings.Setup(s => s.PostEffect).Returns(PostEffectType.None);

        _service.SaveVideoSettings();

        _mockRepository.Verify(
            r => r.SaveVideoSettings(It.Is<VideoSettingsEntity>(e =>
                e.Fullscreen == true &&
                e.InternalRoll == 45.0f &&
                e.UIScale == 2u &&
                e.ShowFPS == true &&
                e.ScreenResolution == ScreenResolutionType.Res240p &&
                e.PostEffect == PostEffectType.None)),
            Times.Once);
    }

    [Fact]
    public void SaveAudioSettings_CreatesCorrectEntity()
    {
        _mockAudioSettings.Setup(s => s.MasterVolume).Returns(0.8f);
        _mockAudioSettings.Setup(s => s.MusicVolume).Returns(0.7f);
        _mockAudioSettings.Setup(s => s.SoundEffectsVolume).Returns(0.9f);
        _mockAudioSettings.Setup(s => s.IsMuted).Returns(false);
        _mockAudioSettings.Setup(s => s.MusicEnabled).Returns(true);
        _mockAudioSettings.Setup(s => s.SoundEffectsEnabled).Returns(true);
        _mockAudioSettings.Setup(s => s.MusicMode).Returns("Random");

        _service.SaveAudioSettings();

        _mockRepository.Verify(
            r => r.SaveAudioSettings(It.Is<AudioSettingsEntity>(e =>
                e.MasterVolume == 0.8f &&
                e.MusicVolume == 0.7f &&
                e.SoundEffectsVolume == 0.9f &&
                e.IsMuted == false &&
                e.MusicEnabled == true &&
                e.SoundEffectsEnabled == true &&
                e.MusicMode == "Random")),
            Times.Once);
    }

    [Fact]
    public void SaveAllSettings_CallsAllSaveMethods()
    {
        _mockControlsSettings
            .Setup(s => s.GetButtonBinding(It.IsAny<RaceAction>(), It.IsAny<InputDevice>()))
            .Returns(100u);

        _service.SaveAllSettings();

        _mockRepository.Verify(r => r.SaveControlsSettings(It.IsAny<ControlsSettingsEntity>()), Times.Once);
        _mockRepository.Verify(r => r.SaveVideoSettings(It.IsAny<VideoSettingsEntity>()), Times.Once);
        _mockRepository.Verify(r => r.SaveAudioSettings(It.IsAny<AudioSettingsEntity>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChanges(), Times.Exactly(4));
    }

    [Fact]
    public void SaveControlsSettings_HandlesException()
    {
        _mockControlsSettings
            .Setup(s => s.GetButtonBinding(It.IsAny<RaceAction>(), It.IsAny<InputDevice>()))
            .Returns(100u);
        _mockRepository
            .Setup(r => r.SaveControlsSettings(It.IsAny<ControlsSettingsEntity>()))
            .Throws(new Exception("DB Error"));

        _service.SaveControlsSettings();

        // Should not throw
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void SaveVideoSettings_HandlesException()
    {
        _mockRepository
            .Setup(r => r.SaveVideoSettings(It.IsAny<VideoSettingsEntity>()))
            .Throws(new Exception("DB Error"));

        _service.SaveVideoSettings();

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void SaveAudioSettings_HandlesException()
    {
        _mockRepository
            .Setup(r => r.SaveAudioSettings(It.IsAny<AudioSettingsEntity>()))
            .Throws(new Exception("DB Error"));

        _service.SaveAudioSettings();

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void SaveAllSettings_ContinuesOnException()
    {
        _mockRepository
            .Setup(r => r.SaveControlsSettings(It.IsAny<ControlsSettingsEntity>()))
            .Throws(new Exception("DB Error"));

        _service.SaveAllSettings();

        // Should still try to save video and audio
        _mockRepository.Verify(r => r.SaveVideoSettings(It.IsAny<VideoSettingsEntity>()), Times.Once);
        _mockRepository.Verify(r => r.SaveAudioSettings(It.IsAny<AudioSettingsEntity>()), Times.Once);
    }
}
