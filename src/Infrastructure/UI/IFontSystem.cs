using WipeoutRewrite.Infrastructure.Graphics;
using OpenTK.Mathematics;

namespace WipeoutRewrite.Infrastructure.UI;

/// <summary>
/// Interface para sistema de fontes.
/// </summary>
public interface IFontSystem
{
    /// <summary>
    /// Carrega fontes do diretório de assets.
    /// </summary>
    void LoadFonts(string assetsPath);
    
    /// <summary>
    /// Desenha texto na tela.
    /// </summary>
    void DrawText(IRenderer renderer, string text, Vector2 pos, TextSize textSize, Color4 color);
    
    /// <summary>
    /// Desenha texto centralizado na tela.
    /// </summary>
    void DrawTextCentered(IRenderer renderer, string text, Vector2 pos, TextSize textSize, Color4 color);
    
    /// <summary>
    /// Retorna largura do texto em pixels.
    /// </summary>
    int GetTextWidth(string text, TextSize textSize);
}
