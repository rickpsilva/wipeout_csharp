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

    // Debug: track which types we encounter

    public ModelLoader(ILogger<ModelLoader> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region methods

    /// <summary>
    /// Creates a simple mock model for testing when PRM loading fails
    /// </summary>
    public Mesh CreateMockModel(string name)
    {
        return CreateMockModelScaled(name, 1.0f);
    }

    /// <summary>
    /// Creates a mock model with scaled geometry - Wipeout-style futuristic racer
    /// </summary>
    public Mesh CreateMockModelScaled(string name, float scale = 1.0f)
    {
        var mesh = new Mesh(name)
        {
            Origin = new Vec3(0, 0, 0),
            Radius = 500.0f * scale,
            Flags = 0,
            // Wipeout-style anti-gravity racer - elongated with prominent wings
            Vertices = new Vec3[]
        {
                // Sharp nose (0-3)
                new(0, 20 * scale, 500 * scale),           // 0: Nose tip (sharp point)
                new(-25 * scale, -5 * scale, 450 * scale), // 1: Nose left
                new(25 * scale, -5 * scale, 450 * scale),  // 2: Nose right  
                new(0, 35 * scale, 450 * scale),           // 3: Nose top
                
                // Front body (4-7)
                new(-50 * scale, 5 * scale, 350 * scale),  // 4: Front left
                new(50 * scale, 5 * scale, 350 * scale),   // 5: Front right
                new(0, 50 * scale, 350 * scale),           // 6: Front top (cockpit)
                new(0, -10 * scale, 350 * scale),          // 7: Front bottom
                
                // Mid body / main section (8-11)
                new(-70 * scale, 10 * scale, 200 * scale), // 8: Mid left
                new(70 * scale, 10 * scale, 200 * scale),  // 9: Mid right
                new(0, 55 * scale, 200 * scale),           // 10: Mid top
                new(0, -5 * scale, 200 * scale),           // 11: Mid bottom
                
                // Wing tips - LARGE for silhouette (12-15)
                new(-180 * scale, 5 * scale, 150 * scale), // 12: Left wing tip (extended)
                new(180 * scale, 5 * scale, 150 * scale),  // 13: Right wing tip (extended)
                new(-160 * scale, 0 * scale, 100 * scale), // 14: Left wing trailing
                new(160 * scale, 0 * scale, 100 * scale),  // 15: Right wing trailing
                
                // Engine pods - rear section (16-23)
                new(-60 * scale, 20 * scale, 80 * scale),  // 16: Left engine top
                new(60 * scale, 20 * scale, 80 * scale),   // 17: Right engine top
                new(-60 * scale, -10 * scale, 80 * scale), // 18: Left engine bottom
                new(60 * scale, -10 * scale, 80 * scale),  // 19: Right engine bottom
                new(-55 * scale, 15 * scale, -50 * scale), // 20: Left exhaust top
                new(55 * scale, 15 * scale, -50 * scale),  // 21: Right exhaust top
                new(-55 * scale, -10 * scale, -50 * scale),// 22: Left exhaust bottom
                new(55 * scale, -10 * scale, -50 * scale), // 23: Right exhaust bottom
                
                // Tail / rear stabilizers (24-27)
                new(0, 45 * scale, 50 * scale),            // 24: Center fin top
                new(0, -5 * scale, 50 * scale),            // 25: Center bottom
                new(0, 60 * scale, -30 * scale),           // 26: Tail fin top
                new(0, 0 * scale, -30 * scale),            // 27: Tail fin base
        },

            Normals = new Vec3[]
        {
                new(0, 1, 0),  // Top
                new(0, -1, 0), // Bottom
                new(-1, 0, 0), // Left
                new(1, 0, 0),  // Right
                new(0, 0, 1),  // Front
                new(0, 0, -1), // Back
        }
        };

        // Color palette - HIGH CONTRAST colors
        var yellow = (r: (byte)255, g: (byte)220, b: (byte)0, a: (byte)255);    // Bright yellow
        var orange = (r: (byte)255, g: (byte)100, b: (byte)0, a: (byte)255);    // Orange
        var red = (r: (byte)255, g: (byte)0, b: (byte)0, a: (byte)255);         // Pure red
        var darkBlue = (r: (byte)0, g: (byte)50, b: (byte)150, a: (byte)255);   // Dark blue
        var white = (r: (byte)255, g: (byte)255, b: (byte)255, a: (byte)255);   // White
        var black = (r: (byte)40, g: (byte)40, b: (byte)40, a: (byte)255);      // Almost black

        mesh.Primitives = new List<Primitive>
            {
                // === NOSE - Yellow/Orange ===
                new F3 { CoordIndices = new short[] { 0, 3, 1 }, Color = yellow },
                new F3 { CoordIndices = new short[] { 0, 2, 3 }, Color = yellow },
                new F3 { CoordIndices = new short[] { 0, 1, 2 }, Color = orange },
                
                // === FRONT BODY - Yellow top, dark sides ===
                new F3 { CoordIndices = new short[] { 1, 3, 4 }, Color = orange },
                new F3 { CoordIndices = new short[] { 2, 5, 3 }, Color = orange },
                new F3 { CoordIndices = new short[] { 3, 6, 4 }, Color = yellow },
                new F3 { CoordIndices = new short[] { 3, 5, 6 }, Color = yellow },
                new F3 { CoordIndices = new short[] { 1, 4, 7 }, Color = darkBlue },
                new F3 { CoordIndices = new short[] { 2, 7, 5 }, Color = darkBlue },
                
                // === COCKPIT TOP - Bright yellow ===
                new F3 { CoordIndices = new short[] { 6, 10, 4 }, Color = yellow },
                new F3 { CoordIndices = new short[] { 6, 5, 10 }, Color = yellow },
                
                // === MAIN BODY - Orange/Dark contrast ===
                new F3 { CoordIndices = new short[] { 4, 8, 7 }, Color = black },
                new F3 { CoordIndices = new short[] { 5, 7, 9 }, Color = black },
                new F3 { CoordIndices = new short[] { 10, 8, 4 }, Color = orange },
                new F3 { CoordIndices = new short[] { 10, 9, 5 }, Color = orange },
                new F3 { CoordIndices = new short[] { 7, 8, 11 }, Color = darkBlue },
                new F3 { CoordIndices = new short[] { 7, 11, 9 }, Color = darkBlue },
                
                // === LARGE WINGS - Red/Dark for visibility ===
                // Left wing
                new F3 { CoordIndices = new short[] { 8, 12, 11 }, Color = red },
                new F3 { CoordIndices = new short[] { 12, 14, 11 }, Color = red },
                new F3 { CoordIndices = new short[] { 8, 10, 12 }, Color = orange },
                // Right wing
                new F3 { CoordIndices = new short[] { 9, 11, 13 }, Color = red },
                new F3 { CoordIndices = new short[] { 13, 11, 15 }, Color = red },
                new F3 { CoordIndices = new short[] { 9, 13, 10 }, Color = orange },
                
                // === ENGINE PODS - Dark with red accents ===
                // Left pod
                new F3 { CoordIndices = new short[] { 8, 16, 14 }, Color = darkBlue },
                new F3 { CoordIndices = new short[] { 14, 16, 18 }, Color = black },
                new F3 { CoordIndices = new short[] { 16, 20, 18 }, Color = red },
                new F3 { CoordIndices = new short[] { 18, 20, 22 }, Color = red },
                // Right pod
                new F3 { CoordIndices = new short[] { 9, 15, 17 }, Color = darkBlue },
                new F3 { CoordIndices = new short[] { 15, 19, 17 }, Color = black },
                new F3 { CoordIndices = new short[] { 17, 19, 21 }, Color = red },
                new F3 { CoordIndices = new short[] { 19, 23, 21 }, Color = red },
                
                // === REAR / TAIL ===
                new F3 { CoordIndices = new short[] { 10, 24, 16 }, Color = yellow },
                new F3 { CoordIndices = new short[] { 10, 17, 24 }, Color = yellow },
                new F3 { CoordIndices = new short[] { 24, 26, 16 }, Color = orange },
                new F3 { CoordIndices = new short[] { 24, 17, 26 }, Color = orange },
                
                // === ENGINE EXHAUSTS - Bright white/yellow glow ===
                new F3 { CoordIndices = new short[] { 20, 21, 26 }, Color = white },
                new F3 { CoordIndices = new short[] { 20, 26, 22 }, Color = yellow },
                new F3 { CoordIndices = new short[] { 21, 23, 26 }, Color = yellow },
                new F3 { CoordIndices = new short[] { 22, 26, 23 }, Color = white },
            };

        _logger.LogInformation("Created enhanced mock model with {VertCount} vertices, {PrimCount} primitives",
            mesh.Vertices.Length, mesh.Primitives.Count);

        return mesh;
    }

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

            while (p < bytes.Length - 144)
            {
                // Read object header
                string name = ReadFixedString(bytes, ref p, 16);

                short verticesLen = ReadI16(bytes, ref p); p += 2; // padding
                int verticesPtr = ReadI32(bytes, ref p);

                short normalsLen = ReadI16(bytes, ref p); p += 2; // padding
                int normalsPtr = ReadI32(bytes, ref p);

                short primitivesLen = ReadI16(bytes, ref p); p += 2; // padding
                int primitivesPtr = ReadI32(bytes, ref p);

                p += 4; // unused ptr
                p += 4; // unused ptr
                p += 4; // skeleton ref

                int extent = ReadI32(bytes, ref p);
                short flags = ReadI16(bytes, ref p); p += 2; // padding
                int nextPtr = ReadI32(bytes, ref p);

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

                // Validate counts
                if (verticesLen < 0 || normalsLen < 0 || primitivesLen < 0 ||
                    verticesLen > 10000 || normalsLen > 10000 || primitivesLen > 10000)
                {
                    _logger.LogWarning("Invalid object counts at index {Index}, stopping scan", objectIndex);
                    break;
                }

                // Only add objects with vertices (skip markers/lights)
                if (verticesLen > 0)
                {
                    objects.Add((objectIndex, name));
                    _logger.LogInformation("Found object {Index}: '{Name}' ({VertCount} vertices, {PrimCount} primitives)",
                        objectIndex, name, verticesLen, primitivesLen);
                }

                // Skip data for this object (even if verticesLen == 0, we still need to skip)
                p += verticesLen * 8; // Each vertex: 3*i16 + pad = 8 bytes
                p += normalsLen * 8; // Each normal: 3*i16 + pad = 8 bytes

                // Skip primitives
                for (int i = 0; i < primitivesLen; i++)
                {
                    if (p + 4 > bytes.Length)
                    {
                        _logger.LogWarning("Not enough bytes to skip primitive, stopping scan");
                        return objects;
                    }

                    short prmType = ReadI16(bytes, ref p);
                    short prmFlag = ReadI16(bytes, ref p);

                    int skipped = SkipPrimitive(bytes, ref p, prmType);
                    if (skipped < 0)
                    {
                        _logger.LogWarning("Unknown primitive type {Type} at position {Pos}, stopping scan", prmType, p);
                        return objects;
                    }
                }

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
    /// Creates the original simple mock model (kept for compatibility)
    /// </summary>
    // Note: Previous simple mock generator was replaced with more detailed version above.

    /// <summary>
    /// Load a PRM file and return a Mesh.
    /// Directly ported from objects_load() in object.c
    /// </summary>
    public Mesh LoadFromPrmFile(string filepath, int objectIndex = 0)
    {
        try
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
            Console.WriteLine($"[ModelLoader] Loading PRM: {filepath} ({bytes.Length} bytes), objectIndex={objectIndex}");
            _logger.LogInformation("Loading PRM: {Path} ({Size} bytes), objectIndex={ObjIdx}", filepath, bytes.Length, objectIndex);

            int p = 0; // Current position in byte array
            Mesh? resultMesh = null;
            int currentObjectIndex = 0;

            // PRM files can contain multiple objects (like object.c:29 while (p < length))
            // We'll load all and return the first one with vertices
            // Console.WriteLine($"[ModelLoader] Entering while loop: p={p}, bytes.Length={bytes.Length}, condition={p < bytes.Length - 144}");
            while (p < bytes.Length - 144) // Need at least header size
            {
                // Console.WriteLine($"[ModelLoader] Loop iteration: p={p}");
                _logger.LogInformation("=== Starting to read object at position {Pos} (remaining: {Remain} bytes) ===", p, bytes.Length - p);
                int objectStart = p;

                // Read object header (based on object.c lines 35-76)
                string name = ReadFixedString(bytes, ref p, 16);
                _logger.LogInformation("Object name: '{Name}' at p={Pos}", name, p);

                short verticesLen = ReadI16(bytes, ref p); p += 2; // padding
                _logger.LogInformation("verticesLen={V}, p={Pos}", verticesLen, p);
                int verticesPtr = ReadI32(bytes, ref p); // We'll ignore pointers and read sequentially

                short normalsLen = ReadI16(bytes, ref p); p += 2; // padding
                int normalsPtr = ReadI32(bytes, ref p);

                short primitivesLen = ReadI16(bytes, ref p); p += 2; // padding
                int primitivesPtr = ReadI32(bytes, ref p);

                p += 4; // unused ptr
                p += 4; // unused ptr
                p += 4; // skeleton ref

                int extent = ReadI32(bytes, ref p);
                short flags = ReadI16(bytes, ref p); p += 2; // padding
                int nextPtr = ReadI32(bytes, ref p);

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

                _logger.LogInformation("Object: {Name}, Verts={VertCount}, Normals={NormCount}, Prims={PrimCount}",
                    name, verticesLen, normalsLen, primitivesLen);
                _logger.LogDebug("Current position in file: {Pos} of {Total}", p, bytes.Length);

                // Skip objects with invalid counts
                // Console.WriteLine($"[ModelLoader] Object '{name}': vertices={verticesLen}, normals={normalsLen}, prims={primitivesLen}");
                if (verticesLen < 0 || normalsLen < 0 || primitivesLen < 0 ||
                    verticesLen > 10000 || normalsLen > 10000 || primitivesLen > 10000)
                {
                    // Console.WriteLine($"[ModelLoader] Invalid counts - stopping (resultMesh={(resultMesh == null ? "null" : "SET")})");
                    _logger.LogWarning("Invalid object counts, stopping parse");
                    break; // Exit loop - if we already have resultMesh, that's OK
                }

                // Skip objects with no vertices (markers, lights, etc) - but consume their data
                if (verticesLen == 0)
                {
                    // Console.WriteLine($"[ModelLoader] Skipping object '{name}' (p={p})");
                    _logger.LogDebug("Skipping object '{Name}' with no vertices (consuming {NormCount} normals, {PrimCount} primitives)",
                        name, normalsLen, primitivesLen);

                    // Skip normals data
                    p += normalsLen * 8; // Each normal: 3*i16 + pad = 8 bytes
                                         // Console.WriteLine($"[ModelLoader] Skipped {normalsLen} normals, p={p}");

                    // Skip primitives data
                    for (int i = 0; i < primitivesLen; i++)
                    {
                        if (p + 4 > bytes.Length)
                        {
                            _logger.LogWarning("ERROR: Not enough bytes to read primitive {Index}/{Total} at p={Pos}, fileSize={Size}", i, primitivesLen, p, bytes.Length);
                            // If we already have a valid mesh, stop here gracefully
                            if (resultMesh != null)
                            {
                                _logger.LogWarning("File truncated but already have valid mesh, returning it");
                                return resultMesh;
                            }
                            break;
                        }

                        int pBefore = p;
                        // if (i < 3) Console.WriteLine($"[ModelLoader] About to read prim {i} at p={p}, next 4 bytes: {bytes[p]:X2} {bytes[p+1]:X2} {bytes[p+2]:X2} {bytes[p+3]:X2}");
                        short prmType = ReadI16(bytes, ref p);
                        short prmFlag = ReadI16(bytes, ref p);

                        int skipped = SkipPrimitive(bytes, ref p, prmType);
                        if (skipped < 0)
                        {
                            _logger.LogWarning("Unknown primitive type {Type} at position {Pos}", prmType, p);
                            break;
                        }
                        // if (i < 5) Console.WriteLine($"[ModelLoader] Prim {i}: type={prmType}, flag={prmFlag}, p: {pBefore} → {p} (advanced {p-pBefore} bytes)");
                    }

                    // Console.WriteLine($"[ModelLoader] Skipped to position {p} after {primitivesLen} primitives");
                    _logger.LogDebug("Skipped to position {Pos}", p);
                    continue; // Go to next object
                }

                // Create mesh
                _logger.LogInformation("Loading object '{Name}' with {VertCount} vertices, {NormCount} normals, {PrimCount} primitives", name, verticesLen, normalsLen, primitivesLen);

                var mesh = new Mesh(name)
                {
                    Origin = new Vec3(originX, originY, originZ),
                    Flags = flags,
                    // Read vertices (based on object.c lines 77-90)
                    Vertices = new Vec3[verticesLen]
                };
                float radius = 0;
                for (int i = 0; i < verticesLen; i++)
                {
                    short x = ReadI16(bytes, ref p);
                    short y = ReadI16(bytes, ref p);
                    short z = ReadI16(bytes, ref p);
                    p += 2; // padding

                    mesh.Vertices[i] = new Vec3(x, y, z);

                    // Calculate radius
                    if (Math.Abs(x) > radius) radius = Math.Abs(x);
                    if (Math.Abs(y) > radius) radius = Math.Abs(y);
                    if (Math.Abs(z) > radius) radius = Math.Abs(z);
                }
                mesh.Radius = radius;

                // Read normals (based on object.c lines 94-99)
                mesh.Normals = new Vec3[normalsLen];
                for (int i = 0; i < normalsLen; i++)
                {
                    short nx = ReadI16(bytes, ref p);
                    short ny = ReadI16(bytes, ref p);
                    short nz = ReadI16(bytes, ref p);
                    p += 2; // padding

                    mesh.Normals[i] = new Vec3(nx, ny, nz);
                }

                // Read primitives (based on object.c lines 101-450)
                mesh.Primitives = new List<Primitive>();

                var primitiveTypeCounts = new Dictionary<short, int>();
                for (int i = 0; i < primitivesLen; i++)
                {
                    short prmType = ReadI16(bytes, ref p);
                    short prmFlag = ReadI16(bytes, ref p);

                    if (primitiveTypeCounts.TryGetValue(prmType, out var existing))
                        primitiveTypeCounts[prmType] = existing + 1;
                    else
                        primitiveTypeCounts[prmType] = 1;

                    _logger.LogDebug("  Primitive {Index}: type={Type}, flag={Flag}, pos={Pos}", i, prmType, prmFlag, p);

                    try
                    {
                        var parsed = ParsePrimitive(bytes, ref p, prmType, prmFlag);
                        if (parsed != null && parsed.Count > 0)
                        {
                            mesh.Primitives.AddRange(parsed);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to parse primitive {Index} type {Type}: {Error}", i, prmType, ex.Message);
                        break; // Stop parsing on error
                    }
                }

                _logger.LogDebug("Primitive types in '{Name}': {Types}", name, string.Join(", ", primitiveTypeCounts.Select(kv => $"type{kv.Key}={kv.Value}")));
                _logger.LogInformation("Loaded object '{Name}': {VertCount} vertices, {PrimCount} primitives",
                    mesh.Name, mesh.Vertices.Length, mesh.Primitives.Count);

                // Log first few primitive colors for debugging
                int colorSampleCount = 0;
                foreach (var prim in mesh.Primitives)
                {
                    if (colorSampleCount++ >= 5) break;
                    if (prim is FT3 ft3)
                        _logger.LogInformation("  Sample FT3 color: R={R} G={G} B={B} A={A}", ft3.Color.r, ft3.Color.g, ft3.Color.b, ft3.Color.a);
                    else if (prim is F3 f3)
                        _logger.LogInformation("  Sample F3 color: R={R} G={G} B={B} A={A}", f3.Color.r, f3.Color.g, f3.Color.b, f3.Color.a);
                    else if (prim is G3 g3)
                        _logger.LogInformation("  Sample G3 colors: R={R0},{R1},{R2} G={G0},{G1},{G2} B={B0},{B1},{B2}",
                            g3.Colors[0].r, g3.Colors[1].r, g3.Colors[2].r,
                            g3.Colors[0].g, g3.Colors[1].g, g3.Colors[2].g,
                            g3.Colors[0].b, g3.Colors[1].b, g3.Colors[2].b);
                }

                // Check if this is the object we want (by index)
                if (verticesLen > 0 && mesh.Primitives.Count > 0)
                {
                    if (currentObjectIndex == objectIndex)
                    {
                        _logger.LogInformation("Found target object at index {Index}: '{Name}'", objectIndex, mesh.Name);
                        resultMesh = mesh;
                        break; // We found our target, stop parsing
                    }
                    currentObjectIndex++;
                }

                _logger.LogDebug("End of object, p={Pos}, remaining={Remaining}", p, bytes.Length - p);
            } // end while loop

            _logger.LogDebug("Exited loop, p={Pos}, fileSize={Size}", p, bytes.Length);

            if (resultMesh == null)
            {
                throw new InvalidDataException("No valid mesh found in PRM file");
            }

            return resultMesh;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading PRM file: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Parse a single primitive based on type.
    /// Ported from the switch statement in object.c lines 107-450
    /// </summary>
    // Return a list since some PRM primitives (e.g. GT4) expand into multiple triangles
    private System.Collections.Generic.List<Primitive> ParsePrimitive(byte[] bytes, ref int p, short type, short flag)
    {
        // Debug: log all primitive types being parsed
        // Track new primitive types seen and log first-occurrence only.
        _primitiveTypesSeen ??= new HashSet<short>();
        if (_primitiveTypesSeen.Add(type))
        {
            _logger.LogWarning("DEBUG: Encountered primitive type {Type} for first time", type);
        }

        switch (type)
        {
            case 1: // PRM_TYPE_F3
                {
                    var f3 = new F3
                    {
                        Flags = flag,
                        CoordIndices = new short[3]
                        {
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p)
                        }
                    };
                    p += 2; // pad1
                    f3.Color = RgbaFromU32(ReadU32(bytes, ref p));
                    return new System.Collections.Generic.List<Primitive> { f3 };
                }

            case 2: // PRM_TYPE_FT3 - Textured Triangle
                {
                    var ft3 = new FT3
                    {
                        Flags = flag,
                        CoordIndices = new short[3]
                        {
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p)
                        },
                        TextureId = ReadI16(bytes, ref p)
                    };
                    short cba = ReadI16(bytes, ref p);
                    short tsb = ReadI16(bytes, ref p);

                    ft3.UVs = new (byte u, byte v)[3]
                    {
                        (ReadU8(bytes, ref p), ReadU8(bytes, ref p)),
                        (ReadU8(bytes, ref p), ReadU8(bytes, ref p)),
                        (ReadU8(bytes, ref p), ReadU8(bytes, ref p))
                    };

                    p += 2; // pad1
                    ft3.Color = RgbaFromU32(ReadU32(bytes, ref p));

                    // Convert UV bytes to float 0-1 range
                    for (int k = 0; k < 3; k++)
                    {
                        ft3.UVsF[k] = (ft3.UVs[k].u / 255f, ft3.UVs[k].v / 255f);
                    }

                    return new System.Collections.Generic.List<Primitive> { ft3 };
                }

            case 3: // PRM_TYPE_F4 - Flat Quad (4 vertices, solid color)
                {
                    var f4 = new F4
                    {
                        Flags = flag,
                        CoordIndices = new short[4]
                        {
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p)
                        },
                        Color = RgbaFromU32(ReadU32(bytes, ref p))
                    };

                    return new System.Collections.Generic.List<Primitive> { f4 };
                }

            case 4: // PRM_TYPE_FT4 - Textured Quad (4 vertices)
                {
                    var ft4 = new FT4
                    {
                        Flags = flag,
                        CoordIndices = new short[4]
                        {
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p)
                        },
                        TextureId = ReadI16(bytes, ref p)
                    };
                    p += 4; // cba, tsb

                    ft4.UVs = new (byte u, byte v)[4]
                    {
                        (ReadU8(bytes, ref p), ReadU8(bytes, ref p)),
                        (ReadU8(bytes, ref p), ReadU8(bytes, ref p)),
                        (ReadU8(bytes, ref p), ReadU8(bytes, ref p)),
                        (ReadU8(bytes, ref p), ReadU8(bytes, ref p))
                    };

                    p += 2; // pad1
                    ft4.Color = RgbaFromU32(ReadU32(bytes, ref p));

                    // Convert UV bytes to float 0-1 range
                    for (int k = 0; k < 4; k++)
                    {
                        ft4.UVsF[k] = (ft4.UVs[k].u / 255f, ft4.UVs[k].v / 255f);
                    }

                    return new System.Collections.Generic.List<Primitive> { ft4 };
                }

            case 5: // PRM_TYPE_G3
                {
                    var g3 = new G3
                    {
                        Flags = flag,
                        CoordIndices = new short[3]
                        {
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p)
                        }
                    };
                    p += 2; // pad1
                    g3.Colors = new (byte r, byte g, byte b, byte a)[3]
                    {
                        RgbaFromU32(ReadU32(bytes, ref p)),
                        RgbaFromU32(ReadU32(bytes, ref p)),
                        RgbaFromU32(ReadU32(bytes, ref p))
                    };
                    return new System.Collections.Generic.List<Primitive> { g3 };
                }

            case 6: // PRM_TYPE_GT3 - Gouraud Textured Triangle
                {
                    var gt3 = new GT3
                    {
                        Flags = flag,
                        CoordIndices = new short[3]
                        {
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p)
                        },
                        TextureId = ReadI16(bytes, ref p)
                    };
                    p += 4; // cba, tsb

                    gt3.UVs = new (byte u, byte v)[3]
                    {
                        (ReadU8(bytes, ref p), ReadU8(bytes, ref p)),
                        (ReadU8(bytes, ref p), ReadU8(bytes, ref p)),
                        (ReadU8(bytes, ref p), ReadU8(bytes, ref p))
                    };

                    p += 2; // pad1
                    gt3.Colors = new (byte r, byte g, byte b, byte a)[3]
                    {
                        RgbaFromU32(ReadU32(bytes, ref p)),
                        RgbaFromU32(ReadU32(bytes, ref p)),
                        RgbaFromU32(ReadU32(bytes, ref p))
                    };

                    for (int k = 0; k < 3; k++)
                    {
                        gt3.UVsF[k] = (gt3.UVs[k].u / 255f, gt3.UVs[k].v / 255f);
                    }

                    return new System.Collections.Generic.List<Primitive> { gt3 };
                }

            case 7: // PRM_TYPE_G4 - Gouraud Quad (4 vertices, Gouraud shading, no texture)
                {
                    var g4 = new G4
                    {
                        Flags = flag,
                        CoordIndices = new short[4]
                        {
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p)
                        },
                        Colors = new (byte r, byte g, byte b, byte a)[4]
                    {
                        RgbaFromU32(ReadU32(bytes, ref p)),
                        RgbaFromU32(ReadU32(bytes, ref p)),
                        RgbaFromU32(ReadU32(bytes, ref p)),
                        RgbaFromU32(ReadU32(bytes, ref p))
                    }
                    };

                    return new System.Collections.Generic.List<Primitive> { g4 };
                }

            case 8: // PRM_TYPE_GT4 - Gouraud Textured Quad (split into 2 triangles)
                {
                    short i0 = ReadI16(bytes, ref p);
                    short i1 = ReadI16(bytes, ref p);
                    short i2 = ReadI16(bytes, ref p);
                    short i3 = ReadI16(bytes, ref p);

                    short texId = ReadI16(bytes, ref p);
                    p += 4; // cba, tsb

                    byte u0 = ReadU8(bytes, ref p), v0 = ReadU8(bytes, ref p);
                    byte u1 = ReadU8(bytes, ref p), v1 = ReadU8(bytes, ref p);
                    byte u2 = ReadU8(bytes, ref p), v2 = ReadU8(bytes, ref p);
                    byte u3 = ReadU8(bytes, ref p), v3 = ReadU8(bytes, ref p);

                    p += 2; // pad1

                    var c0 = RgbaFromU32(ReadU32(bytes, ref p));
                    var c1 = RgbaFromU32(ReadU32(bytes, ref p));
                    var c2 = RgbaFromU32(ReadU32(bytes, ref p));
                    var (r, g, b, a) = RgbaFromU32(ReadU32(bytes, ref p));

                    var gt3a = new GT3
                    {
                        Flags = flag,
                        CoordIndices = new short[] { i0, i1, i2 },
                        TextureId = texId,
                        UVs = new[] { (u0, v0), (u1, v1), (u2, v2) },
                        Colors = new[] { c0, c1, c2 }
                    };
                    for (int k = 0; k < 3; k++)
                    {
                        gt3a.UVsF[k] = (gt3a.UVs[k].u / 255f, gt3a.UVs[k].v / 255f);
                    }

                    // Create second triangle (0,2,3) from the quad to fully represent GT4
                    var gt3b = new GT3
                    {
                        Flags = flag,
                        // Match original C rendering: second triangle uses coords[1],coords[3],coords[2]
                        CoordIndices = new short[] { i1, i3, i2 },
                        TextureId = texId,
                        UVs = new[] { (u1, v1), (u3, v3), (u2, v2) },
                        Colors = new[] { c1, c2, (r, g, b, a) }
                    };
                    gt3b.UVsF[0] = (gt3b.UVs[0].u / 255f, gt3b.UVs[0].v / 255f);
                    gt3b.UVsF[1] = (gt3b.UVs[1].u / 255f, gt3b.UVs[1].v / 255f);
                    gt3b.UVsF[2] = (gt3b.UVs[2].u / 255f, gt3b.UVs[2].v / 255f);

                    return new System.Collections.Generic.List<Primitive> { gt3a, gt3b };
                }

            case 12: // PRM_TYPE_LSF3 - Light source flat triangle
                {
                    var lsf3 = new F3
                    {
                        Flags = flag,
                        CoordIndices = new short[3]
                        {
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p)
                        }
                    };
                    short normal = ReadI16(bytes, ref p);
                    lsf3.Color = RgbaFromU32(ReadU32(bytes, ref p));
                    return new System.Collections.Generic.List<Primitive> { lsf3 };
                }

            case 13: // PRM_TYPE_LSFT3 - Light source flat textured triangle
                {
                    var lsft3 = new FT3
                    {
                        Flags = flag,
                        CoordIndices = new short[3]
                        {
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p),
                            ReadI16(bytes, ref p)
                        }
                    };
                    short normal = ReadI16(bytes, ref p);
                    lsft3.TextureId = ReadI16(bytes, ref p);
                    p += 4; // cba, tsb

                    lsft3.UVs = new (byte u, byte v)[3]
                    {
                        (ReadU8(bytes, ref p), ReadU8(bytes, ref p)),
                        (ReadU8(bytes, ref p), ReadU8(bytes, ref p)),
                        (ReadU8(bytes, ref p), ReadU8(bytes, ref p))
                    };

                    lsft3.Color = RgbaFromU32(ReadU32(bytes, ref p));

                    for (int k = 0; k < 3; k++)
                    {
                        lsft3.UVsF[k] = (lsft3.UVs[k].u / 255f, lsft3.UVs[k].v / 255f);
                    }

                    return new System.Collections.Generic.List<Primitive> { lsft3 };
                }

            case 14: // PRM_TYPE_LSF4
            case 15: // PRM_TYPE_LSFT4
            case 16: // PRM_TYPE_LSG3
            case 17: // PRM_TYPE_LSGT3
            case 18: // PRM_TYPE_LSG4
            case 19: // PRM_TYPE_LSGT4
                     // TODO: Implement remaining light source types
                _logger.LogDebug("Skipping unimplemented light source primitive type {Type}", type);
                return new System.Collections.Generic.List<Primitive>();

            case 10: // PRM_TYPE_TSPR - Transparent sprite
            case 11: // PRM_TYPE_BSPR - Billboard sprite
                     // Skip sprite primitives
                p += 14; // sizeof(SPR) - 2 (type already read)
                return new System.Collections.Generic.List<Primitive>();

            case 20: // PRM_TYPE_SPLINE
                p += 40; // sizeof(Spline) - 2
                return new System.Collections.Generic.List<Primitive>();

            case 21: // PRM_TYPE_INFINITE_LIGHT
                p += 18; // sizeof(InfiniteLight) - 2
                return new System.Collections.Generic.List<Primitive>();

            case 22: // PRM_TYPE_POINT_LIGHT
                p += 26; // sizeof(PointLight) - 2
                return new System.Collections.Generic.List<Primitive>();

            case 23: // PRM_TYPE_SPOT_LIGHT
                p += 42; // sizeof(SpotLight) - 2
                return new System.Collections.Generic.List<Primitive>();

            default:
                _logger.LogWarning("Unknown primitive type {Type}, stopping parse", type);
                return new System.Collections.Generic.List<Primitive>();
        }
    }

    // Binary reading helpers (little-endian)

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

    private static uint ReadU32(byte[] bytes, ref int p)
    {
        return (uint)ReadI32(bytes, ref p);
    }

    // NOTE: ReadI8 removed — not used in current model parsing code.

    private static byte ReadU8(byte[] bytes, ref int p)
    {
        return bytes[p++];
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

    /// <summary>
    /// Skip bytes for a primitive without parsing (for objects we don't want to load).
    /// Returns number of bytes skipped, or -1 if unknown type.
    /// </summary>
    private static int SkipPrimitive(byte[] bytes, ref int p, short type)
    {
        int start = p;

        switch (type)
        {
            case 1: // PRM_TYPE_F3: 3*i16 + pad + u32 = 12 bytes
                p += 12;
                break;
            case 2: // PRM_TYPE_FT3: 3*i16 + 3*i16 + 6*u8 + pad + u32 = 24 bytes
                p += 24;
                break;
            case 3: // PRM_TYPE_F4: 4*i16 + u32 = 12 bytes
                p += 12;
                break;
            case 4: // PRM_TYPE_FT4: 4*i16 + 3*i16 + 8*u8 + pad + u32 = 28 bytes
                p += 28;
                break;
            case 5: // PRM_TYPE_G3: 3*i16 + pad + 3*u32 = 20 bytes
                p += 20;
                break;
            case 6: // PRM_TYPE_GT3: 3*i16 + 3*i16 + 6*u8 + pad + 3*u32 = 32 bytes
                p += 32;
                break;
            case 7: // PRM_TYPE_G4: 4*i16 + 4*u32 = 24 bytes
                p += 24;
                break;
            case 8: // PRM_TYPE_GT4: 4*i16 + 3*i16 + 8*u8 + pad + 4*u32 = 40 bytes
                p += 40;
                break;
            case 9: // PRM_TYPE_LF2: (unknown, skip for now)
                p += 12;
                break;
            case 10: // PRM_TYPE_TSPR: i16 + i16 + i16 + i16 + u32 = 12 bytes
                p += 12;
                break;
            case 11: // PRM_TYPE_BSPR: i16 + i16 + i16 + i16 + u32 = 12 bytes
                p += 12;
                break;
            case 12: // PRM_TYPE_LSF3: 3*i16 + i16 + u32 = 12 bytes
                p += 12;
                break;
            case 13: // PRM_TYPE_LSFT3: 3*i16 + i16 + 3*i16 + 6*u8 + u32 = 24 bytes
                p += 24;
                break;
            case 14: // PRM_TYPE_LSF4: 4*i16 + i16 + pad + u32 = 16 bytes
                p += 16;
                break;
            case 15: // PRM_TYPE_LSFT4: 4*i16 + i16 + 3*i16 + 8*u8 + u32 = 30 bytes
                p += 30;
                break;
            case 16: // PRM_TYPE_LSG3: 3*i16 + 3*i16 + 3*u32 = 24 bytes
                p += 24;
                break;
            case 17: // PRM_TYPE_LSGT3: 3*i16 + 3*i16 + 3*i16 + 6*u8 + 3*u32 = 36 bytes
                p += 36;
                break;
            case 18: // PRM_TYPE_LSG4: 4*i16 + 4*i16 + 4*u32 = 32 bytes
                p += 32;
                break;
            case 19: // PRM_TYPE_LSGT4: 4*i16 + 4*i16 + 3*i16 + 8*u8 + pad + 4*u32 = 46 bytes
                p += 46;
                break;
            case 20: // PRM_TYPE_SPLINE: (vec3+pad)*3 + rgba = 16+16+16+4 = 52 bytes
                p += 52;
                break;
            case 21: // PRM_TYPE_INFINITE_LIGHT: i16*3 + pad + rgba = 6+2+4 = 12 bytes
                p += 12;
                break;
            case 22: // PRM_TYPE_POINT_LIGHT: vec3 + pad + rgba + i16*2 = 12+4+4+4 = 24 bytes
                p += 24;
                break;
            case 23: // PRM_TYPE_SPOT_LIGHT: vec3 + pad + i16*3 + pad + rgba + i16*4 = 12+4+6+2+4+8 = 36 bytes
                p += 36;
                break;
            default:
                return -1; // Unknown type
        }

        return p - start;
    }

    #endregion 
}