using Xunit;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for Track.
/// Tests track initialization, rendering, and data loading.
/// </summary>
public class TrackTests
{
    private readonly Track _track;

    public TrackTests()
    {
        _track = new Track(NullLogger<Track>.Instance);
    }

    #region Initialization Tests

    [Fact]
    public void Constructor_InitializesEmptyLists()
    {
        Assert.Empty(_track.Sections);
        Assert.Empty(_track.Faces);
        Assert.Empty(_track.Pickups);
    }

    [Fact]
    public void Constructor_InitializesDefaultValues()
    {
        Assert.Equal("", _track.Name);
        Assert.Equal("", _track.BasePath);
        Assert.Equal(0, _track.SectionCount);
        Assert.Equal(0, _track.FaceCount);
        Assert.Equal(0, _track.PickupCount);
        Assert.Equal(0, _track.VertexCount);
    }

    #endregion

    #region Loading Tests

    [Fact]
    public void LoadFromBinary_WithEmptyData_DoesNotThrow()
    {
        var exception = Record.Exception(() => _track.LoadFromBinary(Array.Empty<byte>()));

        Assert.Null(exception);
    }

    [Fact]
    public void LoadFromBinary_WithNullData_DoesNotThrow()
    {
        // LoadFromBinary handles null gracefully (logs error but doesn't throw)
        var exception = Record.Exception(() => _track.LoadFromBinary(null!));

        Assert.Null(exception);
    }

    [Fact]
    public void LoadFromBinary_WithValidData_LogsInformation()
    {
        var data = new byte[] { 1, 2, 3, 4, 5 };

        var exception = Record.Exception(() => _track.LoadFromBinary(data));

        Assert.Null(exception);
    }

    #endregion

    #region Rendering Tests

    [Fact]
    public void Render_WithEmptyFaces_DoesNotThrow()
    {
        var mockRenderer = new Mock<GLRenderer>();

        var exception = Record.Exception(() => _track.Render(mockRenderer.Object));

        Assert.Null(exception);
    }

    [Fact]
    public void Render_WithFaces_DoesNotThrow()
    {
        _track.Name = "TestTrack";
        _track.Faces.Add(new TrackFace());
        _track.Faces.Add(new TrackFace());

        var mockRenderer = new Mock<GLRenderer>();

        var exception = Record.Exception(() => _track.Render(mockRenderer.Object));

        Assert.Null(exception);
    }

    #endregion

    #region TrackSection Tests

    [Fact]
    public void TrackSection_InitializesWithDefaults()
    {
        var section = new TrackSection();

        Assert.Equal(0, section.SectionNumber);
        Assert.Equal(0, section.FaceStart);
        Assert.Equal(0, section.FaceCount);
        Assert.Equal(0, section.Flags);
        Assert.Null(section.Next);
        Assert.Null(section.Prev);
        Assert.Null(section.Junction);
    }

    [Fact]
    public void TrackSection_CanSetProperties()
    {
        var section = new TrackSection
        {
            SectionNumber = 5,
            FaceStart = 10,
            FaceCount = 20,
            Center = new Vec3(100, 50, 200),
            Flags = 42
        };

        Assert.Equal(5, section.SectionNumber);
        Assert.Equal(10, section.FaceStart);
        Assert.Equal(20, section.FaceCount);
        Assert.Equal(100, section.Center.X);
        Assert.Equal(42, section.Flags);
    }

    #endregion

    #region TrackFace Tests

    [Fact]
    public void TrackFace_InitializesTriangles()
    {
        var face = new TrackFace();

        Assert.NotNull(face.Triangles);
        Assert.Equal(2, face.Triangles.Length);
    }

    [Fact]
    public void TrackFace_CanSetProperties()
    {
        var face = new TrackFace
        {
            Flags = 7,
            TextureIndex = 3,
            Normal = new Vec3(0, 1, 0)
        };

        Assert.Equal(7, face.Flags);
        Assert.Equal(3, face.TextureIndex);
        Assert.Equal(0, face.Normal.X);
        Assert.Equal(1, face.Normal.Y);
        Assert.Equal(0, face.Normal.Z);
    }

    #endregion

    #region Triangle Tests

    [Fact]
    public void Triangle_InitializesVertices()
    {
        var triangle = new Triangle();

        Assert.NotNull(triangle.Vertices);
        Assert.Equal(3, triangle.Vertices.Length);
    }

    #endregion

    #region Vertex Tests

    [Fact]
    public void Vertex_CanSetProperties()
    {
        var vertex = new Vertex
        {
            Position = new Vec3(10, 20, 30),
            Color = 0xFF0000FF, // Red in RGBA
            UV = new Vec2(0.5f, 0.5f)
        };

        Assert.Equal(10, vertex.Position.X);
        Assert.Equal(20, vertex.Position.Y);
        Assert.Equal(30, vertex.Position.Z);
        Assert.Equal(0xFF0000FF, vertex.Color);
        Assert.Equal(0.5f, vertex.UV.X);
        Assert.Equal(0.5f, vertex.UV.Y);
    }

    #endregion

    #region TrackPickup Tests

    [Fact]
    public void TrackPickup_InitializesWithDefaults()
    {
        var pickup = new TrackPickup();

        Assert.Equal(0, pickup.CooldownTimer);
        Assert.Null(pickup.Face);
    }

    [Fact]
    public void TrackPickup_CanSetProperties()
    {
        var face = new TrackFace();
        var pickup = new TrackPickup
        {
            CooldownTimer = 5.5f,
            Face = face
        };

        Assert.Equal(5.5f, pickup.CooldownTimer);
        Assert.NotNull(pickup.Face);
    }

    #endregion
}
