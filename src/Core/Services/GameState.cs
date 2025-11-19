using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Core.Services
{
    public enum GameMode
    {
        Intro,
        SplashScreen,  // Title screen with "PRESS ENTER" and wiptitle.tim
        Menu,          // Main menu with START GAME, OPTIONS, QUIT
        AttractMode,
        Loading,
        Racing,
        Paused,
        GameOver,
        Victory
    }

    public class GameState
    {
        private readonly ILogger<GameState> _logger;
        
        public GameMode CurrentMode { get; set; }
        public Track? CurrentTrack { get; set; }
        public List<Ship> Ships { get; set; }
        public Ship? PlayerShip { get; set; }

        // Dados de corrida
        public int LapNumber { get; set; }
        public float RaceTime { get; set; }
        public int Position { get; set; } // Player position in race
        public int TotalPlayers { get; set; }

        // Settings
        public int Difficulty { get; set; }
        public int GameSpeed { get; set; }
        public bool EnableAI { get; set; }

        public GameState(ILogger<GameState> logger)
        {
            _logger = logger;
            CurrentMode = GameMode.Menu;
            Ships = new List<Ship>();
            LapNumber = 1;
            RaceTime = 0;
            Position = 1;
            TotalPlayers = 8;
            Difficulty = 1;
            GameSpeed = 1;
            EnableAI = true;
        }

        public void Initialize(Track track, int playerShipId = 0)
        {
            CurrentTrack = track;
            CurrentMode = GameMode.Loading;
            Ships.Clear();
            LapNumber = 1;
            RaceTime = 0;

            // Criar naves IA
            for (int i = 0; i < TotalPlayers; i++)
            {
                var ship = new Ship($"Ship_{i}", i);
                if (i == playerShipId)
                {
                    PlayerShip = ship;
                }
                Ships.Add(ship);
            }

            _logger.LogInformation("Game initialized with track: {TrackName}, {ShipCount} ships", track.Name, Ships.Count);
        }

        public void Update(float deltaTime)
        {
            if (CurrentMode != GameMode.Racing)
                return;

            RaceTime += deltaTime;

            // Atualizar todas as naves
            foreach (var ship in Ships)
            {
                ship.Update(deltaTime);
            }

            // TODO: collision logic, AI, checkpoints
        }

        public void Render(GLRenderer renderer)
        {
            if (CurrentTrack != null)
            {
                CurrentTrack.Render(renderer);
            }

            foreach (var ship in Ships)
            {
                ship.Render(renderer);
            }
        }
    }
}
