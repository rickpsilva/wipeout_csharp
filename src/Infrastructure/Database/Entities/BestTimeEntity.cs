namespace WipeoutRewrite.Infrastructure.Database.Entities;

/// <summary>
/// Database entity for Best Times.
/// Maps to the database table for storing race records.
/// </summary>
public class BestTimeEntity
{
    public int Id { get; set; }

    public string CircuitName { get; set; } = string.Empty;
    public string RacingClass { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // e.g., "Race", "TimeTrialStandard"
    public long TimeMilliseconds { get; set; }
    public string PilotName { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;

    // Metadata
    public DateTime CreatedDate { get; set; }
    public DateTime? BeatDate { get; set; }
}
