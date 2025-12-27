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
        ShipPreview,   // Preview ship before race
        Loading,
        Racing,
        Paused,
        GameOver,
        Victory
    }

    public class GameState : IGameState
    {
        private readonly ILogger<GameState> _logger;

        private readonly IGameObjectCollection _gameObjects;

        private readonly IGameObject _model;
        public GameMode CurrentMode { get; set; }
        public Track CurrentTrack { get; set; } = null!;

        // Dados de corrida
        public int LapNumber { get; set; }
        public float RaceTime { get; set; }
        public int Position { get; set; } // Player position in race
        public int TotalPlayers { get; set; } = 8;

        // Settings
        public int Difficulty { get; set; }
        public int GameSpeed { get; set; }
        public bool EnableAI { get; set; }

        public GameState(
            ILogger<GameState> logger,
            IGameObjectCollection gameObjects,
            IGameObject model
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gameObjects = gameObjects ?? throw new ArgumentNullException(nameof(gameObjects));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            
            CurrentMode = GameMode.Menu;
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
            _gameObjects.Clear();
            LapNumber = 1;
            RaceTime = 0;
            _gameObjects.Init(null);
           
            _logger.LogInformation("Game initialized with track: {TrackName}, {ShipCount} ships", track.Name, _gameObjects.GetAll.Count);
        }

        public void Update(float deltaTime)
        {
            if (CurrentMode != GameMode.Racing)
                return;

            RaceTime += deltaTime;

            _gameObjects.Update();

            // TODO: collision logic, AI, checkpoints
        }

        public void Render(GLRenderer renderer)
        {
            CurrentTrack?.Render(renderer);

           _gameObjects.Renderer();
        }

        public void SetPlayerShip(bool accelerate, bool brake, bool turnLeft, bool turnRight, bool boostLeft, bool boostRight)
        {
            _logger.LogDebug("Setting player ship controls: Accel={Accelerate}, Brake={Brake}, Left={TurnLeft}, Right={TurnRight}, BoostL={BoostLeft}, BoostR={BoostRight}",
                accelerate, brake, turnLeft, turnRight, boostLeft, boostRight);
        }
    }

}
