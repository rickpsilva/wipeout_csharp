namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Represents a single best time record.
/// </summary>
public class BestTimeRecord
{
    /// <summary>
    /// The pilot name.
    /// </summary>
    public string PilotName { get; set; } = "UNKNOWN";

    /// <summary>
    /// The team name.
    /// </summary>
    public string TeamName { get; set; } = "UNKNOWN";

    /// <summary>
    /// The circuit/track name.
    /// </summary>
    public string CircuitName { get; set; } = "UNKNOWN";

    /// <summary>
    /// The racing class (Venom, Rapier).
    /// </summary>
    public string RacingClass { get; set; } = "VENOM";

    /// <summary>
    /// The race category (Race, TimeTrialStandard).
    /// </summary>
    public string Category { get; set; } = "Race";

    /// <summary>
    /// The best time in milliseconds.
    /// </summary>
    public long TimeMilliseconds { get; set; }

    /// <summary>
    /// The date/time when the record was set.
    /// </summary>
    public DateTime RecordDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Formats the time as MM:SS.T (matching wipeout-rewrite format)
    /// </summary>
    public string FormatTime()
    {
        long totalMilliseconds = TimeMilliseconds;
        long tenths = (totalMilliseconds / 100) % 10;
        long secs = (totalMilliseconds / 1000) % 60;
        long mins = totalMilliseconds / (60 * 1000);
        
        return $"{mins:D2}:{secs:D2}.{tenths}";
    }
}

/// <summary>
/// Defines the interface for best times/records management.
/// Tracks and persists player race records.
/// </summary>
public interface IBestTimesManager
{
    /// <summary>
    /// Gets all best time records.
    /// </summary>
    IReadOnlyList<BestTimeRecord> GetAllRecords();

    /// <summary>
    /// Gets best times for a specific circuit.
    /// </summary>
    /// <param name="circuitName">The circuit name to filter by.</param>
    /// <returns>List of best time records for the circuit.</returns>
    IReadOnlyList<BestTimeRecord> GetRecordsForCircuit(string circuitName);

    /// <summary>
    /// Gets the best time for a specific circuit and class.
    /// </summary>
    /// <param name="circuitName">The circuit name.</param>
    /// <param name="racingClass">The racing class (Venom, Rapier).</param>
    /// <returns>The best time record or null if not found.</returns>
    BestTimeRecord? GetBestTime(string circuitName, string racingClass);

    /// <summary>
    /// Adds or updates a best time record.
    /// </summary>
    /// <param name="record">The record to add or update.</param>
    /// <returns>True if the record was added/updated; false if it wasn't better than existing.</returns>
    bool AddOrUpdateRecord(BestTimeRecord record);

    /// <summary>
    /// Clears all best time records.
    /// </summary>
    void ClearAllRecords();

    /// <summary>
    /// Gets the total number of records.
    /// </summary>
    int GetRecordCount();
}
