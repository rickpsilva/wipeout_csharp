using Xunit;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Presentation;

namespace WipeoutRewrite.Tests;

public class MenuItemTests
{
    private class TestMenuItem : MenuItem
    {
        public bool WasActivated { get; private set; }
        
        public override void OnActivate(IMenuManager menu)
        {
            WasActivated = true;
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var item = new TestMenuItem();

        // Assert
        Assert.Equal(string.Empty, item.Label);
        Assert.Equal(0, item.Data);
        Assert.True(item.IsEnabled);
        Assert.Null(item.ContentViewPort);
    }

    [Fact]
    public void Label_ShouldBeSettable()
    {
        // Arrange
        var item = new TestMenuItem();
        var label = "Test Label";

        // Act
        item.Label = label;

        // Assert
        Assert.Equal(label, item.Label);
    }

    [Fact]
    public void Data_ShouldBeSettable()
    {
        // Arrange
        var item = new TestMenuItem();
        var data = 42;

        // Act
        item.Data = data;

        // Assert
        Assert.Equal(data, item.Data);
    }

    [Fact]
    public void IsEnabled_ShouldBeSettable()
    {
        // Arrange
        var item = new TestMenuItem { IsEnabled = true };

        // Act
        item.IsEnabled = false;

        // Assert
        Assert.False(item.IsEnabled);
    }

    [Fact]
    public void PreviewInfo_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var item = new TestMenuItem();

        // Assert
        Assert.Null(item.ContentViewPort);
    }

    [Fact]
    public void PreviewInfo_ShouldBeSettable()
    {
        // Arrange
        var item = new TestMenuItem();
        var previewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 5);

        // Act
        item.ContentViewPort = previewInfo;

        // Assert
        Assert.NotNull(item.ContentViewPort);
        Assert.Equal(typeof(CategoryShip), item.ContentViewPort.CategoryType);
        Assert.Equal(5, item.ContentViewPort.ModelIndex);
    }

    [Fact]
    public void PreviewInfo_CanBeSetToNull()
    {
        // Arrange
        var item = new TestMenuItem
        {
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryShip), 0)
        };

        // Act
        item.ContentViewPort = null;

        // Assert
        Assert.Null(item.ContentViewPort);
    }

    [Fact]
    public void MenuButton_WithPreviewInfo_ShouldStoreCorrectly()
    {
        // Arrange
        var previewInfo = new ContentPreview3DInfo(typeof(CategoryMsDos), 3);
        
        // Act
        var button = new MenuButton
        {
            Label = "Options",
            ContentViewPort = previewInfo
        };

        // Assert
        Assert.Equal("Options", button.Label);
        Assert.NotNull(button.ContentViewPort);
        Assert.Equal(typeof(CategoryMsDos), button.ContentViewPort.CategoryType);
        Assert.Equal(3, button.ContentViewPort.ModelIndex);
    }

    [Fact]
    public void MenuButton_WithoutPreviewInfo_ShouldHaveNullPreviewInfo()
    {
        // Act
        var button = new MenuButton
        {
            Label = "Start Game"
        };

        // Assert
        Assert.Null(button.ContentViewPort);
    }

    [Fact]
    public void MenuToggle_WithPreviewInfo_ShouldStoreCorrectly()
    {
        // Arrange
        var previewInfo = new ContentPreview3DInfo(typeof(CategoryTeams), 2);
        
        // Act
        var toggle = new MenuToggle
        {
            Label = "Fullscreen",
            ContentViewPort = previewInfo,
            Options = new[] { "OFF", "ON" }
        };

        // Assert
        Assert.Equal("Fullscreen", toggle.Label);
        Assert.NotNull(toggle.ContentViewPort);
        Assert.Equal(typeof(CategoryTeams), toggle.ContentViewPort.CategoryType);
        Assert.Equal(2, toggle.ContentViewPort.ModelIndex);
    }

    [Fact]
    public void PreviewInfo_WithDifferentCategories_ShouldMaintainCorrectTypes()
    {
        // Arrange & Act
        var shipItem = new MenuButton
        {
            Label = "Ship",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryShip), 0)
        };
        
        var msDosItem = new MenuButton
        {
            Label = "MsDos",
            ContentViewPort = new ContentPreview3DInfo(typeof(CategoryMsDos), 1)
        };

        // Assert
        Assert.Equal(typeof(CategoryShip), shipItem.ContentViewPort!.CategoryType);
        Assert.Equal(typeof(CategoryMsDos), msDosItem.ContentViewPort!.CategoryType);
        Assert.NotEqual(shipItem.ContentViewPort.CategoryType, msDosItem.ContentViewPort.CategoryType);
    }
}
