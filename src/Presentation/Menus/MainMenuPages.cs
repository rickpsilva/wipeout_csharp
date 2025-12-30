using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Core.Entities;
using static WipeoutRewrite.Infrastructure.UI.UIConstants;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Presentation;

namespace WipeoutRewrite.Presentation.Menus;

public static class MainMenuPages
{
    private static readonly string[] ToggleOffOn = new[] { "OFF", "ON" };
    private static readonly string[] VolumeOptions = new[] { "0", "10", "20", "30", "40", "50", "60", "70", "80", "90", "100" };
    // Store references for menu callbacks
    public static GameState? GameStateRef { get; set; }
    public static IContentPreview3D? ContentPreview3DRef { get; set; }
    
    public static MenuPage CreateMainMenu()
    {
        var page = new MenuPage
        {
            Title = Strings.MenuOptions,
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, 120),
            ItemsAnchor = UIAnchor.MiddleCenter
        };
        
        // START GAME button
        page.Items.Add(new MenuButton
        {
            Label = Strings.MenuStartGame,
            Data = 0,
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 7),
            OnClick = (menu, data) =>
            {
                var raceClassPage = CreateRaceClassMenu();
                menu.PushPage(raceClassPage);
            }
        });
        
        // OPTIONS button
        page.Items.Add(new MenuButton
        {
            Label = Strings.MenuOptions,
            Data = 1,
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryMsDos), 3),
            OnClick = (menu, data) =>
            {
                var optionsPage = CreateOptionsMenu();
                menu.PushPage(optionsPage);
            }
        });
        
        // QUIT button (not available on web/emscripten)
        page.Items.Add(new MenuButton
        {
            Label = Strings.MenuQuit,
            Data = 2,
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryMsDos), 1),
            OnClick = (menu, data) =>
            {
                var confirmPage = CreateQuitConfirmation();
                menu.PushPage(confirmPage);
            }
        });
        
        // DrawCallback removed - preview 3D will be controlled by Game.cs
        // only when we are on the main menu page
        
        return page;
    }
    
    public static MenuPage CreateQuitConfirmation()
    {
        var page = new MenuPage
        {
            Title = Strings.QuitTitle,
            LayoutFlags = MenuLayoutFlags.Horizontal | MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(0, 0),
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsPos = new Vec2i(0, 50),
            ItemsAnchor = UIAnchor.MiddleCenter
        };
        
        page.Items.Add(new MenuButton
        {
            Label = Strings.QuitYes,
            OnClick = (menu, data) =>
            {
                Environment.Exit(0);
            }
        });
        
        page.Items.Add(new MenuButton
        {
            Label = Strings.QuitNo,
            OnClick = (menu, data) =>
            {
                menu.PopPage();
            }
        });
        
        return page;
    }
    
    public static MenuPage CreateOptionsMenu()
    {
        var page = new MenuPage
        {
            Title = "OPTIONS",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        page.Items.Add(new MenuButton
        {
            Label = "CONTROLS",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 0),
            OnClick = (menu, data) =>
            {
                // TODO: Create controls page
            }
        });
        
        page.Items.Add(new MenuButton
        {
            Label = "VIDEO",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 1),
            OnClick = (menu, data) =>
            {
                var videoPage = CreateVideoOptionsMenu();
                menu.PushPage(videoPage);
            }
        });
        
        page.Items.Add(new MenuButton
        {
            Label = "AUDIO",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 2),
            OnClick = (menu, data) =>
            {
                var audioPage = CreateAudioOptionsMenu();
                menu.PushPage(audioPage);
            }
        });
        
        page.Items.Add(new MenuButton
        {
            Label = "BEST TIMES",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 3),
            OnClick = (menu, data) =>
            {
                // TODO: Create best times page
            }
        });
        
        return page;
    }
    
    public static MenuPage CreateVideoOptionsMenu()
    {
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
        
            page.Items.Add(new MenuToggle
        {
            Label = "FULLSCREEN",
            Options = ToggleOffOn,
            CurrentIndex = 0,
            OnChange = (menu, value) =>
            {
                // TODO: Toggle fullscreen
                Console.WriteLine($"Fullscreen: {value}");
            }
        });
        
        page.Items.Add(new MenuToggle
        {
            Label = "SHOW FPS",
            Options = ToggleOffOn,
            CurrentIndex = 0,
            OnChange = (menu, value) =>
            {
                // TODO: Toggle FPS display
                Console.WriteLine($"Show FPS: {value}");
            }
        });
        
        return page;
    }
    
    public static MenuPage CreateAudioOptionsMenu()
    {
        var page = new MenuPage
        {
            Title = "AUDIO OPTIONS",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(-160, -100),
            TitleAnchor = UIAnchor.MiddleCenter,
            ItemsPos = new Vec2i(-160, -80),
            ItemsAnchor = UIAnchor.MiddleCenter,
            BlockWidth = 320
        };
        
        // Use shared static array to avoid allocating a new array each time
        string[] volumeOptions = VolumeOptions;
        
        page.Items.Add(new MenuToggle
        {
            Label = "MUSIC VOLUME",
            Options = volumeOptions,
            CurrentIndex = 10,
            OnChange = (menu, value) =>
            {
                float volume = value * 0.1f;
                Console.WriteLine($"Music volume: {volume}");
            }
        });
        
        page.Items.Add(new MenuToggle
        {
            Label = "SOUND EFFECTS VOLUME",
            Options = volumeOptions,
            CurrentIndex = 10,
            OnChange = (menu, value) =>
            {
                float volume = value * 0.1f;
                Console.WriteLine($"SFX volume: {volume}");
            }
        });
        
        return page;
    }
    
    public static MenuPage CreateRaceClassMenu()
    {
        var page = new MenuPage
        {
            Title = "SELECT RACING CLASS",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        page.Items.Add(new MenuButton
        {
            Label = "VENOM CLASS",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 4),
            OnClick = (menu, data) =>
            {
                var raceTypePage = CreateRaceTypeMenu();
                menu.PushPage(raceTypePage);
            }
        });
        
        page.Items.Add(new MenuButton
        {
            Label = "RAPIER CLASS",
            IsEnabled = false, // Locked until unlocked
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 5),
            OnClick = (menu, data) =>
            {
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
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        page.Items.Add(new MenuButton
        {
            Label = "SINGLE RACE",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 6),
            OnClick = (menu, data) =>
            {
                var teamPage = CreateTeamMenu();
                menu.PushPage(teamPage);
            }
        });
        
        page.Items.Add(new MenuButton
        {
            Label = "TIME TRIAL",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 7),
            OnClick = (menu, data) =>
            {
                var teamPage = CreateTeamMenu();
                menu.PushPage(teamPage);
            }
        });
        
        return page;
    }
    
    public static MenuPage CreateTeamMenu()
    {
        var page = new MenuPage
        {
            Title = "SELECT YOUR TEAM",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        string[] teams = new[] { "FEISAR", "GOTEKI 45", "AG SYSTEMS", "AURICOM" };
        
        for (int i = 0; i < teams.Length; i++)
        {
            var team = teams[i];
            int shipIndex = i; // Capture index for ship model
            page.Items.Add(new MenuButton
            {
                Label = team,
                PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), shipIndex),
                OnClick = (menu, data) =>
                {
                    var pilotPage = CreatePilotMenu(team);
                    menu.PushPage(pilotPage);
                }
            });
        }
        
        return page;
    }
    
    public static MenuPage CreatePilotMenu(string team)
    {
        var page = new MenuPage
        {
            Title = "CHOOSE YOUR PILOT",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -110),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        // Simplified: just add 2 pilots per team
        page.Items.Add(new MenuButton
        {
            Label = $"{team} PILOT 1",
            OnClick = (menu, data) =>
            {
                var circuitPage = CreateCircuitMenu();
                menu.PushPage(circuitPage);
            }
        });
        
        page.Items.Add(new MenuButton
        {
            Label = $"{team} PILOT 2",
            OnClick = (menu, data) =>
            {
                var circuitPage = CreateCircuitMenu();
                menu.PushPage(circuitPage);
            }
        });
        
        return page;
    }
    
    public static MenuPage CreateCircuitMenu()
    {
        var page = new MenuPage
        {
            Title = "SELECT RACING CIRCUIT",
            LayoutFlags = MenuLayoutFlags.Vertical | MenuLayoutFlags.Fixed,
            TitlePos = new Vec2i(0, 30),
            TitleAnchor = UIAnchor.TopCenter,
            ItemsPos = new Vec2i(0, -100),
            ItemsAnchor = UIAnchor.BottomCenter
        };
        
        string[] circuits = new[]
        {
            "ALTIMA VII",
            "KARBONIS V",
            "TERRAMAX",
            "KORODERA",
            "ARRIDOS IV",
            "SILVERSTREAM",
            "FIRESTAR"
        };
        
        foreach (var circuit in circuits)
        {
            string circuitName = circuit; // Capture for closure
            page.Items.Add(new MenuButton
            {
                Label = circuit,
                OnClick = (menu, data) =>
                {
                    // Start race
                    if (GameStateRef != null)
                    {
                        GameStateRef.CurrentMode = GameMode.Racing;
                        Console.WriteLine($"Starting race on {circuitName}");
                        // TODO: Initialize race with selected circuit
                    }
                }
            });
        }
        
        return page;
    }
}
