using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Core.Services
{
    public interface IGameState
    {
        ITrack? CurrentTrack { get; }
        void SetPlayerShip(bool accelerate, bool brake, bool turnLeft, bool turnRight, bool boostLeft, bool boostRight);
        GameMode CurrentMode { get; set; }
        void Initialize(int playerShipId = 0);
        void Update(float deltaTime);
        void Render(GLRenderer renderer);
    }
}