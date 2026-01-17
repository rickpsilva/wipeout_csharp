namespace WipeoutRewrite.Infrastructure.Input;

/// <summary>
/// Maps input key codes to human-readable names.
/// Based on wipeout-rewrite C's button_names array from input.c
/// </summary>
public static class KeyNameMapping
{
    // Keyboard key codes (SDL scancode numbers)
    private static readonly Dictionary<int, string> KeyNames = new()
    {
        { 4, "A" },
        { 5, "B" },
        { 6, "C" },
        { 7, "D" },
        { 8, "E" },
        { 9, "F" },
        { 10, "G" },
        { 11, "H" },
        { 12, "I" },
        { 13, "J" },
        { 14, "K" },
        { 15, "L" },
        { 16, "M" },
        { 17, "N" },
        { 18, "O" },
        { 19, "P" },
        { 20, "Q" },
        { 21, "R" },
        { 22, "S" },
        { 23, "T" },
        { 24, "U" },
        { 25, "V" },
        { 26, "W" },
        { 27, "X" },
        { 28, "Y" },
        { 29, "Z" },
        { 30, "1" },
        { 31, "2" },
        { 32, "3" },
        { 33, "4" },
        { 34, "5" },
        { 35, "6" },
        { 36, "7" },
        { 37, "8" },
        { 38, "9" },
        { 39, "0" },
        { 40, "RETURN" },
        { 41, "ESCAPE" },
        { 42, "BACKSP" },
        { 43, "TAB" },
        { 44, "SPACE" },
        { 45, "MINUS" },
        { 46, "EQUALS" },
        { 47, "LBRACKET" },
        { 48, "RBRACKET" },
        { 49, "BACKSLASH" },
        { 51, "SEMICOLON" },
        { 52, "APOSTROPHE" },
        { 53, "GRAVE" },
        { 54, "COMMA" },
        { 55, "PERIOD" },
        { 56, "SLASH" },
        { 57, "CAPSLOCK" },
        { 58, "F1" },
        { 59, "F2" },
        { 60, "F3" },
        { 61, "F4" },
        { 62, "F5" },
        { 63, "F6" },
        { 64, "F7" },
        { 65, "F8" },
        { 66, "F9" },
        { 67, "F10" },
        { 68, "F11" },
        { 69, "F12" },
        { 70, "PRTSC" },
        { 71, "SCRLK" },
        { 72, "PAUSE" },
        { 73, "INSERT" },
        { 74, "HOME" },
        { 75, "PG UP" },
        { 76, "DELETE" },
        { 77, "END" },
        { 78, "PG DOWN" },
        { 79, "RIGHT" },
        { 80, "LEFT" },
        { 81, "DOWN" },
        { 82, "UP" },
        { 83, "NLOCK" },
        { 84, "KPDIV" },
        { 85, "KPMUL" },
        { 86, "KPMINUS" },
        { 87, "KPPLUS" },
        { 88, "KPENTER" },
        { 89, "KP1" },
        { 90, "KP2" },
        { 91, "KP3" },
        { 92, "KP4" },
        { 93, "KP5" },
        { 94, "KP6" },
        { 95, "KP7" },
        { 96, "KP8" },
        { 97, "KP9" },
        { 98, "KP0" },
        { 99, "KPPERIOD" },
        { 100, "LCTRL" },
        { 101, "LSHIFT" },
        { 102, "LALT" },
        { 103, "LGUI" },
        { 104, "RCTRL" },
        { 105, "RSHIFT" },
        { 106, "RALT" },
        { 107, "RGUI" },
    };

    // Gamepad button codes
    private static readonly Dictionary<int, string> GamepadNames = new()
    {
        { 119, "A" },         // INPUT_GAMEPAD_A
        { 123, "Y" },         // INPUT_GAMEPAD_Y
        { 120, "B" },         // INPUT_GAMEPAD_B
        { 121, "X" },         // INPUT_GAMEPAD_X
        { 124, "LSHLDR" },    // INPUT_GAMEPAD_L_SHOULDER
        { 125, "RSHLDR" },    // INPUT_GAMEPAD_R_SHOULDER
        { 128, "LTRIG" },     // INPUT_GAMEPAD_L_TRIGGER
        { 129, "RTRIG" },     // INPUT_GAMEPAD_R_TRIGGER
        { 126, "SELECT" },    // INPUT_GAMEPAD_SELECT
        { 127, "START" },     // INPUT_GAMEPAD_START
        { 130, "LSTK" },      // INPUT_GAMEPAD_L_STICK_PRESS
        { 131, "RSTK" },      // INPUT_GAMEPAD_R_STICK_PRESS
        { 132, "DPUP" },      // INPUT_GAMEPAD_DPAD_UP
        { 133, "DPDOWN" },    // INPUT_GAMEPAD_DPAD_DOWN
        { 134, "DPLEFT" },    // INPUT_GAMEPAD_DPAD_LEFT
        { 135, "DPRIGHT" },   // INPUT_GAMEPAD_DPAD_RIGHT
    };

    /// <summary>
    /// Get the name of a keyboard key or gamepad button.
    /// </summary>
    public static string GetKeyName(int code)
    {
        // Try keyboard keys first
        if (KeyNames.TryGetValue(code, out var keyName))
            return keyName;

        // Try gamepad buttons
        if (GamepadNames.TryGetValue(code, out var gamepadName))
            return gamepadName;

        // Unknown code
        return $"KEY{code}";
    }
}
