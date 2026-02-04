namespace WipeoutRewrite.Infrastructure.UI;

/// <summary>
/// Centralized constants for menu page identifiers.
/// Replaces magic strings like "SELECT YOUR TEAM" with type-safe constants.
/// </summary>
public static class MenuPageIds
{
    // Main menu pages
    public const string Title = "titleScreen";
    public const string Main = "mainMenu";
    public const string Options = "optionsMenu";
    
    // Selection menus
    public const string Team = "teamSelectMenu";
    public const string Pilot = "pilotSelectMenu";
    public const string Circuit = "circuitSelectMenu";
    public const string RaceClass = "raceClassSelectMenu";
    public const string RaceType = "raceTypeSelectMenu";
    
    // Options submenus
    public const string OptionsControls = "optionsControlsMenu";
    public const string OptionsVideo = "optionsVideoMenu";
    public const string OptionsAudio = "optionsAudioMenu";
    public const string OptionsBestTimes = "optionsBestTimesMenu";
    
    // Special pages
    public const string QuitConfirmation = "quitConfirmationMenu";
    public const string Credits = "creditsScreen";
    public const string AwaitingInput = "awaitingInputMenu";
    
    // Best Times Viewer pages (dynamic)
    public const string BestTimesViewer = "bestTimesViewer";
    
    // Loading game page
    public const string LoadingGame = "loadingGameMenu";
    // In-game menu
    public const string PlayingWipeout = "playingWipeout";
}
