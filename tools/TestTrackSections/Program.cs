using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Graphics;

// Test loading TRACK.TRS directly
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddConsole();
});
var logger = loggerFactory.CreateLogger<TrackSectionLoader>();

string trackTrsPath = "/home/rick/workspace/wipeout_csharp/assets/wipeout/track02/track.trs";

if (!File.Exists(trackTrsPath))
{
    Console.WriteLine($"ERROR: Track file not found: {trackTrsPath}");
    return;
}

var loader = new TrackSectionLoader(logger);
loader.LoadSections(trackTrsPath);

Console.WriteLine("\n✓ Track TRS loaded successfully!");
