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

namespace WipeoutRewrite
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup Dependency Injection Container
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("========================================");
            logger.LogInformation("Starting Wipeout (C#)");
            logger.LogInformation("========================================");

            // Initialize database
            try
            {
                var dbInitializer = serviceProvider.GetRequiredService<DatabaseInitializer>();
                dbInitializer.Initialize();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize database");
                throw;
            }

            // Resolve and run the game
            using (var game = serviceProvider.GetRequiredService<IGame>())
            {
                game.Run();
            }

            logger.LogInformation("Wipeout closed");

            // Cleanup
            serviceProvider.Dispose();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Logging Configuration
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);

                builder.AddFilter("Wipeout", LogLevel.Debug);
                // Also write logs to a diagnostics file so CI and local debugging can
                // capture historical logs. File placed under build/diagnostics/wipeout_log.txt
                try
                {
                    var logPath = System.IO.Path.Combine("build", "diagnostics", "wipeout_log.txt");
                    var logDir = System.IO.Path.GetDirectoryName(logPath);
                    
                    // Ensure directory exists
                    if (!string.IsNullOrEmpty(logDir) && !System.IO.Directory.Exists(logDir))
                    {
                        System.IO.Directory.CreateDirectory(logDir);
                    }
                    
                    // Clear the log file at startup instead of appending
                    System.IO.File.WriteAllText(logPath, "");
                    builder.AddProvider(new WipeoutRewrite.Infrastructure.Logging.FileLoggerProvider(logPath, LogLevel.Debug));
                }
                catch (Exception ex)
                {
                    // If adding file logger fails we still continue with console only
                    Console.WriteLine($"Warning: failed to create file logger: {ex.Message}");
                }
            });

            // Database Configuration
            var dbPath = Path.Combine(AppContext.BaseDirectory, "data", "wipeout_settings.db");
            services.AddDbContext<GameSettingsDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}")
            );
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<DatabaseInitializer>();
            services.AddSingleton<SettingsPersistenceService>();

            // Window Settings
            var gws = GameWindowSettings.Default;
            gws.UpdateFrequency = 60.0;
            services.AddSingleton(gws);

            var nws = new NativeWindowSettings()
            {
                ClientSize = new OpenTK.Mathematics.Vector2i(1280, 720),
                Title = "WipeoutRewrite - C#",
            };
            services.AddSingleton(nws);

            // Core
            services.AddSingleton<IRenderer, GLRenderer>();
            services.AddSingleton<ICamera, Camera>();
            services.AddSingleton<ICameraFactory, CameraFactory>();
            services.AddSingleton<IVideoPlayer, IntroVideoPlayer>();
            services.AddSingleton<IMusicPlayer, MusicPlayer>();
            services.AddSingleton<IAssetLoader, AssetLoader>();
            services.AddSingleton<IGameState, GameState>();
            services.AddSingleton<IOptionsFactory>(sp => 
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var repository = sp.GetRequiredService<ISettingsRepository>();
                return new OptionsFactory(loggerFactory, repository);
            });
            services.AddSingleton<IControlsSettings>(sp =>
            {
                var factory = sp.GetRequiredService<IOptionsFactory>();
                return factory.CreateControlsSettings();
            });
            services.AddSingleton<IVideoSettings>(sp =>
            {
                var factory = sp.GetRequiredService<IOptionsFactory>();
                return factory.CreateVideoSettings();
            });
            services.AddSingleton<IAudioSettings>(sp =>
            {
                var factory = sp.GetRequiredService<IOptionsFactory>();
                return factory.CreateAudioSettings();
            });
            services.AddSingleton<IBestTimesManager>(sp =>
            {
                var factory = sp.GetRequiredService<IOptionsFactory>();
                return factory.CreateBestTimesManager();
            });

            services.AddSingleton<IMenuManager, MenuManager>();
            services.AddTransient<ITrack, Track>();
            services.AddSingleton<ITrackFactory, TrackFactory>();

            //UI
            services.AddSingleton<IFontSystem, FontSystem>();
            services.AddSingleton<IMenuRenderer, MenuRenderer>();

            //Presentation
            services.AddSingleton<IAttractMode, AttractMode>();
            services.AddSingleton<IContentPreview3D, ContentPreview3D>();
            services.AddSingleton<ITitleScreen, TitleScreen>();
            services.AddSingleton<ICreditsScreen, CreditsScreen>();


            // Ship Services
            services.AddSingleton<IGameObjectCollection, GameObjectCollection>();
            services.AddTransient<IGameObject, GameObject>();
            services.AddTransient<IGameObjectFactory, GameObjectFactory>();

            // Graphics
            services.AddSingleton<ITextureManager, TextureManager>();

            // Assets
            services.AddSingleton<ICmpImageLoader, CmpImageLoader>();
            services.AddSingleton<ITimImageLoader, TimImageLoader>();
            services.AddSingleton<IAssetPathResolver, AssetPathResolver>();
            
            // Model Loaders
            services.AddSingleton<IModelLoader, ModelLoader>();
             
            // Game - The main application class
            services.AddSingleton<IGame, Game>();
        }
    }
}
