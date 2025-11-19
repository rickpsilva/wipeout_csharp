using OpenTK.Windowing.GraphicsLibraryFramework;

namespace WipeoutRewrite.Infrastructure.Input;

/// <summary>
/// Interface for game input management.
/// Allows swapping implementations (keyboard, joystick, network) and facilitates testing.
/// </summary>
public interface IInputManager
{
    /// <summary>
    /// Atualiza estado do input (deve ser chamado a cada frame).
    /// </summary>
    void Update(KeyboardState keyboardState);
    
    /// <summary>
    /// Checks if an action was pressed this frame (single press).
    /// </summary>
    bool IsActionPressed(GameAction action);
    
    /// <summary>
    /// Checks if an action is being held down.
    /// </summary>
    bool IsActionDown(GameAction action, KeyboardState keyboardState);
}
