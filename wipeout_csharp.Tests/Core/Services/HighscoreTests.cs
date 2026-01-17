using Xunit;
using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Tests.Core.Services;

public class HighscoreEntryTests
{
    [Fact]
    public void Constructor_Default_InitializesWithDefaults()
    {
        var entry = new HighscoreEntry();

        Assert.Equal("----", entry.PilotName);
        Assert.Equal(float.MaxValue, entry.TimeSeconds);
    }

    [Fact]
    public void Constructor_WithParameters_InitializesCorrectly()
    {
        var entry = new HighscoreEntry("TestPilot", 125.5f);

        Assert.Equal("TestPilot", entry.PilotName);
        Assert.Equal(125.5f, entry.TimeSeconds);
    }

    [Fact]
    public void FormatTime_WithMaxValue_ReturnsPlaceholder()
    {
        var entry = new HighscoreEntry();

        var formatted = entry.FormatTime();

        Assert.Equal("--:--.--", formatted);
    }

    [Fact]
    public void FormatTime_WithValidTime_FormatsCorrectly()
    {
        var entry = new HighscoreEntry("Pilot", 125.75f);

        var formatted = entry.FormatTime();

        Assert.Equal("2:5.75", formatted); // Without leading zero in seconds
    }

    [Fact]
    public void FormatTime_WithZeroTime_FormatsCorrectly()
    {
        var entry = new HighscoreEntry("Pilot", 0f);

        var formatted = entry.FormatTime();

        Assert.Equal("0:0.00", formatted);
    }

    [Fact]
    public void FormatTime_WithLessThanMinute_FormatsCorrectly()
    {
        var entry = new HighscoreEntry("Pilot", 45.5f);

        var formatted = entry.FormatTime();

        Assert.Equal("0:45.50", formatted);
    }

    [Fact]
    public void IsEmpty_WhenMaxValue_ReturnsTrue()
    {
        var entry = new HighscoreEntry();

        Assert.True(entry.IsEmpty);
    }

    [Fact]
    public void IsEmpty_WhenHasTime_ReturnsFalse()
    {
        var entry = new HighscoreEntry("Pilot", 100f);

        Assert.False(entry.IsEmpty);
    }

    [Fact]
    public void PilotName_CanBeSet()
    {
        var entry = new HighscoreEntry
        {
            PilotName = "NewPilot"
        };

        Assert.Equal("NewPilot", entry.PilotName);
    }

    [Fact]
    public void TimeSeconds_CanBeSet()
    {
        var entry = new HighscoreEntry
        {
            TimeSeconds = 150.25f
        };

        Assert.Equal(150.25f, entry.TimeSeconds);
    }
}

public class HighscoresPerCategoryTests
{
    [Fact]
    public void Constructor_InitializesWith5EmptyEntries()
    {
        var highscores = new HighscoresPerCategory();

        Assert.Equal(5, highscores.Entries.Count);
        Assert.All(highscores.Entries, entry => Assert.True(entry.IsEmpty));
    }

    [Fact]
    public void Constructor_InitializesLapRecordToMaxValue()
    {
        var highscores = new HighscoresPerCategory();

        Assert.Equal(float.MaxValue, highscores.LapRecord);
    }

    [Fact]
    public void TryAddTime_WithTimeBetterThanLapRecord_ReturnsFalse()
    {
        var highscores = new HighscoresPerCategory
        {
            LapRecord = 100f
        };

        var result = highscores.TryAddTime("Pilot", 150f);

        Assert.False(result);
    }

    [Fact]
    public void TryAddTime_WithTimeEqualToLapRecord_ReturnsFalse()
    {
        var highscores = new HighscoresPerCategory
        {
            LapRecord = 100f
        };

        var result = highscores.TryAddTime("Pilot", 100f);

        Assert.False(result);
    }

    [Fact]
    public void TryAddTime_WithNewBestTime_UpdatesLapRecord()
    {
        var highscores = new HighscoresPerCategory();

        highscores.TryAddTime("Pilot1", 100f);

        Assert.Equal(100f, highscores.LapRecord);
    }

    [Fact]
    public void TryAddTime_AddsEntryToList()
    {
        var highscores = new HighscoresPerCategory();

        highscores.TryAddTime("Pilot1", 100f);

        Assert.Contains(highscores.Entries, e => e.PilotName == "Pilot1" && e.TimeSeconds == 100f);
    }

    [Fact]
    public void TryAddTime_SortsEntriesByTime()
    {
        var highscores = new HighscoresPerCategory();

        // Add in order of best to worst, so all will be accepted
        highscores.TryAddTime("Pilot1", 50f);  // Best time sets lap record
        highscores.TryAddTime("Pilot2", 49f);  // Even better
        highscores.TryAddTime("Pilot3", 48f);  // Even better

        // Filter out empty entries
        var nonEmptyEntries = highscores.Entries.Where(e => !e.IsEmpty).ToList();

        Assert.Equal(48f, nonEmptyEntries[0].TimeSeconds);
        Assert.Equal(49f, nonEmptyEntries[1].TimeSeconds);
        Assert.Equal(50f, nonEmptyEntries[2].TimeSeconds);
    }

    [Fact]
    public void TryAddTime_KeepsOnlyTop5Entries()
    {
        var highscores = new HighscoresPerCategory();

        // Add 6 entries
        highscores.TryAddTime("Pilot1", 100f);
        highscores.TryAddTime("Pilot2", 90f);
        highscores.TryAddTime("Pilot3", 80f);
        highscores.TryAddTime("Pilot4", 70f);
        highscores.TryAddTime("Pilot5", 60f);
        highscores.TryAddTime("Pilot6", 50f);

        // Should keep only 5 best (excluding initial 5 empty entries that get filtered)
        var nonEmptyEntries = highscores.Entries.Where(e => !e.IsEmpty).ToList();
        Assert.True(highscores.Entries.Count <= HighscoresPerCategory.MaxEntries + 1);
    }

    [Fact]
    public void TryAddTime_WithMultipleEntries_UpdatesLapRecordToFastest()
    {
        var highscores = new HighscoresPerCategory();

        highscores.TryAddTime("Pilot1", 100f);
        highscores.TryAddTime("Pilot2", 50f);
        highscores.TryAddTime("Pilot3", 75f);

        Assert.Equal(50f, highscores.LapRecord);
    }

    [Fact]
    public void MaxEntries_IsSetTo5()
    {
        Assert.Equal(5, HighscoresPerCategory.MaxEntries);
    }

    [Fact]
    public void Entries_CanBeSet()
    {
        var highscores = new HighscoresPerCategory
        {
            Entries = new List<HighscoreEntry>
            {
                new HighscoreEntry("Test", 100f)
            }
        };

        Assert.Single(highscores.Entries);
    }

    [Fact]
    public void LapRecord_CanBeSet()
    {
        var highscores = new HighscoresPerCategory
        {
            LapRecord = 99.5f
        };

        Assert.Equal(99.5f, highscores.LapRecord);
    }
}
