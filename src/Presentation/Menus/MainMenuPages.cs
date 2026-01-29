using System.Linq;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Core.Data;
using static WipeoutRewrite.Infrastructure.UI.UIConstants;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Infrastructure.UI;
using WipeoutRewrite.Infrastructure.Input;
using UIAnchor = WipeoutRewrite.Core.Services.UIAnchor;

namespace WipeoutRewrite.Presentation.Menus;

/// <summary>
/// Actions for Best Times Viewer navigation
/// </summary>
public enum BestTimesViewerAction
{
    PreviousClass,
    NextClass,
    PreviousCircuit,
    NextCircuit
}

/// <summary>
/// Main menu pages factory for Wipeout menu system.
/// Replicates wipeout-rewrite C structure exactly:
/// - Main/Options menus: MENU_FIXED with title at top, items at bottom
/// - Submenus (Controls, Video, Audio): Dynamic centered
/// - Best Times: MENU_FIXED for type selection, then dynamic for viewer
/// </summary>
public class MainMenuPages : IMainMenuPages
{
    // Helper function for clamping values
    private int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
    
    // Helper to create error page
    private MenuPage CreateErrorPage(string title) => new MenuPage
    {
        Title = title,
        LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed | MenuLayoutFlags.AlignCenter,
        TitlePos = new Vec2i(0, 30),
        TitleAnchor = UIAnchor.TopCenter
    };

    private readonly ILogger<MainMenuPages> _logger;
    private readonly IGameState _gameState;
    private readonly ISettingsPersistenceService _settingsPersistenceService;
    private readonly IBestTimesManager _bestTimesManager;
    private readonly IMenuBuilder _menuBuilder;
    private readonly IMenuActionHandler _menuActionHandler;
    private readonly IControlsSettings _controlsSettings;
    private readonly IVideoSettings _videoSettings;
    private readonly IAudioSettings _audioSettings;
    private readonly IGameDataService _gameDataService;


    public Action? QuitGameActionCallBack { get; set; }
    public OpenTK.Windowing.GraphicsLibraryFramework.KeyboardState? CurrentKeyboardState { get; set; }

    public MainMenuPages(
        ILogger<MainMenuPages> logger, 
        IGameState gameState,
        ISettingsPersistenceService settingsPersistenceService,
        IBestTimesManager bestTimesManager,
        IMenuBuilder menuBuilder,
        IMenuActionHandler menuActionHandler,
        IControlsSettings controlsSettings,
        IVideoSettings videoSettings,
        IAudioSettings audioSettings,
        IGameDataService gameDataService     
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        _settingsPersistenceService = settingsPersistenceService ?? throw new ArgumentNullException(nameof(settingsPersistenceService));
        _bestTimesManager = bestTimesManager ?? throw new ArgumentNullException(nameof(bestTimesManager));
        _menuBuilder = menuBuilder ?? throw new ArgumentNullException(nameof(menuBuilder));
        _menuActionHandler = menuActionHandler ?? throw new ArgumentNullException(nameof(menuActionHandler));
        _controlsSettings = controlsSettings ?? throw new ArgumentNullException(nameof(controlsSettings));
        _videoSettings = videoSettings ?? throw new ArgumentNullException(nameof(videoSettings));
        _audioSettings = audioSettings ?? throw new ArgumentNullException(nameof(audioSettings));
        _gameDataService = gameDataService ?? throw new ArgumentNullException(nameof(gameDataService));
    }
    
    // ===== MAIN MENU =====
    // Structure: MENU_FIXED, title at top (30), items at bottom (-110)
    public MenuPage CreateMainMenu()
    {
        var page = new MenuPage
        {
            Id = MenuPageIds.Main,
            Title = "MAIN MENU",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed | MenuLayoutFlags.AlignCenter,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        // START GAME - Ship preview (CategoryShip, index 7 = Feisar)
        page.Items.Add(new MenuButton
        {
            Label = "START GAME",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryShip), 7),
            OnClick = (menu, data) =>
            {
                var raceClassPage = CreateRaceClassMenu();
                menu.PushPage(raceClassPage);
            }
        });
        
        // OPTIONS - MsDos logo preview (CategoryMsDos, index 3)
        page.Items.Add(new MenuButton
        {
            Label = "OPTIONS",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryMsDos), 3),
            OnClick = (menu, data) =>
            {
                var optionsPage = CreateOptionsMenu();
                menu.PushPage(optionsPage);
            }
        });
        
        // QUIT - MsDos logo preview (CategoryMsDos, index 1)
        page.Items.Add(new MenuButton
        {
            Label = "QUIT",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryMsDos), 1),
            OnClick = (menu, data) =>
            {
                // Show confirmation dialog (matching C code)
                var confirmPage = CreateQuitConfirmation();
                menu.PushPage(confirmPage);
            }
        });
        
        return page;
    }
    
    // ===== OPTIONS MENU =====
    // Structure: MENU_FIXED, title at top (30), items at bottom (-110)
    public MenuPage CreateOptionsMenu()
    {
        var page = new MenuPage
        {
            Id = MenuPageIds.Options,
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed | MenuLayoutFlags.AlignCenter,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        // CONTROLS - Controller prop preview (CategoryProp, index 1)
        page.Items.Add(new MenuButton
        {
            Label = "CONTROLS",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryProp), 1),
            OnClick = (menu, data) =>
            {
                menu.PushPage(CreateControlsMenu());
            }
        });
        
        // VIDEO - Rescue ship preview (CategoryProp, index 0)
        page.Items.Add(new MenuButton
        {
            Label = "VIDEO",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryProp), 0),
            OnClick = (menu, data) =>
            {
                menu.PushPage(CreateVideoMenu());
            }
        });
        
        // AUDIO - Headphones preview (CategoryOptions, index 3)
        page.Items.Add(new MenuButton
        {
            Label = "AUDIO",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryOptions), 3),
            OnClick = (menu, data) =>
            {
                menu.PushPage(CreateAudioMenu());
            }
        });
        
        // BEST TIMES - Stopwatch preview (CategoryOptions, index 0)
        page.Items.Add(new MenuButton
        {
            Label = "BEST TIMES",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryOptions), 0),
            OnClick = (menu, data) =>
            {
                menu.PushPage(CreateBestTimesMenu());
            }
        });
        
        return page;
    }
    
    // ===== CONTROLS MENU =====
    // Structure: MENU_FIXED, title at LEFT (-160, -100), items at LEFT (-160, -50)
    // Shows 9 actions with keyboard/joystick bindings side-by-side with custom rendering
    /// <summary>
    /// Controls menu - displays keyboard and joystick bindings in a 3-column table.
    /// Equivalent to page_options_controls_init() in wipeout-rewrite.
    /// </summary>
    public MenuPage CreateControlsMenu()
    {        
        var page = new MenuPage
        {
            Id = MenuPageIds.OptionsControls,
            Title = "CONTROLS",  // System renders title automatically in FIXED menus
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(-160, -100),  // Matching C code
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsPos = new Vec2i(-160, -50),  // Matching C code
            ItemsAnchor = UIAnchor.MiddleCenter,
            BlockWidth = 320  // Matching C code
        };
        
        if (_controlsSettings == null)
            return page;
        
        // Set controls settings for menu draw functions
        MenuDrawFunctions.SetControlsSettings(_controlsSettings);
        
        // Add 9 invisible items for selection tracking (one per action)
        for (int i = 0; i < 9; i++)
        {
            int actionIndex = i;  // Capture for lambda
            page.Items.Add(new MenuButton
            {
                Label = "",  // Empty - only used for selection index
                Data = actionIndex,
                IsEnabled = true,
                OnClick = (menu, data) =>
                {
                    // When user presses SELECT/ENTER, show "AWAITING INPUT" page
                    var awaitingPage = CreateAwaitingInputMenu((RaceAction)actionIndex);
                    menu.PushPage(awaitingPage);
                }
            });
        }
        
        // Custom draw callback - delegate to MenuDrawFunctions
        page.DrawCallback = (renderer) =>
        {
            MenuDrawFunctions.DrawControlsTable(page);
        };
        
        return page;
    }
    
    // ===== AWAITING INPUT MENU =====
    // Structure: Shows "AWAITING INPUT" with countdown timer
    // Equivalent to page_options_controls_set_init() and page_options_control_set_draw() in C
    private float _awaitInputDeadline;
    private RaceAction _currentControlAction;
    private bool _awaitingKeyRelease = true;  // Wait for all keys to be released first
    
    public MenuPage CreateAwaitingInputMenu(RaceAction action)
    {
        _currentControlAction = action;
        _awaitInputDeadline = 3.0f; // 3 seconds countdown (will be decremented in Game.cs)
        _awaitingKeyRelease = true;  // Must release all keys first

        var page = new MenuPage
        {
            Id = MenuPageIds.AwaitingInput,
            Title = "AWAITING INPUT",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,  // Centered like C code
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsAnchor = UIAnchor.MiddleCenter,
            // Custom draw callback to show countdown
            DrawCallback = (renderer) =>
                {
                    // Draw countdown number (3, 2, 1) - UI_SIZE_16 in C
                    int remaining = Math.Max(0, (int)Math.Ceiling(_awaitInputDeadline));
                    string countdownText = remaining.ToString();

                    Vec2i pos = new Vec2i(0, 24);  // Below title
                    Vec2i screenPos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, pos);
                    UIHelper.DrawTextCentered(countdownText, screenPos, 16, UIColor.Default);
                }
        };

        return page;
    }
    
    /// <summary>
    /// Updates the awaiting input countdown. Called from Game.cs.
    /// Returns true if still waiting, false if timeout occurred.
    /// </summary>
    public bool UpdateAwaitingInput(float deltaTime)
    {
        _awaitInputDeadline -= deltaTime;
        return _awaitInputDeadline > 0;
    }
    
    /// <summary>
    /// Checks if we're still waiting for keys to be released.
    /// Once all keys are released, starts capturing new input.
    /// </summary>
    public bool UpdateKeyReleaseState(bool anyKeyDown)
    {
        if (_awaitingKeyRelease)
        {
            // Wait for all keys to be released
            if (!anyKeyDown)
            {
                _awaitingKeyRelease = false;  // All keys released, start capturing
            }
            return true;  // Still waiting for release
        }
        return false;  // Ready to capture
    }
    
    /// <summary>
    /// Handles button capture for control remapping.
    /// Called from Game.cs when a button is pressed during "AWAITING INPUT".
    /// </summary>
    public void CaptureButtonForControl(uint button, bool isKeyboard)
    {
        if (_controlsSettings == null)
            return;
        
        InputDevice device = isKeyboard ? InputDevice.Keyboard : InputDevice.Joystick;
        
        // Unbind this button if it's already bound to another action
        for (int i = 0; i < (int)RaceAction.MaxActions; i++)
        {
            if (_controlsSettings.GetButtonBinding((RaceAction)i, device) == button)
            {
                _controlsSettings.SetButtonBinding((RaceAction)i, device, 0);
            }
        }
        
        // Bind the new button
        _controlsSettings.SetButtonBinding(_currentControlAction, device, button);
        
        // Save the updated binding to database
        _settingsPersistenceService.SaveControlsSettings();
    }
    
    // ===== VIDEO MENU =====
    // Structure: MENU_FIXED, title at LEFT (-160, -100), items at LEFT (-160, -60)
    // Shows 6 toggles with options
    public MenuPage CreateVideoMenu()
    {
        var page = new MenuPage
        {
            Id = MenuPageIds.OptionsVideo,
            Title = "VIDEO OPTIONS",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(-160, -100),
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsPos = new Vec2i(-160, -60),
            ItemsAnchor = UIAnchor.MiddleCenter,
            BlockWidth = 320
        };
        
        if (_videoSettings == null)
        {
            page.Items.Add(new MenuButton { Label = "ERROR: Video not initialized", IsEnabled = false });
            return page;
        }
        
        // Fullscreen
        page.Items.Add(new MenuToggle
        {
            Label = "FULLSCREEN",
            Options = new[] { "OFF", "ON" },
            CurrentIndex = _videoSettings.Fullscreen ? 1 : 0,
            OnChange = (menu, value) => {
                _videoSettings.Fullscreen = (value == 1);
                _settingsPersistenceService.SaveVideoSettings();
            }
        });
        
        // Internal Roll (camera tilt) - 0-100 scale
        var rollIndex = Clamp((int)(_videoSettings.InternalRoll * 10), 0, 10);
        page.Items.Add(new MenuToggle
        {
            Label = "INTERNAL VIEW ROLL",
            Options = new[] { "0", "10", "20", "30", "40", "50", "60", "70", "80", "90", "100" },
            CurrentIndex = rollIndex,
            OnChange = (menu, value) => {
                _videoSettings.InternalRoll = value * 0.1f;
                _settingsPersistenceService.SaveVideoSettings();
            }
        });
        
        // UI Scale (AUTO, 1X, 2X, 3X, 4X)
        page.Items.Add(new MenuToggle
        {
            Label = "UI SCALE",
            Options = new[] { "AUTO", "1X", "2X", "3X", "4X" },
            CurrentIndex = (int)_videoSettings.UIScale,
            OnChange = (menu, value) => {
                _videoSettings.UIScale = (uint)value;
                _settingsPersistenceService?.SaveVideoSettings();
            }
        });
        
        // Show FPS
        page.Items.Add(new MenuToggle
        {
            Label = "SHOW FPS",
            Options = new[] { "OFF", "ON" },
            CurrentIndex = _videoSettings.ShowFPS ? 1 : 0,
            OnChange = (menu, value) => {
                _videoSettings.ShowFPS = (value == 1);
                _settingsPersistenceService.SaveVideoSettings();
            }
        });
        
        // Screen Resolution (NATIVE, 240P, 480P)
        page.Items.Add(new MenuToggle
        {
            Label = "SCREEN RESOLUTION",
            Options = new[] { "NATIVE", "240P", "480P" },
            CurrentIndex = (int)_videoSettings.ScreenResolution,
            OnChange = (menu, value) => {
                _videoSettings.ScreenResolution = (ScreenResolutionType)value;
                _settingsPersistenceService?.SaveVideoSettings();
            }
        });
        
        // Post Effect (NONE, CRT EFFECT)
        page.Items.Add(new MenuToggle
        {
            Label = "POST PROCESSING",
            Options = new[] { "NONE", "CRT EFFECT" },
            CurrentIndex = (int)_videoSettings.PostEffect,
            OnChange = (menu, value) => {
                _videoSettings.PostEffect = (PostEffectType)value;
                _settingsPersistenceService.SaveVideoSettings();
            }
        });
        
        return page;
    }
    
    // ===== AUDIO MENU =====
    // Structure: MENU_FIXED, title at LEFT (-160, -100), items at LEFT (-160, -80)
    // Shows 2 volume toggles
    public MenuPage CreateAudioMenu()
    {
        var page = new MenuPage
        {
            Id = MenuPageIds.OptionsAudio,
            Title = "AUDIO OPTIONS",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(-160, -100),
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsPos = new Vec2i(-160, -60),  // Same as Video menu
            ItemsAnchor = UIAnchor.MiddleCenter,
            BlockWidth = 320
        };
        
        if (_audioSettings == null)
        {
            page.Items.Add(new MenuButton { Label = "ERROR: Audio not initialized", IsEnabled = false });
            return page;
        }
        
        var volumeOptions = new[] { "0", "10", "20", "30", "40", "50", "60", "70", "80", "90", "100" };
        
        // Music Volume
        page.Items.Add(new MenuToggle
        {
            Label = "MUSIC VOLUME",
            Options = volumeOptions,
            CurrentIndex = Clamp((int)(_audioSettings.MusicVolume * 10), 0, 10),
            OnChange = (menu, value) => {
                _audioSettings.MusicVolume = value * 0.1f;
                _settingsPersistenceService?.SaveAudioSettings();
            }
        });
        
        // Sound Effects Volume
        page.Items.Add(new MenuToggle
        {
            Label = "SOUND EFFECTS VOLUME",
            Options = volumeOptions,
            CurrentIndex = Clamp((int)(_audioSettings.SoundEffectsVolume * 10), 0, 10),
            OnChange = (menu, value) => {
                _audioSettings.SoundEffectsVolume = value * 0.1f;
                _settingsPersistenceService.SaveAudioSettings();
            }
        });
        
        return page;
    }
    
    // ===== BEST TIMES MENU =====
    // Structure: MENU_FIXED (like main menu), title at top (30), items at bottom (-110)
    public MenuPage CreateBestTimesMenu()
    {
        var page = new MenuPage
        {
            Id = MenuPageIds.OptionsBestTimes,
            Title = "VIEW BEST TIMES",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed | MenuLayoutFlags.AlignCenter,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        // Two main categories: TIME TRIAL and RACE
        page.Items.Add(new MenuButton
        {
            Label = "TIME TRIAL TIMES",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryOptions), 3),  // Stopwatch
            OnClick = (menu, data) =>
            {
                menu.PushPage(CreateBestTimesViewerMenu(HighscoreType.TimeTrial));
            }
        });
        
        page.Items.Add(new MenuButton
        {
            Label = "RACE TIMES",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryOptions), 3),  // Stopwatch
            OnClick = (menu, data) =>
            {
                menu.PushPage(CreateBestTimesViewerMenu(HighscoreType.Race));
            }
        });
        
        return page;
    }
    
    // ===== BEST TIMES VIEWER =====
    // Structure: MENU_FIXED (only), special navigation with arrow keys
    // Unlike C where this bypasses menu system, we use a custom draw callback
    // User navigates: UP/DOWN = change class, LEFT/RIGHT = change circuit
    private static int _bestTimesCurrentClass = 0;  // 0=Venom, 1=Rapier
    private static int _bestTimesCurrentCircuit = 0;  // 0-6 circuits
    
    /// <summary>
    /// Handles input for Best Times Viewer (called from Game.cs)
    /// </summary>
    public void HandleBestTimesViewerInput(BestTimesViewerAction action)
    {
        switch (action)
        {
            case BestTimesViewerAction.PreviousClass:
                _bestTimesCurrentClass = (_bestTimesCurrentClass - 1 + 2) % 2;
                break;
            case BestTimesViewerAction.NextClass:
                _bestTimesCurrentClass = (_bestTimesCurrentClass + 1) % 2;
                break;
            case BestTimesViewerAction.PreviousCircuit:
                _bestTimesCurrentCircuit = (_bestTimesCurrentCircuit - 1 + 7) % 7;
                break;
            case BestTimesViewerAction.NextCircuit:
                _bestTimesCurrentCircuit = (_bestTimesCurrentCircuit + 1) % 7;
                break;
        }
    }
    
    private MenuPage CreateBestTimesViewerMenu(HighscoreType type)
    {
        _bestTimesCurrentClass = 0;
        _bestTimesCurrentCircuit = 0;
        
        var page = new MenuPage
        {
            Id = MenuPageIds.BestTimesViewer,
            Title = type == HighscoreType.TimeTrial ? "BEST TIME TRIAL TIMES" : "BEST RACE TIMES",
            LayoutFlags = MenuLayoutFlags.Fixed | MenuLayoutFlags.AlignCenter,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter
        };
        
        // Custom draw function to display highscores
        page.DrawCallback = (renderer) =>
        {
            DrawBestTimesViewerData(renderer, type, _bestTimesCurrentClass, _bestTimesCurrentCircuit);
        };
        
        // Add invisible button just to have something selectable (allows back navigation)
        page.Items.Add(new MenuButton
        {
            Label = "",  // Empty label - not displayed
            OnClick = (menu, data) => { /* Do nothing, just allows back navigation */ }
        });
        
        return page;
    }
    
    private void DrawBestTimesViewerData(IMenuRenderer renderer, HighscoreType type, int classIndex, int circuitIndex)
    {
        // Get class and circuit names
        var classNames = new[] { "VENOM CLASS", "RAPIER CLASS" };
        var circuitDisplayNames = new[] { "ALTIMA VII", "KARBONIS V", "TERRAMAX", "KORODERA", "ARRIDOS IV", "SILVERSTREAM", "FIRESTAR" };
        // Map display names to database circuit names
        var circuitDatabaseNames = new[] { "Meltdown", "Mandrake", "Phoenix", "Piranha", "Riptide", "Fusion", "Volcano" };
        
        // Handle input for changing circuit (LEFT/RIGHT) and class (UP/DOWN)
        if (InputManager.IsActionPressed(GameAction.MenuRight, CurrentKeyboardState))
        {
            _bestTimesCurrentCircuit = (_bestTimesCurrentCircuit + 1) % circuitDisplayNames.Length;
        }
        if (InputManager.IsActionPressed(GameAction.MenuLeft, CurrentKeyboardState))
        {
            _bestTimesCurrentCircuit = (_bestTimesCurrentCircuit - 1 + circuitDisplayNames.Length) % circuitDisplayNames.Length;
        }
        if (InputManager.IsActionPressed(GameAction.MenuDown, CurrentKeyboardState))
        {
            _bestTimesCurrentClass = (_bestTimesCurrentClass + 1) % classNames.Length;
        }
        if (InputManager.IsActionPressed(GameAction.MenuUp, CurrentKeyboardState))
        {
            _bestTimesCurrentClass = (_bestTimesCurrentClass - 1 + classNames.Length) % classNames.Length;
        }
        
        // Draw class name at top
        Vec2i classPos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(0, 70));
        UIHelper.DrawTextCentered(classNames[_bestTimesCurrentClass], classPos, FontSizes.MenuTitle, UIColor.Accent);
        
        // Draw circuit name below class name
        Vec2i circuitPos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(0, 100));
        UIHelper.DrawTextCentered(circuitDisplayNames[_bestTimesCurrentCircuit], circuitPos, FontSizes.MenuItem, UIColor.Default);
        
        // Get actual highscores from manager
        if (_bestTimesManager != null)
        {
            var raceClassStr = _bestTimesCurrentClass == 0 ? "Venom" : "Rapier";  // Match database format
            var circuitName = circuitDatabaseNames[_bestTimesCurrentCircuit];
            var categoryStr = type == HighscoreType.TimeTrial ? "TimeTrialStandard" : "Race";
            
            // Get records for this circuit/class/category
            var allRecords = _bestTimesManager.GetRecordsForCircuit(circuitName);
            var records = allRecords
                .Where(r => r.RacingClass == raceClassStr && r.Category == categoryStr)
                .OrderBy(r => r.TimeMilliseconds)
                .Take(5)
                .ToList();
            
            // Draw 5 highscore entries - left aligned with times on right
            int startY = 130;
            int nameColumnX = -100;  // Left side for names
            int timeColumnX = 60;    // Right side for times
            
            for (int i = 0; i < 5; i++)
            {
                Vec2i rowY = new Vec2i(0, startY + i * 20);
                
                if (i < records.Count)
                {
                    var record = records[i];
                    string timeStr = record.FormatTime();  // Use built-in formatter MM:SS.T
                    
                    // Draw name on left
                    Vec2i namePos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(nameColumnX, rowY.Y));
                    UIHelper.DrawText(record.PilotName, namePos, FontSizes.MenuItem, UIColor.Default);
                    
                    // Draw time on right
                    Vec2i timePos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(timeColumnX, rowY.Y));
                    UIHelper.DrawText(timeStr, timePos, FontSizes.MenuItem, UIColor.Default);
                }
                else
                {
                    // Draw placeholder
                    Vec2i namePos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(nameColumnX, rowY.Y));
                    UIHelper.DrawText("-----------", namePos, FontSizes.MenuItem, UIColor.Default);
                    
                    Vec2i timePos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(timeColumnX, rowY.Y));
                    UIHelper.DrawText("--:--.-", timePos, FontSizes.MenuItem, UIColor.Default);
                }
            }
            
            // Draw lap record at bottom (use best time from all records)
            if (records.Count != 0)
            {
                long bestTimeMs = records.Min(r => r.TimeMilliseconds);
                long tenths = (bestTimeMs / 100) % 10;
                long secs = (bestTimeMs / 1000) % 60;
                long mins = bestTimeMs / (60 * 1000);
                string lapTimeStr = $"{mins:D2}:{secs:D2}.{tenths}";
                
                Vec2i lapLabelPos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(nameColumnX, startY + 120));
                UIHelper.DrawText("LAP RECORD", lapLabelPos, FontSizes.MenuItem, UIColor.Accent);
                
                Vec2i lapTimePos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(timeColumnX, startY + 120));
                UIHelper.DrawText(lapTimeStr, lapTimePos, FontSizes.MenuItem, UIColor.Accent);
            }
            else
            {
                Vec2i lapLabelPos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(nameColumnX, startY + 120));
                UIHelper.DrawText("LAP RECORD", lapLabelPos, FontSizes.MenuItem, UIColor.Accent);
                
                Vec2i lapTimePos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(timeColumnX, startY + 120));
                UIHelper.DrawText("--:--.-", lapTimePos, FontSizes.MenuItem, UIColor.Accent);
            }
        }
        else
        {
            // No manager - show placeholders
            int startY = 130;
            int nameColumnX = -100;
            int timeColumnX = 60;
            
            for (int i = 0; i < 5; i++)
            {
                Vec2i namePos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(nameColumnX, startY + i * 20));
                UIHelper.DrawText("-----------", namePos, FontSizes.MenuItem, UIColor.Default);
                
                Vec2i timePos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(timeColumnX, startY + i * 20));
                UIHelper.DrawText("--:--.-", timePos, FontSizes.MenuItem, UIColor.Default);
            }
            
            Vec2i lapLabelPos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(nameColumnX, startY + 120));
            UIHelper.DrawText("LAP RECORD", lapLabelPos, FontSizes.MenuItem, UIColor.Accent);
            
            Vec2i lapTimePos = UIHelper.ScaledPos(UIAnchor.TopCenter, new Vec2i(timeColumnX, startY + 120));
            UIHelper.DrawText("--:--.-", lapTimePos, FontSizes.MenuItem, UIColor.Accent);
        }
    }
        
    // ===== RACE MENUS =====
    // Structure: MENU_FIXED, title at top (30), items at bottom (-110)
    public MenuPage CreateRaceClassMenu()
    {
        var page = new MenuPage
        {
            Id = MenuPageIds.RaceClass,
            Title = "SELECT RACING CLASS",
            // NOTE: In C code, page_race_class_draw() adds MENU_FIXED flag during drawing!
            // So despite initial menu_push, it becomes FIXED (0,30) / (0,-110)
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed | MenuLayoutFlags.AlignCenter,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        // VENOM CLASS (matching C code: def.race_classes[RACE_CLASS_VENOM])
        page.Items.Add(new MenuButton
        {
            Label = "VENOM CLASS",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryPilot), 8),
            OnClick = (menu, data) =>
            {
                _gameState.SelectedRaceClass = RaceClass.Venom;
                
                var raceTypePage = CreateRaceTypeMenu();
                menu.PushPage(raceTypePage);
            }
        });
        
        // RAPIER CLASS (matching C code: def.race_classes[RACE_CLASS_RAPIER])
        page.Items.Add(new MenuButton
        {
            Label = "RAPIER CLASS",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryPilot), 9),
            OnClick = (menu, data) =>
            {
                _gameState.SelectedRaceClass = RaceClass.Rapier;
                {
                    _gameState.SelectedRaceClass = RaceClass.Rapier;
                }
                var raceTypePage = CreateRaceTypeMenu();
                menu.PushPage(raceTypePage);
            }
        });
        
        return page;
    }
    
    public MenuPage CreateRaceTypeMenu()
    {
        var page = new MenuPage
        {
            Title = "SELECT RACE TYPE",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed | MenuLayoutFlags.AlignCenter,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        // CHAMPIONSHIP RACE (matching C code: def.race_types[RACE_TYPE_CHAMPIONSHIP])
        page.Items.Add(new MenuButton
        {
            Label = "CHAMPIONSHIP RACE",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryMsDos), 2),
            OnClick = (menu, data) =>
            {
                _gameState.SelectedRaceType = RaceType.Championship;
                var teamPage = CreateTeamMenu();
                menu.PushPage(teamPage);
            }
        });
        
        // SINGLE RACE (matching C code: def.race_types[RACE_TYPE_SINGLE])
        page.Items.Add(new MenuButton
        {
            Label = "SINGLE RACE",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryMsDos), 0),
            OnClick = (menu, data) =>
            {
                _gameState.SelectedRaceType = RaceType.Single;
                
                var teamPage = CreateTeamMenu();
                menu.PushPage(teamPage);
            }
        });
        
        // TIME TRIAL (matching C code: def.race_types[RACE_TYPE_TIME_TRIAL])
        page.Items.Add(new MenuButton
        {
            Label = "TIME TRIAL",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryOptions), 0),
            OnClick = (menu, data) =>
            {

                _gameState.SelectedRaceType = RaceType.TimeTrial;
                
                var teamPage = CreateTeamMenu();
                menu.PushPage(teamPage);
            }
        });
        
        return page;
    }
    
    public MenuPage CreateTeamMenu()
    {
        // Try using MenuBuilder if available (data-driven approach)
        if (_menuBuilder != null && _gameState != null && _gameDataService != null)
        {
            try
            {
                var menuPage = _menuBuilder.BuildMenu("teamSelectMenu");
                menuPage.Id = MenuPageIds.Team;
                // Connect OnClick callbacks for each team item
                var teamsList = _gameDataService.GetTeams().ToList();
                for (int i = 0; i < menuPage.Items.Count && i < teamsList.Count; i++)
                {
                    var team = teamsList[i];
                    var teamId = team.Id;
                    var index = i;
                    
                    if (menuPage.Items[i] is MenuButton button)
                    {
                        button.OnClick = (menu, data) =>
                        {
                            // Save selected team to game state
                            _gameState.SelectedTeam = (Team)teamId;                            
                            var pilotPage = CreatePilotMenu(teamId);
                            menu.PushPage(pilotPage);
                        };
                        
                        // Add preview info for ship
                        button.ContentViewPort = new ContentPreview3DInfo(typeof(CategoryShip), index);
                    }
                }
                
                return menuPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error building team menu: {ex.Message}");
                return CreateErrorPage("ERROR LOADING MENU");
            }
        }
        
        return CreateErrorPage("MENU SYSTEM ERROR");
    }
    
    public MenuPage CreatePilotMenu(int teamId)
    {
        var page = new MenuPage
        {
            Id = MenuPageIds.Pilot,
            Title = "CHOOSE YOUR PILOT",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed | MenuLayoutFlags.AlignCenter,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        // Get pilots for the selected team
        if (_gameDataService != null)
        {
            var allPilots = _gameDataService.GetPilots();
            var teamPilots = allPilots.Where(p => p.TeamId == teamId).ToList();
            
            for (int i = 0; i < teamPilots.Count; i++)
            {
                var pilot = teamPilots[i];
                var pilotIndex = i;
                
                page.Items.Add(new MenuButton
                {
                    Label = pilot.Name,
                    Data = pilot.Id,  // Store pilot ID for preview rendering
                    ContentViewPort = new ContentPreview3DInfo(typeof(CategoryPilot), pilot.LogoModelIndex),
                    OnClick = (menu, data) =>
                    {
                        // Championship mode: Skip circuit selection (championship uses all circuits)
                        if (_gameState != null && _gameState.SelectedRaceType == RaceType.Championship)
                        {
                            // TODO: Start championship or show championship info screen
                            // For now, show empty menu (race not implemented yet)
                            var emptyPage = new MenuPage
                            {
                                Title = "CHAMPIONSHIP STARTING...",
                                LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed | MenuLayoutFlags.AlignCenter,
                                TitlePos = new Vec2i(0, 0),
                                TitleAnchor = UIAnchor.TopCenter
                            };
                            menu.PushPage(emptyPage);
                        }
                        else
                        {
                            // Single Race / Time Trial: Show circuit selection
                            var circuitPage = CreateCircuitMenu();
                            menu.PushPage(circuitPage);
                        }
                    }
                });
            }
        }
        
        return page;
    }
    
    public MenuPage CreateCircuitMenu()
    {
        // Try using MenuBuilder if available (data-driven approach)
        if (_menuBuilder != null && _gameState != null && _gameDataService != null)
        {
            try
            {
                var menuPage = _menuBuilder.BuildMenu("circuitSelectMenu");
                menuPage.Id = MenuPageIds.Circuit;
                // Connect OnClick callbacks for each circuit item
                var circuitsList = _gameDataService.GetCircuits().ToList();
                for (int i = 0; i < menuPage.Items.Count && i < circuitsList.Count; i++)
                {
                    var circuit = circuitsList[i];
                    var circuitName = circuit.Name;
                    var circuitIndex = i;
                    
                    if (menuPage.Items[i] is MenuButton button)
                    {
                        button.OnClick = (menu, data) =>
                        {
                            _gameState.SelectedCircuit = (Circuit)circuitIndex;
                            _gameState.CurrentMode = GameMode.Racing;
                            Console.WriteLine($"Starting race on {circuitName}");
                        };
                        
                        // Set circuit preview image
                        button.ContentViewPort = ContentPreview3DInfo.CreateTrackImagePreview(circuitIndex);
                    }
                }
                
                return menuPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error building circuit menu: {ex.Message}");
                return CreateErrorPage("ERROR LOADING MENU");
            }
        }
        
        return CreateErrorPage("MENU SYSTEM ERROR");
    }

    // ===== QUIT CONFIRMATION =====
    // Matching menu_confirm in C: MENU_HORIZONTAL, title + subtitle, centered
    public MenuPage CreateQuitConfirmation()
    {
        var page = new MenuPage
        {
            Id = MenuPageIds.QuitConfirmation,
            Title = "ARE YOU SURE YOU\nWANT TO QUIT",  // Two-line title (like C subtitle)
            LayoutFlags = MenuLayoutFlags.Horizontal | MenuLayoutFlags.AlignCenter,
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsAnchor = UIAnchor.MiddleCenter
        };
        
        page.Items.Add(new MenuButton
        {
            Label = "YES",
            Data = 1,  // Matching C code (YES = 1)
            OnClick = (menu, data) =>
            {
                // Close the game application
                QuitGameActionCallBack?.Invoke();
            }
        });
        
        page.Items.Add(new MenuButton
        {
            Label = "NO",
            Data = 0,  // Matching C code (NO = 0)
            OnClick = (menu, data) =>
            {
                menu.PopPage();
            }
        });

        return page;
    }
}
