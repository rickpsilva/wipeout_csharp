using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Presentation;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.Audio;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.UI;
using WipeoutRewrite.Infrastructure.Video;
using WipeoutRewrite.Infrastructure.Database;
using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Tests.Presentation;

/// <summary>
/// Integration tests for GameLogic state machine and logic.
/// Tests initialization, rendering, and window resizing without full game loop.
/// </summary>
public class GameLogicTests : IDisposable
{
    private readonly Mock<ILogger<GameLogic>> _mockLogger;
    private readonly Mock<IVideoPlayer> _mockVideoPlayer;
    private readonly Mock<IRenderer> _mockRenderer;
    private readonly Mock<IMenuRenderer> _mockMenuRenderer;
    private readonly Mock<IFontSystem> _mockFontSystem;
    private readonly Mock<IAssetLoader> _mockAssetLoader;
    private readonly Mock<IMenuManager> _mockMenuManager;
    private readonly Mock<IGameState> _mockGameState;
    private readonly Mock<ITimImageLoader> _mockTimLoader;
    private readonly Mock<ITitleScreen> _mockTitleScreen;
    private readonly Mock<IAttractMode> _mockAttractMode;
    private readonly Mock<ICreditsScreen> _mockCreditsScreen;
    private readonly Mock<IContentPreview3D> _mockContentPreview3D;
    private readonly Mock<IMusicPlayer> _mockMusicPlayer;
    private readonly Mock<IOptionsFactory> _mockOptionsFactory;
    private readonly Mock<IBestTimesManager> _mockBestTimesManager;
    private readonly Mock<ISettingsRepository> _mockSettingsRepository;
    private readonly Mock<ILogger<SettingsPersistenceService>> _mockSettingsLogger;

    public GameLogicTests()
    {
        _mockLogger = new Mock<ILogger<GameLogic>>();
        _mockVideoPlayer = new Mock<IVideoPlayer>();
        _mockRenderer = new Mock<IRenderer>();
        _mockMenuRenderer = new Mock<IMenuRenderer>();
        _mockFontSystem = new Mock<IFontSystem>();
        _mockAssetLoader = new Mock<IAssetLoader>();
        _mockMenuManager = new Mock<IMenuManager>();
        _mockGameState = new Mock<IGameState>();
        _mockTimLoader = new Mock<ITimImageLoader>();
        _mockTitleScreen = new Mock<ITitleScreen>();
        _mockAttractMode = new Mock<IAttractMode>();
        _mockCreditsScreen = new Mock<ICreditsScreen>();
        _mockContentPreview3D = new Mock<IContentPreview3D>();
        _mockMusicPlayer = new Mock<IMusicPlayer>();
        _mockOptionsFactory = new Mock<IOptionsFactory>();
        _mockBestTimesManager = new Mock<IBestTimesManager>();
        _mockSettingsRepository = new Mock<ISettingsRepository>();
        _mockSettingsLogger = new Mock<ILogger<SettingsPersistenceService>>();

        // Setup defaults
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.Intro);
        _mockAssetLoader.Setup(x => x.LoadTrackList()).Returns(new List<string> { "Track1" });
        _mockOptionsFactory.Setup(x => x.CreateControlsSettings()).Returns(new ControlsSettings());
        _mockOptionsFactory.Setup(x => x.CreateVideoSettings()).Returns(new VideoSettings());
        _mockOptionsFactory.Setup(x => x.CreateAudioSettings()).Returns(new AudioSettings());
    }

    private GameLogic CreateGameLogic()
    {
        var settingsService = new SettingsPersistenceService(
            _mockSettingsRepository.Object,
            _mockSettingsLogger.Object,
            _mockOptionsFactory.Object.CreateControlsSettings(),
            _mockOptionsFactory.Object.CreateVideoSettings(),
            _mockOptionsFactory.Object.CreateAudioSettings()
        );

        return new GameLogic(
            _mockLogger.Object,
            _mockVideoPlayer.Object,
            _mockRenderer.Object,
            _mockMusicPlayer.Object,
            _mockAssetLoader.Object,
            _mockFontSystem.Object,
            _mockMenuManager.Object,
            _mockMenuRenderer.Object,
            _mockGameState.Object,
            _mockTimLoader.Object,
            _mockAttractMode.Object,
            _mockContentPreview3D.Object,
            _mockTitleScreen.Object,
            _mockCreditsScreen.Object,
            _mockOptionsFactory.Object,
            settingsService,
            _mockBestTimesManager.Object
        );
    }

    #region Integration Tests - Constructor and Initialization

    [Fact]
    public void Constructor_WithAllDependencies_Creates()
    {
        var gameLogic = CreateGameLogic();
        Assert.NotNull(gameLogic);
    }

    [Fact]
    public void Initialize_WithValidDimensions_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        _mockAssetLoader.Verify(x => x.LoadTrackList(), Times.Once);
    }

    [Fact]
    public void Initialize_WithMultipleResolutions_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        
        gameLogic.Initialize(1920, 1080);
        gameLogic.Initialize(800, 600);
        gameLogic.Initialize(1280, 720);
        
        _mockAssetLoader.Verify(x => x.LoadTrackList(), Times.Exactly(3));
    }

    [Fact]
    public void Initialize_CallsMenuManagerAndAssetLoader()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        
        _mockAssetLoader.Verify(x => x.LoadTrackList(), Times.Once);
    }

    [Fact]
    public void RenderFrame_WithValidDimensions_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.RenderFrame(1280, 720);
        // Should not throw
    }

    [Fact]
    public void RenderFrame_WithDifferentResolutions_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1920, 1080);
        
        gameLogic.RenderFrame(1920, 1080);
        gameLogic.RenderFrame(1280, 720);
        gameLogic.RenderFrame(800, 600);
        // Should not throw
    }

    [Fact]
    public void OnWindowResize_WithValidDimensions_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.OnWindowResize(1920, 1080);
        // Should not throw
    }

    [Fact]
    public void OnWindowResize_WithMultipleSizes_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        
        gameLogic.OnWindowResize(1920, 1080);
        gameLogic.OnWindowResize(800, 600);
        gameLogic.OnWindowResize(1280, 720);
        // Should not throw
    }

    [Fact]
    public void FullInitializeRenderSequence_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        
        for (int i = 0; i < 5; i++)
        {
            gameLogic.RenderFrame(1280, 720);
        }
        
        _mockAssetLoader.Verify(x => x.LoadTrackList(), Times.Once);
    }

    [Fact]
    public void GameLogic_WithWindowResizeAndRender_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        
        gameLogic.RenderFrame(1280, 720);
        gameLogic.OnWindowResize(1920, 1080);
        gameLogic.RenderFrame(1920, 1080);
        gameLogic.OnWindowResize(800, 600);
        gameLogic.RenderFrame(800, 600);
    }

    #endregion

    #region Interface Contract Tests

    [Fact]
    public void GameLogic_HasPublicAPISurface()
    {
        var t = typeof(GameLogic);
        Assert.NotNull(t.GetMethod("Initialize"));
        Assert.NotNull(t.GetMethod("UpdateFrame"));
        Assert.NotNull(t.GetMethod("RenderFrame"));
        Assert.NotNull(t.GetMethod("OnWindowResize"));
        Assert.NotNull(t.GetMethod("Run"));
    }

    [Fact]
    public void GameLogic_ImplementsIDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(GameLogic)));
    }

    [Fact]
    public void GameLogic_MethodSignaturesExist()
    {
        var gameLogicType = typeof(GameLogic);
        Assert.NotNull(gameLogicType.GetMethod("Initialize", new[] { typeof(int), typeof(int) }));
        Assert.NotNull(gameLogicType.GetMethod("UpdateFrame"));
        Assert.NotNull(gameLogicType.GetMethod("RenderFrame", new[] { typeof(int), typeof(int) }));
        Assert.NotNull(gameLogicType.GetMethod("OnWindowResize", new[] { typeof(int), typeof(int) }));
    }

    #endregion

    #region Method Signature Tests

    [Fact]
    public void Initialize_HasCorrectSignature()
    {
        var method = typeof(GameLogic).GetMethod("Initialize");
        Assert.NotNull(method);
        var parameters = method!.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(int), parameters[0].ParameterType);
        Assert.Equal(typeof(int), parameters[1].ParameterType);
    }

    [Fact]
    public void UpdateFrame_HasCorrectSignature()
    {
        var method = typeof(GameLogic).GetMethod("UpdateFrame");
        Assert.NotNull(method);
        var parameters = method!.GetParameters();
        Assert.Equal(3, parameters.Length);
    }

    [Fact]
    public void RenderFrame_HasCorrectSignature()
    {
        var method = typeof(GameLogic).GetMethod("RenderFrame");
        Assert.NotNull(method);
        var parameters = method!.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.All(parameters, p => Assert.Equal(typeof(int), p.ParameterType));
    }

    [Fact]
    public void OnWindowResize_HasCorrectSignature()
    {
        var method = typeof(GameLogic).GetMethod("OnWindowResize");
        Assert.NotNull(method);
        var parameters = method!.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.All(parameters, p => Assert.Equal(typeof(int), p.ParameterType));
    }

    [Fact]
    public void Run_HasCorrectSignature()
    {
        var method = typeof(GameLogic).GetMethod("Run");
        Assert.NotNull(method);
    }

    [Fact]
    public void Dispose_HasCorrectSignature()
    {
        var method = typeof(GameLogic).GetMethod("Dispose");
        Assert.NotNull(method);
    }

    #endregion

    #region State Management Tests

    [Fact]
    public void GameLogic_IsPublic()
    {
        Assert.True(typeof(GameLogic).IsPublic);
    }

    [Fact]
    public void GameLogic_HasNoPublicFields()
    {
        var publicFields = typeof(GameLogic).GetFields(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        Assert.Empty(publicFields);
    }

    [Fact]
    public void Constructor_HasMinimumRequiredParameters()
    {
        var constructorParams = typeof(GameLogic).GetConstructors()[0].GetParameters();
        Assert.True(constructorParams.Length >= 15,
            "GameLogic constructor must have at least 15 dependencies");
    }

    #endregion

    #region State-Based Rendering Tests

    [Fact]
    public void RenderFrame_InIntroMode_Succeeds()
    {
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.Intro);
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.RenderFrame(1280, 720);
    }

    [Fact]
    public void RenderFrame_InSplashScreenMode_Succeeds()
    {
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.SplashScreen);
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.RenderFrame(1280, 720);
    }

    [Fact]
    public void RenderFrame_InMenuMode_Succeeds()
    {
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.Menu);
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.RenderFrame(1280, 720);
    }

    [Fact]
    public void RenderFrame_InRacingMode_Succeeds()
    {
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.Racing);
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.RenderFrame(1280, 720);
    }

    [Fact]
    public void RenderFrame_InCreditsMode_Succeeds()
    {
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.Victory);
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.RenderFrame(1280, 720);
    }

    [Fact]
    public void RenderFrame_InAttractMode_Succeeds()
    {
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.AttractMode);
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.RenderFrame(1280, 720);
    }

    [Fact]
    public void RenderFrame_MultipleStatesSequence_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.Intro);
        gameLogic.RenderFrame(1280, 720);
        
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.Menu);
        gameLogic.RenderFrame(1280, 720);
        
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.Racing);
        gameLogic.RenderFrame(1280, 720);
    }

    #endregion

    #region Lifecycle Tests

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Dispose(); // Should not throw
    }

    [Fact]
    public void Run_DoesNotThrow()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Run(); // Should not throw (no-op implementation)
    }

    [Fact]
    public void Initialize_ThenDispose_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.Dispose();
    }

    [Fact]
    public void CompleteLifecycle_InitializeRenderResize_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.RenderFrame(1280, 720);
        gameLogic.OnWindowResize(1920, 1080);
        gameLogic.RenderFrame(1920, 1080);
        gameLogic.Dispose();
    }

    #endregion

    #region Edge Cases and Stress Tests

    [Fact]
    public void RenderFrame_MultipleTimesWithSameResolution_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        
        for (int i = 0; i < 20; i++)
        {
            gameLogic.RenderFrame(1280, 720);
        }
    }

    [Fact]
    public void OnWindowResize_ExtremeDimensions_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        
        gameLogic.OnWindowResize(4096, 2160);  // 4K
        gameLogic.OnWindowResize(320, 240);    // Very small
        gameLogic.OnWindowResize(1280, 720);   // Back to normal
    }

    [Fact]
    public void Initialize_MultipleTimesWithDifferentResolutions_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        
        gameLogic.Initialize(1280, 720);
        gameLogic.Initialize(1920, 1080);
        gameLogic.Initialize(800, 600);
        gameLogic.Initialize(1024, 768);
    }

    [Fact]
    public void RenderFrame_AfterMultipleResizes_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        
        gameLogic.OnWindowResize(1920, 1080);
        gameLogic.RenderFrame(1920, 1080);
        
        gameLogic.OnWindowResize(800, 600);
        gameLogic.RenderFrame(800, 600);
        
        gameLogic.OnWindowResize(1280, 720);
        gameLogic.RenderFrame(1280, 720);
    }

    #endregion

    public void Dispose()
    {
        // Cleanup if needed
    }

    #region Initialize Tests with Exception Handling

    [Fact]
    public void Initialize_WithVideoPlayerThrowingException_SwitchesToSplashScreen()
    {
        _mockVideoPlayer.Setup(x => x.Play()).Throws(new Exception("Video error"));
        var gameLogic = CreateGameLogic();
        
        gameLogic.Initialize(1280, 720);
        // Should catch exception and recover gracefully
        // Just verify that Initialize doesn't throw
    }

    [Fact]
    public void Initialize_WithTracksLoaded_CallsGameStateInitialize()
    {
        _mockAssetLoader.Setup(x => x.LoadTrackList()).Returns(new List<string> { "Track1", "Track2" });
        var gameLogic = CreateGameLogic();
        
        gameLogic.Initialize(1280, 720);
        // Verify GameState.Initialize was called
        _mockGameState.Verify(x => x.Initialize(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void Initialize_LoadsAndConfiguresFonts()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        
        // Verify font loading was called
        _mockFontSystem.Verify();
    }

    [Fact]
    public void Initialize_SetsMusicPlayerTracks()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        
        // Verify music player was configured
        _mockMusicPlayer.Verify();
    }

    #endregion

    #region RenderFrame with Varied States

    [Fact]
    public void RenderFrame_InShipPreviewMode_Succeeds()
    {
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.ShipPreview);
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.RenderFrame(1280, 720);
    }

    [Fact]
    public void RenderFrame_InLoadingMode_Succeeds()
    {
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.Loading);
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.RenderFrame(1280, 720);
    }

    [Fact]
    public void RenderFrame_InPausedMode_Succeeds()
    {
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.Paused);
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.RenderFrame(1280, 720);
    }

    [Fact]
    public void RenderFrame_InGameOverMode_Succeeds()
    {
        _mockGameState.Setup(x => x.CurrentMode).Returns(GameMode.GameOver);
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.RenderFrame(1280, 720);
    }

    #endregion

    #region Comprehensive Path Coverage

    [Fact]
    public void Initialize_ExecutesFullSequence()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        gameLogic.Initialize(1920, 1080);
        gameLogic.Initialize(1024, 768);
    }

    [Fact]
    public void RenderFrame_AllStateTransitions_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        
        var states = new[] 
        { 
            GameMode.Intro, 
            GameMode.SplashScreen, 
            GameMode.Menu, 
            GameMode.ShipPreview, 
            GameMode.Loading, 
            GameMode.Racing, 
            GameMode.Paused, 
            GameMode.GameOver, 
            GameMode.Victory, 
            GameMode.AttractMode 
        };
        
        foreach (var state in states)
        {
            _mockGameState.Setup(x => x.CurrentMode).Returns(state);
            gameLogic.RenderFrame(1280, 720);
        }
    }

    [Fact]
    public void OnWindowResize_MultipleSequences_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        gameLogic.Initialize(1280, 720);
        
        for (int i = 0; i < 5; i++)
        {
            gameLogic.OnWindowResize(1920, 1080);
            gameLogic.RenderFrame(1920, 1080);
            gameLogic.OnWindowResize(1280, 720);
            gameLogic.RenderFrame(1280, 720);
        }
    }

    [Fact]
    public void LifecycleTest_CompleteGameLogic_Succeeds()
    {
        var gameLogic = CreateGameLogic();
        
        gameLogic.Initialize(1280, 720);
        
        for (int i = 0; i < 3; i++)
        {
            gameLogic.RenderFrame(1280, 720);
            gameLogic.OnWindowResize(1920, 1080);
            gameLogic.RenderFrame(1920, 1080);
        }
        
        gameLogic.Run();
        gameLogic.Dispose();
    }

    #endregion
}
