using OpenTK.Mathematics;

namespace WipeoutRewrite.Tools.Managers;

/// <summary>
/// Manages multiple cameras in the scene.
/// </summary>
public class CameraManager : ICameraManager
{
    public SceneCamera ActiveCamera
    {
        get => _activeCamera ?? throw new InvalidOperationException("No active camera set");
        set => _activeCamera = value ?? throw new ArgumentNullException(nameof(value));
    }

    public IReadOnlyList<SceneCamera> Cameras => _cameras.AsReadOnly();

    private SceneCamera? _activeCamera;
    private readonly List<SceneCamera> _cameras;

    public CameraManager()
    {
        _cameras = new List<SceneCamera>();
    }

    public SceneCamera AddCamera(string name, ICamera camera)
    {
        var sceneCamera = new SceneCamera(name, camera);
        _cameras.Add(sceneCamera);

        // Set first camera as active
        if (_activeCamera == null)
        {
            _activeCamera = sceneCamera;
        }

        return sceneCamera;
    }

    public SceneCamera? FindCameraByName(string name)
    {
        return _cameras.FirstOrDefault(c => c.Name == name);
    }

    public void RemoveCamera(SceneCamera camera)
    {
        if (_cameras.Count <= 1)
        {
            throw new InvalidOperationException("Cannot remove the last camera");
        }

        _cameras.Remove(camera);

        if (_activeCamera == camera)
        {
            _activeCamera = _cameras.FirstOrDefault();
        }
    }

    public void SetActiveCamera(string name)
    {
        var camera = FindCameraByName(name);
        if (camera != null)
        {
            _activeCamera = camera;
        }
    }
}

/// <summary>
/// Represents a camera in the scene with metadata.
/// </summary>
public class SceneCamera
{
    #region properties
    public ICamera Camera { get; }
    public string Name { get; set; }
    public float SavedDistance { get; set; }
    public float SavedPitch { get; set; }

    // Camera-specific properties for UI
    public Vector3 SavedPosition { get; set; }

    public float SavedYaw { get; set; }
    #endregion

    public SceneCamera(string name, ICamera camera)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Camera = camera ?? throw new ArgumentNullException(nameof(camera));

        // Save initial state
        SaveCurrentState();
    }

    public void RestoreSavedState()
    {
        // Note: ICamera properties are read-only, so we need to update via Camera methods
        // This will be handled by the Camera implementation
    }

    public void SaveCurrentState()
    {
        SavedPosition = Camera.Position;
        SavedYaw = Camera.Yaw;
        SavedPitch = Camera.Pitch;
        SavedDistance = Camera.Distance;
    }

    public override string ToString() => Name;
}