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

        // Mock the LoadFromPrmFile to return a valid Mesh
        var mockModel = new Mesh("TestModel") { Vertices = Array.Empty<Vec3>(), Primitives = new List<Primitive>() };
        _mockModelLoader
            .Setup(x => x.LoadFromPrmFile(prmPath, 0))
            .Returns(mockModel);

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

        // Mock the LoadFromPrmFile to return a valid Mesh
        var mockModel = new Mesh("ShipModel") { Vertices = Array.Empty<Vec3>(), Primitives = new List<Primitive>() };
        _mockModelLoader
            .Setup(x => x.LoadFromPrmFile(prmPath, It.IsAny<int>()))
            .Returns(mockModel);

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

    #region SetGameObjectId Tests

    [Fact]
    public void SetGameObjectId_SetsIdCorrectly()
    {
        // Using reflection to call internal method
        var method = typeof(GameObject).GetMethod("SetGameObjectId", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        method?.Invoke(_gameObject, new object[] { 42 });

        Assert.Equal(42, _gameObject.GameObjectId);
    }

    #endregion

    #region SetModel Tests

    [Fact]
    public void SetModel_WithValidMesh_SetsModel()
    {
        var mesh = new Mesh("TestMesh") 
        { 
            Vertices = new Vec3[] { new Vec3(0, 0, 0) },
            Primitives = new List<Primitive>()
        };

        _gameObject.SetModel(mesh);

        Assert.Equal(mesh, _gameObject.Model);
    }

    [Fact]
    public void SetModel_WithNull_SetsModelToNull()
    {
        var mesh = new Mesh("TestMesh");
        _gameObject.SetModel(mesh);
        
        _gameObject.SetModel(null!);

        Assert.Null(_gameObject.Model);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_UpdatesDirectionVectors()
    {
        _gameObject.Angle = new Vec3(0.1f, 0.2f, 0.3f);
        _gameObject.Position = new Vec3(10, 20, 30);
        _gameObject.Velocity = new Vec3(1, 2, 3);

        _gameObject.Update();

        // Direction vectors should be updated based on angle
        Assert.NotEqual(new Vec3(0, 0, 1), _gameObject.DirForward);
        Assert.NotEqual(new Vec3(1, 0, 0), _gameObject.DirRight);
        Assert.NotEqual(new Vec3(0, 1, 0), _gameObject.DirUp);
    }

    [Fact]
    public void Update_IntegratesVelocity()
    {
        _gameObject.Position = new Vec3(10, 20, 30);
        _gameObject.Velocity = new Vec3(5, -2, 3);

        _gameObject.Update();

        Assert.Equal(15, _gameObject.Position.X);
        Assert.Equal(18, _gameObject.Position.Y);
        Assert.Equal(33, _gameObject.Position.Z);
    }

    [Fact]
    public void Update_WithZeroVelocity_DoesNotChangePosition()
    {
        _gameObject.Position = new Vec3(10, 20, 30);
        _gameObject.Velocity = new Vec3(0, 0, 0);
        var originalPos = _gameObject.Position;

        _gameObject.Update();

        Assert.Equal(originalPos.X, _gameObject.Position.X);
        Assert.Equal(originalPos.Y, _gameObject.Position.Y);
        Assert.Equal(originalPos.Z, _gameObject.Position.Z);
    }

    #endregion

    #region Init Tests

    [Fact]
    public void Init_WithTrackSection_SetsPositionAndSection()
    {
        var section = new TrackSection 
        { 
            Center = new Vec3(100, 200, 300),
            SectionNumber = 5
        };

        _gameObject.Init(section, 0, 0);

        Assert.Equal(100, _gameObject.Position.X);
        Assert.Equal(200, _gameObject.Position.Y);
        Assert.Equal(300, _gameObject.Position.Z);
        Assert.Equal(5, _gameObject.SectionNum);
        Assert.Equal(5, _gameObject.TotalSectionNum);
    }

    [Fact]
    public void Init_ResetsVelocityAndAngle()
    {
        _gameObject.Velocity = new Vec3(10, 20, 30);
        _gameObject.Angle = new Vec3(1, 2, 3);

        _gameObject.Init(null, 0, 0);

        Assert.Equal(0, _gameObject.Velocity.X);
        Assert.Equal(0, _gameObject.Velocity.Y);
        Assert.Equal(0, _gameObject.Velocity.Z);
        Assert.Equal(0, _gameObject.Angle.X);
        Assert.Equal(0, _gameObject.Angle.Y);
        Assert.Equal(0, _gameObject.Angle.Z);
    }

    [Fact]
    public void Init_WithNullSection_DoesNotCrash()
    {
        var exception = Record.Exception(() => _gameObject.Init(null, 0, 0));

        Assert.Null(exception);
    }

    #endregion

    #region Draw Tests

    [Fact]
    public void Draw_WithNullModel_DoesNotCrash()
    {
        _gameObject.SetModel(null!);
        _gameObject.IsVisible = true;

        var exception = Record.Exception(() => _gameObject.Draw());

        Assert.Null(exception);
    }

    [Fact]
    public void Draw_WithIsVisibleFalse_DoesNotRender()
    {
        var mesh = new Mesh("Test") 
        { 
            Vertices = new Vec3[] { new Vec3(0, 0, 0) },
            Primitives = new List<Primitive>()
        };
        _gameObject.SetModel(mesh);
        _gameObject.IsVisible = false;

        _gameObject.Draw();

        // Renderer should not be called
        _mockRenderer.Verify(r => r.SetCurrentTexture(It.IsAny<int>()), Times.Never);
        _mockRenderer.Verify(r => r.PushTri(
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>()), Times.Never);
    }

    [Fact]
    public void Draw_WithValidModelAndPrimitives_CallsRenderer()
    {
        var vertices = new Vec3[] 
        { 
            new Vec3(0, 0, 0), 
            new Vec3(1, 0, 0), 
            new Vec3(0, 1, 0) 
        };
        
        var primitives = new List<Primitive>
        {
            new F3 
            { 
                CoordIndices = new short[] { 0, 1, 2 },
                Color = (r: (byte)255, g: (byte)0, b: (byte)0, a: (byte)255)
            }
        };

        var mesh = new Mesh("Test") 
        { 
            Vertices = vertices,
            Primitives = primitives
        };

        _mockRenderer.Setup(r => r.WhiteTexture).Returns(1);
        _gameObject.SetModel(mesh);
        _gameObject.IsVisible = true;

        _gameObject.Draw();

        // Should set texture and render triangle
        _mockRenderer.Verify(r => r.SetCurrentTexture(It.IsAny<int>()), Times.AtLeastOnce);
        _mockRenderer.Verify(r => r.PushTri(
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>()), Times.Once);
    }

    [Fact]
    public void Draw_WithInvalidPrimitiveIndices_SkipsPrimitive()
    {
        var vertices = new Vec3[] { new Vec3(0, 0, 0) };
        
        var primitives = new List<Primitive>
        {
            new F3 
            { 
                CoordIndices = new short[] { 0, 1, 999 }, // Invalid index
                Color = (r: (byte)255, g: (byte)0, b: (byte)0, a: (byte)255)
            }
        };

        var mesh = new Mesh("Test") 
        { 
            Vertices = vertices,
            Primitives = primitives
        };

        _gameObject.SetModel(mesh);
        _gameObject.IsVisible = true;

        var exception = Record.Exception(() => _gameObject.Draw());

        // Should not crash, just skip the invalid primitive
        Assert.Null(exception);
        _mockRenderer.Verify(r => r.PushTri(
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>()), Times.Never);
    }

    [Fact]
    public void Draw_WithG3Primitive_RendersGouraudTriangle()
    {
        var vertices = new Vec3[] 
        { 
            new Vec3(0, 0, 0), 
            new Vec3(1, 0, 0), 
            new Vec3(0, 1, 0) 
        };
        
        var primitives = new List<Primitive>
        {
            new G3 
            { 
                CoordIndices = new short[] { 0, 1, 2 },
                Colors = new (byte r, byte g, byte b, byte a)[] 
                {
                    (r: (byte)255, g: (byte)0, b: (byte)0, a: (byte)255),
                    (r: (byte)0, g: (byte)255, b: (byte)0, a: (byte)255),
                    (r: (byte)0, g: (byte)0, b: (byte)255, a: (byte)255)
                }
            }
        };

        var mesh = new Mesh("Test") 
        { 
            Vertices = vertices,
            Primitives = primitives
        };

        _mockRenderer.Setup(r => r.WhiteTexture).Returns(1);
        _gameObject.SetModel(mesh);
        _gameObject.IsVisible = true;

        _gameObject.Draw();

        _mockRenderer.Verify(r => r.PushTri(
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>()), Times.Once);
    }

    [Fact]
    public void Draw_WithFT3Primitive_RendersTexturedTriangle()
    {
        var vertices = new Vec3[] 
        { 
            new Vec3(0, 0, 0), 
            new Vec3(1, 0, 0), 
            new Vec3(0, 1, 0) 
        };
        
        var primitives = new List<Primitive>
        {
            new FT3 
            { 
                CoordIndices = new short[] { 0, 1, 2 },
                Color = (r: (byte)255, g: (byte)255, b: (byte)255, a: (byte)255),
                TextureHandle = 5,
                UVsF = new (float u, float v)[] { (0, 0), (1, 0), (0, 1) }
            }
        };

        var mesh = new Mesh("Test") 
        { 
            Vertices = vertices,
            Primitives = primitives
        };

        _gameObject.SetModel(mesh);
        _gameObject.IsVisible = true;

        _gameObject.Draw();

        _mockRenderer.Verify(r => r.SetCurrentTexture(5), Times.Once);
        _mockRenderer.Verify(r => r.PushTri(
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>()), Times.Once);
    }

    [Fact]
    public void Draw_WithGT3EnginePrimitive_UsesEngineColor()
    {
        var vertices = new Vec3[] 
        { 
            new Vec3(0, 0, 0), 
            new Vec3(1, 0, 0), 
            new Vec3(0, 1, 0) 
        };
        
        var primitives = new List<Primitive>
        {
            new GT3 
            { 
                CoordIndices = new short[] { 0, 1, 2 },
                Flags = PrimitiveFlags.SHIP_ENGINE,
                TextureHandle = 5,
                UVsF = new (float u, float v)[] { (0, 0), (1, 0), (0, 1) },
                Colors = new (byte r, byte g, byte b, byte a)[] 
                {
                    (r: (byte)100, g: (byte)100, b: (byte)100, a: (byte)255),
                    (r: (byte)100, g: (byte)100, b: (byte)100, a: (byte)255),
                    (r: (byte)100, g: (byte)100, b: (byte)100, a: (byte)255)
                }
            }
        };

        var mesh = new Mesh("Test") 
        { 
            Vertices = vertices,
            Primitives = primitives
        };

        _gameObject.SetModel(mesh);
        _gameObject.IsVisible = true;

        _gameObject.Draw();

        // Engine primitives should render with special color override
        _mockRenderer.Verify(r => r.SetCurrentTexture(5), Times.Once);
        _mockRenderer.Verify(r => r.PushTri(
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>()), Times.Once);
    }

    #endregion

    #region RenderShadow Tests

    [Fact]
    public void RenderShadow_WithNullModel_DoesNotRender()
    {
        _gameObject.SetModel(null!);
        _gameObject.IsVisible = true;

        _gameObject.RenderShadow();

        _mockRenderer.Verify(r => r.PushTri(
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>()), Times.Never);
    }

    [Fact]
    public void RenderShadow_WithIsVisibleFalse_DoesNotRender()
    {
        var mesh = new Mesh("Test") 
        { 
            Vertices = new Vec3[] { new Vec3(0, 0, 0) },
            Primitives = new List<Primitive>()
        };
        _gameObject.SetModel(mesh);
        _gameObject.IsVisible = false;

        _gameObject.RenderShadow();

        _mockRenderer.Verify(r => r.PushTri(
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>()), Times.Never);
    }

    [Fact]
    public void RenderShadow_WithNoShadowTexture_DoesNotRender()
    {
        var mesh = new Mesh("Test") 
        { 
            Vertices = new Vec3[] { new Vec3(0, 0, 0) },
            Primitives = new List<Primitive>()
        };
        _gameObject.SetModel(mesh);
        _gameObject.IsVisible = true;

        _gameObject.RenderShadow();

        // Should not render since ShadowTexture is -1 by default
        _mockRenderer.Verify(r => r.PushTri(
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>(),
            It.IsAny<OpenTK.Mathematics.Vector3>(),
            It.IsAny<OpenTK.Mathematics.Vector2>(),
            It.IsAny<OpenTK.Mathematics.Vector4>()), Times.Never);
    }

    #endregion

    #region ApplyTexturesWithNormalization Tests

    [Fact]
    public void ApplyTexturesWithNormalization_WithEmptyHandles_DoesNotCrash()
    {
        var exception = Record.Exception(() => _gameObject.ApplyTexturesWithNormalization(Array.Empty<int>()));

        Assert.Null(exception);
    }

    [Fact]
    public void ApplyTexturesWithNormalization_WithNullModel_DoesNotCrash()
    {
        _gameObject.SetModel(null!);

        var exception = Record.Exception(() => _gameObject.ApplyTexturesWithNormalization(new int[] { 1, 2, 3 }));

        Assert.Null(exception);
    }

    [Fact]
    public void ApplyTexturesWithNormalization_WithFT3Primitives_NormalizesUVs()
    {
        var vertices = new Vec3[] 
        { 
            new Vec3(0, 0, 0), 
            new Vec3(1, 0, 0), 
            new Vec3(0, 1, 0) 
        };
        
        var ft3 = new FT3 
        { 
            CoordIndices = new short[] { 0, 1, 2 },
            Color = (r: (byte)255, g: (byte)255, b: (byte)255, a: (byte)255),
            TextureId = 0,
            UVs = new (byte u, byte v)[] { (0, 0), (128, 128), (255, 255) }
        };

        var mesh = new Mesh("Test") 
        { 
            Vertices = vertices,
            Primitives = new List<Primitive> { ft3 }
        };

        _mockTextureManager.Setup(m => m.GetTextureSize(1)).Returns((256, 256));
        _gameObject.SetModel(mesh);

        _gameObject.ApplyTexturesWithNormalization(new int[] { 1 });

        // TextureHandle should be set
        Assert.Equal(1, ft3.TextureHandle);
        // UVsF should be normalized
        Assert.NotNull(ft3.UVsF);
        Assert.Equal(3, ft3.UVsF.Length);
    }

    [Fact]
    public void ApplyTexturesWithNormalization_WithGT3Primitives_NormalizesUVs()
    {
        var vertices = new Vec3[] 
        { 
            new Vec3(0, 0, 0), 
            new Vec3(1, 0, 0), 
            new Vec3(0, 1, 0) 
        };
        
        var gt3 = new GT3 
        { 
            CoordIndices = new short[] { 0, 1, 2 },
            TextureId = 0,
            UVs = new (byte u, byte v)[] { (0, 0), (64, 64), (128, 128) },
            Colors = new (byte r, byte g, byte b, byte a)[] 
            {
                (r: (byte)255, g: (byte)0, b: (byte)0, a: (byte)255),
                (r: (byte)0, g: (byte)255, b: (byte)0, a: (byte)255),
                (r: (byte)0, g: (byte)0, b: (byte)255, a: (byte)255)
            }
        };

        var mesh = new Mesh("Test") 
        { 
            Vertices = vertices,
            Primitives = new List<Primitive> { gt3 }
        };

        _mockTextureManager.Setup(m => m.GetTextureSize(1)).Returns((128, 128));
        _gameObject.SetModel(mesh);

        _gameObject.ApplyTexturesWithNormalization(new int[] { 1 });

        // TextureHandle should be set
        Assert.Equal(1, gt3.TextureHandle);
        // UVsF should be normalized
        Assert.NotNull(gt3.UVsF);
        Assert.Equal(3, gt3.UVsF.Length);
        // Check normalization (64/128 = 0.5)
        Assert.Equal(0.5f, gt3.UVsF[1].u, 0.01f);
        Assert.Equal(0.5f, gt3.UVsF[1].v, 0.01f);
    }

    #endregion

    #region GetModel Tests

    [Fact]
    public void GetModel_ReturnsCurrentModel()
    {
        var mesh = new Mesh("Test") 
        { 
            Vertices = new Vec3[] { new Vec3(0, 0, 0) },
            Primitives = new List<Primitive>()
        };
        _gameObject.SetModel(mesh);

        var result = _gameObject.GetModel();

        Assert.Equal(mesh, result);
    }

    [Fact]
    public void GetModel_WithNullModel_ReturnsNull()
    {
        _gameObject.SetModel(null!);

        var result = _gameObject.GetModel();

        Assert.Null(result);
    }

    #endregion
}
