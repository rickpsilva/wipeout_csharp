using Xunit;
using WipeoutRewrite.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using WipeoutRewrite.Infrastructure.Database;

namespace WipeoutRewrite.Tests.Core.Services;

public class OptionsFactoryTests
{
    private OptionsFactory CreateFactory()
    {
        var mockRepository = new Mock<ISettingsRepository>();
        return new OptionsFactory(new TestLoggerFactory(), mockRepository.Object);
    }

    [Fact]
    public void CreateControlsSettings_ReturnsValidInstance()
    {
        var factory = CreateFactory();

        var settings = factory.CreateControlsSettings();

        Assert.NotNull(settings);
        Assert.IsAssignableFrom<IControlsSettings>(settings);
        Assert.True(settings.IsValid());
    }

    [Fact]
    public void CreateVideoSettings_ReturnsValidInstance()
    {
        var factory = CreateFactory();

        var settings = factory.CreateVideoSettings();

        Assert.NotNull(settings);
        Assert.IsAssignableFrom<IVideoSettings>(settings);
        Assert.True(settings.IsValid());
    }

    [Fact]
    public void CreateAudioSettings_ReturnsValidInstance()
    {
        var factory = CreateFactory();

        var settings = factory.CreateAudioSettings();

        Assert.NotNull(settings);
        Assert.IsAssignableFrom<IAudioSettings>(settings);
        Assert.True(settings.IsValid());
    }

    [Fact]
    public void CreateBestTimesManager_ReturnsValidInstance()
    {
        var factory = CreateFactory();

        var manager = factory.CreateBestTimesManager();

        Assert.NotNull(manager);
        Assert.IsAssignableFrom<IBestTimesManager>(manager);
    }

    [Fact]
    public void CreateControlsSettings_MultipleCallsReturnDifferentInstances()
    {
        var factory = CreateFactory();

        var settings1 = factory.CreateControlsSettings();
        var settings2 = factory.CreateControlsSettings();

        Assert.NotSame(settings1, settings2);
    }

    [Fact]
    public void CreateVideoSettings_MultipleCallsReturnDifferentInstances()
    {
        var factory = CreateFactory();

        var settings1 = factory.CreateVideoSettings();
        var settings2 = factory.CreateVideoSettings();

        Assert.NotSame(settings1, settings2);
    }

    [Fact]
    public void CreateAudioSettings_MultipleCallsReturnDifferentInstances()
    {
        var factory = CreateFactory();

        var settings1 = factory.CreateAudioSettings();
        var settings2 = factory.CreateAudioSettings();

        Assert.NotSame(settings1, settings2);
    }

    [Fact]
    public void CreateBestTimesManager_MultipleCallsReturnDifferentInstances()
    {
        var factory = CreateFactory();

        var manager1 = factory.CreateBestTimesManager();
        var manager2 = factory.CreateBestTimesManager();

        Assert.NotSame(manager1, manager2);
    }
}

// Test logger factory and logger helpers
public class TestLoggerFactory : ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider) { }
    public ILogger CreateLogger(string categoryName) => new TestLoggerForTests();
    public void Dispose() { }
}

public class TestLoggerForTests : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}

// Test logger helper
public class TestLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
