using Xunit;
using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Tests.Core.Services;

public class ControlsSettingsTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var settings = new ControlsSettings();

        // Check that all 9 game actions have keyboard bindings
        for (int i = 0; i < 9; i++)
        {
            var action = (RaceAction)i;
            uint keyboardBinding = settings.GetButtonBinding(action, InputDevice.Keyboard);
            Assert.NotEqual(0u, keyboardBinding);
        }
    }

    [Fact]
    public void IsValid_WithDefaultValues_ReturnsTrue()
    {
        var settings = new ControlsSettings();

        Assert.True(settings.IsValid());
    }

    [Fact]
    public void GetButtonBinding_ReturnsKeyboardBindings()
    {
        var settings = new ControlsSettings();

        // UP action should have keyboard binding
        uint upKeyboard = settings.GetButtonBinding(RaceAction.Up, InputDevice.Keyboard);
        Assert.Equal(82u, upKeyboard); // SDL_SCANCODE_UP
    }

    [Fact]
    public void SetButtonBinding_UpdatesKeyboardBinding()
    {
        var settings = new ControlsSettings();

        settings.SetButtonBinding(RaceAction.Up, InputDevice.Keyboard, 100u);

        uint binding = settings.GetButtonBinding(RaceAction.Up, InputDevice.Keyboard);
        Assert.Equal(100u, binding);
    }

    [Fact]
    public void ResetToDefaults_RestoresDefaultKeyBindings()
    {
        var settings = new ControlsSettings();

        // Change a binding
        settings.SetButtonBinding(RaceAction.Up, InputDevice.Keyboard, 999u);

        // Reset
        settings.ResetToDefaults();

        // Check it's restored
        uint binding = settings.GetButtonBinding(RaceAction.Up, InputDevice.Keyboard);
        Assert.Equal(82u, binding); // SDL_SCANCODE_UP
    }

    [Theory]
    [InlineData(RaceAction.Up)]
    [InlineData(RaceAction.Down)]
    [InlineData(RaceAction.Left)]
    [InlineData(RaceAction.Right)]
    [InlineData(RaceAction.BrakeLeft)]
    [InlineData(RaceAction.BrakeRight)]
    [InlineData(RaceAction.Thrust)]
    [InlineData(RaceAction.Fire)]
    [InlineData(RaceAction.ChangeView)]
    public void AllGameActions_HaveValidDefaultBindings(RaceAction action)
    {
        var settings = new ControlsSettings();

        uint keyboardBinding = settings.GetButtonBinding(action, InputDevice.Keyboard);
        Assert.NotEqual(0u, keyboardBinding);
        Assert.True(settings.IsValid());
    }
}
