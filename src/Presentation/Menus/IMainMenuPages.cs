using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Presentation.Menus;

/// <summary>
/// Defines the contract for managing main menu pages and their interactions in the game.
/// </summary>
/// <remarks>
/// This interface is responsible for creating various menu pages, handling user input,
/// and managing callbacks for game state changes. It provides methods to construct menus
/// for options, controls, video/audio settings, race configuration, and pilot selection.
/// </remarks>
public interface IMainMenuPages
{
    /// <summary>
    /// Gets or sets the callback action to be invoked when the user quits the game.
    /// </summary>
    Action? QuitGameActionCallBack { get; set; }

    /// <summary>
    /// Gets or sets the current state of the keyboard input.
    /// </summary>
    OpenTK.Windowing.GraphicsLibraryFramework.KeyboardState? CurrentKeyboardState { get; set; }

    /// <summary>
    /// Creates and returns the main menu page.
    /// </summary>
    /// <returns>A MenuPage object representing the main menu.</returns>
    MenuPage CreateMainMenu();

    /// <summary>
    /// Creates and returns the options menu page.
    /// </summary>
    /// <returns>A MenuPage object representing the options menu.</returns>
    MenuPage CreateOptionsMenu();

    /// <summary>
    /// Creates and returns the controls menu page for rebinding controls.
    /// </summary>
    /// <returns>A MenuPage object representing the controls menu.</returns>
    MenuPage CreateControlsMenu();

    /// <summary>
    /// Creates and returns a menu page for awaiting user input for a specific race action.
    /// </summary>
    /// <param name="action">The race action for which input is being awaited.</param>
    /// <returns>A MenuPage object representing the awaiting input menu.</returns>
    MenuPage CreateAwaitingInputMenu(RaceAction action);

    /// <summary>
    /// Updates the awaiting input state based on elapsed time.
    /// </summary>
    /// <param name="deltaTime">The elapsed time in seconds since the last update.</param>
    /// <returns>True if the input awaiting state has changed; otherwise, false.</returns>
    bool UpdateAwaitingInput(float deltaTime);

    /// <summary>
    /// Updates the key release state based on whether any key is currently held down.
    /// </summary>
    /// <param name="anyKeyDown">True if any key is currently pressed; otherwise, false.</param>
    /// <returns>True if the key release state has changed; otherwise, false.</returns>
    bool UpdateKeyReleaseState(bool anyKeyDown);

    /// <summary>
    /// Captures a button input and assigns it to a control action.
    /// </summary>
    /// <param name="button">The button identifier to capture.</param>
    /// <param name="isKeyboard">True if the button is a keyboard key; false if it is a gamepad button.</param>
    void CaptureButtonForControl(uint button, bool isKeyboard);

    /// <summary>
    /// Creates and returns the video settings menu page.
    /// </summary>
    /// <returns>A MenuPage object representing the video menu.</returns>
    MenuPage CreateVideoMenu();

    /// <summary>
    /// Creates and returns the audio settings menu page.
    /// </summary>
    /// <returns>A MenuPage object representing the audio menu.</returns>
    MenuPage CreateAudioMenu();

    /// <summary>
    /// Creates and returns the best times viewer menu page.
    /// </summary>
    /// <returns>A MenuPage object representing the best times menu.</returns>
    MenuPage CreateBestTimesMenu();

    /// <summary>
    /// Handles user input for the best times viewer.
    /// </summary>
    /// <param name="action">The action to be performed in the best times viewer.</param>
    void HandleBestTimesViewerInput(BestTimesViewerAction action);

    /// <summary>
    /// Creates and returns the race class selection menu page.
    /// </summary>
    /// <returns>A MenuPage object representing the race class menu.</returns>
    MenuPage CreateRaceClassMenu();

    /// <summary>
    /// Creates and returns the race type selection menu page.
    /// </summary>
    /// <returns>A MenuPage object representing the race type menu.</returns>
    MenuPage CreateRaceTypeMenu();

    /// <summary>
    /// Creates and returns the team selection menu page.
    /// </summary>
    /// <returns>A MenuPage object representing the team menu.</returns>
    MenuPage CreateTeamMenu();

    /// <summary>
    /// Creates and returns the pilot selection menu page for a specific team.
    /// </summary>
    /// <param name="teamId">The identifier of the team for which pilots are being selected.</param>
    /// <returns>A MenuPage object representing the pilot menu.</returns>
    MenuPage CreatePilotMenu(int teamId);

    /// <summary>
    /// Creates and returns the circuit/track selection menu page.
    /// </summary>
    /// <returns>A MenuPage object representing the circuit menu.</returns>
    MenuPage CreateCircuitMenu();

    /// <summary>
    /// Creates and returns the quit confirmation dialog menu page.
    /// </summary>
    /// <returns>A MenuPage object representing the quit confirmation menu.</returns>
    MenuPage CreateQuitConfirmation();
}