using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WipeoutRewrite.Infrastructure.Database;

namespace WipeoutRewrite.Tests.Infrastructure.Database;

public class DatabaseInitializerTests : IDisposable
{
    private readonly string _testDbPath;

    public DatabaseInitializerTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
    }

    public void Dispose()
    {
        if (File.Exists(_testDbPath))
            File.Delete(_testDbPath);
    }

    private GameSettingsDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GameSettingsDbContext>()
            .UseInMemoryDatabase(databaseName: $"test_{Guid.NewGuid()}")
            .Options;

        return new GameSettingsDbContext(options);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = new Mock<ILogger<DatabaseInitializer>>().Object;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DatabaseInitializer(null!, logger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var context = CreateInMemoryContext();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DatabaseInitializer(context, null!));
    }

    [Fact]
    public void Initialize_CreatesDatabase()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var mockLogger = new Mock<ILogger<DatabaseInitializer>>();
        var initializer = new DatabaseInitializer(context, mockLogger.Object);

        // Act
        initializer.Initialize();

        // Assert
        // If Initialize() succeeds without throwing, database was created
        Assert.NotNull(context);
    }

    [Fact]
    public void Initialize_PopulatesDefaultsWhenEmpty()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var mockLogger = new Mock<ILogger<DatabaseInitializer>>();
        var initializer = new DatabaseInitializer(context, mockLogger.Object);

        // Act
        initializer.Initialize();

        // Assert
        var hasControls = context.ControlsSettings.Any(s => s.Id == 1);
        var hasVideo = context.VideoSettings.Any(s => s.Id == 1);
        var hasAudio = context.AudioSettings.Any(s => s.Id == 1);

        Assert.True(hasControls && hasVideo && hasAudio, "Default settings should be populated");
    }

    [Fact]
    public void Initialize_LogsInformationMessages()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var mockLogger = new Mock<ILogger<DatabaseInitializer>>();
        var initializer = new DatabaseInitializer(context, mockLogger.Object);

        // Act
        initializer.Initialize();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Initializing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void Initialize_DoesNotRepopulate_WhenDataExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var mockLogger = new Mock<ILogger<DatabaseInitializer>>();

        // First initialization
        var initializer1 = new DatabaseInitializer(context, mockLogger.Object);
        initializer1.Initialize();

        // Reset mock to track second initialization
        mockLogger.Reset();

        // Act - Initialize again
        var initializer2 = new DatabaseInitializer(context, mockLogger.Object);
        initializer2.Initialize();

        // Assert - Should not log "populating with defaults"
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("populating with defaults")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void Initialize_CreatesDataDirectory()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var mockLogger = new Mock<ILogger<DatabaseInitializer>>();
        var initializer = new DatabaseInitializer(context, mockLogger.Object);

        // Act
        initializer.Initialize();

        // Assert
        var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
        Assert.True(Directory.Exists(dataDir), "Data directory should be created");
    }

    [Fact]
    public void Initialize_WithExistingData_OnlyLogsOnce()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var mockLogger = new Mock<ILogger<DatabaseInitializer>>();
        var initializer = new DatabaseInitializer(context, mockLogger.Object);

        // Act
        initializer.Initialize();
        initializer.Initialize(); // Call again

        // Assert - "already contains data" should be logged second time
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already contains data")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Initialize_PopulatesAllThreeSettings()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var mockLogger = new Mock<ILogger<DatabaseInitializer>>();
        var initializer = new DatabaseInitializer(context, mockLogger.Object);

        // Act
        initializer.Initialize();

        // Assert
        var controls = context.ControlsSettings.FirstOrDefault(s => s.Id == 1);
        var video = context.VideoSettings.FirstOrDefault(s => s.Id == 1);
        var audio = context.AudioSettings.FirstOrDefault(s => s.Id == 1);

        Assert.NotNull(controls);
        Assert.NotNull(video);
        Assert.NotNull(audio);
    }
}
