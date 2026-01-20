using OpenTK.Windowing.GraphicsLibraryFramework;
using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Presentation;

/// <summary>
/// Pure state machine logic for game mode transitions.
/// No side effects, no dependencies - 100% testable.
/// </summary>
public static class GameStateTransitions
{
    public enum TransitionResult
    {
        NoChange,
        TransitionTo,
        PopMenu,
        PushMenu,
    }

    public static (GameMode nextMode, TransitionResult action) GetNextMode(
        GameMode currentMode,
        bool menuSelectPressed,
        bool menuBackPressed,
        bool menuUpPressed,
        bool menuDownPressed,
        bool exitPressed,
        bool anyKeyDown,
        bool attractTimeElapsed,
        float timeSinceAttractTrigger)
    {
        // Intro: Can skip with MenuSelect or End on its own
        if (currentMode == GameMode.Intro)
        {
            if (menuSelectPressed)
                return (GameMode.SplashScreen, TransitionResult.TransitionTo);
            return (GameMode.Intro, TransitionResult.NoChange);
        }

        // SplashScreen: MenuSelect -> Menu, attract timeout -> AttractMode
        if (currentMode == GameMode.SplashScreen)
        {
            if (menuSelectPressed)
                return (GameMode.Menu, TransitionResult.TransitionTo);
            
            if (attractTimeElapsed)
                return (GameMode.AttractMode, TransitionResult.TransitionTo);
            
            return (GameMode.SplashScreen, TransitionResult.NoChange);
        }

        // AttractMode: Any key -> back to SplashScreen
        if (currentMode == GameMode.AttractMode)
        {
            if (anyKeyDown)
                return (GameMode.SplashScreen, TransitionResult.TransitionTo);
            
            return (GameMode.AttractMode, TransitionResult.NoChange);
        }

        // Menu: ESC/MenuBack -> pop or return to splash
        if (currentMode == GameMode.Menu)
        {
            if (menuBackPressed)
                return (GameMode.SplashScreen, TransitionResult.PopMenu);
            
            if (exitPressed)
                return (GameMode.SplashScreen, TransitionResult.PopMenu);
            
            return (GameMode.Menu, TransitionResult.NoChange);
        }

        return (currentMode, TransitionResult.NoChange);
    }

    /// <summary>
    /// Determine if UI scale needs recalculation based on screen height.
    /// Formula from original C code: scale = max(1, sh >= 720 ? sh / 360 : sh / 240)
    /// </summary>
    public static int CalculateAutoUIScale(int screenHeight, int manualScale)
    {
        int autoScale = Math.Max(1, screenHeight >= 720 ? screenHeight / 360 : screenHeight / 240);
        
        // If user has set manual scale (not 0), cap the auto scale
        if (manualScale > 0)
            return Math.Min(manualScale, autoScale);
        
        return autoScale;
    }

    /// <summary>
    /// Determine if we should check best times viewer input (uses different input handling).
    /// </summary>
    public static bool IsBestTimesViewerMode(string? pageTitle)
    {
        if (string.IsNullOrEmpty(pageTitle))
            return false;
        
        return pageTitle.Contains("BEST TIME TRIAL TIMES") ||
               pageTitle.Contains("BEST RACE TIMES");
    }
}
