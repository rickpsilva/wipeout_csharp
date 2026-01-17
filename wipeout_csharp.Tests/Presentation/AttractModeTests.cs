using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Presentation;

namespace WipeoutRewrite.Tests.Presentation;

public class AttractModeTests
{
    private Mock<ILogger<AttractMode>> CreateMockLogger()
    {
        return new Mock<ILogger<AttractMode>>();
    }

    private Mock<IGameState> CreateMockGameState()
    {
        var mock = new Mock<IGameState>();
        mock.Setup(x => x.CurrentMode).Returns(GameMode.SplashScreen);
        return mock;
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var gameState = CreateMockGameState().Object;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AttractMode(null!, gameState));
    }

    [Fact]
    public void Constructor_WithNullGameState_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = CreateMockLogger().Object;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AttractMode(logger, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameState = CreateMockGameState().Object;

        // Act
        var mode = new AttractMode(logger, gameState);

        // Assert
        Assert.NotNull(mode);
    }

    [Fact]
    public void Start_SetsGameModeToAttractMode()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameStateMock = CreateMockGameState();
        var mode = new AttractMode(logger, gameStateMock.Object);

        // Act
        mode.Start();

        // Assert
        gameStateMock.VerifySet(x => x.CurrentMode = GameMode.AttractMode, Times.Once);
    }

    [Fact]
    public void Start_LogsInformationMessage()
    {
        // Arrange
        var loggerMock = CreateMockLogger();
        var gameState = CreateMockGameState().Object;
        var mode = new AttractMode(loggerMock.Object, gameState);

        // Act
        mode.Start();

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("attract mode")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Stop_SetsGameModeToSplashScreen()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameStateMock = CreateMockGameState();
        var mode = new AttractMode(logger, gameStateMock.Object);
        mode.Start(); // Start first

        // Act
        mode.Stop();

        // Assert
        gameStateMock.VerifySet(x => x.CurrentMode = GameMode.SplashScreen, Times.Once);
    }

    [Fact]
    public void Update_WhenNotActive_DoesNotThrow()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameState = CreateMockGameState().Object;
        var mode = new AttractMode(logger, gameState);

        // Act & Assert
        mode.Update(16.0f); // Should not throw
    }

    [Fact]
    public void Update_WhenActive_DoesNotThrow()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameState = CreateMockGameState().Object;
        var mode = new AttractMode(logger, gameState);
        mode.Start();

        // Act & Assert
        mode.Update(16.0f); // Should not throw
    }

    [Fact]
    public void Update_WithVariousDeltaTimes_DoesNotThrow()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameState = CreateMockGameState().Object;
        var mode = new AttractMode(logger, gameState);
        mode.Start();

        // Act & Assert
        mode.Update(0f);
        mode.Update(1f);
        mode.Update(33.33f);
        mode.Update(100f);
    }

    [Fact]
    public void Render_WhenNotActive_DoesNotThrow()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameState = CreateMockGameState().Object;
        var renderer = new Mock<IRenderer>().Object;
        var mode = new AttractMode(logger, gameState);

        // Act & Assert
        mode.Render(renderer); // Should not throw
    }

    [Fact]
    public void Render_WhenActive_DoesNotThrow()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameState = CreateMockGameState().Object;
        var renderer = new Mock<IRenderer>().Object;
        var mode = new AttractMode(logger, gameState);
        mode.Start();

        // Act & Assert
        mode.Render(renderer); // Should not throw
    }

    [Fact]
    public void StartThenStop_CyclesCorrectly()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameStateMock = CreateMockGameState();
        var mode = new AttractMode(logger, gameStateMock.Object);

        // Act
        mode.Start();
        mode.Stop();

        // Assert
        gameStateMock.VerifySet(x => x.CurrentMode = GameMode.AttractMode, Times.Once);
        gameStateMock.VerifySet(x => x.CurrentMode = GameMode.SplashScreen, Times.Once);
    }

    [Fact]
    public void MultipleStarts_BothSetModeToAttractMode()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameStateMock = CreateMockGameState();
        var mode = new AttractMode(logger, gameStateMock.Object);

        // Act
        mode.Start();
        mode.Start();

        // Assert
        gameStateMock.VerifySet(x => x.CurrentMode = GameMode.AttractMode, Times.Exactly(2));
    }

    [Fact]
    public void UpdateAfterStart_DoesNotThrow()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameState = CreateMockGameState().Object;
        var mode = new AttractMode(logger, gameState);
        mode.Start();

        // Act & Assert
        mode.Update(16f);
        mode.Update(16f);
        mode.Update(16f);
    }

    [Fact]
    public void RenderAfterStart_WithValidRenderer_DoesNotThrow()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameState = CreateMockGameState().Object;
        var renderer = new Mock<IRenderer>().Object;
        var mode = new AttractMode(logger, gameState);
        mode.Start();

        // Act & Assert
        mode.Render(renderer);
        mode.Render(renderer);
    }

    [Fact]
    public void StopBeforeStart_SetsModeToSplashScreen()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameStateMock = CreateMockGameState();
        var mode = new AttractMode(logger, gameStateMock.Object);

        // Act
        mode.Stop();

        // Assert
        gameStateMock.VerifySet(x => x.CurrentMode = GameMode.SplashScreen, Times.Once);
    }

    [Fact]
    public void SequentialCalls_DoNotThrow()
    {
        // Arrange
        var logger = CreateMockLogger().Object;
        var gameState = CreateMockGameState().Object;
        var renderer = new Mock<IRenderer>().Object;
        var mode = new AttractMode(logger, gameState);

        // Act & Assert
        mode.Start();
        mode.Update(16f);
        mode.Render(renderer);
        mode.Update(16f);
        mode.Render(renderer);
        mode.Stop();
        mode.Update(16f); // Update after stop
        mode.Render(renderer); // Render after stop
    }
}
