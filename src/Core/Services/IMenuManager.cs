namespace WipeoutRewrite.Core.Services;

public interface IMenuManager
{
    void PushPage(MenuPage page);
    void PopPage();
    MenuPage? CurrentPage { get; }
    void Update(float deltaTime);
    bool HandleInput(MenuAction action);
    bool ShouldBlink();
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

public class MenuPage
{
    public string Title { get; set; } = "";
    public List<MenuItem> Items { get; } = new();
    public int SelectedIndex { get; set; }
    public MenuLayoutFlags LayoutFlags { get; set; }
    public Vec2i TitlePos { get; set; }
    public Vec2i ItemsPos { get; set; }
    public UIAnchor TitleAnchor { get; set; }
    public UIAnchor ItemsAnchor { get; set; }
    public int BlockWidth { get; set; } = 200;
    public Action<float>? DrawCallback { get; set; } // Custom draw for 3D models, etc.
    
    public MenuItem? SelectedItem => SelectedIndex >= 0 && SelectedIndex < Items.Count 
        ? Items[SelectedIndex] 
        : null;
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
    public int X;
    public int Y;
    
    public Vec2i(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public abstract class MenuItem
{
    public string Label { get; set; } = "";
    public int Data { get; set; }
    public bool IsEnabled { get; set; } = true;
    
    public abstract void OnActivate(IMenuManager menu);
}

public class MenuButton : MenuItem
{
    public Action<IMenuManager, int>? OnClick { get; set; }
    
    public override void OnActivate(IMenuManager menu)
    {
        OnClick?.Invoke(menu, Data);
    }
}

public class MenuToggle : MenuItem
{
    public string[] Options { get; set; } = Array.Empty<string>();
    public int CurrentIndex { get; set; }
    public Action<IMenuManager, int>? OnChange { get; set; }
    
    public string CurrentValue => CurrentIndex >= 0 && CurrentIndex < Options.Length 
        ? Options[CurrentIndex] 
        : "";
    
    public override void OnActivate(IMenuManager menu)
    {
        // Left/Right will cycle through options
    }
    
    public void Increment()
    {
        if (Options.Length > 0)
        {
            CurrentIndex = (CurrentIndex + 1) % Options.Length;
            OnChange?.Invoke(null!, CurrentIndex);
        }
    }
    
    public void Decrement()
    {
        if (Options.Length > 0)
        {
            CurrentIndex = (CurrentIndex - 1 + Options.Length) % Options.Length;
            OnChange?.Invoke(null!, CurrentIndex);
        }
    }
}
