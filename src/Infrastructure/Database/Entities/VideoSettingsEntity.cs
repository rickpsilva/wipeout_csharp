using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Infrastructure.Database.Entities;

/// <summary>
/// Database entity for Video Settings.
/// Maps to the database table for storing video configuration.
/// </summary>
public class VideoSettingsEntity
{
    public int Id { get; set; } = 1; // Single row

    public bool Fullscreen { get; set; }
    public float InternalRoll { get; set; }
    public uint UIScale { get; set; }
    public bool ShowFPS { get; set; }
    public ScreenResolutionType ScreenResolution { get; set; }
    public PostEffectType PostEffect { get; set; }

    // Metadata
    public DateTime LastModified { get; set; }
}
