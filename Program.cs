using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.Desktop;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.Audio;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.Database;
using WipeoutRewrite.Core.Graphics;
using WipeoutRewrite.Infrastructure.UI;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Factory;
using WipeoutRewrite.Infrastructure.Video;
using WipeoutRewrite.Presentation;

namespace WipeoutRewrite;

class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting Wipeout (C#)");

        try
        {
            serviceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
            using var game = serviceProvider.GetRequiredService<IGame>();
            game.Run();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error");
            throw;
        }
        finally
        {
            logger.LogInformation("Wipeout closed");
            serviceProvider.Dispose();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        ConfigureLogging(services);
        ConfigureDatabase(services);
        ConfigureWindow(services);
        RegisterCoreServices(services);
    }

    private static void ConfigureLogging(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders().AddConsole().SetMinimumLevel(LogLevel.Debug);
            
            try
            {
                var logPath = Path.Combine("build", "diagnostics", "wipeout_log.txt");
                var logDir = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(logDir))
                    Directory.CreateDirectory(logDir);
                
                File.WriteAllText(logPath, "");
                builder.AddProvider(new WipeoutRewrite.Infrastructure.Logging.FileLoggerProvider(logPath, LogLevel.Debug));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: file logger failed - {ex.Message}");
            }
        });
    }

    private static void ConfigureDatabase(IServiceCollection services)
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "data", "wipeout_settings.db");
        services.AddDbContext<GameSettingsDbContext>(opts => opts.UseSqlite($"Data Source={dbPath}"));
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<DatabaseInitializer>();
        services.AddSingleton<SettingsPersistenceService>();
    }

    private static void ConfigureWindow(IServiceCollection services)
    {
        var gws = GameWindowSettings.Default;
        gws.UpdateFrequency = 60.0;
        services.AddSingleton(gws);

        services.AddSingleton(new NativeWindowSettings
        {
            ClientSize = new OpenTK.Mathematics.Vector2i(1280, 720),
            Title = "WipeoutRewrite - C#"
        });
    }

    private static void RegisterCoreServices(IServiceCollection services)
    {
        // Core graphics and rendering
        services.AddSingleton<IRenderer, GLRenderer>();
        services.AddSingleton<ICamera, Camera>();
        services.AddSingleton<ICameraFactory, CameraFactory>();
        services.AddSingleton<ITextureManager, TextureManager>();
        
        // Media
        services.AddSingleton<IVideoPlayer, IntroVideoPlayer>();
        services.AddSingleton<IMusicPlayer, MusicPlayer>();
        
        // Assets and loaders
        services.AddSingleton<IAssetLoader, AssetLoader>();
        services.AddSingleton<ICmpImageLoader, CmpImageLoader>();
        services.AddSingleton<ITimImageLoader, TimImageLoader>();
        services.AddSingleton<IAssetPathResolver, AssetPathResolver>();
        services.AddSingleton<IModelLoader, ModelLoader>();

        // Game state and options
        services.AddSingleton<IGameState, GameState>();
        services.AddSingleton<IOptionsFactory>(sp => 
            new OptionsFactory(sp.GetRequiredService<ILoggerFactory>(), sp.GetRequiredService<ISettingsRepository>()));
        services.AddSingleton<IControlsSettings>(sp => sp.GetRequiredService<IOptionsFactory>().CreateControlsSettings());
        services.AddSingleton<IVideoSettings>(sp => sp.GetRequiredService<IOptionsFactory>().CreateVideoSettings());
        services.AddSingleton<IAudioSettings>(sp => sp.GetRequiredService<IOptionsFactory>().CreateAudioSettings());
        services.AddSingleton<IBestTimesManager>(sp => sp.GetRequiredService<IOptionsFactory>().CreateBestTimesManager());

        // Game objects
        services.AddSingleton<IGameObjectCollection, GameObjectCollection>();
        services.AddTransient<IGameObject, GameObject>();
        services.AddTransient<IGameObjectFactory, GameObjectFactory>();

        // UI and menus
        services.AddSingleton<IMenuManager, MenuManager>();
        services.AddSingleton<IFontSystem, FontSystem>();
        services.AddSingleton<IMenuRenderer, MenuRenderer>();

        // Presentation
        services.AddSingleton<ITitleScreen, TitleScreen>();
        services.AddSingleton<IAttractMode, AttractMode>();
        services.AddSingleton<IContentPreview3D, ContentPreview3D>();
        services.AddSingleton<ICreditsScreen, CreditsScreen>();

        // Tracks
        services.AddTransient<ITrack, Track>();
        services.AddSingleton<ITrackFactory, TrackFactory>();

        // Game
        services.AddSingleton<IGame, Game>();
    }
}
