namespace WipeoutRewrite.Core.Data;

/// <summary>
/// AI opponent settings for specific race class and rank.
/// Mirrors ai_setting_t from wipeout-rewrite.
/// </summary>
public record AiSettings
{
    public bool FightBack { get; init; }
    public int Rank { get; init; }
    public float ThrustMagnitude { get; init; }
    public float ThrustMax { get; init; }
}

/// <summary>
/// Circuit data with settings per race class.
/// Mirrors circut_t from wipeout-rewrite.
/// </summary>
public record CircuitData
{
    public string DisplayName { get; init; } = string.Empty;
    public int Id { get; init; }
    public bool IsBonus { get; init; }
    public string Name { get; init; } = string.Empty;
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
/// Circuit settings for a specific race class.
/// Mirrors circut_settings_t from wipeout-rewrite.
/// </summary>
public record CircuitSettings
{
    public float BehindSpeed { get; init; }
    public float SkyYOffset { get; init; }
    public float SpreadBase { get; init; }
    public float SpreadFactor { get; init; }
    public float StartLinePos { get; init; }
    public string TrackPath { get; init; } = string.Empty;
}

/// <summary>
/// Game constants.
/// </summary>
public record GameConstants
{
    public int NumAiOpponents { get; init; }
    public int NumLaps { get; init; }
    public int NumLives { get; init; }
    public int NumNonBonusCircuits { get; init; }
    public int NumPilotsPerTeam { get; init; }
    public int QualifyingRank { get; init; }
    public int[] RacePointsForRank { get; init; } = Array.Empty<int>();
    public int[] ShipModelToPilot { get; init; } = Array.Empty<int>();
}

/// <summary>
/// Root data structure for game_data.json.
/// Mirrors game_def_t from wipeout-rewrite.
/// </summary>
public record GameDataRoot
{
    public Dictionary<string, AiSettings[]> AiSettings { get; init; } = new();
    public CircuitData[] Circuits { get; init; } = Array.Empty<CircuitData>();
    public GameConstants Constants { get; init; } = new GameConstants();
    public string LastModified { get; init; } = string.Empty;
    public MenuStructureDefinition MenuStructure { get; init; } = new MenuStructureDefinition();
    public PilotData[] Pilots { get; init; } = Array.Empty<PilotData>();
    public RaceClassData[] RaceClasses { get; init; } = Array.Empty<RaceClassData>();
    public RaceTypeData[] RaceTypes { get; init; } = Array.Empty<RaceTypeData>();
    public TeamData[] Teams { get; init; } = Array.Empty<TeamData>();
    public string Version { get; init; } = string.Empty;
}

/// <summary>
/// Pilot data.
/// Mirrors pilot_t from wipeout-rewrite.
/// </summary>
public record PilotData
{
    public string DisplayName { get; init; } = string.Empty;
    public int Id { get; init; }
    public int LogoModelIndex { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PortraitPath { get; init; } = string.Empty;
    public int ShipNumber { get; init; }
    public int ShipIndex { get; init; }
    public int TeamId { get; init; }
}

/// <summary>
/// Race class definition (Venom, Rapier).
/// </summary>
public record RaceClassData
{
    public string DisplayName { get; init; } = string.Empty;
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// Race type definition (Championship, Single, Time Trial).
/// </summary>
public record RaceTypeData
{
    public string DisplayName { get; init; } = string.Empty;
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// Ship attributes for a specific race class (Venom or Rapier).
/// Mirrors team_attributes_t from wipeout-rewrite.
/// </summary>
public record ShipAttributes
{
    public float Mass { get; init; }
    public float Resistance { get; init; }
    public float Skid { get; init; }
    public float ThrustMax { get; init; }
    public float TurnRate { get; init; }
    public float TurnRateMax { get; init; }
}

/// <summary>
/// Team data with attributes per race class.
/// Mirrors team_t from wipeout-rewrite.
/// </summary>
public record TeamData
{
    public Dictionary<string, ShipAttributes> Attributes { get; init; } = new();
    public string DisplayName { get; init; } = string.Empty;
    public int Id { get; init; }
    public int LogoModelIndex { get; init; }
    public string Name { get; init; } = string.Empty;
    public int[] Pilots { get; init; } = Array.Empty<int>();

    /// <summary>
    /// Get attributes for specific race class (venom/rapier).
    /// </summary>
    public ShipAttributes GetAttributes(string raceClass) =>
        Attributes.TryGetValue(raceClass.ToLowerInvariant(), out var attrs)
            ? attrs
            : throw new KeyNotFoundException($"No attributes for race class: {raceClass}");
}