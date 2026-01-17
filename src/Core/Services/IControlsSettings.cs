namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Enum for race game actions (mirroring wipeout-rewrite action_t).
/// </summary>
public enum RaceAction
{
    Up,
    Down,
    Left,
    Right,
    BrakeLeft,
    BrakeRight,
    Thrust,
    Fire,
    ChangeView,
    MaxActions = ChangeView + 1
}

/// <summary>
/// Enum for input device types.
/// </summary>
public enum InputDevice
{
    Keyboard = 0,
    Joystick = 1
}

/// <summary>
/// Defines the interface for control settings management.
/// Handles button mappings for game actions (keyboard and joystick bindings).
/// </summary>
public interface IControlsSettings
{
    /// <summary>
    /// Gets or sets the button binding for a specific action and device.
    /// buttons[action][device] where device: 0=Keyboard, 1=Joystick
    /// </summary>
    uint GetButtonBinding(RaceAction action, InputDevice device);
    void SetButtonBinding(RaceAction action, InputDevice device, uint button);

    /// <summary>
    /// Resets all control settings to defaults.
    /// </summary>
    void ResetToDefaults();

    /// <summary>
    /// Validates the current settings.
    /// </summary>
    /// <returns>True if all settings are valid; otherwise false.</returns>
    bool IsValid();
}
