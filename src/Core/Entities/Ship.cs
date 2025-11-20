using System;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Infrastructure.Graphics;

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
            if (!IsVisible)
                return;
                
            // Calculate transformation matrix
            Mat4 transformMatrix = CalculateTransformMatrix();
            
            // TODO: When model loading is implemented:
            // 1. Load ship model from allsh.prm (done once at startup)
            // 2. Call object_draw(model, &transformMatrix)
            // 3. Render exhaust plume effect
            
            _logger?.LogDebug("Rendering ship {Name} at {Pos} with transform matrix", 
                Name, Position);
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
