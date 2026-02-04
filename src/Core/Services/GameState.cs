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

    public enum RaceType
    {
        Championship = 0,
        Single = 1,
        TimeTrial = 2
    }

    public enum Team
    {
        AgSystems = 0,
        Auricom = 1,
        Qirex = 2,
        Feisar = 3
    }

    public enum Pilot
    {
        John_Dekka = 0,
        Daniel_Chang = 1,
        Arial_Tetsuo = 2,
        Anastasia_Cherovoski = 3,
        Kel_Solaar = 4,
        Arian_Tetsuvo = 5,
        Sofia_Del_La_Rent = 6,
        Paul_Jackson = 7
    }

    public class GameState : IGameState
    {
        private readonly ILogger<GameState> _logger;
        private readonly IGameObjectCollection _gameObjects;
        private readonly ITrack? _track;
        private readonly IGameObject _model;

        // Settings references (injected)
        private readonly IVideoSettings _videoSettings;
        private readonly IAudioSettings _audioSettings;
        private readonly IControlsSettings _controlsSettings;

        public GameMode CurrentMode { get; set; }
        public ITrack? CurrentTrack { get; private set; }

        // Menu Selections (mirrors wipeout-rewrite g.race_class, g.team, g.pilot, g.circut, g.race_type)
        public RaceClass SelectedRaceClass { get; set; } = RaceClass.Venom;
        public RaceType SelectedRaceType { get; set; } = RaceType.Single;
        public Team SelectedTeam { get; set; } = Team.Feisar;
        public int SelectedPilot { get; set; } = 0; // 0-7 (2 pilots per team)
        public Circuit SelectedCircuit { get; set; } = Circuit.AltimaVII;
        public bool IsAttractMode { get; set; } = false;

        // Race State (mirrors wipeout-rewrite g.race_time, g.lives, g.race_position, etc.)
        public int LapNumber { get; set; }
        public float RaceTime { get; set; }
        public int Position { get; set; } // Player position in race
        public int TotalPlayers { get; set; } = 8;
        public int Lives { get; set; } = 3;
        public bool IsNewLapRecord { get; set; } = false;
        public bool IsNewRaceRecord { get; set; } = false;
        public float BestLap { get; set; } = 0f;

        // Legacy settings (deprecated - use injected settings instead)
        public int Difficulty { get; set; }
        public int GameSpeed { get; set; }
        public bool EnableAI { get; set; }

        // Settings Access (forwards to injected settings)
        public bool IsFullscreen => _videoSettings.Fullscreen;
        public float MusicVolume => _audioSettings.MusicVolume;
        public float SoundEffectsVolume => _audioSettings.SoundEffectsVolume;

        public GameState(
            ILogger<GameState> logger,
            IGameObjectCollection gameObjects,
            IGameObject model,
            IVideoSettings videoSettings,
            IAudioSettings audioSettings,
            IControlsSettings controlsSettings,
            ITrack? track = null
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gameObjects = gameObjects ?? throw new ArgumentNullException(nameof(gameObjects));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _videoSettings = videoSettings ?? throw new ArgumentNullException(nameof(videoSettings));
            _audioSettings = audioSettings ?? throw new ArgumentNullException(nameof(audioSettings));
            _controlsSettings = controlsSettings ?? throw new ArgumentNullException(nameof(controlsSettings));
            _track = track; // Track is optional, will be loaded when needed
            
            CurrentMode = GameMode.Menu;
            LapNumber = 1;
            RaceTime = 0;
            Position = 1;
            TotalPlayers = 8;
            Lives = 3;
            Difficulty = 1;
            GameSpeed = 1;
            EnableAI = true;

            // Default menu selections
            SelectedRaceClass = RaceClass.Venom;
            SelectedRaceType = RaceType.Single;
            SelectedTeam = Team.Feisar;
            SelectedPilot = 0;
            SelectedCircuit = Circuit.AltimaVII;
        }

        public void Initialize()
        {
            CurrentTrack = GetCurrentTrack();
            CurrentMode = GameMode.Loading;
            _gameObjects.Clear();
            LapNumber = 1;
            RaceTime = 0;
            Lives = 3;
            Position = 1;
            IsNewLapRecord = false;
            IsNewRaceRecord = false;
            BestLap = 0f;
            _gameObjects.Init(null);
           
            _logger.LogInformation("Game initialized: Track={Circuit}, Team={Team}, Pilot={Pilot}, Class={RaceClass}, Type={RaceType}",
                SelectedCircuit,
                SelectedTeam,
                SelectedPilot,
                SelectedRaceClass,
                SelectedRaceType);
        }

        /// <summary>
        /// Reset race state for a new race (called when user completes menu selections and starts race).
        /// </summary>
        public void StartNewRace()
        {
            _logger.LogInformation("Starting new race: {RaceType} on {Circuit} with {Team} (Class: {RaceClass})",
                SelectedRaceType, SelectedCircuit, SelectedTeam, SelectedRaceClass);
            
            Initialize();
            CurrentMode = GameMode.Racing;
        }

        private ITrack? GetCurrentTrack() => _track;

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
