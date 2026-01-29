using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Core.Graphics;

/// <summary>
/// Simple and clean PRM model loader, directly ported from wipeout-rewrite/src/wipeout/object.c
/// This replaces the experimental ModelLoader with a proven, working implementation.
/// </summary>
public class ModelLoader : IModelLoader
{
    private readonly ILogger<ModelLoader> _logger;
    private HashSet<short>? _primitiveTypesSeen;

    public ModelLoader(ILogger<ModelLoader> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region methods

    /// <summary>
    /// Scans a PRM file and returns a list of object names and their indices.
    /// This is useful for displaying available models in a PRM file without loading them all.
    /// </summary>
    public List<(int index, string name)> GetObjectsInPrmFile(string filepath)
    {
        var objects = new List<(int, string)>();

        try
        {
            if (!File.Exists(filepath))
            {
                _logger.LogWarning("PRM file not found for scanning: {Path}", filepath);
                return objects;
            }

            byte[] bytes = File.ReadAllBytes(filepath);
            int p = 0;
            int objectIndex = 0;

            while (TryReadObjectHeader(bytes, ref p, out var header))
            {
                if (IsInvalidCount(header))
                {
                    _logger.LogWarning("Invalid object counts at index {Index}, stopping scan", objectIndex);
                    break;
                }

                if (header.VerticesLen > 0)
                {
                    objects.Add((objectIndex, header.Name));
                    _logger.LogInformation("Found object {Index}: '{Name}' ({VertCount} vertices, {PrimCount} primitives)",
                        objectIndex, header.Name, header.VerticesLen, header.PrimitivesLen);
                }

                SkipObjectData(bytes, ref p, header);
                objectIndex++;
            }

            _logger.LogInformation("Scan complete: {Count} objects found in {Path}", objects.Count, filepath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning PRM file: {Path}", filepath);
        }

        return objects;
    }

    /// <summary>
    /// Load ALL objects from a PRM file and return them as a list.
    /// This matches the behavior of objects_load() in object.c which returns a linked list of ALL objects.
    /// Useful for scene.prm and sky.prm files that contain multiple models.
    /// </summary>
    public List<Mesh> LoadAllObjectsFromPrmFile(string filepath)
    {
        try
        {
            var bytes = ReadPrmBytes(filepath);
            var meshes = new List<Mesh>();
            foreach (var mesh in EnumerateMeshes(bytes, null))
            {
                meshes.Add(mesh);
            }
            _logger.LogInformation("Loaded {Count} total objects from {Path}", meshes.Count, filepath);
            return meshes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading all objects from PRM file: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Load a PRM file and return a Mesh.
    /// Directly ported from oo
    /// <summary>
    /// Creates the original simple mock model (kept for compatibility)
    /// </summary>
    /// <summary>
    /// Load a PRM file and return a Mesh.
    /// Directly ported from objects_load() in object.c
    /// </summary>
    public Mesh LoadFromPrmFile(string filepath, int objectIndex = 0)
    {
        try
        {
            var bytes = ReadPrmBytes(filepath);
            var mesh = EnumerateMeshes(bytes, objectIndex).FirstOrDefault();
            return mesh ?? throw new InvalidDataException("No valid mesh found in PRM file");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading PRM file: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Converts byte UV coordinates (0-255) to normalized float UV coordinates (0.0-1.0).
    /// PlayStation 1 PRM files store texture coordinates as bytes, but OpenGL expects floats in the 0-1 range.
    /// </summary>
    /// <typeparam name="T">A textured primitive type that implements <see cref="ITexturedPrimitive"/></typeparam>
    /// <param name="primitive">The primitive whose UVsF property will be populated</param>
    /// <param name="uvs">Array of byte UV coordinates read from the PRM file</param>
    private static void ConvertUVsToFloat<T>(T primitive, (byte u, byte v)[] uvs) where T : ITexturedPrimitive
    {
        for (int i = 0; i < uvs.Length; i++)
            primitive.UVsF[i] = (uvs[i].u / 255f, uvs[i].v / 255f);
    }

    private IEnumerable<Mesh> EnumerateMeshes(byte[] bytes, int? targetIndex)
    {
        int p = 0;
        int currentIndex = 0;

        while (TryReadObjectHeader(bytes, ref p, out var header))
        {
            if (IsInvalidCount(header))
            {
                _logger.LogWarning("Invalid counts for object {Index}, stopping parse", currentIndex);
                yield break;
            }

            bool isTarget = !targetIndex.HasValue || currentIndex == targetIndex.Value;

            if (header.VerticesLen == 0)
            {
                SkipObjectData(bytes, ref p, header);
                currentIndex++;
                continue;
            }

            if (!isTarget)
            {
                SkipObjectData(bytes, ref p, header);
                currentIndex++;
                continue;
            }

            var mesh = ReadMesh(bytes, ref p, header);
            _logger.LogInformation("Loaded object '{Name}' (index {Index}): {VertCount} vertices, {PrimCount} primitives",
                mesh.Name, currentIndex, mesh.Vertices.Length, mesh.Primitives.Count);

            yield return mesh;

            currentIndex++;

            if (targetIndex.HasValue)
                yield break;
        }
    }

    /// <summary>
    /// Returns the size in bytes for a given primitive type in PRM file format.
    /// Returns -1 for unknown types.
    /// </summary>
    private static int GetPrimitiveSizeInBytes(PrimitiveType type)
    {
        return type switch
        {
            PrimitiveType.F3 => 12,        // 3*i16 + pad + u32
            PrimitiveType.FT3 => 24,       // 3*i16 + 3*i16 + 6*u8 + pad + u32
            PrimitiveType.F4 => 12,        // 4*i16 + u32
            PrimitiveType.FT4 => 28,       // 4*i16 + 3*i16 + 8*u8 + pad + u32
            PrimitiveType.G3 => 20,        // 3*i16 + pad + 3*u32
            PrimitiveType.GT3 => 32,       // 3*i16 + 3*i16 + 6*u8 + pad + 3*u32
            PrimitiveType.G4 => 24,        // 4*i16 + 4*u32
            PrimitiveType.GT4 => 40,       // 4*i16 + 3*i16 + 8*u8 + pad + 4*u32
            PrimitiveType.LF2 => 12,       // Unknown, estimated
            PrimitiveType.TSPR => 12,      // Transparent sprite
            PrimitiveType.BSPR => 12,      // Billboard sprite
            PrimitiveType.LSF3 => 12,      // Light source flat triangle
            PrimitiveType.LSFT3 => 24,     // Light source flat textured triangle
            PrimitiveType.LSF4 => 16,      // Light source flat quad
            PrimitiveType.LSFT4 => 30,     // Light source flat textured quad
            PrimitiveType.LSG3 => 24,      // Light source gouraud triangle
            PrimitiveType.LSGT3 => 36,     // Light source gouraud textured triangle
            PrimitiveType.LSG4 => 32,      // Light source gouraud quad
            PrimitiveType.LSGT4 => 46,     // Light source gouraud textured quad
            PrimitiveType.Spline => 52,    // (vec3+pad)*3 + rgba
            PrimitiveType.InfiniteLight => 12,  // i16*3 + pad + rgba
            PrimitiveType.PointLight => 24,     // vec3 + pad + rgba + i16*2
            PrimitiveType.SpotLight => 36,      // vec3 + pad + i16*3 + pad + rgba + i16*4
            _ => -1
        };
    }

    private List<Primitive> HandleUnknownType(short type)
    {
        _logger.LogWarning("Unknown primitive type {Type}, stopping parse", type);
        return new List<Primitive>();
    }

    private static bool IsInvalidCount(in PrmObjectHeader header)
    {
        return header.VerticesLen < 0 || header.NormalsLen < 0 || header.PrimitivesLen < 0 ||
               header.VerticesLen > 10000 || header.NormalsLen > 10000 || header.PrimitivesLen > 10000;
    }

    private void LogNewPrimitiveType(short type)
    {
        _primitiveTypesSeen ??= new HashSet<short>();
        if (_primitiveTypesSeen.Add(type))
        {
            _logger.LogWarning("DEBUG: Encountered primitive type {Type} for first time", type);
        }
    }

    private static List<Primitive> ParseF3(byte[] bytes, ref int p, short flag)
    {
        var f3 = new F3 { Flags = flag, CoordIndices = ReadCoordIndices(bytes, ref p, 3) };
        p += 2; // pad1
        f3.Color = RgbaFromU32(ReadU32(bytes, ref p));
        return new List<Primitive> { f3 };
    }

    private static List<Primitive> ParseF4(byte[] bytes, ref int p, short flag)
    {
        var f4 = new F4
        {
            Flags = flag,
            CoordIndices = ReadCoordIndices(bytes, ref p, 4),
            Color = RgbaFromU32(ReadU32(bytes, ref p))
        };
        return new List<Primitive> { f4 };
    }

    private static List<Primitive> ParseFT3(byte[] bytes, ref int p, short flag)
    {
        var ft3 = new FT3
        {
            Flags = flag,
            CoordIndices = ReadCoordIndices(bytes, ref p, 3),
            TextureId = ReadI16(bytes, ref p)
        };
        p += 4; // cba, tsb
        ft3.UVs = ReadUVs(bytes, ref p, 3);
        p += 2; // pad1
        ft3.Color = RgbaFromU32(ReadU32(bytes, ref p));
        ConvertUVsToFloat(ft3, ft3.UVs);
        return new List<Primitive> { ft3 };
    }

    private static List<Primitive> ParseFT4(byte[] bytes, ref int p, short flag)
    {
        var ft4 = new FT4
        {
            Flags = flag,
            CoordIndices = ReadCoordIndices(bytes, ref p, 4),
            TextureId = ReadI16(bytes, ref p)
        };
        p += 4; // cba, tsb
        ft4.UVs = ReadUVs(bytes, ref p, 4);
        p += 2; // pad1
        ft4.Color = RgbaFromU32(ReadU32(bytes, ref p));
        ConvertUVsToFloat(ft4, ft4.UVs);
        return new List<Primitive> { ft4 };
    }

    private static List<Primitive> ParseG3(byte[] bytes, ref int p, short flag)
    {
        var g3 = new G3 { Flags = flag, CoordIndices = ReadCoordIndices(bytes, ref p, 3) };
        p += 2; // pad1
        g3.Colors = ReadColors(bytes, ref p, 3);
        return new List<Primitive> { g3 };
    }

    private static List<Primitive> ParseG4(byte[] bytes, ref int p, short flag)
    {
        var g4 = new G4
        {
            Flags = flag,
            CoordIndices = ReadCoordIndices(bytes, ref p, 4),
            Colors = ReadColors(bytes, ref p, 4)
        };
        return new List<Primitive> { g4 };
    }

    private static List<Primitive> ParseGT3(byte[] bytes, ref int p, short flag)
    {
        var gt3 = new GT3
        {
            Flags = flag,
            CoordIndices = ReadCoordIndices(bytes, ref p, 3),
            TextureId = ReadI16(bytes, ref p)
        };
        p += 4; // cba, tsb
        gt3.UVs = ReadUVs(bytes, ref p, 3);
        p += 2; // pad1
        gt3.Colors = ReadColors(bytes, ref p, 3);
        ConvertUVsToFloat(gt3, gt3.UVs);
        return new List<Primitive> { gt3 };
    }

    private static List<Primitive> ParseGT4(byte[] bytes, ref int p, short flag)
    {
        var coords = new short[] { ReadI16(bytes, ref p), ReadI16(bytes, ref p), ReadI16(bytes, ref p), ReadI16(bytes, ref p) };
        var texId = ReadI16(bytes, ref p);
        p += 4; // cba, tsb

        var uvs = new[] { (ReadU8(bytes, ref p), ReadU8(bytes, ref p)),
                          (ReadU8(bytes, ref p), ReadU8(bytes, ref p)),
                          (ReadU8(bytes, ref p), ReadU8(bytes, ref p)),
                          (ReadU8(bytes, ref p), ReadU8(bytes, ref p)) };
        p += 2; // pad1

        var colors = new[] { RgbaFromU32(ReadU32(bytes, ref p)),
                             RgbaFromU32(ReadU32(bytes, ref p)),
                             RgbaFromU32(ReadU32(bytes, ref p)),
                             RgbaFromU32(ReadU32(bytes, ref p)) };

        var gt3a = new GT3
        {
            Flags = flag,
            CoordIndices = new short[] { coords[0], coords[1], coords[2] },
            TextureId = texId,
            UVs = new[] { uvs[0], uvs[1], uvs[2] },
            Colors = new[] { colors[0], colors[1], colors[2] }
        };
        ConvertUVsToFloat(gt3a, gt3a.UVs);

        var gt3b = new GT3
        {
            Flags = flag,
            CoordIndices = new short[] { coords[1], coords[3], coords[2] },
            TextureId = texId,
            UVs = new[] { uvs[1], uvs[3], uvs[2] },
            Colors = new[] { colors[1], colors[2], colors[3] }
        };
        ConvertUVsToFloat(gt3b, gt3b.UVs);

        return new List<Primitive> { gt3a, gt3b };
    }

    private static List<Primitive> ParseLSF3(byte[] bytes, ref int p, short flag)
    {
        var lsf3 = new F3 { Flags = flag, CoordIndices = ReadCoordIndices(bytes, ref p, 3) };
        _ = ReadI16(bytes, ref p); // skip light source field
        lsf3.Color = RgbaFromU32(ReadU32(bytes, ref p));
        return new List<Primitive> { lsf3 };
    }

    private static List<Primitive> ParseLSFT3(byte[] bytes, ref int p, short flag)
    {
        var lsft3 = new FT3 { Flags = flag, CoordIndices = ReadCoordIndices(bytes, ref p, 3) };
        _ = ReadI16(bytes, ref p); // skip light source field
        lsft3.TextureId = ReadI16(bytes, ref p);
        p += 4; // cba, tsb
        lsft3.UVs = ReadUVs(bytes, ref p, 3);
        lsft3.Color = RgbaFromU32(ReadU32(bytes, ref p));
        ConvertUVsToFloat(lsft3, lsft3.UVs);
        return new List<Primitive> { lsft3 };
    }

    /// <summary>
    /// Parse a single primitive based on type.
    /// Ported from the switch statement in object.c lines 107-450
    /// Returns a list since some PRM primitives (e.g. GT4) expand into multiple triangles.
    /// </summary>
    private List<Primitive> ParsePrimitive(byte[] bytes, ref int p, short type, short flag)
    {
        LogNewPrimitiveType(type);

        return (PrimitiveType)type switch
        {
            PrimitiveType.F3 => ParseF3(bytes, ref p, flag),
            PrimitiveType.FT3 => ParseFT3(bytes, ref p, flag),
            PrimitiveType.F4 => ParseF4(bytes, ref p, flag),
            PrimitiveType.FT4 => ParseFT4(bytes, ref p, flag),
            PrimitiveType.G3 => ParseG3(bytes, ref p, flag),
            PrimitiveType.GT3 => ParseGT3(bytes, ref p, flag),
            PrimitiveType.G4 => ParseG4(bytes, ref p, flag),
            PrimitiveType.GT4 => ParseGT4(bytes, ref p, flag),
            PrimitiveType.TSPR or PrimitiveType.BSPR => SkipSprite(bytes, ref p),
            PrimitiveType.LSF3 => ParseLSF3(bytes, ref p, flag),
            PrimitiveType.LSFT3 => ParseLSFT3(bytes, ref p, flag),
            PrimitiveType.LSF4 or PrimitiveType.LSFT4 or PrimitiveType.LSG3 or
            PrimitiveType.LSGT3 or PrimitiveType.LSG4 or PrimitiveType.LSGT4 => SkipUnimplementedLightSource(type),
            PrimitiveType.Spline or PrimitiveType.InfiniteLight or
            PrimitiveType.PointLight or PrimitiveType.SpotLight => SkipLightData(bytes, ref p, (PrimitiveType)type),
            _ => HandleUnknownType(type)
        };
    }

    private static (byte r, byte g, byte b, byte a)[] ReadColors(byte[] bytes, ref int p, int count)
    {
        var colors = new (byte, byte, byte, byte)[count];
        for (int i = 0; i < count; i++)
            colors[i] = RgbaFromU32(ReadU32(bytes, ref p));
        return colors;
    }

    private static short[] ReadCoordIndices(byte[] bytes, ref int p, int count)
    {
        var indices = new short[count];
        for (int i = 0; i < count; i++)
            indices[i] = ReadI16(bytes, ref p);
        return indices;
    }

    private static string ReadFixedString(byte[] bytes, ref int p, int length)
    {
        string result = System.Text.Encoding.ASCII.GetString(bytes, p, length);
        p += length;
        int nullIdx = result.IndexOf('\0');
        return nullIdx >= 0 ? result[..nullIdx] : result;
    }

    private static short ReadI16(byte[] bytes, ref int p)
    {
        // BIG-ENDIAN (matches get_i16 in utils.h)
        short value = (short)((bytes[p] << 8) | bytes[p + 1]);
        p += 2;
        return value;
    }

    private static int ReadI32(byte[] bytes, ref int p)
    {
        // BIG-ENDIAN (matches get_i32 in utils.h)
        int value = (bytes[p] << 24) | (bytes[p + 1] << 16) | (bytes[p + 2] << 8) | bytes[p + 3];
        p += 4;
        return value;
    }

    private Mesh ReadMesh(byte[] bytes, ref int p, in PrmObjectHeader header)
    {
        var mesh = new Mesh(header.Name)
        {
            Origin = new Vec3(header.OriginX, header.OriginY, header.OriginZ),
            Flags = header.Flags
        };

        mesh.Vertices = ReadVertices(bytes, ref p, header.VerticesLen, out float radius);
        mesh.Radius = radius;
        mesh.Normals = ReadNormals(bytes, ref p, header.NormalsLen);
        mesh.Primitives = ReadPrimitives(bytes, ref p, header.PrimitivesLen);

        return mesh;
    }

    private static Vec3[] ReadNormals(byte[] bytes, ref int p, short count)
    {
        var normals = new Vec3[count];
        for (int i = 0; i < count; i++)
        {
            short nx = ReadI16(bytes, ref p);
            short ny = ReadI16(bytes, ref p);
            short nz = ReadI16(bytes, ref p);
            p += 2; // padding
            normals[i] = new Vec3(nx, ny, nz);
        }
        return normals;
    }

    private List<Primitive> ReadPrimitives(byte[] bytes, ref int p, short count)
    {
        var primitives = new List<Primitive>();

        for (int i = 0; i < count; i++)
        {
            if (p + 4 > bytes.Length)
            {
                _logger.LogWarning("Truncated primitive data at {Pos}", p);
                break;
            }

            short prmType = ReadI16(bytes, ref p);
            short prmFlag = ReadI16(bytes, ref p);

            try
            {
                var parsed = ParsePrimitive(bytes, ref p, prmType, prmFlag);
                if (parsed.Count > 0)
                {
                    primitives.AddRange(parsed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to parse primitive {Index} type {Type}: {Error}", i, prmType, ex.Message);
                break;
            }
        }

        return primitives;
    }

    private byte[] ReadPrmBytes(string filepath)
    {
        if (string.IsNullOrWhiteSpace(filepath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filepath));
        }

        if (!File.Exists(filepath))
        {
            throw new FileNotFoundException($"PRM file not found: {filepath}", filepath);
        }

        byte[] bytes = File.ReadAllBytes(filepath);
        _logger.LogInformation("Loading PRM: {Path} ({Size} bytes)", filepath, bytes.Length);
        return bytes;
    }

    private static uint ReadU32(byte[] bytes, ref int p)
    {
        return (uint)ReadI32(bytes, ref p);
    }

    private static byte ReadU8(byte[] bytes, ref int p)
    {
        return bytes[p++];
    }

    private static (byte u, byte v)[] ReadUVs(byte[] bytes, ref int p, int count)
    {
        var uvs = new (byte, byte)[count];
        for (int i = 0; i < count; i++)
            uvs[i] = (ReadU8(bytes, ref p), ReadU8(bytes, ref p));
        return uvs;
    }

    private static Vec3[] ReadVertices(byte[] bytes, ref int p, short count, out float radius)
    {
        radius = 0;
        var vertices = new Vec3[count];
        for (int i = 0; i < count; i++)
        {
            short x = ReadI16(bytes, ref p);
            short y = ReadI16(bytes, ref p);
            short z = ReadI16(bytes, ref p);
            p += 2; // padding

            vertices[i] = new Vec3(x, y, z);

            if (Math.Abs(x) > radius) radius = Math.Abs(x);
            if (Math.Abs(y) > radius) radius = Math.Abs(y);
            if (Math.Abs(z) > radius) radius = Math.Abs(z);
        }
        return vertices;
    }

    /// <summary>
    /// Convert a 32-bit RGBA value to tuple (matches rgba_from_u32 in utils.h)
    /// </summary>
    private static (byte r, byte g, byte b, byte a) RgbaFromU32(uint v)
    {
        return (
            (byte)((v >> 24) & 0xFF),
            (byte)((v >> 16) & 0xFF),
            (byte)((v >> 8) & 0xFF),
            255  // Alpha always 255 for now
        );
    }

    private static List<Primitive> SkipLightData(byte[] bytes, ref int p, PrimitiveType type)
    {
        int skipBytes = GetPrimitiveSizeInBytes(type);
        if (skipBytes > 0)
            p += skipBytes;
        return new List<Primitive>();
    }

    private static void SkipObjectData(byte[] bytes, ref int p, in PrmObjectHeader header)
    {
        p += header.VerticesLen * 8; // vertices
        p += header.NormalsLen * 8; // normals

        for (int i = 0; i < header.PrimitivesLen; i++)
        {
            if (p + 4 > bytes.Length)
                break;

            short prmType = ReadI16(bytes, ref p);
            _ = ReadI16(bytes, ref p); // flag ignored here
            int skipped = SkipPrimitive(bytes, ref p, prmType);
            if (skipped < 0)
                break;
        }
    }

    /// <summary>
    /// Skip bytes for a primitive without parsing (for objects we don't want to load).
    /// Returns number of bytes skipped, or -1 if unknown type.
    /// </summary>
    private static int SkipPrimitive(byte[] bytes, ref int p, short type)
    {
        int bytesToSkip = GetPrimitiveSizeInBytes((PrimitiveType)type);
        if (bytesToSkip < 0)
            return -1;

        p += bytesToSkip;
        return bytesToSkip;
    }

    private static List<Primitive> SkipSprite(byte[] bytes, ref int p)
    {
        p += 12; // Skip remaining sprite data
        return new List<Primitive>();
    }

    private List<Primitive> SkipUnimplementedLightSource(short type)
    {
        _logger.LogDebug("Skipping unimplemented light source primitive type {Type}", type);
        return new List<Primitive>();
    }

    private static bool TryReadObjectHeader(byte[] bytes, ref int p, out PrmObjectHeader header)
    {
        header = default;

        const int minimumHeaderSize = 144;
        if (p > bytes.Length - minimumHeaderSize)
        {
            return false;
        }

        string name = ReadFixedString(bytes, ref p, 16);

        short verticesLen = ReadI16(bytes, ref p); p += 2; // padding
        _ = ReadI32(bytes, ref p); // verticesPtr

        short normalsLen = ReadI16(bytes, ref p); p += 2; // padding
        _ = ReadI32(bytes, ref p); // normalsPtr

        short primitivesLen = ReadI16(bytes, ref p); p += 2; // padding
        _ = ReadI32(bytes, ref p); // primitivesPtr

        p += 4; // unused ptr
        p += 4; // unused ptr
        p += 4; // skeleton ref

        _ = ReadI32(bytes, ref p); // extent
        short flags = ReadI16(bytes, ref p); p += 2; // padding
        _ = ReadI32(bytes, ref p); // nextPtr (unused)

        p += 3 * 3 * 2; // relative rotation matrix (3x3 i16)
        p += 2; // padding

        int originX = ReadI32(bytes, ref p);
        int originY = ReadI32(bytes, ref p);
        int originZ = ReadI32(bytes, ref p);

        p += 3 * 3 * 2; // absolute rotation matrix
        p += 2; // padding
        p += 3 * 4; // absolute translation
        p += 2; // skeleton update flag
        p += 2; // padding
        p += 4; // skeleton super
        p += 4; // skeleton sub
        p += 4; // skeleton next

        header = new PrmObjectHeader(name, verticesLen, normalsLen, primitivesLen, originX, originY, originZ, flags);
        return true;
    }

    #endregion 

    /// <summary>
    /// Primitive type identifiers from PlayStation 1 PRM file format
    /// </summary>
    private enum PrimitiveType : short
    {
        F3 = 1,              // Flat triangle
        FT3 = 2,             // Flat textured triangle
        F4 = 3,              // Flat quad
        FT4 = 4,             // Flat textured quad
        G3 = 5,              // Gouraud triangle
        GT3 = 6,             // Gouraud textured triangle
        G4 = 7,              // Gouraud quad
        GT4 = 8,             // Gouraud textured quad
        LF2 = 9,             // Line (unknown)
        TSPR = 10,           // Transparent sprite
        BSPR = 11,           // Billboard sprite
        LSF3 = 12,           // Light source flat triangle
        LSFT3 = 13,          // Light source flat textured triangle
        LSF4 = 14,           // Light source flat quad
        LSFT4 = 15,          // Light source flat textured quad
        LSG3 = 16,           // Light source gouraud triangle
        LSGT3 = 17,          // Light source gouraud textured triangle
        LSG4 = 18,           // Light source gouraud quad
        LSGT4 = 19,          // Light source gouraud textured quad
        Spline = 20,         // Spline curve
        InfiniteLight = 21,  // Infinite light source
        PointLight = 22,     // Point light source
        SpotLight = 23       // Spot light source
    }

    private record struct PrmObjectHeader(
                                    string Name,
                                    short VerticesLen,
                                    short NormalsLen,
                                    short PrimitivesLen,
                                    int OriginX,
                                    int OriginY,
                                    int OriginZ,
                                    short Flags);
}