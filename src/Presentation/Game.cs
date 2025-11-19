using System;
using System.IO;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.Video;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.Audio;
using WipeoutRewrite.Infrastructure.Input;
using WipeoutRewrite.Infrastructure.UI;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Presentation;
using WipeoutRewrite.Presentation.Menus;

namespace WipeoutRewrite
{
    public class Game : GameWindow
    {
        private readonly ILogger<Game> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private IntroVideoPlayer? _introPlayer;
        private readonly IRenderer _renderer;
        private IMenuRenderer? _menuRenderer;
        private readonly IFontSystem _fontSystem;
        private float _spriteX, _spriteY;
        private readonly IAssetLoader _assetLoader;
        private readonly IMenuManager _menuManager;
        private readonly GameState _gameState;
        private readonly TimImageLoader _timLoader;
        private TitleScreen? _titleScreen;
        private AttractMode? _attractMode;
        private CreditsScreen? _creditsScreen;
        private readonly IMusicPlayer _musicPlayer;
        
        // Menu background
        private int _menuBackgroundTexture;
        private bool _menuBackgroundLoaded;

        /// <summary>
        /// Construtor com Dependency Injection.
        /// All dependencies are injected via constructor.
        /// </summary>
        public Game(
            GameWindowSettings gws,
            NativeWindowSettings nws,
            IRenderer renderer,
            IMusicPlayer musicPlayer,
            IAssetLoader assetLoader,
            IFontSystem fontSystem,
            IMenuManager menuManager,
            GameState gameState,
            TimImageLoader timLoader,
            ILogger<Game> logger,
            ILoggerFactory loggerFactory)
            : base(gws, nws)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _musicPlayer = musicPlayer ?? throw new ArgumentNullException(nameof(musicPlayer));
            _assetLoader = assetLoader ?? throw new ArgumentNullException(nameof(assetLoader));
            _fontSystem = fontSystem ?? throw new ArgumentNullException(nameof(fontSystem));
            _menuManager = menuManager ?? throw new ArgumentNullException(nameof(menuManager));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _timLoader = timLoader ?? throw new ArgumentNullException(nameof(timLoader));
            
            _spriteX = 0;
            _spriteY = 0;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            // Inicializar subsistemas
            Renderer.Init(); // O seu Renderer.Init()

            // Inicializar renderer com tamanho da janela
            _renderer.Init(Size.X, Size.Y);

            // Inicializar AssetLoader com caminho local dos assets
            string assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "assets");
            _assetLoader.Initialize(assetsPath);

            // Carregar fontes
            _fontSystem.LoadFonts(assetsPath);

            // Carregar lista de tracks
            if (_assetLoader.LoadTrackList() is { Count: > 0 } tracks)
            {
                _logger.LogInformation("Loaded tracks: {Tracks}", string.Join(", ", tracks));

                // Inicializar GameState com primeira track
                var trackLogger = _loggerFactory.CreateLogger<Track>();
                var track = new Track(tracks[0], trackLogger);
                _gameState.Initialize(track, playerShipId: 0);
            }

            // Carregar textura de exemplo (assets/sprite.png)
            string workDir = System.IO.Directory.GetCurrentDirectory();
            _logger.LogDebug("Working directory: {WorkDir}", workDir);
            string spritePath = System.IO.Path.Combine(workDir, "assets", "sprite.png");
            _logger.LogDebug("Looking for sprite at: {SpritePath}", spritePath);
            if (System.IO.File.Exists(spritePath))
            {
                try
                {
                    _renderer.LoadSpriteTexture(spritePath);
                    _logger.LogInformation("Sprite texture loaded successfully!");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading sprite");
                }
            }
            else
            {
                _logger.LogWarning("Sprite texture not found at: {SpritePath}", spritePath);
            }
            _logger.LogInformation("Game loaded. Window: {Width}x{Height}", Size.X, Size.Y);

            // Inicializar menu e title screen
            _menuRenderer = new MenuRenderer(Size.X, Size.Y, _renderer, _fontSystem);
            _titleScreen = new TitleScreen(_timLoader, _fontSystem);
            _creditsScreen = new CreditsScreen(_fontSystem);
            _attractMode = new AttractMode(_gameState);
            
            // Load menu background texture (wipeout1.tim)
            LoadMenuBackground();

            // Initialize music
            string musicPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "wipeout", "music");
            _musicPlayer.LoadTracks(musicPath);

            // Load and start intro video AFTER everything is ready
            // Use .mpeg (faster) by default, .mp4 available but slower
            string introPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "wipeout", "intro.mpeg");
            if (!File.Exists(introPath))
            {
                introPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "wipeout", "intro.mp4");
            }
            
            if (File.Exists(introPath))
            {
                _logger.LogInformation("Reproduzindo intro: {IntroPath}", introPath);
                try
                {
                    var introLogger = _loggerFactory.CreateLogger<IntroVideoPlayer>();
                    _introPlayer = new IntroVideoPlayer(introPath, introLogger);
                    _introPlayer.Play();
                    _gameState.CurrentMode = GameMode.Intro;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao carregar vídeo");
                    _gameState.CurrentMode = GameMode.SplashScreen;
                }
            }
            else
            {
                _logger.LogWarning("Intro não encontrado: {IntroPath}", introPath);
                _gameState.CurrentMode = GameMode.SplashScreen;
                _musicPlayer?.SetMode(MusicMode.Random); // Start music on splash
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            // NOTE: InputManager.Update() is called at the END of this method
            // so _previousState stores the state from the PREVIOUS frame

            // Update music
            _musicPlayer?.Update((float)args.Time);

            // Process game actions - Exit only when NOT in menu (menu uses ESC for back)
            if (_gameState?.CurrentMode != GameMode.Menu && InputManager.IsActionPressed(GameAction.Exit, KeyboardState))
            {
                Close();
            }

            // Toggle fullscreen com F11
            if (KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.F11))
            {
                if (WindowState == OpenTK.Windowing.Common.WindowState.Fullscreen)
                {
                    WindowState = OpenTK.Windowing.Common.WindowState.Normal;
                    _logger.LogInformation("Modo janela");
                }
                else
                {
                    WindowState = OpenTK.Windowing.Common.WindowState.Fullscreen;
                    _logger.LogInformation("Modo fullscreen");
                }
            }

            // Saltar intro com Enter
            if (_gameState.CurrentMode == GameMode.Intro && _introPlayer != null)
            {
                if (InputManager.IsActionPressed(GameAction.MenuSelect, KeyboardState))
                {
                    _introPlayer.Skip();
                    _gameState.CurrentMode = GameMode.SplashScreen;
                    _musicPlayer?.SetMode(MusicMode.Random); // Start music when skipping intro
                    _logger.LogInformation("Saltando para splash screen...");
                }
            }

            // Splash screen logic
            else if (_gameState.CurrentMode == GameMode.SplashScreen && _titleScreen != null)
            {
                _titleScreen.Update((float)args.Time, out bool shouldStartAttract, out bool shouldStartMenu);
                
                if (InputManager.IsActionPressed(GameAction.MenuSelect, KeyboardState))
                {
                    _gameState.CurrentMode = GameMode.Menu;
                    _menuManager?.PushPage(MainMenuPages.CreateMainMenu());
                    _logger.LogInformation("Entrando no menu principal: {Title}, {Count} items", 
                        _menuManager?.CurrentPage?.Title, _menuManager?.CurrentPage?.Items.Count);
                    // Force input update so next frame doesn't immediately trigger Select
                    InputManager.Update(KeyboardState);
                }
                else if (shouldStartAttract)
                {
                    _gameState.CurrentMode = GameMode.AttractMode;
                    _creditsScreen?.Reset();
                    _logger.LogInformation("Iniciando attract mode (credits)...");
                    // TODO: When racing engine is implemented, start race with AI + credits
                }
            }

            // Attract mode (credits) - qualquer tecla volta ao splash
            if (_gameState.CurrentMode == GameMode.AttractMode && _creditsScreen != null)
            {
                _creditsScreen.Update((float)args.Time);
                
                // Qualquer tecla volta ao splash screen
                if (KeyboardState.IsAnyKeyDown)
                {
                    _gameState.CurrentMode = GameMode.SplashScreen;
                    _titleScreen?.Reset();
                    _logger.LogInformation("Voltando ao splash screen...");
                }
            }

            // Menu navigation
            if (_gameState.CurrentMode == GameMode.Menu && _menuManager != null)
            {
                _menuManager.Update((float)args.Time);
                
                if (InputManager.IsActionPressed(GameAction.MenuUp, KeyboardState))
                {
                    _menuManager.HandleInput(MenuAction.Up);
                    _logger.LogDebug("Menu: UP pressed, selected={Selected}", _menuManager.CurrentPage?.SelectedIndex);
                }
                if (InputManager.IsActionPressed(GameAction.MenuDown, KeyboardState))
                {
                    _menuManager.HandleInput(MenuAction.Down);
                    _logger.LogDebug("Menu: DOWN pressed, selected={Selected}", _menuManager.CurrentPage?.SelectedIndex);
                }
                if (InputManager.IsActionPressed(GameAction.MenuLeft, KeyboardState))
                {
                    _menuManager.HandleInput(MenuAction.Left);
                }
                if (InputManager.IsActionPressed(GameAction.MenuRight, KeyboardState))
                {
                    _menuManager.HandleInput(MenuAction.Right);
                }
                if (InputManager.IsActionPressed(GameAction.MenuSelect, KeyboardState))
                {
                    var page = _menuManager.CurrentPage;
                    var item = page?.SelectedItem;
                    _logger.LogInformation("Menu: ENTER pressed on '{Title}', item {Index}: {Label}", 
                        page?.Title, page?.SelectedIndex, item?.Label);
                    if (item != null && item.IsEnabled)
                    {
                        _menuManager.HandleInput(MenuAction.Select);
                    }
                }
                if (InputManager.IsActionPressed(GameAction.MenuBack, KeyboardState))
                {
                    if (_menuManager.CurrentPage != null && _menuManager.HandleInput(MenuAction.Back))
                    {
                        _logger.LogInformation("Menu: BACKSPACE - voltou para página anterior");
                    }
                    else
                    {
                        // No more pages, go back to title
                        _gameState.CurrentMode = GameMode.SplashScreen;
                        _titleScreen?.Reset();
                        _logger.LogInformation("Menu: BACKSPACE - voltando para title screen...");
                    }
                }
                if (InputManager.IsActionPressed(GameAction.Exit, KeyboardState))
                {
                    // ESC in menu goes back to splash screen
                    _gameState.CurrentMode = GameMode.SplashScreen;
                    _titleScreen?.Reset();
                    _logger.LogInformation("Menu: ESC - voltando para title screen...");
                }
            }

            // Attract mode update
            if (_gameState.CurrentMode == GameMode.AttractMode && _attractMode != null)
            {
                _attractMode.Update((float)args.Time);
                
                // Skip attract mode with any key
                if (KeyboardState.IsAnyKeyDown)
                {
                    _attractMode.Stop();
                    _titleScreen?.OnAttractComplete();
                    _logger.LogInformation("Saindo do attract mode...");
                }
            }

            // Atualizar estado do jogo
            if (_gameState != null)
            {
                _gameState.Update((float)args.Time);
                
                // Se temos um jogador, processar input
                if (_gameState.PlayerShip != null)
                {
                    _gameState.PlayerShip.InputAccelerate = InputManager.IsActionDown(GameAction.Accelerate, KeyboardState);
                    _gameState.PlayerShip.InputBrake = InputManager.IsActionDown(GameAction.Brake, KeyboardState);
                    _gameState.PlayerShip.InputTurnLeft = InputManager.IsActionDown(GameAction.TurnLeft, KeyboardState);
                    _gameState.PlayerShip.InputTurnRight = InputManager.IsActionDown(GameAction.TurnRight, KeyboardState);
                    _gameState.PlayerShip.InputBoostLeft = InputManager.IsActionDown(GameAction.BoostLeft, KeyboardState);
                    _gameState.PlayerShip.InputBoostRight = InputManager.IsActionDown(GameAction.BoostRight, KeyboardState);
                }
            }

            // Movimento do sprite baseado em input (para teste visual)
            float speed = 5f;
            if (InputManager.IsActionDown(GameAction.Accelerate, KeyboardState))
                _spriteY -= speed;
            if (InputManager.IsActionDown(GameAction.Brake, KeyboardState))
                _spriteY += speed;
            if (InputManager.IsActionDown(GameAction.TurnLeft, KeyboardState))
                _spriteX -= speed;
            if (InputManager.IsActionDown(GameAction.TurnRight, KeyboardState))
                _spriteX += speed;

            // Clamp sprite dentro dos limites da janela
            float spriteSize = 128;
            _spriteX = Math.Max(0, Math.Min(_spriteX, Size.X - spriteSize));
            _spriteY = Math.Max(0, Math.Min(_spriteY, Size.Y - spriteSize));

            // TODO: Update game logic here (physics, AI)
            
            // Update input state at the END of frame for next frame's comparison
            InputManager.Update(KeyboardState);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            if (_renderer == null) return;

            // Render intro video if playing
            if (_gameState.CurrentMode == GameMode.Intro && _introPlayer != null)
            {
                if (_introPlayer.IsPlaying)
                {
                    _introPlayer.Update(); // Atualiza frame
                    
                    // Draw video centered with correct aspect ratio
                    _renderer.RenderVideoFrame(
                        _introPlayer.GetTextureId(),
                        _introPlayer.GetWidth(),
                        _introPlayer.GetHeight(),
                        ClientSize.X,
                        ClientSize.Y
                    );
                }
                else
                {
                    // Video ended, go to splash screen
                    _introPlayer.Dispose();
                    _introPlayer = null;
                    _gameState.CurrentMode = GameMode.SplashScreen;
                    _musicPlayer?.SetMode(MusicMode.Random); // Start music on splash
                    _logger.LogInformation("Intro terminada, a mostrar splash screen...");
                }
            }
            else if (_gameState.CurrentMode == GameMode.SplashScreen && _titleScreen != null)
            {
                // Render title screen
                _titleScreen.Render(_renderer, ClientSize.X, ClientSize.Y);
            }
            else if (_gameState.CurrentMode == GameMode.AttractMode && _creditsScreen != null)
            {
                // Render credits
                _creditsScreen.Render(_renderer, ClientSize.X, ClientSize.Y);
            }
            else if (_gameState.CurrentMode == GameMode.Menu && _menuManager != null && _menuRenderer != null)
            {
                // Render menu
                _renderer.BeginFrame();
                
                // Draw background texture (wipeout1.tim)
                if (_menuBackgroundLoaded)
                {
                    _renderer.SetCurrentTexture(_menuBackgroundTexture);
                    _renderer.PushSprite(0, 0, ClientSize.X, ClientSize.Y, new OpenTK.Mathematics.Vector4(1, 1, 1, 1));
                }
                
                _menuRenderer.RenderMenu(_menuManager);
                _renderer.EndFrame();
            }
            else if (_gameState.CurrentMode == GameMode.AttractMode && _attractMode != null)
            {
                // Render attract mode
                _renderer.BeginFrame();
                _attractMode.Render(_renderer);
                _renderer.EndFrame();
            }

            

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            
            // Atualizar viewport do OpenGL
            GL.Viewport(0, 0, e.Width, e.Height);
            
            // Update renderer dimensions
            if (_renderer != null)
            {
                _renderer.UpdateScreenSize(e.Width, e.Height);
                
                // Recreate menu renderer with new dimensions for proper positioning
                _menuRenderer = new MenuRenderer(e.Width, e.Height, _renderer, _fontSystem);
                _logger.LogDebug("MenuRenderer updated for new window size: {Width}x{Height}", e.Width, e.Height);
            }
        }

        private void LoadMenuBackground()
        {
            try
            {
                string timPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "wipeout", "textures", "wipeout1.tim");
                if (File.Exists(timPath))
                {
                    var (pixels, width, height) = _timLoader.LoadTim(timPath, false);
                    
                    // Create OpenGL texture
                    _menuBackgroundTexture = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, _menuBackgroundTexture);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                        PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    
                    _menuBackgroundLoaded = true;
                    _logger.LogInformation("Menu background loaded: {Width}x{Height}", width, height);
                }
                else
                {
                    _logger.LogWarning("Menu background not found: {Path}", timPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu background");
            }
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            _introPlayer?.Dispose();
            _renderer?.Cleanup();
        }
    }
}
