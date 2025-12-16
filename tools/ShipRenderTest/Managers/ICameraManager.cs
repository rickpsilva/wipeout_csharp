namespace WipeoutRewrite.Tools.Managers;

/// <summary>
/// Interface for managing multiple cameras in the scene.
/// </summary>
public interface ICameraManager
{
    /// <summary>
    /// Gets or sets the active camera.
    /// </summary>
    SceneCamera ActiveCamera { get; set; }
    /// <summary>
    /// Gets all cameras in the scene.
    /// </summary>
    IReadOnlyList<SceneCamera> Cameras { get; }

    /// <summary>
    /// Adds a new camera to the scene.
    /// </summary>
    SceneCamera AddCamera(string name, ICamera camera);
    /// <summary>
    /// Finds a camera by name.
    /// </summary>
    SceneCamera? FindCameraByName(string name);
    /// <summary>
    /// Removes a camera from the scene.
    /// </summary>
    void RemoveCamera(SceneCamera camera);
    /// <summary>
    /// Sets the active camera by name.
    /// </summary>
    void SetActiveCamera(string name);
}