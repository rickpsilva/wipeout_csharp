using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Tests.Core.Services;

public class GameStateTests
{
    private readonly Mock<IGameObjectCollection> _mockGameObjects;
    private readonly Mock<IGameObject> _mockModel;
    private readonly Mock<ITrack> _mockTrack;
    private readonly Mock<IVideoSettings> _mockVideoSettings;
    private readonly Mock<IAudioSettings> _mockAudioSettings;
    private readonly Mock<IControlsSettings> _mockControlsSettings;
    private readonly ILogger<GameState> _logger;
    private readonly GameState _gameState;

    public GameStateTests()
    {
        _mockGameObjects = new Mock<IGameObjectCollection>();
        _mockModel = new Mock<IGameObject>();
        _mockTrack = new Mock<ITrack>();
        _mockVideoSettings = new Mock<IVideoSettings>();
        _mockAudioSettings = new Mock<IAudioSettings>();
        _mockControlsSettings = new Mock<IControlsSettings>();
        _logger = new NullLogger<GameState>();

        _mockGameObjects.Setup(g => g.GetAll).Returns(new List<GameObject>());
        _mockVideoSettings.Setup(v => v.Fullscreen).Returns(false);
        _mockAudioSettings.Setup(a => a.MusicVolume).Returns(0.8f);
        _mockAudioSettings.Setup(a => a.SoundEffectsVolume).Returns(0.7f);

        _gameState = new GameState(
            _logger,
            _mockGameObjects.Object,
            _mockModel.Object,
            _mockVideoSettings.Object,
            _mockAudioSettings.Object,
            _mockControlsSettings.Object,
            _mockTrack.Object
        );
    }

    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        Assert.Equal(GameMode.Menu, _gameState.CurrentMode);
        Assert.Equal(1, _gameState.LapNumber);
        Assert.Equal(0, _gameState.RaceTime);
        Assert.Equal(1, _gameState.Position);
        Assert.Equal(8, _gameState.TotalPlayers);
        Assert.Equal(3, _gameState.Lives);
        Assert.Equal(1, _gameState.Difficulty);
        Assert.Equal(1, _gameState.GameSpeed);
        Assert.True(_gameState.EnableAI);
        
        // Verify menu selections defaults
        Assert.Equal(RaceClass.Venom, _gameState.SelectedRaceClass);
        Assert.Equal(RaceType.Single, _gameState.SelectedRaceType);
        Assert.Equal(Team.Feisar, _gameState.SelectedTeam);
        Assert.Equal(0, _gameState.SelectedPilot);
        Assert.Equal(Circuit.AltimaVII, _gameState.SelectedCircuit);
        Assert.False(_gameState.IsAttractMode);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GameState(
            null!,
            _mockGameObjects.Object,
            _mockModel.Object,
            _mockVideoSettings.Object,
            _mockAudioSettings.Object,
            _mockControlsSettings.Object
        ));
    }

    [Fact]
    public void Constructor_WithNullGameObjects_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GameState(
            _logger,
            null!,
            _mockModel.Object,
            _mockVideoSettings.Object,
            _mockAudioSettings.Object,
            _mockControlsSettings.Object
        ));
    }

    [Fact]
    public void Constructor_WithNullModel_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GameState(
            _logger,
            _mockGameObjects.Object,
            null!,
            _mockVideoSettings.Object,
            _mockAudioSettings.Object,
            _mockControlsSettings.Object
        ));
    }

    [Fact]
    public void Initialize_SetsCurrentTrackAndModeToLoading()
    {
        _mockTrack.Setup(t => t.Name).Returns("TestTrack");

        _gameState.Initialize();

        Assert.Equal(GameMode.Loading, _gameState.CurrentMode);
        Assert.NotNull(_gameState.CurrentTrack);
        Assert.Equal(1, _gameState.LapNumber);
        Assert.Equal(0, _gameState.RaceTime);
    }

    [Fact]
    public void Initialize_ClearsGameObjects()
    {
        _gameState.Initialize();

        _mockGameObjects.Verify(g => g.Clear(), Times.Once);
        _mockGameObjects.Verify(g => g.Init(null), Times.Once);
    }

    [Fact]
    public void Update_WhenNotRacing_DoesNotUpdateRaceTime()
    {
        _gameState.CurrentMode = GameMode.Menu;
        var initialTime = _gameState.RaceTime;

        _gameState.Update(1.0f);

        Assert.Equal(initialTime, _gameState.RaceTime);
    }

    [Fact]
    public void Update_WhenRacing_UpdatesRaceTime()
    {
        _gameState.CurrentMode = GameMode.Racing;
        _gameState.RaceTime = 0;

        _gameState.Update(1.5f);

        Assert.Equal(1.5f, _gameState.RaceTime);
    }

    [Fact]
    public void Update_WhenRacing_UpdatesGameObjects()
    {
        _gameState.CurrentMode = GameMode.Racing;

        _gameState.Update(1.0f);

        _mockGameObjects.Verify(g => g.Update(), Times.Once);
    }

    [Fact]
    public void Render_CallsTrackRender()
    {
        _mockTrack.Setup(t => t.Name).Returns("TestTrack");
        _gameState.Initialize(); // This sets CurrentTrack

        var mockRenderer = new Mock<GLRenderer>();

        _gameState.Render(mockRenderer.Object);

        _mockTrack.Verify(t => t.Render(It.IsAny<GLRenderer>()), Times.Once);
    }

    [Fact]
    public void Render_CallsGameObjectsRenderer()
    {
        var mockRenderer = new Mock<GLRenderer>();

        _gameState.Render(mockRenderer.Object);

        _mockGameObjects.Verify(g => g.Renderer(), Times.Once);
    }

    [Fact]
    public void SetPlayerShip_DoesNotThrow()
    {
        var exception = Record.Exception(() => 
            _gameState.SetPlayerShip(true, false, true, false, false, true));

        Assert.Null(exception);
    }

    [Fact]
    public void CurrentMode_CanBeSet()
    {
        _gameState.CurrentMode = GameMode.Racing;

        Assert.Equal(GameMode.Racing, _gameState.CurrentMode);
    }

    [Fact]
    public void LapNumber_CanBeSet()
    {
        _gameState.LapNumber = 3;

        Assert.Equal(3, _gameState.LapNumber);
    }

    [Fact]
    public void Position_CanBeSet()
    {
        _gameState.Position = 5;

        Assert.Equal(5, _gameState.Position);
    }

    [Fact]
    public void TotalPlayers_CanBeSet()
    {
        _gameState.TotalPlayers = 12;

        Assert.Equal(12, _gameState.TotalPlayers);
    }

    [Fact]
    public void Difficulty_CanBeSet()
    {
        _gameState.Difficulty = 3;

        Assert.Equal(3, _gameState.Difficulty);
    }

    [Fact]
    public void GameSpeed_CanBeSet()
    {
        _gameState.GameSpeed = 2;

        Assert.Equal(2, _gameState.GameSpeed);
    }

    [Fact]
    public void EnableAI_CanBeSet()
    {
        _gameState.EnableAI = false;

        Assert.False(_gameState.EnableAI);
    }
}
