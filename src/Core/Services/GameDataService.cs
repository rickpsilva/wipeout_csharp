using System.Text.Json;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Data;

/// <summary>
/// Default implementation of game data service.
/// Loads data from JSON on first access (lazy loading).
/// </summary>
public class GameDataService : IGameDataService
{
    private readonly ILogger<GameDataService> _logger;
    private GameDataRoot? _data;
    private readonly object _lock = new();
    
    public GameDataService(ILogger<GameDataService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public void Load(string jsonPath)
    {
        lock (_lock)
        {
            try
            {
                _logger.LogInformation("Loading game data from: {Path}", jsonPath);
                
                if (!File.Exists(jsonPath))
                    throw new FileNotFoundException($"Game data file not found: {jsonPath}");
                
                var json = File.ReadAllText(jsonPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                
                _data = JsonSerializer.Deserialize<GameDataRoot>(json, options);
                
                if (_data == null)
                    throw new InvalidOperationException("Failed to deserialize game data");
                
                _logger.LogInformation("Game data loaded successfully: Version={Version}, Teams={Teams}, Pilots={Pilots}, Circuits={Circuits}",
                    _data.Version,
                    _data.Teams.Length,
                    _data.Pilots.Length,
                    _data.Circuits.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load game data from: {Path}", jsonPath);
                throw;
            }
        }
    }
    
    private void EnsureLoaded()
    {
        if (_data == null)
        {
            // Auto-load from default path if not explicitly loaded
            var defaultPath = Path.Combine(AppContext.BaseDirectory, "data", "game_data.json");
            Load(defaultPath);
        }
    }
    
    public IReadOnlyList<TeamData> GetTeams()
    {
        EnsureLoaded();
        return _data!.Teams;
    }
    
    public TeamData GetTeam(int teamId)
    {
        EnsureLoaded();
        var team = _data!.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null)
            throw new ArgumentException($"Team with ID {teamId} not found");
        return team;
    }
    
    public IReadOnlyList<PilotData> GetPilots()
    {
        EnsureLoaded();
        return _data!.Pilots;
    }
    
    public PilotData GetPilot(int pilotId)
    {
        EnsureLoaded();
        var pilot = _data!.Pilots.FirstOrDefault(p => p.Id == pilotId);
        if (pilot == null)
            throw new ArgumentException($"Pilot with ID {pilotId} not found");
        return pilot;
    }
    
    public IReadOnlyList<PilotData> GetPilotsForTeam(int teamId)
    {
        EnsureLoaded();
        var team = GetTeam(teamId);
        return team.Pilots
            .Select(pilotId => GetPilot(pilotId))
            .ToList()
            .AsReadOnly();
    }
    
    public IReadOnlyList<CircuitData> GetCircuits()
    {
        EnsureLoaded();
        return _data!.Circuits;
    }
    
    public IReadOnlyList<CircuitData> GetNonBonusCircuits()
    {
        EnsureLoaded();
        return _data!.Circuits
            .Where(c => !c.IsBonus)
            .ToList()
            .AsReadOnly();
    }
    
    public CircuitData GetCircuit(int circuitId)
    {
        EnsureLoaded();
        var circuit = _data!.Circuits.FirstOrDefault(c => c.Id == circuitId);
        if (circuit == null)
            throw new ArgumentException($"Circuit with ID {circuitId} not found");
        return circuit;
    }
    
    public IReadOnlyList<AiSettings> GetAiSettings(string raceClass)
    {
        EnsureLoaded();
        var key = raceClass.ToLowerInvariant();
        if (!_data!.AiSettings.TryGetValue(key, out var settings))
            throw new ArgumentException($"AI settings for race class '{raceClass}' not found");
        return settings;
    }
    
    public IReadOnlyList<RaceClassData> GetRaceClasses()
    {
        EnsureLoaded();
        return _data!.RaceClasses;
    }
    
    public IReadOnlyList<RaceTypeData> GetRaceTypes()
    {
        EnsureLoaded();
        return _data!.RaceTypes;
    }
    
    public GameConstants GetConstants()
    {
        EnsureLoaded();
        return _data!.Constants;
    }
    
    public MenuStructureDefinition? GetMenuStructure()
    {
        EnsureLoaded();
        return _data?.MenuStructure;
    }
}
