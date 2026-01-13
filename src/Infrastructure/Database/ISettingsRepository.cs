using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Infrastructure.Database.Entities;

namespace WipeoutRewrite.Infrastructure.Database;

/// <summary>
/// Repository interface for accessing game settings from the database.
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Load controls settings from database.
    /// </summary>
    ControlsSettingsEntity LoadControlsSettings();

    /// <summary>
    /// Save controls settings to database.
    /// </summary>
    void SaveControlsSettings(ControlsSettingsEntity entity);

    /// <summary>
    /// Load video settings from database.
    /// </summary>
    VideoSettingsEntity LoadVideoSettings();

    /// <summary>
    /// Save video settings to database.
    /// </summary>
    void SaveVideoSettings(VideoSettingsEntity entity);

    /// <summary>
    /// Load audio settings from database.
    /// </summary>
    AudioSettingsEntity LoadAudioSettings();

    /// <summary>
    /// Save audio settings to database.
    /// </summary>
    void SaveAudioSettings(AudioSettingsEntity entity);

    /// <summary>
    /// Get all best times.
    /// </summary>
    IReadOnlyList<BestTimeEntity> GetAllBestTimes();

    /// <summary>
    /// Get best times for a circuit.
    /// </summary>
    IReadOnlyList<BestTimeEntity> GetBestTimesForCircuit(string circuitName);

    /// <summary>
    /// Get best time for circuit and class.
    /// </summary>
    BestTimeEntity? GetBestTime(string circuitName, string racingClass);

    /// <summary>
    /// Add or update a best time record.
    /// </summary>
    void AddOrUpdateBestTime(BestTimeEntity entity);

    /// <summary>
    /// Save all changes to database.
    /// </summary>
    void SaveChanges();
}

/// <summary>
/// Default implementation of the settings repository.
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly GameSettingsDbContext _context;

    public SettingsRepository(GameSettingsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public ControlsSettingsEntity LoadControlsSettings()
    {
        return _context.ControlsSettings.FirstOrDefault(s => s.Id == 1)
            ?? throw new InvalidOperationException("Controls settings not found in database");
    }

    public void SaveControlsSettings(ControlsSettingsEntity entity)
    {
        var existing = _context.ControlsSettings.FirstOrDefault(s => s.Id == 1);
        if (existing != null)
        {
            _context.ControlsSettings.Remove(existing);
        }
        entity.Id = 1;
        entity.LastModified = DateTime.UtcNow;
        _context.ControlsSettings.Add(entity);
    }

    public VideoSettingsEntity LoadVideoSettings()
    {
        return _context.VideoSettings.FirstOrDefault(s => s.Id == 1)
            ?? throw new InvalidOperationException("Video settings not found in database");
    }

    public void SaveVideoSettings(VideoSettingsEntity entity)
    {
        var existing = _context.VideoSettings.FirstOrDefault(s => s.Id == 1);
        if (existing != null)
        {
            _context.VideoSettings.Remove(existing);
        }
        entity.Id = 1;
        entity.LastModified = DateTime.UtcNow;
        _context.VideoSettings.Add(entity);
    }

    public AudioSettingsEntity LoadAudioSettings()
    {
        return _context.AudioSettings.FirstOrDefault(s => s.Id == 1)
            ?? throw new InvalidOperationException("Audio settings not found in database");
    }

    public void SaveAudioSettings(AudioSettingsEntity entity)
    {
        var existing = _context.AudioSettings.FirstOrDefault(s => s.Id == 1);
        if (existing != null)
        {
            _context.AudioSettings.Remove(existing);
        }
        entity.Id = 1;
        entity.LastModified = DateTime.UtcNow;
        _context.AudioSettings.Add(entity);
    }

    public IReadOnlyList<BestTimeEntity> GetAllBestTimes()
    {
        return _context.BestTimes
            .OrderBy(b => b.TimeMilliseconds)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<BestTimeEntity> GetBestTimesForCircuit(string circuitName)
    {
        return _context.BestTimes
            .Where(b => b.CircuitName == circuitName)
            .OrderBy(b => b.TimeMilliseconds)
            .ToList()
            .AsReadOnly();
    }

    public BestTimeEntity? GetBestTime(string circuitName, string racingClass)
    {
        return _context.BestTimes
            .Where(b => b.CircuitName == circuitName && b.RacingClass == racingClass)
            .OrderBy(b => b.TimeMilliseconds)
            .FirstOrDefault();
    }

    public void AddOrUpdateBestTime(BestTimeEntity entity)
    {
        var existing = _context.BestTimes.FirstOrDefault(b =>
            b.CircuitName == entity.CircuitName &&
            b.RacingClass == entity.RacingClass &&
            b.PilotName == entity.PilotName);

        if (existing != null && entity.TimeMilliseconds < existing.TimeMilliseconds)
        {
            existing.TimeMilliseconds = entity.TimeMilliseconds;
            existing.Team = entity.Team;
            existing.BeatDate = DateTime.UtcNow;
        }
        else if (existing == null)
        {
            entity.CreatedDate = DateTime.UtcNow;
            _context.BestTimes.Add(entity);
        }
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}
