namespace WipeoutRewrite.Infrastructure.Database.Entities;

/// <summary>
/// Database entity for Controls Settings.
/// Maps to the database table for storing keyboard and joystick button bindings.
/// </summary>
public class ControlsSettingsEntity
{
    public int Id { get; set; } = 1; // Single row

    // Button bindings: 9 actions × 2 devices (keyboard=0, joystick=1)
    // Action 0: Up
    public uint UpKeyboard { get; set; }
    public uint UpJoystick { get; set; }

    // Action 1: Down
    public uint DownKeyboard { get; set; }
    public uint DownJoystick { get; set; }

    // Action 2: Left
    public uint LeftKeyboard { get; set; }
    public uint LeftJoystick { get; set; }

    // Action 3: Right
    public uint RightKeyboard { get; set; }
    public uint RightJoystick { get; set; }

    // Action 4: Brake Left
    public uint BrakeLeftKeyboard { get; set; }
    public uint BrakeLeftJoystick { get; set; }

    // Action 5: Brake Right
    public uint BrakeRightKeyboard { get; set; }
    public uint BrakeRightJoystick { get; set; }

    // Action 6: Thrust
    public uint ThrustKeyboard { get; set; }
    public uint ThrustJoystick { get; set; }

    // Action 7: Fire
    public uint FireKeyboard { get; set; }
    public uint FireJoystick { get; set; }

    // Action 8: Change View
    public uint ChangeViewKeyboard { get; set; }
    public uint ChangeViewJoystick { get; set; }

    // Metadata
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Convert entity to button array compatible with internal format.
    /// </summary>
    public uint[,] ToButtonArray()
    {
        var buttons = new uint[9, 2];
        buttons[0, 0] = UpKeyboard;         buttons[0, 1] = UpJoystick;
        buttons[1, 0] = DownKeyboard;       buttons[1, 1] = DownJoystick;
        buttons[2, 0] = LeftKeyboard;       buttons[2, 1] = LeftJoystick;
        buttons[3, 0] = RightKeyboard;      buttons[3, 1] = RightJoystick;
        buttons[4, 0] = BrakeLeftKeyboard;  buttons[4, 1] = BrakeLeftJoystick;
        buttons[5, 0] = BrakeRightKeyboard; buttons[5, 1] = BrakeRightJoystick;
        buttons[6, 0] = ThrustKeyboard;     buttons[6, 1] = ThrustJoystick;
        buttons[7, 0] = FireKeyboard;       buttons[7, 1] = FireJoystick;
        buttons[8, 0] = ChangeViewKeyboard; buttons[8, 1] = ChangeViewJoystick;
        return buttons;
    }

    /// <summary>
    /// Load from button array.
    /// </summary>
    public void FromButtonArray(uint[,] buttons)
    {
        UpKeyboard = buttons[0, 0];         UpJoystick = buttons[0, 1];
        DownKeyboard = buttons[1, 0];       DownJoystick = buttons[1, 1];
        LeftKeyboard = buttons[2, 0];       LeftJoystick = buttons[2, 1];
        RightKeyboard = buttons[3, 0];      RightJoystick = buttons[3, 1];
        BrakeLeftKeyboard = buttons[4, 0];  BrakeLeftJoystick = buttons[4, 1];
        BrakeRightKeyboard = buttons[5, 0]; BrakeRightJoystick = buttons[5, 1];
        ThrustKeyboard = buttons[6, 0];     ThrustJoystick = buttons[6, 1];
        FireKeyboard = buttons[7, 0];       FireJoystick = buttons[7, 1];
        ChangeViewKeyboard = buttons[8, 0]; ChangeViewJoystick = buttons[8, 1];
        LastModified = DateTime.UtcNow;
    }
}
