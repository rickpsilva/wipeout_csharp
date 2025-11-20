using OpenTK.Mathematics;

namespace WipeoutRewrite.Infrastructure.UI;

/// <summary>
/// Centralized constants for UI (fonts, colors, spacing).
/// Facilitates maintenance and future translations/localizations.
/// </summary>
public static class UIConstants
{
    // ===== FONT SIZES =====
    public static class FontSizes
    {
        public const int MenuTitle = 16;
        public const int MenuItem = 16;
        public const int SplashText = 16;
        public const int Credits = 16;
        public const int CreditsTitle = 16;
    }
    
    // ===== COLORS =====
    public static class Colors
    {
        // Menu colors
        public static readonly Color4 MenuTitleDefault = new(1.0f, 1.0f, 1.0f, 1.0f);  // White
        public static readonly Color4 MenuItemDefault = new(1.0f, 1.0f, 1.0f, 1.0f);   // White
        public static readonly Color4 MenuItemSelected = new(1.0f, 0.8f, 0.0f, 1.0f);  // Yellow/Gold
        public static readonly Color4 MenuItemDisabled = new(0.5f, 0.5f, 0.5f, 1.0f);  // Gray
        
        // Splash screen
        public static readonly Color4 SplashText = new(0.5f, 0.5f, 0.5f, 1.0f);  // Gray
        public static readonly Color4 SplashTextYellow = new(1.0f, 0.8f, 0.0f, 1.0f);  // Yellow (blink state)
        
        // Credits
        public static readonly Color4 CreditsTitle = new(1.0f, 1.0f, 1.0f, 1.0f);  // White
        public static readonly Color4 CreditsText = new(0.7f, 0.7f, 0.7f, 1.0f);   // Light Gray
    }
    
    // ===== SPACING =====
    public static class Spacing
    {
        public const int MenuTitleLineHeight = 24;
        public const int MenuItemVerticalSpacing = 24;
        public const int MenuItemHorizontalSpacing = 80;
        public const int CreditsLineHeight = 30;
    }
    
    // ===== TEXT STRINGS (para futura i18n) =====
    public static class Strings
    {
        // Splash Screen
        public const string SplashPressEnter = "PRESS ENTER";
        
        // Main Menu
        public const string MenuStartGame = "START GAME";
        public const string MenuOptions = "OPTIONS";
        public const string MenuQuit = "QUIT";
        
        // Quit Confirmation
        public const string QuitTitle = "ARE YOU SURE YOU\nWANT TO QUIT";
        public const string QuitYes = "YES";
        public const string QuitNo = "NO";
        
        // Race Class
        public const string RaceClassTitle = "SELECT RACE CLASS";
        public const string RaceClassVenom = "VENOM";
        public const string RaceClassRapier = "RAPIER";
        
        // Race Type
        public const string RaceTypeTitle = "SELECT RACE TYPE";
        public const string RaceTypeSingleRace = "SINGLE RACE";
        public const string RaceTypeChampionship = "CHAMPIONSHIP";
        public const string RaceTypeTimeTrials = "TIME TRIALS";
        
        // Options
        public const string OptionsTitle = "OPTIONS";
        public const string OptionsControls = "CONTROLS";
        public const string OptionsVideo = "VIDEO";
        public const string OptionsAudio = "AUDIO";
        public const string OptionsBestTimes = "BEST TIMES";
        public const string OptionsBack = "BACK";
        
        // Video Options
        public const string VideoTitle = "VIDEO OPTIONS";
        public const string VideoFullscreen = "FULLSCREEN";
        public const string VideoResolution = "RESOLUTION";
        public const string VideoVSync = "VSYNC";
        
        // Audio Options
        public const string AudioTitle = "AUDIO OPTIONS";
        public const string AudioMusicVolume = "MUSIC VOLUME";
        public const string AudioSfxVolume = "SFX VOLUME";
        public const string AudioMusicMode = "MUSIC MODE";
        
        // Music Modes
        public const string MusicModeRandom = "RANDOM";
        public const string MusicModeSequential = "SEQUENTIAL";
        public const string MusicModeLoop = "LOOP";
        
        // Teams
        public const string TeamAGSystems = "AG SYSTEMS";
        public const string TeamAuricom = "AURICOM";
        public const string TeamQirex = "QIREX";
        public const string TeamFeisar = "FEISAR";
        
        // Circuits
        public const string CircuitAltima = "ALTIMA VII";
        public const string CircuitKarbonis = "KARBONIS V";
        public const string CircuitTerramax = "TERRAMAX";
        public const string CircuitKorodera = "KORODERA";
        public const string CircuitArridos = "ARRIDOS IV";
        public const string CircuitSilverstream = "SILVERSTREAM";
        public const string CircuitFirestar = "FIRESTAR";
        
        // Credits
        public static readonly string[] CreditsLines = 
        {
            "",
            "",
            "WIPEOUT",
            "",
            "ORIGINAL GAME",
            "PSYGNOSIS 1995",
            "",
            "",
            "C# REWRITE",
            "",
            "PROGRAMMING",
            "COMMUNITY PROJECT",
            "",
            "",
            "GRAPHICS",
            "THE DESIGNERS REPUBLIC",
            "",
            "",
            "MUSIC",
            "COLD STORAGE",
            "ORBITAL",
            "LEFTFIELD",
            "CHEMICAL BROTHERS",
            "",
            "",
            "SPECIAL THANKS",
            "DOMINIC SZABLEWSKI",
            "PHOBOSLAB",
            "",
            "",
            "",
            "",
            ""
        };
        
        public static readonly string[] CreditsTitles = 
        {
            "WIPEOUT",
            "ORIGINAL GAME",
            "C# REWRITE",
            "PROGRAMMING",
            "GRAPHICS",
            "MUSIC",
            "SPECIAL THANKS"
        };
    }
}
