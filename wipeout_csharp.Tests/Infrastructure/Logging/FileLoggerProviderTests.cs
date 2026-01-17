using Xunit;
using System.IO;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Infrastructure.Logging;

namespace WipeoutRewrite.Tests.Infrastructure.Logging;

public class FileLoggerProviderTests : IDisposable
{
    private readonly string _tempFilePath;

    public FileLoggerProviderTests()
    {
        _tempFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.log");
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
            File.Delete(_tempFilePath);
    }

    [Fact]
    public void Constructor_CreatesFileInSpecifiedLocation()
    {
        // Arrange & Act
        using var provider = new FileLoggerProvider(_tempFilePath);

        // Assert
        Assert.True(File.Exists(_tempFilePath));
    }

    [Fact]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileLoggerProvider(null!));
    }

    [Fact]
    public void Constructor_WithEmptyPath_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileLoggerProvider(""));
    }

    [Fact]
    public void Constructor_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var dir = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}");
        var filePath = Path.Combine(dir, "test.log");

        try
        {
            // Act
            using var provider = new FileLoggerProvider(filePath);

            // Assert
            Assert.True(Directory.Exists(dir));
            Assert.True(File.Exists(filePath));
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Constructor_WithMinLevel_SetsMinLevel()
    {
        // Arrange & Act
        using var provider = new FileLoggerProvider(_tempFilePath, LogLevel.Warning);

        // Assert
        Assert.Equal(LogLevel.Warning, provider.MinLevel);
    }

    [Fact]
    public void Constructor_DefaultMinLevel_IsInformation()
    {
        // Arrange & Act
        using var provider = new FileLoggerProvider(_tempFilePath);

        // Assert
        Assert.Equal(LogLevel.Information, provider.MinLevel);
    }

    [Fact]
    public void CreateLogger_ReturnsILogger()
    {
        // Arrange
        using var provider = new FileLoggerProvider(_tempFilePath);

        // Act
        var logger = provider.CreateLogger("test");

        // Assert
        Assert.NotNull(logger);
        Assert.IsAssignableFrom<ILogger>(logger);
    }

    [Fact]
    public void Logger_WritesInfoMessageToFile()
    {
        // Arrange
        using var provider = new FileLoggerProvider(_tempFilePath);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Test info message");

        // Assert
        var contents = File.ReadAllText(_tempFilePath);
        Assert.Contains("Test info message", contents);
        Assert.Contains("INFO", contents);
        Assert.Contains("TestCategory", contents);
    }

    [Fact]
    public void Logger_WritesDebugMessageWhenMinLevelIsDebug()
    {
        // Arrange
        using var provider = new FileLoggerProvider(_tempFilePath, LogLevel.Debug);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogDebug("Debug message");

        // Assert
        var contents = File.ReadAllText(_tempFilePath);
        Assert.Contains("Debug message", contents);
    }

    [Fact]
    public void Logger_IgnoresDebugMessageWhenMinLevelIsInformation()
    {
        // Arrange
        using var provider = new FileLoggerProvider(_tempFilePath, LogLevel.Information);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogDebug("Debug message");

        // Assert
        var contents = File.ReadAllText(_tempFilePath);
        Assert.DoesNotContain("Debug message", contents);
    }

    [Fact]
    public void Logger_WritesWarningMessage()
    {
        // Arrange
        using var provider = new FileLoggerProvider(_tempFilePath);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogWarning("Warning message");

        // Assert
        var contents = File.ReadAllText(_tempFilePath);
        Assert.Contains("Warning message", contents);
        Assert.Contains("WARN", contents);
    }

    [Fact]
    public void Logger_WritesErrorMessage()
    {
        // Arrange
        using var provider = new FileLoggerProvider(_tempFilePath);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogError("Error message");

        // Assert
        var contents = File.ReadAllText(_tempFilePath);
        Assert.Contains("Error message", contents);
        Assert.Contains("ERROR", contents);
    }

    [Fact]
    public void Logger_WritesExceptionToFile()
    {
        // Arrange
        using var provider = new FileLoggerProvider(_tempFilePath);
        var logger = provider.CreateLogger("TestCategory");
        var ex = new InvalidOperationException("Test exception");

        // Act
        logger.LogError(ex, "An error occurred");

        // Assert
        var contents = File.ReadAllText(_tempFilePath);
        Assert.Contains("An error occurred", contents);
        Assert.Contains("InvalidOperationException", contents);
        Assert.Contains("Test exception", contents);
    }

    [Fact]
    public void Logger_LogWithEventId()
    {
        // Arrange
        using var provider = new FileLoggerProvider(_tempFilePath);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.Log(LogLevel.Information, new EventId(123, "TestEvent"), "Message", null, (s, e) => s.ToString()!);

        // Assert
        var contents = File.ReadAllText(_tempFilePath);
        Assert.Contains("Message", contents);
        Assert.Contains("[123]", contents);
    }

    [Fact]
    public void BeginScope_ReturnsDisposable()
    {
        // Arrange
        using var provider = new FileLoggerProvider(_tempFilePath);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        var scope = logger.BeginScope("test scope");

        // Assert
        Assert.NotNull(scope);
        Assert.IsAssignableFrom<IDisposable>(scope);
        scope.Dispose(); // Should not throw
    }

    [Fact]
    public void IsEnabled_ReturnsTrueForGreaterOrEqualLevel()
    {
        // Arrange
        using var provider = new FileLoggerProvider(_tempFilePath, LogLevel.Warning);
        var logger = provider.CreateLogger("TestCategory");

        // Act & Assert
        Assert.True(logger.IsEnabled(LogLevel.Warning));
        Assert.True(logger.IsEnabled(LogLevel.Error));
        Assert.False(logger.IsEnabled(LogLevel.Information));
    }

    [Fact]
    public void Dispose_ClosesFileHandle()
    {
        // Arrange
        var provider = new FileLoggerProvider(_tempFilePath);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Before dispose");
        provider.Dispose();

        // Assert - should be able to delete file after dispose
        try
        {
            File.Delete(_tempFilePath);
        }
        catch
        {
            Assert.Fail("File should be deletable after provider is disposed");
        }
    }

    [Fact]
    public void Logger_PreservesExistingLogContent()
    {
        // Arrange
        File.WriteAllText(_tempFilePath, "Existing content\n");

        // Act
        using var provider = new FileLoggerProvider(_tempFilePath);
        var logger = provider.CreateLogger("TestCategory");
        logger.LogInformation("New message");

        // Assert
        var contents = File.ReadAllText(_tempFilePath);
        Assert.Contains("Existing content", contents);
        Assert.Contains("New message", contents);
    }

    [Fact]
    public void Logger_IncludesTimestamp()
    {
        // Arrange
        using var provider = new FileLoggerProvider(_tempFilePath);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Test message");

        // Assert
        var contents = File.ReadAllText(_tempFilePath);
        // Check for ISO format timestamp: YYYY-MM-DD HH:MM:SS.SSSZ
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}Z", contents);
    }

    [Fact]
    public void MultipleLoggers_ShareSameProvider()
    {
        // Arrange
        using var provider = new FileLoggerProvider(_tempFilePath);
        var logger1 = provider.CreateLogger("Category1");
        var logger2 = provider.CreateLogger("Category2");

        // Act
        logger1.LogInformation("Message from logger1");
        logger2.LogInformation("Message from logger2");

        // Assert
        var contents = File.ReadAllText(_tempFilePath);
        Assert.Contains("Message from logger1", contents);
        Assert.Contains("Message from logger2", contents);
        Assert.Contains("Category1", contents);
        Assert.Contains("Category2", contents);
    }

    [Theory]
    [InlineData(LogLevel.Debug, "DEBUG")]
    [InlineData(LogLevel.Information, "INFO")]
    [InlineData(LogLevel.Warning, "WARN")]
    [InlineData(LogLevel.Error, "ERROR")]
    [InlineData(LogLevel.Critical, "CRIT")]
    public void Logger_WritesCorrectLogLevel(LogLevel logLevel, string expectedLabel)
    {
        // Arrange
        using var provider = new FileLoggerProvider(_tempFilePath, LogLevel.Debug);
        var logger = provider.CreateLogger("TestCategory");

        // Act
        logger.Log(logLevel, "Test message");

        // Assert
        var contents = File.ReadAllText(_tempFilePath);
        Assert.Contains(expectedLabel, contents);
    }
}
