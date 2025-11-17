using System;
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
            Console.WriteLine("Iniciando WipeoutRewrite (C#) — versão de base.");

            var gws = GameWindowSettings.Default;
            gws.UpdateFrequency = 60.0;

            var nws = new NativeWindowSettings()
            {
                ClientSize = new OpenTK.Mathematics.Vector2i(1280, 720),
                Title = "WipeoutRewrite - C#",
            };

            // Composition Root: criar dependências
            var renderer = new GLRenderer();
            var musicPlayer = new MusicPlayer();
            var assetLoader = new AssetLoader();
            var fontSystem = new FontSystem();
            var menuManager = new MenuManager();

            using (var game = new Game(gws, nws, renderer, musicPlayer, assetLoader, fontSystem, menuManager))
            {
                // Inicializar subsistemas antes de correr
                Renderer.Init();
                game.Run();
            }
        }
    }
}
