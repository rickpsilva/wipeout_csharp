namespace WipeoutRewrite.Presentation;

public class ContentPreview3D : IContentPreview3D
{
    #region fields
    private readonly ICamera _camera;
    private bool _cameraConfigured = false;

    // Further away in negative Z
    private Vec3 _cameraOffset = new Vec3(0, 3, 8);

    private GameObjectCategory _currentCategory = GameObjectCategory.Unknown;
    private int _currentCategoryIndex = -1;
    private int _currentModelId = -1;
    private readonly IGameObjectCollection _gameObjects;
    private bool _initialized = false;
    private readonly ILogger<ContentPreview3D> _logger;
    private readonly IRenderer _renderer;
    private float _rotationAngle = 0f;

    // Camera offset relative to the ship
    private float _rotationSpeed = 0.01f;

    // Positioning configurations (adjustable)
    private Vec3 _shipPosition = new Vec3(0, 0, -15);

    // Mapping of marker types to categories
    private static readonly Dictionary<System.Type, GameObjectCategory> CategoryMap = new()
        {
            { typeof(CategoryShip), GameObjectCategory.Ship },
            { typeof(CategoryMsDos), GameObjectCategory.MsDos },
            { typeof(CategoryTeams), GameObjectCategory.Teams },
            { typeof(CategoryOptions), GameObjectCategory.Options },
            { typeof(CategoryWeapon), GameObjectCategory.Weapon },
            { typeof(CategoryPickup), GameObjectCategory.Pickup },
            { typeof(CategoryProp), GameObjectCategory.Prop },
            { typeof(CategoryObstacle), GameObjectCategory.Obstacle },
            { typeof(CategoryPilot), GameObjectCategory.Pilot },
            { typeof(CategoryCamera), GameObjectCategory.Camera }
        };

    #endregion 

    public ContentPreview3D(
        ILogger<ContentPreview3D> logger,
        IGameObjectCollection gameObjects,
        IRenderer renderer,
        ICamera camera
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameObjects = gameObjects ?? throw new ArgumentNullException(nameof(gameObjects));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
    }

    #region methods

    public void Render<T>(int categoryIndex)
    {
        // Config the camera - ALWAYS reconfigure for testing
        // if (!_cameraConfigured)  // Commented out to force reconfiguration
        {
            _camera.SetAspectRatio(1280f / 720f); // Default aspect ratio
            _camera.SetIsometricMode(false);

            // Absolute basics: Standard OpenGL camera setup
            // Camera back at Z=300, looking at origin
            _camera.Position = new OpenTK.Mathematics.Vector3(0, 0, 300);
            _camera.Target = new OpenTK.Mathematics.Vector3(0, 0, 0);

            _cameraConfigured = true;
            _logger.LogInformation("Camera configured for ContentPreview3D: Position={Pos}, Target={Target}",
                _camera.Position, _camera.Target);
        }

        // Get the category based on type T
        var markerType = typeof(T);
        if (!CategoryMap.TryGetValue(markerType, out var category))
        {
            _logger.LogWarning("Unknown marker type {Type}, cannot render", markerType.Name);
            return;
        }

        // Get objects in this category
        var objectsInCategory = _gameObjects.GetByCategory(category);
        if (objectsInCategory == null || objectsInCategory.Count == 0)
        {
            _logger.LogWarning("No objects found in category {Category}", category);
            return;
        }

        // Validate index
        if (categoryIndex < 0 || categoryIndex >= objectsInCategory.Count)
        {
            _logger.LogWarning("Index {Index} out of range for category {Category} (count: {Count})",
                categoryIndex, category, objectsInCategory.Count);
            return;
        }

        // Get the specific object by index in the category
        var targetObject = objectsInCategory[categoryIndex];
        int modelId = targetObject.GameObjectId;

        if (!_initialized)
        {
            Initialize(modelId);
        }

        // Check if we need to change the object (category, index, or modelId changed)
        bool needsUpdate = _currentCategory != category ||
                          _currentCategoryIndex != categoryIndex ||
                          _currentModelId != modelId;

        if (needsUpdate)
        {
            _logger.LogInformation("Changing preview: {Category}[{Index}] -> GameObject ID {ModelId} (Name: {Name})",
                category, categoryIndex, modelId, targetObject.Name);

            // Hide the previous object
            if (_currentModelId >= 0 && _currentCategory != GameObjectCategory.Unknown)
            {
                var oldObject = _gameObjects.GetAll.Find(s =>
                    s.GameObjectId == _currentModelId && s.Category == _currentCategory);
                if (oldObject != null)
                {
                    oldObject.IsVisible = false;
                    _logger.LogDebug("Hidden previous object: {Name}", oldObject.Name);
                }
            }

            // Show the new object
            targetObject.IsVisible = true;

            // Position based on category (Ships further back, MsDos closer)
            float zPosition = category == GameObjectCategory.Ship ? -700 : -400;
            targetObject.Position = new Vec3(0, 0, zPosition);

            // Both Ships and MsDos need 180° flip on Z axis (Ships for orientation, MsDos to flip text)
            targetObject.Angle = new Vec3(0, 0, MathF.PI);

            _logger.LogInformation("Showing object: {Name}, Position: {Pos}, HasModel: {HasModel}",
                targetObject.Name, targetObject.Position, targetObject.Model != null);

            _currentModelId = modelId;
            _currentCategory = category;
            _currentCategoryIndex = categoryIndex;
        }

        // Update rotation (from right to left)
        _rotationAngle += _rotationSpeed;
        if (_rotationAngle > MathF.PI * 2)
            _rotationAngle -= MathF.PI * 2;

        // Use targetObject directly instead of searching again
        if (targetObject != null)
        {
            // Rotation in Y to rotate from right to left
            // Both categories need 180° Z flip
            targetObject.Angle = new Vec3(0, _rotationAngle, MathF.PI);
            // Apply configured position (maintain the adjusted position)
            // Don't override position here, it was set in needsUpdate
        }

        // Render 3D object (only the selected object)
        if (targetObject == null || !targetObject.IsVisible)
        {
            _logger.LogWarning("Cannot render: targetObject is {Status}",
                targetObject == null ? "null" : "invisible");
            return;
        }

        // Debug: Check if object has a model and primitives
        if (targetObject.Model == null)
        {
            _logger.LogWarning("Object {Name} has no Model loaded!", targetObject.Name);
            return;
        }

        // Log detailed model info
        var primitiveCount = targetObject.Model.Primitives?.Count ?? 0;
        _logger.LogDebug("Rendering {Name}: Model has {PrimitiveCount} primitives",
            targetObject.Name, primitiveCount);

        if (primitiveCount == 0)
        {
            _logger.LogWarning("Object {Name} has Model but 0 primitives - nothing to render!",
                targetObject.Name);
            return;
        }

        try
        {
            // Clear only the depth buffer (keep the color buffer with the background)
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // CRITICAL: Reset viewport and scissor (may be set from 2D rendering)
            GL.Viewport(0, 0, _renderer.ScreenWidth, _renderer.ScreenHeight);
            GL.Disable(EnableCap.ScissorTest);

            // CRITICAL: Ensure polygon mode is FILL, not LINE (wireframe)
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            // CRITICAL: Force flush any pending 2D geometry before switching to 3D
            // This ensures we don't mix 2D and 3D geometry in the same batch
            _renderer.Flush();

            // Force 3D state after 2D rendering
            _renderer.SetPassthroughProjection(false);
            _renderer.SetProjectionMatrix(_camera.GetProjectionMatrix());
            _renderer.SetViewMatrix(_camera.GetViewMatrix());
            _renderer.SetModelMatrix(OpenTK.Mathematics.Matrix4.Identity);

            // Ensure correct OpenGL state for 3D rendering
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            _renderer.SetDepthTest(true);
            _renderer.SetDepthWrite(true);
            _renderer.SetFaceCulling(true);

            _renderer.SetCurrentTexture(_renderer.WhiteTexture);

            // Render only the selected object
            targetObject.Draw();

            // CRITICAL: Flush 3D geometry NOW before any state changes
            _renderer.Flush();

            // Render shadow of the object
            _renderer.SetBlending(true);
            targetObject.RenderShadow();
            _renderer.SetBlending(false);

            // CRITICAL: Flush shadow geometry before returning to caller
            _renderer.Flush();

            _renderer.SetDepthTest(false);
            _renderer.SetDepthWrite(false);
            _renderer.SetFaceCulling(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering 3D preview");
        }
    }

    /// <summary>
    /// Configure the camera offset relative to the ship
    /// </summary>
    public void SetCameraOffset(float x, float y, float z)
    {
        _cameraOffset = new Vec3(x, y, z);
        _logger.LogInformation($"Camera offset set to: ({x}, {y}, {z})");
    }

    public void SetModel(int modelId)
    {
        _currentModelId = modelId;
        _logger.LogInformation($"Setting 3D model for preview: {modelId}");
    }

    /// <summary>
    /// Configure the rotation speed
    /// </summary>
    public void SetRotationSpeed(float speed)
    {
        _rotationSpeed = speed;
    }

    /// <summary>
    /// Configure the ship position in the 3D preview
    /// </summary>
    public void SetShipPosition(float x, float y, float z)
    {
        _shipPosition = new Vec3(x, y, z);
        _logger.LogInformation($"Ship position set to: ({x}, {y}, {z})");
    }

    private void Initialize(int modelId)
    {
        _logger.LogInformation("Initializing ContentPreview3D");

        // Initialize objects if not already done
        if (_gameObjects.GetAll.Count == 0)
        {
            _gameObjects.Init(null);
        }

        // Make all objects INVISIBLE initially
        foreach (var obj in _gameObjects.GetAll)
        {
            obj.IsVisible = false;
        }

        // Find and configure the desired ship
        var ship = _gameObjects.GetAll.Find(s => s.GameObjectId == modelId);
        if (ship != null)
        {
            ship.Position = _shipPosition;
            ship.Angle = new Vec3(0, 0, MathF.PI);
            _logger.LogInformation("Ship {ModelId} configured for preview at position {Pos}", modelId, ship.Position);
        }

        _initialized = true;
        _currentModelId = modelId;
    }

    #endregion 
}