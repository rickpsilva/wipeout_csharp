using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Data;
using WipeoutRewrite.Infrastructure.UI;

namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Service to handle menu actions and update game state accordingly.
/// Bridges the gap between UI (MenuItem selections) and game logic (GameState).
/// 
/// Supports actions like:
/// - "navigate": Change to next menu
/// - "selectTeam", "selectPilot", "selectCircuit": Update game selections
/// - "startRace": Begin race with current selections
/// - "quit": Exit application
/// </summary>
public interface IMenuActionHandler
{
    /// <summary>
    /// Handle a menu action from a selected menu item.
    /// Updates game state based on action type and parameters.
    /// </summary>
    void HandleAction(string action, MenuItemDefinition itemDef, GameState gameState);
    
    /// <summary>
    /// Navigate to a specific menu.
    /// </summary>
    void NavigateToMenu(string menuId);
    
    /// <summary>
    /// Get the current menu ID.
    /// </summary>
    string? GetCurrentMenuId();
    
    /// <summary>
    /// Set callback for quit action.
    /// </summary>
    void SetQuitCallback(Action quitCallback);
    
    /// <summary>
    /// Set callback for menu navigation.
    /// </summary>
    void SetNavigationCallback(Action<string> navigationCallback);
}

/// <summary>
/// Default implementation of menu action handler.
/// Maps menu item actions to game state changes.
/// </summary>
public class MenuActionHandler : IMenuActionHandler
{
    private readonly ILogger<MenuActionHandler> _logger;
    private readonly IGameDataService _gameDataService;
    
    private string? _currentMenuId;
    private Action? _quitCallback;
    private Action<string>? _navigationCallback;
    
    public MenuActionHandler(ILogger<MenuActionHandler> logger, IGameDataService gameDataService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameDataService = gameDataService ?? throw new ArgumentNullException(nameof(gameDataService));
    }
    
    public void SetQuitCallback(Action quitCallback)
    {
        _quitCallback = quitCallback;
    }
    
    public void SetNavigationCallback(Action<string> navigationCallback)
    {
        _navigationCallback = navigationCallback;
    }
    
    public string? GetCurrentMenuId()
    {
        return _currentMenuId;
    }
    
    public void NavigateToMenu(string menuId)
    {
        _currentMenuId = menuId;
        _navigationCallback?.Invoke(menuId);
        _logger.LogDebug("Navigated to menu: {MenuId}", menuId);
    }
    
    /// <summary>
    /// Handle a menu item action.
    /// Maps action names to game state updates.
    /// </summary>
    public void HandleAction(string action, MenuItemDefinition itemDef, GameState gameState)
    {
        if (string.IsNullOrEmpty(action))
        {
            return;
        }
        
        _logger.LogDebug("Handling menu action: {Action} with item: {ItemId}", action, itemDef.Id);
        
        switch (action.ToLowerInvariant())
        {
            case "navigate":
                HandleNavigate(itemDef, gameState);
                break;
                
            case "selectteam":
                HandleSelectTeam(itemDef, gameState);
                break;
                
            case "selectpilot":
                HandleSelectPilot(itemDef, gameState);
                break;
                
            case "selectcircuit":
                HandleSelectCircuit(itemDef, gameState);
                break;
                
            case "selectraceclass":
                HandleSelectRaceClass(itemDef, gameState);
                break;
                
            case "selectracetype":
                HandleSelectRaceType(itemDef, gameState);
                break;
                
            case "startrace":
                HandleStartRace(itemDef, gameState);
                break;
                
            case "quit":
                HandleQuit(gameState);
                break;
                
            case "backmenu":
                HandleBackMenu(gameState);
                break;
                
            default:
                _logger.LogWarning("Unknown menu action: {Action}", action);
                break;
        }
    }
    
    private void HandleNavigate(MenuItemDefinition itemDef, GameState gameState)
    {
        if (!string.IsNullOrEmpty(itemDef.NextMenu))
        {
            NavigateToMenu(itemDef.NextMenu);
        }
        else
        {
            _logger.LogWarning("Navigate action with no NextMenu specified in item: {ItemId}", itemDef.Id);
        }
    }
    
    private void HandleSelectTeam(MenuItemDefinition itemDef, GameState gameState)
    {
        // Extract team ID from action parameters
        if (itemDef.ActionParams?.TryGetValue("teamId", out var teamIdObj) == true &&
            int.TryParse(teamIdObj.ToString(), out int teamId))
        {
            var teams = _gameDataService.GetTeams().ToList();
            if (teamId >= 0 && teamId < teams.Count)
            {
                var team = teams[teamId];
                var teamEnum = (Team)teamId;
                gameState.SelectedTeam = teamEnum;
                _logger.LogInformation("Selected team: {TeamName} (ID: {TeamId})", team.Name, teamId);
            }
        }
    }
    
    private void HandleSelectPilot(MenuItemDefinition itemDef, GameState gameState)
    {
        // Extract pilot index from action parameters
        if (itemDef.ActionParams?.TryGetValue("pilotIndex", out var pilotIndexObj) == true &&
            int.TryParse(pilotIndexObj.ToString(), out int pilotIndex))
        {
            gameState.SelectedPilot = pilotIndex;
            _logger.LogInformation("Selected pilot: {PilotIndex}", pilotIndex);
        }
    }
    
    private void HandleSelectCircuit(MenuItemDefinition itemDef, GameState gameState)
    {
        // Extract circuit ID from action parameters
        if (itemDef.ActionParams?.TryGetValue("circuitId", out var circuitIdObj) == true &&
            int.TryParse(circuitIdObj.ToString(), out int circuitId))
        {
            var circuits = _gameDataService.GetCircuits().ToList();
            if (circuitId >= 0 && circuitId < circuits.Count)
            {
                var circuit = circuits[circuitId];
                var circuitEnum = (Circuit)circuitId;
                gameState.SelectedCircuit = circuitEnum;
                _logger.LogInformation("Selected circuit: {CircuitName} (ID: {CircuitId})", circuit.Name, circuitId);
            }
        }
    }
    
    private void HandleSelectRaceClass(MenuItemDefinition itemDef, GameState gameState)
    {
        // Extract race class ID from action parameters
        if (itemDef.ActionParams?.TryGetValue("raceClassId", out var raceClassIdObj) == true &&
            int.TryParse(raceClassIdObj.ToString(), out int raceClassId))
        {
            var raceClass = (RaceClass)raceClassId;
            gameState.SelectedRaceClass = raceClass;
            _logger.LogInformation("Selected race class: {RaceClass} (ID: {RaceClassId})", raceClass, raceClassId);
        }
    }
    
    private void HandleSelectRaceType(MenuItemDefinition itemDef, GameState gameState)
    {
        // Extract race type ID from action parameters
        if (itemDef.ActionParams?.TryGetValue("raceTypeId", out var raceTypeIdObj) == true &&
            int.TryParse(raceTypeIdObj.ToString(), out int raceTypeId))
        {
            var raceType = (RaceType)raceTypeId;
            gameState.SelectedRaceType = raceType;
            _logger.LogInformation("Selected race type: {RaceType} (ID: {RaceTypeId})", raceType, raceTypeId);
        }
    }
    
    private void HandleStartRace(MenuItemDefinition itemDef, GameState gameState)
    {
        _logger.LogInformation("Starting race with class={RaceClass}, team={Team}, pilot={Pilot}, circuit={Circuit}",
            gameState.SelectedRaceClass,
            gameState.SelectedTeam,
            gameState.SelectedPilot,
            gameState.SelectedCircuit);
        
        // Trigger race start
        gameState.StartNewRace();
        _currentMenuId = null; // Clear menu context when race starts
    }
    
    private void HandleQuit(GameState gameState)
    {
        _logger.LogInformation("Quit action triggered");
        gameState.CurrentMode = GameMode.Menu;
        _quitCallback?.Invoke();
    }
    
    private void HandleBackMenu(GameState gameState)
    {
        _logger.LogDebug("Back menu action (typically handled by menu system itself)");
        // Menu system usually handles back navigation, this is just for logging
    }
}
