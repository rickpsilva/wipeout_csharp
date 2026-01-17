namespace WipeoutRewrite.Infrastructure.Database.Entities;

/// <summary>
/// Database entity for Audio Settings.
/// Maps to the database table for storing audio configuration.
/// </summary>
public class AudioSettingsEntity
{
    public int Id { get; set; } = 1; // Single row

    public float MasterVolume { get; set; }
    public float MusicVolume { get; set; }
    public float SoundEffectsVolume { get; set; }
    public bool IsMuted { get; set; }
    public bool MusicEnabled { get; set; }
    public bool SoundEffectsEnabled { get; set; }
    public string MusicMode { get; set; } = "Random";

    // Metadata
    public DateTime LastModified { get; set; }
}
