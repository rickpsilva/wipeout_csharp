using System;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Core.Entities
{
    /// <summary>
    /// Entidade Ship - representa uma nave no jogo.
    /// </summary>
    public class Ship
    {
        private readonly ILogger<Ship>? _logger;
        
        // Identification
        public string Name { get; set; } = "";
        public int ShipId { get; set; }

        // Position and rotation
        public Vec3 Position { get; set; }
        public Vec3 Rotation { get; set; }
        public Vec3 Velocity { get; set; }
        public Vec3 AngularVelocity { get; set; }

        // Propriedades de voo
        public float Speed { get; set; }
        public float MaxSpeed { get; set; }
        public float Acceleration { get; set; }
        public float SteeringForce { get; set; }
        public float Theta { get; set; } // Angle of inclination

        // Armadura e escudo
        public int Shield { get; set; }
        public int MaxShield { get; set; }
        public bool IsDestroyed { get; set; }

        // Weapons and ammunition
        public int WeaponType { get; set; }
        public int Ammo { get; set; }
        public float FireCooldown { get; set; }

        // Boost/Turbo
        public int BoostEnergy { get; set; }
        public int MaxBoost { get; set; }
        public bool IsBoosting { get; set; }

        // Estado de voo
        public int CurrentTrackSectionIndex { get; set; }
        public float DistanceAlongTrack { get; set; }
        public bool IsAirborne { get; set; }

        // Input do jogador (para naves do jogador)
        public bool InputAccelerate { get; set; }
        public bool InputBrake { get; set; }
        public bool InputTurnLeft { get; set; }
        public bool InputTurnRight { get; set; }
        public bool InputBoostLeft { get; set; }
        public bool InputBoostRight { get; set; }

        // Propriedades da nave (baseadas no tipo)
        public ShipClass ShipClass { get; set; }

        public Ship(string name, int shipId, ILogger<Ship>? logger = null)
        {
            _logger = logger;
            Name = name;
            ShipId = shipId;

            Position = new Vec3(0, 0, 0);
            Rotation = new Vec3(0, 0, 0);
            Velocity = new Vec3(0, 0, 0);
            AngularVelocity = new Vec3(0, 0, 0);

            Speed = 0;
            MaxSpeed = 500;
            Acceleration = 100;
            SteeringForce = 50;
            Theta = 0;

            Shield = 100;
            MaxShield = 100;
            IsDestroyed = false;

            WeaponType = 0;
            Ammo = 100;
            FireCooldown = 0;

            BoostEnergy = 100;
            MaxBoost = 100;
            IsBoosting = false;

            CurrentTrackSectionIndex = 0;
            DistanceAlongTrack = 0;
            IsAirborne = false;

            ShipClass = ShipClass.Unknown;
        }

        public void Update(float deltaTime)
        {
            // TODO: implement update logic (velocity, position, physics)
            if (FireCooldown > 0)
                FireCooldown -= deltaTime;
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

        public void Render(IRenderer renderer)
        {
            // TODO: implement ship rendering
            // Por enquanto apenas um placeholder
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
