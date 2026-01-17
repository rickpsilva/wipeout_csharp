using Microsoft.EntityFrameworkCore;
using WipeoutRewrite.Infrastructure.Database.Entities;

namespace WipeoutRewrite.Infrastructure.Database;

/// <summary>
/// Database context for Wipeout game settings.
/// Manages all database operations for controls, video, audio settings and best times.
/// </summary>
public class GameSettingsDbContext : DbContext
{
    public const string DefaultDatabaseName = "wipeout_settings.db";
    // Use absolute path based on application directory
    private static readonly string DefaultDatabasePath = Path.Combine(
        AppContext.BaseDirectory, "data", "wipeout_settings.db"
    );

    public DbSet<ControlsSettingsEntity> ControlsSettings { get; set; } = null!;
    public DbSet<VideoSettingsEntity> VideoSettings { get; set; } = null!;
    public DbSet<AudioSettingsEntity> AudioSettings { get; set; } = null!;
    public DbSet<BestTimeEntity> BestTimes { get; set; } = null!;

    public GameSettingsDbContext(DbContextOptions<GameSettingsDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Create a context with SQLite connection string.
    /// </summary>
    public static GameSettingsDbContext Create(string? databasePath = null)
    {
        var path = databasePath ?? DefaultDatabasePath;
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var connectionString = $"Data Source={path}";
        var options = new DbContextOptionsBuilder<GameSettingsDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new GameSettingsDbContext(options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ControlsSettingsEntity
        modelBuilder.Entity<ControlsSettingsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedNever(); // Manual ID (1 for single row)
        });

        // Configure VideoSettingsEntity
        modelBuilder.Entity<VideoSettingsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedNever(); // Manual ID (1 for single row)
        });

        // Configure AudioSettingsEntity
        modelBuilder.Entity<AudioSettingsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedNever(); // Manual ID (1 for single row)
        });

        // Configure BestTimeEntity
        modelBuilder.Entity<BestTimeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();
            
            // Create composite index for circuit + class lookups
            entity.HasIndex(e => new { e.CircuitName, e.RacingClass });
        });
    }
}
