using Xunit;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Presentation.Menus;
using WipeoutRewrite.Presentation;

namespace WipeoutRewrite.Tests;

public class MainMenuPagesTests
{
    [Fact]
    public void CreateMainMenu_ShouldCreatePageWithThreeItems()
    {
        // Act
        var page = MainMenuPages.CreateMainMenu();

        // Assert
        Assert.NotNull(page);
        Assert.Equal(3, page.Items.Count);
    }

    [Fact]
    public void CreateMainMenu_StartGameButton_ShouldHaveShipPreview()
    {
        // Act
        var page = MainMenuPages.CreateMainMenu();
        var startGameItem = page.Items[0];

        // Assert
        Assert.NotNull(startGameItem.PreviewInfo);
        Assert.Equal(typeof(CategoryShip), startGameItem.PreviewInfo.CategoryType);
        Assert.Equal(7, startGameItem.PreviewInfo.ModelIndex);
    }

    [Fact]
    public void CreateMainMenu_OptionsButton_ShouldHaveMsDosPreview()
    {
        // Act
        var page = MainMenuPages.CreateMainMenu();
        var optionsItem = page.Items[1];

        // Assert
        Assert.NotNull(optionsItem.PreviewInfo);
        Assert.Equal(typeof(CategoryMsDos), optionsItem.PreviewInfo.CategoryType);
        Assert.Equal(3, optionsItem.PreviewInfo.ModelIndex);
    }

    [Fact]
    public void CreateMainMenu_QuitButton_ShouldHaveMsDosPreview()
    {
        // Act
        var page = MainMenuPages.CreateMainMenu();
        var quitItem = page.Items[2];

        // Assert
        Assert.NotNull(quitItem.PreviewInfo);
        Assert.Equal(typeof(CategoryMsDos), quitItem.PreviewInfo.CategoryType);
        Assert.Equal(1, quitItem.PreviewInfo.ModelIndex);
    }

    [Fact]
    public void CreateOptionsMenu_ShouldCreatePageWithFourItems()
    {
        // Act
        var page = MainMenuPages.CreateOptionsMenu();

        // Assert
        Assert.NotNull(page);
        Assert.Equal(4, page.Items.Count);
    }

    [Fact]
    public void CreateOptionsMenu_AllItems_ShouldHaveShipPreviews()
    {
        // Act
        var page = MainMenuPages.CreateOptionsMenu();

        // Assert - All items should have CategoryShip previews with indices 0-3
        for (int i = 0; i < page.Items.Count; i++)
        {
            var item = page.Items[i];
            Assert.NotNull(item.PreviewInfo);
            Assert.Equal(typeof(CategoryShip), item.PreviewInfo.CategoryType);
            Assert.Equal(i, item.PreviewInfo.ModelIndex);
        }
    }

    [Fact]
    public void CreateRaceClassMenu_ShouldCreatePageWithTwoItems()
    {
        // Act
        var page = MainMenuPages.CreateRaceClassMenu();

        // Assert
        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public void CreateRaceClassMenu_VenomClass_ShouldHaveShipPreviewIndex4()
    {
        // Act
        var page = MainMenuPages.CreateRaceClassMenu();
        var venomItem = page.Items[0];

        // Assert
        Assert.NotNull(venomItem.PreviewInfo);
        Assert.Equal(typeof(CategoryShip), venomItem.PreviewInfo.CategoryType);
        Assert.Equal(4, venomItem.PreviewInfo.ModelIndex);
    }

    [Fact]
    public void CreateRaceClassMenu_RapierClass_ShouldHaveShipPreviewIndex5()
    {
        // Act
        var page = MainMenuPages.CreateRaceClassMenu();
        var rapierItem = page.Items[1];

        // Assert
        Assert.NotNull(rapierItem.PreviewInfo);
        Assert.Equal(typeof(CategoryShip), rapierItem.PreviewInfo.CategoryType);
        Assert.Equal(5, rapierItem.PreviewInfo.ModelIndex);
        Assert.False(rapierItem.IsEnabled); // Should be locked by default
    }

    [Fact]
    public void CreateRaceTypeMenu_ShouldCreatePageWithTwoItems()
    {
        // Act
        var page = MainMenuPages.CreateRaceTypeMenu();

        // Assert
        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public void CreateRaceTypeMenu_SingleRace_ShouldHaveShipPreviewIndex6()
    {
        // Act
        var page = MainMenuPages.CreateRaceTypeMenu();
        var singleRaceItem = page.Items[0];

        // Assert
        Assert.NotNull(singleRaceItem.PreviewInfo);
        Assert.Equal(typeof(CategoryShip), singleRaceItem.PreviewInfo.CategoryType);
        Assert.Equal(6, singleRaceItem.PreviewInfo.ModelIndex);
    }

    [Fact]
    public void CreateRaceTypeMenu_TimeTrial_ShouldHaveShipPreviewIndex7()
    {
        // Act
        var page = MainMenuPages.CreateRaceTypeMenu();
        var timeTrialItem = page.Items[1];

        // Assert
        Assert.NotNull(timeTrialItem.PreviewInfo);
        Assert.Equal(typeof(CategoryShip), timeTrialItem.PreviewInfo.CategoryType);
        Assert.Equal(7, timeTrialItem.PreviewInfo.ModelIndex);
    }

    [Fact]
    public void CreateTeamMenu_ShouldCreatePageWithFourTeams()
    {
        // Act
        var page = MainMenuPages.CreateTeamMenu();

        // Assert
        Assert.NotNull(page);
        Assert.Equal(4, page.Items.Count);
    }

    [Fact]
    public void CreateTeamMenu_AllTeams_ShouldHaveShipPreviews()
    {
        // Act
        var page = MainMenuPages.CreateTeamMenu();

        // Assert - Each team should have a ship preview with index matching their position
        for (int i = 0; i < page.Items.Count; i++)
        {
            var item = page.Items[i];
            Assert.NotNull(item.PreviewInfo);
            Assert.Equal(typeof(CategoryShip), item.PreviewInfo.CategoryType);
            Assert.Equal(i, item.PreviewInfo.ModelIndex);
        }
    }

    [Fact]
    public void CreateTeamMenu_Teams_ShouldHaveCorrectLabels()
    {
        // Act
        var page = MainMenuPages.CreateTeamMenu();

        // Assert
        Assert.Equal("FEISAR", page.Items[0].Label);
        Assert.Equal("GOTEKI 45", page.Items[1].Label);
        Assert.Equal("AG SYSTEMS", page.Items[2].Label);
        Assert.Equal("AURICOM", page.Items[3].Label);
    }

    [Fact]
    public void CreateQuitConfirmation_Items_ShouldNotHavePreviewInfo()
    {
        // Act
        var page = MainMenuPages.CreateQuitConfirmation();

        // Assert - Quit confirmation buttons don't need 3D previews
        foreach (var item in page.Items)
        {
            Assert.Null(item.PreviewInfo);
        }
    }

    [Fact]
    public void CreateVideoOptionsMenu_Items_ShouldNotHavePreviewInfo()
    {
        // Act
        var page = MainMenuPages.CreateVideoOptionsMenu();

        // Assert - Toggle items in video options don't need 3D previews
        foreach (var item in page.Items)
        {
            Assert.Null(item.PreviewInfo);
        }
    }

    [Fact]
    public void CreateAudioOptionsMenu_Items_ShouldNotHavePreviewInfo()
    {
        // Act
        var page = MainMenuPages.CreateAudioOptionsMenu();

        // Assert - Toggle items in audio options don't need 3D previews
        foreach (var item in page.Items)
        {
            Assert.Null(item.PreviewInfo);
        }
    }

    [Fact]
    public void CreatePilotMenu_Items_ShouldNotHavePreviewInfo()
    {
        // Act
        var page = MainMenuPages.CreatePilotMenu("FEISAR");

        // Assert - Pilot selection doesn't have 3D previews yet
        foreach (var item in page.Items)
        {
            Assert.Null(item.PreviewInfo);
        }
    }

    [Fact]
    public void CreateCircuitMenu_Items_ShouldNotHavePreviewInfo()
    {
        // Act
        var page = MainMenuPages.CreateCircuitMenu();

        // Assert - Circuit selection doesn't have 3D previews yet
        foreach (var item in page.Items)
        {
            Assert.Null(item.PreviewInfo);
        }
    }
}
