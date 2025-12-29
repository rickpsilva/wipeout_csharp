namespace WipeoutRewrite.Core.Services;

public interface IMenuManager
{
    MenuPage? CurrentPage { get; }

    bool HandleInput(MenuAction action);
    void PopPage();
    void PushPage(MenuPage page);
    bool ShouldBlink();
    void Update(float deltaTime);
}

public class MenuButton : MenuItem
{
    public Action<IMenuManager, int>? OnClick { get; set; }

    public override void OnActivate(IMenuManager menu)
    {
        OnClick?.Invoke(menu, Data);
    }
}

public abstract class MenuItem
{
    public int Data { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string Label { get; set; } = "";

    /// <summary>
    /// Optional 3D content preview information for this menu item.
    /// When set, defines which 3D model should be rendered when this item is selected.
    /// </summary>
    public ContentPreview3DInfo? PreviewInfo { get; set; }

    public abstract void OnActivate(IMenuManager menu);
}

public class MenuPage
{
    #region properties
    public int BlockWidth { get; set; } = 200;
    public Action<float>? DrawCallback { get; set; }
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