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

namespace WipeoutRewrite;

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
    private readonly IMenuManager _menuManager;
    private readonly IGameState _gameState;
    private readonly ITimImageLoader _timLoader;
    private readonly ITitleScreen _titleScreen;
    private readonly IAttractMode _attractMode;
    private readonly ICreditsScreen _creditsScreen;
    private readonly IContentPreview3D _contentPreview3D;
    private readonly IMusicPlayer _musicPlayer;
    private readonly IOptionsFactory _optionsFactory;
    private readonly ISettingsPersistenceService _settingsPersistenceService;
    private readonly IBestTimesManager _bestTimesManager;
    private readonly IMenuBuilder _menuBuilder;
    private readonly IMenuActionHandler _menuActionHandler;
    private readonly IGameDataService _gameDataService;
    private readonly IMainMenuPages _mainMenuPages;
    private readonly IMenuPageRenderer _menuPageRenderer;
    
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
        ISettingsPersistenceService settingsPersistenceService,
        IBestTimesManager bestTimesManager,
        IMenuBuilder menuBuilder,
        IMenuActionHandler menuActionHandler,
        IGameDataService gameDataService,
        IMainMenuPages mainMenuPages,
        IMenuPageRenderer menuPageRenderer
        )
        : base(gws, nws)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _introVideoPlayer = introVideoPlayer ?? throw new ArgumentNullException(nameof(introVideoPlayer));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _musicPlayer = musicPlayer ?? throw new ArgumentNullException(nameof(musicPlayer));
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
        _mainMenuPages = mainMenuPages ?? throw new ArgumentNullException(nameof(mainMenuPages));
        _menuPageRenderer = menuPageRenderer ?? throw new ArgumentNullException(nameof(menuPageRenderer));
    }

    [ExcludeFromCodeCoverage]
    protected override void OnLoad()
    {
        base.OnLoad();

        // renderer initialization with window size
        _renderer.Init(Size.X, Size.Y);

        // AssetLoader initialization with local assets path
        string assetsPath = AssetPaths.GetAssetsRootPath();

        // Load fonts
        _fontSystem.LoadFonts(assetsPath);

        // Initialize IGameState with first track
        _gameState.Initialize();

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
                    
        // Initialize MenuBuilder with game data
        _menuBuilder.Initialize(_gameDataService);
        
        // Load menu background texture (wipeout1.tim)
        LoadMenuBackground();

        // Initialize music
        string musicPath = AssetPaths.GetMusicPath(AssetPaths.GetWipeoutAssetsPath());
        _musicPlayer.LoadTracks(musicPath);

        // Load and start intro video AFTER everything is ready (OpenGL is now initialized)
        _logger.LogInformation("Loading intro video...");
        try
        {
            _introVideoPlayer.Play();
            _gameState.CurrentMode = GameMode.Intro;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading video: {Message}", ex.Message);
            _gameState.CurrentMode = GameMode.SplashScreen;
            _musicPlayer.SetMode(MusicMode.Random); // Start music on splash if intro fails
        }
    }

    [ExcludeFromCodeCoverage]
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        // Update MainMenuPages with current keyboard state for input handling
        _mainMenuPages.CurrentKeyboardState = KeyboardState;

        // Calculate UI scale dynamically based on window height (matching C code)
        // Formula from game.c: scale = max(1, sh >= 720 ? sh / 360 : sh / 240)
        int screenHeight = Size.Y;
        int autoScale = Math.Max(1, screenHeight >= 720 ? screenHeight / 360 : screenHeight / 240);

        // If user has set manual scale (not 0), cap the auto scale
        var videoSettings = _optionsFactory.CreateVideoSettings();
        int finalScale = videoSettings.UIScale == 0 ? autoScale : Math.Min((int)videoSettings.UIScale, autoScale);
        UIHelper.SetUIScale(finalScale);

        // Update music
        _musicPlayer.Update((float)args.Time);

        // Process game actions - Exit only when NOT in menu (menu uses ESC for back)
        if (_gameState.CurrentMode != GameMode.Menu && 
            InputManager.IsActionPressed(GameAction.Exit, KeyboardState))
        {
            Close();
        }

        // Toggle fullscreen com F11
        ToogleFullscreen();

        // Skip intro with Enter
        UpdateIntroMode(KeyboardState);

        // Splash screen logic
        UpdateSplashScreenMode(KeyboardState, (float)args.Time);

        // Attract mode (credits)
        UpdateAttractMode(KeyboardState, (float)args.Time);

        // Menu navigation
        UpdateMenuMode(KeyboardState, (float)args.Time);

        // Update game state
        _gameState.Update((float)args.Time);

        // TODO: Update game logic here (physics, AI)
        // Update input state at the END of frame for next frame's comparison
        InputManager.Update(KeyboardState);
    }

    private void ToogleFullscreen()
    {
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
    }

    [ExcludeFromCodeCoverage]
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        if (_renderer == null) return;

        if (_gameState.CurrentMode == GameMode.Intro)
        {
            // Render intro video if playing
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
                // Start music on splash
                _musicPlayer.SetMode(MusicMode.Random);
                _logger.LogDebug("Intro finished, showing splash screen...");
            }
        }
        else if (_gameState.CurrentMode == GameMode.SplashScreen)
        {
            // Render title screen
            _titleScreen.Render(ClientSize.X, ClientSize.Y);
        }
        else if (_gameState.CurrentMode == GameMode.AttractMode)
        {
            // Render attract mode (credits)
            _attractMode.Render(_renderer);
            // Render credits
            _creditsScreen.Render(ClientSize.X, ClientSize.Y);
        }
        else if (_gameState.CurrentMode == GameMode.Menu)
        {
            // Render menu with ship in background
            _renderer.BeginFrame();
            RenderMenuBackground();
            
            // 3D preview of selected menu item
            var currentPage = _menuManager.CurrentPage;
            if (currentPage != null)
            {
                _menuPageRenderer.UpdateDynamicPreview(
                    currentPage,
                    currentPage.Id ?? string.Empty,
                    _gameDataService);

                Render3DViewPort(currentPage);
            }
            
            // 2D menu UI
            _renderer.Setup2DRendering();
            _renderer.SetDepthTest(false);
            _renderer.SetPassthroughProjection(false);
            _menuRenderer.RenderMenu(_menuManager);
            _renderer.EndFrame2D();
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
        _introVideoPlayer.Dispose();
        _renderer.Cleanup();
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
        if (_gameState.CurrentMode != GameMode.Intro) 
            return;

        bool menuSelectPressed = InputManager.IsActionPressed(GameAction.MenuSelect, keyboardState);
        
        var (nextMode, action) = GameStateTransitions.GetNextMode(
            currentMode: _gameState.CurrentMode,
            menuSelectPressed: menuSelectPressed,
            menuBackPressed: false,
            menuUpPressed: false,
            menuDownPressed: false,
            exitPressed: false,
            anyKeyDown: false,
            attractTimeElapsed: false,
            timeSinceAttractTrigger: 0);
        
        if (action == GameStateTransitions.TransitionResult.TransitionTo && nextMode == GameMode.SplashScreen)
        {
            _introVideoPlayer.Skip();
            _gameState.CurrentMode = nextMode;
            _musicPlayer.SetMode(MusicMode.Random);
            _logger.LogInformation("Saltando para splash screen...");
            InputManager.Update(keyboardState);
        }
    }

    /// <summary>
    /// Handles splash screen mode updates and transitions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private void UpdateSplashScreenMode(KeyboardState keyboardState, float deltaTime)
    {
        if (_gameState.CurrentMode != GameMode.SplashScreen)
            return;

        _titleScreen.Update(deltaTime, out bool shouldStartAttract, out bool shouldStartMenu);
        
        bool menuSelectPressed = InputManager.IsActionPressed(GameAction.MenuSelect, keyboardState);
        
        var (nextMode, action) = GameStateTransitions.GetNextMode(
            currentMode: _gameState.CurrentMode,
            menuSelectPressed: menuSelectPressed,
            menuBackPressed: false,
            menuUpPressed: false,
            menuDownPressed: false,
            exitPressed: false,
            anyKeyDown: false,
            attractTimeElapsed: shouldStartAttract,
            timeSinceAttractTrigger: 0);
        
        if (action == GameStateTransitions.TransitionResult.TransitionTo)
        {
            _gameState.CurrentMode = nextMode;
            
            if (nextMode == GameMode.Menu)
            {
                // Initialize preview for menu - position objects at origin
                _contentPreview3D.SetShipPosition(0, 0, 0);
                _contentPreview3D.SetRotationSpeed(0.015f);
                
                _mainMenuPages.QuitGameActionCallBack = () => Close();
                _menuManager.PushPage(_mainMenuPages.CreateMainMenu());
                
                _logger.LogInformation("Entering main menu: {Title}, {Count} items", 
                    _menuManager.CurrentPage?.Title ?? "<no page>", _menuManager.CurrentPage?.Items?.Count ?? 0);
                InputManager.Update(keyboardState);
            }
            else if (nextMode == GameMode.AttractMode)
            {
                _creditsScreen.Reset();
                _logger.LogInformation("Starting attract mode (credits)...");
            }
        }
    }

    /// <summary>
    /// Handles attract mode updates and transitions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private void UpdateAttractMode(KeyboardState keyboardState, float deltaTime)
    {
        if (_gameState.CurrentMode != GameMode.AttractMode)
            return;

        _creditsScreen.Update(deltaTime);
        
        var (nextMode, action) = GameStateTransitions.GetNextMode(
            currentMode: _gameState.CurrentMode,
            menuSelectPressed: false,
            menuBackPressed: false,
            menuUpPressed: false,
            menuDownPressed: false,
            exitPressed: false,
            anyKeyDown: keyboardState.IsAnyKeyDown,
            attractTimeElapsed: false,
            timeSinceAttractTrigger: 0);
        
        if (action == GameStateTransitions.TransitionResult.TransitionTo && nextMode == GameMode.SplashScreen)
        {
            _gameState.CurrentMode = nextMode;
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
        bool isAwaitingInput = page?.Id == MenuPageIds.AwaitingInput;
        
        if (isAwaitingInput)
        {
            HandleAwaitingInputMode(keyboardState, deltaTime);
            return;
        }
        
        // Special handling for Best Times Viewer
        bool isBestTimesViewer = page?.Id == MenuPageIds.BestTimesViewer;
        
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
        bool stillWaiting = _mainMenuPages.UpdateAwaitingInput(deltaTime);
        
        if (!stillWaiting)
        {
            _menuManager.PopPage();
            _logger.LogInformation("Control remap timeout - returning to controls menu");
            return;
        }
        
        bool waitingForRelease = _mainMenuPages.UpdateKeyReleaseState(keyboardState.IsAnyKeyDown);
        
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
                            _mainMenuPages.CaptureButtonForControl(buttonCode, true);
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
        // Handle navigation
        if (InputManager.IsActionPressed(GameAction.MenuUp, keyboardState))
            _mainMenuPages.HandleBestTimesViewerInput(BestTimesViewerAction.PreviousClass);
        if (InputManager.IsActionPressed(GameAction.MenuDown, keyboardState))
            _mainMenuPages.HandleBestTimesViewerInput(BestTimesViewerAction.NextClass);
        if (InputManager.IsActionPressed(GameAction.MenuLeft, keyboardState))
            _mainMenuPages.HandleBestTimesViewerInput(BestTimesViewerAction.PreviousCircuit);
        if (InputManager.IsActionPressed(GameAction.MenuRight, keyboardState))
            _mainMenuPages.HandleBestTimesViewerInput(BestTimesViewerAction.NextCircuit);
        
        // Handle Back/ESC to exit viewer
        bool menuBackPressed = InputManager.IsActionPressed(GameAction.MenuBack, keyboardState);
        bool exitPressed = InputManager.IsActionPressed(GameAction.Exit, keyboardState);
        
        if (GameStateTransitions.ShouldPopMenu(
            _menuManager.CurrentPage?.Id,
            menuBackPressed,
            exitPressed,
            _menuManager.CurrentPage != null))
        {
            _menuManager.PopPage();
            _logger.LogInformation("Best Times Viewer: Returning to options menu");
        }
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

    /// <summary>
    /// Renders the menu background texture.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private void RenderMenuBackground()
    {
        _renderer.Setup2DRendering();
        if (_menuBackgroundLoaded)
        {
            _renderer.SetCurrentTexture(_menuBackgroundTexture);
            _renderer.PushSprite(0, 0, ClientSize.X, ClientSize.Y, new OpenTK.Mathematics.Vector4(1, 1, 1, 1));
        }
        _renderer.EndFrame2D();
    }

    /// <summary>
    /// Renders viewport items (3D models or track images) for the current menu page.
    /// Handles both single-preview and multi-preview layouts.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private void Render3DViewPort(MenuPage currentPage)
    {
        // Check for multi-preview layout
        if (currentPage?.PreviewLayout != null && currentPage.PreviewLayout.Previews.Count > 0)
        {
            // Render multiple previews with position-based viewport switching
            foreach (var preview in currentPage.PreviewLayout.Previews)
            {
                _renderer.Setup2DRendering();
                _renderer.SetDepthTest(false);
                
                PreviewPositionHelper.ApplyPositionLayout(preview.Position, ClientSize.X, ClientSize.Y);
                RenderViewPortItem(preview.Info);
                
                GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
                _renderer.EndFrame2D();
            }
        }
        else if (currentPage?.SelectedItem?.ContentViewPort != null)
        {
            // Render single preview item
            _renderer.Setup2DRendering();
            _renderer.SetDepthTest(false);
            RenderViewPortItem(currentPage.SelectedItem.ContentViewPort);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            _renderer.EndFrame2D();
        }
    }

    /// <summary>
    /// Renders a single viewport item (3D model or track image).
    /// </summary>
    [ExcludeFromCodeCoverage]
    private void RenderViewPortItem(ContentPreview3DInfo contentViewPort)
    {
        if (contentViewPort.IsTrackImage)
        {
            _contentPreview3D.RenderTrackImage(contentViewPort.ModelIndex);
        }
        else
        {
            // Render 3D model with reflection
            var renderMethod = typeof(IContentPreview3D)
                .GetMethod(nameof(IContentPreview3D.Render), new[] { typeof(int), typeof(float?) })
                ?.MakeGenericMethod(contentViewPort.CategoryType);
            renderMethod?.Invoke(
                _contentPreview3D, 
                new object?[] { 
                    contentViewPort.ModelIndex, 
                    contentViewPort.CustomScale 
                });
        }
    }
}
