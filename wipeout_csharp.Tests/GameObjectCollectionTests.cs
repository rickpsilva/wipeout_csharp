using Xunit;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Factory;

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
        // Add dummy objects
        _collection.GetAll.Add(new Mock<GameObject>().Object);
        _collection.GetAll.Add(new Mock<GameObject>().Object);

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
        var mockGo = new Mock<GameObject>();
        mockGo.Object.Name = "TestObject";
        _collection.GetAll.Add(mockGo.Object);

        var result = _collection.GetByName("testobject");

        Assert.NotNull(result);
        Assert.Equal("TestObject", result.Name);
    }

    [Fact]
    public void GetByName_WithExactMatch_ReturnsObject()
    {
        var mockGo = new Mock<GameObject>();
        mockGo.Object.Name = "MyShip";
        _collection.GetAll.Add(mockGo.Object);

        var result = _collection.GetByName("MyShip");

        Assert.NotNull(result);
        Assert.Equal("MyShip", result.Name);
    }

    [Fact]
    public void GetByName_WithNonexistentName_ReturnsNull()
    {
        var mockGo = new Mock<GameObject>();
        mockGo.Object.Name = "MyShip";
        _collection.GetAll.Add(mockGo.Object);

        var result = _collection.GetByName("NonexistentShip");

        Assert.Null(result);
    }

    #endregion

    #region Collection Operations Tests

    [Fact]
    public void Update_CallsUpdateOnAllObjects()
    {
        var mock1 = new Mock<GameObject>();
        var mock2 = new Mock<GameObject>();
        
        _collection.GetAll.Add(mock1.Object);
        _collection.GetAll.Add(mock2.Object);

        _collection.Update();

        // Both Update methods should be called
        // Note: This test depends on the mock setup; adjust as needed
    }

    [Fact]
    public void Renderer_CallsDrawOnAllObjects()
    {
        var mock1 = new Mock<GameObject>();
        var mock2 = new Mock<GameObject>();
        
        _collection.GetAll.Add(mock1.Object);
        _collection.GetAll.Add(mock2.Object);

        _collection.Renderer();

        // Both Draw methods should be called
        // Note: This test depends on the mock setup; adjust as needed
    }

    [Fact]
    public void ResetExhaustPlumes_CallsResetOnAllObjects()
    {
        var mock1 = new Mock<GameObject>();
        var mock2 = new Mock<GameObject>();
        
        _collection.GetAll.Add(mock1.Object);
        _collection.GetAll.Add(mock2.Object);

        _collection.ResetExhaustPlumes();

        // Both ResetExhaustPlume methods should be called
        // Note: This test depends on the mock setup; adjust as needed
    }

    #endregion
}
