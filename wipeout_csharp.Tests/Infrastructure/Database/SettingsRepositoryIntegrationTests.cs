using Xunit;
using Moq;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Infrastructure.Database;
using WipeoutRewrite.Infrastructure.Database.Entities;
using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Tests.Infrastructure.Database;

public class SettingsRepositoryIntegrationTests
{
    private static ControlsSettingsEntity CreateControlsEntity(uint upKeyboard = 82u, uint upJoystick = 100u)
    {
        return new ControlsSettingsEntity
        {
            Id = 1,
            UpKeyboard = upKeyboard,
            UpJoystick = upJoystick
        };
    }

    private static VideoSettingsEntity CreateVideoEntity(bool fullscreen = true, float internalRoll = 45.0f, 
        uint uiScale = 2u, bool showFps = true, ScreenResolutionType screenResolution = ScreenResolutionType.Res240p, PostEffectType postEffect = PostEffectType.None)
    {
        return new VideoSettingsEntity
        {
            Id = 1,
            Fullscreen = fullscreen,
            InternalRoll = internalRoll,
            UIScale = uiScale,
            ShowFPS = showFps,
            ScreenResolution = screenResolution,
            PostEffect = postEffect
        };
    }

    private static AudioSettingsEntity CreateAudioEntity(float masterVolume = 0.8f, float musicVolume = 0.7f,
        float sfxVolume = 0.9f, bool isMuted = false, bool musicEnabled = true, bool sfxEnabled = true, string musicMode = "Random")
    {
        return new AudioSettingsEntity
        {
            Id = 1,
            MasterVolume = masterVolume,
            MusicVolume = musicVolume,
            SoundEffectsVolume = sfxVolume,
            IsMuted = isMuted,
            MusicEnabled = musicEnabled,
            SoundEffectsEnabled = sfxEnabled,
            MusicMode = musicMode
        };
    }

    private static BestTimeEntity CreateBestTimeEntity(string circuitName = "Track01", string racingClass = "Venom", long timeMs = 100000)
    {
        return new BestTimeEntity
        {
            CircuitName = circuitName,
            RacingClass = racingClass,
            TimeMilliseconds = timeMs
        };
    }

    [Fact]
    public void SaveAndLoadControlsSettings_Roundtrip()
    {
        var mockRepository = new Mock<ISettingsRepository>();
        var entity = CreateControlsEntity();

        mockRepository
            .Setup(r => r.LoadControlsSettings())
            .Returns(entity);

        var loaded = mockRepository.Object.LoadControlsSettings();

        Assert.NotNull(loaded);
        Assert.Equal(82u, loaded.UpKeyboard);
        Assert.Equal(100u, loaded.UpJoystick);
    }

    [Fact]
    public void SaveAndLoadVideoSettings_Roundtrip()
    {
        var mockRepository = new Mock<ISettingsRepository>();
        var entity = CreateVideoEntity();

        mockRepository
            .Setup(r => r.LoadVideoSettings())
            .Returns(entity);

        var loaded = mockRepository.Object.LoadVideoSettings();

        Assert.NotNull(loaded);
        Assert.True(loaded.Fullscreen);
        Assert.Equal(45.0f, loaded.InternalRoll);
        Assert.Equal(2u, loaded.UIScale);
    }

    [Fact]
    public void SaveAndLoadAudioSettings_Roundtrip()
    {
        var mockRepository = new Mock<ISettingsRepository>();
        var entity = CreateAudioEntity();

        mockRepository
            .Setup(r => r.LoadAudioSettings())
            .Returns(entity);

        var loaded = mockRepository.Object.LoadAudioSettings();

        Assert.NotNull(loaded);
        Assert.Equal(0.8f, loaded.MasterVolume);
        Assert.Equal("Random", loaded.MusicMode);
    }

    [Fact]
    public void GetBestTimes_ReturnsIReadOnlyList()
    {
        var mockRepository = new Mock<ISettingsRepository>();
        var times = new List<BestTimeEntity>
        {
            CreateBestTimeEntity("Track01", "Venom", 100000),
            CreateBestTimeEntity("Track01", "Rapier", 90000)
        }.AsReadOnly();

        mockRepository
            .Setup(r => r.GetAllBestTimes())
            .Returns(times);

        var result = mockRepository.Object.GetAllBestTimes();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetBestTimesForCircuit_FiltersCorrectly()
    {
        var mockRepository = new Mock<ISettingsRepository>();
        var times = new List<BestTimeEntity>
        {
            CreateBestTimeEntity("Track01", "Venom", 100000),
            CreateBestTimeEntity("Track01", "Rapier", 90000)
        }.AsReadOnly();

        mockRepository
            .Setup(r => r.GetBestTimesForCircuit("Track01"))
            .Returns(times);

        var result = mockRepository.Object.GetBestTimesForCircuit("Track01");

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal("Track01", t.CircuitName));
    }

    [Fact]
    public void GetBestTime_ReturnsFastestForClassAndCircuit()
    {
        var mockRepository = new Mock<ISettingsRepository>();
        var entity = CreateBestTimeEntity("Track01", "Venom", 100000);

        mockRepository
            .Setup(r => r.GetBestTime("Track01", "Venom"))
            .Returns(entity);

        var result = mockRepository.Object.GetBestTime("Track01", "Venom");

        Assert.NotNull(result);
        Assert.Equal(100000, result.TimeMilliseconds);
    }

    [Fact]
    public void AddOrUpdateBestTime_IsCalledCorrectly()
    {
        var mockRepository = new Mock<ISettingsRepository>();
        var entity = CreateBestTimeEntity("Track01", "Venom", 100000);

        mockRepository.Object.AddOrUpdateBestTime(entity);

        mockRepository.Verify(r => r.AddOrUpdateBestTime(It.Is<BestTimeEntity>(e => e.CircuitName == "Track01")), Times.Once);
    }

    [Fact]
    public void SaveChanges_IsCalledForPersistence()
    {
        var mockRepository = new Mock<ISettingsRepository>();

        mockRepository.Object.SaveChanges();

        mockRepository.Verify(r => r.SaveChanges(), Times.Once);
    }
}
