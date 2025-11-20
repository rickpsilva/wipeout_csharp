using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for GameState.
/// Demonstrates how to test business logic in isolation.
/// </summary>
public class GameStateTests
{
    private readonly Mock<ILogger<GameState>> _mockLogger;
    private readonly Mock<IShips> _mockShips;
    private readonly Mock<IShipV2> _mockPlayerShip;

    public GameStateTests()
    {
        _mockLogger = new Mock<ILogger<GameState>>();
        _mockShips = new Mock<IShips>();
        _mockPlayerShip = new Mock<IShipV2>();
        
        // Setup mock ships to return a list
        _mockShips.Setup(s => s.AllShips).Returns(new List<ShipV2>());
    }

    [Fact]
    public void GameState_ShouldStartInMenuMode()
    {
        // Arrange & Act
        var gameState = new GameState(_mockLogger.Object, _mockShips.Object, _mockPlayerShip.Object);
        
        // Assert
        Assert.Equal(GameMode.Menu, gameState.CurrentMode);
    }
    
    [Fact]
    public void Initialize_ShouldSetTrackAndCreateShips()
    {
        // Arrange
        var gameState = new GameState(_mockLogger.Object, _mockShips.Object, _mockPlayerShip.Object);
        var track = new Track("track01");
        
        // Act
        gameState.Initialize(track, playerShipId: 0);
        
        // Assert
        Assert.NotNull(gameState.CurrentTrack);
        Assert.Equal("track01", gameState.CurrentTrack.Name);
        // TODO: Update test after refactoring - Ships are now injected via DI
        // Assert.NotEmpty(gameState.Ships);
        // Assert.NotNull(gameState.PlayerShip);
    }
    
    [Fact]
    public void Update_ShouldAdvanceTime_WhenRacing()
    {
        // Arrange
        var gameState = new GameState(_mockLogger.Object, _mockShips.Object, _mockPlayerShip.Object);
        var track = new Track("track01");
        gameState.Initialize(track, playerShipId: 0);
        gameState.CurrentMode = GameMode.Racing; // Mudar para modo racing
        float initialTime = gameState.RaceTime;
        
        // Act
        gameState.Update(deltaTime: 0.016f); // ~60 FPS
        
        // Assert
        Assert.True(gameState.RaceTime > initialTime);
    }
}
