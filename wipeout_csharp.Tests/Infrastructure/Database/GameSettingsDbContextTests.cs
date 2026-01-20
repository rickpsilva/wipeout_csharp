using Xunit;
using Microsoft.EntityFrameworkCore;
using WipeoutRewrite.Infrastructure.Database;
using WipeoutRewrite.Infrastructure.Database.Entities;

namespace WipeoutRewrite.Tests.Infrastructure.Database;

public class GameSettingsDbContextTests : IDisposable
{
    private readonly string _tempDbPath;

    public GameSettingsDbContextTests()
    {
        _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
    }

    public void Dispose()
    {
        if (File.Exists(_tempDbPath))
            File.Delete(_tempDbPath);
    }

    private GameSettingsDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GameSettingsDbContext>()
            .UseInMemoryDatabase(databaseName: $"test_{Guid.NewGuid()}")
            .Options;

        return new GameSettingsDbContext(options);
    }

    [Fact]
    public void Constructor_WithValidOptions_CreatesContext()
    {
        // Arrange & Act
        var context = CreateInMemoryContext();

        // Assert
        Assert.NotNull(context);
    }

    [Fact]
    public void DbSet_ControlsSettings_IsNotNull()
    {
        // Arrange & Act
        var context = CreateInMemoryContext();

        // Assert
        Assert.NotNull(context.ControlsSettings);
    }

    [Fact]
    public void DbSet_VideoSettings_IsNotNull()
    {
        // Arrange & Act
        var context = CreateInMemoryContext();

        // Assert
        Assert.NotNull(context.VideoSettings);
    }

    [Fact]
    public void DbSet_AudioSettings_IsNotNull()
    {
        // Arrange & Act
        var context = CreateInMemoryContext();

        // Assert
        Assert.NotNull(context.AudioSettings);
    }

    [Fact]
    public void DbSet_BestTimes_IsNotNull()
    {
        // Arrange & Act
        var context = CreateInMemoryContext();

        // Assert
        Assert.NotNull(context.BestTimes);
    }

    [Fact]
    public void CanInsert_ControlsSettings()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controls = new ControlsSettingsEntity { Id = 1 };

        // Act
        context.ControlsSettings.Add(controls);
        context.SaveChanges();

        // Assert
        var retrieved = context.ControlsSettings.FirstOrDefault(s => s.Id == 1);
        Assert.NotNull(retrieved);
    }

    [Fact]
    public void CanInsert_VideoSettings()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var video = new VideoSettingsEntity { Id = 1 };

        // Act
        context.VideoSettings.Add(video);
        context.SaveChanges();

        // Assert
        var retrieved = context.VideoSettings.FirstOrDefault(s => s.Id == 1);
        Assert.NotNull(retrieved);
    }

    [Fact]
    public void CanInsert_AudioSettings()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var audio = new AudioSettingsEntity { Id = 1 };

        // Act
        context.AudioSettings.Add(audio);
        context.SaveChanges();

        // Assert
        var retrieved = context.AudioSettings.FirstOrDefault(s => s.Id == 1);
        Assert.NotNull(retrieved);
    }

    [Fact]
    public void CanInsert_BestTimeEntity()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var bestTime = new BestTimeEntity
        {
            CircuitName = "Track1",
            RacingClass = "AG",
            PilotName = "Player",
            TimeMilliseconds = 100000
        };

        // Act
        context.BestTimes.Add(bestTime);
        context.SaveChanges();

        // Assert
        var retrieved = context.BestTimes.FirstOrDefault(b => b.CircuitName == "Track1");
        Assert.NotNull(retrieved);
    }

    [Fact]
    public void CanUpdate_ControlsSettings()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var oldTime = DateTime.UtcNow.AddDays(-1);
        var controls = new ControlsSettingsEntity { Id = 1, LastModified = oldTime };
        context.ControlsSettings.Add(controls);
        context.SaveChanges();

        // Act
        var retrieved = context.ControlsSettings.First(s => s.Id == 1);
        retrieved.LastModified = DateTime.UtcNow;
        context.SaveChanges();

        // Assert
        var updated = context.ControlsSettings.First(s => s.Id == 1);
        Assert.True(updated.LastModified > oldTime);
    }

    [Fact]
    public void CanDelete_ControlsSettings()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controls = new ControlsSettingsEntity { Id = 1 };
        context.ControlsSettings.Add(controls);
        context.SaveChanges();

        // Act
        var retrieved = context.ControlsSettings.First(s => s.Id == 1);
        context.ControlsSettings.Remove(retrieved);
        context.SaveChanges();

        // Assert
        var deleted = context.ControlsSettings.FirstOrDefault(s => s.Id == 1);
        Assert.Null(deleted);
    }

    [Fact]
    public void BestTimes_OrderByTimeMilliseconds()
    {
        // Arrange
        var context = CreateInMemoryContext();
        context.BestTimes.AddRange(
            new BestTimeEntity { CircuitName = "Track1", TimeMilliseconds = 120000 },
            new BestTimeEntity { CircuitName = "Track2", TimeMilliseconds = 100000 },
            new BestTimeEntity { CircuitName = "Track3", TimeMilliseconds = 110000 }
        );
        context.SaveChanges();

        // Act
        var ordered = context.BestTimes.OrderBy(b => b.TimeMilliseconds).ToList();

        // Assert
        Assert.Equal(3, ordered.Count);
        Assert.Equal(100000, ordered[0].TimeMilliseconds);
        Assert.Equal(110000, ordered[1].TimeMilliseconds);
        Assert.Equal(120000, ordered[2].TimeMilliseconds);
    }

    [Fact]
    public void BestTimes_FilterByCircuitName()
    {
        // Arrange
        var context = CreateInMemoryContext();
        context.BestTimes.AddRange(
            new BestTimeEntity { CircuitName = "Track1", TimeMilliseconds = 100000 },
            new BestTimeEntity { CircuitName = "Track2", TimeMilliseconds = 110000 },
            new BestTimeEntity { CircuitName = "Track1", TimeMilliseconds = 105000 }
        );
        context.SaveChanges();

        // Act
        var filtered = context.BestTimes.Where(b => b.CircuitName == "Track1").ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, b => Assert.Equal("Track1", b.CircuitName));
    }

    [Fact]
    public void BestTimes_FilterByRacingClass()
    {
        // Arrange
        var context = CreateInMemoryContext();
        context.BestTimes.AddRange(
            new BestTimeEntity { CircuitName = "Track1", RacingClass = "AG" },
            new BestTimeEntity { CircuitName = "Track2", RacingClass = "VEL" },
            new BestTimeEntity { CircuitName = "Track3", RacingClass = "AG" }
        );
        context.SaveChanges();

        // Act
        var filtered = context.BestTimes.Where(b => b.RacingClass == "AG").ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, b => Assert.Equal("AG", b.RacingClass));
    }

    [Fact]
    public void BestTimes_CompositeFilter_CircuitAndClass()
    {
        // Arrange
        var context = CreateInMemoryContext();
        context.BestTimes.AddRange(
            new BestTimeEntity { CircuitName = "Track1", RacingClass = "AG", TimeMilliseconds = 100000 },
            new BestTimeEntity { CircuitName = "Track1", RacingClass = "VEL", TimeMilliseconds = 90000 },
            new BestTimeEntity { CircuitName = "Track2", RacingClass = "AG", TimeMilliseconds = 95000 }
        );
        context.SaveChanges();

        // Act
        var filtered = context.BestTimes
            .Where(b => b.CircuitName == "Track1" && b.RacingClass == "AG")
            .ToList();

        // Assert
        Assert.Single(filtered);
        Assert.Equal(100000, filtered[0].TimeMilliseconds);
    }

    [Fact]
    public void MultipleBestTimes_SameCircuitClass_CanCoexist()
    {
        // Arrange
        var context = CreateInMemoryContext();
        context.BestTimes.AddRange(
            new BestTimeEntity { CircuitName = "Track1", RacingClass = "AG", PilotName = "Player1", TimeMilliseconds = 100000 },
            new BestTimeEntity { CircuitName = "Track1", RacingClass = "AG", PilotName = "Player2", TimeMilliseconds = 95000 }
        );
        context.SaveChanges();

        // Act
        var results = context.BestTimes.Where(b => b.CircuitName == "Track1" && b.RacingClass == "AG").ToList();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void Create_ReturnsContextWithSqliteOptions()
    {
        // Arrange & Act
        var context = GameSettingsDbContext.Create(_tempDbPath);

        // Assert
        Assert.NotNull(context);
        context.Dispose();
    }

    [Fact]
    public void Create_CreatesDataDirectory()
    {
        // Arrange
        var customPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}", "subdir", "test.db");

        try
        {
            // Act
            var context = GameSettingsDbContext.Create(customPath);
            context.Dispose();

            // Assert
            var directory = Path.GetDirectoryName(customPath);
            Assert.NotNull(directory);
            Assert.True(Directory.Exists(directory));
        }
        finally
        {
            var directory = Path.GetDirectoryName(customPath);
            if (directory != null && Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void DefaultDatabaseName_IsCorrect()
    {
        // Assert
        Assert.Equal("wipeout_settings.db", GameSettingsDbContext.DefaultDatabaseName);
    }
}
