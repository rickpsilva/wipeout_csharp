using WipeoutRewrite.Infrastructure.UI;

namespace WipeoutRewrite.Core.Services;

public class MenuManager : IMenuManager
{
    private readonly Stack<MenuPage> _pageStack = new();
    private float _blinkTimer;
    private const float BlinkInterval = 0.3f;
    
    public MenuPage? CurrentPage => _pageStack.Count > 0 ? _pageStack.Peek() : null;
    
    public void PushPage(MenuPage page)
    {
        _pageStack.Push(page);
        _blinkTimer = 0f;
    }
    
    public void PopPage()
    {
        if (_pageStack.Count > 0)
        {
            _pageStack.Pop();
            _blinkTimer = 0f;
        }
    }
    
    public void Update(float deltaTime)
    {
        _blinkTimer += deltaTime;
        if (_blinkTimer >= BlinkInterval * 2)
        {
            _blinkTimer = 0f;
        }
    }
    
    public bool ShouldBlink()
    {
        return _blinkTimer < BlinkInterval;
    }
    
    public bool HandleInput(MenuAction action)
    {
        var page = CurrentPage;
        if (page == null || page.Items.Count == 0)
            return false;
        
        var selectedItem = page.SelectedItem;
        bool isHorizontal = page.LayoutFlags.HasFlag(MenuLayoutFlags.Horizontal);
        bool isVertical = page.LayoutFlags.HasFlag(MenuLayoutFlags.Vertical);
        
        switch (action)
        {
            case MenuAction.Up:
                if (isVertical || !isHorizontal)
                {
                    page.SelectedIndex = (page.SelectedIndex - 1 + page.Items.Count) % page.Items.Count;
                    return true;
                }
                break;
                
            case MenuAction.Down:
                if (isVertical || !isHorizontal)
                {
                    page.SelectedIndex = (page.SelectedIndex + 1) % page.Items.Count;
                    return true;
                }
                break;
                
            case MenuAction.Left:
                if (selectedItem is MenuToggle toggle)
                {
                    toggle.Decrement();
                    return true;
                }
                else if (isHorizontal)
                {
                    page.SelectedIndex = (page.SelectedIndex - 1 + page.Items.Count) % page.Items.Count;
                    return true;
                }
                break;
                
            case MenuAction.Right:
                if (selectedItem is MenuToggle toggle2)
                {
                    toggle2.Increment();
                    return true;
                }
                else if (isHorizontal)
                {
                    page.SelectedIndex = (page.SelectedIndex + 1) % page.Items.Count;
                    return true;
                }
                break;
                
            case MenuAction.Select:
                if (selectedItem != null && selectedItem.IsEnabled)
                {
                    selectedItem.OnActivate(this);
                    return true;
                }
                break;
                
            case MenuAction.Back:
                if (_pageStack.Count > 1)
                {
                    PopPage();
                    return true;
                }
                break;
        }
        
        return false;
    }
}

public class MenuButton : MenuItem
{
    public Action<IMenuManager, int>? OnClick { get; set; }

    public override void OnActivate(IMenuManager menu)
    {
        OnClick?.Invoke(menu, Data);
    }
}

/// <summary>
/// Abstract base class representing a menu item in the menu system.
/// Provides core properties and behavior that all menu items must implement.
/// </summary>
/// <remarks>
/// Derived classes must implement the <see cref="OnActivate"/> method to define
/// the specific behavior when the menu item is activated by the user.
/// </remarks>
public abstract class MenuItem
{
    /// <summary>
    /// Gets or sets arbitrary integer data associated with this menu item.
    /// Can be used to store identifiers, indices, or other numeric metadata.
    /// </summary>
    public int Data { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this menu item is enabled.
    /// When disabled, the item typically appears grayed out and cannot be activated.
    /// </summary>
    /// <value>
    /// <c>true</c> if the menu item is enabled; otherwise, <c>false</c>. Defaults to <c>true</c>.
    /// </value>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the display text label for this menu item.
    /// This text is typically rendered in the user interface.
    /// </summary>
    /// <value>
    /// The label text for the menu item. Defaults to an empty string.
    /// </value>
    public string Label { get; set; } = "";

    /// <summary>
    /// Gets or sets optional 3D content preview information for this menu item.
    /// When set, defines which 3D model should be rendered when this item is selected.
    /// </summary>
    /// <value>
    /// A <see cref="ContentPreview3DInfo"/> object containing preview data, or <c>null</c> if no preview is available.
    /// </value>
    public ContentPreview3DInfo? ContentViewPort { get; set; }

    /// <summary>
    /// Called when this menu item is activated by the user.
    /// Derived classes must implement this method to define the specific behavior
    /// triggered by activating this menu item.
    /// </summary>
    /// <param name="menu">
    /// The <see cref="IMenuManager"/> instance that manages the menu system.
    /// Can be used to navigate, modify menu state, or perform menu-related operations.
    /// </param>
    public abstract void OnActivate(IMenuManager menu);
}

public class MenuPage
{
    #region properties
    public int BlockWidth { get; set; } = 200;
    public Action<IMenuRenderer>? DrawCallback { get; set; }
    
    /// <summary>
    /// Optional unique identifier for this page (e.g., "awaitingInput", "bestTimes").
    /// Used by menu handlers to determine specific behavior per page.
    /// </summary>
    public string? Id { get; set; }
    
    public List<MenuItem> Items { get; } = new();
    public UIAnchor ItemsAnchor { get; set; }
    public Vec2i ItemsPos { get; set; }
    public MenuLayoutFlags LayoutFlags { get; set; }
    public int SelectedIndex { get; set; }

    // Custom draw for 3D models, etc.

    public MenuItem? SelectedItem => SelectedIndex >= 0 && SelectedIndex < Items.Count
        ? Items[SelectedIndex]
        : null;

    public string Title { get; set; } = "";
    public UIAnchor TitleAnchor { get; set; }
    public Vec2i TitlePos { get; set; }
    
    /// <summary>
    /// Optional multi-preview layout for screens with multiple 3D models/images
    /// (e.g., team selection with center logo + 2 ships)
    /// </summary>
    public PreviewLayout? PreviewLayout { get; set; }
    #endregion 
}

public class MenuToggle : MenuItem
{
    public int CurrentIndex { get; set; }

    public string CurrentValue => CurrentIndex >= 0 && CurrentIndex < Options.Length
        ? Options[CurrentIndex]
        : "";

    public Action<IMenuManager, int>? OnChange { get; set; }
    public string[] Options { get; set; } = Array.Empty<string>();

    public void Decrement()
    {
        if (Options.Length > 0)
        {
            CurrentIndex = (CurrentIndex - 1 + Options.Length) % Options.Length;
            OnChange?.Invoke(null!, CurrentIndex);
        }
    }

    public void Increment()
    {
        if (Options.Length > 0)
        {
            CurrentIndex = (CurrentIndex + 1) % Options.Length;
            OnChange?.Invoke(null!, CurrentIndex);
        }
    }

    public override void OnActivate(IMenuManager menu)
    {
        // Left/Right will cycle through options
    }
}

public enum MenuAction
{
    Up,
    Down,
    Left,
    Right,
    Select,
    Back
}

[Flags]
public enum MenuLayoutFlags
{
    None = 0,
    Vertical = 1,
    Horizontal = 2,
    Fixed = 4,
    AlignCenter = 8
}

public enum UIAnchor
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}