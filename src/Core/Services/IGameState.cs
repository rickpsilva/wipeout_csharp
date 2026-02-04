using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Represents the state and behavior of the game, managing player input, game mode, 
/// and rendering of the current game session.
/// </summary>
public interface IGameState
{
    /// <summary>
    /// Gets or sets the currently selected race class.
    /// </summary>
    RaceClass SelectedRaceClass { get; set; }

    /// <summary>
    /// Gets or sets the currently selected race type.
    /// </summary>
    RaceType SelectedRaceType { get; set; }

    /// <summary>
    /// Gets or sets the currently selected team.
    /// </summary>
    Team SelectedTeam { get; set; }

    /// <summary>
    /// Gets or sets the index of the currently selected pilot.
    /// </summary>
    int SelectedPilot { get; set; }

    /// <summary>
    /// Gets or sets the currently selected circuit.
    /// </summary>
    Circuit SelectedCircuit { get; set; }

    /// <summary>
    /// Gets the current track being played, or null if no track is loaded.
    /// </summary>
    ITrack? CurrentTrack { get; }

    /// <summary>
    /// Gets or sets the current game mode.
    /// </summary>
    GameMode CurrentMode { get; set; }

    /// <summary>
    /// Sets the player ship control input states for acceleration, braking, turning, and boost actions.
    /// </summary>
    /// <param name="accelerate">Whether the player is accelerating.</param>
    /// <param name="brake">Whether the player is braking.</param>
    /// <param name="turnLeft">Whether the player is turning left.</param>
    /// <param name="turnRight">Whether the player is turning right.</param>
    /// <param name="boostLeft">Whether the player is activating left boost.</param>
    /// <param name="boostRight">Whether the player is activating right boost.</param>
    void SetPlayerShip(bool accelerate, bool brake, bool turnLeft, bool turnRight, bool boostLeft, bool boostRight);

    /// <summary>
    /// Initializes the game state with an optional player ship identifier.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Updates the game state based on elapsed time since the last frame.
    /// </summary>
    /// <param name="deltaTime">The elapsed time in seconds since the last update.</param>
    void Update(float deltaTime);

    /// <summary>
    /// Renders the current game state using the provided OpenGL renderer.
    /// </summary>
    /// <param name="renderer">The OpenGL renderer to use for drawing.</param>
    void Render(GLRenderer renderer);
}