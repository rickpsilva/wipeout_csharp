using Xunit;
using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Tests.Core.Services;

public class BestTimeRecordTests
{
    [Fact]
    public void FormatTime_WithZeroSeconds_ReturnsZeroFormatted()
    {
        var record = new BestTimeRecord
        {
            PilotName = "Test",
            TeamName = "Test",
            CircuitName = "Track01",
            RacingClass = "Venom",
            TimeMilliseconds = 0
        };

        var formatted = record.FormatTime();

        Assert.Equal("00:00.0", formatted);
    }

    [Fact]
    public void FormatTime_WithSeconds_ReturnsCorrectFormat()
    {
        var record = new BestTimeRecord
        {
            PilotName = "Test",
            TeamName = "Test",
            CircuitName = "Track01",
            RacingClass = "Venom",
            TimeMilliseconds = 61234  // 1 minute 1.234 seconds
        };

        var formatted = record.FormatTime();

        Assert.Equal("01:01.2", formatted);
    }

    [Fact]
    public void FormatTime_WithMinutesAndSeconds_ReturnsCorrectFormat()
    {
        var record = new BestTimeRecord
        {
            PilotName = "Test",
            TeamName = "Test",
            CircuitName = "Track01",
            RacingClass = "Venom",
            TimeMilliseconds = 255789  // 4 minutes 15.789 seconds
        };

        var formatted = record.FormatTime();

        Assert.Equal("04:15.7", formatted);
    }

    [Theory]
    [InlineData(100, "00:00.1")]      // 0.1 seconds
    [InlineData(500, "00:00.5")]      // 0.5 seconds
    [InlineData(1000, "00:01.0")]     // 1 second
    [InlineData(5999, "00:05.9")]     // 5.999 seconds
    [InlineData(60000, "01:00.0")]    // 1 minute
    [InlineData(121500, "02:01.5")]   // 2 minutes 1.5 seconds
    public void FormatTime_WithVariousMilliseconds_ReturnsExpected(long ms, string expected)
    {
        var record = new BestTimeRecord
        {
            PilotName = "Test",
            TeamName = "Test",
            CircuitName = "Track01",
            RacingClass = "Venom",
            TimeMilliseconds = ms
        };

        var formatted = record.FormatTime();

        Assert.Equal(expected, formatted);
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
        var record = new BestTimeRecord
        {
            PilotName = "Pilot Name",
            TeamName = "Team Name",
            CircuitName = "Track 01",
            RacingClass = "Venom",
            Category = "TimeTrialStandard",
            TimeMilliseconds = 60000,
            RecordDate = new DateTime(2024, 1, 1)
        };

        Assert.Equal("Pilot Name", record.PilotName);
        Assert.Equal("Team Name", record.TeamName);
        Assert.Equal("Track 01", record.CircuitName);
        Assert.Equal("Venom", record.RacingClass);
        Assert.Equal("TimeTrialStandard", record.Category);
        Assert.Equal(60000, record.TimeMilliseconds);
    }

    [Fact]
    public void BestTimeRecord_IsValid_WithAllProperties()
    {
        var record = new BestTimeRecord
        {
            PilotName = "Test",
            TeamName = "Team",
            CircuitName = "Track",
            RacingClass = "Venom",
            TimeMilliseconds = 100000
        };

        // Should not throw
        Assert.NotNull(record);
        Assert.NotNull(record.FormatTime());
    }
}
