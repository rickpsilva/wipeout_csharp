using Microsoft.Extensions.Logging;
using WipeoutRewrite.Infrastructure.Database.Entities;

namespace WipeoutRewrite.Infrastructure.Database;

/// <summary>
/// Initializes the game settings database.
/// Validates database exists, creates it if needed, and populates with default values.
/// </summary>
public class DatabaseInitializer
{
    private readonly GameSettingsDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(GameSettingsDbContext context, ILogger<DatabaseInitializer> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initialize the database: create if needed and populate with defaults.
    /// </summary>
    public void Initialize()
    {
        _logger.LogInformation("[DB] Initializing game settings database...");

        try
        {
            // Ensure data directory exists
            var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
                _logger.LogInformation("[DB] Created data directory: {0}", dataDir);
            }

            // Create database and tables if they don't exist
            _context.Database.EnsureCreated();
            _logger.LogInformation("[DB] Database ensured");

            // Check if we need to populate with defaults
            if (!HasData())
            {
                _logger.LogInformation("[DB] No data found, populating with defaults...");
                PopulateDefaults();
                _context.SaveChanges();
                _logger.LogInformation("[DB] Defaults populated successfully");
            }
            else
            {
                _logger.LogInformation("[DB] Database already contains data");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DB] Error initializing database");
            throw;
        }
    }

    /// <summary>
    /// Check if database has any settings data.
    /// </summary>
    private bool HasData()
    {
        try
        {
            return _context.ControlsSettings.Any(s => s.Id == 1) &&
                   _context.VideoSettings.Any(s => s.Id == 1) &&
                   _context.AudioSettings.Any(s => s.Id == 1);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Populate database with default values.
    /// </summary>
    private void PopulateDefaults()
    {
        // Create default controls settings
        var defaultControls = CreateDefaultControls();
        _context.ControlsSettings.Add(defaultControls);

        // Create default video settings
        var defaultVideo = CreateDefaultVideo();
        _context.VideoSettings.Add(defaultVideo);

        // Create default audio settings
        var defaultAudio = CreateDefaultAudio();
        _context.AudioSettings.Add(defaultAudio);

        // Populate best times with default values from wipeout-rewrite
        PopulateBestTimes();
    }

    /// <summary>
    /// Create default control settings entity.
    /// Mirrors the default values from wipeout-rewrite save.c
    /// </summary>
    private ControlsSettingsEntity CreateDefaultControls()
    {
        var entity = new ControlsSettingsEntity
        {
            Id = 1,
            // Default keyboard bindings (from wipeout-rewrite)
            UpKeyboard = 82,          // INPUT_KEY_UP
            DownKeyboard = 81,        // INPUT_KEY_DOWN
            LeftKeyboard = 80,        // INPUT_KEY_LEFT
            RightKeyboard = 79,       // INPUT_KEY_RIGHT
            BrakeLeftKeyboard = 6,    // INPUT_KEY_C
            BrakeRightKeyboard = 25,  // INPUT_KEY_V
            ThrustKeyboard = 27,      // INPUT_KEY_X
            FireKeyboard = 29,        // INPUT_KEY_Z
            ChangeViewKeyboard = 4,   // INPUT_KEY_A

            // Default gamepad bindings (from wipeout-rewrite)
            UpJoystick = 120,         // INPUT_GAMEPAD_DPAD_UP
            DownJoystick = 121,       // INPUT_GAMEPAD_DPAD_DOWN
            LeftJoystick = 122,       // INPUT_GAMEPAD_DPAD_LEFT
            RightJoystick = 123,      // INPUT_GAMEPAD_DPAD_RIGHT
            BrakeLeftJoystick = 112,  // INPUT_GAMEPAD_L_SHOULDER
            BrakeRightJoystick = 113, // INPUT_GAMEPAD_R_SHOULDER
            ThrustJoystick = 108,     // INPUT_GAMEPAD_A
            FireJoystick = 111,       // INPUT_GAMEPAD_X
            ChangeViewJoystick = 109, // INPUT_GAMEPAD_Y

            LastModified = DateTime.UtcNow
        };
        return entity;
    }

    /// <summary>
    /// Create default video settings entity.
    /// Mirrors the default values from wipeout-rewrite
    /// </summary>
    private VideoSettingsEntity CreateDefaultVideo()
    {
        return new VideoSettingsEntity
        {
            Id = 1,
            Fullscreen = false,
            InternalRoll = 0.6f,      // From wipeout-rewrite
            UIScale = 0,              // 0 = AUTO
            ShowFPS = false,
            ScreenResolution = 0,     // 0 = Native
            PostEffect = 0,           // 0 = None
            LastModified = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create default audio settings entity.
    /// Mirrors the default values from wipeout-rewrite
    /// </summary>
    private AudioSettingsEntity CreateDefaultAudio()
    {
        return new AudioSettingsEntity
        {
            Id = 1,
            MasterVolume = 1.0f,
            MusicVolume = 0.5f,          // From wipeout-rewrite
            SoundEffectsVolume = 0.6f,   // From wipeout-rewrite (sfx_volume)
            IsMuted = false,
            MusicEnabled = true,
            SoundEffectsEnabled = true,
            MusicMode = "Random",
            LastModified = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Populate default best times from wipeout-rewrite.
    /// Data includes race times for all race classes, tracks, and categories.
    /// </summary>
    private void PopulateBestTimes()
    {
        var bestTimes = new List<BestTimeEntity>
        {
            // VENOM Class - Track 1 (Meltdown)
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 254500, PilotName = "WIP" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 271170, PilotName = "EOU" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 289500, PilotName = "TPC" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 294500, PilotName = "NOT" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 314500, PilotName = "PSX" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 254500, PilotName = "MVE" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 271170, PilotName = "ALM" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 289500, PilotName = "POL" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 294500, PilotName = "NIK" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 314500, PilotName = "DAR" },

            // VENOM Class - Track 2 (Mandrake)
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 159330, PilotName = "AJY" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 172670, PilotName = "AJS" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 191000, PilotName = "DLS" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 207670, PilotName = "MAK" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 219330, PilotName = "JED" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 159330, PilotName = "DAR" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 172670, PilotName = "STU" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 191000, PilotName = "MOC" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 207670, PilotName = "DOM" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 219330, PilotName = "NIK" },

            // VENOM Class - Track 3 (Phoenix)
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 171000, PilotName = "JD" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 189330, PilotName = "AJC" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 202670, PilotName = "MSA" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 219330, PilotName = "SD" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 232670, PilotName = "TIM" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 171000, PilotName = "PHO" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 189330, PilotName = "ENI" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 202670, PilotName = "XR" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 219330, PilotName = "ISI" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 232670, PilotName = "NG" },

            // VENOM Class - Track 4 (Piranha)
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 251330, PilotName = "POL" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 263000, PilotName = "DAR" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 283000, PilotName = "JAS" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 294670, PilotName = "ROB" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 314820, PilotName = "DJR" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 251330, PilotName = "DOM" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 263000, PilotName = "DJR" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 283000, PilotName = "MPI" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 294670, PilotName = "GOC" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 314820, PilotName = "SUE" },

            // VENOM Class - Track 5 (Riptide)
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 236170, PilotName = "NIK" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 253170, PilotName = "SAL" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 262330, PilotName = "DOM" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 282670, PilotName = "LG" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 298170, PilotName = "LNK" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 236170, PilotName = "NIK" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 253170, PilotName = "ROB" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 262330, PilotName = "AM" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 282670, PilotName = "JAS" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 298170, PilotName = "DAR" },

            // VENOM Class - Track 6 (Fusion)
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 182330, PilotName = "HAN" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 196330, PilotName = "PER" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 214830, PilotName = "FEC" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 228830, PilotName = "TPI" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 244330, PilotName = "ZZA" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 182330, PilotName = "FC" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 196330, PilotName = "SUE" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 214830, PilotName = "ROB" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 228830, PilotName = "JEN" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 244330, PilotName = "NT" },

            // VENOM Class - Track 7 (Volcano)
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 195400, PilotName = "CAN" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 209230, PilotName = "WEH" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 227900, PilotName = "AVE" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 239900, PilotName = "ABO" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Venom", Category = "Race", TimeMilliseconds = 240730, PilotName = "NUS" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 195400, PilotName = "DJR" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 209230, PilotName = "NIK" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 227900, PilotName = "JAS" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 239900, PilotName = "NCW" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Venom", Category = "TimeTrialStandard", TimeMilliseconds = 240730, PilotName = "LOU" },

            // RAPIER Class - Track 1 (Meltdown)
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 200670, PilotName = "AJY" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 213500, PilotName = "DLS" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 228670, PilotName = "AJS" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 247670, PilotName = "MAK" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 263000, PilotName = "JED" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 200670, PilotName = "NCW" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 213500, PilotName = "LEE" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 228670, PilotName = "STU" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 247670, PilotName = "JAS" },
            new BestTimeEntity { CircuitName = "Meltdown", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 263000, PilotName = "ROB" },

            // RAPIER Class - Track 2 (Mandrake)
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 134580, PilotName = "BOR" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 147000, PilotName = "ING" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 162250, PilotName = "HIS" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 183080, PilotName = "COR" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 198250, PilotName = "ES" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 134580, PilotName = "NIK" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 147000, PilotName = "POL" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 162250, PilotName = "DAR" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 183080, PilotName = "STU" },
            new BestTimeEntity { CircuitName = "Mandrake", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 198250, PilotName = "ROB" },

            // RAPIER Class - Track 3 (Phoenix)
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 142080, PilotName = "AJS" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 159420, PilotName = "DLS" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 178080, PilotName = "MAK" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 190250, PilotName = "JED" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 206580, PilotName = "AJY" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 142080, PilotName = "POL" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 159420, PilotName = "JIM" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 178080, PilotName = "TIM" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 190250, PilotName = "MOC" },
            new BestTimeEntity { CircuitName = "Phoenix", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 206580, PilotName = "PC" },

            // RAPIER Class - Track 4 (Piranha)
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 224170, PilotName = "DLS" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 237000, PilotName = "DJR" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 257500, PilotName = "LEE" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 272830, PilotName = "MOC" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 285170, PilotName = "MPI" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 224170, PilotName = "TIM" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 237000, PilotName = "JIM" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 257500, PilotName = "NIK" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 272830, PilotName = "JAS" },
            new BestTimeEntity { CircuitName = "Piranha", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 285170, PilotName = "LG" },

            // RAPIER Class - Track 5 (Riptide)
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 191000, PilotName = "MAK" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 203670, PilotName = "STU" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 221830, PilotName = "JAS" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 239000, PilotName = "ROB" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 254500, PilotName = "DOM" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 191000, PilotName = "LG" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 203670, PilotName = "LOU" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 221830, PilotName = "JIM" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 239000, PilotName = "HAN" },
            new BestTimeEntity { CircuitName = "Riptide", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 254500, PilotName = "NT" },

            // RAPIER Class - Track 6 (Fusion)
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 156670, PilotName = "JED" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 170330, PilotName = "NCW" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 188830, PilotName = "LOU" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 201000, PilotName = "DAR" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 221500, PilotName = "POL" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 156670, PilotName = "STU" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 170330, PilotName = "DAV" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 188830, PilotName = "DOM" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 201000, PilotName = "MOR" },
            new BestTimeEntity { CircuitName = "Fusion", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 221500, PilotName = "GAN" },

            // RAPIER Class - Track 7 (Volcano)
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 162420, PilotName = "PC" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 179580, PilotName = "POL" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 194750, PilotName = "DAR" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 208920, PilotName = "DAR" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Rapier", Category = "Race", TimeMilliseconds = 224580, PilotName = "MSC" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 162420, PilotName = "THA" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 179580, PilotName = "NKS" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 194750, PilotName = "FOR" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 208920, PilotName = "PLA" },
            new BestTimeEntity { CircuitName = "Volcano", RacingClass = "Rapier", Category = "TimeTrialStandard", TimeMilliseconds = 224580, PilotName = "YIN" },
        };

        foreach (var time in bestTimes)
        {
            _context.BestTimes.Add(time);
        }
    }
}
