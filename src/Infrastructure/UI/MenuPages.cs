using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Infrastructure.UI;

/// <summary>
/// Metadata and configuration for each menu page.
/// Provides clear definition of every menu screen in the game.
/// Eliminates scattered if statements and provides a declarative approach.
/// </summary>
public record MenuPageDefinition(
    string Id,
    string Title,
    MenuLayoutFlags LayoutFlags,
    Vec2i TitlePos,
    UIAnchor TitleAnchor,
    Vec2i? ItemsPos = null,
    UIAnchor? ItemsAnchor = null
);

/// <summary>
/// Catalog of all menu pages in the game.
/// Centralizes the definition of menu layouts and properties.
/// </summary>
public static class MenuPages
{
    // ===== MAIN FLOW =====
    
    public static readonly MenuPageDefinition Title = new(
        Id: MenuPageIds.Title,
        Title: "WIPEOUT",
        LayoutFlags: MenuLayoutFlags.Horizontal | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(0, 0),
        TitleAnchor: UIAnchor.MiddleCenter
    );
    
    public static readonly MenuPageDefinition Main = new(
        Id: MenuPageIds.Main,
        Title: "MAIN MENU",
        LayoutFlags: MenuLayoutFlags.Fixed | MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(0, 30),
        TitleAnchor: UIAnchor.TopCenter,
        ItemsPos: new Vec2i(0, -110),
        ItemsAnchor: UIAnchor.BottomCenter
    );
    
    public static readonly MenuPageDefinition Options = new(
        Id: MenuPageIds.Options,
        Title: "OPTIONS",
        LayoutFlags: MenuLayoutFlags.Fixed | MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(0, 30),
        TitleAnchor: UIAnchor.TopCenter,
        ItemsPos: new Vec2i(0, -110),
        ItemsAnchor: UIAnchor.BottomCenter
    );
    
    // ===== RACE SELECTION FLOW =====
    // Clear sequence: RaceClass -> RaceType -> Team -> Pilot -> Circuit -> RaceStart
    
    public static readonly MenuPageDefinition RaceClass = new(
        Id: MenuPageIds.RaceClass,
        Title: "SELECT RACE CLASS",
        LayoutFlags: MenuLayoutFlags.Fixed | MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(0, 30),
        TitleAnchor: UIAnchor.TopCenter,
        ItemsPos: new Vec2i(0, -110),
        ItemsAnchor: UIAnchor.BottomCenter
    );
    
    public static readonly MenuPageDefinition RaceType = new(
        Id: MenuPageIds.RaceType,
        Title: "RACE TYPE",
        LayoutFlags: MenuLayoutFlags.Fixed | MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(0, 30),
        TitleAnchor: UIAnchor.TopCenter,
        ItemsPos: new Vec2i(0, -110),
        ItemsAnchor: UIAnchor.BottomCenter
    );
    
    public static readonly MenuPageDefinition Team = new(
        Id: MenuPageIds.Team,
        Title: "SELECT YOUR TEAM",
        LayoutFlags: MenuLayoutFlags.Fixed | MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(0, 30),
        TitleAnchor: UIAnchor.TopCenter,
        ItemsPos: new Vec2i(0, -110),
        ItemsAnchor: UIAnchor.BottomCenter
    );
    
    public static readonly MenuPageDefinition Pilot = new(
        Id: MenuPageIds.Pilot,
        Title: "CHOOSE YOUR PILOT",
        LayoutFlags: MenuLayoutFlags.Fixed | MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(0, 30),
        TitleAnchor: UIAnchor.TopCenter,
        ItemsPos: new Vec2i(0, -110),
        ItemsAnchor: UIAnchor.BottomCenter
    );
    
    public static readonly MenuPageDefinition Circuit = new(
        Id: MenuPageIds.Circuit,
        Title: "SELECT RACING CIRCUIT",
        LayoutFlags: MenuLayoutFlags.Fixed | MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(0, 30),
        TitleAnchor: UIAnchor.TopCenter,
        ItemsPos: new Vec2i(0, -110),
        ItemsAnchor: UIAnchor.BottomCenter
    );
    
    public static readonly MenuPageDefinition RaceStart = new(
        Id: MenuPageIds.LoadingGame,
        Title: "READY TO RACE",
        LayoutFlags: MenuLayoutFlags.Fixed | MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(0, 0),
        TitleAnchor: UIAnchor.MiddleCenter,
        ItemsPos: new Vec2i(0, 50),
        ItemsAnchor: UIAnchor.MiddleCenter
    );
    
    // ===== OPTIONS SUBMENUS =====
    
    public static readonly MenuPageDefinition OptionsControls = new(
        Id: MenuPageIds.OptionsControls,
        Title: "CONTROLS",
        LayoutFlags: MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(-160, -100),
        TitleAnchor: UIAnchor.MiddleCenter,
        ItemsPos: new Vec2i(-160, -80),
        ItemsAnchor: UIAnchor.MiddleCenter
    );
    
    public static readonly MenuPageDefinition OptionsVideo = new(
        Id: MenuPageIds.OptionsVideo,
        Title: "VIDEO SETTINGS",
        LayoutFlags: MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(-160, -100),
        TitleAnchor: UIAnchor.MiddleCenter,
        ItemsPos: new Vec2i(-160, -80),
        ItemsAnchor: UIAnchor.MiddleCenter
    );
    
    public static readonly MenuPageDefinition OptionsAudio = new(
        Id: MenuPageIds.OptionsAudio,
        Title: "AUDIO SETTINGS",
        LayoutFlags: MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(-160, -100),
        TitleAnchor: UIAnchor.MiddleCenter,
        ItemsPos: new Vec2i(-160, -80),
        ItemsAnchor: UIAnchor.MiddleCenter
    );
    
    public static readonly MenuPageDefinition OptionsBestTimes = new(
        Id: MenuPageIds.OptionsBestTimes,
        Title: "BEST TIMES",
        LayoutFlags: MenuLayoutFlags.Fixed | MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(0, 30),
        TitleAnchor: UIAnchor.TopCenter,
        ItemsPos: new Vec2i(0, -110),
        ItemsAnchor: UIAnchor.BottomCenter
    );
    
    // ===== SPECIAL SCREENS =====
    
    public static readonly MenuPageDefinition QuitConfirmation = new(
        Id: MenuPageIds.QuitConfirmation,
        Title: "QUIT GAME?",
        LayoutFlags: MenuLayoutFlags.Horizontal | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(0, 0),
        TitleAnchor: UIAnchor.MiddleCenter,
        ItemsPos: new Vec2i(0, 50),
        ItemsAnchor: UIAnchor.MiddleCenter
    );
    
    public static readonly MenuPageDefinition Credits = new(
        Id: MenuPageIds.Credits,
        Title: "CREDITS",
        LayoutFlags: MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
        TitlePos: new Vec2i(0, 0),
        TitleAnchor: UIAnchor.MiddleCenter
    );
    
    /// <summary>
    /// Gets menu page definition by ID.
    /// </summary>
    public static MenuPageDefinition? GetDefinition(string pageId) =>
        pageId switch
        {
            var id when id == Title.Id => Title,
            var id when id == Main.Id => Main,
            var id when id == Options.Id => Options,
            var id when id == RaceClass.Id => RaceClass,
            var id when id == RaceType.Id => RaceType,
            var id when id == Team.Id => Team,
            var id when id == Pilot.Id => Pilot,
            var id when id == Circuit.Id => Circuit,
            var id when id == RaceStart.Id => RaceStart,
            var id when id == OptionsControls.Id => OptionsControls,
            var id when id == OptionsVideo.Id => OptionsVideo,
            var id when id == OptionsAudio.Id => OptionsAudio,
            var id when id == OptionsBestTimes.Id => OptionsBestTimes,
            var id when id == QuitConfirmation.Id => QuitConfirmation,
            var id when id == Credits.Id => Credits,
            _ => null
        };
}
