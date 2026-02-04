using WipeoutRewrite.Core.Graphics;

namespace WipeoutRewrite.Core.Entities
{
    /// <summary>
    /// GameObject entity - represents any 3D object in the game (ships, props, etc.).
    /// Can load PRM models, CMP textures, and handle rendering.
    /// </summary>
    public interface IGameObject
    {
        Mesh? GetModel();
        void Load(int modelIndex = 0);
        void Init(TrackSection? section, int pilot, int position);
        void InitExhaustPlume();
        void ResetExhaustPlume();
        Mat4 CalculateTransformMatrix();
        void Draw();
        void RenderShadow();
        void Update();
        void CollideWithTrack(TrackFace face);
        void CollideWithShip(GameObject other);
        Vec3 GetCockpitPosition();
        Vec3 GetNosePosition();
        Vec3 GetWingLeftPosition();
        Vec3 GetWingRightPosition();
    }
}
   