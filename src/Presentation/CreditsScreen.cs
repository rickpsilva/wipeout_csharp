using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.UI;
using static WipeoutRewrite.Infrastructure.UI.UIConstants;

namespace WipeoutRewrite.Presentation;

public class CreditsScreen
{
    private float _scrollY;
    private const float ScrollSpeed = 30.0f; // pixels por segundo
    private readonly IFontSystem? _fontSystem;
    private readonly string[] _creditsLines = Strings.CreditsLines;
    
    public CreditsScreen(IFontSystem? fontSystem = null)
    {
        _fontSystem = fontSystem;
        Reset();
    }
    
    public void Reset()
    {
        _scrollY = 0f;
    }
    
    public void Update(float deltaTime)
    {
        _scrollY += ScrollSpeed * deltaTime;
        
        // Reset ao fim do scroll
        if (_scrollY > _creditsLines.Length * 30)
        {
            Reset();
        }
    }
    
    public void Render(IRenderer renderer, int screenWidth, int screenHeight)
    {
        // Begin 2D rendering (BeginFrame já faz o Clear com fundo escuro)
        renderer.BeginFrame();
        
        // TODO: Quando o racing engine estiver implementado, renderizar corrida em background aqui
        
        if (_fontSystem != null)
        {
            float startY = screenHeight - _scrollY;
            int lineHeight = Spacing.CreditsLineHeight;
            
            for (int i = 0; i < _creditsLines.Length; i++)
            {
                float y = startY + (i * lineHeight);
                
                // Só desenhar se estiver visível
                if (y > -lineHeight && y < screenHeight + lineHeight)
                {
                    string line = _creditsLines[i];
                    if (!string.IsNullOrEmpty(line))
                    {
                        // Títulos em branco, resto em cinzento
                        bool isTitle = Array.Exists(Strings.CreditsTitles, t => t == line);
                        
                        var color = isTitle ? Colors.CreditsTitle : Colors.CreditsText;
                        
                        Vector2 pos = new Vector2(screenWidth / 2, y);
                        _fontSystem.DrawTextCentered(renderer, line, pos, TextSize.Size16, color);
                    }
                }
            }
        }
        
        renderer.EndFrame();
    }
}
