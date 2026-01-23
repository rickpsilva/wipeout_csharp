using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Core.Graphics;
using WipeoutRewrite.Infrastructure.Video;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.Audio;
using WipeoutRewrite.Infrastructure.Input;
using WipeoutRewrite.Infrastructure.UI;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Core.Data;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Presentation;
using WipeoutRewrite.Presentation.Menus;

namespace WipeoutRewrite
{
    /// <summary>
    /// Main game window class that orchestrates all game systems.
    /// </summary>
    public class Game : GameWindow, IGameWindow
    {
        private readonly ILogger<Game> _logger;
        private readonly IVideoPlayer _introVideoPlayer;
        private readonly IRenderer _renderer;
        private readonly IMenuRenderer _menuRenderer;
        private readonly IFontSystem _fontSystem;
        private readonly IAssetLoader _assetLoader;
        private readonly IMenuManager _menuManager;
        private readonly IGameState _gameState;
        private readonly ITimImageLoader _timLoader;
        private readonly ITitleScreen _titleScreen;
        private readonly IAttractMode _attractMode;
        private readonly ICreditsScreen _creditsScreen;
        private readonly IContentPreview3D _contentPreview3D;
        private readonly IMusicPlayer _musicPlayer;
        private readonly IOptionsFactory _optionsFactory;
        private readonly SettingsPersistenceService _settingsPersistenceService;
        private readonly IBestTimesManager _bestTimesManager;
        private readonly IMenuBuilder _menuBuilder;
        private readonly IMenuActionHandler _menuActionHandler;
        private readonly IGameDataService _gameDataService;
        
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
            ILogger<Game> logger,
            IVideoPlayer introVideoPlayer,
            IRenderer renderer,
            IMusicPlayer musicPlayer,
            IAssetLoader assetLoader,
            IFontSystem fontSystem,
            IMenuManager menuManager,
            IMenuRenderer menuRenderer,
            IGameState gameState,
            ITimImageLoader timLoader,
            IAttractMode attractMode,
            IContentPreview3D contentPreview3D,
            ITitleScreen titleScreen,
            ICreditsScreen creditsScreen,
            IOptionsFactory optionsFactory,
            SettingsPersistenceService settingsPersistenceService,
            IBestTimesManager bestTimesManager,
            IMenuBuilder menuBuilder,
            IMenuActionHandler menuActionHandler,
            IGameDataService gameDataService
            )
            : base(gws, nws)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _introVideoPlayer = introVideoPlayer ?? throw new ArgumentNullException(nameof(introVideoPlayer));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _musicPlayer = musicPlayer ?? throw new ArgumentNullException(nameof(musicPlayer));
            _assetLoader = assetLoader ?? throw new ArgumentNullException(nameof(assetLoader));
            _fontSystem = fontSystem ?? throw new ArgumentNullException(nameof(fontSystem));
            _menuManager = menuManager ?? throw new ArgumentNullException(nameof(menuManager));
            _menuRenderer = menuRenderer ?? throw new ArgumentNullException(nameof(_menuRenderer));    
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _timLoader = timLoader ?? throw new ArgumentNullException(nameof(timLoader));
            _attractMode = attractMode ?? throw new ArgumentNullException(nameof(_attractMode));
            _contentPreview3D = contentPreview3D ?? throw new ArgumentNullException(nameof(contentPreview3D));
            _titleScreen = titleScreen ?? throw new ArgumentNullException(nameof(titleScreen));
            _creditsScreen = creditsScreen ?? throw new ArgumentNullException(nameof(creditsScreen));
            _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
            _settingsPersistenceService = settingsPersistenceService ?? throw new ArgumentNullException(nameof(settingsPersistenceService));
            _bestTimesManager = bestTimesManager ?? throw new ArgumentNullException(nameof(bestTimesManager));
            _menuBuilder = menuBuilder ?? throw new ArgumentNullException(nameof(menuBuilder));
            _menuActionHandler = menuActionHandler ?? throw new ArgumentNullException(nameof(menuActionHandler));
            _gameDataService = gameDataService ?? throw new ArgumentNullException(nameof(gameDataService));
        }

        [ExcludeFromCodeCoverage]
        protected override void OnLoad()
        {
            base.OnLoad();

            // renderer initialization with window size
            _renderer.Init(Size.X, Size.Y);

            // AssetLoader initialization with local assets path
            string assetsPath = AssetPaths.GetAssetsRootPath();
            _assetLoader.Initialize(assetsPath);

            // Load fonts
            _fontSystem.LoadFonts(assetsPath);

            // Load track list
            if (_assetLoader.LoadTrackList() is { Count: > 0 } tracks)
            {
                _logger.LogInformation("Loaded tracks: {Tracks}", string.Join(", ", tracks));

                // Initialize IGameState with first track
                _gameState.Initialize(playerShipId: 0);
            }

            // Load example texture (assets/sprite.png)
            string workDir = Directory.GetCurrentDirectory();
            _logger.LogDebug("Working directory: {WorkDir}", workDir);
            string spritePath = Path.Combine(AssetPaths.GetAssetsRootPath(), "sprite.png");
            _logger.LogDebug("Looking for sprite at: {SpritePath}", spritePath);
            if (File.Exists(spritePath))
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

            // Initialize menu and title screen
            _menuRenderer.SetWindowSize(Size.X, Size.Y);
            
            // UI scale is now calculated dynamically every frame based on window height
            // No need to set it here - see OnUpdateFrame()
            
            // Initialize TitleScreen (loads textures - OpenGL is now ready)
            _titleScreen.Initialize();
            
            // Set menu references for callbacks and services
            MainMenuPages.GameStateRef = _gameState as GameState;
            MainMenuPages.OptionsFactoryRef = _optionsFactory;
            MainMenuPages.SettingsPersistenceServiceRef = _settingsPersistenceService;
            MainMenuPages.BestTimesManagerRef = _bestTimesManager;
            MainMenuPages.MenuBuilderRef = _menuBuilder;
            MainMenuPages.MenuActionHandlerRef = _menuActionHandler;
            MainMenuPages.GameDataServiceRef = _gameDataService;
            
            // Initialize MenuBuilder with game data
            _menuBuilder.Initialize(_gameDataService);
            
            // Load menu background texture (wipeout1.tim)
            LoadMenuBackground();

            // Initialize music
            string musicPath = AssetPaths.GetMusicPath(AssetPaths.GetWipeoutAssetsPath());
            _musicPlayer.LoadTracks(musicPath);

            // Load and start intro video AFTER everything is ready (OpenGL is now initialized)
            _logger.LogInformation("Carregando vídeo intro...");
            try
            {
                _introVideoPlayer.Play();
                _gameState.CurrentMode = GameMode.Intro;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar vídeo: {Message}", ex.Message);
                _gameState.CurrentMode = GameMode.SplashScreen;
                _musicPlayer?.SetMode(MusicMode.Random); // Start music on splash if intro fails
            }
        }

        [ExcludeFromCodeCoverage]
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            // Update MainMenuPages with current keyboard state for input handling
            MainMenuPages.CurrentKeyboardState = KeyboardState;

            // Calculate UI scale dynamically based on window height (matching C code)
            // Formula from game.c: scale = max(1, sh >= 720 ? sh / 360 : sh / 240)
            int screenHeight = Size.Y;
            int autoScale = Math.Max(1, screenHeight >= 720 ? screenHeight / 360 : screenHeight / 240);
            
            // If user has set manual scale (not 0), cap the auto scale
            var videoSettings = _optionsFactory.CreateVideoSettings();
            int finalScale = videoSettings.UIScale == 0 ? autoScale : Math.Min((int)videoSettings.UIScale, autoScale);
            UIHelper.SetUIScale(finalScale);

            // NOTE: InputManager.Update() is called at the END of this method
            // so _previousState stores the state from the PREVIOUS frame

#pragma warning disable CS8602 // _gameState and _menuManager are validated in constructor
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
            UpdateIntroMode(KeyboardState);

            // Splash screen logic
            UpdateSplashScreenMode(KeyboardState, (float)args.Time);

            // Attract mode (credits)
            UpdateAttractMode(KeyboardState, (float)args.Time);

            // Menu navigation
            UpdateMenuMode(KeyboardState, (float)args.Time);

            // Attract mode visual update
            if (_gameState?.CurrentMode == GameMode.AttractMode && _attractMode != null)
            {
                _attractMode.Update((float)args.Time);
                
                // Skip attract mode with any key
                if (KeyboardState.IsAnyKeyDown)
                {
                    _attractMode.Stop();
                    _titleScreen.OnAttractComplete();
                    _logger.LogInformation("Exiting attract mode...");
                }
            }

            // Update game state
            if (_gameState != null)
            {
                _gameState.Update((float)args.Time);
                
                // If we have a player, process input
         
                _gameState.SetPlayerShip(
                    InputManager.IsActionDown(GameAction.Accelerate, KeyboardState),
                    InputManager.IsActionDown(GameAction.Brake, KeyboardState),
                    InputManager.IsActionDown(GameAction.TurnLeft, KeyboardState),
                    InputManager.IsActionDown(GameAction.TurnRight, KeyboardState),
                    InputManager.IsActionDown(GameAction.BoostLeft, KeyboardState),
                    InputManager.IsActionDown(GameAction.BoostRight, KeyboardState)
                );
                    
            }

            // TODO: Update game logic here (physics, AI)
            
            // Update input state at the END of frame for next frame's comparison
            InputManager.Update(KeyboardState);
#pragma warning restore CS8602
        }

        [ExcludeFromCodeCoverage]
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            if (_renderer == null) return;

            // Render intro video if playing
            if (_gameState.CurrentMode == GameMode.Intro && _introVideoPlayer != null)
            {
                if (_introVideoPlayer.IsPlaying)
                {
                    _introVideoPlayer.Update(); // Atualiza frame
                    
                    // Get current frame data and render
                    byte[]? frameData = _introVideoPlayer.GetCurrentFrameData();
                    if (frameData != null && frameData.Length > 0)
                    {
                        // Begin frame to clear screen and setup state
                        _renderer.BeginFrame();
                        _renderer.Setup2DRendering();
                        
                        _renderer.RenderVideoFrame(
                            frameData,
                            _introVideoPlayer.GetWidth(),
                            _introVideoPlayer.GetHeight(),
                            ClientSize.X,
                            ClientSize.Y
                        );
                        
                        _renderer.EndFrame2D();
                    }
                    else
                    {
                        _logger.LogWarning("Frame data is null or empty");
                    }
                }
                else
                {
                    // Video ended, go to splash screen
                    _introVideoPlayer.Dispose();
                    _gameState.CurrentMode = GameMode.SplashScreen;
                    _musicPlayer?.SetMode(MusicMode.Random); // Start music on splash
                    _logger.LogInformation("Intro finished, showing splash screen...");
                }
            }
            else if (_gameState.CurrentMode == GameMode.SplashScreen)
            {
                // Render title screen
                _titleScreen.Render(ClientSize.X, ClientSize.Y);
            }
            else if (_gameState.CurrentMode == GameMode.AttractMode)
            {
                // Render credits
                _creditsScreen.Render(ClientSize.X, ClientSize.Y);
            }
            else if (_gameState.CurrentMode == GameMode.Menu)
            {
                // Render menu with ship in background
                _renderer.BeginFrame();
                
                // 2D background texture
                _renderer.Setup2DRendering();
                if (_menuBackgroundLoaded)
                {
                    _renderer.SetCurrentTexture(_menuBackgroundTexture);
                    _renderer.PushSprite(0, 0, ClientSize.X, ClientSize.Y, new OpenTK.Mathematics.Vector4(1, 1, 1, 1));
                }
                _renderer.EndFrame2D();
                
                // Preview (3D model or 2D track image) in its own 2D context
                var currentPage = _menuManager.CurrentPage;
                if (currentPage != null && currentPage.SelectedItem != null)
                {
                    var selectedItem = currentPage.SelectedItem;
                    if (selectedItem.PreviewInfo != null)
                    {
                        var previewInfo = selectedItem.PreviewInfo;
                        
                        // Setup 2D context for preview rendering
                        _renderer.Setup2DRendering();
                        _renderer.SetDepthTest(false);  // Disable depth test for 2D preview
                        
                        // Check if this is a track image preview
                        if (previewInfo.IsTrackImage)
                        {
                            _contentPreview3D.RenderTrackImage(previewInfo.ModelIndex);
                        }
                        else
                        {
                            // Render 3D model
                            // Specify parameter types to resolve ambiguity between the two Render overloads
                            var renderMethod = typeof(IContentPreview3D)
                                .GetMethod(nameof(IContentPreview3D.Render), new[] { typeof(int), typeof(float?) })
                                ?.MakeGenericMethod(previewInfo.CategoryType);
                            
                            // Pass ModelIndex and CustomScale
                            renderMethod?.Invoke(_contentPreview3D, new object?[] { previewInfo.ModelIndex, previewInfo.CustomScale });
                        }
                        
                        // End preview 2D context
                        _renderer.EndFrame2D();
                    }
                }
                
                // 2D menu UI
                _renderer.Setup2DRendering();
                _renderer.SetDepthTest(false);
                _renderer.SetPassthroughProjection(false);
                _menuRenderer.RenderMenu(_menuManager);
                _renderer.EndFrame2D();
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

        [ExcludeFromCodeCoverage]
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            
            // Atualizar viewport do OpenGL
            GL.Viewport(0, 0, e.Width, e.Height);
            
            // Update renderer dimensions
            if (_renderer != null)
            {
                _renderer.UpdateScreenSize(e.Width, e.Height);
                
                // Update UIHelper window size (critical for text centering!)
                UIHelper.SetWindowSize(e.Width, e.Height);
                
                // Recreate menu renderer with new dimensions for proper positioning
                _menuRenderer.SetWindowSize(e.Width, e.Height);
                _logger.LogDebug("MenuRenderer and UIHelper updated for new window size: {Width}x{Height}", e.Width, e.Height);
            }
        }

        [ExcludeFromCodeCoverage]
        private void LoadMenuBackground()
        {
            try
            {
                string timPath = AssetPaths.GetTexturePath(AssetPaths.GetWipeoutAssetsPath(), "wipeout1.tim");
                if (File.Exists(timPath))
                {
                    // Preserve TIM transparency when loading the menu background
                    var (pixels, width, height) = _timLoader.LoadTim(timPath, true);
                    
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
            _introVideoPlayer?.Dispose();
            _renderer?.Cleanup();
        }

        public override void Run()
        {
            base.Run();
        }

        /// <summary>
        /// Handles intro video mode updates and transitions.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void UpdateIntroMode(KeyboardState keyboardState)
        {
            if (_gameState?.CurrentMode == GameMode.Intro && _introVideoPlayer != null)
            {
                if (InputManager.IsActionPressed(GameAction.MenuSelect, keyboardState))
                {
                    _introVideoPlayer.Skip();
                    _gameState.CurrentMode = GameMode.SplashScreen;
                    _musicPlayer?.SetMode(MusicMode.Random); // Start music when skipping intro
                    _logger.LogInformation("Saltando para splash screen...");
                }
            }
        }

        /// <summary>
        /// Handles splash screen mode updates and transitions.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void UpdateSplashScreenMode(KeyboardState keyboardState, float deltaTime)
        {
            if (_gameState?.CurrentMode != GameMode.SplashScreen)
                return;

            _titleScreen.Update(deltaTime, out bool shouldStartAttract, out bool shouldStartMenu);
            
            if (InputManager.IsActionPressed(GameAction.MenuSelect, keyboardState))
            {
                _gameState.CurrentMode = GameMode.Menu;
                
                // Initialize preview for menu - position objects at origin
                _contentPreview3D.SetShipPosition(0, 0, 0);
                _contentPreview3D.SetRotationSpeed(0.015f);
                
                // Pass dependencies to MainMenuPages
                MainMenuPages.GameStateRef = _gameState as GameState;
                MainMenuPages.ContentPreview3DRef = _contentPreview3D;
                MainMenuPages.OptionsFactoryRef = _optionsFactory;
                MainMenuPages.SettingsPersistenceServiceRef = _settingsPersistenceService;
                MainMenuPages.QuitGameAction = () => Close();
                
                _menuManager?.PushPage(MainMenuPages.CreateMainMenu());
                
                _logger.LogInformation("Entering main menu: {Title}, {Count} items", 
                    _menuManager?.CurrentPage?.Title ?? "<no page>", _menuManager?.CurrentPage?.Items?.Count ?? 0);
                InputManager.Update(keyboardState);
            }
            else if (shouldStartAttract)
            {
                _gameState.CurrentMode = GameMode.AttractMode;
                _creditsScreen.Reset();
                _logger.LogInformation("Starting attract mode (credits)...");
            }
        }

        /// <summary>
        /// Handles attract mode updates and transitions.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void UpdateAttractMode(KeyboardState keyboardState, float deltaTime)
        {
            if (_gameState?.CurrentMode != GameMode.AttractMode)
                return;

            _creditsScreen.Update(deltaTime);
            
            // Any key returns to splash screen
            if (keyboardState.IsAnyKeyDown)
            {
                _gameState.CurrentMode = GameMode.SplashScreen;
                _titleScreen.Reset();
                _logger.LogInformation("Returning to splash screen...");
            }
        }

        /// <summary>
        /// Handles menu mode input and state transitions.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void UpdateMenuMode(KeyboardState keyboardState, float deltaTime)
        {
            if (_gameState.CurrentMode != GameMode.Menu)
                return;

            _menuManager.Update(deltaTime);
            
            var page = _menuManager.CurrentPage;
            
            // Special handling for "AWAITING INPUT" (control remapping)
            bool isAwaitingInput = page?.Title == "AWAITING INPUT";
            
            if (isAwaitingInput)
            {
                HandleAwaitingInputMode(keyboardState, deltaTime);
                return;
            }
            
            // Special handling for Best Times Viewer
            bool isBestTimesViewer = page?.Title?.Contains("BEST TIME TRIAL TIMES") == true || 
                                    page?.Title?.Contains("BEST RACE TIMES") == true;
            
            if (isBestTimesViewer)
            {
                HandleBestTimesViewerNavigation(keyboardState);
            }
            else
            {
                HandleNormalMenuNavigation(keyboardState);
            }
        }

        /// <summary>
        /// Handles awaiting input mode (control remapping).
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void HandleAwaitingInputMode(KeyboardState keyboardState, float deltaTime)
        {
            bool stillWaiting = MainMenuPages.UpdateAwaitingInput(deltaTime);
            
            if (!stillWaiting)
            {
                _menuManager.PopPage();
                _logger.LogInformation("Control remap timeout - returning to controls menu");
                return;
            }
            
            bool waitingForRelease = MainMenuPages.UpdateKeyReleaseState(keyboardState.IsAnyKeyDown);
            
            if (!waitingForRelease)
            {
                if (keyboardState.IsKeyDown(Keys.Escape))
                {
                    _menuManager.PopPage();
                    _logger.LogInformation("Control remap cancelled");
                }
                else if (keyboardState.IsAnyKeyDown)
                {
                    foreach (Keys key in Enum.GetValues(typeof(Keys)))
                    {
                        if (key == Keys.Unknown || key == Keys.Escape)
                            continue;
                            
                        if (keyboardState.IsKeyDown(key))
                        {
                            uint buttonCode = InputManager.MapKeyToButtonCode(key);
                            if (buttonCode != 0)
                            {
                                MainMenuPages.CaptureButtonForControl(buttonCode, true);
                                _menuManager.PopPage();
                                _logger.LogInformation("Control remapped to key: {Key} (code: {Code})", key, buttonCode);
                                break;
                            }
                        }
                    }
                }
            }
            
            InputManager.Update(keyboardState);
        }

        /// <summary>
        /// Handles Best Times Viewer navigation.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void HandleBestTimesViewerNavigation(KeyboardState keyboardState)
        {
            if (InputManager.IsActionPressed(GameAction.MenuUp, keyboardState))
                MainMenuPages.HandleBestTimesViewerInput(BestTimesViewerAction.PreviousClass);
            if (InputManager.IsActionPressed(GameAction.MenuDown, keyboardState))
                MainMenuPages.HandleBestTimesViewerInput(BestTimesViewerAction.NextClass);
            if (InputManager.IsActionPressed(GameAction.MenuLeft, keyboardState))
                MainMenuPages.HandleBestTimesViewerInput(BestTimesViewerAction.PreviousCircuit);
            if (InputManager.IsActionPressed(GameAction.MenuRight, keyboardState))
                MainMenuPages.HandleBestTimesViewerInput(BestTimesViewerAction.NextCircuit);
        }

        /// <summary>
        /// Handles normal menu navigation.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void HandleNormalMenuNavigation(KeyboardState keyboardState)
        {
            if (InputManager.IsActionPressed(GameAction.MenuUp, keyboardState))
            {
                _menuManager.HandleInput(MenuAction.Up);
                _logger.LogDebug("Menu: UP pressed, selected={Selected}", _menuManager.CurrentPage?.SelectedIndex);
            }
            if (InputManager.IsActionPressed(GameAction.MenuDown, keyboardState))
            {
                _menuManager.HandleInput(MenuAction.Down);
                _logger.LogDebug("Menu: DOWN pressed, selected={Selected}", _menuManager.CurrentPage?.SelectedIndex);
            }
            if (InputManager.IsActionPressed(GameAction.MenuLeft, keyboardState))
                _menuManager.HandleInput(MenuAction.Left);
            if (InputManager.IsActionPressed(GameAction.MenuRight, keyboardState))
                _menuManager.HandleInput(MenuAction.Right);
            
            if (InputManager.IsActionPressed(GameAction.MenuSelect, keyboardState))
            {
                var item = _menuManager.CurrentPage?.SelectedItem;
                _logger.LogInformation("Menu: ENTER pressed on '{Title}', item {Index}: {Label}", 
                    _menuManager.CurrentPage?.Title, _menuManager.CurrentPage?.SelectedIndex, item?.Label);
                if (item != null && item.IsEnabled)
                    _menuManager.HandleInput(MenuAction.Select);
            }
            if (InputManager.IsActionPressed(GameAction.MenuBack, keyboardState))
            {
                if (_menuManager.CurrentPage != null && _menuManager.HandleInput(MenuAction.Back))
                {
                    _logger.LogInformation("Menu: BACKSPACE - returned to previous page");
                }
                else
                {
                    _gameState.CurrentMode = GameMode.SplashScreen;
                    _titleScreen.Reset();
                    _logger.LogInformation("Menu: BACKSPACE - returning to title screen...");
                }
            }
            if (InputManager.IsActionPressed(GameAction.Exit, keyboardState))
            {
                _gameState.CurrentMode = GameMode.SplashScreen;
                _titleScreen.Reset();
                _logger.LogInformation("Menu: ESC - returning to title screen...");
            }
        }
    }
}
