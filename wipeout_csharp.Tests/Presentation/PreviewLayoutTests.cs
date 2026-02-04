using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using WipeoutRewrite.Core.Data;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Presentation;

namespace WipeoutRewrite.Tests.Presentation;

public class PreviewLayoutTests
{
    private readonly IGameDataService _gameDataService = CreateGameDataServiceStub();

    [Fact]
    public void PreviewLayout_CreateNew_IsEmpty()
    {
        var layout = new PreviewLayout();
        Assert.Empty(layout.Previews);
    }

    [Fact]
    public void PreviewLayout_AddPreview_AddsToList()
    {
        var layout = new PreviewLayout();
        var info = new ContentPreview3DInfo(typeof(CategoryShip), 0);

        layout.AddPreview(PreviewPosition.Center, info);

        Assert.Single(layout.Previews);
        Assert.Equal(PreviewPosition.Center, layout.Previews[0].Position);
        Assert.Equal(info, layout.Previews[0].Info);
    }

    [Fact]
    public void PreviewLayout_AddMultiplePreviews_AllStored()
    {
        var layout = new PreviewLayout();
        var centerInfo = new ContentPreview3DInfo(typeof(CategoryTeams), 0);
        var leftInfo = new ContentPreview3DInfo(typeof(CategoryShip), 0, 0.6f);
        var rightInfo = new ContentPreview3DInfo(typeof(CategoryShip), 1, 0.6f);

        layout.AddPreview(PreviewPosition.Center, centerInfo);
        layout.AddPreview(PreviewPosition.LeftBottom, leftInfo);
        layout.AddPreview(PreviewPosition.RightBottom, rightInfo);

        Assert.Equal(3, layout.Previews.Count);
        Assert.Equal(PreviewPosition.Center, layout.Previews[0].Position);
        Assert.Equal(PreviewPosition.LeftBottom, layout.Previews[1].Position);
        Assert.Equal(PreviewPosition.RightBottom, layout.Previews[2].Position);
    }

    [Fact]
    public void PreviewLayout_Clear_RemovesAllPreviews()
    {
        var layout = new PreviewLayout();
        var info1 = new ContentPreview3DInfo(typeof(CategoryShip), 0);
        var info2 = new ContentPreview3DInfo(typeof(CategoryShip), 1);
        layout.AddPreview(PreviewPosition.Center, info1);
        layout.AddPreview(PreviewPosition.LeftBottom, info2);

        layout.Clear();

        Assert.Empty(layout.Previews);
    }

    [Fact]
    public void PreviewLayoutFactory_CreateTeamSelectionLayout_ReturnsValidLayout()
    {
        var layout = PreviewLayoutFactory.CreateTeamSelectionLayout(_gameDataService, 0);

        Assert.NotNull(layout);
        Assert.Equal(3, layout.Previews.Count);
    }

    [Fact]
    public void PreviewLayoutFactory_CreateTeamSelectionLayout_HasCorrectPositions()
    {
        var layout = PreviewLayoutFactory.CreateTeamSelectionLayout(_gameDataService, 0);

        Assert.Equal(PreviewPosition.Center, layout.Previews[0].Position);
        Assert.Equal(PreviewPosition.LeftBottom, layout.Previews[1].Position);
        Assert.Equal(PreviewPosition.RightBottom, layout.Previews[2].Position);
    }

    [Fact]
    public void PreviewLayoutFactory_CreateTeamSelectionLayout_CenterIsTeamLogo()
    {
        var layout = PreviewLayoutFactory.CreateTeamSelectionLayout(_gameDataService, 0);
        var centerPreview = layout.Previews[0];

        Assert.Equal(typeof(CategoryTeams), centerPreview.Info.CategoryType);
        Assert.Equal(3, centerPreview.Info.ModelIndex);
    }

    [Fact]
    public void PreviewLayoutFactory_CreateTeamSelectionLayout_LeftShipCorrect()
    {
        var layout = PreviewLayoutFactory.CreateTeamSelectionLayout(_gameDataService, 1);
        var leftPreview = layout.Previews[1];

        Assert.Equal(typeof(CategoryShip), leftPreview.Info.CategoryType);
        Assert.Equal(5, leftPreview.Info.ModelIndex);
        Assert.Equal(1.5f, leftPreview.Info.CustomScale);
    }

    [Fact]
    public void PreviewLayoutFactory_CreateTeamSelectionLayout_RightShipCorrect()
    {
        var layout = PreviewLayoutFactory.CreateTeamSelectionLayout(_gameDataService, 2);
        var rightPreview = layout.Previews[2];

        Assert.Equal(typeof(CategoryShip), rightPreview.Info.CategoryType);
        Assert.Equal(4, rightPreview.Info.ModelIndex);
        Assert.Equal(1.5f, rightPreview.Info.CustomScale);
    }

    [Fact]
    public void PreviewPosition_EnumValues_AllDefined()
    {
        var positions = new[]
        {
            PreviewPosition.Center,
            PreviewPosition.LeftBottom,
            PreviewPosition.RightBottom,
            PreviewPosition.TopCenter
        };

        Assert.Equal(4, positions.Length);
        var distinct = new HashSet<PreviewPosition>(positions);
        Assert.Equal(4, distinct.Count);
    }

    [Fact]
    public void PreviewLayoutFactory_CreateTeamSelectionLayout_DynamicForDifferentTeams()
    {
        var team0Layout = PreviewLayoutFactory.CreateTeamSelectionLayout(_gameDataService, 0);
        var team1Layout = PreviewLayoutFactory.CreateTeamSelectionLayout(_gameDataService, 1);
        var team2Layout = PreviewLayoutFactory.CreateTeamSelectionLayout(_gameDataService, 2);
        var team3Layout = PreviewLayoutFactory.CreateTeamSelectionLayout(_gameDataService, 3);

        Assert.Equal(7, team0Layout.Previews[1].Info.ModelIndex);
        Assert.Equal(3, team0Layout.Previews[2].Info.ModelIndex);

        Assert.Equal(5, team1Layout.Previews[1].Info.ModelIndex);
        Assert.Equal(6, team1Layout.Previews[2].Info.ModelIndex);

        Assert.Equal(1, team2Layout.Previews[1].Info.ModelIndex);
        Assert.Equal(4, team2Layout.Previews[2].Info.ModelIndex);

        Assert.Equal(0, team3Layout.Previews[1].Info.ModelIndex);
        Assert.Equal(2, team3Layout.Previews[2].Info.ModelIndex);
    }

    [Fact]
    public void PreviewLayoutFactory_CreateTeamSelectionLayout_CenterTeamIndexMatches()
    {
        var team0Layout = PreviewLayoutFactory.CreateTeamSelectionLayout(_gameDataService, 0);
        var team1Layout = PreviewLayoutFactory.CreateTeamSelectionLayout(_gameDataService, 1);
        var team2Layout = PreviewLayoutFactory.CreateTeamSelectionLayout(_gameDataService, 2);
        var team3Layout = PreviewLayoutFactory.CreateTeamSelectionLayout(_gameDataService, 3);

        Assert.Equal(3, team0Layout.Previews[0].Info.ModelIndex);
        Assert.Equal(0, team1Layout.Previews[0].Info.ModelIndex);
        Assert.Equal(1, team2Layout.Previews[0].Info.ModelIndex);
        Assert.Equal(2, team3Layout.Previews[0].Info.ModelIndex);
    }

    private static IGameDataService CreateGameDataServiceStub()
    {
        var teams = new[]
        {
            new TeamData { Id = 0, LogoModelIndex = 3, Pilots = new[] { 0, 1 } },
            new TeamData { Id = 1, LogoModelIndex = 0, Pilots = new[] { 2, 3 } },
            new TeamData { Id = 2, LogoModelIndex = 1, Pilots = new[] { 4, 5 } },
            new TeamData { Id = 3, LogoModelIndex = 2, Pilots = new[] { 6, 7 } }
        };

        var pilots = new Dictionary<int, PilotData>
        {
            { 0, new PilotData { Id = 0, ShipIndex = 7, TeamId = 0 } },
            { 1, new PilotData { Id = 1, ShipIndex = 3, TeamId = 0 } },
            { 2, new PilotData { Id = 2, ShipIndex = 5, TeamId = 1 } },
            { 3, new PilotData { Id = 3, ShipIndex = 6, TeamId = 1 } },
            { 4, new PilotData { Id = 4, ShipIndex = 1, TeamId = 2 } },
            { 5, new PilotData { Id = 5, ShipIndex = 4, TeamId = 2 } },
            { 6, new PilotData { Id = 6, ShipIndex = 0, TeamId = 3 } },
            { 7, new PilotData { Id = 7, ShipIndex = 2, TeamId = 3 } }
        };

        var mock = new Mock<IGameDataService>();

        mock.Setup(s => s.GetTeam(It.IsAny<int>()))
            .Returns((int id) => teams.First(t => t.Id == id));

        mock.Setup(s => s.GetPilotsForTeam(It.IsAny<int>()))
            .Returns((int id) => teams.First(t => t.Id == id).Pilots.Select(pid => pilots[pid]).ToList());

        return mock.Object;
    }
}
