using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.UI;
using static WipeoutRewrite.Infrastructure.UI.UIConstants;

namespace WipeoutRewrite.Presentation
{
    public class TitleScreen : ITitleScreen
    {
        private float _timer = 0f;
        private float _blinkTimer = 0f;
        private bool _attractShown = false;
        private bool _textureLoaded = false;
        private const float AttractDelayFirst = 10.0f; // 10 seconds before credits
        private const float AttractDelaySubsequent = 10.0f;
        private const float BlinkInterval = 0.5f; // Pisca a cada 0.5s
        
        private int _titleTexture;
        private int _titleWidth;
        private int _titleHeight;
       
        private readonly IFontSystem _fontSystem;
        private readonly ITimImageLoader _timLoader;

        private readonly IRenderer _renderer;
        
        public TitleScreen(
            ITimImageLoader timLoader, 
            IFontSystem fontSystem,
            IRenderer renderer)
        {
            _fontSystem = fontSystem ?? throw new ArgumentNullException(nameof(fontSystem));
            _timLoader = timLoader ?? throw new ArgumentNullException(nameof(timLoader));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

        }
        
        public void Initialize()
        {
            if (!_textureLoaded)
            {
                LoadTitleTexture();
            }
        }
        
        private void LoadTitleTexture()
        {
            try
            {
                string timPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "wipeout", "textures", "wiptitle.tim");
                if (File.Exists(timPath))
                {
                    // Respect TIM transparency (MSB) so title background keeps alpha
                    var (pixels, width, height) = _timLoader.LoadTim(timPath, true);
                    _titleWidth = width;
                    _titleHeight = height;
                    
                    // Create OpenGL texture
                    _titleTexture = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, _titleTexture);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                        PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    
                    _textureLoaded = true;
                    Console.WriteLine($"✓ Title texture loaded: {width}x{height}");
                }
                else
                {
                    Console.WriteLine($"✗ Title texture not found: {timPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error loading title texture: {ex.Message}");
            }
        }
        
        public void Reset()
        {
            _timer = 0f;
            _blinkTimer = 0f;
        }
        
        public void Update(float deltaTime, out bool shouldStartAttract, out bool shouldStartMenu)
        {
            _timer += deltaTime;
            _blinkTimer += deltaTime;
            
            if (_blinkTimer >= BlinkInterval * 2)
            {
                _blinkTimer = 0f;
            }
            
            float attractDelay = _attractShown ? AttractDelaySubsequent : AttractDelayFirst;
            shouldStartAttract = _timer >= attractDelay;
            shouldStartMenu = false;
        }
        
        public void OnAttractComplete()
        {
            _attractShown = true;
            Reset();
        }
        
        public void OnStartPressed()
        {
            // Will trigger menu
        }
        
        public void Render(int screenWidth, int screenHeight)
        {
            // Begin 2D rendering
            _renderer.BeginFrame();
            _renderer.Setup2DRendering();
            
            // Disable depth test for 2D overlay rendering
            _renderer.SetDepthTest(false);
            
            // Render wiptitle.tim texture fullscreen - ALWAYS VISIBLE
            if (_textureLoaded)
            {
                _renderer.SetCurrentTexture(_titleTexture);
                // rgba(128, 128, 128, 255) from C code
                _renderer.PushSprite(0, 0, screenWidth, screenHeight, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            }
            
            // Render "PRESS ENTER" text - ALWAYS BLINKING between gray and yellow
            if (_fontSystem != null)
            {
                string text = Strings.SplashPressEnter;
                
                // Alternate between white and yellow every BlinkInterval (0.5s)
                // 0.0-0.5s: white, 0.5-1.0s: yellow, then repeat
                bool isWhitePhase = ((int)(_blinkTimer / BlinkInterval)) % 2 == 0;
                var color = isWhitePhase ? new Color4(1.0f, 1.0f, 1.0f, 1.0f) : Colors.SplashTextYellow;
                
                // Position text near bottom
                Vector2 pos = new(screenWidth / 2, screenHeight - 60);
                
                _fontSystem.DrawTextCentered(_renderer, text, pos, TextSize.Size16, color);
            }
            
            _renderer.EndFrame2D();
        }
    }
}