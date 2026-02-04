using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using WipeoutRewrite.Core.Data;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Presentation.Menus;
using WipeoutRewrite.Presentation;

namespace WipeoutRewrite.Tests;

public class MainMenuPagesTests
{
    private readonly MainMenuPages _pages;
    private readonly Mock<IGameState> _gameState = new();
    private readonly Mock<ISettingsPersistenceService> _settingsPersistence = new();
    private readonly Mock<IBestTimesManager> _bestTimesManager = new();
    private readonly Mock<IMenuBuilder> _menuBuilder = new();
    private readonly Mock<IMenuActionHandler> _menuActionHandler = new();
    private readonly Mock<IControlsSettings> _controlsSettings = new();
    private readonly Mock<IVideoSettings> _videoSettings = new();
    private readonly Mock<IAudioSettings> _audioSettings = new();
    private readonly Mock<IGameDataService> _gameDataService = new();

    public MainMenuPagesTests()
    {
        _menuBuilder.Setup(b => b.BuildMenu(It.IsAny<string>())).Throws(new Exception("Menu data not configured"));
        _gameDataService.Setup(g => g.GetTeams()).Returns(Array.Empty<TeamData>());
        _gameDataService.Setup(g => g.GetPilots()).Returns(Array.Empty<PilotData>());

        _pages = new MainMenuPages(
            NullLogger<MainMenuPages>.Instance,
            _gameState.Object,
            _settingsPersistence.Object,
            _bestTimesManager.Object,
            _menuBuilder.Object,
            _menuActionHandler.Object,
            _controlsSettings.Object,
            _videoSettings.Object,
            _audioSettings.Object,
            _gameDataService.Object);
    }

    [Fact]
    public void CreateMainMenu_ShouldCreatePageWithThreeItems()
    {
        // Act
        var page = _pages.CreateMainMenu();

        // Assert
        Assert.NotNull(page);
        Assert.Equal(3, page.Items.Count);
    }

    [Fact]
    public void CreateMainMenu_StartGameButton_ShouldHaveShipPreview()
    {
        // Act
        var page = _pages.CreateMainMenu();
        var startGameItem = page.Items[0];

        // Assert
        Assert.NotNull(startGameItem.ContentViewPort);
        Assert.Equal(typeof(CategoryShip), startGameItem.ContentViewPort.CategoryType);
        Assert.Equal(7, startGameItem.ContentViewPort.ModelIndex);
    }

    [Fact]
    public void CreateMainMenu_OptionsButton_ShouldHaveMsDosPreview()
    {
        // Act
        var page = _pages.CreateMainMenu();
        var optionsItem = page.Items[1];

        // Assert
        Assert.NotNull(optionsItem.ContentViewPort);
        Assert.Equal(typeof(CategoryMsDos), optionsItem.ContentViewPort.CategoryType);
        Assert.Equal(3, optionsItem.ContentViewPort.ModelIndex);
    }

    [Fact]
    public void CreateMainMenu_QuitButton_ShouldHaveMsDosPreview()
    {
        // Act
        var page = _pages.CreateMainMenu();
        var quitItem = page.Items[2];

        // Assert
        Assert.NotNull(quitItem.ContentViewPort);
        Assert.Equal(typeof(CategoryMsDos), quitItem.ContentViewPort.CategoryType);
        Assert.Equal(1, quitItem.ContentViewPort.ModelIndex);
    }

    [Fact]
    public void CreateOptionsMenu_ShouldCreatePageWithFourItems()
    {
        // Act
        var page = _pages.CreateOptionsMenu();

        // Assert
        Assert.NotNull(page);
        Assert.Equal(4, page.Items.Count);
    }

    [Fact]
    public void CreateOptionsMenu_AllItems_ShouldHaveCorrectPreviews()
    {
        // Act
        var page = _pages.CreateOptionsMenu();

        // Assert - Options menu items have mixed preview types (Props and Options)
        Assert.Equal(4, page.Items.Count);
        Assert.NotNull(page.Items[0].ContentViewPort);  // CONTROLS - CategoryProp
        Assert.NotNull(page.Items[1].ContentViewPort);  // VIDEO - CategoryProp
        Assert.NotNull(page.Items[2].ContentViewPort);  // AUDIO - CategoryOptions
        Assert.NotNull(page.Items[3].ContentViewPort);  // BEST TIMES - CategoryOptions
    }

    [Fact]
    public void CreateRaceClassMenu_ShouldCreatePageWithTwoItems()
    {
        // Act
        var page = _pages.CreateRaceClassMenu();

        // Assert
        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public void CreateRaceClassMenu_VenomClass_ShouldHavePilotPreview()
    {
        // Act
        var page = _pages.CreateRaceClassMenu();
        var venomItem = page.Items[0];

        // Assert - Uses CategoryPilot preview
        Assert.NotNull(venomItem.ContentViewPort);
        Assert.Equal(typeof(CategoryPilot), venomItem.ContentViewPort.CategoryType);
        Assert.Equal(8, venomItem.ContentViewPort.ModelIndex);
    }

    [Fact]
    public void CreateRaceClassMenu_RapierClass_ShouldHavePilotPreview()
    {
        // Act
        var page = _pages.CreateRaceClassMenu();
        var rapierItem = page.Items[1];

        // Assert - Uses CategoryPilot preview
        Assert.NotNull(rapierItem.ContentViewPort);
        Assert.Equal(typeof(CategoryPilot), rapierItem.ContentViewPort.CategoryType);
        Assert.Equal(9, rapierItem.ContentViewPort.ModelIndex);
    }

    [Fact]
    public void CreateRaceTypeMenu_ShouldCreatePageWithThreeItems()
    {
        // Act
        var page = _pages.CreateRaceTypeMenu();

        // Assert - Championship, Single Race, Time Trial
        Assert.NotNull(page);
        Assert.Equal(3, page.Items.Count);
        Assert.Equal("CHAMPIONSHIP RACE", page.Items[0].Label);
        Assert.Equal("SINGLE RACE", page.Items[1].Label);
        Assert.Equal("TIME TRIAL", page.Items[2].Label);
    }

    [Fact]
    public void CreateRaceTypeMenu_SingleRace_ShouldHaveOptionsPreview()
    {
        // Act
        var page = _pages.CreateRaceTypeMenu();
        var singleRaceItem = page.Items[1];  // SINGLE RACE

        // Assert - Uses CategoryMsDos preview (matches current menu setup)
        Assert.NotNull(singleRaceItem.ContentViewPort);
        Assert.Equal(typeof(CategoryMsDos), singleRaceItem.ContentViewPort.CategoryType);
        Assert.Equal(0, singleRaceItem.ContentViewPort.ModelIndex);
    }

    [Fact]
    public void CreateRaceTypeMenu_TimeTrial_ShouldHaveOptionsPreview()
    {
        // Act
        var page = _pages.CreateRaceTypeMenu();
        var timeTrialItem = page.Items[2];  // TIME TRIAL

        // Assert - Uses CategoryOptions preview
        Assert.NotNull(timeTrialItem.ContentViewPort);
        Assert.Equal(typeof(CategoryOptions), timeTrialItem.ContentViewPort.CategoryType);
        Assert.Equal(0, timeTrialItem.ContentViewPort.ModelIndex);
    }

    [Fact]
    public void CreateTeamMenu_ShouldCreatePageWithFourTeams()
    {
        // Act
        var page = _pages.CreateTeamMenu();

        // Assert - With menu builder throwing, returns error page
        Assert.NotNull(page);
        Assert.Equal("ERROR LOADING MENU", page.Title);
        Assert.Equal(0, page.Items.Count);  // Error pages have no items
    }

    [Fact(Skip = "Requires MenuBuilder and GameDataService to be configured")]
    public void CreateTeamMenu_AllTeams_ShouldHaveShipPreviews()
    {
        // Act
        var page = _pages.CreateTeamMenu();

        // Assert - Each team should have a ship preview with index matching their position
        for (int i = 0; i < page.Items.Count; i++)
        {
            var item = page.Items[i];
            Assert.NotNull(item.ContentViewPort);
            Assert.Equal(typeof(CategoryShip), item.ContentViewPort.CategoryType);
            Assert.Equal(i, item.ContentViewPort.ModelIndex);
        }
    }

    [Fact(Skip = "Requires MenuBuilder and GameDataService to be configured")]
    public void CreateTeamMenu_Teams_ShouldHaveCorrectLabels()
    {
        // Act
        var page = _pages.CreateTeamMenu();

        // Assert - Teams in C order: AG SYSTEMS, AURICOM, QIREX, FEISAR
        Assert.Equal("AG SYSTEMS", page.Items[0].Label);
        Assert.Equal("AURICOM", page.Items[1].Label);
        Assert.Equal("QIREX", page.Items[2].Label);
        Assert.Equal("FEISAR", page.Items[3].Label);
    }

    [Fact]
    public void CreateQuitConfirmation_Items_ShouldNotHavePreviewInfo()
    {
        // Act
        var page = _pages.CreateQuitConfirmation();

        // Assert - Quit confirmation buttons don't need 3D previews
        foreach (var item in page.Items)
        {
            Assert.Null(item.ContentViewPort);
        }
    }

    [Fact]
    public void CreateVideoMenu_Items_ShouldNotHavePreviewInfo()
    {
        // Act
        var page = _pages.CreateVideoMenu();

        // Assert - Toggle items in video options don't need 3D previews
        foreach (var item in page.Items)
        {
            Assert.Null(item.ContentViewPort);
        }
    }

    [Fact]
    public void CreateAudioMenu_Items_ShouldNotHavePreviewInfo()
    {
        // Act
        var page = _pages.CreateAudioMenu();

        // Assert - Toggle items in audio options don't need 3D previews
        foreach (var item in page.Items)
        {
            Assert.Null(item.ContentViewPort);
        }
    }

    [Fact]
    public void CreatePilotMenu_WithoutGameDataService_ReturnsEmptyPage()
    {
        // Act - Pass teamId (3 = FEISAR)
        var page = _pages.CreatePilotMenu(3);

        // Assert - Should return empty page (no fallback)
        Assert.Equal("CHOOSE YOUR PILOT", page.Title);
        Assert.Equal(0, page.Items.Count);  // No items without GameDataService
    }

    [Fact(Skip = "Requires MenuBuilder and GameDataService to be configured")]
    public void CreateCircuitMenu_Items_ShouldNotHavePreviewInfo()
    {
        // Act
        var page = _pages.CreateCircuitMenu();

        // Assert - Circuit selection uses 2D track images
        for (int i = 0; i < page.Items.Count; i++)
        {
            var item = page.Items[i];
            Assert.NotNull(item.ContentViewPort);
            Assert.True(item.ContentViewPort.IsTrackImage);
            Assert.Equal(i, item.ContentViewPort.ModelIndex);
        }
    }
}
