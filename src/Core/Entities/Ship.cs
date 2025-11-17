using System;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Core.Entities
{
    /// <summary>
    /// Entidade Ship - representa uma nave no jogo.
    /// </summary>
    public class Ship
    {
        // Identificação
        public string Name { get; set; } = "";
        public int ShipId { get; set; }

        // Posição e rotação
        public Vec3 Position { get; set; }
        public Vec3 Rotation { get; set; }
        public Vec3 Velocity { get; set; }
        public Vec3 AngularVelocity { get; set; }

        // Propriedades de voo
        public float Speed { get; set; }
        public float MaxSpeed { get; set; }
        public float Acceleration { get; set; }
        public float SteeringForce { get; set; }
        public float Theta { get; set; } // Ângulo de inclinação

        // Armadura e escudo
        public int Shield { get; set; }
        public int MaxShield { get; set; }
        public bool IsDestroyed { get; set; }

        // Armas e munições
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

        public Ship(string name, int shipId)
        {
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
            // TODO: implementar lógica de atualização (velocidade, posição, física)
            if (FireCooldown > 0)
                FireCooldown -= deltaTime;
        }

        public void TakeDamage(int damage)
        {
            Shield -= damage;
            if (Shield <= 0)
            {
                IsDestroyed = true;
                Console.WriteLine($"{Name} foi destruída!");
            }
        }

        public void Render(IRenderer renderer)
        {
            // TODO: implementar renderização da nave
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
