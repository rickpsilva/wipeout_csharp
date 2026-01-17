using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using WipeoutRewrite.Infrastructure.Database;
using WipeoutRewrite.Infrastructure.Database.Entities;

namespace WipeoutRewrite.Tests.Infrastructure.Database;

public class SettingsRepositoryTests
{
    private static GameSettingsDbContext CreateMockContext()
    {
        var options = new DbContextOptionsBuilder<GameSettingsDbContext>()
            .UseInMemoryDatabase(databaseName: $"test_{Guid.NewGuid()}")
            .Options;

        return new GameSettingsDbContext(options);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SettingsRepository(null!));
    }

    [Fact]
    public void LoadControlsSettings_ReturnsDefaultSettings_WhenExists()
    {
        // Arrange
        var context = CreateMockContext();
        var defaultSettings = new ControlsSettingsEntity { Id = 1 };
        context.ControlsSettings.Add(defaultSettings);
        context.SaveChanges();
        var repository = new SettingsRepository(context);

        // Act
        var result = repository.LoadControlsSettings();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void LoadControlsSettings_ThrowsInvalidOperationException_WhenNotFound()
    {
        // Arrange
        var context = CreateMockContext();
        var repository = new SettingsRepository(context);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => repository.LoadControlsSettings());
    }

    [Fact]
    public void SaveControlsSettings_AddsNewSettings()
    {
        // Arrange
        var context = CreateMockContext();
        var repository = new SettingsRepository(context);
        var settings = new ControlsSettingsEntity();

        // Act
        repository.SaveControlsSettings(settings);
        repository.SaveChanges();

        // Assert
        var saved = context.ControlsSettings.FirstOrDefault(s => s.Id == 1);
        Assert.NotNull(saved);
        Assert.Equal(1, saved.Id);
    }

    [Fact]
    public void SaveControlsSettings_UpdatesExistingSettings()
    {
        // Arrange
        var context = CreateMockContext();
        var existing = new ControlsSettingsEntity { Id = 1, LastModified = DateTime.UtcNow.AddDays(-1) };
        context.ControlsSettings.Add(existing);
        context.SaveChanges();

        var repository = new SettingsRepository(context);
        var newSettings = new ControlsSettingsEntity();

        // Act
        repository.SaveControlsSettings(newSettings);
        repository.SaveChanges();

        // Assert
        var all = context.ControlsSettings.Where(s => s.Id == 1).ToList();
        Assert.Single(all);
        Assert.True(all[0].LastModified > existing.LastModified);
    }

    [Fact]
    public void LoadVideoSettings_ReturnsDefaultSettings_WhenExists()
    {
        // Arrange
        var context = CreateMockContext();
        var defaultSettings = new VideoSettingsEntity { Id = 1 };
        context.VideoSettings.Add(defaultSettings);
        context.SaveChanges();
        var repository = new SettingsRepository(context);

        // Act
        var result = repository.LoadVideoSettings();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void LoadVideoSettings_ThrowsInvalidOperationException_WhenNotFound()
    {
        // Arrange
        var context = CreateMockContext();
        var repository = new SettingsRepository(context);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => repository.LoadVideoSettings());
    }

    [Fact]
    public void SaveVideoSettings_AddsNewSettings()
    {
        // Arrange
        var context = CreateMockContext();
        var repository = new SettingsRepository(context);
        var settings = new VideoSettingsEntity();

        // Act
        repository.SaveVideoSettings(settings);
        repository.SaveChanges();

        // Assert
        var saved = context.VideoSettings.FirstOrDefault(s => s.Id == 1);
        Assert.NotNull(saved);
        Assert.Equal(1, saved.Id);
    }

    [Fact]
    public void SaveVideoSettings_UpdatesExistingSettings()
    {
        // Arrange
        var context = CreateMockContext();
        var existing = new VideoSettingsEntity { Id = 1, LastModified = DateTime.UtcNow.AddDays(-1) };
        context.VideoSettings.Add(existing);
        context.SaveChanges();

        var repository = new SettingsRepository(context);
        var newSettings = new VideoSettingsEntity();

        // Act
        repository.SaveVideoSettings(newSettings);
        repository.SaveChanges();

        // Assert
        var all = context.VideoSettings.Where(s => s.Id == 1).ToList();
        Assert.Single(all);
        Assert.True(all[0].LastModified > existing.LastModified);
    }

    [Fact]
    public void LoadAudioSettings_ReturnsDefaultSettings_WhenExists()
    {
        // Arrange
        var context = CreateMockContext();
        var defaultSettings = new AudioSettingsEntity { Id = 1 };
        context.AudioSettings.Add(defaultSettings);
        context.SaveChanges();
        var repository = new SettingsRepository(context);

        // Act
        var result = repository.LoadAudioSettings();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void LoadAudioSettings_ThrowsInvalidOperationException_WhenNotFound()
    {
        // Arrange
        var context = CreateMockContext();
        var repository = new SettingsRepository(context);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => repository.LoadAudioSettings());
    }

    [Fact]
    public void SaveAudioSettings_AddsNewSettings()
    {
        // Arrange
        var context = CreateMockContext();
        var repository = new SettingsRepository(context);
        var settings = new AudioSettingsEntity();

        // Act
        repository.SaveAudioSettings(settings);
        repository.SaveChanges();

        // Assert
        var saved = context.AudioSettings.FirstOrDefault(s => s.Id == 1);
        Assert.NotNull(saved);
        Assert.Equal(1, saved.Id);
    }

    [Fact]
    public void SaveAudioSettings_UpdatesExistingSettings()
    {
        // Arrange
        var context = CreateMockContext();
        var existing = new AudioSettingsEntity { Id = 1, LastModified = DateTime.UtcNow.AddDays(-1) };
        context.AudioSettings.Add(existing);
        context.SaveChanges();

        var repository = new SettingsRepository(context);
        var newSettings = new AudioSettingsEntity();

        // Act
        repository.SaveAudioSettings(newSettings);
        repository.SaveChanges();

        // Assert
        var all = context.AudioSettings.Where(s => s.Id == 1).ToList();
        Assert.Single(all);
        Assert.True(all[0].LastModified > existing.LastModified);
    }

    [Fact]
    public void GetAllBestTimes_ReturnsAllTimes_SortedByTimeMilliseconds()
    {
        // Arrange
        var context = CreateMockContext();
        context.BestTimes.AddRange(
            new BestTimeEntity { CircuitName = "Track1", TimeMilliseconds = 120000 },
            new BestTimeEntity { CircuitName = "Track2", TimeMilliseconds = 100000 },
            new BestTimeEntity { CircuitName = "Track3", TimeMilliseconds = 110000 }
        );
        context.SaveChanges();
        var repository = new SettingsRepository(context);

        // Act
        var result = repository.GetAllBestTimes();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(100000, result[0].TimeMilliseconds);
        Assert.Equal(110000, result[1].TimeMilliseconds);
        Assert.Equal(120000, result[2].TimeMilliseconds);
    }

    [Fact]
    public void GetBestTimesForCircuit_ReturnsOnlyMatching_SortedByTime()
    {
        // Arrange
        var context = CreateMockContext();
        context.BestTimes.AddRange(
            new BestTimeEntity { CircuitName = "Track1", TimeMilliseconds = 120000 },
            new BestTimeEntity { CircuitName = "Track1", TimeMilliseconds = 100000 },
            new BestTimeEntity { CircuitName = "Track2", TimeMilliseconds = 90000 }
        );
        context.SaveChanges();
        var repository = new SettingsRepository(context);

        // Act
        var result = repository.GetBestTimesForCircuit("Track1");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal("Track1", t.CircuitName));
        Assert.Equal(100000, result[0].TimeMilliseconds);
        Assert.Equal(120000, result[1].TimeMilliseconds);
    }

    [Fact]
    public void GetBestTimesForCircuit_ReturnsEmpty_WhenNoMatch()
    {
        // Arrange
        var context = CreateMockContext();
        context.BestTimes.Add(new BestTimeEntity { CircuitName = "Track1", TimeMilliseconds = 100000 });
        context.SaveChanges();
        var repository = new SettingsRepository(context);

        // Act
        var result = repository.GetBestTimesForCircuit("NonExistent");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetBestTime_ReturnsFastestTime_ForCircuitAndClass()
    {
        // Arrange
        var context = CreateMockContext();
        context.BestTimes.AddRange(
            new BestTimeEntity { CircuitName = "Track1", RacingClass = "AG", TimeMilliseconds = 120000 },
            new BestTimeEntity { CircuitName = "Track1", RacingClass = "AG", TimeMilliseconds = 100000 },
            new BestTimeEntity { CircuitName = "Track1", RacingClass = "VEL", TimeMilliseconds = 90000 }
        );
        context.SaveChanges();
        var repository = new SettingsRepository(context);

        // Act
        var result = repository.GetBestTime("Track1", "AG");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100000, result.TimeMilliseconds);
        Assert.Equal("Track1", result.CircuitName);
        Assert.Equal("AG", result.RacingClass);
    }

    [Fact]
    public void GetBestTime_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var context = CreateMockContext();
        var repository = new SettingsRepository(context);

        // Act
        var result = repository.GetBestTime("NonExistent", "NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void AddOrUpdateBestTime_AddsNew_WhenNotExists()
    {
        // Arrange
        var context = CreateMockContext();
        var repository = new SettingsRepository(context);
        var newTime = new BestTimeEntity { CircuitName = "Track1", RacingClass = "AG", PilotName = "Player", TimeMilliseconds = 100000 };

        // Act
        repository.AddOrUpdateBestTime(newTime);
        repository.SaveChanges();

        // Assert
        var saved = context.BestTimes.FirstOrDefault(t => t.CircuitName == "Track1");
        Assert.NotNull(saved);
        Assert.Equal(100000, saved.TimeMilliseconds);
    }

    [Fact]
    public void AddOrUpdateBestTime_UpdatesExisting_WhenFasterTime()
    {
        // Arrange
        var context = CreateMockContext();
        var existing = new BestTimeEntity { CircuitName = "Track1", RacingClass = "AG", PilotName = "Player", TimeMilliseconds = 120000 };
        context.BestTimes.Add(existing);
        context.SaveChanges();

        var repository = new SettingsRepository(context);
        var fasterTime = new BestTimeEntity { CircuitName = "Track1", RacingClass = "AG", PilotName = "Player", TimeMilliseconds = 100000, Team = "NewTeam" };

        // Act
        repository.AddOrUpdateBestTime(fasterTime);
        repository.SaveChanges();

        // Assert
        var updated = context.BestTimes.FirstOrDefault(t => t.CircuitName == "Track1");
        Assert.NotNull(updated);
        Assert.Equal(100000, updated.TimeMilliseconds);
        Assert.Equal("NewTeam", updated.Team);
    }

    [Fact]
    public void AddOrUpdateBestTime_IgnoresSlowerTime()
    {
        // Arrange
        var context = CreateMockContext();
        var existing = new BestTimeEntity { CircuitName = "Track1", RacingClass = "AG", PilotName = "Player", TimeMilliseconds = 100000 };
        context.BestTimes.Add(existing);
        context.SaveChanges();

        var repository = new SettingsRepository(context);
        var slowerTime = new BestTimeEntity { CircuitName = "Track1", RacingClass = "AG", PilotName = "Player", TimeMilliseconds = 120000 };

        // Act
        repository.AddOrUpdateBestTime(slowerTime);
        repository.SaveChanges();

        // Assert
        var result = context.BestTimes.FirstOrDefault(t => t.CircuitName == "Track1");
        Assert.NotNull(result);
        Assert.Equal(100000, result.TimeMilliseconds); // Should remain unchanged
    }

    [Fact]
    public void SaveChanges_SavesContextChanges()
    {
        // Arrange
        var context = CreateMockContext();
        var repository = new SettingsRepository(context);
        var settings = new ControlsSettingsEntity();

        // Act
        repository.SaveControlsSettings(settings);
        repository.SaveChanges();

        // Assert
        var saved = context.ControlsSettings.FirstOrDefault(s => s.Id == 1);
        Assert.NotNull(saved);
    }
}
