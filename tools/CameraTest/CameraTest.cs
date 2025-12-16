using OpenTK.Windowing.GraphicsLibraryFramework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using WipeoutRewrite;

// Setup logger
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

var logger = loggerFactory.CreateLogger<Program>();

// Create camera
var camera = new Camera(logger);
camera.SetAspectRatio(1280f / 720f);

logger.LogInformation("=== Camera Input Test ===");
logger.LogInformation("Camera initial position: {0}", camera.Position);
logger.LogInformation("Camera initial target: {0}", camera.Target);

// Simular KeyboardState com W pressionado
// Nota: KeyboardState é um struct imutável, então não conseguimos modificar diretamente
// Mas podemos chamar Update com um estado vazio para ver se funciona

logger.LogInformation("Calling Update with empty KeyboardState...");
var emptyKeyboard = new KeyboardState();
var emptyMouse = new MouseState();

camera.Update(emptyKeyboard, emptyMouse);
logger.LogInformation("Position after Update: {0}", camera.Position);

logger.LogInformation("=== Test Complete ===");
