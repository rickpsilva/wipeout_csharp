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
        private bool _enterWasPressed; // Para evitar processar Enter em modos consecutivos

        /// <summary>
        /// Construtor com Dependency Injection.
        /// Todas as dependências são injetadas via construtor.
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
            if (_gameState != null && _assetLoader.LoadTrackList() is { Count: > 0 } tracks)
            {
                _logger.LogInformation("Loaded tracks: {Tracks}", string.Join(", ", tracks));

                // Inicializar GameState com primeira track
                var track = new Track(tracks[0], null);
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
            _titleScreen = new TitleScreen(_fontSystem, _timLoader);
            _creditsScreen = new CreditsScreen(_fontSystem);
            _attractMode = new AttractMode(_gameState);

            // Inicializar música
            string musicPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "wipeout", "music");
            _musicPlayer.LoadTracks(musicPath);

            // Carregar e iniciar o vídeo de introdução DEPOIS de tudo estar pronto
            string introPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "wipeout", "intro.mpeg");
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
                _musicPlayer?.SetMode(MusicMode.Random); // Iniciar música no splash
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            InputManager.Update(KeyboardState);

            // Atualizar música
            _musicPlayer?.Update((float)args.Time);

            // Processar ações do jogo
            if (InputManager.IsActionPressed(GameAction.Exit, KeyboardState))
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

            // Controlar estado do Enter para evitar processar em modos consecutivos
            bool enterIsPressed = KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter);
            
            // Saltar intro com Enter
            if (_gameState?.CurrentMode == GameMode.Intro && _introPlayer != null)
            {
                if (enterIsPressed && !_enterWasPressed)
                {
                    _introPlayer.Skip();
                    _gameState.CurrentMode = GameMode.SplashScreen;
                    _musicPlayer?.SetMode(MusicMode.Random); // Iniciar música ao saltar intro
                    _enterWasPressed = true; // Marcar como processado
                    _logger.LogInformation("Saltando para splash screen...");
                }
            }

            // Splash screen logic
            else if (_gameState?.CurrentMode == GameMode.SplashScreen && _titleScreen != null)
            {
                _titleScreen.Update((float)args.Time, out bool shouldStartAttract, out bool shouldStartMenu);
                
                if (enterIsPressed && !_enterWasPressed)
                {
                    _gameState.CurrentMode = GameMode.Menu;
                    _menuManager?.PushPage(MainMenuPages.CreateMainMenu());
                    _enterWasPressed = true; // Marcar como processado
                    _logger.LogInformation("Entrando no menu principal...");
                }
                else if (shouldStartAttract)
                {
                    _gameState.CurrentMode = GameMode.AttractMode;
                    _creditsScreen?.Reset();
                    _logger.LogInformation("Iniciando attract mode (credits)...");
                    // TODO: Quando o racing engine estiver implementado, iniciar corrida com AI + créditos
                }
            }

            // Resetar flag quando tecla for largada
            if (!enterIsPressed)
            {
                _enterWasPressed = false;
            }

            // Attract mode (credits) - qualquer tecla volta ao splash
            if (_gameState?.CurrentMode == GameMode.AttractMode && _creditsScreen != null)
            {
                _creditsScreen.Update((float)args.Time);
                
                // Qualquer tecla volta ao splash screen
                if (enterIsPressed || 
                    KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space) ||
                    KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
                {
                    _gameState.CurrentMode = GameMode.SplashScreen;
                    _titleScreen?.Reset();
                    _enterWasPressed = true; // Marcar como processado para evitar duplo trigger
                    _logger.LogInformation("Voltando ao splash screen...");
                }
            }

            // Menu navigation
            if (_gameState?.CurrentMode == GameMode.Menu && _menuManager != null)
            {
                _menuManager.Update((float)args.Time);
                
                if (InputManager.IsActionPressed(GameAction.MenuUp, KeyboardState))
                {
                    _menuManager.HandleInput(MenuAction.Up);
                }
                if (InputManager.IsActionPressed(GameAction.MenuDown, KeyboardState))
                {
                    _menuManager.HandleInput(MenuAction.Down);
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
                    _menuManager.HandleInput(MenuAction.Select);
                }
                if (InputManager.IsActionPressed(GameAction.MenuBack, KeyboardState))
                {
                    if (_menuManager.CurrentPage != null && _menuManager.HandleInput(MenuAction.Back))
                    {
                        // Page was popped
                    }
                    else
                    {
                        // No more pages, go back to title
                        _gameState.CurrentMode = GameMode.SplashScreen;
                        _titleScreen?.Reset();
                        _logger.LogInformation("Voltando para title screen...");
                    }
                }
            }

            // Attract mode update
            if (_gameState?.CurrentMode == GameMode.AttractMode && _attractMode != null)
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

            // TODO: Atualizar lógica do jogo aqui (física, IA)
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            if (_renderer == null) return;

            // Renderizar vídeo de intro se estiver a tocar
            if (_gameState?.CurrentMode == GameMode.Intro && _introPlayer != null)
            {
                if (_introPlayer.IsPlaying)
                {
                    _introPlayer.Update(); // Atualiza frame
                    
                    // Desenhar vídeo centrado com aspect ratio correto
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
                    // Vídeo terminou, ir para splash screen
                    _introPlayer.Dispose();
                    _introPlayer = null;
                    _gameState.CurrentMode = GameMode.SplashScreen;
                    _musicPlayer?.SetMode(MusicMode.Random); // Iniciar música no splash
                    _logger.LogInformation("Intro terminada, a mostrar splash screen...");
                }
            }
            else if (_gameState?.CurrentMode == GameMode.SplashScreen && _titleScreen != null)
            {
                // Render title screen
                _titleScreen.Render(_renderer, ClientSize.X, ClientSize.Y);
            }
            else if (_gameState?.CurrentMode == GameMode.AttractMode && _creditsScreen != null)
            {
                // Render credits
                _creditsScreen.Render(_renderer, ClientSize.X, ClientSize.Y);
            }
            else if (_gameState?.CurrentMode == GameMode.Menu && _menuManager != null && _menuRenderer != null)
            {
                // Render menu
                _renderer.BeginFrame();
                
                // TODO: Draw background texture (wipeout1.tim)
                
                // Debug: Draw a white square to verify rendering is working
                _renderer.PushSprite(100, 100, 200, 200, new OpenTK.Mathematics.Vector4(1, 0, 0, 1)); // Red square
                
                _menuRenderer.RenderMenu(_menuManager);
                _renderer.EndFrame();
            }
            else if (_gameState?.CurrentMode == GameMode.AttractMode && _attractMode != null)
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
            
            // Atualizar dimensões do renderer
            if (_renderer != null)
            {
                _renderer.UpdateScreenSize(e.Width, e.Height);
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
