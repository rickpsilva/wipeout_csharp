using Xunit;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Presentation;

namespace WipeoutRewrite.Tests;

public class ContentPreview3DInfoTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Arrange
        var categoryType = typeof(CategoryShip);
        var modelIndex = 5;

        // Act
        var info = new ContentPreview3DInfo(categoryType, modelIndex);

        // Assert
        Assert.NotNull(info);
        Assert.Equal(categoryType, info.CategoryType);
        Assert.Equal(modelIndex, info.ModelIndex);
    }

    [Fact]
    public void Constructor_WithNullCategoryType_ShouldThrowArgumentNullException()
    {
        // Arrange
        Type? nullType = null;
        var modelIndex = 5;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ContentPreview3DInfo(nullType!, modelIndex));
    }

    [Fact]
    public void Constructor_WithZeroModelIndex_ShouldInitializeCorrectly()
    {
        // Arrange
        var categoryType = typeof(CategoryMsDos);
        var modelIndex = 0;

        // Act
        var info = new ContentPreview3DInfo(categoryType, modelIndex);

        // Assert
        Assert.Equal(0, info.ModelIndex);
    }

    [Fact]
    public void Constructor_WithNegativeModelIndex_ShouldInitializeCorrectly()
    {
        // Arrange
        var categoryType = typeof(CategoryTeams);
        var modelIndex = -1;

        // Act
        var info = new ContentPreview3DInfo(categoryType, modelIndex);

        // Assert
        Assert.Equal(-1, info.ModelIndex);
    }

    [Fact]
    public void Constructor_WithDifferentCategoryTypes_ShouldStoreDifferentTypes()
    {
        // Arrange & Act
        var shipInfo = new ContentPreview3DInfo(typeof(CategoryShip), 0);
        var msDosInfo = new ContentPreview3DInfo(typeof(CategoryMsDos), 0);
        var teamsInfo = new ContentPreview3DInfo(typeof(CategoryTeams), 0);

        // Assert
        Assert.NotEqual(shipInfo.CategoryType, msDosInfo.CategoryType);
        Assert.NotEqual(shipInfo.CategoryType, teamsInfo.CategoryType);
        Assert.NotEqual(msDosInfo.CategoryType, teamsInfo.CategoryType);
    }

    [Fact]
    public void CategoryType_ShouldBeSettable()
    {
        // Arrange
        var info = new ContentPreview3DInfo(typeof(CategoryShip), 0);
        var newType = typeof(CategoryMsDos);

        // Act
        info.CategoryType = newType;

        // Assert
        Assert.Equal(newType, info.CategoryType);
    }

    [Fact]
    public void ModelIndex_ShouldBeSettable()
    {
        // Arrange
        var info = new ContentPreview3DInfo(typeof(CategoryShip), 0);
        var newIndex = 10;

        // Act
        info.ModelIndex = newIndex;

        // Assert
        Assert.Equal(newIndex, info.ModelIndex);
    }
}
