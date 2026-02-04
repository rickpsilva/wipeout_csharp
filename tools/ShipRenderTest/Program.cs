using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Core.Graphics;
using WipeoutRewrite.Factory;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Tools.Core;
using WipeoutRewrite.Tools.Managers;
using WipeoutRewrite.Tools.Rendering;
using WipeoutRewrite.Tools.UI;

namespace WipeoutRewrite.Tools;

class Program
{
    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddFilter("ShipRenderTest", LogLevel.Debug);
            builder.AddFilter("WipeoutRewrite", LogLevel.Debug);
            builder.AddFilter("Microsoft", LogLevel.Warning);

            try
            {
                builder.AddProvider(new Infrastructure.Logging.FileLoggerProvider(
                    System.IO.Path.Combine("build", "diagnostics", "wipeout_render_log.txt"),
                    LogLevel.Information));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: failed to create file logger: {ex.Message}");
            }
        });

        var gws = GameWindowSettings.Default;
        gws.UpdateFrequency = 60.0;
        services.AddSingleton(gws);

        var nws = new NativeWindowSettings()
        {
            ClientSize = new Vector2i(1280, 720),
            Title = "ShipRenderTest - C#",
        };
        services.AddSingleton(nws);

        // Core services with interfaces
        services.AddSingleton<IModelLoader, ModelLoader>();

        services.AddSingleton<ICamera, Camera>();
        services.AddSingleton<ICameraFactory, CameraFactory>();
        services.AddSingleton<IRenderer, GLRenderer>();
        services.AddSingleton<ICmpImageLoader, CmpImageLoader>();
        services.AddSingleton<ITimImageLoader, TimImageLoader>();
        services.AddSingleton<ITextureManager, TextureManager>();
        services.AddSingleton<IGameObjectCollection, GameObjectCollection>();
        services.AddTransient<IGameObject, GameObject>();
        services.AddTransient<IGameObjectFactory, GameObjectFactory>();
        services.AddSingleton<ITrackNavigationCalculator, TrackNavigationCalculator>();

        // Managers with interfaces
        services.AddSingleton<ICameraManager, CameraManager>();
        services.AddSingleton<ILightManager, LightManager>();
        services.AddSingleton<ISettingsService, AppSettingsManager>();
        services.AddSingleton<IRecentFilesService, RecentFilesManager>();

        // Scene and rendering services with interfaces
        services.AddSingleton<IScene, Scene>();
        services.AddSingleton<IWorldGrid, WorldGrid>();
        services.AddSingleton<IViewGizmo, ViewGizmo>();

        // UI services with interfaces
        services.AddSingleton<IModelBrowser, ModelBrowser>();

        // Factories
        services.AddSingleton<ITrackNavigationCalculatorFactory, TrackNavigationCalculatorFactory>();
        services.AddSingleton<ITrackFactory, TrackFactory>();

        // UI Panels - Singleton para manter estado entre renders
        services.AddSingleton<ISceneHierarchyPanel, SceneHierarchyPanel>();
        services.AddSingleton<ITransformPanel, TransformPanel>();
        services.AddSingleton<ICameraPanel, CameraPanel>();
        services.AddSingleton<ILightPanel, LightPanel>();
        services.AddSingleton<ISettingsPanel, SettingsPanel>();
        services.AddSingleton<IViewportInfoPanel, ViewportInfoPanel>();
        services.AddSingleton<IPropertiesPanel, PropertiesPanel>();
        services.AddSingleton<IAssetBrowserPanel>(provider =>
            new AssetBrowserPanel(
                provider.GetRequiredService<ILogger<AssetBrowserPanel>>(),
                provider.GetRequiredService<IModelBrowser>(),
                provider.GetRequiredService<ITextureManager>(),
                provider.GetRequiredService<IScene>(),
                provider.GetRequiredService<ILogger<ShipRenderWindow>>()
            )
        );
        services.AddSingleton<ITexturePanel, TexturePanel>();
        services.AddSingleton<ITrackViewerPanel, TrackViewerPanel>();
        services.AddSingleton<ITrackDataPanel, TrackDataPanel>();
        services.AddSingleton<FileDialogManager>();
        services.AddSingleton<ShipRenderWindow>();

        // Core Entities
        // Track is Transient: new instance created each time to avoid stale data when loading new tracks
        services.AddTransient<ITrack, Track>();

    }

    static void Main(string[] args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("========================================");
        logger.LogInformation("WipeOut Ship Render Test Tool");
        logger.LogInformation("========================================");

        using (var game = serviceProvider.GetRequiredService<ShipRenderWindow>())
        {
            game.Run();
        }

        logger.LogInformation("WipeoutRewrite encerrado");
        serviceProvider.Dispose();
    }
}