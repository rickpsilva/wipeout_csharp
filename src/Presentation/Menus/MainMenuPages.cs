using System.Linq;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Core.Data;
using WipeoutRewrite.Core.Entities;
using static WipeoutRewrite.Infrastructure.UI.UIConstants;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Presentation;
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
public static class MainMenuPages
{
    // Helper function for clamping values
    private static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
    
    // Store references for menu callbacks
    public static GameState? GameStateRef { get; set; }
    public static IContentPreview3D? ContentPreview3DRef { get; set; }
    public static IOptionsFactory? OptionsFactoryRef { get; set; }
    public static SettingsPersistenceService? SettingsPersistenceServiceRef { get; set; }
    public static IBestTimesManager? BestTimesManagerRef { get; set; }
    public static IMenuBuilder? MenuBuilderRef { get; set; }
    public static IMenuActionHandler? MenuActionHandlerRef { get; set; }
    public static IGameDataService? GameDataServiceRef { get; set; }
    public static Action? QuitGameAction { get; set; }  // Callback to close the game
    public static OpenTK.Windowing.GraphicsLibraryFramework.KeyboardState? CurrentKeyboardState { get; set; }
    
    // Option references - should come from SettingsPersistenceService
    private static IControlsSettings? _controlsSettings;
    private static IVideoSettings? _videoSettings;
    private static IAudioSettings? _audioSettings;
    private static IBestTimesManager? _bestTimesManager;
    
    private static void EnsureOptionsInitialized()
    {
        if (OptionsFactoryRef == null) return;
        
        // Try to get BestTimesManager from injected reference first
        _bestTimesManager ??= BestTimesManagerRef;
        
        // Try to get settings from SettingsPersistenceService first (same instances used for saving)
        if (SettingsPersistenceServiceRef != null)
        {
            _controlsSettings ??= SettingsPersistenceServiceRef.GetControlsSettings();
            _videoSettings ??= SettingsPersistenceServiceRef.GetVideoSettings();
            _audioSettings ??= SettingsPersistenceServiceRef.GetAudioSettings();
        }
        else
        {
            // Fallback to creating new instances if service not available
            _controlsSettings ??= OptionsFactoryRef.CreateControlsSettings();
            _videoSettings ??= OptionsFactoryRef.CreateVideoSettings();
            _audioSettings ??= OptionsFactoryRef.CreateAudioSettings();
        }
        
        // Fallback to creating BestTimesManager if not injected
        _bestTimesManager ??= OptionsFactoryRef.CreateBestTimesManager();
    }
    
    // ===== MAIN MENU =====
    // Structure: MENU_FIXED, title at top (30), items at bottom (-110)
    public static MenuPage CreateMainMenu()
    {
        var page = new MenuPage
        {
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
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 7),
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
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryMsDos), 3),
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
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryMsDos), 1),
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
    public static MenuPage CreateOptionsMenu()
    {
        var page = new MenuPage
        {
            Title = "OPTIONS",
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
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryProp), 1),
            OnClick = (menu, data) =>
            {
                menu.PushPage(CreateControlsMenu());
            }
        });
        
        // VIDEO - Rescue ship preview (CategoryProp, index 0)
        page.Items.Add(new MenuButton
        {
            Label = "VIDEO",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryProp), 0),
            OnClick = (menu, data) =>
            {
                menu.PushPage(CreateVideoMenu());
            }
        });
        
        // AUDIO - Headphones preview (CategoryOptions, index 3)
        page.Items.Add(new MenuButton
        {
            Label = "AUDIO",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryOptions), 3),
            OnClick = (menu, data) =>
            {
                menu.PushPage(CreateAudioMenu());
            }
        });
        
        // BEST TIMES - Stopwatch preview (CategoryOptions, index 0)
        page.Items.Add(new MenuButton
        {
            Label = "BEST TIMES",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryOptions), 0),
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
    public static MenuPage CreateControlsMenu()
    {
        EnsureOptionsInitialized();
        
        var page = new MenuPage
        {
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
    private static float _awaitInputDeadline;
    private static RaceAction _currentControlAction;
    private static bool _awaitingKeyRelease = true;  // Wait for all keys to be released first
    
    public static MenuPage CreateAwaitingInputMenu(RaceAction action)
    {
        _currentControlAction = action;
        _awaitInputDeadline = 3.0f; // 3 seconds countdown (will be decremented in Game.cs)
        _awaitingKeyRelease = true;  // Must release all keys first
        
        var page = new MenuPage
        {
            Title = "AWAITING INPUT",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,  // Centered like C code
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsAnchor = UIAnchor.MiddleCenter
        };
        
        // Custom draw callback to show countdown
        page.DrawCallback = (renderer) =>
        {
            // Draw countdown number (3, 2, 1) - UI_SIZE_16 in C
            int remaining = Math.Max(0, (int)Math.Ceiling(_awaitInputDeadline));
            string countdownText = remaining.ToString();
            
            Vec2i pos = new Vec2i(0, 24);  // Below title
            Vec2i screenPos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, pos);
            UIHelper.DrawTextCentered(countdownText, screenPos, 16, UIColor.Default);
        };
        
        return page;
    }
    
    /// <summary>
    /// Updates the awaiting input countdown. Called from Game.cs.
    /// Returns true if still waiting, false if timeout occurred.
    /// </summary>
    public static bool UpdateAwaitingInput(float deltaTime)
    {
        _awaitInputDeadline -= deltaTime;
        return _awaitInputDeadline > 0;
    }
    
    /// <summary>
    /// Checks if we're still waiting for keys to be released.
    /// Once all keys are released, starts capturing new input.
    /// </summary>
    public static bool UpdateKeyReleaseState(bool anyKeyDown)
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
    public static void CaptureButtonForControl(uint button, bool isKeyboard)
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
        SettingsPersistenceServiceRef?.SaveControlsSettings();
    }
    
    // ===== VIDEO MENU =====
    // Structure: MENU_FIXED, title at LEFT (-160, -100), items at LEFT (-160, -60)
    // Shows 6 toggles with options
    public static MenuPage CreateVideoMenu()
    {
        EnsureOptionsInitialized();
        
        var page = new MenuPage
        {
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
                SettingsPersistenceServiceRef?.SaveVideoSettings();
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
                SettingsPersistenceServiceRef?.SaveVideoSettings();
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
                SettingsPersistenceServiceRef?.SaveVideoSettings();
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
                SettingsPersistenceServiceRef?.SaveVideoSettings();
            }
        });
        
        // Screen Resolution (NATIVE, 240P, 480P)
        page.Items.Add(new MenuToggle
        {
            Label = "SCREEN RESOLUTION",
            Options = new[] { "NATIVE", "240P", "480P" },
            CurrentIndex = _videoSettings.ScreenResolution,
            OnChange = (menu, value) => {
                _videoSettings.ScreenResolution = value;
                SettingsPersistenceServiceRef?.SaveVideoSettings();
            }
        });
        
        // Post Effect (NONE, CRT EFFECT)
        page.Items.Add(new MenuToggle
        {
            Label = "POST PROCESSING",
            Options = new[] { "NONE", "CRT EFFECT" },
            CurrentIndex = _videoSettings.PostEffect,
            OnChange = (menu, value) => {
                _videoSettings.PostEffect = value;
                SettingsPersistenceServiceRef?.SaveVideoSettings();
            }
        });
        
        return page;
    }
    
    // ===== AUDIO MENU =====
    // Structure: MENU_FIXED, title at LEFT (-160, -100), items at LEFT (-160, -80)
    // Shows 2 volume toggles
    public static MenuPage CreateAudioMenu()
    {
        EnsureOptionsInitialized();
        
        var page = new MenuPage
        {
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
                SettingsPersistenceServiceRef?.SaveAudioSettings();
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
                SettingsPersistenceServiceRef?.SaveAudioSettings();
            }
        });
        
        return page;
    }
    
    // ===== BEST TIMES MENU =====
    // Structure: MENU_FIXED (like main menu), title at top (30), items at bottom (-110)
    public static MenuPage CreateBestTimesMenu()
    {
        EnsureOptionsInitialized();
        
        var page = new MenuPage
        {
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
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryOptions), 3),  // Stopwatch
            OnClick = (menu, data) =>
            {
                menu.PushPage(CreateBestTimesViewerMenu(HighscoreType.TimeTrial));
            }
        });
        
        page.Items.Add(new MenuButton
        {
            Label = "RACE TIMES",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryOptions), 3),  // Stopwatch
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
    public static void HandleBestTimesViewerInput(BestTimesViewerAction action)
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
    
    private static MenuPage CreateBestTimesViewerMenu(HighscoreType type)
    {
        _bestTimesCurrentClass = 0;
        _bestTimesCurrentCircuit = 0;
        
        var page = new MenuPage
        {
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
    
    private static void DrawBestTimesViewerData(IMenuRenderer renderer, HighscoreType type, int classIndex, int circuitIndex)
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
            if (records.Any())
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
    
    private static string FormatTime(float seconds)
    {
        // Format as MM:SS.T (tenths of a second)
        long totalMs = (long)(seconds * 1000);
        long tenths = (totalMs / 100) % 10;
        long secs = (totalMs / 1000) % 60;
        long mins = totalMs / (60 * 1000);
        
        return $"{mins:D2}:{secs:D2}.{tenths}";
    }
    
    // ===== RACE MENUS =====
    // Structure: MENU_FIXED, title at top (30), items at bottom (-110)
    
    public static MenuPage CreateRaceClassMenu()
    {
        var page = new MenuPage
        {
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
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryPilot), 8),
            OnClick = (menu, data) =>
            {
                if (GameStateRef != null)
                {
                    GameStateRef.SelectedRaceClass = RaceClass.Venom;
                }
                var raceTypePage = CreateRaceTypeMenu();
                menu.PushPage(raceTypePage);
            }
        });
        
        // RAPIER CLASS (matching C code: def.race_classes[RACE_CLASS_RAPIER])
        page.Items.Add(new MenuButton
        {
            Label = "RAPIER CLASS",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryPilot), 9),
            OnClick = (menu, data) =>
            {
                if (GameStateRef != null)
                {
                    GameStateRef.SelectedRaceClass = RaceClass.Rapier;
                }
                var raceTypePage = CreateRaceTypeMenu();
                menu.PushPage(raceTypePage);
            }
        });
        
        return page;
    }
    
    public static MenuPage CreateRaceTypeMenu()
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
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryMsDos), 2),
            OnClick = (menu, data) =>
            {
                if (GameStateRef != null)
                {
                    GameStateRef.SelectedRaceType = RaceType.Championship;
                }
                var teamPage = CreateTeamMenu();
                menu.PushPage(teamPage);
            }
        });
        
        // SINGLE RACE (matching C code: def.race_types[RACE_TYPE_SINGLE])
        page.Items.Add(new MenuButton
        {
            Label = "SINGLE RACE",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryMsDos), 0),
            OnClick = (menu, data) =>
            {
                if (GameStateRef != null)
                {
                    GameStateRef.SelectedRaceType = RaceType.Single;
                }
                var teamPage = CreateTeamMenu();
                menu.PushPage(teamPage);
            }
        });
        
        // TIME TRIAL (matching C code: def.race_types[RACE_TYPE_TIME_TRIAL])
        page.Items.Add(new MenuButton
        {
            Label = "TIME TRIAL",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryOptions), 0),
            OnClick = (menu, data) =>
            {
                if (GameStateRef != null)
                {
                    GameStateRef.SelectedRaceType = RaceType.TimeTrial;
                }
                var teamPage = CreateTeamMenu();
                menu.PushPage(teamPage);
            }
        });
        
        return page;
    }
    
    public static MenuPage CreateTeamMenu()
    {
        // Try using MenuBuilder if available (data-driven approach)
        if (MenuBuilderRef != null && GameStateRef != null && GameDataServiceRef != null)
        {
            try
            {
                var menuPage = MenuBuilderRef.BuildMenu("teamSelectMenu");
                
                // Connect OnClick callbacks for each team item
                var teamsList = GameDataServiceRef.GetTeams().ToList();
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
                            if (GameStateRef != null)
                            {
                                GameStateRef.SelectedTeam = (Team)teamId;
                            }
                            
                            var pilotPage = CreatePilotMenu(teamId);
                            menu.PushPage(pilotPage);
                        };
                        
                        // Add preview info for ship
                        button.PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), index);
                    }
                }
                
                return menuPage;
            }
            catch (Exception)
            {
                // Fall back to hardcoded if MenuBuilder fails
            }
        }
        
        // Fallback: Original hardcoded implementation
        var page = new MenuPage
        {
            Title = "SELECT YOUR TEAM",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed | MenuLayoutFlags.AlignCenter,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        // Teams in exact C order: AG SYSTEMS, AURICOM, QIREX, FEISAR
        var teams = new[] { "AG SYSTEMS", "AURICOM", "QIREX", "FEISAR" };
        
        for (int i = 0; i < teams.Length; i++)
        {
            var team = teams[i];
            var teamId = i;
            var shipIndex = i;
            page.Items.Add(new MenuButton
            {
                Label = team,
                PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), shipIndex),
                OnClick = (menu, data) =>
                {
                    // Save selected team to game state
                    if (GameStateRef != null)
                    {
                        GameStateRef.SelectedTeam = (Team)teamId;
                    }
                    
                    var pilotPage = CreatePilotMenu(teamId);
                    menu.PushPage(pilotPage);
                }
            });
        }
        
        return page;
    }
    
    public static MenuPage CreatePilotMenu(int teamId)
    {
        var page = new MenuPage
        {
            Title = "CHOOSE YOUR PILOT",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed | MenuLayoutFlags.AlignCenter,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        // Get pilots for the selected team
        if (GameDataServiceRef != null)
        {
            var allPilots = GameDataServiceRef.GetPilots();
            var teamPilots = allPilots.Where(p => p.TeamId == teamId).ToList();
            
            for (int i = 0; i < teamPilots.Count; i++)
            {
                var pilot = teamPilots[i];
                var pilotIndex = i;
                
                page.Items.Add(new MenuButton
                {
                    Label = pilot.Name,
                    PreviewInfo = new ContentPreview3DInfo(typeof(CategoryPilot), pilot.LogoModelIndex),
                    OnClick = (menu, data) =>
                    {
                        // Championship mode: Skip circuit selection (championship uses all circuits)
                        if (GameStateRef != null && GameStateRef.SelectedRaceType == RaceType.Championship)
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
        else
        {
            // Fallback if GameDataService not available
            page.Items.Add(new MenuButton
            {
                Label = "PILOT 1",
                OnClick = (menu, data) =>
                {
                    var circuitPage = CreateCircuitMenu();
                    menu.PushPage(circuitPage);
                }
            });
            
            page.Items.Add(new MenuButton
            {
                Label = "PILOT 2",
                OnClick = (menu, data) =>
                {
                    var circuitPage = CreateCircuitMenu();
                    menu.PushPage(circuitPage);
                }
            });
        }
        
        return page;
    }
    
    public static MenuPage CreateCircuitMenu()
    {
        // Try using MenuBuilder if available (data-driven approach)
        if (MenuBuilderRef != null && GameStateRef != null && GameDataServiceRef != null)
        {
            try
            {
                var menuPage = MenuBuilderRef.BuildMenu("circuitSelectMenu");
                
                // Connect OnClick callbacks for each circuit item
                var circuitsList = GameDataServiceRef.GetCircuits().ToList();
                for (int i = 0; i < menuPage.Items.Count && i < circuitsList.Count; i++)
                {
                    var circuit = circuitsList[i];
                    var circuitName = circuit.Name;
                    var circuitIndex = i;
                    
                    if (menuPage.Items[i] is MenuButton button)
                    {
                        button.OnClick = (menu, data) =>
                        {
                            if (GameStateRef != null)
                            {
                                GameStateRef.SelectedCircuit = (Circuit)circuitIndex;
                                GameStateRef.CurrentMode = GameMode.Racing;
                                Console.WriteLine($"Starting race on {circuitName}");
                            }
                        };
                        
                        // Set circuit preview image
                        button.PreviewInfo = ContentPreview3DInfo.CreateTrackImagePreview(circuitIndex);
                    }
                }
                
                return menuPage;
            }
            catch (Exception)
            {
                // Fall back to hardcoded if MenuBuilder fails
            }
        }
        
        // Fallback: Original hardcoded implementation
        var page = new MenuPage
        {
            Title = "Select Racing Circuit",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed | MenuLayoutFlags.AlignCenter,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        var circuits = new[]
        {
            "ALTIMA VII",
            "KARBONIS V",
            "TERRAMAX",
            "KORODERA",
            "ARRIDOS IV",
            "SILVERSTREAM",
            "FIRESTAR"
        };
        
        for (int i = 0; i < circuits.Length; i++)
        {
            var circuit = circuits[i];
            string circuitName = circuit;
            var circuitIndex = i;
            page.Items.Add(new MenuButton
            {
                Label = circuit,
                PreviewInfo = ContentPreview3DInfo.CreateTrackImagePreview(circuitIndex),
                OnClick = (menu, data) =>
                {
                    if (GameStateRef != null)
                    {
                        GameStateRef.SelectedCircuit = (Circuit)circuitIndex;
                        GameStateRef.CurrentMode = GameMode.Racing;
                        Console.WriteLine($"Starting race on {circuitName}");
                    }
                }
            });
        }
        
        return page;
    }

    // ===== QUIT CONFIRMATION =====
    // Matching menu_confirm in C: MENU_HORIZONTAL, title + subtitle, centered
    public static MenuPage CreateQuitConfirmation()
    {
        var page = new MenuPage
        {
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
                QuitGameAction?.Invoke();
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

    // ===== PAUSE MENU =====
    // Structure: MENU_VERTICAL centered (not MENU_FIXED), dynamic positioning
    public static MenuPage CreatePauseMenu()
    {
        var page = new MenuPage
        {
            Title = "PAUSED",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsAnchor = UIAnchor.MiddleCenter
        };
        
        // CONTINUE
        page.Items.Add(new MenuButton
        {
            Label = "CONTINUE",
            OnClick = (menu, data) =>
            {
                // Unpause - return to race
                if (GameStateRef != null)
                    GameStateRef.CurrentMode = GameMode.Racing;
                menu.PopPage();
            }
        });
        
        // RESTART - shows confirmation
        page.Items.Add(new MenuButton
        {
            Label = "RESTART",
            OnClick = (menu, data) =>
            {
                var confirmPage = CreateRestartConfirmation();
                menu.PushPage(confirmPage);
            }
        });
        
        // QUIT - shows confirmation
        page.Items.Add(new MenuButton
        {
            Label = "QUIT",
            OnClick = (menu, data) =>
            {
                var confirmPage = CreateQuitRaceConfirmation();
                menu.PushPage(confirmPage);
            }
        });
        
        // MUSIC - goes to music submenu
        page.Items.Add(new MenuButton
        {
            Label = "MUSIC",
            OnClick = (menu, data) =>
            {
                var musicPage = CreateMusicMenu();
                menu.PushPage(musicPage);
            }
        });
        
        return page;
    }
    
    // ===== MUSIC MENU =====
    // Structure: MENU_VERTICAL centered, shows music track selection
    public static MenuPage CreateMusicMenu()
    {
        var page = new MenuPage
        {
            Title = "MUSIC",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsAnchor = UIAnchor.MiddleCenter
        };
        
        // TODO: Get actual music tracks from game data
        var musicTracks = new[] 
        {
            "TRACK 1", "TRACK 2", "TRACK 3", "TRACK 4", 
            "TRACK 5", "TRACK 6", "TRACK 7", "TRACK 8"
        };
        
        foreach (var track in musicTracks)
        {
            var trackName = track;
            page.Items.Add(new MenuButton
            {
                Label = trackName,
                OnClick = (menu, data) =>
                {
                    // TODO: Change music track
                    menu.PopPage();
                }
            });
        }
        
        // RANDOM option
        page.Items.Add(new MenuButton
        {
            Label = "RANDOM",
            OnClick = (menu, data) =>
            {
                // TODO: Enable random music mode
                menu.PopPage();
            }
        });
        
        return page;
    }
    
    // ===== RESTART CONFIRMATION =====
    // Structure: MENU_HORIZONTAL, confirmation dialog
    private static MenuPage CreateRestartConfirmation()
    {
        var page = new MenuPage
        {
            Title = "ARE YOU SURE YOU\nWANT TO RESTART",
            LayoutFlags = MenuLayoutFlags.Horizontal | MenuLayoutFlags.AlignCenter,
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsAnchor = UIAnchor.MiddleCenter
        };
        
        page.Items.Add(new MenuButton
        {
            Label = "YES",
            Data = 1,
            OnClick = (menu, data) =>
            {
                // TODO: Restart race
                if (GameStateRef != null)
                    GameStateRef.CurrentMode = GameMode.Racing;
            }
        });
        
        page.Items.Add(new MenuButton
        {
            Label = "NO",
            Data = 0,
            OnClick = (menu, data) =>
            {
                menu.PopPage();
            }
        });
        
        return page;
    }
    
    // ===== QUIT RACE CONFIRMATION =====
    // Structure: MENU_HORIZONTAL, confirmation dialog
    private static MenuPage CreateQuitRaceConfirmation()
    {
        var page = new MenuPage
        {
            Title = "ARE YOU SURE YOU\nWANT TO QUIT",
            LayoutFlags = MenuLayoutFlags.Horizontal | MenuLayoutFlags.AlignCenter,
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsAnchor = UIAnchor.MiddleCenter
        };
        
        page.Items.Add(new MenuButton
        {
            Label = "YES",
            Data = 1,
            OnClick = (menu, data) =>
            {
                if (GameStateRef != null)
                    GameStateRef.CurrentMode = GameMode.Menu;
                // Clear menu stack and return to main menu
                while (menu.CurrentPage != null)
                {
                    if (!menu.HandleInput(MenuAction.Back))
                        break;
                }
            }
        });
        
        page.Items.Add(new MenuButton
        {
            Label = "NO",
            Data = 0,
            OnClick = (menu, data) =>
            {
                menu.PopPage();
            }
        });
        
        return page;
    }

    // ===== GAME OVER MENU =====
    // Structure: MENU_VERTICAL centered, single empty button to quit
    public static MenuPage CreateGameOverMenu()
    {
        var page = new MenuPage
        {
            Title = "GAME OVER",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsAnchor = UIAnchor.MiddleCenter
        };
        
        // Empty button - just press Enter to continue
        page.Items.Add(new MenuButton
        {
            Label = "",
            OnClick = (menu, data) =>
            {
                if (GameStateRef != null)
                    GameStateRef.CurrentMode = GameMode.Menu;
            }
        });
        
        return page;
    }
    
    // ===== RACE STATS MENU =====
    // Structure: MENU_FIXED, shows race results after finishing
    // Title changes based on qualification status
    public static MenuPage CreateRaceStatsMenu(bool qualified, bool isTimeTrial, int position, List<float> lapTimes, float bestLap, string pilotName)
    {
        string title;
        if (isTimeTrial)
            title = "";
        else if (qualified)
            title = "CONGRATULATIONS";
        else
            title = "FAILED TO QUALIFY";
            
        var page = new MenuPage
        {
            Title = title,
            LayoutFlags = MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(0, -100),
            TitleAnchor = UIAnchor.MiddleCenter
        };
        
        // Custom draw function to display race stats
        page.DrawCallback = (renderer) =>
        {
            DrawRaceStats(renderer, qualified, isTimeTrial, position, lapTimes, bestLap, pilotName);
        };
        
        // Empty button - just press Enter to continue
        page.Items.Add(new MenuButton
        {
            Label = "",
            OnClick = (menu, data) =>
            {
                // Continue to next screen (points table or menu)
                menu.PopPage();
            }
        });
        
        return page;
    }
    
    private static void DrawRaceStats(IMenuRenderer renderer, bool qualified, bool isTimeTrial, int position, List<float> lapTimes, float bestLap, string pilotName)
    {
        int startY = 0;
        
        // Draw pilot portrait if not time trial
        // TODO: Draw pilot portrait texture here
        
        // Draw position
        if (!isTimeTrial)
        {
            string positionText = $"POSITION: {position}";
            Vec2i pos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(0, startY));
            UIHelper.DrawTextCentered(positionText, pos, FontSizes.MenuTitle, UIColor.Accent);
            startY += 30;
        }
        
        // Draw lap times
        Vec2i headerPos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(0, startY));
        UIHelper.DrawTextCentered("LAP TIMES", headerPos, FontSizes.MenuItem, UIColor.Default);
        startY += 20;
        
        for (int i = 0; i < lapTimes.Count; i++)
        {
            string lapText = $"LAP {i + 1}: {FormatTime(lapTimes[i])}";
            Vec2i lapPos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(0, startY));
            UIHelper.DrawTextCentered(lapText, lapPos, FontSizes.MenuItem, UIColor.Default);
            startY += 15;
        }
        
        // Draw total time
        float totalTime = lapTimes.Sum();
        startY += 10;
        Vec2i totalPos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(0, startY));
        UIHelper.DrawTextCentered($"TOTAL TIME: {FormatTime(totalTime)}", totalPos, FontSizes.MenuItem, UIColor.Accent);
        
        // Draw best lap
        startY += 20;
        Vec2i bestPos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(0, startY));
        UIHelper.DrawTextCentered($"BEST LAP: {FormatTime(bestLap)}", bestPos, FontSizes.MenuItem, UIColor.Accent);
    }
    
    // ===== RACE POINTS TABLE =====
    // Structure: MENU_FIXED, shows points awarded in race
    public static MenuPage CreateRacePointsMenu(List<(string pilotName, int points, bool isPlayer)> pilots)
    {
        var page = new MenuPage
        {
            Title = "RACE POINTS",
            LayoutFlags = MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(0, -100),
            TitleAnchor = UIAnchor.MiddleCenter
        };
        
        // Custom draw function
        page.DrawCallback = (renderer) =>
        {
            DrawPointsTable(renderer, pilots, "PILOT NAME", "POINTS");
        };
        
        // Empty button - just press Enter to continue
        page.Items.Add(new MenuButton
        {
            Label = "",
            OnClick = (menu, data) =>
            {
                menu.PopPage();
            }
        });
        
        return page;
    }
    
    // ===== CHAMPIONSHIP TABLE =====
    // Structure: MENU_FIXED, shows championship standings
    public static MenuPage CreateChampionshipTableMenu(List<(string pilotName, int points, bool isPlayer)> pilots)
    {
        var page = new MenuPage
        {
            Title = "CHAMPIONSHIP TABLE",
            LayoutFlags = MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(0, -100),
            TitleAnchor = UIAnchor.MiddleCenter
        };
        
        // Custom draw function
        page.DrawCallback = (renderer) =>
        {
            DrawPointsTable(renderer, pilots, "PILOT NAME", "POINTS");
        };
        
        // Empty button - just press Enter to continue
        page.Items.Add(new MenuButton
        {
            Label = "",
            OnClick = (menu, data) =>
            {
                menu.PopPage();
            }
        });
        
        return page;
    }
    
    private static void DrawPointsTable(IMenuRenderer renderer, List<(string pilotName, int points, bool isPlayer)> pilots, string header1, string header2)
    {
        int startY = -50;
        
        // Draw table header
        Vec2i headerPos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(-100, startY));
        UIHelper.DrawText(header1, headerPos, FontSizes.MenuItem, UIColor.Default);
        
        Vec2i pointsHeaderPos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(100, startY));
        UIHelper.DrawText(header2, pointsHeaderPos, FontSizes.MenuItem, UIColor.Default);
        
        startY += 25;
        
        // Draw each pilot
        foreach (var (pilotName, points, isPlayer) in pilots)
        {
            var color = isPlayer ? UIColor.Accent : UIColor.Default;
            
            Vec2i namePos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(-100, startY));
            UIHelper.DrawText(pilotName, namePos, FontSizes.MenuItem, color);
            
            Vec2i pointsPos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(100, startY));
            UIHelper.DrawText(points.ToString(), pointsPos, FontSizes.MenuItem, color);
            
            startY += 18;
        }
    }
    
    // ===== HALL OF FAME =====
    // Structure: MENU_FIXED, interactive character-by-character name entry
    private static string _hallOfFameName = "";
    private static int _hallOfFameCharIndex = 0;
    private static readonly char[] _hallOfFameChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
    
    public static MenuPage CreateHallOfFameMenu(int position, float time, List<(string name, float time)> existingEntries)
    {
        _hallOfFameName = "";
        _hallOfFameCharIndex = 0;
        
        var page = new MenuPage
        {
            Title = "HALL OF FAME",
            LayoutFlags = MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(0, -100),
            TitleAnchor = UIAnchor.MiddleCenter
        };
        
        // Custom draw function
        page.DrawCallback = (renderer) =>
        {
            DrawHallOfFame(renderer, position, time, existingEntries);
        };
        
        // Empty button for navigation
        page.Items.Add(new MenuButton
        {
            Label = "",
            OnClick = (menu, data) =>
            {
                // Cannot go back from Hall of Fame
            }
        });
        
        return page;
    }
    
    private static void DrawHallOfFame(IMenuRenderer renderer, int position, float time, List<(string name, float time)> existingEntries)
    {
        int startY = -80;
        
        // Show all 5 entries with new entry inserted at correct position
        var allEntries = new List<(string name, float time, bool isNew)>(existingEntries.Select(e => (e.name, e.time, false)));
        allEntries.Insert(position - 1, (_hallOfFameName.PadRight(3, '_'), time, true));
        
        for (int i = 0; i < Math.Min(5, allEntries.Count); i++)
        {
            var (name, entryTime, isNew) = allEntries[i];
            var color = isNew ? UIColor.Accent : UIColor.Default;
            
            string line = $"{i + 1}. {name,-3} {FormatTime(entryTime)}";
            Vec2i pos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(0, startY));
            UIHelper.DrawTextCentered(line, pos, FontSizes.MenuItem, color);
            startY += 20;
        }
        
        // Draw character selector
        startY += 40;
        
        // Show current character being selected
        if (_hallOfFameName.Length < 3)
        {
            Vec2i charPos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(0, startY));
            UIHelper.DrawTextCentered(_hallOfFameChars[_hallOfFameCharIndex].ToString(), charPos, FontSizes.MenuTitle, UIColor.Accent);
            
            startY += 30;
            Vec2i instructionPos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(0, startY));
            UIHelper.DrawTextCentered("UP/DOWN: SELECT  ENTER: CONFIRM", instructionPos, FontSizes.MenuItem, UIColor.Default);
        }
        else
        {
            Vec2i donePos = UIHelper.ScaledPos(UIAnchor.MiddleCenter, new Vec2i(0, startY));
            UIHelper.DrawTextCentered("PRESS ENTER TO CONTINUE", donePos, FontSizes.MenuItem, UIColor.Accent);
        }
    }
    
    // Hall of Fame input handling (call from Game.cs)
    public static void HandleHallOfFameInput(HallOfFameAction action)
    {
        switch (action)
        {
            case HallOfFameAction.Up:
                _hallOfFameCharIndex = (_hallOfFameCharIndex - 1 + _hallOfFameChars.Length) % _hallOfFameChars.Length;
                break;
            case HallOfFameAction.Down:
                _hallOfFameCharIndex = (_hallOfFameCharIndex + 1) % _hallOfFameChars.Length;
                break;
            case HallOfFameAction.Confirm:
                if (_hallOfFameName.Length < 3)
                {
                    _hallOfFameName += _hallOfFameChars[_hallOfFameCharIndex];
                    _hallOfFameCharIndex = 0;
                }
                break;
            case HallOfFameAction.Delete:
                if (_hallOfFameName.Length > 0)
                    _hallOfFameName = _hallOfFameName.Substring(0, _hallOfFameName.Length - 1);
                break;
        }
    }
    
    // ===== TEXT SCROLL MENU (for credits) =====
    // Structure: Auto-scrolling text with '#' prefix for headers
    
    public static MenuPage CreateTextScrollMenu(string[] lines, string title = "")
    {
        var page = new MenuPage
        {
            Title = title,
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.AlignCenter,
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsAnchor = UIAnchor.MiddleCenter
        };
        
        // Custom draw function for auto-scrolling credits
        page.DrawCallback = (renderer) =>
        {
            DrawTextScroll(renderer, lines);
        };
        
        // Empty button
        page.Items.Add(new MenuButton
        {
            Label = "",
            OnClick = (menu, data) =>
            {
                menu.PopPage();
            }
        });
        
        return page;
    }
    
    private static void DrawTextScroll(IMenuRenderer renderer, string[] lines)
    {
        // TODO: Get actual elapsed time from somewhere
        float elapsedTime = 0; // This should come from system time
        
        int uiScale = 2;
        int speed = 32;
        float scrollY = elapsedTime * uiScale * speed;
        
        // Get screen dimensions
        // TODO: Get actual screen height
        int screenHeight = 720; // Default assumption
        
        float y = screenHeight - scrollY;
        
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line))
            {
                y += 12 * uiScale;
                continue;
            }
            
            if (line.StartsWith("#"))
            {
                y += 48 * uiScale;
                
                // Draw title (remove # prefix)
                string titleText = line.Substring(1);
                Vec2i pos = new Vec2i(screenHeight / 2, (int)y);
                UIHelper.DrawTextCentered(titleText, pos, FontSizes.CreditsTitle, UIColor.Accent);
                
                y += 32 * uiScale;
            }
            else
            {
                // Draw normal text
                Vec2i pos = new Vec2i(screenHeight / 2, (int)y);
                UIHelper.DrawTextCentered(line, pos, FontSizes.CreditsText, UIColor.Default);
                
                y += 12 * uiScale;
            }
        }
    }

    // ===== VIDEO OPTIONS MENU (deprecated - alias for CreateVideoMenu) =====
    public static MenuPage CreateVideoOptionsMenu()
    {
        return CreateVideoMenu();
    }

    // ===== AUDIO OPTIONS MENU (deprecated - alias for CreateAudioMenu) =====
    public static MenuPage CreateAudioOptionsMenu()
    {
        return CreateAudioMenu();
    }
}

/// <summary>
/// Actions for Hall of Fame character entry
/// </summary>
public enum HallOfFameAction
{
    Up,
    Down,
    Confirm,
    Delete
}
