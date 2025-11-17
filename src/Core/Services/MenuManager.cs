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
