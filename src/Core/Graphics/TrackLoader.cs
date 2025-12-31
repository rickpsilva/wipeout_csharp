using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Core.Graphics;

/// <summary>
/// Loads and manages track geometry from track.trv (vertices) and track.trf (faces).
/// Each face is a quad (4 vertices) rendered as 2 triangles.
/// </summary>
public class TrackLoader
{
    private readonly ILogger<TrackLoader> _logger;
    private List<TrackVertex>? _loadedVertices;
    private List<TrackFace>? _loadedFaces;

    public TrackLoader(ILogger<TrackLoader> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get the loaded vertices (for diagnostic/inspection purposes).
    /// </summary>
    public List<TrackVertex>? LoadedVertices => _loadedVertices;

    /// <summary>
    /// Get the loaded faces (for animation setup).
    /// </summary>
    public List<TrackFace>? LoadedFaces => _loadedFaces;

    /// <summary>
    /// Track vertex data
    /// </summary>
    public class TrackVertex
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    /// <summary>
    /// Track face (quad) data - consists of 4 vertices, normal, texture, flags, and color
    /// </summary>
    public class TrackFace
    {
        public short[] VertexIndices { get; set; } = new short[4]; // Indices into vertex array
        public float NormalX { get; set; }
        public float NormalY { get; set; }
        public float NormalZ { get; set; }
        public byte TextureId { get; set; }
        public byte Flags { get; set; }
        public (byte R, byte G, byte B, byte A) Color { get; set; }
    }

    /// <summary>
    /// Load all vertices from track.trv file.
    /// Each vertex is stored as 3 i32 values (X, Y, Z) + 4 bytes padding = 16 bytes per vertex
    /// </summary>
    public void LoadVertices(string trvFilePath)
    {
        if (!File.Exists(trvFilePath))
            throw new FileNotFoundException($"track.trv not found: {trvFilePath}");

        byte[] bytes = File.ReadAllBytes(trvFilePath);
        _logger.LogInformation("[TRACK] Loaded track.trv: {Size} bytes", bytes.Length);

        _loadedVertices = new List<TrackVertex>();
        int p = 0;

        // Each vertex is 16 bytes: 4 i32 values (X, Y, Z, padding)
        while (p + 16 <= bytes.Length) // Need exactly 16 bytes
        {
            int x = ReadI32(bytes, ref p);
            int y = ReadI32(bytes, ref p);
            int z = ReadI32(bytes, ref p);
            p += 4; // Skip padding

            _loadedVertices.Add(new TrackVertex { X = x, Y = y, Z = z });
        }

        _logger.LogInformation("[TRACK] Loaded {Count} vertices from track.trv", _loadedVertices.Count);
    }

    /// <summary>
    /// Load all faces from track.trf file.
    /// Each face is 20 bytes:
    ///   4x i16 (vertex indices) = 8 bytes
    ///   3x i16 (normal.x, normal.y, normal.z) = 6 bytes
    ///   1x i8  (texture ID) = 1 byte
    ///   1x i8  (flags) = 1 byte
    ///   1x u32 (color RGBA) = 4 bytes
    /// Total = 20 bytes per face
    /// </summary>
    public void LoadFaces(string trfFilePath)
    {
        if (!File.Exists(trfFilePath))
            throw new FileNotFoundException($"track.trf not found: {trfFilePath}");

        byte[] bytes = File.ReadAllBytes(trfFilePath);
        _logger.LogInformation("[TRACK] Loaded track.trf: {Size} bytes", bytes.Length);

        _loadedFaces = new List<TrackFace>();
        int p = 0;

        // Each face is 20 bytes
        const int FACE_SIZE = 20;
        while (p + FACE_SIZE <= bytes.Length)
        {
            var face = new TrackFace();

            // Read 4 vertex indices (BIG-ENDIAN)
            face.VertexIndices[0] = ReadI16(bytes, ref p);
            face.VertexIndices[1] = ReadI16(bytes, ref p);
            face.VertexIndices[2] = ReadI16(bytes, ref p);
            face.VertexIndices[3] = ReadI16(bytes, ref p);

            // Read normal (3x i16 BE, divided by 4096.0 to get normalized float)
            face.NormalX = ReadI16(bytes, ref p) / 4096.0f;
            face.NormalY = ReadI16(bytes, ref p) / 4096.0f;
            face.NormalZ = ReadI16(bytes, ref p) / 4096.0f;

            // Read texture ID and flags
            face.TextureId = bytes[p++];
            face.Flags = bytes[p++];

            // Read color (u32 BE RGBA)
            uint colorRaw = ReadU32(bytes, ref p);
            face.Color = RgbaFromU32(colorRaw);

            _loadedFaces.Add(face);
        }

        _logger.LogInformation("[TRACK] Loaded {Count} faces from track.trf", _loadedFaces.Count);
    }

    /// <summary>
    /// Convert loaded track geometry to a mesh that can be rendered.
    /// Must call LoadVertices() and LoadFaces() first.
    /// </summary>
    public Mesh ConvertToMesh()
    {
        if (_loadedVertices == null || _loadedFaces == null)
            throw new InvalidOperationException("Must call LoadVertices() and LoadFaces() first");

        return ConvertToMesh(_loadedVertices, _loadedFaces);
    }

    /// <summary>
    /// Convert track geometry to a mesh that can be rendered.
    /// Faces are quads, so each quad is split into 2 triangles.
    /// </summary>
    private Mesh ConvertToMesh(List<TrackVertex> vertices, List<TrackFace> faces)
    {
        var mesh = new Mesh("Track")
        {
            // Convert track vertices to Vec3
            Vertices = new Vec3[vertices.Count],
            Normals = new Vec3[vertices.Count]
        };

        // Accumulate face normals per vertex to approximate lighting
        var normalAccum = new Vec3[vertices.Count];
        var normalCounts = new int[vertices.Count];

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        for (int i = 0; i < vertices.Count; i++)
        {
            mesh.Vertices[i] = new Vec3(vertices[i].X, vertices[i].Y, vertices[i].Z);
            
            minX = Math.Min(minX, vertices[i].X);
            maxX = Math.Max(maxX, vertices[i].X);
            minY = Math.Min(minY, vertices[i].Y);
            maxY = Math.Max(maxY, vertices[i].Y);
            minZ = Math.Min(minZ, vertices[i].Z);
            maxZ = Math.Max(maxZ, vertices[i].Z);
        }

        _logger.LogInformation("[TRACK] Mesh bounds: X[{MinX}, {MaxX}] Y[{MinY}, {MaxY}] Z[{MinZ}, {MaxZ}]",
            minX, maxX, minY, maxY, minZ, maxZ);

        // Convert faces to primitives (quads -> 2 textured triangles)
        mesh.Primitives = new List<Primitive>();

        // UV layout matches track_uv in wipeout-rewrite/src/wipeout/track.c
        (byte u, byte v)[] uvStandard = new (byte, byte)[]
        {
            (128, 0),   // 0
            (0,   0),   // 1
            (0, 128),   // 2
            (128,128),  // 3
        };
        (byte u, byte v)[] uvFlipped = new (byte, byte)[]
        {
            (0,   0),   // 0
            (128, 0),   // 1
            (128,128),  // 2
            (0, 128),   // 3
        };

        const byte FACE_FLIP_TEXTURE = 1 << 2;

        foreach (var face in faces)
        {
            // Validate indices
            if (face.VertexIndices.Any(idx => idx < 0 || idx >= vertices.Count))
            {
                _logger.LogWarning("[TRACK] Face has invalid vertex index, skipping");
                continue;
            }

            // Accumulate normals per vertex for simple lighting
            for (int i = 0; i < 4; i++)
            {
                int vi = face.VertexIndices[i];
                normalAccum[vi] = new Vec3(
                    normalAccum[vi].X + face.NormalX,
                    normalAccum[vi].Y + face.NormalY,
                    normalAccum[vi].Z + face.NormalZ);
                normalCounts[vi]++;
            }

            var uv = (face.Flags & FACE_FLIP_TEXTURE) != 0 ? uvFlipped : uvStandard;

            // Tri 0: (v2, v1, v0) - reversed for correct winding order
            // wipeout-rewrite reverses vertex order during rendering
            var tri0 = new FT3
            {
                CoordIndices = new short[3]
                {
                    face.VertexIndices[2],
                    face.VertexIndices[1],
                    face.VertexIndices[0]
                },
                TextureId = face.TextureId,
                UVs = new (byte u, byte v)[] { uv[2], uv[1], uv[0] },
                Color = (face.Color.R, face.Color.G, face.Color.B, face.Color.A)
            };
            
            // Tri 1: (v2, v0, v3) - reversed for correct winding order
            // wipeout-rewrite reverses vertex order during rendering
            var tri1 = new FT3
            {
                CoordIndices = new short[3]
                {
                    face.VertexIndices[2],
                    face.VertexIndices[0],
                    face.VertexIndices[3]
                },
                TextureId = face.TextureId,
                UVs = new (byte u, byte v)[] { uv[2], uv[0], uv[3] },
                Color = (face.Color.R, face.Color.G, face.Color.B, face.Color.A)
            };

            mesh.Primitives.Add(tri0);
            mesh.Primitives.Add(tri1);
        }

        // Normalize accumulated normals
        for (int i = 0; i < mesh.Normals.Length; i++)
        {
            if (normalCounts[i] == 0)
            {
                mesh.Normals[i] = new Vec3(0, 1, 0);
                continue;
            }

            var n = normalAccum[i];
            float len = MathF.Sqrt(n.X * n.X + n.Y * n.Y + n.Z * n.Z);
            if (len > 0.0001f)
            {
                mesh.Normals[i] = new Vec3(n.X / len, n.Y / len, n.Z / len);
            }
            else
            {
                mesh.Normals[i] = new Vec3(0, 1, 0);
            }
        }

        _logger.LogInformation("[TRACK] Created mesh with {VertCount} vertices and {PrimCount} primitives",
            mesh.Vertices.Length, mesh.Primitives.Count);

        return mesh;
    }

    // Binary reading helpers
    private static short ReadI16(byte[] bytes, ref int p)
    {
        short value = (short)((bytes[p] << 8) | bytes[p + 1]);
        p += 2;
        return value;
    }

    private static int ReadI32(byte[] bytes, ref int p)
    {
        int value = (bytes[p] << 24) | (bytes[p + 1] << 16) | (bytes[p + 2] << 8) | bytes[p + 3];
        p += 4;
        return value;
    }

    private static uint ReadU32(byte[] bytes, ref int p)
    {
        return (uint)ReadI32(bytes, ref p);
    }

    private static (byte r, byte g, byte b, byte a) RgbaFromU32(uint color)
    {
        // Extract RGBA from u32: R=bits[24-31], G=bits[16-23], B=bits[8-15], A=always 255
        // This matches wipeout-rewrite's rgba_from_u32() function
        return (
            (byte)((color >> 24) & 0xFF),  // Red
            (byte)((color >> 16) & 0xFF),  // Green
            (byte)((color >> 8) & 0xFF),   // Blue
            255                             // Alpha (always fully opaque)
        );
    }
}
