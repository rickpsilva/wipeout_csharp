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
        /// Render the ship 3D model.
        /// TODO: Load and render actual 3D model from PRM file
        /// </summary>
        public void Render(IRenderer renderer)
        {
            if (!IsVisible)
                return;
                
            // TODO: Implement 3D model rendering
            // 1. Load ship model from allsh.prm
            // 2. Apply transformation matrix from position + angle
            // 3. Render model with textures
            // 4. Render exhaust plume effect
            
            _logger?.LogDebug("Rendering ship {Name} at {Pos}", Name, Position);
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
