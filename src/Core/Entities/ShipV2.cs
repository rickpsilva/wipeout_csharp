using System;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Graphics;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.Assets;
using Microsoft.Extensions.Logging.Abstractions;

namespace WipeoutRewrite.Core.Entities
{
    public class ShipV2 : IShipV2
    {
        private readonly ILogger<ShipV2> _logger;

        private readonly ITextureManager _textureManager;

        private readonly IRenderer _renderer;

        public string Name { get; set; } = "UnnamedShip";
        public int ShipId { get; private set; } = 0;

        public Mesh? GetModel()
        {
            return Model;
        }

        /// <summary>
        /// Load PRM Model
        /// </summary>
        public Mesh? Model { get; private set; }

        /// <summary>
        /// Textures IDs loaded from CMP (if any)
        /// </summary>
        public int[] Texture { get; set; }

        /// <summary>
        /// Shadow texture handle (shad1.tim - shad4.tim)
        /// </summary>
        public int ShadowTexture { get; private set; } = -1;

        public ShipV2(
            IRenderer renderer,
            ILogger<ShipV2> logger,
            ITextureManager textureManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _textureManager = textureManager ?? throw new ArgumentNullException(nameof(textureManager));

            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _logger.LogInformation("ShipV2 criada: {Name}", Name);
            Texture = Array.Empty<int>();
            Model = null;
        }

        // --- Minimal runtime state (subset of ship_t) ---
        public Vec3 Position { get; set; } = new Vec3(0, 0, 0);
        public Vec3 Velocity { get; set; } = new Vec3(0, 0, 0);
        // Rotação inicial: PI em Z para corrigir orientação do modelo 3D
        // (modelo PRM vem invertido, precisa rotação de 180° para ficar correto)
        public Vec3 Angle { get; set; } = new Vec3(0, 0, MathF.PI);
        public Vec3 DirForward { get; private set; } = new Vec3(0, 0, 1);
        public Vec3 DirRight { get; private set; } = new Vec3(1, 0, 0);
        public Vec3 DirUp { get; private set; } = new Vec3(0, 1, 0);

        public int SectionNum { get; set; }
        public int TotalSectionNum { get; set; }
        public bool IsVisible { get; set; }
        public bool IsFlying => Position.Y > 0;

        public void ShipLoad(int shipIndex = 0)
        {
            ShipId = shipIndex;
            Name = "Ship_" + ShipId;
            // Allow overriding the PRM path via environment variable (used by test script)
            string? envPrm = Environment.GetEnvironmentVariable("SHIPRENDER_PRM");
            string? prmPath = null;
            if (!string.IsNullOrEmpty(envPrm) && System.IO.File.Exists(envPrm))
            {
                prmPath = envPrm;
            }
            else
            {
                string[] candidates = new string[] {
                    System.IO.Path.Combine("assets", "wipeout", "common", "allsh.prm"),
                    System.IO.Path.Combine("..", "..", "assets", "wipeout", "common", "allsh.prm"),
                    System.IO.Path.Combine("..", "..", "..", "assets", "wipeout", "common", "allsh.prm"),
                };

                foreach (var c in candidates)
                {
                    if (System.IO.File.Exists(c))
                    {
                        prmPath = c;
                        _logger.LogInformation("Found PRM at: {Path}", System.IO.Path.GetFullPath(c));
                        break;
                    }
                }
            }

            if (prmPath != null)
            {
                // Allow selecting which ship from allsh.prm (0-7, default 0)
                string? shipIndexEnv = Environment.GetEnvironmentVariable("SHIP_INDEX");
                if (!string.IsNullOrEmpty(shipIndexEnv) && int.TryParse(shipIndexEnv, out int parsedIndex))
                {
                    shipIndex = parsedIndex;
                }


                _logger.LogInformation("Loading PRM model from {Prm}, ship index={Index}", prmPath, shipIndex);
                LoadPrm(prmPath, shipIndex);

                // Try load CMP with same base name
                string cmpCandidate = System.IO.Path.ChangeExtension(prmPath, ".cmp");
                if (System.IO.File.Exists(cmpCandidate))
                {
                    _logger.LogInformation("Loading CMP textures from {Cmp}", cmpCandidate);
                    LoadCmpTextures(cmpCandidate);

                }
                else
                {
                    _logger.LogInformation("No CMP file found alongside PRM ({Cmp})", cmpCandidate);
                }

                // Load shadow texture (shad1.tim - shad4.tim)
                // C original uses: ships[i].shadow_texture = shadow_textures_start + (i >> 1)
                // So ships 0-1 use shad1, 2-3 use shad2, 4-5 use shad3, 6-7 use shad4
                int shadowIndex = (shipIndex >> 1) + 1; // 0-1→1, 2-3→2, 4-5→3, 6-7→4
                _logger.LogInformation("ShipV2: Ship index={ShipIndex}, calculated shadow index={ShadowIndex}", 
                    shipIndex, shadowIndex);
                string shadowPath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(prmPath) ?? "",
                    "..",
                    "textures",
                    $"shad{shadowIndex}.tim"
                );
                _logger.LogInformation("ShipV2: Attempting to load shadow texture from: {ShadowPath}", 
                    System.IO.Path.GetFullPath(shadowPath));
                LoadShadowTexture(shadowPath);
            }
            else
            {
                _logger.LogWarning("No PRM file found for ships");
            }
        }

        public void ShipInit(TrackSection? section, int pilot, int position)
        {
            if (section != null)
            {
                Position = section.Center;
                SectionNum = section.SectionNumber;
                TotalSectionNum = section.SectionNumber;
            }

            Velocity = new Vec3(0, 0, 0);
            Angle = new Vec3(0, 0, 0);
        }

        public void InitExhaustPlume()
        {
            // Stub
        }

        public void ResetExhaustPlume()
        {
            // Stub
        }

        /// <summary>
        /// Calculate transformation matrix from position and rotation.
        /// Used for rendering the ship model at the correct position/orientation.
        /// </summary>
        public Mat4 CalculateTransformMatrix()
        {
            return Mat4.FromPositionAndAngles(Position, Angle);
        }

        /*
         Render the ship model using the provided renderer.
         Order:
            F3 (flat triangles, sem cor)
            F4 (flat quads, sem cor)
            FT3 (flat textured triangles, uma cor + textura)
            FT4 (flat textured quads, uma cor + textura)
            G3 (Gouraud triangles, cores por vértice, sem textura)
            G4 (Gouraud quads, cores por vértice, sem textura)
            GT3 (Gouraud textured triangles, cores por vértice + textura)
            GT4 (não renderizado diretamente pois é expandido em GT3 pelo ModelLoader)
        */
        public void Draw()
        {
            // Based on object_draw() from wipeout-rewrite/src/wipeout/object.c
            // Processes all primitives in rendering order: GT3, GT4, FT3, FT4, G3, G4, F3, F4
            if (!IsVisible || Model == null)
                return;

            Mat4 transformMatrix = CalculateTransformMatrix();

            // Count primitives by type and flags for debugging
            var typeCount = new Dictionary<string, int>();
            int singleSidedCount = 0;
            int doubleSidedCount = 0;

            // Render in two passes to avoid z-fighting:
            // Pass 1: Non-textured primitives (G3, G4) - render FIRST (background)
            // Pass 2: Textured primitives (GT3, FT3, FT4, F3, F4) - render LAST (foreground)
            // This prevents black G3/G4 from covering colored GT3 engine exhausts
            
            // PASS 1: Non-textured primitives (G3, G4) - background layer
            foreach (var primitive in Model.Primitives)
            {
                bool isPrimitiveEngine = (primitive.Flags & PrimitiveFlags.SHIP_ENGINE) != 0;
                _renderer.SetFaceCulling(false);
                
                switch (primitive)
                {
                    case G3 g3:
                        // Skip G3 if it has ENGINE flag (C code dies if this happens)
                        if (!isPrimitiveEngine)
                            RenderG3(_renderer, g3, Model.Vertices, transformMatrix);
                        break;

                    case G4 g4:
                        // Skip G4 if it has ENGINE flag (C code dies if this happens)
                        if (!isPrimitiveEngine)
                            RenderG4(_renderer, g4, Model.Vertices, transformMatrix);
                        break;
                        
                    default:
                        // Skip all other primitive types in pass 1
                        break;
                }
            }
            
            // PASS 2: Textured primitives - foreground layer
            foreach (var primitive in Model.Primitives)
            {
                string typeStr = primitive.GetType().Name;
                if (!typeCount.ContainsKey(typeStr))
                    typeCount[typeStr] = 0;
                typeCount[typeStr]++;

                // Check PRM_SINGLE_SIDED flag for statistics
                bool isSingleSided = (primitive.Flags & PrimitiveFlags.SINGLE_SIDED) != 0;
                bool isEngine = (primitive.Flags & PrimitiveFlags.SHIP_ENGINE) != 0;
                
                if (isSingleSided)
                    singleSidedCount++;
                else
                    doubleSidedCount++;
                
                // WORKAROUND: Disable culling for ships due to mixed winding orders in PRM files
                // Some primitives (wings) have inverted vertex order
                // C original has same issue - TODO comment (line 465 object.c) shows PRM_SINGLE_SIDED not implemented
                // So C effectively renders all ships double-sided despite culling being globally enabled
                // Depth test (now enabled in GLRenderer.Init) prevents seeing through the ship
                _renderer.SetFaceCulling(false);

                // Rendering order from C implementation (object.c):
                // Case statement order: GT3, GT4, FT3, FT4, G3, G4, F3, F4
                // Note: GT4 is expanded to GT3 pairs by ModelLoader, so we only see GT3 here
                switch (primitive)
                {
                    case GT3 gt3:
                        RenderGT3(_renderer, gt3, Model.Vertices, transformMatrix, isEngine);
                        break;

                    case FT3 ft3:
                        RenderFT3(_renderer, ft3, Model.Vertices, transformMatrix);
                        break;

                    case FT4 ft4:
                        RenderFT4(_renderer, ft4, Model.Vertices, transformMatrix);
                        break;

                    case G3 g3:
                        // Skip in pass 2, already rendered in pass 1
                        break;

                    case G4 g4:
                        // Skip in pass 2, already rendered in pass 1
                        break;

                    case F3 f3:
                        RenderF3(_renderer, f3, Model.Vertices, transformMatrix);
                        break;

                    case F4 f4:
                        RenderF4(_renderer, f4, Model.Vertices, transformMatrix);
                        break;
                }
            }
            
            // Log primitive statistics
            int enginePrimitives = Model.Primitives.Count(p => (p.Flags & PrimitiveFlags.SHIP_ENGINE) != 0);
            _logger.LogDebug("Ship '{Name}' primitives: {Total} total, {SingleSided} single-sided, {DoubleSided} double-sided, {Engine} engine",
                Name, Model.Primitives.Count, singleSidedCount, doubleSidedCount, enginePrimitives);
        }

        /// <summary>
        /// Transform 3D vertex to world space using model matrix.
        /// This is used for full 3D perspective rendering (not isometric 2D).
        /// The GPU will handle the projection from 3D to 2D screen space.
        /// </summary>
        private static OpenTK.Mathematics.Vector3 TransformTo3D(Vec3 vertex, Mat4 transform)
        {
            // Apply model transformation (rotation + translation)
            Vec3 transformed = transform.TransformPoint(vertex);
            
            // Return 3D world coordinates - GPU will apply view and projection matrices
            return new OpenTK.Mathematics.Vector3(transformed.X, transformed.Y, transformed.Z);
        }

        /// <summary>
        /// Render a flat textured triangle (FT3).
        /// </summary>
        private static void RenderFT3(IRenderer renderer, FT3 primitive, Vec3[] vertices, Mat4 transform)
        {
            // Get vertices
            Vec3 v0 = vertices[primitive.CoordIndices[0]];
            Vec3 v1 = vertices[primitive.CoordIndices[1]];
            Vec3 v2 = vertices[primitive.CoordIndices[2]];
            
            // Use primitive color from model
            var color = new OpenTK.Mathematics.Vector4(
                primitive.Color.r / 255f,
                primitive.Color.g / 255f,
                primitive.Color.b / 255f,
                primitive.Color.a / 255f
            );
            
            // Apply texture if available
            int texHandle = primitive.TextureHandle > 0 ? primitive.TextureHandle : renderer.WhiteTexture;
            
            // Get UV coordinates from primitive (normalized 0..1)
            var uv0 = (primitive.UVsF != null && primitive.UVsF.Length > 0)
                ? new OpenTK.Mathematics.Vector2(primitive.UVsF[0].u, primitive.UVsF[0].v)
                : new OpenTK.Mathematics.Vector2(0.5f, 0.5f);
            var uv1 = (primitive.UVsF != null && primitive.UVsF.Length > 1)
                ? new OpenTK.Mathematics.Vector2(primitive.UVsF[1].u, primitive.UVsF[1].v)
                : new OpenTK.Mathematics.Vector2(0.5f, 0.5f);
            var uv2 = (primitive.UVsF != null && primitive.UVsF.Length > 2)
                ? new OpenTK.Mathematics.Vector2(primitive.UVsF[2].u, primitive.UVsF[2].v)
                : new OpenTK.Mathematics.Vector2(0.5f, 0.5f);

            renderer.SetCurrentTexture(texHandle);
            
            renderer.PushTri(
                TransformTo3D(v2, transform),
                uv2,
                color,
                TransformTo3D(v1, transform),
                uv1,
                color,
                TransformTo3D(v0, transform),
                uv0,
                color
            );
        }
        
        /// <summary>
        /// Render a Gouraud textured triangle (GT3).
        /// </summary>
        private static void RenderGT3(IRenderer renderer, GT3 primitive, Vec3[] vertices, Mat4 transform, bool isEngine = false)
        {
            // Similar to FT3 but with per-vertex colors
            Vec3 v0 = vertices[primitive.CoordIndices[0]];
            Vec3 v1 = vertices[primitive.CoordIndices[1]];
            Vec3 v2 = vertices[primitive.CoordIndices[2]];
            
            // Apply texture if available
            // Engine primitives use their original texture but with overridden vertex colors
            int texHandle = primitive.TextureHandle > 0 ? primitive.TextureHandle : renderer.WhiteTexture;

            // Get UV coordinates from primitive (normalized 0..1)
            var uv0 = (primitive.UVsF != null && primitive.UVsF.Length > 0)
                ? new OpenTK.Mathematics.Vector2(primitive.UVsF[0].u, primitive.UVsF[0].v)
                : new OpenTK.Mathematics.Vector2(0.5f, 0.5f);
            var uv1 = (primitive.UVsF != null && primitive.UVsF.Length > 1)
                ? new OpenTK.Mathematics.Vector2(primitive.UVsF[1].u, primitive.UVsF[1].v)
                : new OpenTK.Mathematics.Vector2(0.5f, 0.5f);
            var uv2 = (primitive.UVsF != null && primitive.UVsF.Length > 2)
                ? new OpenTK.Mathematics.Vector2(primitive.UVsF[2].u, primitive.UVsF[2].v)
                : new OpenTK.Mathematics.Vector2(0.5f, 0.5f);

            // Per-vertex colors for Gouraud shading
            // Special handling for PRM_SHIP_ENGINE: override colors to make exhaust visible
            // Based on ship.c lines 330-332: engines get RGB(180,97,120) with alpha=140
            OpenTK.Mathematics.Vector4 c0, c1, c2;
            if (isEngine)
            {
                // Engine exhaust glow color from C code - exact values
                // RGB(180,97,120) = warm pinkish-orange exhaust glow
                c0 = c1 = c2 = new OpenTK.Mathematics.Vector4(
                    180f/255f,  // 0.706 red
                    97f/255f,   // 0.380 green  
                    120f/255f,  // 0.471 blue
                    1.0f        // Full alpha
                );
            }
            else
            {
                c0 = new OpenTK.Mathematics.Vector4(primitive.Colors[0].r / 255f, primitive.Colors[0].g / 255f, primitive.Colors[0].b / 255f, primitive.Colors[0].a / 255f);
                c1 = new OpenTK.Mathematics.Vector4(primitive.Colors[1].r / 255f, primitive.Colors[1].g / 255f, primitive.Colors[1].b / 255f, primitive.Colors[1].a / 255f);
                c2 = new OpenTK.Mathematics.Vector4(primitive.Colors[2].r / 255f, primitive.Colors[2].g / 255f, primitive.Colors[2].b / 255f, primitive.Colors[2].a / 255f);
            }

            renderer.SetCurrentTexture(texHandle);
            renderer.PushTri(
                TransformTo3D(v2, transform),
                uv2,
                c2,
                TransformTo3D(v1, transform),
                uv1,
                c1,
                TransformTo3D(v0, transform),
                uv0,
                c0
            );
        }

        /// <summary>
        /// Render a flat triangle (F3) - solid color, no texture.
        /// </summary>
        private static void RenderF3(IRenderer renderer, F3 primitive, Vec3[] vertices, Mat4 transform)
        {
            Vec3 v0 = vertices[primitive.CoordIndices[0]];
            Vec3 v1 = vertices[primitive.CoordIndices[1]];
            Vec3 v2 = vertices[primitive.CoordIndices[2]];
            
            // Use primitive color from model
            var color = new OpenTK.Mathematics.Vector4(
                primitive.Color.r / 255f,
                primitive.Color.g / 255f,
                primitive.Color.b / 255f,
                primitive.Color.a / 255f
            );
            
            renderer.SetCurrentTexture(renderer.WhiteTexture);
            renderer.PushTri(
                TransformTo3D(v0, transform),
                new OpenTK.Mathematics.Vector2(0.5f, 0.5f), // Center for solid color
                color,
                TransformTo3D(v1, transform),
                new OpenTK.Mathematics.Vector2(0.5f, 0.5f),
                color,
                TransformTo3D(v2, transform),
                new OpenTK.Mathematics.Vector2(0.5f, 0.5f),
                color
            );
        }

        /// <summary>
        /// Render a flat textured quad (FT4).
        /// Quads are split into 2 triangles: (v2,v1,v0) + (v2,v3,v1)
        /// </summary>
        private static void RenderFT4(IRenderer renderer, FT4 primitive, Vec3[] vertices, Mat4 transform)
        {
            Vec3 v0 = vertices[primitive.CoordIndices[0]];
            Vec3 v1 = vertices[primitive.CoordIndices[1]];
            Vec3 v2 = vertices[primitive.CoordIndices[2]];
            Vec3 v3 = vertices[primitive.CoordIndices[3]];
            
            // Use primitive color from model (same for all vertices)
            var color = new OpenTK.Mathematics.Vector4(
                primitive.Color.r / 255f,
                primitive.Color.g / 255f,
                primitive.Color.b / 255f,
                primitive.Color.a / 255f
            );
            
            // Apply texture if available
            int texHandle = primitive.TextureHandle > 0 ? primitive.TextureHandle : renderer.WhiteTexture;
            
            // Get UV coordinates from primitive
            var uv0 = (primitive.UVsF != null && primitive.UVsF.Length > 0)
                ? new OpenTK.Mathematics.Vector2(primitive.UVsF[0].u, primitive.UVsF[0].v)
                : new OpenTK.Mathematics.Vector2(0.5f, 0.5f);
            var uv1 = (primitive.UVsF != null && primitive.UVsF.Length > 1)
                ? new OpenTK.Mathematics.Vector2(primitive.UVsF[1].u, primitive.UVsF[1].v)
                : new OpenTK.Mathematics.Vector2(0.5f, 0.5f);
            var uv2 = (primitive.UVsF != null && primitive.UVsF.Length > 2)
                ? new OpenTK.Mathematics.Vector2(primitive.UVsF[2].u, primitive.UVsF[2].v)
                : new OpenTK.Mathematics.Vector2(0.5f, 0.5f);
            var uv3 = (primitive.UVsF != null && primitive.UVsF.Length > 3)
                ? new OpenTK.Mathematics.Vector2(primitive.UVsF[3].u, primitive.UVsF[3].v)
                : new OpenTK.Mathematics.Vector2(0.5f, 0.5f);

            renderer.SetCurrentTexture(texHandle);
            
            // First triangle: (v2, v1, v0)
            renderer.PushTri(
                TransformTo3D(v2, transform),
                uv2,
                color,
                TransformTo3D(v1, transform),
                uv1,
                color,
                TransformTo3D(v0, transform),
                uv0,
                color
            );
            
            // Second triangle: (v2, v3, v1)
            renderer.PushTri(
                TransformTo3D(v2, transform),
                uv2,
                color,
                TransformTo3D(v3, transform),
                uv3,
                color,
                TransformTo3D(v1, transform),
                uv1,
                color
            );
        }

        /// <summary>
        /// Render a flat quad (F4) - solid color, no texture.
        /// Quads are split into 2 triangles: (v2,v1,v0) + (v2,v3,v1)
        /// </summary>
        private static void RenderF4(IRenderer renderer, F4 primitive, Vec3[] vertices, Mat4 transform)
        {
            Vec3 v0 = vertices[primitive.CoordIndices[0]];
            Vec3 v1 = vertices[primitive.CoordIndices[1]];
            Vec3 v2 = vertices[primitive.CoordIndices[2]];
            Vec3 v3 = vertices[primitive.CoordIndices[3]];
            
            // Use primitive color from model
            var color = new OpenTK.Mathematics.Vector4(
                primitive.Color.r / 255f,
                primitive.Color.g / 255f,
                primitive.Color.b / 255f,
                primitive.Color.a / 255f
            );
            
            var centerUV = new OpenTK.Mathematics.Vector2(0.5f, 0.5f);
            
            renderer.SetCurrentTexture(renderer.WhiteTexture);
            // First triangle: (v2, v1, v0)
            renderer.PushTri(
                TransformTo3D(v2, transform),
                centerUV,
                color,
                TransformTo3D(v1, transform),
                centerUV,
                color,
                TransformTo3D(v0, transform),
                centerUV,
                color
            );
            
            // Second triangle: (v2, v3, v1)
            renderer.PushTri(
                TransformTo3D(v2, transform),
                centerUV,
                color,
                TransformTo3D(v3, transform),
                centerUV,
                color,
                TransformTo3D(v1, transform),
                centerUV,
                color
            );
        }

        /// <summary>
        /// Render a Gouraud triangle (G3) - per-vertex colors, no texture.
        /// </summary>
        private static void RenderG3(IRenderer renderer, G3 primitive, Vec3[] vertices, Mat4 transform)
        {
            Vec3 v0 = vertices[primitive.CoordIndices[0]];
            Vec3 v1 = vertices[primitive.CoordIndices[1]];
            Vec3 v2 = vertices[primitive.CoordIndices[2]];
            
            // Per-vertex colors for Gouraud shading
            var c0 = new OpenTK.Mathematics.Vector4(primitive.Colors[0].r / 255f, primitive.Colors[0].g / 255f, primitive.Colors[0].b / 255f, primitive.Colors[0].a / 255f);
            var c1 = new OpenTK.Mathematics.Vector4(primitive.Colors[1].r / 255f, primitive.Colors[1].g / 255f, primitive.Colors[1].b / 255f, primitive.Colors[1].a / 255f);
            var c2 = new OpenTK.Mathematics.Vector4(primitive.Colors[2].r / 255f, primitive.Colors[2].g / 255f, primitive.Colors[2].b / 255f, primitive.Colors[2].a / 255f);
            
            var centerUV = new OpenTK.Mathematics.Vector2(0.5f, 0.5f);
            
            renderer.SetCurrentTexture(renderer.WhiteTexture);
            renderer.PushTri(
                TransformTo3D(v2, transform),
                centerUV,
                c2,
                TransformTo3D(v1, transform),
                centerUV,
                c1,
                TransformTo3D(v0, transform),
                centerUV,
                c0
            );
        }

        /// <summary>
        /// Render a Gouraud quad (G4) - per-vertex colors, no texture.
        /// Quads are split into 2 triangles: (v2,v1,v0) + (v2,v3,v1)
        /// </summary>
        private static void RenderG4(IRenderer renderer, G4 primitive, Vec3[] vertices, Mat4 transform)
        {
            Vec3 v0 = vertices[primitive.CoordIndices[0]];
            Vec3 v1 = vertices[primitive.CoordIndices[1]];
            Vec3 v2 = vertices[primitive.CoordIndices[2]];
            Vec3 v3 = vertices[primitive.CoordIndices[3]];
            
            // Per-vertex colors
            var c0 = new OpenTK.Mathematics.Vector4(primitive.Colors[0].r / 255f, primitive.Colors[0].g / 255f, primitive.Colors[0].b / 255f, primitive.Colors[0].a / 255f);
            var c1 = new OpenTK.Mathematics.Vector4(primitive.Colors[1].r / 255f, primitive.Colors[1].g / 255f, primitive.Colors[1].b / 255f, primitive.Colors[1].a / 255f);
            var c2 = new OpenTK.Mathematics.Vector4(primitive.Colors[2].r / 255f, primitive.Colors[2].g / 255f, primitive.Colors[2].b / 255f, primitive.Colors[2].a / 255f);
            var c3 = new OpenTK.Mathematics.Vector4(primitive.Colors[3].r / 255f, primitive.Colors[3].g / 255f, primitive.Colors[3].b / 255f, primitive.Colors[3].a / 255f);
            
            var centerUV = new OpenTK.Mathematics.Vector2(0.5f, 0.5f);
            
            renderer.SetCurrentTexture(renderer.WhiteTexture);
            // First triangle: (v2, v1, v0)
            renderer.PushTri(
                TransformTo3D(v2, transform),
                centerUV,
                c2,
                TransformTo3D(v1, transform),
                centerUV,
                c1,
                TransformTo3D(v0, transform),
                centerUV,
                c0
            );
            
            // Second triangle: (v2, v3, v1)
            renderer.PushTri(
                TransformTo3D(v2, transform),
                centerUV,
                c2,
                TransformTo3D(v3, transform),
                centerUV,
                c3,
                TransformTo3D(v1, transform),
                centerUV,
                c1
            );
        }

        /// <summary>
        /// Render ship shadow beneath the ship.
        /// Based on the Wipeout original implementation that uses semi-transparent shadow textures.
        /// In the C version (ship.c), shadow textures (shad1.tim - shad4.tim) are loaded and 
        /// rendered beneath the ship projected onto the track.
        /// 
        /// For isometric 2D rendering, we simulate this by rendering a semi-transparent
        /// dark quad beneath the ship at track level.
        /// </summary>
        public void RenderShadow()
        {
            if (!IsVisible || Model == null)
                return;

            // Based on ship.c ship_draw_shadow():
            // Shadow is a single triangle from nose to wing tips, projected onto ground
            // In C original, uses shadow texture (shad1.tim-shad4.tim) with ship silhouette
            // For now, render simple triangle shadow until we load shadow textures
            
            // Calculate shadow triangle points like C original:
            // nose = position + forward * 384
            // wingLeft = position - right * 256 - forward * 384
            // wingRight = position + right * 256 - forward * 384
            Vec3 nose = Position.Add(DirForward.Multiply(384));
            Vec3 wingLeft = Position.Subtract(DirRight.Multiply(256)).Subtract(DirForward.Multiply(384));
            Vec3 wingRight = Position.Add(DirRight.Multiply(256)).Subtract(DirForward.Multiply(384));
            
            // Project shadows down to ground level
            float groundOffset = -200f; // Shadow below ship
            Vec3 shadowNose = new Vec3(nose.X, groundOffset, nose.Z);
            Vec3 shadowWingLeft = new Vec3(wingLeft.X, groundOffset, wingLeft.Z);
            Vec3 shadowWingRight = new Vec3(wingRight.X, groundOffset, wingRight.Z);

            // Transform to 3D world space (no rotation, just position offset)
            var p1 = TransformTo3D(shadowWingLeft, Mat4.Identity());
            var p2 = TransformTo3D(shadowWingRight, Mat4.Identity());
            var p3 = TransformTo3D(shadowNose, Mat4.Identity());

            // Shadow color: semi-transparent black (rgba(0,0,0,128) in C)
            var shadowColor = new OpenTK.Mathematics.Vector4(0.0f, 0.0f, 0.0f, 0.5f);
            
            // UVs from C original (ship.c line 430-438):
            // wingLeft: (0, 256), wingRight: (128, 256), nose: (64, 0)
            // Normalized to 0.0-1.0 range (assuming 128x256 texture)
            var uvWingLeft = new OpenTK.Mathematics.Vector2(0.0f, 1.0f);
            var uvWingRight = new OpenTK.Mathematics.Vector2(1.0f, 1.0f);
            var uvNose = new OpenTK.Mathematics.Vector2(0.5f, 0.0f);

            // Use shadow texture if loaded, otherwise white texture
            int texHandle = (ShadowTexture >= 0) ? ShadowTexture : _renderer.WhiteTexture;
            _renderer.SetCurrentTexture(texHandle);
            
            // Render shadow as single triangle (like C original)
            // Order: wingLeft, wingRight, nose
            _renderer.PushTri(
                p1,
                uvWingLeft,
                shadowColor,
                p2,
                uvWingRight,
                shadowColor,
                p3,
                uvNose,
                shadowColor
            );
        }

        public void Update()
        {
            // Minimal: recalc direction vectors and integrate velocity (no timing here)
            float sx = MathF.Sin(Angle.X);
            float cx = MathF.Cos(Angle.X);
            float sy = MathF.Sin(Angle.Y);
            float cy = MathF.Cos(Angle.Y);
            float sz = MathF.Sin(Angle.Z);
            float cz = MathF.Cos(Angle.Z);

            DirForward = new Vec3(
                -(sy * cx),
                -sx,
                (cy * cx)
            );

            DirRight = new Vec3(
                (cy * cz) + (sy * sz * sx),
                -(sz * cx),
                (sy * cz) - (cy * sx * sz)
            );

            DirUp = new Vec3(
                (cy * sz) - (sy * sx * cz),
                -(cx * cz),
                (sy * sz) + (cy * sx * cz)
            );

            Position = Position.Add(Velocity);
        }

        public void CollideWithTrack(TrackFace face)
        {
            // Simplified collision handling
            Velocity = new Vec3(Velocity.X, 0, Velocity.Z);
        }

        public void CollideWithShip(ShipV2 other)
        {
            var avg = this.Velocity.Add(other.Velocity).Multiply(0.5f);
            this.Velocity = avg;
            other.Velocity = avg;
        }

        public Vec3 GetCockpitPosition()
        {
            return Position.Add(DirUp.Multiply(128));
        }

        public Vec3 GetNosePosition()
        {
            return Position.Add(DirForward.Multiply(512));
        }

        public Vec3 GetWingLeftPosition()
        {
            return Position.Subtract(DirRight.Multiply(256)).Subtract(DirForward.Multiply(256));
        }

        public Vec3 GetWingRightPosition()
        {
            return Position.Add(DirRight.Multiply(256)).Subtract(DirForward.Multiply(256));
        }


        /// <summary>
        /// Load PRM Model
        /// </summary>
        private void LoadPrm(string prmPath, int objectIndex = 0)
        {
            if (string.IsNullOrEmpty(prmPath))
                throw new ArgumentException("prmPath não pode ser nulo ou vazio", nameof(prmPath));

            _logger.LogInformation("ShipV2: carregando PRM de {Path} (objectIndex={Index})", prmPath, objectIndex);

            // Reuse the existing ModelLoader implementation
            var loaderLogger = _logger as ILogger<ModelLoader>;
            var loader = new ModelLoader(loaderLogger);
            var mesh = loader.LoadFromPrmFile(prmPath, objectIndex);

            Model = mesh;
            _logger.LogInformation("ShipV2: PRM carregado: {Name} vertices={Count} prims={PrimCount}", mesh.Name, mesh.Vertices?.Length ?? 0, mesh.Primitives?.Count ?? 0);
        }

        private void LoadCmpTextures(string cmpPath)
        {
            if (string.IsNullOrEmpty(cmpPath))
                throw new ArgumentException("cmpPath não pode ser nulo ou vazio", nameof(cmpPath));

            _logger.LogInformation("ShipV2: carregando CMP de {Path}", cmpPath);

            int[] handles = _textureManager.LoadTexturesFromCmp(cmpPath);

            Texture = handles;
            _logger.LogInformation("ShipV2: CMP carregado: {Count} texturas", handles.Length);
            
            // Map texture handles to primitives and normalize UVs by texture size
            if (Model != null && handles.Length > 0)
            {
                int mappedCount = 0;
                foreach (var primitive in Model.Primitives)
                {
                    if (primitive is FT3 ft3 && ft3.TextureId >= 0 && ft3.TextureId < handles.Length)
                    {
                        ft3.TextureHandle = handles[ft3.TextureId];
                        // Normalize UVs by actual texture size
                        if (handles[ft3.TextureId] > 0)
                        {
                            var (width, height) = _textureManager.GetTextureSize(handles[ft3.TextureId]);
                            for (int i = 0; i < ft3.UVs.Length; i++)
                            {
                                float u = ft3.UVs[i].u / (float)width;
                                float v = ft3.UVs[i].v / (float)height;
                                ft3.UVsF[i] = (u, v);
                            }
                        }
                        mappedCount++;
                    }
                    else if (primitive is FT4 ft4 && ft4.TextureId >= 0 && ft4.TextureId < handles.Length)
                    {
                        ft4.TextureHandle = handles[ft4.TextureId];
                        // Normalize UVs by actual texture size
                        if (handles[ft4.TextureId] > 0)
                        {
                            var (width, height) = _textureManager.GetTextureSize(handles[ft4.TextureId]);
                            for (int i = 0; i < ft4.UVs.Length; i++)
                            {
                                float u = ft4.UVs[i].u / (float)width;
                                float v = ft4.UVs[i].v / (float)height;
                                ft4.UVsF[i] = (u, v);
                            }
                        }
                        mappedCount++;
                    }
                    else if (primitive is GT3 gt3 && gt3.TextureId >= 0 && gt3.TextureId < handles.Length)
                    {
                        gt3.TextureHandle = handles[gt3.TextureId];
                        // Normalize UVs by actual texture size
                        if (handles[gt3.TextureId] > 0)
                        {
                            var (width, height) = _textureManager.GetTextureSize(handles[gt3.TextureId]);
                            for (int i = 0; i < gt3.UVs.Length; i++)
                            {
                                float u = gt3.UVs[i].u / (float)width;
                                float v = gt3.UVs[i].v / (float)height;
                                gt3.UVsF[i] = (u, v);
                            }
                        }
                        mappedCount++;
                    }
                }
                _logger.LogInformation("ShipV2: Mapeadas {Count} texturas para {Total} primitivos texturizados", 
                    mappedCount, Model.Primitives.Count(p => p is FT3 or FT4 or GT3));
            }
        }

        private void LoadShadowTexture(string shadowPath)
        {
            if (!System.IO.File.Exists(shadowPath))
            {
                _logger.LogWarning("ShipV2: Shadow texture not found at {Path}", shadowPath);
                return;
            }

            try
            {
                _logger.LogInformation("ShipV2: Loading shadow texture from {Path}", shadowPath);
                var timLoader = new TimImageLoader(NullLogger<TimImageLoader>.Instance);
                
                // Load TIM with semi-transparent flag (alpha channel)
                var (pixels, width, height) = timLoader.LoadTim(shadowPath, transparent: true);
                
                // Upload to GPU
                ShadowTexture = _renderer.CreateTexture(pixels, width, height);
                
                _logger.LogInformation("ShipV2: Shadow texture loaded successfully ({Width}x{Height}), handle={Handle}", 
                    width, height, ShadowTexture);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ShipV2: Failed to load shadow texture from {Path}", shadowPath);
                ShadowTexture = -1;
            }
        }

    }
}
