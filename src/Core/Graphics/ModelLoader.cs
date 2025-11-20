using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Core.Graphics
{
    /// <summary>
    /// Mock model loader for demonstration purposes.
    /// Loads hardcoded ship geometry instead of parsing PRM files.
    /// 
    /// TODO: Implement full PRM parser based on wipeout-rewrite/src/wipeout/object.c
    /// </summary>
    public class ModelLoader
    {
        private readonly ILogger<ModelLoader>? _logger;

        public ModelLoader(ILogger<ModelLoader>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates a mock ship model with basic geometry.
        /// In production, this would parse the PRM binary format.
        /// </summary>
        public Mesh CreateMockShipModel(string shipName)
        {
            _logger?.LogInformation("Creating mock model for ship: {ShipName}", shipName);

            var mesh = new Mesh(shipName)
            {
                Origin = new Vec3(0, 0, 0),
                Radius = 500.0f,
                Flags = 0
            };

            // Create simple ship vertices (simplified wedge shape)
            mesh.Vertices = new Vec3[]
            {
                // Nose (front)
                new Vec3(0, 0, 384),       // 0: Front tip
                
                // Front wings
                new Vec3(-256, -60, 150),  // 1: Left front wing
                new Vec3(256, -60, 150),   // 2: Right front wing
                new Vec3(0, 60, 150),      // 3: Top front (cockpit)
                
                // Rear
                new Vec3(-256, -60, -384), // 4: Left rear
                new Vec3(256, -60, -384),  // 5: Right rear
                new Vec3(0, 0, -384),      // 6: Rear center (engine)
                
                // Bottom center
                new Vec3(0, -60, 0),       // 7: Bottom center
            };

            // Create normals (simplified)
            mesh.Normals = new Vec3[]
            {
                new Vec3(0, 1, 0),    // 0: Up
                new Vec3(0, -1, 0),   // 1: Down
                new Vec3(-1, 0, 0),   // 2: Left
                new Vec3(1, 0, 0),    // 3: Right
                new Vec3(0, 0, 1),    // 4: Forward
                new Vec3(0, 0, -1),   // 5: Backward
            };

            // Create primitives (triangles)
            mesh.Primitives = new List<Primitive>
            {
                // Top surface
                CreateFT3(0, 3, 1, 0, new byte[] { 64, 0, 0, 128, 128, 128 }),
                CreateFT3(0, 2, 3, 0, new byte[] { 64, 0, 128, 128, 0, 128 }),
                
                // Left side
                CreateFT3(0, 1, 4, 0, new byte[] { 64, 0, 0, 128, 128, 128 }),
                CreateFT3(1, 7, 4, 0, new byte[] { 0, 128, 64, 64, 128, 128 }),
                
                // Right side
                CreateFT3(0, 5, 2, 0, new byte[] { 64, 0, 128, 128, 0, 128 }),
                CreateFT3(2, 5, 7, 0, new byte[] { 0, 128, 128, 128, 64, 64 }),
                
                // Bottom
                CreateFT3(1, 2, 7, 0, new byte[] { 0, 0, 128, 0, 64, 64 }),
                
                // Rear left
                CreateFT3(4, 6, 1, 0, new byte[] { 0, 128, 64, 128, 128, 64 }),
                
                // Rear right
                CreateFT3(6, 5, 2, 0, new byte[] { 64, 128, 128, 128, 128, 64 }),
            };

            _logger?.LogInformation("Created mock model with {VertexCount} vertices and {PrimCount} primitives",
                mesh.Vertices.Length, mesh.Primitives.Count);

            return mesh;
        }
        

        /// <summary>
        /// Helper to create FT3 primitive
        /// </summary>
        private FT3 CreateFT3(short v0, short v1, short v2, short textureId, byte[] uvs)
        {
            return new FT3
            {
                CoordIndices = new short[] { v0, v1, v2 },
                TextureId = textureId,
                UVs = new[]
                {
                    (uvs[0], uvs[1]),
                    (uvs[2], uvs[3]),
                    (uvs[4], uvs[5])
                },
                Color = (128, 128, 128, 255) // Default gray color
            };
        }

        /// <summary>
        /// TODO: Full PRM loader implementation.
        /// 
        /// This would:
        /// 1. Read binary PRM file format
        /// 2. Parse header (name, vertex count, normal count, primitive count)
        /// 3. Load vertices array (int16 x, y, z + padding)
        /// 4. Load normals array (int16 x, y, z + padding)
        /// 5. Load primitives with different types:
        ///    - F3/F4: Flat triangles/quads
        ///    - FT3/FT4: Textured triangles/quads
        ///    - G3/G4: Gouraud shaded
        ///    - GT3/GT4: Gouraud + textured
        ///    - LS*: Light source variations
        /// 6. Handle texture references and UV coordinates
        /// 7. Calculate bounding sphere radius
        /// 
        /// Reference: wipeout-rewrite/src/wipeout/object.c::objects_load()
        /// </summary>
        public Mesh LoadFromPrmFile(string filepath)
        {
            throw new NotImplementedException(
                "Full PRM parser not yet implemented. " +
                "Use CreateMockShipModel() for demonstration. " +
                "See wipeout-rewrite/src/wipeout/object.c for reference implementation.");
        }
    }
}
