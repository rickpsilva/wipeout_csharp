using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Infrastructure.Database;

namespace WipeoutRewrite.Tests.Core.Services;

public class BestTimesManagerTests
{
    private BestTimesManager CreateManager()
    {
        var logger = new TestLoggerFactory().CreateLogger<BestTimesManager>() as ILogger<BestTimesManager> ?? new NullLogger<BestTimesManager>();
        var repository = new Mock<ISettingsRepository>().Object;
        return new BestTimesManager(logger, repository);
    }

    private BestTimeRecord CreateTestRecord(string pilotName = "Test Pilot", string teamName = "Test Team", 
        string circuitName = "Track 01", string racingClass = "Venom", long timeMs = 60000, DateTime? date = null)
    {
        return new BestTimeRecord
        {
            PilotName = pilotName,
            TeamName = teamName,
            CircuitName = circuitName,
            RacingClass = racingClass,
            TimeMilliseconds = timeMs,
            RecordDate = date ?? DateTime.Now
        };
    }

    [Fact]
    public void AddOrUpdateRecord_WithNewRecord_AddsRecord()
    {
        var manager = CreateManager();
        var record = CreateTestRecord();

        manager.AddOrUpdateRecord(record);

        Assert.Single(manager.GetAllRecords());
    }

    [Fact]
    public void GetRecordCount_WithMultipleRecords_ReturnsCorrectCount()
    {
        var manager = CreateManager();
        var date = DateTime.Now;

        manager.AddOrUpdateRecord(CreateTestRecord("Pilot A", "Team A", "Track 01", timeMs: 60000, date: date));
        manager.AddOrUpdateRecord(CreateTestRecord("Pilot B", "Team B", "Track 02", timeMs: 65000, date: date));

        Assert.Equal(2, manager.GetRecordCount());
    }

    [Fact]
    public void AddOrUpdateRecord_WithFasterTime_UpdatesRecord()
    {
        var manager = CreateManager();
        var date = DateTime.Now;

        manager.AddOrUpdateRecord(CreateTestRecord(timeMs: 60000, date: date));
        manager.AddOrUpdateRecord(CreateTestRecord(timeMs: 55000, date: date.AddMinutes(1)));

        var bestTime = manager.GetBestTime("Track 01", "Venom");
        Assert.Equal(55000, bestTime?.TimeMilliseconds);
    }

    [Fact]
    public void AddOrUpdateRecord_WithSlowerTime_IgnoresRecord()
    {
        var manager = CreateManager();
        var date = DateTime.Now;

        manager.AddOrUpdateRecord(CreateTestRecord(timeMs: 60000, date: date));
        manager.AddOrUpdateRecord(CreateTestRecord(timeMs: 65000, date: date.AddMinutes(1)));

        Assert.Single(manager.GetAllRecords());
        var bestTime = manager.GetBestTime("Track 01", "Venom");
        Assert.Equal(60000, bestTime?.TimeMilliseconds);
    }

    [Fact]
    public void GetRecordsForCircuit_ReturnsOnlyCircuitRecords()
    {
        var manager = CreateManager();
        var date = DateTime.Now;

        manager.AddOrUpdateRecord(CreateTestRecord("Pilot A", "Team A", "Track 01", timeMs: 60000, date: date));
        manager.AddOrUpdateRecord(CreateTestRecord("Pilot B", "Team B", "Track 02", timeMs: 65000, date: date));

        var track01Records = manager.GetRecordsForCircuit("Track 01");

        Assert.Single(track01Records);
        Assert.All(track01Records, r => Assert.Equal("Track 01", r.CircuitName));
    }

    [Fact]
    public void GetBestTime_WithNonExistentRecord_ReturnsNull()
    {
        var manager = CreateManager();

        var result = manager.GetBestTime("Nonexistent", "Venom");

        Assert.Null(result);
    }

    [Fact]
    public void ClearAllRecords_RemovesAllRecords()
    {
        var manager = CreateManager();

        manager.AddOrUpdateRecord(CreateTestRecord());
        manager.ClearAllRecords();

        Assert.Empty(manager.GetAllRecords());
    }

    [Fact]
    public void BestTimeRecord_FormatTime_ReturnsCorrectFormat()
    {
        var record = CreateTestRecord(timeMs: 125500);
        var formatted = record.FormatTime();
        Assert.Equal("02:05.5", formatted);
    }

    [Fact]
    public void BestTimeRecord_FormatTime_WithSmallValue_ReturnsCorrectFormat()
    {
        var record = CreateTestRecord(timeMs: 5500);
        var formatted = record.FormatTime();
        Assert.Equal("00:05.5", formatted);
    }

    [Fact]
    public void AddOrUpdateRecord_WithNullRecord_ReturnsFalse()
    {
        var manager = CreateManager();

        var result = manager.AddOrUpdateRecord(null!);

        Assert.False(result);
        Assert.Empty(manager.GetAllRecords());
    }

    [Fact]
    public void GetRecordsForCircuit_WithEmptyString_ReturnsEmpty()
    {
        var manager = CreateManager();

        manager.AddOrUpdateRecord(CreateTestRecord());
        var result = manager.GetRecordsForCircuit("");

        Assert.Empty(result);
    }

    [Fact]
    public void GetRecordsForCircuit_ReturnsSortedByTime()
    {
        var manager = CreateManager();
        var date = DateTime.Now;

        manager.AddOrUpdateRecord(CreateTestRecord(timeMs: 70000, date: date, racingClass: "Venom"));
        manager.AddOrUpdateRecord(CreateTestRecord(timeMs: 50000, date: date, racingClass: "Rapier"));
        manager.AddOrUpdateRecord(CreateTestRecord(timeMs: 60000, date: date, racingClass: "Piranha"));

        var records = manager.GetRecordsForCircuit("Track 01");

        Assert.Equal(3, records.Count);
        Assert.Equal(50000, records[0].TimeMilliseconds);
        Assert.Equal(60000, records[1].TimeMilliseconds);
        Assert.Equal(70000, records[2].TimeMilliseconds);
    }

    [Fact]
    public void GetBestTime_WithNullParameters_ReturnsNull()
    {
        var manager = CreateManager();

        Assert.Null(manager.GetBestTime(null!, "Venom"));
        Assert.Null(manager.GetBestTime("Track 01", null!));
        Assert.Null(manager.GetBestTime("", "Venom"));
    }

    [Fact]
    public void GetBestTime_WithMultipleRecordsForCircuit_ReturnsFastest()
    {
        var manager = CreateManager();
        var date = DateTime.Now;

        manager.AddOrUpdateRecord(CreateTestRecord(timeMs: 70000, date: date));
        manager.AddOrUpdateRecord(CreateTestRecord(timeMs: 50000, date: date));

        var result = manager.AddOrUpdateRecord(CreateTestRecord(timeMs: 90000, date: date));

        Assert.False(result);
        var best = manager.GetBestTime("Track 01", "Venom");
        Assert.Equal(50000, best?.TimeMilliseconds);
    }

    [Fact]
    public void GetRecordsForCircuit_WithMultipleDifferentClasses_ReturnsAll()
    {
        var manager = CreateManager();
        var date = DateTime.Now;

        manager.AddOrUpdateRecord(CreateTestRecord("Pilot A", "Team A", racingClass: "Venom", timeMs: 60000, date: date));
        manager.AddOrUpdateRecord(CreateTestRecord("Pilot B", "Team B", racingClass: "Rapier", timeMs: 50000, date: date));

        var track01Records = manager.GetRecordsForCircuit("Track 01");

        Assert.Equal(2, track01Records.Count);
    }

    [Fact]
    public void AddOrUpdateRecord_WithSameTime_IgnoresRecord()
    {
        var manager = CreateManager();
        var date = DateTime.Now;

        manager.AddOrUpdateRecord(CreateTestRecord(timeMs: 60000, date: date));
        var result = manager.AddOrUpdateRecord(CreateTestRecord("Another Pilot", "Another Team", timeMs: 60000, date: date.AddMinutes(1)));

        Assert.False(result);
        Assert.Single(manager.GetAllRecords());
    }

    [Fact]
    public void GetAllRecords_ReturnsReadOnlyList()
    {
        var manager = CreateManager();

        manager.AddOrUpdateRecord(CreateTestRecord());
        var records = manager.GetAllRecords();

        Assert.IsAssignableFrom<IReadOnlyList<BestTimeRecord>>(records);
        Assert.Single(records);
    }

    [Fact]
    public void ClearAllRecords_ResetsRecordCount()
    {
        var manager = CreateManager();

        manager.AddOrUpdateRecord(CreateTestRecord());
        Assert.Equal(1, manager.GetRecordCount());

        manager.ClearAllRecords();

        Assert.Equal(0, manager.GetRecordCount());
    }

    [Fact]
    public void GetRecordsForCircuit_ReturnsDifferentResultsForDifferentCircuits()
    {
        var manager = CreateManager();
        var date = DateTime.Now;

        manager.AddOrUpdateRecord(CreateTestRecord(circuitName: "Track 01", timeMs: 60000, date: date));
        manager.AddOrUpdateRecord(CreateTestRecord(circuitName: "Track 02", timeMs: 65000, date: date));
        manager.AddOrUpdateRecord(CreateTestRecord(circuitName: "Track 03", timeMs: 70000, date: date));

        var track01 = manager.GetRecordsForCircuit("Track 01");
        var track02 = manager.GetRecordsForCircuit("Track 02");
        var track03 = manager.GetRecordsForCircuit("Track 03");

        Assert.Single(track01);
        Assert.Single(track02);
        Assert.Single(track03);
    }

    [Fact]
    public void AddOrUpdateRecord_ReturnsTrue_WhenSuccessful()
    {
        var manager = CreateManager();

        var result = manager.AddOrUpdateRecord(CreateTestRecord());

        Assert.True(result);
    }
}

// Helper NullLogger for tests
public class NullLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
