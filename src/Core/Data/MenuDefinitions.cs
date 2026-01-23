namespace WipeoutRewrite.Core.Data;

/// <summary>
/// Describes a single menu item (button/toggle) in the menu structure.
/// </summary>
public record MenuItemDefinition
{
    /// <summary>
    /// Unique identifier for this menu item (used for navigation and actions).
    /// </summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// Display label for the menu item.
    /// </summary>
    public string Label { get; init; } = string.Empty;
    
    /// <summary>
    /// Next menu to navigate to when selected (optional, for terminal items like "QUIT").
    /// </summary>
    public string? NextMenu { get; init; }
    
    /// <summary>
    /// Preview 3D object info (category and index).
    /// Example: { "category": "ship", "index": 0 }
    /// </summary>
    public PreviewInfo? Preview { get; init; }
    
    /// <summary>
    /// Action to perform when selected (e.g., "startRace", "setMultiplayerMode").
    /// </summary>
    public string? Action { get; init; }
    
    /// <summary>
    /// Parameters for the action (JSON object).
    /// Example: { "mode": "local2player" }
    /// </summary>
    public Dictionary<string, object>? ActionParams { get; init; }
    
    /// <summary>
    /// Condition to show/hide this item.
    /// Example: "hasRapierClass", "!onlineEnabled"
    /// </summary>
    public string? Condition { get; init; }
    
    /// <summary>
    /// Whether this item is enabled (grayed out if false).
    /// </summary>
    public bool? IsEnabled { get; init; }
}

/// <summary>
/// Preview 3D object reference.
/// </summary>
public record PreviewInfo
{
    /// <summary>
    /// Category name: "ship", "team", "pilot", "msdos", "options", etc.
    /// </summary>
    public string Category { get; init; } = string.Empty;
    
    /// <summary>
    /// Index within the category.
    /// </summary>
    public int Index { get; init; }
}

/// <summary>
/// Describes a single menu page/screen in the game.
/// </summary>
public record MenuDefinition
{
    /// <summary>
    /// Unique identifier for this menu (e.g., "mainMenu", "raceTypeMenu").
    /// </summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// Menu title displayed at top or middle.
    /// </summary>
    public string Title { get; init; } = string.Empty;
    
    /// <summary>
    /// Menu subtitle (optional, for confirmation dialogs).
    /// </summary>
    public string? Subtitle { get; init; }
    
    /// <summary>
    /// Layout type: "vertical", "horizontal", "fixed".
    /// </summary>
    public string Layout { get; init; } = "vertical";
    
    /// <summary>
    /// Whether this menu has fixed positioning.
    /// </summary>
    public bool? IsFixed { get; init; }
    
    /// <summary>
    /// Menu items (buttons/toggles).
    /// Can be static (hard-coded items) or dynamic (loaded from game data).
    /// </summary>
    public MenuItemDefinition[]? Items { get; init; }
    
    /// <summary>
    /// If dynamic, source of items ("teams", "pilots", "circuits", "raceTypes", etc.).
    /// </summary>
    public string? DynamicSource { get; init; }
    
    /// <summary>
    /// Navigation rules based on conditions.
    /// Key: condition (e.g., "onTeamSelected"), Value: next menu ID.
    /// </summary>
    public Dictionary<string, string>? ConditionalNavigation { get; init; }
    
    /// <summary>
    /// Draw callback type (e.g., "bestTimesViewer", "custom").
    /// </summary>
    public string? DrawCallback { get; init; }
    
    /// <summary>
    /// Title position (for fixed menus).
    /// Format: { "x": 0, "y": 30 }
    /// </summary>
    public PositionInfo? TitlePos { get; init; }
    
    /// <summary>
    /// Items position (for fixed menus).
    /// </summary>
    public PositionInfo? ItemsPos { get; init; }
}

/// <summary>
/// Position information for menu elements.
/// </summary>
public record PositionInfo
{
    public int X { get; init; }
    public int Y { get; init; }
}

/// <summary>
/// Root menu structure definition.
/// Maps menu IDs to their definitions.
/// </summary>
public record MenuStructureDefinition
{
    /// <summary>
    /// Mapping of menu ID to menu definition.
    /// </summary>
    public Dictionary<string, MenuDefinition> Menus { get; init; } = new();
    
    /// <summary>
    /// Entry point menu ID (usually "mainMenu").
    /// </summary>
    public string StartMenuId { get; init; } = "mainMenu";
}

/// <summary>
/// Menu action configuration (what happens when a menu item is selected).
/// </summary>
public record MenuActionConfig
{
    /// <summary>
    /// Action name: "startRace", "setGameMode", "setTeam", "setPilot", "setCircuit", "quit", etc.
    /// </summary>
    public string Action { get; init; } = string.Empty;
    
    /// <summary>
    /// Parameters for the action.
    /// </summary>
    public Dictionary<string, object>? Parameters { get; init; }
}
