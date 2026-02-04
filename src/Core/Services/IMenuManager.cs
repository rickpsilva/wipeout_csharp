using WipeoutRewrite.Infrastructure.UI;

namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Manages the menu system, including page navigation and input handling.
/// </summary>
public interface IMenuManager
{
    /// <summary>
    /// Gets the currently active menu page.
    /// </summary>
    MenuPage? CurrentPage { get; }

    /// <summary>
    /// Handles user input actions and processes them in the current menu page.
    /// </summary>
    /// <param name="action">The menu action to handle.</param>
    /// <returns>True if the action was handled; otherwise, false.</returns>
    bool HandleInput(MenuAction action);

    /// <summary>
    /// Removes the current page from the menu stack and returns to the previous page.
    /// </summary>
    void PopPage();

    /// <summary>
    /// Adds a new page to the menu stack and makes it the current page.
    /// </summary>
    /// <param name="page">The menu page to push onto the stack.</param>
    void PushPage(MenuPage page);

    /// <summary>
    /// Determines whether the menu should display a blinking effect.
    /// </summary>
    /// <returns>True if blinking should occur; otherwise, false.</returns>
    bool ShouldBlink();

    /// <summary>
    /// Updates the menu state with the elapsed time since the last update.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update, in seconds.</param>
    void Update(float deltaTime);
}

public struct Vec2i
{
    public int X { get; set; }
    public int Y { get; set; }

    public Vec2i(int x, int y)
    {
        X = x;
        Y = y;
    }
}