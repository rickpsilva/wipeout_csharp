using System;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Core.Graphics;

namespace WipeoutRewrite.Core.Entities
{
    /// <summary>
    /// Ship entity - represents a racing ship in the game.
    /// Based on wipeout-rewrite/src/wipeout/ship.c
    /// </summary>
    public class Ship
    {
        // Physics constants (from ship.h)
        private const float ShipFlyingGravity = 80000.0f;
        private const float ShipOnTrackGravity = 30000.0f;
        private const float ShipMinResistance = 20.0f;
        private const float ShipMaxResistance = 74.0f;
        private const float ShipTrackMagnet = 64.0f;
        private const float ShipTrackFloat = 256.0f;
        
        private readonly ILogger<Ship>? _logger;
        
        /// <summary>
        /// 3D model for rendering (loaded from PRM file)
        /// </summary>
        public Mesh? Model { get; set; }
        
        // Identification
        public string Name { get; set; } = "";
        public int ShipId { get; set; }
        public int Pilot { get; set; }

        // Position and orientation
        public Vec3 Position { get; set; }
        public Vec3 Angle { get; set; } // Rotation angles (pitch, yaw, roll)
        public Vec3 Velocity { get; set; }
        public Vec3 Acceleration { get; set; }
        public Vec3 AngularVelocity { get; set; }
        public Vec3 AngularAcceleration { get; set; }
        
        // Direction vectors (calculated from angles)
        public Vec3 DirForward { get; private set; }
        public Vec3 DirRight { get; private set; }
        public Vec3 DirUp { get; private set; }

        // Flight properties
        public float Speed { get; set; }
        public float Mass { get; set; }
        public float ThrustMag { get; set; }
        public float ThrustMax { get; set; }
        public float CurrentThrustMax { get; set; }
        public float TurnRate { get; set; }
        public float TurnRateMax { get; set; }
        public float Resistance { get; set; }
        public float Skid { get; set; }
        public float BrakeLeft { get; set; }
        public float BrakeRight { get; set; }
        
        // Shield and damage
        public int Shield { get; set; }
        public int MaxShield { get; set; }
        public bool IsDestroyed { get; set; }

        // Weapons
        public int WeaponType { get; set; }
        
        // State flags
        public bool IsFlying { get; set; }
        public bool IsRacing { get; set; }
        public bool IsVisible { get; set; }
        
        // Track section
        public int SectionNum { get; set; }
        public int TotalSectionNum { get; set; }

        // Player input (for player-controlled ships)
        // TODO: Move to separate input handler class
        public bool InputAccelerate { get; set; }
        public bool InputBrake { get; set; }
        public bool InputTurnLeft { get; set; }
        public bool InputTurnRight { get; set; }
        public bool InputBoostLeft { get; set; }
        public bool InputBoostRight { get; set; }
        
        // Ship type
        public ShipClass ShipClass { get; set; }

        public Ship(string name, int shipId, ILogger<Ship>? logger = null)
        {
            _logger = logger;
            Name = name;
            ShipId = shipId;
            Pilot = 0;

            Position = new Vec3(0, 0, 0);
            Angle = new Vec3(0, 0, 0);
            Velocity = new Vec3(0, 0, 0);
            Acceleration = new Vec3(0, 0, 0);
            AngularVelocity = new Vec3(0, 0, 0);
            AngularAcceleration = new Vec3(0, 0, 0);
            
            DirForward = new Vec3(0, 0, 1);
            DirRight = new Vec3(1, 0, 0);
            DirUp = new Vec3(0, 1, 0);

            Speed = 0;
            Mass = 150.0f;
            ThrustMag = 0;
            ThrustMax = 3840.0f;
            CurrentThrustMax = 0;
            TurnRate = 0;
            TurnRateMax = 2.3f;
            Resistance = 0.003f;
            Skid = 0.9f;
            BrakeLeft = 0;
            BrakeRight = 0;

            Shield = 100;
            MaxShield = 100;
            IsDestroyed = false;

            WeaponType = 0;
            
            IsFlying = false;
            IsRacing = true;
            IsVisible = true;
            
            SectionNum = 0;
            TotalSectionNum = 0;

            ShipClass = ShipClass.Unknown;
        }

        /// <summary>
        /// Update ship physics for one frame.
        /// Calculates direction vectors from rotation angles and updates position.
        /// Based on ship_update() from ship.c
        /// </summary>
        public void Update(float deltaTime)
        {
            // Calculate direction vectors from angles (ship.c:454-470)
            UpdateDirectionVectors();
            
            // Update position based on velocity
            Position = Position.Add(Velocity.Multiply(deltaTime));
            
            // Update angles based on angular velocity
            Angle = Angle.Add(AngularVelocity.Multiply(deltaTime));
            
            // Update speed (magnitude of velocity)
            Speed = Velocity.Length();
            
            _logger?.LogDebug("Ship {Name} updated: Pos={Pos}, Speed={Speed:F1}", 
                Name, Position, Speed);
        }
        
        /// <summary>
        /// Calculate forward, right, and up direction vectors from rotation angles.
        /// Uses standard 3D rotation matrix decomposition.
        /// From ship.c lines 454-470
        /// </summary>
        private void UpdateDirectionVectors()
        {
            float sx = MathF.Sin(Angle.X);
            float cx = MathF.Cos(Angle.X);
            float sy = MathF.Sin(Angle.Y);
            float cy = MathF.Cos(Angle.Y);
            float sz = MathF.Sin(Angle.Z);
            float cz = MathF.Cos(Angle.Z);

            // Forward vector
            DirForward = new Vec3(
                -(sy * cx),
                -sx,
                cy * cx
            );

            // Right vector  
            DirRight = new Vec3(
                (cy * cz) + (sy * sz * sx),
                -(sz * cx),
                (sy * cz) - (cy * sx * sz)
            );

            // Up vector
            DirUp = new Vec3(
                (cy * sz) - (sy * sx * cz),
                -(cx * cz),
                (sy * sz) + (cy * sx * cz)
            );
        }

        public void TakeDamage(int damage)
        {
            Shield -= damage;
            if (Shield <= 0)
            {
                IsDestroyed = true;
                _logger?.LogWarning("{ShipName} foi destruída!", Name);
            }
        }

        /// <summary>
        /// Calculate transformation matrix from position and rotation.
        /// Used for rendering the ship model at the correct position/orientation.
        /// </summary>
        public Mat4 CalculateTransformMatrix()
        {
            return Mat4.FromPositionAndAngles(Position, Angle);
        }
        
        /// <summary>
        /// Calculate cockpit position (for camera positioning).
        /// From ship.c: ship_cockpit()
        /// </summary>
        public Vec3 GetCockpitPosition()
        {
            // Cockpit is 150 units forward and 60 units up from ship center
            return Position + DirForward * 150.0f + DirUp * 60.0f;
        }
        
        /// <summary>
        /// Calculate nose position (front of ship).
        /// From ship.c: ship_nose()
        /// </summary>
        public Vec3 GetNosePosition()
        {
            return Position + DirForward * 384.0f;
        }
        
        /// <summary>
        /// Calculate left wing position.
        /// From ship.c: ship_wing_left()
        /// </summary>
        public Vec3 GetWingLeftPosition()
        {
            return Position - DirRight * 256.0f - DirForward * 384.0f;
        }
        
        /// <summary>
        /// Calculate right wing position.
        /// From ship.c: ship_wing_right()
        /// </summary>
        public Vec3 GetWingRightPosition()
        {
            return Position + DirRight * 256.0f - DirForward * 384.0f;
        }
        
        /// <summary>
        /// Render the ship 3D model.
        /// Based on ship_draw() from ship.c
        /// </summary>
        public void Render(IRenderer renderer)
        {
            if (!IsVisible || Model == null)
                return;
                
            // Calculate transformation matrix
            Mat4 transformMatrix = CalculateTransformMatrix();
            
            // Render each primitive (triangle) of the model
            foreach (var primitive in Model.Primitives)
            {
                if (primitive is FT3 ft3)
                {
                    RenderFT3(renderer, ft3, Model.Vertices, transformMatrix);
                }
                else if (primitive is GT3 gt3)
                {
                    RenderGT3(renderer, gt3, Model.Vertices, transformMatrix);
                }
                else if (primitive is F3 f3)
                {
                    RenderF3(renderer, f3, Model.Vertices, transformMatrix);
                }
            }
            
            _logger?.LogDebug("Rendered ship {Name} with {PrimCount} primitives", 
                Name, Model.Primitives.Count);
        }
        
        /// <summary>
        /// Render a flat textured triangle (FT3).
        /// </summary>
        private void RenderFT3(IRenderer renderer, FT3 primitive, Vec3[] vertices, Mat4 transform)
        {
            // Get vertices
            Vec3 v0 = vertices[primitive.CoordIndices[0]];
            Vec3 v1 = vertices[primitive.CoordIndices[1]];
            Vec3 v2 = vertices[primitive.CoordIndices[2]];
            
            // Transform vertices by model matrix
            // TODO: Implement Mat4 * Vec3 transformation
            
            // For now, render at ship position (simplified)
            var color = new OpenTK.Mathematics.Vector4(
                primitive.Color.r / 255f,
                primitive.Color.g / 255f,
                primitive.Color.b / 255f,
                primitive.Color.a / 255f
            );
            
            renderer.PushTri(
                new OpenTK.Mathematics.Vector3(v0.X + Position.X, v0.Y + Position.Y, v0.Z + Position.Z),
                new OpenTK.Mathematics.Vector2(primitive.UVs[0].u, primitive.UVs[0].v),
                color,
                new OpenTK.Mathematics.Vector3(v1.X + Position.X, v1.Y + Position.Y, v1.Z + Position.Z),
                new OpenTK.Mathematics.Vector2(primitive.UVs[1].u, primitive.UVs[1].v),
                color,
                new OpenTK.Mathematics.Vector3(v2.X + Position.X, v2.Y + Position.Y, v2.Z + Position.Z),
                new OpenTK.Mathematics.Vector2(primitive.UVs[2].u, primitive.UVs[2].v),
                color
            );
        }
        
        /// <summary>
        /// Render a Gouraud textured triangle (GT3).
        /// </summary>
        private void RenderGT3(IRenderer renderer, GT3 primitive, Vec3[] vertices, Mat4 transform)
        {
            // Similar to FT3 but with per-vertex colors
            Vec3 v0 = vertices[primitive.CoordIndices[0]];
            Vec3 v1 = vertices[primitive.CoordIndices[1]];
            Vec3 v2 = vertices[primitive.CoordIndices[2]];
            
            renderer.PushTri(
                new OpenTK.Mathematics.Vector3(v0.X + Position.X, v0.Y + Position.Y, v0.Z + Position.Z),
                new OpenTK.Mathematics.Vector2(primitive.UVs[0].u, primitive.UVs[0].v),
                new OpenTK.Mathematics.Vector4(primitive.Colors[0].r / 255f, primitive.Colors[0].g / 255f, 
                    primitive.Colors[0].b / 255f, primitive.Colors[0].a / 255f),
                new OpenTK.Mathematics.Vector3(v1.X + Position.X, v1.Y + Position.Y, v1.Z + Position.Z),
                new OpenTK.Mathematics.Vector2(primitive.UVs[1].u, primitive.UVs[1].v),
                new OpenTK.Mathematics.Vector4(primitive.Colors[1].r / 255f, primitive.Colors[1].g / 255f, 
                    primitive.Colors[1].b / 255f, primitive.Colors[1].a / 255f),
                new OpenTK.Mathematics.Vector3(v2.X + Position.X, v2.Y + Position.Y, v2.Z + Position.Z),
                new OpenTK.Mathematics.Vector2(primitive.UVs[2].u, primitive.UVs[2].v),
                new OpenTK.Mathematics.Vector4(primitive.Colors[2].r / 255f, primitive.Colors[2].g / 255f, 
                    primitive.Colors[2].b / 255f, primitive.Colors[2].a / 255f)
            );
        }
        
        /// <summary>
        /// Render a flat triangle (F3) - solid color, no texture.
        /// </summary>
        private void RenderF3(IRenderer renderer, F3 primitive, Vec3[] vertices, Mat4 transform)
        {
            Vec3 v0 = vertices[primitive.CoordIndices[0]];
            Vec3 v1 = vertices[primitive.CoordIndices[1]];
            Vec3 v2 = vertices[primitive.CoordIndices[2]];
            
            var color = new OpenTK.Mathematics.Vector4(
                primitive.Color.r / 255f,
                primitive.Color.g / 255f,
                primitive.Color.b / 255f,
                primitive.Color.a / 255f
            );
            
            renderer.PushTri(
                new OpenTK.Mathematics.Vector3(v0.X + Position.X, v0.Y + Position.Y, v0.Z + Position.Z),
                new OpenTK.Mathematics.Vector2(0, 0),
                color,
                new OpenTK.Mathematics.Vector3(v1.X + Position.X, v1.Y + Position.Y, v1.Z + Position.Z),
                new OpenTK.Mathematics.Vector2(0, 0),
                color,
                new OpenTK.Mathematics.Vector3(v2.X + Position.X, v2.Y + Position.Y, v2.Z + Position.Z),
                new OpenTK.Mathematics.Vector2(0, 0),
                color
            );
        }
        
        /// <summary>
        /// Render ship shadow projected onto track surface.
        /// Based on ship_draw_shadow() from ship.c
        /// 
        /// Algorithm:
        /// 1. Calculate nose and wing positions in 3D space
        /// 2. Get track face below ship (base face of current section)
        /// 3. Project all three points onto track face using plane projection
        /// 4. Render shadow as semi-transparent triangle
        /// </summary>
        public void RenderShadow(IRenderer renderer)
        {
            if (!IsVisible || IsFlying)
                return;
                
            // Calculate shadow vertices (nose + two wings in 3D space)
            Vec3 nose = GetNosePosition();
            Vec3 wingLeft = GetWingLeftPosition();
            Vec3 wingRight = GetWingRightPosition();
            
            // TODO: Get track face below ship (requires track system integration)
            // For now, assume horizontal plane at Y=0
            Vec3 trackFacePoint = new Vec3(Position.X, 0, Position.Z);
            Vec3 trackNormal = new Vec3(0, 1, 0); // Up vector
            
            // Project all three points onto the track face
            Vec3 noseProjected = nose.ProjectOntoPlane(trackFacePoint, trackNormal);
            Vec3 wingLeftProjected = wingLeft.ProjectOntoPlane(trackFacePoint, trackNormal);
            Vec3 wingRightProjected = wingRight.ProjectOntoPlane(trackFacePoint, trackNormal);
            
            // Render shadow triangle with semi-transparent black color (rgba: 0, 0, 0, 128)
            // UV coordinates from ship.c: wingLeft=(0,256), wingRight=(128,256), nose=(64,0)
            var shadowColor = new OpenTK.Mathematics.Vector4(0f, 0f, 0f, 0.5f); // 128/256 = 0.5 alpha
            
            renderer.PushTri(
                new OpenTK.Mathematics.Vector3(wingLeftProjected.X, wingLeftProjected.Y, wingLeftProjected.Z),
                new OpenTK.Mathematics.Vector2(0, 256),
                shadowColor,
                new OpenTK.Mathematics.Vector3(wingRightProjected.X, wingRightProjected.Y, wingRightProjected.Z),
                new OpenTK.Mathematics.Vector2(128, 256),
                shadowColor,
                new OpenTK.Mathematics.Vector3(noseProjected.X, noseProjected.Y, noseProjected.Z),
                new OpenTK.Mathematics.Vector2(64, 0),
                shadowColor
            );
            
            _logger?.LogDebug("Rendered shadow for ship {Name} at position {Position}", Name, Position);
        }
    }

    public enum ShipClass
    {
        Unknown,
        Phantom,
        Qirex,
        Flash,
        Assegai
    }
}
