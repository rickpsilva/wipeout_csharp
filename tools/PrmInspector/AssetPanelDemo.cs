using System;
using System.IO;
using System.Linq;
using WipeoutRewrite.Core.Graphics;
using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Tools.PrmInspector
{
    /// <summary>
    /// Demonstração de como os ficheiros .prm aparecem no Asset Panel
    /// </summary>
    class AssetPanelDemo
    {
        static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder => 
                builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
            var logger = loggerFactory.CreateLogger<ModelLoader>();
            var loader = new ModelLoader(logger);
            
            string modelsPath = "/home/rick/workspace/wipeout_csharp/assets/wipeout/common";
            
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           ASSET PANEL - PRM FILE BROWSER DEMO             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            var prmFiles = Directory.GetFiles(modelsPath, "*.prm")
                .OrderBy(f => f)
                .Where(f => !Path.GetFileName(f).StartsWith("shp"))  // Excluir shp1s-shp7s
                .ToArray();
            
            foreach (var prmFile in prmFiles.Take(15))  // Primeiros 15 ficheiros
            {
                var fileName = Path.GetFileName(prmFile);
                var objects = loader.GetObjectsInPrmFile(prmFile);
                
                if (objects.Count > 1)
                {
                    // Múltiplos objetos - mostra com expand/collapse
                    Console.WriteLine($"📦 ▶ {fileName,-40} ({objects.Count} models)");
                    foreach (var (index, name) in objects.Take(3))  // Primeiros 3
                    {
                        Console.WriteLine($"      └ [{index}] {name}");
                    }
                    if (objects.Count > 3)
                    {
                        Console.WriteLine($"      └ ... and {objects.Count - 3} more");
                    }
                    Console.WriteLine();
                }
                else if (objects.Count == 1)
                {
                    // Um objeto - mostra direto
                    var (index, name) = objects[0];
                    var displayName = string.IsNullOrWhiteSpace(name) ? $"object {index}" : name;
                    Console.WriteLine($"📦   {fileName}: {displayName}");
                }
                else
                {
                    // Sem objetos
                    Console.WriteLine($"📦   {fileName,-40} (no models)");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("Legenda:");
            Console.WriteLine("  📦 ▶  = Ficheiro com múltiplos modelos (clique para expandir)");
            Console.WriteLine("  📦    = Ficheiro com um único modelo");
            Console.WriteLine("  └    = Modelo dentro de um ficheiro");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
        }
    }
}
