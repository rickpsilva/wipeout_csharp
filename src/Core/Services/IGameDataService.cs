namespace WipeoutRewrite.Core.Data;

/// <summary>
/// Service for loading and accessing game data from JSON.
/// Provides centralized access to teams, pilots, circuits, and game constants.
/// Mirrors game_def_t access pattern from wipeout-rewrite.
/// </summary>
public interface IGameDataService
{
    /// <summary>
    /// Load game data from JSON file.
    /// </summary>
    void Load(string jsonPath);
    
    /// <summary>
    /// Get all teams.
    /// </summary>
    IReadOnlyList<TeamData> GetTeams();
    
    /// <summary>
    /// Get team by ID.
    /// </summary>
    TeamData GetTeam(int teamId);
    
    /// <summary>
    /// Get all pilots.
    /// </summary>
    IReadOnlyList<PilotData> GetPilots();
    
    /// <summary>
    /// Get pilot by ID.
    /// </summary>
    PilotData GetPilot(int pilotId);
    
    /// <summary>
    /// Get pilots for specific team.
    /// </summary>
    IReadOnlyList<PilotData> GetPilotsForTeam(int teamId);
    
    /// <summary>
    /// Get all circuits.
    /// </summary>
    IReadOnlyList<CircuitData> GetCircuits();
    
    /// <summary>
    /// Get circuits excluding bonus circuits.
    /// </summary>
    IReadOnlyList<CircuitData> GetNonBonusCircuits();
    
    /// <summary>
    /// Get circuit by ID.
    /// </summary>
    CircuitData GetCircuit(int circuitId);
    
    /// <summary>
    /// Get AI settings for race class.
    /// </summary>
    IReadOnlyList<AiSettings> GetAiSettings(string raceClass);
    
    /// <summary>
    /// Get all race classes.
    /// </summary>
    IReadOnlyList<RaceClassData> GetRaceClasses();
    
    /// <summary>
    /// Get all race types.
    /// </summary>
    IReadOnlyList<RaceTypeData> GetRaceTypes();
    
    /// <summary>
    /// Get game constants.
    /// </summary>
    GameConstants GetConstants();
    
    /// <summary>
    /// Get menu structure definition.
    /// </summary>
    MenuStructureDefinition? GetMenuStructure();
}