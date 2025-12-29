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
        Assert.Null(item.PreviewInfo);
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
        Assert.Null(item.PreviewInfo);
    }

    [Fact]
    public void PreviewInfo_ShouldBeSettable()
    {
        // Arrange
        var item = new TestMenuItem();
        var previewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 5);

        // Act
        item.PreviewInfo = previewInfo;

        // Assert
        Assert.NotNull(item.PreviewInfo);
        Assert.Equal(typeof(CategoryShip), item.PreviewInfo.CategoryType);
        Assert.Equal(5, item.PreviewInfo.ModelIndex);
    }

    [Fact]
    public void PreviewInfo_CanBeSetToNull()
    {
        // Arrange
        var item = new TestMenuItem
        {
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 0)
        };

        // Act
        item.PreviewInfo = null;

        // Assert
        Assert.Null(item.PreviewInfo);
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
            PreviewInfo = previewInfo
        };

        // Assert
        Assert.Equal("Options", button.Label);
        Assert.NotNull(button.PreviewInfo);
        Assert.Equal(typeof(CategoryMsDos), button.PreviewInfo.CategoryType);
        Assert.Equal(3, button.PreviewInfo.ModelIndex);
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
        Assert.Null(button.PreviewInfo);
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
            PreviewInfo = previewInfo,
            Options = new[] { "OFF", "ON" }
        };

        // Assert
        Assert.Equal("Fullscreen", toggle.Label);
        Assert.NotNull(toggle.PreviewInfo);
        Assert.Equal(typeof(CategoryTeams), toggle.PreviewInfo.CategoryType);
        Assert.Equal(2, toggle.PreviewInfo.ModelIndex);
    }

    [Fact]
    public void PreviewInfo_WithDifferentCategories_ShouldMaintainCorrectTypes()
    {
        // Arrange & Act
        var shipItem = new MenuButton
        {
            Label = "Ship",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryShip), 0)
        };
        
        var msDosItem = new MenuButton
        {
            Label = "MsDos",
            PreviewInfo = new ContentPreview3DInfo(typeof(CategoryMsDos), 1)
        };

        // Assert
        Assert.Equal(typeof(CategoryShip), shipItem.PreviewInfo!.CategoryType);
        Assert.Equal(typeof(CategoryMsDos), msDosItem.PreviewInfo!.CategoryType);
        Assert.NotEqual(shipItem.PreviewInfo.CategoryType, msDosItem.PreviewInfo.CategoryType);
    }
}
