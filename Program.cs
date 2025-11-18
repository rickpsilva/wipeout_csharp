using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.Desktop;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.Audio;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.UI;
using WipeoutRewrite.Core.Services;

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
            logger.LogInformation("Iniciando WipeoutRewrite (C#)");
            logger.LogInformation("========================================");

            // Inicializar subsistemas antes de correr
            Renderer.Init();

            // Resolver e executar o jogo
            using (var game = serviceProvider.GetRequiredService<Game>())
            {
                game.Run();
            }

            logger.LogInformation("WipeoutRewrite encerrado");

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
                builder.SetMinimumLevel(LogLevel.Information);
                
                // Pode configurar níveis por namespace:
                builder.AddFilter("WipeoutRewrite", LogLevel.Debug);
                builder.AddFilter("Microsoft", LogLevel.Warning);
            });

            // Window Settings (como Singleton - será injetado no Game)
            var gws = GameWindowSettings.Default;
            gws.UpdateFrequency = 60.0;
            services.AddSingleton(gws);

            var nws = new NativeWindowSettings()
            {
                ClientSize = new OpenTK.Mathematics.Vector2i(1280, 720),
                Title = "WipeoutRewrite - C#",
            };
            services.AddSingleton(nws);

            // Asset Loaders (usado por outros serviços)
            services.AddSingleton<CmpImageLoader>();
            services.AddSingleton<TimImageLoader>();
            
            // Core Services - Singleton (uma instância para toda a aplicação)
            services.AddSingleton<IRenderer, GLRenderer>();
            services.AddSingleton<IMusicPlayer, MusicPlayer>();
            services.AddSingleton<IAssetLoader, AssetLoader>();
            services.AddSingleton<IFontSystem, FontSystem>();
            services.AddSingleton<IMenuManager, MenuManager>();
            services.AddSingleton<GameState>();

            // Game - O ponto de entrada principal
            services.AddSingleton<Game>();
        }
    }
}
