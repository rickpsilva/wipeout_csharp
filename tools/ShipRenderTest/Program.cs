using System;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using Microsoft.Extensions.DependencyInjection;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Factory;
using WipeoutRewrite.Infrastructure.Graphics;
namespace WipeoutRewrite.Tools
{
    /// <summary>
    /// Entry point for Ship Render Test Tool.
    /// Run this to debug ship rendering in isolation.
    /// 
    /// Usage: dotnet run --project tools/ShipRenderTest.csproj
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {

            // Setup simple logger factory
            // using var loggerFactory = LoggerFactory.Create(builder =>
            // {
            //     builder.AddConsole();
            //     builder.SetMinimumLevel(LogLevel.Debug);
            // });
            
            // var logger = loggerFactory.CreateLogger<ShipRenderWindow>();
            
            // Setup Dependency Injection Container
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("========================================");
            logger.LogInformation("WipeOut Ship Render Test Tool");
            logger.LogInformation("========================================");

            // Resolver e executar o jogo
            using (var game = serviceProvider.GetRequiredService<ShipRenderWindow>())
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
                builder.SetMinimumLevel(LogLevel.Debug);
                
                // Pode configurar níveis por namespace:
                builder.AddFilter("ShipRenderTest", LogLevel.Debug);
                builder.AddFilter("WipeoutRewrite", LogLevel.Debug);
                builder.AddFilter("Microsoft", LogLevel.Warning);
                // Also write logs to a diagnostics file so CI and local debugging can
                // capture historical logs. File placed under build/diagnostics/wipeout_log.txt
                try
                {
                    // Use provider that exists in Infrastructure/Logging
                    builder.AddProvider(new Infrastructure.Logging.FileLoggerProvider(System.IO.Path.Combine("build","diagnostics","wipeout_render_log.txt"), LogLevel.Information));
                }
                catch (Exception ex)
                {
                    // If adding file logger fails we still continue with console only
                    var lp = System.Diagnostics.Process.GetCurrentProcess();
                    Console.WriteLine($"Warning: failed to create file logger: {ex.Message}");
                }
            });

            // Window Settings (como Singleton - será injetado no Game)
            var gws = GameWindowSettings.Default;
            gws.UpdateFrequency = 60.0;
            services.AddSingleton(gws);

            var nws = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(1280, 720),
                Title = "ShipRenderTest - C#",
            };
            services.AddSingleton(nws);

            // Model Loaders
            services.AddSingleton<ICamera, Camera>();
            services.AddSingleton<IRenderer, GLRenderer>();

            // Registrar Ships como singleton
            services.AddSingleton<IShips, Ships>();
            services.AddTransient<IShipV2, ShipV2>();
            services.AddTransient<IShipFactory, ShipFactory>();
            
            // O ILogger<Ships> será resolvido automaticamente pelo DI
            // quando Ships for instanciado
            // services.AddSingleton<IMenuManager, MenuManager>();
            // services.AddSingleton<GameState>();

            // Game - O ponto de entrada principal
            services.AddSingleton<ShipRenderWindow>();
        }
    }
}
