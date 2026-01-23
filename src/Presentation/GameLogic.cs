using Microsoft.Extensions.Logging;
using OpenTK.Windowing.GraphicsLibraryFramework;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.Audio;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.Input;
using WipeoutRewrite.Infrastructure.UI;
using WipeoutRewrite.Infrastructure.Video;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Presentation.Menus;

namespace WipeoutRewrite.Presentation;

/// <summary>
/// Pure game logic (no GameWindow inheritance).
/// Handles state machine, updates, and rendering coordination.
/// 100% testable.
/// </summary>
public class GameLogic : IDisposable
{
    private readonly ILogger<GameLogic> _logger;
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

    private float _spriteX, _spriteY = 0;
    private int _menuBackgroundTexture;
    private bool _menuBackgroundLoaded;

    public GameLogic(
        ILogger<GameLogic> logger,
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
        IBestTimesManager bestTimesManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _introVideoPlayer = introVideoPlayer ?? throw new ArgumentNullException(nameof(introVideoPlayer));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _musicPlayer = musicPlayer ?? throw new ArgumentNullException(nameof(musicPlayer));
        _assetLoader = assetLoader ?? throw new ArgumentNullException(nameof(assetLoader));
        _fontSystem = fontSystem ?? throw new ArgumentNullException(nameof(fontSystem));
        _menuManager = menuManager ?? throw new ArgumentNullException(nameof(menuManager));
        _menuRenderer = menuRenderer ?? throw new ArgumentNullException(nameof(menuRenderer));
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        _timLoader = timLoader ?? throw new ArgumentNullException(nameof(timLoader));
        _attractMode = attractMode ?? throw new ArgumentNullException(nameof(attractMode));
        _contentPreview3D = contentPreview3D ?? throw new ArgumentNullException(nameof(contentPreview3D));
        _titleScreen = titleScreen ?? throw new ArgumentNullException(nameof(titleScreen));
        _creditsScreen = creditsScreen ?? throw new ArgumentNullException(nameof(creditsScreen));
        _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
        _settingsPersistenceService = settingsPersistenceService ?? throw new ArgumentNullException(nameof(settingsPersistenceService));
        _bestTimesManager = bestTimesManager ?? throw new ArgumentNullException(nameof(bestTimesManager));
    }

    public void Initialize(int windowWidth, int windowHeight)
    {
        _renderer.Init(windowWidth, windowHeight);

        string assetsPath = AssetPaths.GetWipeoutAssetsPath();
        _assetLoader.Initialize(assetsPath);
        _fontSystem.LoadFonts(assetsPath);

        if (_assetLoader.LoadTrackList() is { Count: > 0 } tracks)
        {
            _logger.LogInformation("Loaded tracks: {Tracks}", string.Join(", ", tracks));
            _gameState.Initialize(playerShipId: 0);
        }

        _menuRenderer.SetWindowSize(windowWidth, windowHeight);
        _titleScreen.Initialize();

        MainMenuPages.GameStateRef = _gameState as GameState;
        MainMenuPages.OptionsFactoryRef = _optionsFactory;
        MainMenuPages.SettingsPersistenceServiceRef = _settingsPersistenceService;
        MainMenuPages.BestTimesManagerRef = _bestTimesManager;

        LoadMenuBackground();

        string musicPath = AssetPaths.GetMusicPath(AssetPaths.GetWipeoutAssetsPath());
        _musicPlayer.LoadTracks(musicPath);

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
            _musicPlayer?.SetMode(MusicMode.Random);
        }
    }

    public void UpdateFrame(float deltaTime, OpenTK.Mathematics.Vector2i windowSize, KeyboardState keyboard)
    {
        MainMenuPages.CurrentKeyboardState = keyboard;

        // Calculate UI scale dynamically
        int autoScale = GameStateTransitions.CalculateAutoUIScale(windowSize.Y, 0);
        var videoSettings = _optionsFactory.CreateVideoSettings();
        int finalScale = videoSettings.UIScale == 0 ? autoScale : Math.Min((int)videoSettings.UIScale, autoScale);
        UIHelper.SetUIScale(finalScale);

        _musicPlayer?.Update(deltaTime);

        // Exit only when NOT in menu
        if (_gameState?.CurrentMode != GameMode.Menu && InputManager.IsActionPressed(GameAction.Exit, keyboard))
        {
            // Signal to close (handled by Game)
            return;
        }

        // State transitions
        if (_gameState?.CurrentMode == GameMode.Intro)
        {
            if (InputManager.IsActionPressed(GameAction.MenuSelect, keyboard))
            {
                _introVideoPlayer.Skip();
                _gameState.CurrentMode = GameMode.SplashScreen;
                _musicPlayer?.SetMode(MusicMode.Random);
                _logger.LogInformation("Saltando para splash screen...");
            }
        }
        else if (_gameState?.CurrentMode == GameMode.SplashScreen)
        {
            _titleScreen.Update(deltaTime, out bool shouldStartAttract, out bool shouldStartMenu);

            if (InputManager.IsActionPressed(GameAction.MenuSelect, keyboard))
            {
                _gameState.CurrentMode = GameMode.Menu;
                _contentPreview3D.SetShipPosition(0, 0, 0);
                _contentPreview3D.SetRotationSpeed(0.015f);

                MainMenuPages.GameStateRef = _gameState as GameState;
                MainMenuPages.ContentPreview3DRef = _contentPreview3D;
                MainMenuPages.OptionsFactoryRef = _optionsFactory;
                MainMenuPages.SettingsPersistenceServiceRef = _settingsPersistenceService;
                MainMenuPages.QuitGameAction = () => { /* Handled by Game */ };

                _menuManager?.PushPage(MainMenuPages.CreateMainMenu());
                _logger.LogInformation("Entering main menu");
                InputManager.Update(keyboard);
            }
            else if (shouldStartAttract)
            {
                _gameState.CurrentMode = GameMode.AttractMode;
                _creditsScreen.Reset();
                _logger.LogInformation("Starting attract mode (credits)...");
            }
        }

        // Menu logic
        if (_gameState?.CurrentMode == GameMode.Menu)
        {
            _menuManager.Update(deltaTime);

            var page = _menuManager.CurrentPage;
            bool isAwaitingInput = page?.Title == "AWAITING INPUT";

            if (isAwaitingInput)
            {
                // Input remapping logic - simplified for now
                _logger.LogDebug("Menu awaiting input");
            }
            else
            {
                bool isBestTimesViewer = GameStateTransitions.IsBestTimesViewerMode(page?.Title);

                if (isBestTimesViewer)
                {
                    if (InputManager.IsActionPressed(GameAction.MenuUp, keyboard))
                        MainMenuPages.HandleBestTimesViewerInput(BestTimesViewerAction.PreviousClass);
                    if (InputManager.IsActionPressed(GameAction.MenuDown, keyboard))
                        MainMenuPages.HandleBestTimesViewerInput(BestTimesViewerAction.NextClass);
                    if (InputManager.IsActionPressed(GameAction.MenuLeft, keyboard))
                        MainMenuPages.HandleBestTimesViewerInput(BestTimesViewerAction.PreviousCircuit);
                    if (InputManager.IsActionPressed(GameAction.MenuRight, keyboard))
                        MainMenuPages.HandleBestTimesViewerInput(BestTimesViewerAction.NextCircuit);
                }
                else
                {
                    if (InputManager.IsActionPressed(GameAction.MenuUp, keyboard))
                        _menuManager.HandleInput(MenuAction.Up);
                    if (InputManager.IsActionPressed(GameAction.MenuDown, keyboard))
                        _menuManager.HandleInput(MenuAction.Down);
                    if (InputManager.IsActionPressed(GameAction.MenuLeft, keyboard))
                        _menuManager.HandleInput(MenuAction.Left);
                    if (InputManager.IsActionPressed(GameAction.MenuRight, keyboard))
                        _menuManager.HandleInput(MenuAction.Right);
                }

                if (InputManager.IsActionPressed(GameAction.MenuSelect, keyboard))
                {
                    var item = page?.SelectedItem;
                    if (item != null && item.IsEnabled)
                        _menuManager.HandleInput(MenuAction.Select);
                }
                if (InputManager.IsActionPressed(GameAction.MenuBack, keyboard))
                {
                    if (_menuManager.CurrentPage != null && _menuManager.HandleInput(MenuAction.Back))
                    {
                        _logger.LogInformation("Menu: returned to previous page");
                    }
                    else
                    {
                        _gameState.CurrentMode = GameMode.SplashScreen;
                        _titleScreen.Reset();
                    }
                }
                if (InputManager.IsActionPressed(GameAction.Exit, keyboard))
                {
                    _gameState.CurrentMode = GameMode.SplashScreen;
                    _titleScreen.Reset();
                }
            }
        }

        // Attract mode
        if (_gameState?.CurrentMode == GameMode.AttractMode)
        {
            _creditsScreen.Update(deltaTime);

            if (keyboard.IsAnyKeyDown)
            {
                _gameState.CurrentMode = GameMode.SplashScreen;
                _titleScreen.Reset();
                _logger.LogInformation("Exiting attract mode...");
            }

            _attractMode?.Update(deltaTime);
        }

        // Update game state
        _gameState?.Update(deltaTime);
        _gameState?.SetPlayerShip(
            InputManager.IsActionDown(GameAction.Accelerate, keyboard),
            InputManager.IsActionDown(GameAction.Brake, keyboard),
            InputManager.IsActionDown(GameAction.TurnLeft, keyboard),
            InputManager.IsActionDown(GameAction.TurnRight, keyboard),
            InputManager.IsActionDown(GameAction.BoostLeft, keyboard),
            InputManager.IsActionDown(GameAction.BoostRight, keyboard)
        );

        // Sprite movement
        float speed = 5f;
        if (InputManager.IsActionDown(GameAction.Accelerate, keyboard))
            _spriteY -= speed;
        if (InputManager.IsActionDown(GameAction.Brake, keyboard))
            _spriteY += speed;
        if (InputManager.IsActionDown(GameAction.TurnLeft, keyboard))
            _spriteX -= speed;
        if (InputManager.IsActionDown(GameAction.TurnRight, keyboard))
            _spriteX += speed;

        float spriteSize = 128;
        _spriteX = Math.Max(0, Math.Min(_spriteX, windowSize.X - spriteSize));
        _spriteY = Math.Max(0, Math.Min(_spriteY, windowSize.Y - spriteSize));

        InputManager.Update(keyboard);
    }

    public void RenderFrame(int clientWidth, int clientHeight)
    {
        if (_renderer == null) return;

        if (_gameState.CurrentMode == GameMode.Intro && _introVideoPlayer != null)
        {
            if (_introVideoPlayer.IsPlaying)
            {
                _introVideoPlayer.Update();
                byte[]? frameData = _introVideoPlayer.GetCurrentFrameData();
                if (frameData != null && frameData.Length > 0)
                {
                    _renderer.BeginFrame();
                    _renderer.Setup2DRendering();
                    _renderer.RenderVideoFrame(frameData, _introVideoPlayer.GetWidth(), _introVideoPlayer.GetHeight(), clientWidth, clientHeight);
                    _renderer.EndFrame2D();
                }
            }
            else
            {
                _introVideoPlayer.Dispose();
                _gameState.CurrentMode = GameMode.SplashScreen;
                _musicPlayer?.SetMode(MusicMode.Random);
                _logger.LogInformation("Intro finished");
            }
        }
        else if (_gameState.CurrentMode == GameMode.SplashScreen)
        {
            _titleScreen.Render(clientWidth, clientHeight);
        }
        else if (_gameState.CurrentMode == GameMode.AttractMode)
        {
            _creditsScreen.Render(clientWidth, clientHeight);
        }
        else if (_gameState.CurrentMode == GameMode.Menu)
        {
            _renderer.BeginFrame();
            _renderer.Setup2DRendering();
            if (_menuBackgroundLoaded)
            {
                _renderer.SetCurrentTexture(_menuBackgroundTexture);
                _renderer.PushSprite(0, 0, clientWidth, clientHeight, new OpenTK.Mathematics.Vector4(1, 1, 1, 1));
            }
            _renderer.EndFrame2D();

            // Render preview (3D model or 2D track image) in its own 2D context
            var currentPage = _menuManager.CurrentPage;
            if (currentPage != null && currentPage.SelectedItem?.PreviewInfo != null)
            {
                var previewInfo = currentPage.SelectedItem.PreviewInfo;
                
                // Setup 2D context for preview rendering
                _renderer.Setup2DRendering();
                _renderer.SetDepthTest(false);
                
                // Check if this is a track image preview
                if (previewInfo.IsTrackImage)
                {
                    _contentPreview3D.RenderTrackImage(previewInfo.ModelIndex);
                }
                else
                {
                    // Render 3D model
                    var renderMethod = typeof(IContentPreview3D)
                        .GetMethod(nameof(IContentPreview3D.Render), new[] { typeof(int), typeof(float?) })
                        ?.MakeGenericMethod(previewInfo.CategoryType);
                    renderMethod?.Invoke(_contentPreview3D, new object?[] { previewInfo.ModelIndex, previewInfo.CustomScale });
                }
                
                // End preview 2D context
                _renderer.EndFrame2D();
            }

            // Render menu UI in final 2D context
            _renderer.Setup2DRendering();
            _renderer.SetDepthTest(false);
            _renderer.SetPassthroughProjection(false);
            _menuRenderer.RenderMenu(_menuManager);
            _renderer.EndFrame2D();
        }
        else if (_gameState.CurrentMode == GameMode.AttractMode && _attractMode != null)
        {
            _renderer.BeginFrame();
            _attractMode.Render(_renderer);
            _renderer.EndFrame();
        }
    }

    public void OnWindowResize(int width, int height)
    {
        _renderer?.UpdateScreenSize(width, height);
        UIHelper.SetWindowSize(width, height);
        _menuRenderer.SetWindowSize(width, height);
        _logger.LogDebug("Window resized: {Width}x{Height}", width, height);
    }

    private void LoadMenuBackground()
    {
        try
        {
            string timPath = AssetPaths.GetTexturePath(AssetPaths.GetWipeoutAssetsPath(), "wipeout1.tim");
            if (System.IO.File.Exists(timPath))
            {
                var (pixels, width, height) = _timLoader.LoadTim(timPath, true);
                _menuBackgroundTexture = _renderer.CreateTexture(pixels, width, height);
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

    public void Run()
    {
        // No-op: Run() is called by Game class (GameWindow adapter)
        // GameLogic doesn't manage the game loop; it's purely logic-based
    }

    public void Dispose()
    {
        // Cleanup if needed for future resource management
        // Currently no unmanaged resources; can be extended if needed
    }
}
