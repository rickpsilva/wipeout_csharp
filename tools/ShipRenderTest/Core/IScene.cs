using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Core.Graphics;
using WipeoutRewrite.Tools.Managers;

namespace WipeoutRewrite.Tools.Core;

/// <summary>
/// Represents a scene interface that manages objects, cameras, lights, and their selections within a 3D environment.
/// </summary>
public interface IScene
{
    /// <summary>
    /// Gets or sets the active track in the scene.
    /// </summary>
    Track? ActiveTrack { get; set; }
    
    /// <summary>
    /// Gets or sets the track loader used to load track geometry data.
    /// </summary>
    TrackLoader? TrackLoader { get; set; }
    
    /// <summary>
    /// Gets the camera manager responsible for managing cameras in the scene.
    /// </summary>
    ICameraManager CameraManager { get; }
    /// <summary>
    /// Gets the light manager responsible for managing lights in the scene.
    /// </summary>
    ILightManager LightManager { get; }
    /// <summary>
    /// Gets a read-only list of all scene objects currently in the scene.
    /// </summary>
    IReadOnlyList<SceneObject> Objects { get; }
    /// <summary>
    /// Gets or sets the currently selected camera in the scene.
    /// </summary>
    SceneCamera? SelectedCamera { get; set; }
    /// <summary>
    /// Gets the type of the currently selected entity.
    /// </summary>
    EntityType? SelectedEntityType { get; }
    /// <summary>
    /// Gets or sets the currently selected directional light in the scene.
    /// </summary>
    DirectionalLight? SelectedLight { get; set; }
    /// <summary>
    /// Gets or sets the currently selected scene object.
    /// </summary>
    SceneObject? SelectedObject { get; set; }

    /// <summary>
    /// Gets the collection of standalone textures loaded from CMP files.
    /// Key is the CMP filename, value is the array of texture handles.
    /// </summary>
    IReadOnlyDictionary<string, int[]> StandaloneTextures { get; }

    /// <summary>
    /// Adds standalone textures from a CMP file.
    /// </summary>
    /// <param name="cmpFileName">The name of the CMP file (without path).</param>
    /// <param name="textureHandles">The array of texture handles.</param>
    void AddStandaloneTextures(string cmpFileName, int[] textureHandles);

    /// <summary>
    /// Clears all standalone textures.
    /// </summary>
    void ClearStandaloneTextures();

    /// <summary>
    /// Adds a new object to the scene with the specified name and optional game object.
    /// </summary>
    /// <param name="name">The name of the object to add.</param>
    /// <param name="ship">The optional game object associated with the scene object.</param>
    /// <returns>The newly created scene object.</returns>
    SceneObject AddObject(string name, GameObject? ship = null);
    /// <summary>
    /// Clears all current selections in the scene.
    /// </summary>
    void ClearSelection();
    /// <summary>
    /// Removes the specified object from the scene.
    /// </summary>
    /// <param name="obj">The scene object to remove.</param>
    void RemoveObject(SceneObject obj);
}