using WipeoutRewrite.Core.Data;
using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Presentation;

/// <summary>
/// Factory for creating preview layouts for different menu screens
/// </summary>
public static class PreviewLayoutFactory
{
    private static readonly int[] DefaultLeftShipIndices = { 7, 5, 1, 0 };
    private static readonly int[] DefaultRightShipIndices = { 3, 6, 4, 2 };

    /// <summary>
    /// Creates the preview layout for the "Select Your Team" screen
    /// Displays: Team logo (center) + 2 ships (left/right bottom)
    /// </summary>
    private static readonly int[] DefaultTeamLogoIndices = { 3, 0, 1, 2 };

    /// <summary>
    /// Creates a preview layout for the team selection screen, showing the team logo and both pilots' ships.
    /// </summary>
    /// <param name="gameDataService"></param>
    /// <param name="teamIndex"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static PreviewLayout CreateTeamSelectionLayout(IGameDataService gameDataService, int teamIndex)
    {
        if (gameDataService == null) throw new ArgumentNullException(nameof(gameDataService));

        var team = gameDataService.GetTeam(teamIndex);
        var pilots = gameDataService.GetPilotsForTeam(team.Id);

        int teamLogoIndex = team.LogoModelIndex != 0 || DefaultTeamLogoIndices.Length == 0
            ? team.LogoModelIndex
            : GetDefault(DefaultTeamLogoIndices, teamIndex);

        int leftShipIndex = pilots.ElementAtOrDefault(0)?.ShipIndex
            ?? GetDefault(DefaultLeftShipIndices, teamIndex);

        int rightShipIndex = pilots.ElementAtOrDefault(1)?.ShipIndex
            ?? GetDefault(DefaultRightShipIndices, teamIndex);

        var layout = new PreviewLayout();

        // Center: Team logo (teams.prm, index matches teamIndex)
        layout.AddPreview(
            PreviewPosition.Center,
            new ContentPreview3DInfo(typeof(CategoryTeams), teamLogoIndex, customScale: 0.1f)
        );

        // Left bottom: Team ship 1 (ships.prm, index = teamIndex * 2)
        layout.AddPreview(
            PreviewPosition.LeftBottom,
            new ContentPreview3DInfo(typeof(CategoryShip), leftShipIndex, customScale: 1.5f)
        );

        // Right bottom: Team ship 2
        layout.AddPreview(
            PreviewPosition.RightBottom,
            new ContentPreview3DInfo(typeof(CategoryShip), rightShipIndex, customScale: 1.5f)
        );

        return layout;
    }

    /// <summary>
    /// Creates a preview layout for the pilot selection screen, showing the pilot's ship.
    /// </summary>
    /// <param name="gameDataService"></param>
    /// <param name="pilotIndex"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static PreviewLayout CreatePilotSelectionLayout(IGameDataService gameDataService, int pilotIndex)
    {
        if (gameDataService == null) throw new ArgumentNullException(nameof(gameDataService));

        var pilot = gameDataService.GetPilot(pilotIndex);

        var layout = new PreviewLayout();

        layout.AddPreview(
            PreviewPosition.Center,
            new ContentPreview3DInfo(typeof(CategoryPilot), pilot.LogoModelIndex, customScale: 0.1f)
        );

        // Center bottom: Ship of the pilot
        layout.AddPreview(
            PreviewPosition.BottomCenter,
            new ContentPreview3DInfo(typeof(CategoryShip),  pilot.ShipIndex, customScale: 1.5f)
        );

        return layout;
    }

    private static int GetDefault(int[] map, int teamIndex)
    {
        if (map.Length == 0) return 0;
        int index = teamIndex % map.Length;
        if (index < 0) index += map.Length;
        return map[index];
    }
}