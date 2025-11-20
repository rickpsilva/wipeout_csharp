# PRM Model Loading System

## Overview

The 3D model system implements loading and rendering of ship models from PlayStation PRM (Primitive) format. Currently includes a mock loader for demonstration, with the full PRM parser marked as future work.

## Architecture

### Core Components

#### Mesh.cs
**Location:** `src/Core/Graphics/Mesh.cs`

Represents a 3D model with:
- **Vertices** - 3D positions (Vec3 array)
- **Normals** - Normal vectors for lighting (Vec3 array)
- **Primitives** - List of polygons (triangles, quads)
- **Metadata** - Name, origin, radius, flags

#### Primitive Types

Based on PlayStation GPU primitive types from `wipeout-rewrite/src/wipeout/object.h`:

```csharp
public enum PrimitiveType : short
{
    F3 = 1,      // Flat triangle (solid color)
    F4 = 2,      // Flat quad
    FT3 = 3,     // Flat textured triangle
    FT4 = 4,     // Flat textured quad
    G3 = 5,      // Gouraud triangle (smooth shading)
    G4 = 6,      // Gouraud quad
    GT3 = 7,     // Gouraud textured triangle
    GT4 = 8,     // Gouraud textured quad
}
```

**Implemented Classes:**
- `FT3` - Flat textured triangle (most common in ship models)
- `GT3` - Gouraud textured triangle (smooth per-vertex colors)
- `F3` - Flat triangle (solid color, no texture)

Each primitive contains:
- Coordinate indices (references to vertex array)
- Texture ID
- UV coordinates (texture mapping)
- Color data (RGBA)

#### ModelLoader.cs
**Location:** `src/Core/Graphics/ModelLoader.cs`

Provides model loading functionality:
- `CreateMockShipModel()` - Generates simplified ship geometry
- `LoadFromPrmFile()` - TODO: Full PRM binary parser

## Current Implementation (Mock Loader)

### Mock Ship Geometry

The mock loader creates a simplified wedge-shaped ship:

```
        Nose (front)
           /\
          /  \
   Left  /    \ Right
  Wing  /      \ Wing
       /        \
      /          \
     /____________\
   Rear          Rear
```

**Vertices (8 total):**
- 0: Nose tip (0, 0, 384)
- 1-2: Front wings (±256, -60, 150)
- 3: Cockpit (0, 60, 150)
- 4-5: Rear corners (±256, -60, -384)
- 6: Rear center (0, 0, -384)
- 7: Bottom center (0, -60, 0)

**Primitives (9 triangles):**
- Top surface (2 triangles)
- Left side (2 triangles)
- Right side (2 triangles)
- Bottom (1 triangle)
- Rear surfaces (2 triangles)

### Usage Example

```csharp
// Create loader
var loader = new ModelLoader(logger);

// Generate mock ship model
var shipModel = loader.CreateMockShipModel("Feisar");

// Assign to ship
ship.Model = shipModel;

// Render will now draw the 3D model
ship.Render(renderer);
```

## Integration with Ship Class

The Ship entity now includes:

```csharp
public class Ship
{
    public Mesh? Model { get; set; }
    
    public void Render(IRenderer renderer)
    {
        if (!IsVisible || Model == null)
            return;
            
        // Render each primitive
        foreach (var primitive in Model.Primitives)
        {
            if (primitive is FT3 ft3)
                RenderFT3(renderer, ft3, Model.Vertices, transform);
            // ... other primitive types
        }
    }
}
```

### Primitive Rendering Methods

**RenderFT3** - Flat textured triangle:
- Retrieves vertices by coordinate indices
- Transforms by ship position/rotation
- Applies texture UVs
- Calls `renderer.PushTri()` with color

**RenderGT3** - Gouraud textured triangle:
- Similar to FT3 but with per-vertex colors
- Enables smooth color gradients across triangle

**RenderF3** - Flat triangle:
- No texture, solid color only
- Used for untextured geometry

## PRM File Format (PlayStation)

### Binary Structure

Based on analysis of `wipeout-rewrite/src/wipeout/object.c`:

```
PRM File Structure:
├── Object Header
│   ├── Name (16 bytes)
│   ├── Vertex count (int16)
│   ├── Normal count (int16)
│   ├── Primitive count (int16)
│   ├── Origin (vec3)
│   ├── Extent/Flags
│   └── Radius
├── Vertex Array
│   └── [x, y, z, padding] * count (int16 values)
├── Normal Array
│   └── [x, y, z, padding] * count (int16 values)
└── Primitive Array
    └── [type, flag, data...] * count (variable size per type)
```

### Primitive Data Structures

Each primitive type has specific data layout:

**FT3 (Flat Textured Triangle):**
```c
struct FT3 {
    int16_t type;          // 3
    int16_t flag;
    int16_t coords[3];     // Vertex indices
    int16_t texture;       // Texture ID
    int16_t cba;           // Color lookup
    int16_t tsb;           // Texture page
    uint8_t u0, v0;        // UV coord 0
    uint8_t u1, v1;        // UV coord 1
    uint8_t u2, v2;        // UV coord 2
    int16_t pad;
    rgba_t color;          // RGBA color
};
```

**GT3 (Gouraud Textured Triangle):**
```c
struct GT3 {
    int16_t type;          // 7
    int16_t flag;
    int16_t coords[3];     // Vertex indices
    int16_t texture;       // Texture ID
    int16_t cba, tsb;
    uint8_t u0, v0;
    uint8_t u1, v1;
    uint8_t u2, v2;
    int16_t pad;
    rgba_t color[3];       // Per-vertex colors
};
```

### Additional Primitive Types

The original engine supports many more types:
- **LSF3/LSF4** - Light source flat (with normal)
- **LSFT3/LSFT4** - Light source flat textured
- **LSG3/LSG4** - Light source Gouraud
- **LSGT3/LSGT4** - Light source Gouraud textured
- **SPR** - Sprite primitives
- **Spline** - Bezier curves

## Future Implementation: Full PRM Parser

### Implementation Steps

1. **Binary Reader Setup**
```csharp
public Mesh LoadFromPrmFile(string filepath)
{
    byte[] bytes = File.ReadAllBytes(filepath);
    int position = 0;
    
    // Parse header
    string name = ReadString(bytes, ref position, 16);
    short vertexCount = ReadInt16(bytes, ref position);
    short normalCount = ReadInt16(bytes, ref position);
    short primitiveCount = ReadInt16(bytes, ref position);
    // ...
}
```

2. **Parse Vertices**
```csharp
Vec3[] vertices = new Vec3[vertexCount];
for (int i = 0; i < vertexCount; i++)
{
    float x = ReadInt16(bytes, ref position);
    float y = ReadInt16(bytes, ref position);
    float z = ReadInt16(bytes, ref position);
    position += 2; // padding
    vertices[i] = new Vec3(x, y, z);
}
```

3. **Parse Primitives**
```csharp
for (int i = 0; i < primitiveCount; i++)
{
    short type = ReadInt16(bytes, ref position);
    short flag = ReadInt16(bytes, ref position);
    
    switch ((PrimitiveType)type)
    {
        case PrimitiveType.FT3:
            primitives.Add(ParseFT3(bytes, ref position));
            break;
        case PrimitiveType.GT3:
            primitives.Add(ParseGT3(bytes, ref position));
            break;
        // ... other types
    }
}
```

4. **Calculate Bounding Sphere**
```csharp
float radius = 0;
foreach (var v in vertices)
{
    float dist = MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
    if (dist > radius)
        radius = dist;
}
```

### Challenges

1. **Endianness** - PlayStation is little-endian, same as x86/x64
2. **Padding** - Careful alignment between fields
3. **Texture References** - Need texture list to resolve texture IDs
4. **Variable Size** - Different primitive types have different sizes
5. **Memory Layout** - C structs may not align with C# structs directly

### Testing Strategy

1. Load known PRM file (e.g., `wipeout/common/allsh.prm`)
2. Verify vertex count matches expected
3. Check primitive count
4. Validate vertex coordinates are reasonable
5. Ensure UV coordinates are in 0-255 range
6. Compare with original C implementation output

## Testing

### Current Tests (13 tests in ModelTests.cs)

**Mesh Construction:**
- Constructor initialization
- Empty arrays by default

**Primitive Types:**
- FT3, GT3, F3 type verification
- Array initialization
- Coordinate index count

**Mock Loader:**
- Returns valid mesh
- Correct vertex count (8)
- Has triangles
- Proper ship shape (nose at front)
- Valid coordinate indices
- UV coordinates in range
- NotImplementedException for real loader

### Test Results

- **Total:** 166/166 tests passing ✅
- **Coverage:** 91.43% on model classes
- **New Tests:** 13 tests for model system

## Performance Considerations

### Memory

- Vertex data: 12 bytes per vertex (3 * float)
- Normal data: 12 bytes per normal
- Primitive data: varies by type (48-80 bytes typical)
- Ship model: ~1000 vertices, ~2000 primitives = ~220KB

### Rendering

- Transformation: Matrix * Vector for each vertex
- Culling: Check bounding sphere before rendering
- Batching: Group primitives by texture for efficiency
- LOD: Could implement multiple detail levels

## References

- Original PRM loader: [`wipeout-rewrite/src/wipeout/object.c`](../../wipeout-rewrite/src/wipeout/object.c)
- Primitive structures: [`wipeout-rewrite/src/wipeout/object.h`](../../wipeout-rewrite/src/wipeout/object.h)
- Object rendering: [`wipeout-rewrite/src/wipeout/object.c::object_draw()`](../../wipeout-rewrite/src/wipeout/object.c)
- Ship loading: [`wipeout-rewrite/src/wipeout/ship.c::ships_load()`](../../wipeout-rewrite/src/wipeout/ship.c)

## Integration Points

### Texture System
Models reference textures by ID. Need:
- Texture loading from TIM files
- Texture atlas management
- UV coordinate mapping

### Lighting System
Light source primitives (LS*) use normals for lighting:
- Normal transformation by model matrix
- Dot product with light direction
- Ambient + diffuse + specular calculations

### Collision System
Could use simplified collision mesh:
- Bounding sphere for broad phase
- Specific vertices (nose, wings) for narrow phase
- Or separate collision model (alcol.prm)

## Future Enhancements

1. **Full PRM Parser** - Complete binary format support
2. **Quad Support** - FT4, GT4 rendering (split into 2 triangles)
3. **Sprite Support** - SPR primitives for exhaust
4. **LOD System** - Multiple detail levels
5. **Instancing** - Render multiple ships efficiently
6. **Model Cache** - Load PRM once, share between ships
7. **Model Editor** - Tool to view/edit PRM files
