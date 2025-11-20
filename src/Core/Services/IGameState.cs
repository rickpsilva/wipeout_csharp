using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Core.Services
{
    public interface IGameState
    {
        void SetPlayerShip(bool accelerate, bool brake, bool turnLeft, bool turnRight, bool boostLeft, bool boostRight);
        GameMode CurrentMode { get; set; }
        Track CurrentTrack { get; }
        void Initialize(Track track, int playerShipId = 0);
        void Update(float deltaTime);
        void Render(GLRenderer renderer);
    }
}