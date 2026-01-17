using Xunit;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Presentation;

namespace WipeoutRewrite.Tests.Presentation;

public class GameStateTransitionsTests
{
    [Fact]
    public void GetNextMode_IntroWithMenuSelect_TransitionsToSplashScreen()
    {
        // Act
        var (nextMode, action) = GameStateTransitions.GetNextMode(
            currentMode: GameMode.Intro,
            menuSelectPressed: true,
            menuBackPressed: false,
            menuUpPressed: false,
            menuDownPressed: false,
            exitPressed: false,
            anyKeyDown: false,
            attractTimeElapsed: false,
            timeSinceAttractTrigger: 0);

        // Assert
        Assert.Equal(GameMode.SplashScreen, nextMode);
        Assert.Equal(GameStateTransitions.TransitionResult.TransitionTo, action);
    }

    [Fact]
    public void GetNextMode_IntroWithoutInput_StaysInIntro()
    {
        // Act
        var (nextMode, action) = GameStateTransitions.GetNextMode(
            currentMode: GameMode.Intro,
            menuSelectPressed: false,
            menuBackPressed: false,
            menuUpPressed: false,
            menuDownPressed: false,
            exitPressed: false,
            anyKeyDown: false,
            attractTimeElapsed: false,
            timeSinceAttractTrigger: 0);

        // Assert
        Assert.Equal(GameMode.Intro, nextMode);
        Assert.Equal(GameStateTransitions.TransitionResult.NoChange, action);
    }

    [Fact]
    public void GetNextMode_SplashScreenWithMenuSelect_TransitionsToMenu()
    {
        // Act
        var (nextMode, action) = GameStateTransitions.GetNextMode(
            currentMode: GameMode.SplashScreen,
            menuSelectPressed: true,
            menuBackPressed: false,
            menuUpPressed: false,
            menuDownPressed: false,
            exitPressed: false,
            anyKeyDown: false,
            attractTimeElapsed: false,
            timeSinceAttractTrigger: 0);

        // Assert
        Assert.Equal(GameMode.Menu, nextMode);
        Assert.Equal(GameStateTransitions.TransitionResult.TransitionTo, action);
    }

    [Fact]
    public void GetNextMode_SplashScreenWithAttractTimeout_TransitionsToAttractMode()
    {
        // Act
        var (nextMode, action) = GameStateTransitions.GetNextMode(
            currentMode: GameMode.SplashScreen,
            menuSelectPressed: false,
            menuBackPressed: false,
            menuUpPressed: false,
            menuDownPressed: false,
            exitPressed: false,
            anyKeyDown: false,
            attractTimeElapsed: true,
            timeSinceAttractTrigger: 10);

        // Assert
        Assert.Equal(GameMode.AttractMode, nextMode);
        Assert.Equal(GameStateTransitions.TransitionResult.TransitionTo, action);
    }

    [Fact]
    public void GetNextMode_AttractModeWithAnyKey_TransitionsToSplashScreen()
    {
        // Act
        var (nextMode, action) = GameStateTransitions.GetNextMode(
            currentMode: GameMode.AttractMode,
            menuSelectPressed: false,
            menuBackPressed: false,
            menuUpPressed: false,
            menuDownPressed: false,
            exitPressed: false,
            anyKeyDown: true,
            attractTimeElapsed: false,
            timeSinceAttractTrigger: 0);

        // Assert
        Assert.Equal(GameMode.SplashScreen, nextMode);
        Assert.Equal(GameStateTransitions.TransitionResult.TransitionTo, action);
    }

    [Fact]
    public void GetNextMode_MenuWithMenuBack_PopMenu()
    {
        // Act
        var (nextMode, action) = GameStateTransitions.GetNextMode(
            currentMode: GameMode.Menu,
            menuSelectPressed: false,
            menuBackPressed: true,
            menuUpPressed: false,
            menuDownPressed: false,
            exitPressed: false,
            anyKeyDown: false,
            attractTimeElapsed: false,
            timeSinceAttractTrigger: 0);

        // Assert
        Assert.Equal(GameMode.SplashScreen, nextMode);
        Assert.Equal(GameStateTransitions.TransitionResult.PopMenu, action);
    }

    [Fact]
    public void GetNextMode_MenuWithExit_PopMenu()
    {
        // Act
        var (nextMode, action) = GameStateTransitions.GetNextMode(
            currentMode: GameMode.Menu,
            menuSelectPressed: false,
            menuBackPressed: false,
            menuUpPressed: false,
            menuDownPressed: false,
            exitPressed: true,
            anyKeyDown: false,
            attractTimeElapsed: false,
            timeSinceAttractTrigger: 0);

        // Assert
        Assert.Equal(GameMode.SplashScreen, nextMode);
        Assert.Equal(GameStateTransitions.TransitionResult.PopMenu, action);
    }

    [Fact]
    public void CalculateAutoUIScale_With720Height_ReturnsCorrectScale()
    {
        // Act
        int scale = GameStateTransitions.CalculateAutoUIScale(screenHeight: 720, manualScale: 0);

        // Assert
        Assert.Equal(2, scale); // 720 / 360 = 2
    }

    [Fact]
    public void CalculateAutoUIScale_With480Height_ReturnsCorrectScale()
    {
        // Act
        int scale = GameStateTransitions.CalculateAutoUIScale(screenHeight: 480, manualScale: 0);

        // Assert
        Assert.Equal(2, scale); // 480 / 240 = 2
    }

    [Fact]
    public void CalculateAutoUIScale_With1440Height_ReturnsCorrectScale()
    {
        // Act
        int scale = GameStateTransitions.CalculateAutoUIScale(screenHeight: 1440, manualScale: 0);

        // Assert
        Assert.Equal(4, scale); // 1440 / 360 = 4
    }

    [Fact]
    public void CalculateAutoUIScale_WithManualScale_CapsToAuto()
    {
        // Act - manual scale 5 but auto would be 2
        int scale = GameStateTransitions.CalculateAutoUIScale(screenHeight: 720, manualScale: 5);

        // Assert
        Assert.Equal(2, scale); // Min(5, 2) = 2
    }

    [Fact]
    public void CalculateAutoUIScale_WithLowerManualScale_UsesManual()
    {
        // Act - manual scale 1 but auto would be 2
        int scale = GameStateTransitions.CalculateAutoUIScale(screenHeight: 720, manualScale: 1);

        // Assert
        Assert.Equal(1, scale); // Manual is lower, use it
    }

    [Fact]
    public void IsBestTimesViewerMode_WithBestTimeTrialTitle_ReturnsTrue()
    {
        // Act
        bool result = GameStateTransitions.IsBestTimesViewerMode("BEST TIME TRIAL TIMES");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsBestTimesViewerMode_WithBestRaceTitle_ReturnsTrue()
    {
        // Act
        bool result = GameStateTransitions.IsBestTimesViewerMode("BEST RACE TIMES");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsBestTimesViewerMode_WithOtherTitle_ReturnsFalse()
    {
        // Act
        bool result = GameStateTransitions.IsBestTimesViewerMode("MAIN MENU");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsBestTimesViewerMode_WithNull_ReturnsFalse()
    {
        // Act
        bool result = GameStateTransitions.IsBestTimesViewerMode(null);

        // Assert
        Assert.False(result);
    }
}
