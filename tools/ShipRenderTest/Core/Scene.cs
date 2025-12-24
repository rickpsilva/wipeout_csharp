using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Tools.Managers;

namespace WipeoutRewrite.Tools.Core;

/// <summary>
/// Represents a 3D scene containing objects, cameras, and lights.
/// Follows Single Responsibility Principle - delegates light management to LightManager.
/// </summary>
public class Scene : IScene
{
    #region properties
    public ICameraManager CameraManager => _cameraManager;
    public ILightManager LightManager => _lightManager;
    public IReadOnlyList<SceneObject> Objects => _objects.AsReadOnly();

    public SceneCamera? SelectedCamera
    {
        get => _selectedCamera;
        set
        {
            _selectedCamera = value;
            if (value != null)
            {
                _selectedObject = null;
                _selectedLight = null;
            }
        }
    }

    public EntityType? SelectedEntityType
    {
        get
        {
            if (_selectedObject != null) return EntityType.Object;
            if (_selectedCamera != null) return EntityType.Camera;
            if (_selectedLight != null) return EntityType.Light;
            return null;
        }
    }

    public DirectionalLight? SelectedLight
    {
        get => _selectedLight;
        set
        {
            _selectedLight = value;
            if (value != null)
            {
                _selectedObject = null;
                _selectedCamera = null;
            }
        }
    }

    public SceneObject? SelectedObject
    {
        get => _selectedObject;
        set
        {
            _selectedObject = value;
            if (value != null)
            {
                _selectedCamera = null;
                _selectedLight = null;
            }
        }
    }

    #endregion 

    #region fields
    private readonly ICameraManager _cameraManager;
    private readonly ILightManager _lightManager;
    private readonly List<SceneObject> _objects;
    private SceneCamera? _selectedCamera;
    private DirectionalLight? _selectedLight;
    private SceneObject? _selectedObject;
    #endregion 

    public Scene(ICameraManager cameraManager, ILightManager lightManager)
    {
        _cameraManager = cameraManager ?? throw new ArgumentNullException(nameof(cameraManager));
        _lightManager = lightManager ?? throw new ArgumentNullException(nameof(lightManager));
        _objects = new List<SceneObject>();
    }

    public SceneObject AddObject(string name, ShipV2? ship = null)
    {
        var obj = new SceneObject(name) { Ship = ship };
        _objects.Add(obj);
        return obj;
    }

    public void ClearSelection()
    {
        _selectedObject = null;
        _selectedCamera = null;
        _selectedLight = null;
    }

    public void RemoveObject(SceneObject obj)
    {
        _objects.Remove(obj);
        if (_selectedObject == obj)
        {
            _selectedObject = null;
        }
    }
}

/// <summary>
/// Represents an object in the 3D scene (ship, model, etc.)
/// </summary>
public class SceneObject
{
    #region properties
    public bool IsVisible { get; set; }
    public string Name { get; set; }
    public Vec3 Position { get; set; }
    public Vec3 Rotation { get; set; }
    public float Scale { get; set; }
    public ShipV2? Ship { get; set; }
    #endregion 

    public SceneObject(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Position = new Vec3(0, 0, 0);
        Rotation = new Vec3(0, 0, 0);
        Scale = 0.1f;
        IsVisible = true;
    }

    public override string ToString() => Name;
}

/// <summary>
/// Type of entity in the scene
/// </summary>
public enum EntityType
{
    Camera,
    Object,
    Light
}