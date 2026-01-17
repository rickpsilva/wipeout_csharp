using WipeoutRewrite.Core.Services;
using Xunit;

namespace WipeoutRewrite.Tests.Core.Services;

public class MenuToggleTests
{
    [Fact]
    public void Constructor_InitializesDefaults()
    {
        var toggle = new MenuToggle();
        
        Assert.Equal(0, toggle.CurrentIndex);
        Assert.Empty(toggle.Options);
        Assert.Equal("", toggle.CurrentValue);
        Assert.True(toggle.IsEnabled);
    }

    [Fact]
    public void CurrentValue_WithValidIndex_ReturnsOption()
    {
        var toggle = new MenuToggle
        {
            Options = new[] { "Option1", "Option2", "Option3" },
            CurrentIndex = 1
        };
        
        Assert.Equal("Option2", toggle.CurrentValue);
    }

    [Fact]
    public void CurrentValue_WithInvalidIndex_ReturnsEmpty()
    {
        var toggle = new MenuToggle
        {
            Options = new[] { "Option1", "Option2" },
            CurrentIndex = 5
        };
        
        Assert.Equal("", toggle.CurrentValue);
    }

    [Fact]
    public void CurrentValue_WithNegativeIndex_ReturnsEmpty()
    {
        var toggle = new MenuToggle
        {
            Options = new[] { "Option1", "Option2" },
            CurrentIndex = -1
        };
        
        Assert.Equal("", toggle.CurrentValue);
    }

    [Fact]
    public void Increment_IncreasesIndex()
    {
        var toggle = new MenuToggle
        {
            Options = new[] { "A", "B", "C" },
            CurrentIndex = 0
        };
        
        toggle.Increment();
        
        Assert.Equal(1, toggle.CurrentIndex);
    }

    [Fact]
    public void Increment_WrapsAroundAtEnd()
    {
        var toggle = new MenuToggle
        {
            Options = new[] { "A", "B", "C" },
            CurrentIndex = 2
        };
        
        toggle.Increment();
        
        Assert.Equal(0, toggle.CurrentIndex);
    }

    [Fact]
    public void Increment_CallsOnChange()
    {
        var callbackInvoked = false;
        var toggle = new MenuToggle
        {
            Options = new[] { "A", "B" },
            CurrentIndex = 0,
            OnChange = (menu, index) => callbackInvoked = true
        };
        
        toggle.Increment();
        
        Assert.True(callbackInvoked);
    }

    [Fact]
    public void Decrement_DecreasesIndex()
    {
        var toggle = new MenuToggle
        {
            Options = new[] { "A", "B", "C" },
            CurrentIndex = 2
        };
        
        toggle.Decrement();
        
        Assert.Equal(1, toggle.CurrentIndex);
    }

    [Fact]
    public void Decrement_WrapsAroundAtStart()
    {
        var toggle = new MenuToggle
        {
            Options = new[] { "A", "B", "C" },
            CurrentIndex = 0
        };
        
        toggle.Decrement();
        
        Assert.Equal(2, toggle.CurrentIndex);
    }

    [Fact]
    public void Decrement_CallsOnChange()
    {
        var callbackInvoked = false;
        var toggle = new MenuToggle
        {
            Options = new[] { "A", "B" },
            CurrentIndex = 1,
            OnChange = (menu, index) => callbackInvoked = true
        };
        
        toggle.Decrement();
        
        Assert.True(callbackInvoked);
    }

    [Fact]
    public void Increment_WithEmptyOptions_DoesNothing()
    {
        var toggle = new MenuToggle
        {
            Options = System.Array.Empty<string>(),
            CurrentIndex = 0
        };
        
        toggle.Increment();
        
        Assert.Equal(0, toggle.CurrentIndex);
    }

    [Fact]
    public void Decrement_WithEmptyOptions_DoesNothing()
    {
        var toggle = new MenuToggle
        {
            Options = System.Array.Empty<string>(),
            CurrentIndex = 0
        };
        
        toggle.Decrement();
        
        Assert.Equal(0, toggle.CurrentIndex);
    }

    [Fact]
    public void OnActivate_DoesNothing()
    {
        var toggle = new MenuToggle();
        var manager = new MenuManager();
        
        // Should not throw
        toggle.OnActivate(manager);
        Assert.True(true);
    }

    [Fact]
    public void Label_CanBeSet()
    {
        var toggle = new MenuToggle { Label = "Test Label" };
        Assert.Equal("Test Label", toggle.Label);
    }

    [Fact]
    public void IsEnabled_CanBeSet()
    {
        var toggle = new MenuToggle { IsEnabled = false };
        Assert.False(toggle.IsEnabled);
    }

    [Fact]
    public void Data_CanBeSet()
    {
        var toggle = new MenuToggle { Data = 42 };
        Assert.Equal(42, toggle.Data);
    }
}
