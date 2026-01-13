namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Enum for race classes matching wipeout.
/// </summary>
public enum RaceClass
{
    Venom = 0,
    Rapier = 1
}

/// <summary>
/// Enum for circuits matching wipeout.
/// </summary>
public enum Circuit
{
    AltimaVII = 0,
    KarbonisV = 1,
    Terramax = 2,
    Korodera = 3,
    ArridosIV = 4,
    Silverstream = 5,
    Firestar = 6
}

/// <summary>
/// Enum for highscore type/tab.
/// </summary>
public enum HighscoreType
{
    TimeTrial = 0,
    Race = 1
}

/// <summary>
/// Represents a single highscore entry (time trial or race).
/// Mirrors wipeout-rewrite highscores_entry_t.
/// </summary>
public class HighscoreEntry
{
    public string PilotName { get; set; } = "----";
    public float TimeSeconds { get; set; } = float.MaxValue;

    public HighscoreEntry() { }
    public HighscoreEntry(string pilotName, float timeSeconds)
    {
        PilotName = pilotName;
        TimeSeconds = timeSeconds;
    }

    public string FormatTime()
    {
        if (TimeSeconds == float.MaxValue)
            return "--:--.--";
        
        int minutes = (int)(TimeSeconds / 60);
        float seconds = TimeSeconds % 60;
        return $"{minutes}:{seconds:F2}";
    }

    public bool IsEmpty => TimeSeconds == float.MaxValue;
}

/// <summary>
/// Represents best times for a specific race class, circuit, and type.
/// Mirrors wipeout-rewrite highscores_t (5 entries per combo).
/// </summary>
public class HighscoresPerCategory
{
    public const int MaxEntries = 5;
    public List<HighscoreEntry> Entries { get; set; } = new();
    public float LapRecord { get; set; } = float.MaxValue;

    public HighscoresPerCategory()
    {
        for (int i = 0; i < MaxEntries; i++)
        {
            Entries.Add(new HighscoreEntry());
        }
    }

    /// <summary>
    /// Tries to add a new time. Returns true if it made top 5.
    /// </summary>
    public bool TryAddTime(string pilotName, float timeSeconds)
    {
        if (timeSeconds >= LapRecord)
            return false;

        var newEntry = new HighscoreEntry(pilotName, timeSeconds);
        Entries.Add(newEntry);
        Entries.Sort((a, b) => a.TimeSeconds.CompareTo(b.TimeSeconds));
        
        // Keep only top 5
        if (Entries.Count > MaxEntries)
            Entries.RemoveAt(MaxEntries);

        if (timeSeconds < LapRecord)
            LapRecord = timeSeconds;

        return true;
    }
}
