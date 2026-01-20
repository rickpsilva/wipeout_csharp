using Xunit;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Infrastructure.Logging;

namespace WipeoutRewrite.Tests.Infrastructure.Logging;

public class LoggingHelperTests
{
    private class TestLoggerProvider : ILoggerProvider
    {
        private readonly List<LogEntry> _logs = new();
        
        public IReadOnlyList<LogEntry> Logs => _logs.AsReadOnly();

        public ILogger CreateLogger(string categoryName) => new TestLogger(_logs);

        public void Dispose() { }

        private class TestLogger : ILogger
        {
            private readonly List<LogEntry> _logs;

            public TestLogger(List<LogEntry> logs) => _logs = logs;

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => 
                new NoopDisposable();

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                _logs.Add(new LogEntry
                {
                    LogLevel = logLevel,
                    Message = formatter(state, exception)
                });
            }

            private sealed class NoopDisposable : IDisposable
            {
                public void Dispose() { }
            }
        }
    }

    public class LogEntry
    {
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; } = "";
    }

    [Fact]
    public void LogComponentInfo_FormatsMessageWithComponentPrefix()
    {
        // Arrange
        var provider = new TestLoggerProvider();
        var logger = provider.CreateLogger("test");

        // Act
        LoggingHelper.LogComponentInfo(logger, "TestComponent", "Info message");

        // Assert
        var log = provider.Logs.Single();
        Assert.Equal(LogLevel.Information, log.LogLevel);
        Assert.Equal("[TestComponent] Info message", log.Message);
    }

    [Fact]
    public void LogComponentDebug_FormatsMessageWithComponentPrefix()
    {
        // Arrange
        var provider = new TestLoggerProvider();
        var logger = provider.CreateLogger("test");

        // Act
        LoggingHelper.LogComponentDebug(logger, "DebugComp", "Debug message");

        // Assert
        var log = provider.Logs.Single();
        Assert.Equal(LogLevel.Debug, log.LogLevel);
        Assert.Equal("[DebugComp] Debug message", log.Message);
    }

    [Fact]
    public void LogComponentWarning_FormatsMessageWithComponentPrefix()
    {
        // Arrange
        var provider = new TestLoggerProvider();
        var logger = provider.CreateLogger("test");

        // Act
        LoggingHelper.LogComponentWarning(logger, "WarnComp", "Warning message");

        // Assert
        var log = provider.Logs.Single();
        Assert.Equal(LogLevel.Warning, log.LogLevel);
        Assert.Equal("[WarnComp] Warning message", log.Message);
    }

    [Fact]
    public void LogComponentError_FormatsMessageWithComponentPrefix()
    {
        // Arrange
        var provider = new TestLoggerProvider();
        var logger = provider.CreateLogger("test");

        // Act
        LoggingHelper.LogComponentError(logger, "ErrorComp", "Error message");

        // Assert
        var log = provider.Logs.Single();
        Assert.Equal(LogLevel.Error, log.LogLevel);
        Assert.Equal("[ErrorComp] Error message", log.Message);
    }

    [Fact]
    public void LogComponentInfo_WithFormatArgs_FormatsCorrectly()
    {
        // Arrange
        var provider = new TestLoggerProvider();
        var logger = provider.CreateLogger("test");

        // Act
        LoggingHelper.LogComponentInfo(logger, "Component", "Value: {0}", 42);

        // Assert
        var log = provider.Logs.Single();
        Assert.Equal(LogLevel.Information, log.LogLevel);
        Assert.Contains("[Component]", log.Message);
    }

    [Fact]
    public void LogComponentDebug_WithMultipleFormatArgs_FormatsCorrectly()
    {
        // Arrange
        var provider = new TestLoggerProvider();
        var logger = provider.CreateLogger("test");

        // Act
        LoggingHelper.LogComponentDebug(logger, "Component", "X: {0}, Y: {1}", 10, 20);

        // Assert
        var log = provider.Logs.Single();
        Assert.Equal(LogLevel.Debug, log.LogLevel);
        Assert.Contains("[Component]", log.Message);
    }

    [Fact]
    public void LogComponentWarning_WithNullComponent_UsesComponentName()
    {
        // Arrange
        var provider = new TestLoggerProvider();
        var logger = provider.CreateLogger("test");

        // Act
        LoggingHelper.LogComponentWarning(logger, "", "Message");

        // Assert
        var log = provider.Logs.Single();
        Assert.Equal(LogLevel.Warning, log.LogLevel);
        Assert.Equal("[] Message", log.Message);
    }

    [Fact]
    public void LogComponentError_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var provider = new TestLoggerProvider();
        var logger = provider.CreateLogger("test");

        // Act
        LoggingHelper.LogComponentError(logger, "Component", "Error: {0}", "File not found!");

        // Assert
        var log = provider.Logs.Single();
        Assert.Equal(LogLevel.Error, log.LogLevel);
        Assert.Contains("[Component]", log.Message);
    }

    [Theory]
    [InlineData(LogLevel.Information, "Info")]
    [InlineData(LogLevel.Debug, "Debug")]
    [InlineData(LogLevel.Warning, "Warning")]
    [InlineData(LogLevel.Error, "Error")]
    public void LogMethods_ProduceCorrectLogLevel(LogLevel expectedLevel, string methodName)
    {
        // Arrange
        var provider = new TestLoggerProvider();
        var logger = provider.CreateLogger("test");
        var component = "TestComp";
        var message = "Test message";

        // Act
        switch (methodName)
        {
            case "Info":
                LoggingHelper.LogComponentInfo(logger, component, message);
                break;
            case "Debug":
                LoggingHelper.LogComponentDebug(logger, component, message);
                break;
            case "Warning":
                LoggingHelper.LogComponentWarning(logger, component, message);
                break;
            case "Error":
                LoggingHelper.LogComponentError(logger, component, message);
                break;
        }

        // Assert
        var log = provider.Logs.Single();
        Assert.Equal(expectedLevel, log.LogLevel);
        Assert.Equal($"[{component}] {message}", log.Message);
    }
}
