namespace WipeoutRewrite.Core.Graphics;

#region interfaces

/// <summary>
/// Interface for primitives that support texture mapping with UVs.
/// Used for generic UV conversion operations.
/// </summary>
public interface ITexturedPrimitive
{
    (float u, float v)[] UVsF { get; set; }
}

#endregion

#region classes

/// <summary>
/// Flat triangle (no texture, solid color).
/// Based on F3 struct from object.h
/// </summary>
public class F3 : Primitive
{
    /// <summary>
    /// Solid color (RGBA)
    /// </summary>
    public (byte r, byte g, byte b, byte a) Color { get; set; }

    /// <summary>
    /// Indices into vertex array
    /// </summary>
    public short[] CoordIndices { get; set; } = new short[3];

    public F3()
    {
        Type = PrimitiveType.F3;
    }
}

/// <summary>
/// Flat quad (no texture, solid color).
/// Based on F4 struct from object.h
/// </summary>
public class F4 : Primitive
{
    /// <summary>
    /// Solid color (RGBA)
    /// </summary>
    public (byte r, byte g, byte b, byte a) Color { get; set; }

    /// <summary>
    /// Indices into vertex array
    /// </summary>
    public short[] CoordIndices { get; set; } = new short[4];

    public F4()
    {
        Type = PrimitiveType.F4;
    }
}

/// <summary>
/// Flat textured triangle (most common in ship models).
/// Based on FT3 struct from object.h
/// </summary>
public class FT3 : Primitive, ITexturedPrimitive
{
    #region properties

    /// <summary>
    /// Color tint (RGBA)
    /// </summary>
    public (byte r, byte g, byte b, byte a) Color { get; set; }

    /// <summary>
    /// Indices into vertex array
    /// </summary>
    public short[] CoordIndices { get; set; } = new short[3];

    /// <summary>
    /// GL texture handle mapped from PRM TextureId by the TextureManager.
    /// 0 means use the white/default texture.
    /// </summary>
    public int TextureHandle { get; set; } = 0;

    /// <summary>
    /// Texture ID
    /// </summary>
    public short TextureId { get; set; }

    /// <summary>
    /// UV coordinates for texture mapping
    /// </summary>
    public (byte u, byte v)[] UVs { get; set; } = new (byte, byte)[3];

    /// <summary>
    /// Normalized UV coordinates (0..1) for rendering. Populated by ModelLoader.
    /// </summary>
    public (float u, float v)[] UVsF { get; set; } = new (float, float)[3];

    #endregion

    public FT3()
    {
        Type = PrimitiveType.FT3;
    }
}

/// <summary>
/// Flat textured quad (4 vertices, one texture, single color tint).
/// Based on FT4 struct from object.h
/// </summary>
public class FT4 : Primitive, ITexturedPrimitive
{
    #region properties

    /// <summary>
    /// Color tint (RGBA)
    /// </summary>
    public (byte r, byte g, byte b, byte a) Color { get; set; }

    /// <summary>
    /// Indices into vertex array
    /// </summary>
    public short[] CoordIndices { get; set; } = new short[4];

    /// <summary>
    /// GL texture handle mapped from PRM TextureId by the TextureManager.
    /// 0 means use the white/default texture.
    /// </summary>
    public int TextureHandle { get; set; } = 0;

    /// <summary>
    /// Texture ID
    /// </summary>
    public short TextureId { get; set; }

    /// <summary>
    /// UV coordinates for texture mapping
    /// </summary>
    public (byte u, byte v)[] UVs { get; set; } = new (byte, byte)[4];

    /// <summary>
    /// Normalized UV coordinates (0..1) for rendering. Populated by ModelLoader.
    /// </summary>
    public (float u, float v)[] UVsF { get; set; } = new (float, float)[4];

    #endregion

    public FT4()
    {
        Type = PrimitiveType.FT4;
    }
}

/// <summary>
/// Gouraud shaded triangle (no texture) - per-vertex colors.
/// </summary>
public class G3 : Primitive
{
    public (byte r, byte g, byte b, byte a)[] Colors { get; set; } = new (byte, byte, byte, byte)[3];
    public short[] CoordIndices { get; set; } = new short[3];

    public G3()
    {
        Type = PrimitiveType.G3;
    }
}

/// <summary>
/// Gouraud shaded quad (no texture)
/// </summary>
public class G4 : Primitive
{
    public (byte r, byte g, byte b, byte a)[] Colors { get; set; } = new (byte, byte, byte, byte)[4];
    public short[] CoordIndices { get; set; } = new short[4];

    public G4()
    {
        Type = PrimitiveType.G4;
    }
}

/// <summary>
/// Gouraud textured triangle (for smooth shading).
/// Based on GT3 struct from object.h
/// </summary>
public class GT3 : Primitive, ITexturedPrimitive
{
    #region properties

    /// <summary>
    /// Per-vertex colors for Gouraud shading
    /// </summary>
    public (byte r, byte g, byte b, byte a)[] Colors { get; set; } = new (byte, byte, byte, byte)[3];

    /// <summary>
    /// Indices into vertex array
    /// </summary>
    public short[] CoordIndices { get; set; } = new short[3];

    /// <summary>
    /// GL texture handle mapped from PRM TextureId by the TextureManager.
    /// 0 means use the white/default texture.
    /// </summary>
    public int TextureHandle { get; set; } = 0;

    /// <summary>
    /// Texture ID
    /// </summary>
    public short TextureId { get; set; }

    /// <summary>
    /// UV coordinates for texture mapping
    /// </summary>
    public (byte u, byte v)[] UVs { get; set; } = new (byte, byte)[3];

    /// <summary>
    /// Normalized UV coordinates (0..1) for rendering. Populated by ModelLoader when available.
    /// </summary>
    public (float u, float v)[] UVsF { get; set; } = new (float, float)[3];

    #endregion

    public GT3()
    {
        Type = PrimitiveType.GT3;
    }
}

/// <summary>
/// Gouraud textured quad (texture with per-vertex colors).
/// Based on GT4 struct from object.h
/// </summary>
public class GT4 : Primitive, ITexturedPrimitive
{
    #region properties

    /// <summary>
    /// Per-vertex colors for Gouraud shading (4 colors for quad)
    /// </summary>
    public (byte r, byte g, byte b, byte a)[] Colors { get; set; } = new (byte, byte, byte, byte)[4];

    /// <summary>
    /// Indices into vertex array (4 vertices for quad)
    /// </summary>
    public short[] CoordIndices { get; set; } = new short[4];

    /// <summary>
    /// GL texture handle mapped from PRM TextureId by the TextureManager.
    /// 0 means use the white/default texture.
    /// </summary>
    public int TextureHandle { get; set; } = 0;

    /// <summary>
    /// Texture ID
    /// </summary>
    public short TextureId { get; set; }

    /// <summary>
    /// UV coordinates for texture mapping (4 UV pairs for quad)
    /// </summary>
    public (byte u, byte v)[] UVs { get; set; } = new (byte, byte)[4];

    /// <summary>
    /// Normalized UV coordinates (0..1) for rendering. Populated by ModelLoader when available.
    /// </summary>
    public (float u, float v)[] UVsF { get; set; } = new (float, float)[4];

    #endregion

    public GT4()
    {
        Type = PrimitiveType.GT4;
    }
}

/// <summary>
/// Represents a 3D mesh with vertices, normals, and primitives.
/// Based on Object structure from wipeout-rewrite/src/wipeout/object.h
/// </summary>
public class Mesh
{
    #region properties

    /// <summary>
    /// Flags for mesh characteristics (from PRM format)
    /// </summary>
    public int Flags { get; set; }

    /// <summary>
    /// Mesh name (from PRM file)
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Normal vectors for lighting calculations
    /// </summary>
    public Vec3[] Normals { get; set; } = Array.Empty<Vec3>();

    /// <summary>
    /// Origin point of the mesh
    /// </summary>
    public Vec3 Origin { get; set; }

    /// <summary>
    /// Primitives (triangles, quads) that make up the mesh
    /// </summary>
    public List<Primitive> Primitives { get; set; } = new();

    /// <summary>
    /// Bounding sphere radius for culling
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    /// 3D vertex positions
    /// </summary>
    public Vec3[] Vertices { get; set; } = Array.Empty<Vec3>();

    #endregion

    public Mesh(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Base class for mesh primitives (polygons).
/// Based on Primitive types from wipeout-rewrite/src/wipeout/object.h
/// </summary>
public abstract class Primitive
{
    /// <summary>
    /// Flags for rendering behavior
    /// </summary>
    public short Flags { get; set; }

    /// <summary>
    /// Type of primitive (F3, FT3, GT3, etc.)
    /// </summary>
    public PrimitiveType Type { get; set; }
}

/// <summary>
/// Primitive flags for rendering behavior.
/// Based on PRM_* flag constants from wipeout-rewrite/src/wipeout/object.h
/// </summary>
public static class PrimitiveFlags
{
    /// <summary>
    /// Primitive is part of ship engine exhaust (for animation).
    /// </summary>
    public const short SHIP_ENGINE = 0x0002;

    /// <summary>
    /// Primitive should only be rendered from one side (enable backface culling).
    /// If this flag is NOT set, primitive is double-sided (render both sides).
    /// </summary>
    public const short SINGLE_SIDED = 0x0001;

    /// <summary>
    /// Primitive uses alpha blending (semi-transparent).
    /// </summary>
    public const short TRANSLUCENT = 0x0004;
}

#endregion

/// <summary>
/// Types of primitives supported in PRM format.
/// Based on PRM_TYPE_* constants from object.h
/// </summary>
public enum PrimitiveType : short
{
    F3 = 1,      // Flat triangle
    FT3 = 2,     // Flat textured triangle
    F4 = 3,      // Flat quad
    FT4 = 4,     // Flat textured quad
    G3 = 5,      // Gouraud triangle
    GT3 = 6,     // Gouraud textured triangle
    G4 = 7,      // Gouraud quad
    GT4 = 8,     // Gouraud textured quad
}