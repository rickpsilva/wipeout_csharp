using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.UI;

namespace WipeoutRewrite.Presentation;

public class CreditsScreen
{
    private float _scrollY;
    private const float ScrollSpeed = 30.0f; // pixels por segundo
    private readonly IFontSystem? _fontSystem;
    
    private readonly string[] _creditsLines = 
    {
        "",
        "",
        "WIPEOUT",
        "",
        "ORIGINAL GAME",
        "PSYGNOSIS 1995",
        "",
        "",
        "C# REWRITE",
        "",
        "PROGRAMMING",
        "COMMUNITY PROJECT",
        "",
        "",
        "GRAPHICS",
        "THE DESIGNERS REPUBLIC",
        "",
        "",
        "MUSIC",
        "COLD STORAGE",
        "ORBITAL",
        "LEFTFIELD",
        "CHEMICAL BROTHERS",
        "",
        "",
        "SPECIAL THANKS",
        "DOMINIC SZABLEWSKI",
        "PHOBOSLAB",
        "",
        "",
        "",
        "",
        ""
    };
    
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
            int lineHeight = 30;
            
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
                        bool isTitle = line == "WIPEOUT" || 
                                     line == "ORIGINAL GAME" ||
                                     line == "C# REWRITE" ||
                                     line == "PROGRAMMING" ||
                                     line == "GRAPHICS" ||
                                     line == "MUSIC" ||
                                     line == "SPECIAL THANKS";
                        
                        var color = isTitle 
                            ? new Color4(1.0f, 1.0f, 1.0f, 1.0f) // Branco
                            : new Color4(0.7f, 0.7f, 0.7f, 1.0f); // Cinzento claro
                        
                        Vector2 pos = new Vector2(screenWidth / 2, y);
                        _fontSystem.DrawTextCentered(renderer, line, pos, TextSize.Size8, color);
                    }
                }
            }
        }
        
        renderer.EndFrame();
    }
}
