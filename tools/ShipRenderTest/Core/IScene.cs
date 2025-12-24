using System.Collections.Generic;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Core.Graphics;
using WipeoutRewrite.Tools.Managers;

namespace WipeoutRewrite.Tools.Core
{
    /// <summary>
    /// Interface for a 3D scene containing objects, cameras, and lights.
    /// Follows Interface Segregation Principle.
    /// </summary>
    public interface IScene
    {
        IReadOnlyList<SceneObject> Objects { get; }
        ILightManager LightManager { get; }
        ICameraManager CameraManager { get; }

        SceneObject? SelectedObject { get; set; }
        SceneCamera? SelectedCamera { get; set; }
        DirectionalLight? SelectedLight { get; set; }
        EntityType? SelectedEntityType { get; }

        SceneObject AddObject(string name, ShipV2? ship = null);
        void RemoveObject(SceneObject obj);
        void ClearSelection();
    }
}
