using Xunit;
using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Tests.Core.Services;

public class AudioSettingsTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var settings = new AudioSettings();

        Assert.Equal(1.0f, settings.MasterVolume);
        Assert.Equal(0.7f, settings.MusicVolume);
        Assert.Equal(1.0f, settings.SoundEffectsVolume);
        Assert.False(settings.IsMuted);
        Assert.True(settings.MusicEnabled);
        Assert.True(settings.SoundEffectsEnabled);
        Assert.Equal("Random", settings.MusicMode);
    }

    [Fact]
    public void IsValid_WithDefaultValues_ReturnsTrue()
    {
        var settings = new AudioSettings();

        Assert.True(settings.IsValid());
    }

    [Fact]
    public void IsValid_WithInvalidMasterVolume_ReturnsFalse()
    {
        var settings = new AudioSettings { MasterVolume = 1.5f };

        Assert.False(settings.IsValid());
    }

    [Fact]
    public void IsValid_WithInvalidMusicVolume_ReturnsFalse()
    {
        var settings = new AudioSettings { MusicVolume = -0.1f };

        Assert.False(settings.IsValid());
    }

    [Fact]
    public void IsValid_WithInvalidSoundEffectsVolume_ReturnsFalse()
    {
        var settings = new AudioSettings { SoundEffectsVolume = 2.0f };

        Assert.False(settings.IsValid());
    }

    [Fact]
    public void IsValid_WithInvalidMusicMode_ReturnsFalse()
    {
        var settings = new AudioSettings { MusicMode = "Invalid" };

        Assert.False(settings.IsValid());
    }

    [Fact]
    public void ResetToDefaults_RestoresOriginalValues()
    {
        var settings = new AudioSettings
        {
            MasterVolume = 0.5f,
            MusicVolume = 0.3f,
            SoundEffectsVolume = 0.8f,
            IsMuted = true,
            MusicEnabled = false,
            SoundEffectsEnabled = false,
            MusicMode = "Sequential"
        };

        settings.ResetToDefaults();

        Assert.Equal(1.0f, settings.MasterVolume);
        Assert.Equal(0.7f, settings.MusicVolume);
        Assert.Equal(1.0f, settings.SoundEffectsVolume);
        Assert.False(settings.IsMuted);
        Assert.True(settings.MusicEnabled);
        Assert.True(settings.SoundEffectsEnabled);
        Assert.Equal("Random", settings.MusicMode);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    public void IsValid_WithValidVolumes_ReturnsTrue(float volume)
    {
        var settings = new AudioSettings
        {
            MasterVolume = volume,
            MusicVolume = volume,
            SoundEffectsVolume = volume
        };

        Assert.True(settings.IsValid());
    }

    [Theory]
    [InlineData("Random")]
    [InlineData("Sequential")]
    [InlineData("Loop")]
    public void IsValid_WithValidMusicMode_ReturnsTrue(string mode)
    {
        var settings = new AudioSettings { MusicMode = mode };

        Assert.True(settings.IsValid());
    }
}
