using Microsoft.Extensions.Logging;
using WipeoutRewrite.Infrastructure.Database;
using WipeoutRewrite.Infrastructure.Database.Entities;

namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Default implementation of best times manager.
/// Handles storing and retrieving player race records.
/// Persists data to database when available.
/// </summary>
public class BestTimesManager : IBestTimesManager
{
    private readonly List<BestTimeRecord> _records = new();
    private readonly ILogger<BestTimesManager> _logger;
    private readonly ISettingsRepository? _repository;

    public BestTimesManager(ILogger<BestTimesManager> logger, ISettingsRepository? repository = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = repository;
        LoadFromDatabase();
    }

    private void LoadFromDatabase()
    {
        if (_repository == null)
        {
            _logger.LogWarning("[BEST TIMES] No repository available - best times will not be loaded");
            return;
        }

        try
        {
            var entities = _repository.GetAllBestTimes();
            _logger.LogInformation("[BEST TIMES] Found {Count} entities in database", entities.Count);
            
            foreach (var entity in entities)
            {
                _logger.LogDebug("[BEST TIMES] Loading: {Circuit} - {Class} - {Category} - {Pilot}: {Time}ms", 
                    entity.CircuitName, entity.RacingClass, entity.Category, entity.PilotName, entity.TimeMilliseconds);
                    
                _records.Add(new BestTimeRecord
                {
                    CircuitName = entity.CircuitName,
                    RacingClass = entity.RacingClass,
                    Category = entity.Category,
                    TimeMilliseconds = entity.TimeMilliseconds,
                    PilotName = entity.PilotName,
                    TeamName = entity.Team
                });
            }
            _logger.LogInformation("[BEST TIMES] Successfully loaded {Count} records from database", _records.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BEST TIMES] Failed to load records from database");
        }
    }

    public IReadOnlyList<BestTimeRecord> GetAllRecords()
    {
        return _records.AsReadOnly();
    }

    public IReadOnlyList<BestTimeRecord> GetRecordsForCircuit(string circuitName)
    {
        if (string.IsNullOrEmpty(circuitName))
            return new List<BestTimeRecord>();

        return _records
            .Where(r => r.CircuitName == circuitName)
            .OrderBy(r => r.TimeMilliseconds)
            .ToList()
            .AsReadOnly();
    }

    public BestTimeRecord? GetBestTime(string circuitName, string racingClass)
    {
        if (string.IsNullOrEmpty(circuitName) || string.IsNullOrEmpty(racingClass))
            return null;

        return _records
            .Where(r => r.CircuitName == circuitName && r.RacingClass == racingClass)
            .OrderBy(r => r.TimeMilliseconds)
            .FirstOrDefault();
    }

    public bool AddOrUpdateRecord(BestTimeRecord record)
    {
        if (record == null)
        {
            _logger.LogWarning("[BEST TIMES] Attempted to add null record");
            return false;
        }

        var existing = GetBestTime(record.CircuitName, record.RacingClass);

        if (existing != null && existing.TimeMilliseconds <= record.TimeMilliseconds)
        {
            _logger.LogInformation("[BEST TIMES] Record not better than existing: {Existing} vs {New}",
                existing.FormatTime(), record.FormatTime());
            return false;
        }

        if (existing != null)
        {
            _records.Remove(existing);
        }

        _records.Add(record);

        // Save to database
        if (_repository != null)
        {
            try
            {
                var entity = new BestTimeEntity
                {
                    CircuitName = record.CircuitName,
                    RacingClass = record.RacingClass,
                    TimeMilliseconds = record.TimeMilliseconds,
                    PilotName = record.PilotName,
                    Team = record.TeamName,
                    CreatedDate = DateTime.UtcNow
                };
                _repository.AddOrUpdateBestTime(entity);
                _repository.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BEST TIMES] Failed to save record to database");
            }
        }
        _logger.LogInformation("[BEST TIMES] Added record: {Pilot} - {Team} on {Circuit} ({Class}): {Time}",
            record.PilotName, record.TeamName, record.CircuitName, record.RacingClass, record.FormatTime());

        return true;
    }

    public void ClearAllRecords()
    {
        _records.Clear();
        _logger.LogInformation("[BEST TIMES] All records cleared");
    }

    public int GetRecordCount()
    {
        return _records.Count;
    }
}
