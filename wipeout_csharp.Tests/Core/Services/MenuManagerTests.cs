using WipeoutRewrite.Core.Services;
using Xunit;

namespace WipeoutRewrite.Tests.Core.Services;

public class MenuManagerTests
{
    [Fact]
    public void Constructor_InitializesWithEmptyStack()
    {
        var manager = new MenuManager();

        Assert.Null(manager.CurrentPage);
    }

    [Fact]
    public void PushPage_AddsPageToStack()
    {
        var manager = new MenuManager();
        var page = new MenuPage { Title = "Test Page" };

        manager.PushPage(page);

        Assert.NotNull(manager.CurrentPage);
        Assert.Equal("Test Page", manager.CurrentPage!.Title);
    }

    [Fact]
    public void PopPage_RemovesPageFromStack()
    {
        var manager = new MenuManager();
        var page1 = new MenuPage { Title = "Page 1" };
        var page2 = new MenuPage { Title = "Page 2" };

        manager.PushPage(page1);
        manager.PushPage(page2);
        manager.PopPage();

        Assert.Equal("Page 1", manager.CurrentPage!.Title);
    }

    [Fact]
    public void PopPage_WithEmptyStack_DoesNothing()
    {
        var manager = new MenuManager();

        manager.PopPage();

        Assert.Null(manager.CurrentPage);
    }

    [Fact]
    public void Update_IncreasesBlinkTimer()
    {
        var manager = new MenuManager();

        manager.Update(0.1f);

        Assert.True(manager.ShouldBlink());
    }

    [Fact]
    public void ShouldBlink_ReturnsTrueInitially()
    {
        var manager = new MenuManager();

        Assert.True(manager.ShouldBlink());
    }

    [Fact]
    public void ShouldBlink_ReturnsCorrectlyAfterTime()
    {
        var manager = new MenuManager();

        // First half of interval - should blink
        manager.Update(0.1f);
        Assert.True(manager.ShouldBlink());

        // Second half of interval - should not blink
        manager.Update(0.3f);
        Assert.False(manager.ShouldBlink());

        // Reset after full cycle
        manager.Update(0.3f);
        Assert.True(manager.ShouldBlink());
    }

    [Fact]
    public void HandleInput_WithNullPage_ReturnsFalse()
    {
        var manager = new MenuManager();

        var result = manager.HandleInput(MenuAction.Up);

        Assert.False(result);
    }

    [Fact]
    public void HandleInput_WithEmptyItems_ReturnsFalse()
    {
        var manager = new MenuManager();
        var page = new MenuPage();
        manager.PushPage(page);

        var result = manager.HandleInput(MenuAction.Up);

        Assert.False(result);
    }

    [Fact]
    public void HandleInput_Up_MovesSelectionUp_Vertical()
    {
        var manager = new MenuManager();
        var page = new MenuPage
        {
            LayoutFlags = MenuLayoutFlags.Vertical,
            SelectedIndex = 1
        };
        page.Items.Add(new MenuButton { Label = "Item 1" });
        page.Items.Add(new MenuButton { Label = "Item 2" });
        page.Items.Add(new MenuButton { Label = "Item 3" });
        manager.PushPage(page);

        var result = manager.HandleInput(MenuAction.Up);

        Assert.True(result);
        Assert.Equal(0, page.SelectedIndex);
    }

    [Fact]
    public void HandleInput_Up_WrapsAround_Vertical()
    {
        var manager = new MenuManager();
        var page = new MenuPage
        {
            LayoutFlags = MenuLayoutFlags.Vertical,
            SelectedIndex = 0
        };
        page.Items.Add(new MenuButton { Label = "Item 1" });
        page.Items.Add(new MenuButton { Label = "Item 2" });
        page.Items.Add(new MenuButton { Label = "Item 3" });
        manager.PushPage(page);

        manager.HandleInput(MenuAction.Up);

        Assert.Equal(2, page.SelectedIndex);
    }

    [Fact]
    public void HandleInput_Down_MovesSelectionDown_Vertical()
    {
        var manager = new MenuManager();
        var page = new MenuPage
        {
            LayoutFlags = MenuLayoutFlags.Vertical,
            SelectedIndex = 0
        };
        page.Items.Add(new MenuButton { Label = "Item 1" });
        page.Items.Add(new MenuButton { Label = "Item 2" });
        manager.PushPage(page);

        var result = manager.HandleInput(MenuAction.Down);

        Assert.True(result);
        Assert.Equal(1, page.SelectedIndex);
    }

    [Fact]
    public void HandleInput_Down_WrapsAround_Vertical()
    {
        var manager = new MenuManager();
        var page = new MenuPage
        {
            LayoutFlags = MenuLayoutFlags.Vertical,
            SelectedIndex = 2
        };
        page.Items.Add(new MenuButton { Label = "Item 1" });
        page.Items.Add(new MenuButton { Label = "Item 2" });
        page.Items.Add(new MenuButton { Label = "Item 3" });
        manager.PushPage(page);

        manager.HandleInput(MenuAction.Down);

        Assert.Equal(0, page.SelectedIndex);
    }

    [Fact]
    public void HandleInput_Left_DecrementsToggle()
    {
        var manager = new MenuManager();
        var toggle = new MenuToggle
        {
            Label = "Option",
            Options = new[] { "A", "B", "C" },
            CurrentIndex = 1
        };
        var page = new MenuPage { SelectedIndex = 0 };
        page.Items.Add(toggle);
        manager.PushPage(page);

        var result = manager.HandleInput(MenuAction.Left);

        Assert.True(result);
        Assert.Equal(0, toggle.CurrentIndex);
    }

    [Fact]
    public void HandleInput_Right_IncrementsToggle()
    {
        var manager = new MenuManager();
        var toggle = new MenuToggle
        {
            Label = "Option",
            Options = new[] { "A", "B", "C" },
            CurrentIndex = 0
        };
        var page = new MenuPage { SelectedIndex = 0 };
        page.Items.Add(toggle);
        manager.PushPage(page);

        var result = manager.HandleInput(MenuAction.Right);

        Assert.True(result);
        Assert.Equal(1, toggle.CurrentIndex);
    }

    [Fact]
    public void HandleInput_Left_MovesSelection_Horizontal()
    {
        var manager = new MenuManager();
        var page = new MenuPage
        {
            LayoutFlags = MenuLayoutFlags.Horizontal,
            SelectedIndex = 1
        };
        page.Items.Add(new MenuButton { Label = "Item 1" });
        page.Items.Add(new MenuButton { Label = "Item 2" });
        manager.PushPage(page);

        var result = manager.HandleInput(MenuAction.Left);

        Assert.True(result);
        Assert.Equal(0, page.SelectedIndex);
    }

    [Fact]
    public void HandleInput_Right_MovesSelection_Horizontal()
    {
        var manager = new MenuManager();
        var page = new MenuPage
        {
            LayoutFlags = MenuLayoutFlags.Horizontal,
            SelectedIndex = 0
        };
        page.Items.Add(new MenuButton { Label = "Item 1" });
        page.Items.Add(new MenuButton { Label = "Item 2" });
        manager.PushPage(page);

        var result = manager.HandleInput(MenuAction.Right);

        Assert.True(result);
        Assert.Equal(1, page.SelectedIndex);
    }

    [Fact]
    public void HandleInput_Select_ActivatesButton()
    {
        var manager = new MenuManager();
        var clicked = false;
        var button = new MenuButton
        {
            Label = "Click Me",
            OnClick = (m, d) => clicked = true
        };
        var page = new MenuPage { SelectedIndex = 0 };
        page.Items.Add(button);
        manager.PushPage(page);

        var result = manager.HandleInput(MenuAction.Select);

        Assert.True(result);
        Assert.True(clicked);
    }

    [Fact]
    public void HandleInput_Select_WithDisabledItem_ReturnsFalse()
    {
        var manager = new MenuManager();
        var button = new MenuButton
        {
            Label = "Disabled",
            IsEnabled = false
        };
        var page = new MenuPage { SelectedIndex = 0 };
        page.Items.Add(button);
        manager.PushPage(page);

        var result = manager.HandleInput(MenuAction.Select);

        Assert.False(result);
    }

    [Fact]
    public void HandleInput_Back_PopsPage()
    {
        var manager = new MenuManager();
        var page1 = new MenuPage { Title = "Page 1" };
        var page2 = new MenuPage { Title = "Page 2" };
        page2.Items.Add(new MenuButton { Label = "Button" }); // Add item so page is not empty
        manager.PushPage(page1);
        manager.PushPage(page2);

        var result = manager.HandleInput(MenuAction.Back);

        Assert.True(result);
        Assert.Equal("Page 1", manager.CurrentPage!.Title);
    }

    [Fact]
    public void HandleInput_Back_WithSinglePage_ReturnsFalse()
    {
        var manager = new MenuManager();
        var page = new MenuPage { Title = "Page 1" };
        page.Items.Add(new MenuButton { Label = "Button" }); // Add item
        manager.PushPage(page);

        var result = manager.HandleInput(MenuAction.Back);

        Assert.False(result);
    }

    [Fact]
    public void PushPage_ResetsBlinkTimer()
    {
        var manager = new MenuManager();
        manager.Update(0.5f);
        var page = new MenuPage();

        manager.PushPage(page);

        Assert.True(manager.ShouldBlink());
    }

    [Fact]
    public void PopPage_ResetsBlinkTimer()
    {
        var manager = new MenuManager();
        var page1 = new MenuPage();
        var page2 = new MenuPage();
        manager.PushPage(page1);
        manager.PushPage(page2);
        manager.Update(0.5f);

        manager.PopPage();

        Assert.True(manager.ShouldBlink());
    }
}
