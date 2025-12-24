using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.Desktop;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.Audio;
using WipeoutRewrite.Infrastructure.Assets;
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
                builder.AddFilter("Microsoft", LogLevel.Warning);
                // Also write logs to a diagnostics file so CI and local debugging can
                // capture historical logs. File placed under build/diagnostics/wipeout_log.txt
                try
                {
                    // Use provider that exists in Infrastructure/Logging
                    builder.AddProvider(new WipeoutRewrite.Infrastructure.Logging.FileLoggerProvider(System.IO.Path.Combine("build","diagnostics","wipeout_log.txt"), LogLevel.Debug));
                }
                catch (Exception ex)
                {
                    // If adding file logger fails we still continue with console only
                    var lp = System.Diagnostics.Process.GetCurrentProcess();
                    Console.WriteLine($"Warning: failed to create file logger: {ex.Message}");
                }
            });

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
            services.AddSingleton<IMenuManager, MenuManager>();

            //UI
            services.AddSingleton<IFontSystem, FontSystem>();
            services.AddSingleton<IMenuRenderer, MenuRenderer>();

            //Presentation
            services.AddSingleton<IAttractMode, AttractMode>();
            services.AddSingleton<IContentPreview3D, ContentPreview3D>();
            services.AddSingleton<ITitleScreen, TitleScreen>();
            services.AddSingleton<ICreditsScreen, CreditsScreen>();


            // Ship Services
            services.AddSingleton<IShips, Ships>();
            services.AddTransient<IShipV2, ShipV2>();
            services.AddTransient<IShipFactory, ShipFactory>();

            // Graphics
            services.AddSingleton<ITextureManager, TextureManager>();

            // Assets
            services.AddSingleton<ICmpImageLoader, CmpImageLoader>();
            services.AddSingleton<ITimImageLoader, TimImageLoader>();
            
            // Model Loaders
            services.AddSingleton<ModelLoader>();
             
            // Game - The main application class
            services.AddSingleton<IGame, Game>();
        }
    }
}
