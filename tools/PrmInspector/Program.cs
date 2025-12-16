using System;
using System.IO;
using System.Linq;
using WipeoutRewrite.Core.Graphics;
using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Tools.PrmInspector
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup logging
            var loggerFactory = LoggerFactory.Create(builder => 
                builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var logger = loggerFactory.CreateLogger<ModelLoader>();

            var loader = new ModelLoader(logger);
            
            string modelsPath = "/home/rick/workspace/wipeout_csharp/assets/wipeout/common";
            
            Console.WriteLine($"=== Scanning PRM files in: {modelsPath} ===\n");
            
            if (!Directory.Exists(modelsPath))
            {
                Console.WriteLine($"ERROR: Directory not found: {modelsPath}");
                return;
            }

            var prmFiles = Directory.GetFiles(modelsPath, "*.prm").OrderBy(f => f).ToArray();
            
            // Focus on ship files
            var shipFiles = prmFiles.Where(f => Path.GetFileName(f).StartsWith("shp") || Path.GetFileName(f) == "allsh.prm").ToArray();
            
            Console.WriteLine($"=== Analyzing {shipFiles.Length} ship files ===\n");
            
            foreach (var prmFile in shipFiles)
            {
                Console.WriteLine($"\n--- {Path.GetFileName(prmFile)} ({new FileInfo(prmFile).Length} bytes) ---");
                
                var objects = loader.GetObjectsInPrmFile(prmFile);
                
                if (objects.Count == 0)
                {
                    Console.WriteLine("  (no objects with vertices found)");
                }
                else
                {
                    foreach (var (index, name) in objects)
                    {
                        Console.WriteLine($"  [{index}] {name}");
                    }
                }
            }
            
            Console.WriteLine("\n\n=== Now scanning ALL files ===\n");
            
            foreach (var prmFile in prmFiles)
            {
                Console.WriteLine($"\n--- {Path.GetFileName(prmFile)} ---");
                
                var objects = loader.GetObjectsInPrmFile(prmFile);
                
                if (objects.Count == 0)
                {
                    Console.WriteLine("  (no objects with vertices found)");
                }
                else
                {
                    foreach (var (index, name) in objects)
                    {
                        Console.WriteLine($"  [{index}] {name}");
                    }
                }
            }
            
            Console.WriteLine("\n=== Scan complete ===");
        }
    }
}
