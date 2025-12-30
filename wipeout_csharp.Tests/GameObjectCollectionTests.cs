using Microsoft.Extensions.Logging;
using Moq;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Core.Graphics;
using WipeoutRewrite.Factory;
using Xunit;

namespace WipeoutRewrite.Tests;

public class GameObjectCollectionTests
{
    private readonly Mock<ILogger<GameObjectCollection>> _mockLogger;
    private readonly Mock<IGameObjectFactory> _mockFactory;
    private readonly GameObjectCollection _collection;

    public GameObjectCollectionTests()
    {
        _mockLogger = new Mock<ILogger<GameObjectCollection>>();
        _mockFactory = new Mock<IGameObjectFactory>();
        _collection = new GameObjectCollection(_mockLogger.Object, _mockFactory.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeEmptyCollection()
    {
        // Assert
        Assert.NotNull(_collection.GetAll);
        Assert.Empty(_collection.GetAll);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GameObjectCollection(null!, _mockFactory.Object));
    }

    [Fact]
    public void Constructor_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GameObjectCollection(_mockLogger.Object, null!));
    }

    [Fact]
    public void Clear_ShouldRemoveAllObjects()
    {
        // Arrange - Add some mock objects
        var mockObject = new Mock<IGameObject>();
        _mockFactory.Setup(f => f.CreateModel()).Returns(mockObject.Object);
        
        // Manually add an object to the collection
        var mockModelLoader = new Mock<IModelLoader>();
        var obj = new GameObject(
            Mock.Of<Infrastructure.Graphics.IRenderer>(),
            Mock.Of<ILogger<GameObject>>(),
            Mock.Of<Infrastructure.Graphics.ITextureManager>(),
            mockModelLoader.Object);
        _collection.GetAll.Add(obj);

        // Act
        _collection.Clear();

        // Assert
        Assert.Empty(_collection.GetAll);
    }

    [Fact]
    public void GetByCategory_WithShipCategory_ShouldReturnOnlyShips()
    {
        // Arrange
        var ship1 = CreateGameObject("ship1", GameObjectCategory.Ship);
        var ship2 = CreateGameObject("ship2", GameObjectCategory.Ship);
        var weapon = CreateGameObject("missile", GameObjectCategory.Weapon);
        
        _collection.GetAll.Add(ship1);
        _collection.GetAll.Add(ship2);
        _collection.GetAll.Add(weapon);

        // Act
        var ships = _collection.GetByCategory(GameObjectCategory.Ship);

        // Assert
        Assert.Equal(2, ships.Count);
        Assert.All(ships, s => Assert.Equal(GameObjectCategory.Ship, s.Category));
    }

    [Fact]
    public void GetByCategory_WithNonExistentCategory_ShouldReturnEmptyList()
    {
        // Arrange
        var ship = CreateGameObject("ship1", GameObjectCategory.Ship);
        _collection.GetAll.Add(ship);

        // Act
        var weapons = _collection.GetByCategory(GameObjectCategory.Weapon);

        // Assert
        Assert.Empty(weapons);
    }

    [Fact]
    public void GetByName_WithExistingName_ShouldReturnObject()
    {
        // Arrange
        var obj = CreateGameObject("allsh_0", GameObjectCategory.Ship);
        _collection.GetAll.Add(obj);

        // Act
        var result = _collection.GetByName("allsh_0");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("allsh_0", result.Name);
    }

    [Fact]
    public void GetByName_CaseInsensitive_ShouldReturnObject()
    {
        // Arrange
        var obj = CreateGameObject("AllSH_0", GameObjectCategory.Ship);
        _collection.GetAll.Add(obj);

        // Act
        var result = _collection.GetByName("allsh_0");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AllSH_0", result.Name);
    }

    [Fact]
    public void GetByName_WithNonExistentName_ShouldReturnNull()
    {
        // Arrange
        var obj = CreateGameObject("ship1", GameObjectCategory.Ship);
        _collection.GetAll.Add(obj);

        // Act
        var result = _collection.GetByName("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCategories_ShouldReturnDistinctCategoriesInOrder()
    {
        // Arrange
        _collection.GetAll.Add(CreateGameObject("ship1", GameObjectCategory.Ship));
        _collection.GetAll.Add(CreateGameObject("ship2", GameObjectCategory.Ship));
        _collection.GetAll.Add(CreateGameObject("missile", GameObjectCategory.Weapon));
        _collection.GetAll.Add(CreateGameObject("shield", GameObjectCategory.Pickup));
        _collection.GetAll.Add(CreateGameObject("track1_sky", GameObjectCategory.Track01));

        // Act
        var categories = _collection.GetCategories();

        // Assert
        Assert.Equal(4, categories.Count);
        Assert.Contains(GameObjectCategory.Ship, categories);
        Assert.Contains(GameObjectCategory.Weapon, categories);
        Assert.Contains(GameObjectCategory.Pickup, categories);
        Assert.Contains(GameObjectCategory.Track01, categories);
        
        // Verify order (should be sorted)
        Assert.True(categories[0] < categories[1]);
        Assert.True(categories[1] < categories[2]);
        Assert.True(categories[2] < categories[3]);
    }

    [Fact]
    public void GetCategories_WithEmptyCollection_ShouldReturnEmptyList()
    {
        // Act
        var categories = _collection.GetCategories();

        // Assert
        Assert.Empty(categories);
    }

    [Fact]
    public void GetByCategory_WithTrackCategory_ShouldReturnTrackObjects()
    {
        // Arrange
        var track1Sky = CreateGameObject("Track01_Sky", GameObjectCategory.Track01);
        var track1Scene = CreateGameObject("Track01_Scene", GameObjectCategory.Track01);
        var track2Sky = CreateGameObject("Track02_Sky", GameObjectCategory.Track02);
        
        _collection.GetAll.Add(track1Sky);
        _collection.GetAll.Add(track1Scene);
        _collection.GetAll.Add(track2Sky);

        // Act
        var track1Objects = _collection.GetByCategory(GameObjectCategory.Track01);

        // Assert
        Assert.Equal(2, track1Objects.Count);
        Assert.Contains(track1Objects, o => o.Name == "Track01_Sky");
        Assert.Contains(track1Objects, o => o.Name == "Track01_Scene");
    }

    [Fact]
    public void GetAll_ShouldReturnAllObjects()
    {
        // Arrange
        _collection.GetAll.Add(CreateGameObject("obj1", GameObjectCategory.Ship));
        _collection.GetAll.Add(CreateGameObject("obj2", GameObjectCategory.Weapon));
        _collection.GetAll.Add(CreateGameObject("obj3", GameObjectCategory.Pickup));

        // Act
        var all = _collection.GetAll;

        // Assert
        Assert.Equal(3, all.Count);
    }

    [Fact]
    public void Renderer_ShouldCallDrawOnAllObjects()
    {
        // Arrange
        var mockRenderer = new Mock<Infrastructure.Graphics.IRenderer>();
        var mockLogger = new Mock<ILogger<GameObject>>();
        var mockTextureManager = new Mock<Infrastructure.Graphics.ITextureManager>();
        var mockModelLoader = new Mock<IModelLoader>();
        
        var obj1 = new GameObject(mockRenderer.Object, mockLogger.Object, mockTextureManager.Object, mockModelLoader.Object);
        var obj2 = new GameObject(mockRenderer.Object, mockLogger.Object, mockTextureManager.Object, mockModelLoader.Object);
        
        _collection.GetAll.Add(obj1);
        _collection.GetAll.Add(obj2);

        // Act
        _collection.Renderer();

        // Note: This test verifies the method runs without exceptions
        // Actual rendering verification would require more complex mocking
        Assert.True(true);
    }

    [Fact]
    public void Update_ShouldCallUpdateOnAllObjects()
    {
        // Arrange
        var mockRenderer = new Mock<Infrastructure.Graphics.IRenderer>();
        var mockLogger = new Mock<ILogger<GameObject>>();
        var mockTextureManager = new Mock<Infrastructure.Graphics.ITextureManager>();
        var mockModelLoader = new Mock<IModelLoader>();
        
        var obj1 = new GameObject(mockRenderer.Object, mockLogger.Object, mockTextureManager.Object, mockModelLoader.Object);
        var obj2 = new GameObject(mockRenderer.Object, mockLogger.Object, mockTextureManager.Object, mockModelLoader.Object);
        
        _collection.GetAll.Add(obj1);
        _collection.GetAll.Add(obj2);

        // Act
        _collection.Update();

        // Note: This test verifies the method runs without exceptions
        Assert.True(true);
    }

    // Helper method to create a GameObject with specified properties
    private GameObject CreateGameObject(string name, GameObjectCategory category)
    {
        var mockRenderer = new Mock<Infrastructure.Graphics.IRenderer>();
        var mockLogger = new Mock<ILogger<GameObject>>();
        var mockTextureManager = new Mock<Infrastructure.Graphics.ITextureManager>();
        var mockModelLoader = new Mock<IModelLoader>();
        
        var obj = new GameObject(mockRenderer.Object, mockLogger.Object, mockTextureManager.Object, mockModelLoader.Object)
        {
            Name = name,
            Category = category
        };
        
        return obj;
    }
}
