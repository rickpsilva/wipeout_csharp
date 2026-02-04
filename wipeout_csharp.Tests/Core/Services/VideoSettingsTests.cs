using Xunit;
using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Tests.Core.Services;

public class VideoSettingsTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var settings = new VideoSettings();

        Assert.False(settings.Fullscreen);
        Assert.Equal(0.0f, settings.InternalRoll);
        Assert.Equal(0u, settings.UIScale);  // 0 = AUTO mode (default)
        Assert.False(settings.ShowFPS);
        Assert.Equal(ScreenResolutionType.Native, settings.ScreenResolution);
        Assert.Equal(PostEffectType.None, settings.PostEffect);
    }

    [Fact]
    public void IsValid_WithDefaultValues_ReturnsTrue()
    {
        var settings = new VideoSettings();

        Assert.True(settings.IsValid());
    }

    [Fact]
    public void IsValid_WithInvalidUIScale_ReturnsFalse()
    {
        var settings = new VideoSettings { UIScale = 5 };

        Assert.False(settings.IsValid());
    }

    [Fact]
    public void IsValid_WithInvalidScreenResolution_ReturnsFalse()
    {
        var settings = new VideoSettings { ScreenResolution = (ScreenResolutionType)3 };

        Assert.False(settings.IsValid());
    }

    [Fact]
    public void IsValid_WithInvalidPostEffect_ReturnsFalse()
    {
        var settings = new VideoSettings { PostEffect = (PostEffectType)2 };

        Assert.False(settings.IsValid());
    }

    [Fact]
    public void IsValid_WithInvalidInternalRoll_ReturnsFalse()
    {
        var settings = new VideoSettings { InternalRoll = 200.0f };

        Assert.False(settings.IsValid());
    }

    [Fact]
    public void ResetToDefaults_RestoresOriginalValues()
    {
        var settings = new VideoSettings
        {
            Fullscreen = true,
            InternalRoll = 45.0f,
            UIScale = 4,
            ShowFPS = true,
            ScreenResolution = ScreenResolutionType.Res480p,
            PostEffect = PostEffectType.CRT
        };

        settings.ResetToDefaults();

        Assert.False(settings.Fullscreen);
        Assert.Equal(0.0f, settings.InternalRoll);
        Assert.Equal(0u, settings.UIScale);  // 0 = AUTO mode (default)
        Assert.False(settings.ShowFPS);
        Assert.Equal(ScreenResolutionType.Native, settings.ScreenResolution);
        Assert.Equal(PostEffectType.None, settings.PostEffect);
    }

    [Theory]
    [InlineData(1u)]
    [InlineData(2u)]
    [InlineData(3u)]
    [InlineData(4u)]
    public void IsValid_WithValidUIScale_ReturnsTrue(uint scale)
    {
        var settings = new VideoSettings { UIScale = scale };

        Assert.True(settings.IsValid());
    }

    [Theory]
    [InlineData(ScreenResolutionType.Native)]
    [InlineData(ScreenResolutionType.Res240p)]
    [InlineData(ScreenResolutionType.Res480p)]
    public void IsValid_WithValidScreenResolution_ReturnsTrue(ScreenResolutionType resolution)
    {
        var settings = new VideoSettings { ScreenResolution = resolution };

        Assert.True(settings.IsValid());
    }

    [Theory]
    [InlineData(-180.0f)]
    [InlineData(-90.0f)]
    [InlineData(0.0f)]
    [InlineData(90.0f)]
    [InlineData(180.0f)]
    public void IsValid_WithValidInternalRoll_ReturnsTrue(float roll)
    {
        var settings = new VideoSettings { InternalRoll = roll };

        Assert.True(settings.IsValid());
    }
}
