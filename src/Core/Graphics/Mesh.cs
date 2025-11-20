using System;
using System.Collections.Generic;

namespace WipeoutRewrite.Core.Graphics
{

/// <summary>
/// Represents a 3D mesh with vertices, normals, and primitives.
/// Based on Object structure from wipeout-rewrite/src/wipeout/object.h
/// </summary>
public class Mesh
{
    /// <summary>
    /// Mesh name (from PRM file)
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// 3D vertex positions
    /// </summary>
    public Vec3[] Vertices { get; set; } = Array.Empty<Vec3>();
    
    /// <summary>
    /// Normal vectors for lighting calculations
    /// </summary>
    public Vec3[] Normals { get; set; } = Array.Empty<Vec3>();
    
    /// <summary>
    /// Primitives (triangles, quads) that make up the mesh
    /// </summary>
    public List<Primitive> Primitives { get; set; } = new();
    
    /// <summary>
    /// Origin point of the mesh
    /// </summary>
    public Vec3 Origin { get; set; }
    
    /// <summary>
    /// Bounding sphere radius for culling
    /// </summary>
    public float Radius { get; set; }
    
    /// <summary>
    /// Flags for mesh characteristics (from PRM format)
    /// </summary>
    public int Flags { get; set; }

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
    /// Type of primitive (F3, FT3, GT3, etc.)
    /// </summary>
    public PrimitiveType Type { get; set; }
    
    /// <summary>
    /// Flags for rendering behavior
    /// </summary>
    public short Flags { get; set; }
}

/// <summary>
/// Types of primitives supported in PRM format.
/// Based on PRM_TYPE_* constants from object.h
/// </summary>
public enum PrimitiveType : short
{
    F3 = 1,      // Flat triangle
    F4 = 2,      // Flat quad
    FT3 = 3,     // Flat textured triangle
    FT4 = 4,     // Flat textured quad
    G3 = 5,      // Gouraud triangle
    G4 = 6,      // Gouraud quad
    GT3 = 7,     // Gouraud textured triangle
    GT4 = 8,     // Gouraud textured quad
}

/// <summary>
/// Flat textured triangle (most common in ship models).
/// Based on FT3 struct from object.h
/// </summary>
public class FT3 : Primitive
{
    /// <summary>
    /// Indices into vertex array
    /// </summary>
    public short[] CoordIndices { get; set; } = new short[3];
    
    /// <summary>
    /// Texture ID
    /// </summary>
    public short TextureId { get; set; }
    
    /// <summary>
    /// UV coordinates for texture mapping
    /// </summary>
    public (byte u, byte v)[] UVs { get; set; } = new (byte, byte)[3];
    
    /// <summary>
    /// Color tint (RGBA)
    /// </summary>
    public (byte r, byte g, byte b, byte a) Color { get; set; }

    public FT3()
    {
        Type = PrimitiveType.FT3;
    }
}

/// <summary>
/// Gouraud textured triangle (for smooth shading).
/// Based on GT3 struct from object.h
/// </summary>
public class GT3 : Primitive
{
    /// <summary>
    /// Indices into vertex array
    /// </summary>
    public short[] CoordIndices { get; set; } = new short[3];
    
    /// <summary>
    /// Texture ID
    /// </summary>
    public short TextureId { get; set; }
    
    /// <summary>
    /// UV coordinates for texture mapping
    /// </summary>
    public (byte u, byte v)[] UVs { get; set; } = new (byte, byte)[3];
    
    /// <summary>
    /// Per-vertex colors for Gouraud shading
    /// </summary>
    public (byte r, byte g, byte b, byte a)[] Colors { get; set; } = new (byte, byte, byte, byte)[3];

    public GT3()
    {
        Type = PrimitiveType.GT3;
    }
}

/// <summary>
/// Flat triangle (no texture, solid color).
/// Based on F3 struct from object.h
/// </summary>
public class F3 : Primitive
{
    /// <summary>
    /// Indices into vertex array
    /// </summary>
    public short[] CoordIndices { get; set; } = new short[3];
    
    /// <summary>
    /// Solid color (RGBA)
    /// </summary>
    public (byte r, byte g, byte b, byte a) Color { get; set; }

    public F3()
    {
        Type = PrimitiveType.F3;
    }
}

}
