using Xunit;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Core.Data;
using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Tests.Core.Services;

/// <summary>
/// Integration tests for MenuBuilder service.
/// Tests menu building from JSON definitions with conditional items and dynamic sources.
/// Uses xUnit + Moq for IGameDataService mocking - no real assets or GL contexts.
/// </summary>
public class MenuBuilderIntegrationTests
{
    private readonly MenuBuilder _menuBuilder;
    private readonly Mock<IGameDataService> _mockDataService;
    private readonly GameState _gameState;

    public MenuBuilderIntegrationTests()
    {
        _mockDataService = new Mock<IGameDataService>();
        
        // Create real GameState with mocked dependencies
        var logger = new NullLogger<GameState>();
        var mockGameObjects = new Mock<IGameObjectCollection>();
        var mockModel = new Mock<IGameObject>();
        var mockVideoSettings = new Mock<IVideoSettings>();
        var mockAudioSettings = new Mock<IAudioSettings>();
        var mockControlsSettings = new Mock<IControlsSettings>();
        
        mockGameObjects.Setup(g => g.GetAll).Returns(new List<GameObject>());
        mockVideoSettings.Setup(v => v.Fullscreen).Returns(false);
        mockAudioSettings.Setup(a => a.MusicVolume).Returns(0.8f);
        mockAudioSettings.Setup(a => a.SoundEffectsVolume).Returns(0.7f);
        
        _gameState = new GameState(
            logger,
            mockGameObjects.Object,
            mockModel.Object,
            mockVideoSettings.Object,
            mockAudioSettings.Object,
            mockControlsSettings.Object
        );
        
        // Setup mock data
        SetupMockGameData();
        
        // Create MenuBuilder instance
        _menuBuilder = new MenuBuilder();
    }

    private void SetupMockGameData()
    {
        // Setup menu structure with test menus
        var menuStructure = new MenuStructureDefinition
        {
            StartMenuId = "mainMenu",
            Menus = new Dictionary<string, MenuDefinition>
            {
                {
                    "mainMenu", new MenuDefinition
                    {
                        Id = "mainMenu",
                        Title = "MAIN MENU",
                        Items = new[]
                        {
                            new MenuItemDefinition
                            {
                                Id = "startGame",
                                Label = "START GAME",
                                NextMenu = "raceClassMenu",
                                Action = "navigate"
                            },
                            new MenuItemDefinition
                            {
                                Id = "quitGame",
                                Label = "QUIT",
                                Action = "quit"
                            }
                        },
                        IsFixed = true,
                        Layout = "fixed"
                    }
                },
                {
                    "raceClassMenu", new MenuDefinition
                    {
                        Id = "raceClassMenu",
                        Title = "SELECT RACING CLASS",
                        Items = new[]
                        {
                            new MenuItemDefinition
                            {
                                Id = "venomClass",
                                Label = "VENOM CLASS",
                                NextMenu = "teamSelectMenu",
                                Action = "navigate"
                            },
                            new MenuItemDefinition
                            {
                                Id = "rapierClass",
                                Label = "RAPIER CLASS",
                                NextMenu = "teamSelectMenu",
                                Action = "navigate"
                            }
                        },
                        IsFixed = true,
                        Layout = "fixed"
                    }
                },
                {
                    "teamSelectMenu", new MenuDefinition
                    {
                        Id = "teamSelectMenu",
                        Title = "SELECT YOUR TEAM",
                        DynamicSource = "teams",
                        IsFixed = true,
                        Layout = "fixed"
                    }
                },
                {
                    "conditionalMenu", new MenuDefinition
                    {
                        Id = "conditionalMenu",
                        Title = "CONDITIONAL ITEMS",
                        Items = new[]
                        {
                            new MenuItemDefinition
                            {
                                Id = "rapierOnly",
                                Label = "RAPIER ONLY",
                                Condition = "hasRapierClass",
                                Action = "navigate"
                            },
                            new MenuItemDefinition
                            {
                                Id = "notMultiplayer",
                                Label = "SINGLE PLAYER ONLY",
                                Condition = "!isMultiplayer",
                                Action = "navigate"
                            }
                        },
                        IsFixed = true,
                        Layout = "fixed"
                    }
                }
            }
        };

        _mockDataService
            .Setup(ds => ds.GetMenuStructure())
            .Returns(menuStructure);

        // Setup game data for dynamic sources
        var teams = new[]
        {
            new TeamData 
            { 
                Id = 0, 
                Name = "AG SYSTEMS", 
                DisplayName = "AG SYSTEMS",
                LogoModelIndex = 0,
                Pilots = new[] {0, 1}
            },
            new TeamData 
            { 
                Id = 1, 
                Name = "AURICOM", 
                DisplayName = "AURICOM",
                LogoModelIndex = 1,
                Pilots = new[] {2, 3}
            },
            new TeamData 
            { 
                Id = 2, 
                Name = "QIREX", 
                DisplayName = "QIREX",
                LogoModelIndex = 2,
                Pilots = new[] {4, 5}
            },
            new TeamData 
            { 
                Id = 3, 
                Name = "FEISAR", 
                DisplayName = "FEISAR",
                LogoModelIndex = 3,
                Pilots = new[] {6, 7}
            }
        };

        _mockDataService
            .Setup(ds => ds.GetTeams())
            .Returns(teams);

        var raceClasses = new[]
        {
            new RaceClassData { Id = 0, Name = "VENOM CLASS", DisplayName = "Venom" },
            new RaceClassData { Id = 1, Name = "RAPIER CLASS", DisplayName = "Rapier" }
        };

        _mockDataService
            .Setup(ds => ds.GetRaceClasses())
            .Returns(raceClasses);

        var circuits = new[]
        {
            new CircuitData { Id = 0, Name = "ALTIMA VII", DisplayName = "Altima VII" },
            new CircuitData { Id = 1, Name = "KARBONIS V", DisplayName = "Karbonis V" }
        };

        _mockDataService
            .Setup(ds => ds.GetCircuits())
            .Returns(circuits);

        var raceTypes = new[]
        {
            new RaceTypeData { Id = 0, Name = "CHAMPIONSHIP", DisplayName = "Championship" },
            new RaceTypeData { Id = 1, Name = "SINGLE RACE", DisplayName = "Single Race" }
        };

        _mockDataService
            .Setup(ds => ds.GetRaceTypes())
            .Returns(raceTypes);
    }

    // ===== BASIC MENU BUILDING TESTS =====

    [Fact]
    public void Initialize_LoadsMenuStructureSuccessfully()
    {
        // Act
        _menuBuilder.Initialize(_mockDataService.Object);

        // Assert - no exception thrown, menu structure loaded
        Assert.NotNull(_menuBuilder);
    }

    [Fact]
    public void BuildMenu_CreatesMenuPageWithTitle()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);

        // Act
        var menuPage = _menuBuilder.BuildMenu("mainMenu");

        // Assert
        Assert.NotNull(menuPage);
        Assert.Equal("MAIN MENU", menuPage.Title);
    }

    [Fact]
    public void BuildMenu_PopulatesStaticItems()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);

        // Act
        var menuPage = _menuBuilder.BuildMenu("mainMenu");

        // Assert
        Assert.NotNull(menuPage.Items);
        Assert.NotEmpty(menuPage.Items);
        Assert.Equal(2, menuPage.Items.Count);
        Assert.Equal("START GAME", menuPage.Items[0].Label);
        Assert.Equal("QUIT", menuPage.Items[1].Label);
    }

    [Fact]
    public void BuildMenu_AppliesMenuLayout()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);

        // Act
        var menuPage = _menuBuilder.BuildMenu("raceClassMenu");

        // Assert
        Assert.NotNull(menuPage);
        Assert.True((menuPage.LayoutFlags & MenuLayoutFlags.Fixed) != 0);
    }

    // ===== DYNAMIC ITEMS TESTS =====

    [Fact]
    public void BuildMenu_LoadsDynamicItemsFromSource()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);

        // Act
        var menuPage = _menuBuilder.BuildMenu("teamSelectMenu");

        // Assert
        Assert.NotNull(menuPage);
        Assert.NotEmpty(menuPage.Items);
        Assert.Equal(4, menuPage.Items.Count); // 4 teams
        Assert.Equal("AG SYSTEMS", menuPage.Items[0].Label);
        Assert.Equal("AURICOM", menuPage.Items[1].Label);
    }

    [Fact]
    public void BuildMenu_DynamicItemsConsistentOrder()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);

        // Act
        var menuPage = _menuBuilder.BuildMenu("teamSelectMenu");

        // Assert
        Assert.Equal(4, menuPage.Items.Count);
        Assert.Equal("AG SYSTEMS", menuPage.Items[0].Label);
        Assert.Equal("AURICOM", menuPage.Items[1].Label);
        Assert.Equal("QIREX", menuPage.Items[2].Label);
        Assert.Equal("FEISAR", menuPage.Items[3].Label);
    }

    // ===== CONDITIONAL ITEMS TESTS =====

    [Fact]
    public void EvaluateCondition_HasRapierClass_TrueWhenSelected()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);
        _gameState.SelectedRaceClass = RaceClass.Rapier;

        // Act
        bool result = _menuBuilder.EvaluateCondition("hasRapierClass", _gameState);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateCondition_HasRapierClass_FalseWhenVenom()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);
        _gameState.SelectedRaceClass = RaceClass.Venom;

        // Act
        bool result = _menuBuilder.EvaluateCondition("hasRapierClass", _gameState);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateCondition_HasVenomClass_TrueWhenSelected()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);
        _gameState.SelectedRaceClass = RaceClass.Venom;

        // Act
        bool result = _menuBuilder.EvaluateCondition("hasVenomClass", _gameState);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateCondition_IsMultiplayer_ReturnsFalse()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);
        _gameState.SelectedRaceType = RaceType.Championship;

        // Act
        bool result = _menuBuilder.EvaluateCondition("isMultiplayer", _gameState);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateCondition_IsSinglePlayer_ReturnsTrue()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);

        // Act
        bool result = _menuBuilder.EvaluateCondition("isSinglePlayer", _gameState);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateCondition_NegationWorks()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);
        _gameState.SelectedRaceClass = RaceClass.Venom;

        // Act
        bool result = _menuBuilder.EvaluateCondition("!hasRapierClass", _gameState);

        // Assert
        Assert.True(result); // NOT (hasRapierClass) = true when Venom is selected
    }

    [Fact]
    public void BuildMenu_ConditionalItemsFiltered()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);
        _gameState.SelectedRaceClass = RaceClass.Venom;

        // Act
        var menuPage = _menuBuilder.BuildMenu("conditionalMenu", _gameState);

        // Assert
        Assert.NotNull(menuPage);
        // Should have items but rapierOnly should be filtered out (condition not met)
        Assert.NotEmpty(menuPage.Items);
        // The rapierOnly item shouldn't be in the list since hasRapierClass is false
        Assert.DoesNotContain(menuPage.Items, item => item.Label == "RAPIER ONLY");
    }

    // ===== ERROR HANDLING TESTS =====

    [Fact]
    public void BuildMenu_InvalidMenuId_ThrowsException()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            _menuBuilder.BuildMenu("nonExistentMenu"));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void BuildMenu_ThrowsIfNotInitialized()
    {
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => 
            _menuBuilder.BuildMenu("mainMenu"));
        Assert.Contains("not initialized", ex.Message, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EvaluateCondition_UnknownCondition_ReturnsTrue()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);

        // Act
        bool result = _menuBuilder.EvaluateCondition("unknownCondition", _gameState);

        // Assert
        Assert.True(result); // Default to true for unknown conditions
    }

    // ===== GAME STATE SELECTION TESTS =====

    [Fact]
    public void EvaluateCondition_HasTeamSelected()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);
        _gameState.SelectedTeam = Team.Qirex;

        // Act
        bool result = _menuBuilder.EvaluateCondition("hasTeamSelected", _gameState);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateCondition_HasCircuitSelected()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);
        _gameState.SelectedCircuit = Circuit.KarbonisV;

        // Act
        bool result = _menuBuilder.EvaluateCondition("hasCircuitSelected", _gameState);

        // Assert
        Assert.True(result);
    }

    // ===== LAYOUT CONFIGURATION TESTS =====

    [Fact]
    public void BuildMenu_FixedMenuHasLayoutFlags()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);

        // Act
        var menuPage = _menuBuilder.BuildMenu("raceClassMenu");

        // Assert
        Assert.NotNull(menuPage);
        Assert.True((menuPage.LayoutFlags & MenuLayoutFlags.Fixed) != 0);
    }

    [Fact]
    public void BuildMenu_MultipleBuilds_ConsistentResults()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);

        // Act
        var menuPage1 = _menuBuilder.BuildMenu("mainMenu");
        var menuPage2 = _menuBuilder.BuildMenu("mainMenu");

        // Assert
        Assert.Equal(menuPage1.Items.Count, menuPage2.Items.Count);
        Assert.Equal(menuPage1.Title, menuPage2.Title);
    }

    // ===== EMPTY CONDITION TESTS =====

    [Fact]
    public void EvaluateCondition_NullCondition_ReturnsTrue()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);

        // Act
        bool result = _menuBuilder.EvaluateCondition(null, _gameState);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateCondition_EmptyCondition_ReturnsTrue()
    {
        // Arrange
        _menuBuilder.Initialize(_mockDataService.Object);

        // Act
        bool result = _menuBuilder.EvaluateCondition(string.Empty, _gameState);

        // Assert
        Assert.True(result);
    }
}
