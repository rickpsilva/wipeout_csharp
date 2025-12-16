
using OpenTK.Graphics.OpenGL;
using WipeoutRewrite.Core.Graphics;

namespace WipeoutRewrite.Core.Entities
{
    /// <summary>
    /// Ship entity - represents a racing ship in the game.
    /// Based on wipeout-rewrite/src/wipeout/ship.c
    /// </summary>
    public interface IShipV2
    {
        Mesh? GetModel();
        void ShipLoad(int shipIndex = 0);
        void ShipInit(TrackSection? section, int pilot, int position);
        void InitExhaustPlume();
        void ResetExhaustPlume();
        Mat4 CalculateTransformMatrix();
        void Draw();
        void RenderShadow();
        void Update();
        void CollideWithTrack(TrackFace face);
        void CollideWithShip(ShipV2 other);
        Vec3 GetCockpitPosition();
        Vec3 GetNosePosition();
        Vec3 GetWingLeftPosition();
        Vec3 GetWingRightPosition();
    }
}
   