using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite;

public class AttractMode
{
    private readonly GameState _gameState;
    private bool _isActive;
    private Random _random = new Random();
    
    public AttractMode(GameState gameState)
    {
        _gameState = gameState;
        _isActive = false;
    }
    
    public void Start()
    {
        // Select random pilot, circuit, and race class
        int randomPilot = _random.Next(0, 8);
        int randomCircuit = _random.Next(0, 7); // Assuming 7 non-bonus circuits
        int randomClass = _random.Next(0, 2); // Venom or Rapier
        
        Console.WriteLine($"Starting attract mode: Pilot {randomPilot}, Circuit {randomCircuit}, Class {randomClass}");
        
        // TODO: Load random track and start race
        // For now, just set the mode
        _gameState.CurrentMode = GameMode.AttractMode;
        _isActive = true;
    }
    
    public void Stop()
    {
        _isActive = false;
        _gameState.CurrentMode = GameMode.SplashScreen;
    }
    
    public void Update(float deltaTime)
    {
        if (!_isActive)
            return;
        
        // TODO: Update race state in demo mode
        // Auto-pilot the ship, show "DEMO MODE" text
        
        // Check if race is complete
        // if (raceComplete) Stop();
    }
    
    public void Render(IRenderer renderer)
    {
        if (!_isActive)
            return;
        
        // TODO: Render race with "DEMO MODE" text overlay
        // renderer.DrawText("DEMO MODE", x, y, color);
    }
}
