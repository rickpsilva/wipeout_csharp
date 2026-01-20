using Microsoft.Extensions.Logging;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Core.Entities;

/// <summary>
/// Represents a 3D race track with sections, faces, and pickups.
/// </summary>
public class Track : ITrack
{
    public string BasePath { get; set; } = "";
    public string Name { get; set; } = "";
    public int FaceCount { get; set; }
    public int PickupCount { get; set; }
    public int SectionCount { get; set; }
    public int VertexCount { get; set; }
    public List<TrackFace> Faces { get; set; } = new();
    public List<TrackPickup> Pickups { get; set; } = new();
    public List<TrackSection> Sections { get; set; } = new();

    private readonly ILogger<Track> _logger;

    public Track(ILogger<Track> logger) => 
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public void LoadFromBinary(byte[] data)
    {
        try
        {
            _logger.LogInformation("Loading track data for {Name}... (not yet implemented)", Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading track {Name}", Name);
        }
    }

    public void Render(GLRenderer renderer)
    {
        if (Faces.Count == 0)
        {
            _logger.LogDebug("Track {Name} has no faces to render", Name);
            return;
        }

        _logger.LogDebug("Rendering track {Name} with {FaceCount} faces", Name, Faces.Count);
    }
}

public class TrackSection
{
    public Vec3 Center { get; set; }
    public int FaceCount { get; set; }
    public int FaceStart { get; set; }
    public int Flags { get; set; }
    public TrackSection? Junction { get; set; }
    public TrackSection? Next { get; set; }
    public TrackSection? Prev { get; set; }
    public int SectionNumber { get; set; }
}

public struct TrackFace
{
    public byte Flags { get; set; }
    public Vec3 Normal { get; set; }
    public byte TextureIndex { get; set; }
    public Triangle[] Triangles { get; set; }

    public TrackFace()
    {
        Triangles = new Triangle[2];
        Normal = new Vec3(0, 0, 0);
    }
}

public struct TrackPickup
{
    public float CooldownTimer { get; set; }
    public TrackFace? Face { get; set; }
}

public struct Triangle
{
    public Vertex[] Vertices { get; set; }

    public Triangle() => Vertices = new Vertex[3];
}

public struct Vertex
{
    public uint Color { get; set; }
    public Vec3 Position { get; set; }
    public Vec2 UV { get; set; }
}