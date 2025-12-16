using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Infrastructure.Graphics;
using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Presentation
{
    public class AttractMode : IAttractMode
    {
        private readonly ILogger<AttractMode> _logger;
        private readonly IGameState _gameState;
        private bool _isActive;
        
        public AttractMode(
            ILogger<AttractMode> logger, 
            IGameState gameState)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _isActive = false;
        }
        
        public void Start()
        {

            _logger.LogInformation("Starting attract mode demo");
            
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

        public bool IsActive => _isActive;
    }

}
