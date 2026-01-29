using WipeoutRewrite.Core.Data;
using WipeoutRewrite.Infrastructure.UI;
using WipeoutRewrite.Presentation;

namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Service to dynamically build MenuPage instances from MenuStructureDefinition.
/// This is a data-driven menu system that allows menus to be defined in JSON
/// rather than hardcoded in C# callbacks, enabling non-programmers to modify menus.
/// </summary>
public interface IMenuBuilder
{
    /// <summary>
    /// Initialize builder with game data service.
    /// </summary>
    void Initialize(IGameDataService gameDataService);
    
    /// <summary>
    /// Build a menu page from definition.
    /// </summary>
    MenuPage BuildMenu(string menuId);
    
    /// <summary>
    /// Check if condition is met (for conditional menu items).
    /// </summary>
    bool EvaluateCondition(string condition, GameState gameState);
}

/// <summary>
/// Dynamically builds MenuPage instances from JSON-defined MenuStructureDefinition.
/// Supports:
/// - Static menu items (defined directly in JSON)
/// - Dynamic menu items (loaded from game data - teams, pilots, circuits)
/// - Conditional items (shown/hidden based on game state)
/// - Layout configuration (fixed vs dynamic positioning)
/// - 3D preview assignment per item
/// </summary>
public class MenuBuilder : IMenuBuilder
{
    private MenuStructureDefinition? _menuStructure;
    private IGameDataService? _gameDataService;
    
    /// <summary>
    /// Initialize builder with game data service.
    /// Must be called before BuildMenu().
    /// </summary>
    public void Initialize(IGameDataService gameDataService)
    {
        _gameDataService = gameDataService ?? throw new ArgumentNullException(nameof(gameDataService));
        
        // Load menu structure from game data
        _menuStructure = gameDataService.GetMenuStructure();
        
        if (_menuStructure == null)
        {
            throw new InvalidOperationException("Game data service returned no menu structure");
        }
    }
    
    /// <summary>
    /// Build a menu page from its definition.
    /// The returned MenuPage has items but callbacks are minimal/null -
    /// actual game logic should be implemented in MainMenuPages or a menu event handler.
    /// </summary>
    public MenuPage BuildMenu(string menuId)
    {
        return BuildMenu(menuId, null);
    }
    
    /// <summary>
    /// Build a menu page from its definition with optional game state for conditional items.
    /// </summary>
    public MenuPage BuildMenu(string menuId, GameState? gameState)
    {
        if (_menuStructure == null)
        {
            throw new InvalidOperationException("MenuBuilder not initialized. Call Initialize() first.");
        }
        
        if (!_menuStructure.Menus.TryGetValue(menuId, out var menuDef))
        {
            throw new ArgumentException($"Menu '{menuId}' not found in menu structure", nameof(menuId));
        }
        
        var menu = new MenuPage
        {
            Title = menuDef.Title,
            LayoutFlags = (menuDef.IsFixed == true) ? 
                (MenuLayoutFlags.Fixed | MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter) : 
                MenuLayoutFlags.Vertical
        };
        
        // Set positions and anchors for fixed menus
        if (menuDef.IsFixed == true)
        {
            if (menuDef.TitlePos != null)
            {
                menu.TitlePos = new Vec2i(menuDef.TitlePos.X, menuDef.TitlePos.Y);
                menu.TitleAnchor = UIAnchor.TopCenter;
            }
            if (menuDef.ItemsPos != null)
            {
                menu.ItemsPos = new Vec2i(menuDef.ItemsPos.X, menuDef.ItemsPos.Y);
                menu.ItemsAnchor = UIAnchor.BottomCenter;
            }
        }
        
        // Add static items
        if (menuDef.Items != null)
        {
            foreach (var itemDef in menuDef.Items)
            {
                // Skip items with unmet conditions
                if (gameState != null && !string.IsNullOrEmpty(itemDef.Condition) && 
                    !EvaluateCondition(itemDef.Condition, gameState))
                {
                    continue;
                }
                
                var menuItem = CreateMenuItemFromDefinition(itemDef);
                menu.Items.Add(menuItem);
            }
        }
        
        // Load dynamic items from data source
        if (!string.IsNullOrEmpty(menuDef.DynamicSource) && _gameDataService != null)
        {
            var dynamicItems = LoadDynamicItems(menuDef.DynamicSource);
            foreach (var item in dynamicItems)
            {
                menu.Items.Add(item);
            }
        }
        
        return menu;
    }
    
    /// <summary>
    /// Evaluate a condition string for use in conditional menu items.
    /// Conditions can be negated with "!" prefix.
    /// Examples: "hasRapierClass", "!isMultiplayer", "hasTeamSelected"
    /// </summary>
    public bool EvaluateCondition(string condition, GameState gameState)
    {
        if (string.IsNullOrEmpty(condition))
        {
            return true;
        }
        
        bool negate = condition.StartsWith('!');
        string condName = negate ? condition.Substring(1) : condition;
        
        bool result = condName switch
        {
            // Race class conditions
            "hasRapierClass" => gameState.SelectedRaceClass == RaceClass.Rapier,
            "hasVenomClass" => gameState.SelectedRaceClass == RaceClass.Venom,
            
            // Selection conditions
            "hasTeamSelected" => gameState.SelectedTeam != Team.AgSystems || gameState.SelectedPilot > 0,
            "hasPilotSelected" => gameState.SelectedPilot > 0,
            "hasCircuitSelected" => gameState.SelectedCircuit != Circuit.AltimaVII,
            
            // Game mode conditions
            "isSinglePlayer" => gameState.CurrentMode == GameMode.Menu, // Simplified for now
            "isMultiplayer" => false, // TODO: add multiplayer mode detection
            "onlineEnabled" => false, // Feature not yet implemented
            
            // Unlock conditions
            "hasBonus" => false, // TODO: check save file for bonus circuit unlock
            
            // Default to true for unknown conditions (safe default)
            _ => true
        };
        
        return negate ? !result : result;
    }
    
    /// <summary>
    /// Create a MenuItem (MenuButton) from a MenuItemDefinition.
    /// Note: OnClick callback is not set here - the actual game logic
    /// for handling menu selections should be implemented in MainMenuPages
    /// or a central menu event handler that checks the MenuItem.Data field.
    /// </summary>
    private MenuItem CreateMenuItemFromDefinition(MenuItemDefinition itemDef)
    {
        return new MenuButton
        {
            Label = itemDef.Label,
            IsEnabled = itemDef.IsEnabled != false,
            ContentViewPort = itemDef.Preview != null ? 
                ConvertPreviewInfo(itemDef.Preview) : null,
            OnClick = null // Callbacks set elsewhere in game logic
        };
    }
    
    /// <summary>
    /// Convert our PreviewInfo DTO to the game's ContentPreview3DInfo type.
    /// Maps string category names to actual .NET types.
    /// </summary>
    private ContentPreview3DInfo? ConvertPreviewInfo(PreviewInfo preview)
    {
        // TODO: Map category strings to actual Category types from Presentation namespace
        // For now, return null to avoid circular dependency or type resolution issues
        // This can be improved by passing a category mapper function to MenuBuilder
        return null;
    }
    
    /// <summary>
    /// Load dynamic items from data sources (teams, pilots, circuits, etc.).
    /// These items are generated programmatically from game data.
    /// </summary>
    private List<MenuItem> LoadDynamicItems(string source)
    {
        if (_gameDataService == null)
        {
            return new List<MenuItem>();
        }
        
        return source switch
        {
            "teams" => LoadTeamItems(),
            "pilots" => LoadPilotItems(),
            "circuits" => LoadCircuitItems(),
            "raceTypes" => LoadRaceTypeItems(),
            "raceClasses" => LoadRaceClassItems(),
            _ => new List<MenuItem>()
        };
    }
    
    private List<MenuItem> LoadTeamItems()
    {
        var items = new List<MenuItem>();
        var teams = _gameDataService?.GetTeams().ToArray() ?? Array.Empty<TeamData>();
        
        for (int i = 0; i < teams.Length; i++)
        {
            var team = teams[i];
            var item = new MenuButton
            {
                Label = team.Name,
                ContentViewPort = null, // TODO: Create proper category mapping
                OnClick = null // Callback set in game logic
            };
            items.Add(item);
        }
        
        return items;
    }
    
    private List<MenuItem> LoadPilotItems()
    {
        var items = new List<MenuItem>();
        // Note: Getting pilots requires knowing which team is selected
        // This would need to be passed in or retrieved from GameState
        // For now, return empty - actual implementation will be in MainMenuPages
        return items;
    }
    
    private List<MenuItem> LoadCircuitItems()
    {
        var items = new List<MenuItem>();
        var circuits = _gameDataService?.GetNonBonusCircuits().ToArray() ?? Array.Empty<CircuitData>();
        
        for (int i = 0; i < circuits.Length; i++)
        {
            var circuit = circuits[i];
            var item = new MenuButton
            {
                Label = circuit.Name,
                ContentViewPort = null, // TODO: Create proper category mapping
                OnClick = null // Callback set in game logic
            };
            items.Add(item);
        }
        
        return items;
    }
    
    private List<MenuItem> LoadRaceTypeItems()
    {
        var items = new List<MenuItem>();
        var raceTypes = _gameDataService?.GetRaceTypes().ToArray() ?? Array.Empty<RaceTypeData>();
        
        for (int i = 0; i < raceTypes.Length; i++)
        {
            var raceType = raceTypes[i];
            var item = new MenuButton
            {
                Label = raceType.Name,
                ContentViewPort = null, // TODO: Create proper category mapping
                OnClick = null // Callback set in game logic
            };
            items.Add(item);
        }
        
        return items;
    }
    
    private List<MenuItem> LoadRaceClassItems()
    {
        var items = new List<MenuItem>();
        var classes = _gameDataService?.GetRaceClasses().ToArray() ?? Array.Empty<RaceClassData>();
        
        for (int i = 0; i < classes.Length; i++)
        {
            var raceClass = classes[i];
            var item = new MenuButton
            {
                Label = raceClass.Name,
                ContentViewPort = null, // TODO: Create proper category mapping
                OnClick = null // Callback set in game logic
            };
            items.Add(item);
        }
        
        return items;
    }
}
