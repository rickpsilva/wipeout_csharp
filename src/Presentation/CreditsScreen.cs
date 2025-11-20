using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.UI;
using static WipeoutRewrite.Infrastructure.UI.UIConstants;

namespace WipeoutRewrite.Presentation
{
    public class CreditsScreen : ICreditsScreen
    {
        private float _scrollY;
        private const float ScrollSpeed = 30.0f; // pixels por segundo
        private readonly IFontSystem _fontSystem;
        private readonly IRenderer _renderer;
        private readonly string[] _creditsLines = Strings.CreditsLines;
        
        public CreditsScreen(
            IFontSystem fontSystem, 
            IRenderer renderer)
        {
            _fontSystem = fontSystem ?? throw new ArgumentNullException(nameof(fontSystem));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

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
        
        public void Render(int screenWidth, int screenHeight)
        {
            // Begin 2D rendering (BeginFrame already clears with dark background)
            _renderer.BeginFrame();
            _renderer.Setup2DRendering();
            
            // TODO: Quando o racing engine estiver implementado, renderizar corrida em background aqui
            
            if (_fontSystem != null)
            {
                float startY = screenHeight - _scrollY;
                int lineHeight = Spacing.CreditsLineHeight;
                
                for (int i = 0; i < _creditsLines.Length; i++)
                {
                    float y = startY + (i * lineHeight);
                    
                    // Only draw if visible
                    if (y > -lineHeight && y < screenHeight + lineHeight)
                    {
                        string line = _creditsLines[i];
                        if (!string.IsNullOrEmpty(line))
                        {
                            // Titles in white, rest in grey
                            bool isTitle = Array.Exists(Strings.CreditsTitles, t => t == line);
                            
                            var color = isTitle ? Colors.CreditsTitle : Colors.CreditsText;
                            
                            Vector2 pos = new(screenWidth / 2, y);
                            _fontSystem.DrawTextCentered(_renderer, line, pos, TextSize.Size16, color);
                        }
                    }
                }
            }
            
            _renderer.EndFrame2D();
        }
    }
}


