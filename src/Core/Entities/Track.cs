using Microsoft.Extensions.Logging;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Core.Entities;

/// <summary>
/// Represents a 3D race track with sections, faces, and pickups.
/// </summary>
public class Track : ITrack
{
    #region properties

    // Metadados
    public string BasePath { get; set; } = "";

    public int FaceCount { get; set; }
    public List<TrackFace> Faces { get; set; } = new();
    public string Name { get; set; } = "";
    public int PickupCount { get; set; }
    public List<TrackPickup> Pickups { get; set; } = new();
    public int SectionCount { get; set; }
    public List<TrackSection> Sections { get; set; } = new();
    public int VertexCount { get; set; }
    #endregion

    private readonly ILogger<Track> _logger;

    public Track(ILogger<Track> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LoadFromBinary(byte[] data)
    {
        try
        {
            // Simple binary format parser
            // TODO: implementar parsing completo
            _logger.LogInformation($"Loading track data for {Name}... (stub)");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading track {Name}: {ex.Message}");
        }
    }

    public void Render(GLRenderer renderer)
    {
        // TODO: implement face rendering
        _logger.LogInformation($"Rendering track {Name}... (stub)");
    }
}

// Track section
public class TrackSection
{
    #region properties
    public Vec3 Center { get; set; }
    public int FaceCount { get; set; }
    public int FaceStart { get; set; }
    public int Flags { get; set; }
    public TrackSection? Junction { get; set; }
    public TrackSection? Next { get; set; }
    public TrackSection? Prev { get; set; }
    public int SectionNumber { get; set; }
    #endregion 
}

// Track face (can have 1 or 2 triangles)
public struct TrackFace
{
    public byte Flags { get; set; }

    // up to 2 triangles
    public Vec3 Normal { get; set; }

    public byte TextureIndex { get; set; }
    public Triangle[] Triangles { get; set; }

    public TrackFace()
    {
        Triangles = new Triangle[2];
        Normal = new Vec3(0, 0, 0);
        Flags = 0;
        TextureIndex = 0;
    }
}

// Track pickup
public struct TrackPickup
{
    public float CooldownTimer { get; set; }
    public TrackFace? Face { get; set; }
}

// Triangle structure
public struct Triangle
{
    public Vertex[] Vertices { get; set; }

    // 3 vertices

    public Triangle()
    {
        Vertices = new Vertex[3];
    }
}

// Vertex structure (C-compatible)
public struct Vertex
{
    public uint Color { get; set; }
    // RGBA

    public Vec3 Position { get; set; }
    public Vec2 UV { get; set; }
}