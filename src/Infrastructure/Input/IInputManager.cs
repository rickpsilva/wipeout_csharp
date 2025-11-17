using OpenTK.Windowing.GraphicsLibraryFramework;

namespace WipeoutRewrite.Infrastructure.Input;

/// <summary>
/// Interface para gerenciamento de input.
/// Permite trocar implementações (teclado, joystick, rede) e facilita testes.
/// </summary>
public interface IInputManager
{
    /// <summary>
    /// Atualiza estado do input (deve ser chamado a cada frame).
    /// </summary>
    void Update(KeyboardState keyboardState);
    
    /// <summary>
    /// Verifica se uma ação foi pressionada neste frame (single press).
    /// </summary>
    bool IsActionPressed(GameAction action, KeyboardState keyboardState);
    
    /// <summary>
    /// Verifica se uma ação está mantida pressionada (held down).
    /// </summary>
    bool IsActionDown(GameAction action, KeyboardState keyboardState);
}
