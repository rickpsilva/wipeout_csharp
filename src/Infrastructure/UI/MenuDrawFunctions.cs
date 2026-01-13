using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Infrastructure.Input;

namespace WipeoutRewrite.Infrastructure.UI;

/// <summary>
/// Custom draw functions for menu pages.
/// Equivalent to page_options_control_draw, etc. in main_menu.c
/// </summary>
public static class MenuDrawFunctions
{
    private static IControlsSettings? _controlsSettings;

    public static void SetControlsSettings(IControlsSettings? settings)
    {
        _controlsSettings = settings;
    }

    /// <summary>
    /// Draw controls menu with 3-column table layout.
    /// Equivalent to page_options_control_draw() in C.
    /// </summary>
    public static void DrawControlsTable(MenuPage page)
    {
        if (_controlsSettings == null)
            return;

        // Position calculations (matching C code exactly)
        int left = page.ItemsPos.X + page.BlockWidth - 100;   // Keyboard column
        int right = page.ItemsPos.X + page.BlockWidth;        // Joystick column
        int lineY = page.ItemsPos.Y - 20;

        // Draw column headers (UI_SIZE_8 in C)
        Vec2i leftHeadPos = new(left - UIHelper.GetTextWidth("KEYBOARD", 8), lineY);
        Vec2i leftHeadScreen = UIHelper.ScaledPos(page.ItemsAnchor, leftHeadPos);
        UIHelper.DrawText("KEYBOARD", leftHeadScreen, 8, UIColor.Default);

        Vec2i rightHeadPos = new(right - UIHelper.GetTextWidth("JOYSTICK", 8), lineY);
        Vec2i rightHeadScreen = UIHelper.ScaledPos(page.ItemsAnchor, rightHeadPos);
        UIHelper.DrawText("JOYSTICK", rightHeadScreen, 8, UIColor.Default);

        lineY += 20;

        // Draw each action row
        var actions = new[]
        {
            (RaceAction.Up, "UP"),
            (RaceAction.Down, "DOWN"),
            (RaceAction.Left, "LEFT"),
            (RaceAction.Right, "RIGHT"),
            (RaceAction.BrakeLeft, "BRAKE L"),
            (RaceAction.BrakeRight, "BRAKE R"),
            (RaceAction.Thrust, "THRUST"),
            (RaceAction.Fire, "FIRE"),
            (RaceAction.ChangeView, "VIEW"),
        };

        for (int i = 0; i < actions.Length; i++)
        {
            var (action, label) = actions[i];
            bool isSelected = (i == page.SelectedIndex);
            var textColor = isSelected ? UIColor.Accent : UIColor.Default;

            // Draw action label (left column) - UI_SIZE_8
            Vec2i labelPos = new(page.ItemsPos.X, lineY);
            Vec2i labelScreen = UIHelper.ScaledPos(page.ItemsAnchor, labelPos);
            UIHelper.DrawText(label, labelScreen, 8, textColor);

            // Get button bindings
            uint keyboardButton = _controlsSettings.GetButtonBinding(action, InputDevice.Keyboard);
            uint joystickButton = _controlsSettings.GetButtonBinding(action, InputDevice.Joystick);

            // Draw keyboard binding (right-aligned to left column) - UI_SIZE_8
            if (keyboardButton != 0)
            {
                string keyName = GetButtonName(keyboardButton);
                Vec2i keyPos = new(left - UIHelper.GetTextWidth(keyName, 8), lineY);
                Vec2i keyScreen = UIHelper.ScaledPos(page.ItemsAnchor, keyPos);
                UIHelper.DrawText(keyName, keyScreen, 8, textColor);
            }

            // Draw joystick binding (right-aligned to right column) - UI_SIZE_8
            if (joystickButton != 0)
            {
                string joyName = GetButtonName(joystickButton);
                Vec2i joyPos = new(right - UIHelper.GetTextWidth(joyName, 8), lineY);
                Vec2i joyScreen = UIHelper.ScaledPos(page.ItemsAnchor, joyPos);
                UIHelper.DrawText(joyName, joyScreen, 8, textColor);
            }

            lineY += 12;  // Matching C code line spacing
        }
    }

    /// <summary>
    /// Maps button code to display name (matching wipeout-rewrite button_names array).
    /// </summary>
    private static string GetButtonName(uint button)
    {
        return button switch
        {
            // Keyboard keys (matching INPUT_KEY_* enum values)
            4 => "A", 5 => "B", 6 => "C", 7 => "D", 8 => "E", 9 => "F", 10 => "G", 11 => "H",
            12 => "I", 13 => "J", 14 => "K", 15 => "L", 16 => "M", 17 => "N", 18 => "O", 19 => "P",
            20 => "Q", 21 => "R", 22 => "S", 23 => "T", 24 => "U", 25 => "V", 26 => "W", 27 => "X",
            28 => "Y", 29 => "Z",
            30 => "1", 31 => "2", 32 => "3", 33 => "4", 34 => "5",
            35 => "6", 36 => "7", 37 => "8", 38 => "9", 39 => "0",
            40 => "RETURN", 41 => "ESCAPE", 42 => "BACKSP", 43 => "TAB", 44 => "SPACE",
            45 => "MINUS", 46 => "EQUALS", 47 => "LBRACKET", 48 => "RBRACKET", 49 => "BSLASH",
            50 => "HASH", 51 => "SMICOL", 52 => "APO", 53 => "TILDE",
            54 => "COMMA", 55 => "PERIOD", 56 => "SLASH", 57 => "CAPS",
            58 => "F1", 59 => "F2", 60 => "F3", 61 => "F4", 62 => "F5", 63 => "F6",
            64 => "F7", 65 => "F8", 66 => "F9", 67 => "F10", 68 => "F11", 69 => "F12",
            70 => "PRTSC", 71 => "SCRLK", 72 => "PAUSE", 73 => "INSERT", 74 => "HOME", 75 => "PG UP",
            76 => "DELETE", 77 => "END", 78 => "PG DOWN",
            79 => "RIGHT", 80 => "LEFT", 81 => "DOWN", 82 => "UP",
            83 => "NLOCK", 84 => "KPDIV", 85 => "KPMUL", 86 => "KPMINUS", 87 => "KPPLUS", 88 => "KPENTER",
            89 => "KP1", 90 => "KP2", 91 => "KP3", 92 => "KP4", 93 => "KP5",
            94 => "KP6", 95 => "KP7", 96 => "KP8", 97 => "KP9", 98 => "KP0", 99 => "KPPERIOD",

            // Gamepad buttons (matching INPUT_GAMEPAD_* enum values)
            110 => "A", 111 => "Y", 112 => "B", 113 => "X",
            114 => "LSHLDR", 115 => "RSHLDR", 116 => "LTRIG", 117 => "RTRIG",
            118 => "SELECT", 119 => "START",
            120 => "LSTK", 121 => "RSTK",
            122 => "DPUP", 123 => "DPDOWN", 124 => "DPLEFT", 125 => "DPRIGHT",
            126 => "HOME",
            127 => "LSTKUP", 128 => "LSTKDOWN", 129 => "LSTKLEFT", 130 => "LSTKRIGHT",
            131 => "RSTKUP", 132 => "RSTKDOWN", 133 => "RSTKLEFT", 134 => "RSTKRIGHT",

            _ => "UNKNWN"
        };
    }
}
