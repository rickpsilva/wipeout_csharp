using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Core.Graphics;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for GameObject.
/// Tests model loading, transformation, collisions, and rendering.
/// </summary>
public class GameObjectTests
{
    private readonly Mock<IRenderer> _mockRenderer;
    private readonly Mock<ITextureManager> _mockTextureManager;
    private readonly Mock<IModelLoader> _mockModelLoader;
    private readonly Mock<IAssetPathResolver> _mockAssetPathResolver;
    private readonly ILogger<GameObject> _logger;
    private readonly GameObject _gameObject;

    public GameObjectTests()
    {
        _mockRenderer = new Mock<IRenderer>();
        _mockTextureManager = new Mock<ITextureManager>();
        _mockModelLoader = new Mock<IModelLoader>();
        _mockAssetPathResolver = new Mock<IAssetPathResolver>();
        _logger = NullLogger<GameObject>.Instance;

        _gameObject = new GameObject(
            _mockRenderer.Object,
            _logger,
            _mockTextureManager.Object,
            _mockModelLoader.Object,
            _mockAssetPathResolver.Object
        );
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_InitializesPropertiesWithDefaults()
    {
        var go = new GameObject(
            _mockRenderer.Object,
            _logger,
            _mockTextureManager.Object,
            _mockModelLoader.Object,
            _mockAssetPathResolver.Object
        );

        Assert.Equal("Unnamed_GameObject", go.Name);
        Assert.Equal(GameObjectCategory.Unknown, go.Category);
        Assert.Equal(0, go.GameObjectId);
        Assert.False(go.IsVisible);
        Assert.Empty(go.Texture);
        Assert.Null(go.Model);
    }

    [Fact]
    public void Constructor_WithNullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GameObject(
            null!,
            _logger,
            _mockTextureManager.Object,
            _mockModelLoader.Object,
            _mockAssetPathResolver.Object
        ));

        Assert.Throws<ArgumentNullException>(() => new GameObject(
            _mockRenderer.Object,
            null!,
            _mockTextureManager.Object,
            _mockModelLoader.Object,
            _mockAssetPathResolver.Object
        ));
    }

    #endregion

    #region Position and Transformation Tests

    [Fact]
    public void CalculateTransformMatrix_WithDefaultValues_ReturnsValidMatrix()
    {
        _gameObject.Position = new Vec3(0, 0, 0);
        _gameObject.Angle = new Vec3(0, 0, 0);
        _gameObject.Scale = new Vec3(1, 1, 1);

        var matrix = _gameObject.CalculateTransformMatrix();

        Assert.NotNull(matrix.M);
    }

    [Fact]
    public void CalculateTransformMatrix_WithNonZeroPosition_IncludesTranslation()
    {
        _gameObject.Position = new Vec3(100, 200, 300);
        _gameObject.Angle = new Vec3(0, 0, 0);
        _gameObject.Scale = new Vec3(1, 1, 1);

        var matrix = _gameObject.CalculateTransformMatrix();

        // Matrix should contain translation
        Assert.NotNull(matrix.M);
    }

    [Fact]
    public void IsFlying_WithPositiveY_ReturnsTrue()
    {
        _gameObject.Position = new Vec3(0, 10, 0);

        Assert.True(_gameObject.IsFlying);
    }

    [Fact]
    public void IsFlying_WithZeroOrNegativeY_ReturnsFalse()
    {
        _gameObject.Position = new Vec3(0, 0, 0);
        Assert.False(_gameObject.IsFlying);

        _gameObject.Position = new Vec3(0, -10, 0);
        Assert.False(_gameObject.IsFlying);
    }

    #endregion

    #region Model Loading Tests

    [Fact]
    public void Load_WithNullPrmPath_LogsWarning()
    {
        _mockAssetPathResolver
            .Setup(x => x.ResolvePrmPath("allsh.prm"))
            .Returns((string?)null);

        _gameObject.Load(0);

        // Verify resolver was called
        _mockAssetPathResolver.Verify(
            x => x.ResolvePrmPath("allsh.prm"),
            Times.Once
        );
    }

    [Fact]
    public void Load_WithValidPrmPath_LoadsPrmAndCmp()
    {
        const string prmPath = "/test/path/allsh.prm";
        const string cmpPath = "/test/path/allsh.cmp";

        _mockAssetPathResolver
            .Setup(x => x.ResolvePrmPath("allsh.prm"))
            .Returns(prmPath);

        _mockAssetPathResolver
            .Setup(x => x.ResolveCmpPath(prmPath))
            .Returns(cmpPath);

        _mockAssetPathResolver
            .Setup(x => x.ResolveShadowTexturePath(prmPath, It.IsAny<int>()))
            .Returns((string?)null);

        _gameObject.Load(0);

        // Verify resolver methods were called
        _mockAssetPathResolver.Verify(x => x.ResolvePrmPath("allsh.prm"), Times.Once);
        _mockAssetPathResolver.Verify(x => x.ResolveCmpPath(prmPath), Times.Once);
    }

    [Fact]
    public void LoadModelFromPath_WithEmptyPath_LogsError()
    {
        _gameObject.LoadModelFromPath("");

        // Should log error but not crash
        Assert.Null(_gameObject.Model);
    }

    [Fact]
    public void LoadModelFromPath_WithNullPath_LogsError()
    {
        _gameObject.LoadModelFromPath(null!);

        // Should log error but not crash
        Assert.Null(_gameObject.Model);
    }

    [Fact]
    public void LoadModelFromPath_WithShipModel_LoadsShadowTexture()
    {
        const string prmPath = "/test/path/allsh.prm";

        _mockAssetPathResolver
            .Setup(x => x.ResolveCmpPath(prmPath))
            .Returns((string?)null);

        _mockAssetPathResolver
            .Setup(x => x.ResolveShadowTexturePath(prmPath, It.IsAny<int>()))
            .Returns((string?)null);

        _gameObject.LoadModelFromPath(prmPath, 0);

        // For ship objects (allsh.prm, index 0-7), should attempt shadow loading
        _mockAssetPathResolver.Verify(
            x => x.ResolveShadowTexturePath(prmPath, It.IsAny<int>()),
            Times.Once
        );
    }

    [Fact]
    public void LoadModelFromPath_WithNonShipModel_SkipsShadowLoading()
    {
        const string prmPath = "/test/path/other.prm";

        _mockAssetPathResolver
            .Setup(x => x.ResolveCmpPath(prmPath))
            .Returns((string?)null);

        _gameObject.LoadModelFromPath(prmPath, 0);

        // For non-ship objects, should NOT attempt shadow loading
        _mockAssetPathResolver.Verify(
            x => x.ResolveShadowTexturePath(It.IsAny<string>(), It.IsAny<int>()),
            Times.Never
        );
    }

    #endregion

    #region Bounding Box Tests

    [Fact]
    public void GetModelBounds_WithNullModel_ReturnsZeros()
    {
        _gameObject.SetModel(null!);

        var (minY, maxY) = _gameObject.GetModelBounds();

        Assert.Equal(0, minY);
        Assert.Equal(0, maxY);
    }

    [Fact]
    public void GetModelBounds_WithEmptyVertices_ReturnsZeros()
    {
        var mesh = new Mesh("Test");
        mesh.Vertices = new Vec3[0];

        var (minY, maxY) = _gameObject.GetModelBounds();

        Assert.Equal(0, minY);
        Assert.Equal(0, maxY);
    }

    [Fact]
    public void GetModelBounds_WithMultipleVertices_CalculatesCorrectly()
    {
        var mesh = new Mesh("Test");
        mesh.Vertices = new[]
        {
            new Vec3(0, 10, 0),
            new Vec3(0, 20, 0),
            new Vec3(0, 5, 0),
            new Vec3(0, 15, 0),
        };

        _gameObject.SetModel(mesh);

        var (minY, maxY) = _gameObject.GetModelBounds();

        Assert.Equal(5, minY);
        Assert.Equal(20, maxY);
    }

    #endregion

    #region Collision Tests

    [Fact]
    public void CollideWithShip_AveragesVelocities()
    {
        _gameObject.Velocity = new Vec3(10, 0, 0);
        var other = new GameObject(
            _mockRenderer.Object,
            _logger,
            _mockTextureManager.Object,
            _mockModelLoader.Object,
            _mockAssetPathResolver.Object
        )
        { Velocity = new Vec3(20, 0, 0) };

        _gameObject.CollideWithShip(other);

        // Both should have the same velocity (average)
        Assert.Equal(15, _gameObject.Velocity.X, 0.1f);
        Assert.Equal(15, other.Velocity.X, 0.1f);
    }

    [Fact]
    public void CollideWithTrack_ZerosYVelocity()
    {
        _gameObject.Velocity = new Vec3(10, 20, 30);
        var trackFace = new TrackFace();

        _gameObject.CollideWithTrack(trackFace);

        // Y velocity should be zeroed
        Assert.Equal(10, _gameObject.Velocity.X);
        Assert.Equal(0, _gameObject.Velocity.Y);
        Assert.Equal(30, _gameObject.Velocity.Z);
    }

    #endregion

    #region Position Helper Tests

    [Fact]
    public void GetCockpitPosition_ReturnsValidPosition()
    {
        _gameObject.Position = new Vec3(100, 50, 200);

        var cockpitPos = _gameObject.GetCockpitPosition();

        // Cockpit should be above the object
        Assert.True(cockpitPos.Y > _gameObject.Position.Y);
    }

    [Fact]
    public void GetNosePosition_ReturnsValidPosition()
    {
        _gameObject.Position = new Vec3(100, 50, 200);

        var nosePos = _gameObject.GetNosePosition();

        // Should have valid coordinates
        Assert.True(float.IsFinite(nosePos.X) && float.IsFinite(nosePos.Y) && float.IsFinite(nosePos.Z));
    }

    [Fact]
    public void GetWingLeftPosition_ReturnsValidPosition()
    {
        _gameObject.Position = new Vec3(100, 50, 200);

        var wingPos = _gameObject.GetWingLeftPosition();

        // Should have valid coordinates
        Assert.True(float.IsFinite(wingPos.X) && float.IsFinite(wingPos.Y) && float.IsFinite(wingPos.Z));
    }

    [Fact]
    public void GetWingRightPosition_ReturnsValidPosition()
    {
        _gameObject.Position = new Vec3(100, 50, 200);

        var wingPos = _gameObject.GetWingRightPosition();

        // Should have valid coordinates
        Assert.True(float.IsFinite(wingPos.X) && float.IsFinite(wingPos.Y) && float.IsFinite(wingPos.Z));
    }

    #endregion

    #region Stub Methods Tests

    [Fact]
    public void InitExhaustPlume_DoesNotThrow()
    {
        var exception = Record.Exception(() => _gameObject.InitExhaustPlume());

        Assert.Null(exception);
    }

    [Fact]
    public void ResetExhaustPlume_DoesNotThrow()
    {
        var exception = Record.Exception(() => _gameObject.ResetExhaustPlume());

        Assert.Null(exception);
    }

    #endregion
}
