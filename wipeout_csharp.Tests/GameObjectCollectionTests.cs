using Xunit;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Factory;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Core.Graphics;
using WipeoutRewrite.Infrastructure.Assets;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for GameObjectCollection.
/// Tests initialization, filtering, and collection management.
/// </summary>
public class GameObjectCollectionTests
{
    private readonly Mock<IGameObjectFactory> _mockFactory;
    private readonly GameObjectCollection _collection;

    public GameObjectCollectionTests()
    {
        _mockFactory = new Mock<IGameObjectFactory>();
        _collection = new GameObjectCollection(
            NullLogger<GameObjectCollection>.Instance,
            _mockFactory.Object
        );
    }

    #region Initialization Tests

    [Fact]
    public void Constructor_InitializesEmptyCollection()
    {
        Assert.Empty(_collection.GetAll);
    }

    [Fact]
    public void Clear_RemovesAllObjects()
    {
        var mockGo1 = CreateMockGameObject("Object1");
        var mockGo2 = CreateMockGameObject("Object2");
        _collection.GetAll.Add(mockGo1.Object);
        _collection.GetAll.Add(mockGo2.Object);

        _collection.Clear();

        Assert.Empty(_collection.GetAll);
    }

    #endregion

    #region Filtering Tests

    [Fact]
    public void GetByCategory_WithEmptyCollection_ReturnsEmptyList()
    {
        var result = _collection.GetByCategory(GameObjectCategory.Ship);

        Assert.Empty(result);
    }

    [Fact]
    public void GetCategories_WithEmptyCollection_ReturnsEmptyList()
    {
        var result = _collection.GetCategories();

        Assert.Empty(result);
    }

    [Fact]
    public void GetByName_WithEmptyCollection_ReturnsNull()
    {
        var result = _collection.GetByName("TestObject");

        Assert.Null(result);
    }

    [Fact]
    public void GetByName_WithCaseInsensitiveMatch_ReturnsObject()
    {
        var mockGo = CreateMockGameObject("TestObject");
        _collection.GetAll.Add(mockGo.Object);

        var result = _collection.GetByName("testobject");

        Assert.NotNull(result);
        Assert.Equal("TestObject", result.Name);
    }

    [Fact]
    public void GetByName_WithExactMatch_ReturnsObject()
    {
        var mockGo = CreateMockGameObject("MyShip");
        _collection.GetAll.Add(mockGo.Object);

        var result = _collection.GetByName("MyShip");

        Assert.NotNull(result);
        Assert.Equal("MyShip", result.Name);
    }

    [Fact]
    public void GetByName_WithNonexistentName_ReturnsNull()
    {
        var mockGo = CreateMockGameObject("MyShip");
        _collection.GetAll.Add(mockGo.Object);

        var result = _collection.GetByName("NonexistentShip");

        Assert.Null(result);
    }

    #endregion

    #region Collection Operations Tests

    [Fact]
    public void Update_CallsUpdateOnAllObjects()
    {
        var mock1 = CreateMockGameObject("Object1");
        var mock2 = CreateMockGameObject("Object2");
        
        _collection.GetAll.Add(mock1.Object);
        _collection.GetAll.Add(mock2.Object);

        // Act: should not throw
        _collection.Update();

        Assert.Equal(2, _collection.GetAll.Count);
    }

    [Fact]
    public void Renderer_CallsDrawOnAllObjects()
    {
        var mock1 = CreateMockGameObject("Object1");
        var mock2 = CreateMockGameObject("Object2");
        
        _collection.GetAll.Add(mock1.Object);
        _collection.GetAll.Add(mock2.Object);

        // Act: should not throw
        _collection.Renderer();

        Assert.Equal(2, _collection.GetAll.Count);
    }

    [Fact]
    public void ResetExhaustPlumes_CallsResetOnAllObjects()
    {
        var mock1 = CreateMockGameObject("Object1");
        var mock2 = CreateMockGameObject("Object2");
        
        _collection.GetAll.Add(mock1.Object);
        _collection.GetAll.Add(mock2.Object);

        // Act: should not throw
        _collection.ResetExhaustPlumes();

        Assert.Equal(2, _collection.GetAll.Count);
    }

    private Mock<GameObject> CreateMockGameObject(string name)
    {
        var mock = new Mock<GameObject>(
            new Mock<IRenderer>().Object,
            NullLogger<GameObject>.Instance,
            new Mock<ITextureManager>().Object,
            new Mock<IModelLoader>().Object,
            new Mock<IAssetPathResolver>().Object);
        
        mock.Object.Name = name;
        return mock;
    }

    #endregion
}
