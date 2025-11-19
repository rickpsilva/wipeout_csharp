using WipeoutRewrite.Infrastructure.Graphics;
using OpenTK.Mathematics;

namespace WipeoutRewrite.Infrastructure.UI;

/// <summary>
/// Interface para sistema de fontes.
/// </summary>
public interface IFontSystem
{
    /// <summary>
    /// Loads fonts from assets directory.
    /// </summary>
    void LoadFonts(string assetsPath);
    
    /// <summary>
    /// Draws text on screen.
    /// </summary>
    void DrawText(IRenderer renderer, string text, Vector2 pos, TextSize textSize, Color4 color);
    
    /// <summary>
    /// Draws centered text on screen.
    /// </summary>
    void DrawTextCentered(IRenderer renderer, string text, Vector2 pos, TextSize textSize, Color4 color);
    
    /// <summary>
    /// Returns text width in pixels.
    /// </summary>
    int GetTextWidth(string text, TextSize textSize);
}
