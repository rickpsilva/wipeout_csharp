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
                int uiScale = 2;  // matching ui_get_scale() in C
                
                float y = startY;
                for (int i = 0; i < _creditsLines.Length; i++)
                {
                    string line = _creditsLines[i];
                    
                    if (!string.IsNullOrEmpty(line))
                    {
                        // Lines starting with '#' are titles (UI_SIZE_16, yellow)
                        // Other lines are normal text (UI_SIZE_8, white)
                        if (line.StartsWith("#"))
                        {
                            y += 48 * uiScale;  // Spacing before title
                            
                            // Only draw if visible
                            if (y > -50 && y < screenHeight + 50)
                            {
                                // Remove '#' prefix and render as title
                                string titleText = line.Substring(1);
                                Vector2 pos = new(screenWidth / 2, y);
                                _fontSystem.DrawTextCentered(_renderer, titleText, pos, TextSize.Size16, Colors.CreditsTitle);
                            }
                            
                            y += 32 * uiScale;  // Spacing after title
                        }
                        else
                        {
                            // Only draw if visible
                            if (y > -50 && y < screenHeight + 50)
                            {
                                Vector2 pos = new(screenWidth / 2, y);
                                _fontSystem.DrawTextCentered(_renderer, line, pos, TextSize.Size8, Colors.CreditsText);
                            }
                            
                            y += 12 * uiScale;  // Spacing after normal line
                        }
                    }
                }
            }
            
            _renderer.EndFrame2D();
        }
    }
}


