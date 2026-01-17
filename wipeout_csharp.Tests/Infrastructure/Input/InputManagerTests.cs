using Xunit;
using WipeoutRewrite.Infrastructure.Input;

namespace WipeoutRewrite.Tests.Infrastructure.Input;

public class InputManagerTests
{
    [Fact]
    public void IsActionPressed_WithNullKeyboard_ReturnsFalse()
    {
        // Act
        var result = InputManager.IsActionPressed(GameAction.Accelerate, null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GameAction_HasAccelerateAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.Accelerate));
    }

    [Fact]
    public void GameAction_HasBrakeAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.Brake));
    }

    [Fact]
    public void GameAction_HasTurnLeftAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.TurnLeft));
    }

    [Fact]
    public void GameAction_HasTurnRightAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.TurnRight));
    }

    [Fact]
    public void GameAction_HasBoostLeftAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.BoostLeft));
    }

    [Fact]
    public void GameAction_HasBoostRightAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.BoostRight));
    }

    [Fact]
    public void GameAction_HasWeaponFireAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.WeaponFire));
    }

    [Fact]
    public void GameAction_HasWeaponCycleAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.WeaponCycle));
    }

    [Fact]
    public void GameAction_HasPauseAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.Pause));
    }

    [Fact]
    public void GameAction_HasExitAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.Exit));
    }

    [Fact]
    public void GameAction_HasMenuUpAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.MenuUp));
    }

    [Fact]
    public void GameAction_HasMenuDownAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.MenuDown));
    }

    [Fact]
    public void GameAction_HasMenuLeftAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.MenuLeft));
    }

    [Fact]
    public void GameAction_HasMenuRightAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.MenuRight));
    }

    [Fact]
    public void GameAction_HasMenuSelectAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.MenuSelect));
    }

    [Fact]
    public void GameAction_HasMenuBackAction()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(GameAction), GameAction.MenuBack));
    }

    [Fact]
    public void GameAction_HasExactly16Actions()
    {
        // Assert
        var actions = Enum.GetValues(typeof(GameAction)).Cast<GameAction>().ToList();
        Assert.Equal(16, actions.Count);
    }

    [Fact]
    public void RemapKey_WithValidAction_CanBeExecuted()
    {
        // Act
        // This tests that the method exists and can be called
        InputManager.RemapKey(GameAction.Accelerate, OpenTK.Windowing.GraphicsLibraryFramework.Keys.W);

        // Assert - if no exception, test passes
        Assert.True(true);
    }

    [Fact]
    public void RemapKey_WithMultipleActions_CanAllBeRemapped()
    {
        // Act - Test multiple remaps don't throw
        InputManager.RemapKey(GameAction.Accelerate, OpenTK.Windowing.GraphicsLibraryFramework.Keys.W);
        InputManager.RemapKey(GameAction.Brake, OpenTK.Windowing.GraphicsLibraryFramework.Keys.S);
        InputManager.RemapKey(GameAction.TurnLeft, OpenTK.Windowing.GraphicsLibraryFramework.Keys.A);
        InputManager.RemapKey(GameAction.TurnRight, OpenTK.Windowing.GraphicsLibraryFramework.Keys.D);

        // Assert
        Assert.True(true);
    }

    [Theory]
    [InlineData(GameAction.Accelerate)]
    [InlineData(GameAction.Brake)]
    [InlineData(GameAction.TurnLeft)]
    [InlineData(GameAction.TurnRight)]
    public void RemapKey_WithEachGameAction_DoesNotThrow(GameAction action)
    {
        // Act & Assert
        InputManager.RemapKey(action, OpenTK.Windowing.GraphicsLibraryFramework.Keys.A);
    }

    [Theory]
    [InlineData(GameAction.MenuUp)]
    [InlineData(GameAction.MenuDown)]
    [InlineData(GameAction.MenuLeft)]
    [InlineData(GameAction.MenuRight)]
    [InlineData(GameAction.MenuSelect)]
    [InlineData(GameAction.MenuBack)]
    public void MenuActions_CanBeRemapped(GameAction menuAction)
    {
        // Act & Assert
        InputManager.RemapKey(menuAction, OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter);
    }
}
