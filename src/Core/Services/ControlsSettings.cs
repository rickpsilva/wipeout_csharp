namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Default implementation of control settings management.
/// Stores button mappings for 9 game actions × 2 input devices (keyboard, joystick).
/// Mirrors wipeout-rewrite save_t.buttons[NUM_GAME_ACTIONS][2].
/// </summary>
public class ControlsSettings : IControlsSettings
{
    // buttons[action][device] where device: 0=Keyboard, 1=Joystick
    private uint[,] _buttons = new uint[9, 2];

    public ControlsSettings()
    {
        ResetToDefaults();
    }

    public uint GetButtonBinding(RaceAction action, InputDevice device)
    {
        return _buttons[(int)action, (int)device];
    }

    public void SetButtonBinding(RaceAction action, InputDevice device, uint button)
    {
        _buttons[(int)action, (int)device] = button;
    }

    public void ResetToDefaults()
    {
        // Default keyboard bindings (matching original wipeout)
        _buttons[(int)RaceAction.Up, (int)InputDevice.Keyboard] = 82; // UP arrow
        _buttons[(int)RaceAction.Down, (int)InputDevice.Keyboard] = 81; // DOWN arrow
        _buttons[(int)RaceAction.Left, (int)InputDevice.Keyboard] = 80; // LEFT arrow
        _buttons[(int)RaceAction.Right, (int)InputDevice.Keyboard] = 79; // RIGHT arrow
        _buttons[(int)RaceAction.BrakeLeft, (int)InputDevice.Keyboard] = 6; // C
        _buttons[(int)RaceAction.BrakeRight, (int)InputDevice.Keyboard] = 25; // V
        _buttons[(int)RaceAction.Thrust, (int)InputDevice.Keyboard] = 27; // X
        _buttons[(int)RaceAction.Fire, (int)InputDevice.Keyboard] = 29; // Z
        _buttons[(int)RaceAction.ChangeView, (int)InputDevice.Keyboard] = 4; // A

        // Default joystick bindings (matching C enum: INPUT_GAMEPAD_*)
        // Using standard gamepad layout (Xbox/PlayStation compatible)
        _buttons[(int)RaceAction.Up, (int)InputDevice.Joystick] = 127; // INPUT_GAMEPAD_L_STICK_UP
        _buttons[(int)RaceAction.Down, (int)InputDevice.Joystick] = 128; // INPUT_GAMEPAD_L_STICK_DOWN
        _buttons[(int)RaceAction.Left, (int)InputDevice.Joystick] = 129; // INPUT_GAMEPAD_L_STICK_LEFT
        _buttons[(int)RaceAction.Right, (int)InputDevice.Joystick] = 130; // INPUT_GAMEPAD_L_STICK_RIGHT
        _buttons[(int)RaceAction.BrakeLeft, (int)InputDevice.Joystick] = 114; // INPUT_GAMEPAD_L_SHOULDER
        _buttons[(int)RaceAction.BrakeRight, (int)InputDevice.Joystick] = 115; // INPUT_GAMEPAD_R_SHOULDER
        _buttons[(int)RaceAction.Thrust, (int)InputDevice.Joystick] = 110; // INPUT_GAMEPAD_A
        _buttons[(int)RaceAction.Fire, (int)InputDevice.Joystick] = 112; // INPUT_GAMEPAD_B
        _buttons[(int)RaceAction.ChangeView, (int)InputDevice.Joystick] = 111; // INPUT_GAMEPAD_Y
    }

    public bool IsValid()
    {
        // Basic validation: at least keyboard bindings should be set
        for (int i = 0; i < 9; i++)
        {
            if (_buttons[i, (int)InputDevice.Keyboard] == 0)
                return false;
        }
        return true;
    }
}
