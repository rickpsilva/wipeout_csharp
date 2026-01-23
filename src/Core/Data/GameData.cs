namespace WipeoutRewrite.Core.Data;

/// <summary>
/// Ship attributes for a specific race class (Venom or Rapier).
/// Mirrors team_attributes_t from wipeout-rewrite.
/// </summary>
public record ShipAttributes
{
    public float Mass { get; init; }
    public float ThrustMax { get; init; }
    public float Resistance { get; init; }
    public float TurnRate { get; init; }
    public float TurnRateMax { get; init; }
    public float Skid { get; init; }
}

/// <summary>
/// Team data with attributes per race class.
/// Mirrors team_t from wipeout-rewrite.
/// </summary>
public record TeamData
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public int LogoModelIndex { get; init; }
    public int[] Pilots { get; init; } = Array.Empty<int>();
    public Dictionary<string, ShipAttributes> Attributes { get; init; } = new();
    
    /// <summary>
    /// Get attributes for specific race class (venom/rapier).
    /// </summary>
    public ShipAttributes GetAttributes(string raceClass) =>
        Attributes.TryGetValue(raceClass.ToLowerInvariant(), out var attrs) 
            ? attrs 
            : throw new KeyNotFoundException($"No attributes for race class: {raceClass}");
}

/// <summary>
/// Pilot data.
/// Mirrors pilot_t from wipeout-rewrite.
/// </summary>
public record PilotData
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public int TeamId { get; init; }
    public string PortraitPath { get; init; } = string.Empty;
    public int LogoModelIndex { get; init; }
}

/// <summary>
/// Circuit settings for a specific race class.
/// Mirrors circut_settings_t from wipeout-rewrite.
/// </summary>
public record CircuitSettings
{
    public string TrackPath { get; init; } = string.Empty;
    public float StartLinePos { get; init; }
    public float BehindSpeed { get; init; }
    public float SpreadBase { get; init; }
    public float SpreadFactor { get; init; }
    public float SkyYOffset { get; init; }
}

/// <summary>
/// Circuit data with settings per race class.
/// Mirrors circut_t from wipeout-rewrite.
/// </summary>
public record CircuitData
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public bool IsBonus { get; init; }
    public Dictionary<string, CircuitSettings> Settings { get; init; } = new();
    
    /// <summary>
    /// Get settings for specific race class (venom/rapier).
    /// </summary>
    public CircuitSettings GetSettings(string raceClass) =>
        Settings.TryGetValue(raceClass.ToLowerInvariant(), out var settings) 
            ? settings 
            : throw new KeyNotFoundException($"No settings for race class: {raceClass}");
}

/// <summary>
/// AI opponent settings for specific race class and rank.
/// Mirrors ai_setting_t from wipeout-rewrite.
/// </summary>
public record AiSettings
{
    public int Rank { get; init; }
    public float ThrustMax { get; init; }
    public float ThrustMagnitude { get; init; }
    public bool FightBack { get; init; }
}

/// <summary>
/// Race class definition (Venom, Rapier).
/// </summary>
public record RaceClassData
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

/// <summary>
/// Race type definition (Championship, Single, Time Trial).
/// </summary>
public record RaceTypeData
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

/// <summary>
/// Game constants.
/// </summary>
public record GameConstants
{
    public int NumLaps { get; init; }
    public int NumLives { get; init; }
    public int QualifyingRank { get; init; }
    public int NumAiOpponents { get; init; }
    public int NumPilotsPerTeam { get; init; }
    public int NumNonBonusCircuits { get; init; }
    public int[] RacePointsForRank { get; init; } = Array.Empty<int>();
    public int[] ShipModelToPilot { get; init; } = Array.Empty<int>();
}

/// <summary>
/// Root data structure for game_data.json.
/// Mirrors game_def_t from wipeout-rewrite.
/// </summary>
public record GameDataRoot
{
    public string Version { get; init; } = string.Empty;
    public string LastModified { get; init; } = string.Empty;
    public TeamData[] Teams { get; init; } = Array.Empty<TeamData>();
    public PilotData[] Pilots { get; init; } = Array.Empty<PilotData>();
    public CircuitData[] Circuits { get; init; } = Array.Empty<CircuitData>();
    public Dictionary<string, AiSettings[]> AiSettings { get; init; } = new();
    public RaceClassData[] RaceClasses { get; init; } = Array.Empty<RaceClassData>();
    public RaceTypeData[] RaceTypes { get; init; } = Array.Empty<RaceTypeData>();
    public GameConstants Constants { get; init; } = new GameConstants();    public MenuStructureDefinition MenuStructure { get; init; } = new MenuStructureDefinition();}
