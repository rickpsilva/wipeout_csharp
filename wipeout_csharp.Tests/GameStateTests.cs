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

    public GameStateTests()
    {
        _mockLogger = new Mock<ILogger<GameState>>();
    }

    [Fact]
    public void GameState_ShouldStartInMenuMode()
    {
        // Arrange & Act
        var gameState = new GameState(_mockLogger.Object);
        
        // Assert
        Assert.Equal(GameMode.Menu, gameState.CurrentMode);
    }
    
    [Fact]
    public void Initialize_ShouldSetTrackAndCreateShips()
    {
        // Arrange
        var gameState = new GameState(_mockLogger.Object);
        var track = new Track("track01");
        
        // Act
        gameState.Initialize(track, playerShipId: 0);
        
        // Assert
        Assert.NotNull(gameState.CurrentTrack);
        Assert.Equal("track01", gameState.CurrentTrack.Name);
        Assert.NotEmpty(gameState.Ships);
        Assert.NotNull(gameState.PlayerShip);
    }
    
    [Fact]
    public void Update_ShouldAdvanceTime_WhenRacing()
    {
        // Arrange
        var gameState = new GameState(_mockLogger.Object);
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
