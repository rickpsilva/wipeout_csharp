using ImGuiNET;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Core.Graphics;
using WipeoutRewrite.Factory;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Tools.Core;
using WipeoutRewrite.Tools.Managers;
using WipeoutRewrite.Tools.Rendering;
using WipeoutRewrite.Tools.UI;

namespace WipeoutRewrite.Tools;

/// <summary>
/// Ship Render Test Tool - Dedicated tool to debug ship rendering.
/// Tests different scales, positions, and rendering modes.
/// </summary>
public class ShipRenderWindow : GameWindow
{
    #region fields
    private readonly IAssetBrowserPanel _assetBrowserPanel;
    private ICamera _camera;
    private readonly ICameraFactory _cameraFactory;
    private readonly ICameraPanel _cameraPanel;
    private float _currentUIScale = 1.0f;
    private readonly FileDialogManager _fileDialogManager;
    private readonly IGameObjectCollection _gameObjects;
    private ImGuiController? _imGuiController;
    private bool _isDraggingOrbit = false;
    private bool _isDraggingPan = false;

    // Async loading state
    private bool _isLoading = false;

    // Mouse control state for viewport
    private Vector2 _lastMousePosition;

    private readonly ILightPanel _lightPanel;
    private string _loadingMessage = "";
    private int _loadingProgress = 0;
    private int _loadingTotal = 0;
    private readonly ILogger<ShipRenderWindow> _logger;

    // Scene with objects and cameras
    private SceneCamera? _mainCamera;

    // Test configurations removed - now using Scene-based approach
    private readonly IModelBrowser _modelBrowser;

    private readonly IPropertiesPanel _propertiesPanel;
    private readonly IRecentFilesService _recentFiles;
    private string _renameBuffer = "";
    private object? _renameTarget = null;
    private readonly IRenderer _renderer;

    // 3D orientation gizmo widget
    private IScene _scene;

    // Reference to main camera in scene

    // UI Panels (injected via DI)
    private readonly ISceneHierarchyPanel _scenePanel;

    private ISettingsService _settingsManager;
    private readonly ISettingsPanel _settingsPanel;

    // Toggle transform panel
    private bool _showCamera = true;

    // Toggle scene hierarchy panel

    // Rename dialog state
    private bool _showRenameDialog = false;

    // Toggle camera panel
    private bool _showScene = true;

    private bool _showTransform = true;
    private bool _showViewport = true;

    // Spline debug visualization
    private SplineDebugRenderer? _splineDebugRenderer = null;

    private TrackNavigationCalculator? _splineNavigationCalculator = null;
    private readonly ITextureManager _textureManager;
    private readonly ITexturePanel _texturePanel;
    private float _totalTime = 0f;
    private readonly ITrack _track;
    private readonly ITrackFactory _trackFactory;
    private TrackAnimator? _trackAnimator;
    private readonly ITrackViewerPanel _trackViewerPanel;
    private readonly ITrackDataPanel _trackDataPanel;
    private readonly ITransformPanel _transformPanel;

    // World grid and coordinate axes
    private readonly IViewGizmo _viewGizmo;

    // Viewport framebuffer for rendering 3D scene in ImGui window
    private int _viewportFBO = 0;

    private bool _viewportFocused = false;
    private int _viewportHeight = 600;
    private bool _viewportHovered = false;
    private readonly IViewportInfoPanel _viewportInfoPanel;
    private int _viewportRBO = 0;
    private int _viewportTexture = 0;
    private int _viewportWidth = 800;
    private readonly IWorldGrid _worldGrid;
    #endregion 

    // Can be SceneObject, SceneCamera, or DirectionalLight

    public ShipRenderWindow(
        GameWindowSettings gws,
        NativeWindowSettings nws,
        IGameObjectCollection gameObjects,
        ILogger<ShipRenderWindow> logger,
        IRenderer renderer,
        ITextureManager textureManager,
        ICamera camera,
        ICameraFactory cameraFactory,
        IModelBrowser modelBrowser,
        IScene scene,
        IWorldGrid worldGrid,
        IViewGizmo viewGizmo,
        ISettingsService settingsManager,
        IRecentFilesService recentFilesManager,
        ISceneHierarchyPanel scenePanel,
        ITransformPanel transformPanel,
        ICameraPanel cameraPanel,
        ILightPanel lightPanel,
        ISettingsPanel settingsPanel,
        IViewportInfoPanel viewportInfoPanel,
        IPropertiesPanel propertiesPanel,
        IAssetBrowserPanel assetBrowserPanel,
        ITexturePanel texturePanel,
        ITrackViewerPanel trackViewerPanel,
        ITrackDataPanel trackDataPanel,
        FileDialogManager fileDialogManager,
        ITrack track,
        ITrackFactory trackFactory)
        : base(gws, nws)
    {
        _gameObjects = gameObjects ?? throw new ArgumentNullException(nameof(gameObjects));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _textureManager = textureManager ?? throw new ArgumentNullException(nameof(textureManager));
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _cameraFactory = cameraFactory ?? throw new ArgumentNullException(nameof(cameraFactory));
        _modelBrowser = modelBrowser ?? throw new ArgumentNullException(nameof(modelBrowser));
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _worldGrid = worldGrid ?? throw new ArgumentNullException(nameof(worldGrid));
        _viewGizmo = viewGizmo ?? throw new ArgumentNullException(nameof(viewGizmo));
        _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        _recentFiles = recentFilesManager ?? throw new ArgumentNullException(nameof(recentFilesManager));
        _scenePanel = scenePanel ?? throw new ArgumentNullException(nameof(scenePanel));
        _transformPanel = transformPanel ?? throw new ArgumentNullException(nameof(transformPanel));
        _cameraPanel = cameraPanel ?? throw new ArgumentNullException(nameof(cameraPanel));
        _lightPanel = lightPanel ?? throw new ArgumentNullException(nameof(lightPanel));
        _settingsPanel = settingsPanel ?? throw new ArgumentNullException(nameof(settingsPanel));
        _viewportInfoPanel = viewportInfoPanel ?? throw new ArgumentNullException(nameof(viewportInfoPanel));
        _propertiesPanel = propertiesPanel ?? throw new ArgumentNullException(nameof(propertiesPanel));
        _assetBrowserPanel = assetBrowserPanel ?? throw new ArgumentNullException(nameof(assetBrowserPanel));
        _texturePanel = texturePanel ?? throw new ArgumentNullException(nameof(texturePanel));
        _trackViewerPanel = trackViewerPanel ?? throw new ArgumentNullException(nameof(trackViewerPanel));
        _trackDataPanel = trackDataPanel ?? throw new ArgumentNullException(nameof(trackDataPanel));
        _fileDialogManager = fileDialogManager ?? throw new ArgumentNullException(nameof(fileDialogManager));
        _track = track ?? throw new ArgumentNullException(nameof(track));
        _trackFactory = trackFactory ?? throw new ArgumentNullException(nameof(trackFactory));
    }

    #region methods

    protected override void OnLoad()
    {
        base.OnLoad();

        _renderer.Init(Size.X, Size.Y);

        // Inicializar câmera em modo 3D perspectiva (FOV 73.75° como no C original)
        var aspectRatio = (float)Size.X / Size.Y;
        _camera.SetAspectRatio(aspectRatio);
        _camera.SetIsometricMode(false);

        // Initialize scene with default camera (Scene já foi injetado via DI)
        // Create one default camera to start using factory
        var defaultCamera = _cameraFactory.CreateCamera(
            (float)_viewportWidth / _viewportHeight,
            73.75f);
        _mainCamera = _scene.CameraManager.AddCamera("Camera 1", defaultCamera);
        _scene.CameraManager.SetActiveCamera("Camera 1");
        // Keep _camera reference pointing to active camera for compatibility
        _camera = defaultCamera;

        // Initialize ImGui (with docking enabled)
        _imGuiController = new ImGuiController(Size.X, Size.Y);
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;  // Enable docking

        // Apply UI scale from settings (or detect from OS if not set)
        _currentUIScale = _settingsManager.Settings.UIScale;
        if (_currentUIScale <= 1.0f)
        {
            // Try to detect DPI if no custom scale is set
            float dpiScale = DetectDPIScale();
            if (dpiScale > 1.0f)
            {
                _currentUIScale = dpiScale;
                _settingsManager.SetUIScale(_currentUIScale);
            }
        }

        // Apply UI scale via ImGui
        io.FontGlobalScale = _currentUIScale;

        // Initialize viewport framebuffer
        InitializeFramebuffer();

        // Model browser is now injected via DI - starts empty, user can refresh manually

        // Load other settings
        _showViewport = _settingsManager.Settings.ShowViewport;
        _showTransform = _settingsManager.Settings.ShowTransform;

        // Configure UI Panels (já injetados via DI)
        _scenePanel.IsVisible = _showScene;
        _transformPanel.IsVisible = _showTransform;
        _cameraPanel.IsVisible = _showCamera;

        // Configure ViewportInfoPanel
        _viewportInfoPanel.OnResetCameraRequested += ResetCamera;
        _viewportInfoPanel.AutoRotate = _settingsManager.Settings.AutoRotate;
        _viewportInfoPanel.AutoRotateAxis = _settingsManager.Settings.AutoRotateAxis;

        // Configure PropertiesPanel
        _propertiesPanel.WireframeMode = _settingsManager.Settings.WireframeMode;
        _propertiesPanel.IsVisible = _settingsManager.Settings.ShowProperties;

        // Configure TexturePanel
        _texturePanel.IsVisible = _settingsManager.Settings.ShowTextures;

        // Configure AssetBrowserPanel
        _assetBrowserPanel.OnAddToSceneRequested += AddModelToScene;
        _assetBrowserPanel.IsVisible = _settingsManager.Settings.ShowAssetBrowser;

        // Configure TrackViewerPanel
        // Find the wipeout assets directory by going up from bin/Debug/net8.0 to project root
        string execDir = AppDomain.CurrentDomain.BaseDirectory;
        string? foundDir = FindWipeoutAssetsDirectory(execDir);
        string wipoutDataDir = foundDir ?? Path.GetFullPath(Path.Combine(execDir, "..", "..", "..", "..", "..", "assets", "wipeout"));
        _logger.LogInformation("[INIT] Wipeout assets directory: {Dir}", wipoutDataDir);
        _trackViewerPanel.SetWipeoutDataDirectory(wipoutDataDir);
        _trackViewerPanel.OnTrackLoadRequested += LoadTrack;
        _trackViewerPanel.IsVisible = _settingsManager.Settings.ShowTrackViewer;

        // Configure Directional Lights visibility
        _lightPanel.IsVisible = _settingsManager.Settings.ShowDirectionalLights;

        // Configure TrackDataPanel
        _trackDataPanel.IsVisible = _settingsManager.Settings.ShowTrackDataInspector;

        // Configure SettingsPanel
        _settingsPanel.SetUIScale(_currentUIScale);

        // Configure FileDialogManager
        _fileDialogManager.OnFilesSelected = files =>
        {
            _modelBrowser.ClearModels();
            _modelBrowser.AddModels(files);
            _logger.LogInformation("[EDITOR] Loaded {Count} PRM files into Asset Browser", files.Length);
        };
        _fileDialogManager.OnFolderSelected = folderPath => LoadPrmFolder(folderPath);

        // Load viewport visualization settings
        if (_worldGrid != null)
        {
            _worldGrid.ShowGrid = _settingsManager.Settings.ShowGrid;
            _worldGrid.ShowAxes = _settingsManager.Settings.ShowAxes;
        }

        // Set initial camera position
        ResetCamera();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        // Clear framebuffer at start of frame
        GL.ClearColor(0.15f, 0.15f, 0.18f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // FIRST: Render 3D viewport to FBO (completely isolated, before ImGui)
        RenderViewportToFBO();

        // SECOND: Render ImGui UI (uses FBO texture for display)
        _renderer.BeginFrame();
        if (_imGuiController != null)
        {
            _imGuiController.BeginFrame();
            RenderUIWithoutViewport();
            _imGuiController.EndFrame();
        }
        _renderer.EndFrame();

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        _renderer.UpdateScreenSize(e.Width, e.Height);
        _camera.SetAspectRatio((float)e.Width / e.Height);

        if (_imGuiController != null)
        {
            _imGuiController.WindowResized(e.Width, e.Height);
        }
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        if (_imGuiController != null)
        {
            _imGuiController.PressChar((char)e.Unicode);
        }
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
        {
            Close();
        }

        // Update ImGui input BEFORE frame processing
        if (_imGuiController != null)
        {
            _imGuiController.UpdateMousePosition(MouseState.X, MouseState.Y);
            _imGuiController.UpdateMouseButton(0, MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left));
            _imGuiController.UpdateMouseButton(1, MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right));
            _imGuiController.UpdateMouseButton(2, MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle));

            if (MouseState.ScrollDelta.Y != 0)
            {
                _imGuiController.UpdateMouseScroll(MouseState.ScrollDelta.Y);
            }

            // Update keyboard state for ImGui
            _imGuiController.UpdateKeyboard(KeyboardState);
        }

        // If auto-rotate enabled, rotate the ship automatically
        // Auto-rotate selected object
        if (_viewportInfoPanel != null &&
            _viewportInfoPanel.AutoRotate &&
            _scene.SelectedObject != null &&
            _scene.SelectedObject?.Ship != null)
        {
            _totalTime += (float)args.Time;

            if (_scene.SelectedObject?.Rotation == null)
                return;

            var angle = _scene.SelectedObject.Rotation;
            float rotationSpeed = 2.0f * (float)args.Time;  // 2 rad/s

            // Auto rotate no eixo selecionado
            if (_viewportInfoPanel.AutoRotateAxis == 0)  // X axis
            {
                angle.X += rotationSpeed;
                if (angle.X > MathF.PI * 2)
                    angle.X -= MathF.PI * 2;
            }
            else if (_viewportInfoPanel.AutoRotateAxis == 1)  // Y axis
            {
                angle.Y += rotationSpeed;
                if (angle.Y > MathF.PI * 2)
                    angle.Y -= MathF.PI * 2;
            }
            else if (_viewportInfoPanel.AutoRotateAxis == 2)  // Z axis
            {
                angle.Z += rotationSpeed;
                if (angle.Z > MathF.PI * 2)
                    angle.Z -= MathF.PI * 2;
            }

            _scene.SelectedObject.Rotation = angle;
        }

        _gameObjects.Update();

        // Update track animation (pickups blinking)
        if (_trackAnimator != null)
        {
            _trackAnimator.Update((float)args.Time);
        }
    }

    /// <summary>
    /// Add ALL models from a PRM file.
    /// This loads all objects from the specified PRM file and their corresponding textures.
    /// </summary>
    private void AddAllModelsFromPrmFile(string prmPath, bool isTrack, Vec3? trackPosition = null, float trackScale = 1.0f, bool isSky = false, Vec3? skyOffset = null)
    {
        _logger.LogWarning("[SCENE] *** AddAllModelsFromPrmFile CALLED with path: {Path}, isTrack: {IsTrack} ***", prmPath, isTrack);

        try
        {
            _logger.LogWarning("[SCENE] Loading ALL objects from track file: {Path}", prmPath);

            // Create a logger that will show output
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole();
            });
            var modelLoaderLogger = loggerFactory.CreateLogger<ModelLoader>();
            var modelLoader = new ModelLoader(modelLoaderLogger);

            // Load ALL objects from the PRM file at once
            var allMeshes = modelLoader.LoadAllObjectsFromPrmFile(prmPath);

            _logger.LogInformation("[SCENE] Loaded {Count} objects from {Path}", allMeshes.Count, prmPath);

            // Load CMP textures
            string cmpPath = Path.ChangeExtension(prmPath, ".cmp");
            int[]? textures = null;
            if (File.Exists(cmpPath))
            {
                _logger.LogInformation("[SCENE] Loading textures from {Cmp}", cmpPath);
                textures = _textureManager.LoadTexturesFromCmp(cmpPath);
            }

            // Create a GameObject for each mesh
            float spacing = 50.0f;
            int addedCount = 0;
            var assetResolver = new AssetPathResolver(new NullLogger<AssetPathResolver>());

            foreach (var mesh in allMeshes)
            {
                var shipLogger = new NullLogger<GameObject>();
                var newObject = new GameObject(_renderer, shipLogger, _textureManager, modelLoader, assetResolver)
                {
                    // Track/scene/sky meshes come already oriented
                    Angle = new Vec3(0, 0, 0)
                };

                // Set the model directly (we already loaded it!)
                newObject.SetModel(mesh);

                // Apply textures with UV normalization
                if (textures != null)
                {
                    newObject.ApplyTexturesWithNormalization(textures);
                }

                newObject.IsVisible = true;

                var sceneObject = _scene.AddObject(mesh.Name, newObject);

                // Scene/sky objects use their mesh Origin directly (from PRM int32 origin field)
                // Like C: mat4_set_translation(&obj->mat, obj->origin)
                if (isSky)
                {
                    sceneObject.Scale = 1.0f;
                }

                if (trackPosition.HasValue || trackScale != 1.0f || !isSky)
                {
                    // Use the mesh's Origin directly from PRM file (int32 values cast to float)
                    sceneObject.Position = mesh.Origin;  // No inversion - C uses raw coordinates
                    // Sky uses scale 1.0f, scene/track objects use 0.001f scale
                    sceneObject.Scale = 0.001f;  // Sky vs regular objects
                    _logger.LogInformation("[SCENE] Using origin for '{Name}': origin={Origin}, scale={Scale}", mesh.Name, mesh.Origin, sceneObject.Scale);
                }
                else
                {
                    // Original behavior for non-track files loaded manually
                    float xOffset = _scene.Objects.Count * spacing;
                    sceneObject.Position = new Vec3(xOffset, 0, 0);
                    // Keep default scale of 0.001f for all objects (PSX coordinate scale)
                }

                // Use identity rotation like in C: mat4_identity()
                sceneObject.Rotation = new Vec3(0, 0, 0);  // No rotation
                sceneObject.SourceFilePath = prmPath;  // Track the source file
                sceneObject.IsSky = isSky;
                sceneObject.SkyOffset = skyOffset ?? new Vec3(0, 0, 0);  // No inversion

                addedCount++;
                _logger.LogInformation("[SCENE] Added '{Name}' to scene at position {Pos}", mesh.Name, sceneObject.Position);
            }

            _logger.LogInformation("[SCENE] Added {Count} objects from {Path} to scene", addedCount, prmPath);

            // Select last added object
            if (_scene.Objects.Count > 0)
                _scene.SelectedObject = _scene.Objects[_scene.Objects.Count - 1];

            _recentFiles.AddRecentFile(prmPath, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SCENE] Failed to add all models from track file: {Path}", prmPath);
        }
    }

    /// <summary>
    /// Add a model from Asset Browser to the Scene
    /// </summary>
    private void AddModelToScene(string modelPath, int objectIndex)
    {
        _logger.LogWarning("[SCENE] *** AddModelToScene called: path={Path}, index={Index} ***", modelPath, objectIndex);

        try
        {
            // For regular files or specific object index, load single object
            var shipLogger = new NullLogger<GameObject>();
            var modelLoaderLogger = new NullLogger<ModelLoader>();
            var modelLoader = new ModelLoader(modelLoaderLogger);
            var assetResolver = new AssetPathResolver(new NullLogger<AssetPathResolver>());
            var newShip = new GameObject(_renderer, shipLogger, _textureManager, modelLoader, assetResolver);
            newShip.LoadModelFromPath(modelPath, objectIndex);
            newShip.IsVisible = true;

            _logger.LogInformation("[SCENE] Ship created: Model={HasModel}, Textures={TexCount}, ShadowTexture={Shadow}",
                newShip.Model != null, newShip.Texture?.Length ?? 0, newShip.ShadowTexture);

            // Validate that model was loaded
            if (newShip.Model == null)
            {
                _logger.LogError("[SCENE] Failed to load model from {Path} (index {Index})", modelPath, objectIndex);
                return;
            }

            // Get object name from ModelBrowser using the path and index received
            string objectName = "Object";
            var file = _modelBrowser.PrmFiles.FirstOrDefault(f => f.FilePath == modelPath);
            if (file != null)
            {
                var obj = file.Objects.FirstOrDefault(o => o.Index == objectIndex);
                if (obj != null)
                {
                    objectName = obj.Name;
                }
                else if (file.Objects.Count > 0)
                {
                    objectName = file.Objects[0].Name;
                }
            }

            // Fallback: use filename if object name not found
            if (objectName == "Object" && !string.IsNullOrEmpty(modelPath))
            {
                objectName = Path.GetFileNameWithoutExtension(modelPath);
            }

            // Calculate spawn position with spacing to avoid overlap
            // Space objects 50 units apart in X direction
            float spacing = 50.0f;
            float xOffset = _scene.Objects.Count * spacing;

            // Add to scene with ship
            var sceneObject = _scene.AddObject(objectName, newShip);
            sceneObject.Position = new Vec3(xOffset, 0, 0);  // Offset each object
            //sceneObject.Rotation = new Vec3(0, 0, MathF.PI); // 180° rotation in Z to orient correctly
            // Keep default scale of 1.0f (set in SceneObject constructor)
            sceneObject.SourceFilePath = modelPath;  // Track the source file
            _scene.SelectedObject = sceneObject;

            _logger.LogInformation($"[SCENE] Added {sceneObject.Name} to scene");

            // Calculate and adjust grid position to match the model's bounding box
            // The grid should be positioned at the lowest point of the model
            var (minY, maxY) = newShip.GetModelBounds();

            // Calculate world-space Y position: model's minY scaled and offset by object's position
            // minY is in model space, multiply by scale to get world space distance
            // Add object's Y position to get final world position
            float modelMinYWorldSpace = (minY * sceneObject.Scale) + sceneObject.Position.Y;
            _worldGrid.GridYPosition = modelMinYWorldSpace;
            _logger.LogInformation($"[SCENE] Set GridYPosition to {modelMinYWorldSpace:F2} (model bounds: {minY:F2} to {maxY:F2}, scale: {sceneObject.Scale:F2})");

            // Add to recent files
            _recentFiles.AddRecentFile(modelPath, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SCENE] Failed to add model to scene: {Path}", modelPath);
        }
    }

    private int[] BuildTrackLibraryTextures(string ttfPath, string cmpPath)
    {
        if (!File.Exists(ttfPath) || !File.Exists(cmpPath))
        {
            _logger.LogWarning("[TRACK] Missing TTF ({Ttf}) or CMP ({Cmp}) for track textures", ttfPath, cmpPath);
            return Array.Empty<int>();
        }

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Warning);
        });
        var cmpLoader = new CmpImageLoader(loggerFactory.CreateLogger<CmpImageLoader>());
        var timLoader = new TimImageLoader(loggerFactory.CreateLogger<TimImageLoader>());

        byte[][] cmpImages = cmpLoader.LoadCompressed(cmpPath);
        if (cmpImages.Length == 0)
        {
            _logger.LogWarning("[TRACK] CMP had no images: {Cmp}", cmpPath);
            return Array.Empty<int>();
        }

        byte[] ttfBytes = File.ReadAllBytes(ttfPath);
        int tileCount = ttfBytes.Length / 42; // 16 near (32 bytes) + 4 med (8 bytes) + 1 far (2 bytes)
        var handles = new int[tileCount];

        int p = 0;
        for (int tile = 0; tile < tileCount; tile++)
        {
            short[] near = new short[16];
            for (int i = 0; i < 16; i++) near[i] = ReadI16BE(ttfBytes, ref p);
            for (int i = 0; i < 4; i++) ReadI16BE(ttfBytes, ref p); // med (unused)
            ReadI16BE(ttfBytes, ref p); // far (unused)

            byte[] pixels = new byte[128 * 128 * 4];

            for (int ty = 0; ty < 4; ty++)
            {
                for (int tx = 0; tx < 4; tx++)
                {
                    int idx = near[ty * 4 + tx];
                    if (idx < 0 || idx >= cmpImages.Length)
                    {
                        continue;
                    }

                    var (subPixels, w, h) = timLoader.LoadTimFromBytes(cmpImages[idx], false);
                    if (subPixels == null || subPixels.Length == 0)
                        continue;

                    int destX = tx * 32;
                    int destY = ty * 32;
                    int copyW = Math.Min(w, 32);
                    int copyH = Math.Min(h, 32);

                    for (int y = 0; y < copyH; y++)
                    {
                        for (int x = 0; x < copyW; x++)
                        {
                            int srcIndex = (y * w + x) * 4;
                            int dstIndex = ((destY + y) * 128 + (destX + x)) * 4;
                            pixels[dstIndex + 0] = subPixels[srcIndex + 0];
                            pixels[dstIndex + 1] = subPixels[srcIndex + 1];
                            pixels[dstIndex + 2] = subPixels[srcIndex + 2];
                            pixels[dstIndex + 3] = subPixels[srcIndex + 3];
                        }
                    }
                }
            }

            handles[tile] = _textureManager.CreateTexture(pixels, 128, 128);
        }

        _logger.LogInformation("[TRACK] Built {Count} track tiles from {Ttf}", handles.Length, ttfPath);
        return handles;
    }

    private static (Vec3 min, Vec3 max) ComputeBounds(Mesh mesh)
    {
        if (mesh.Vertices == null || mesh.Vertices.Length == 0)
            return (new Vec3(0, 0, 0), new Vec3(0, 0, 0));

        float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

        foreach (var v in mesh.Vertices)
        {
            minX = MathF.Min(minX, v.X);
            minY = MathF.Min(minY, v.Y);
            minZ = MathF.Min(minZ, v.Z);
            maxX = MathF.Max(maxX, v.X);
            maxY = MathF.Max(maxY, v.Y);
            maxZ = MathF.Max(maxZ, v.Z);
        }

        return (new Vec3(minX, minY, minZ), new Vec3(maxX, maxY, maxZ));
    }

    /// <summary>
    /// Detect DPI scaling from the operating system.
    /// </summary>
    private float DetectDPIScale()
    {
        try
        {
            // Try to get DPI from the current monitor
            var monitor = CurrentMonitor;
            if (monitor != null)
            {
                // Get monitor DPI
                var dpi = monitor.HorizontalDpi;

                // Standard DPI is 96, calculate scale factor
                const float standardDpi = 96.0f;
                float scale = dpi / standardDpi;

                _logger.LogInformation("[DPI] Detected monitor DPI: {Dpi} (scale: {Scale}x)", dpi, scale);

                // Clamp to reasonable values (between 1.0 and 3.0)
                return Math.Clamp(scale, 1.0f, 3.0f);
            }

            // Fallback: try to get from window scale
            var windowScale = ClientSize.X / (float)Size.X;
            if (windowScale > 1.0f && windowScale <= 3.0f)
            {
                _logger.LogInformation("[DPI] Using window scale: {Scale}x", windowScale);
                return windowScale;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[DPI] Failed to detect DPI scale, using default");
        }

        // Default to 1.0 (100%)
        return 1.0f;
    }

    /// <summary>
    /// Find the wipeout assets directory by searching up from the current directory.
    /// </summary>
    private string? FindWipeoutAssetsDirectory(string startDir)
    {
        DirectoryInfo? current = new DirectoryInfo(startDir);

        // Search up to 10 levels up
        for (int i = 0; i < 10 && current != null; i++)
        {
            string assetsPath = Path.Combine(current.FullName, "assets", "wipeout");
            if (Directory.Exists(assetsPath))
            {
                _logger.LogInformation("[INIT] Found wipeout assets at: {Path}", assetsPath);
                return assetsPath;
            }

            current = current.Parent;
        }

        return null;
    }

    private float GetSkyYOffset(int trackNumber)
    {
        return trackNumber switch
        {
            1 => -820f,
            2 => -2520f,
            3 => -1930f,
            4 => -5000f,
            5 => -5000f,
            6 => 0f,
            7 => -2260f,
            8 => -40f,
            9 => -2700f,
            10 => 0f,
            11 => -240f,
            12 => -2120f,
            13 => -2700f,
            14 => 0f,
            _ => 0f
        };
    }

    /// <summary>
    /// Initialize framebuffer for viewport rendering.
    /// </summary>
    private void InitializeFramebuffer()
    {
        // Generate framebuffer
        _viewportFBO = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _viewportFBO);

        // Create texture for color attachment
        _viewportTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _viewportTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb,
            _viewportWidth, _viewportHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, _viewportTexture, 0);

        // Create renderbuffer for depth/stencil
        _viewportRBO = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _viewportRBO);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8,
            _viewportWidth, _viewportHeight);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
            RenderbufferTarget.Renderbuffer, _viewportRBO);

        // Check framebuffer status
        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
        {
            _logger.LogError("[VIEWPORT] Framebuffer is not complete!");
        }

        // Unbind framebuffer
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _logger.LogInformation("[VIEWPORT] Framebuffer initialized: {Width}x{Height}", _viewportWidth, _viewportHeight);
    }

    /// <summary>
    /// Load all PRM files from a folder.
    /// </summary>
    private void LoadPrmFolder(string folderPath)
    {
        // Fire and forget - UI stays responsive
        _ = LoadPrmFolderAsync(folderPath);
    }

    /// <summary>
    /// Load all PRM files from a folder (async with progress).
    /// </summary>
    private async Task LoadPrmFolderAsync(string folderPath)
    {
        _isLoading = true;
        _loadingMessage = "Loading models...";
        _loadingProgress = 0;
        _loadingTotal = 0;

        var progress = new Progress<(int current, int total, string fileName)>(report =>
        {
            _loadingProgress = report.current;
            _loadingTotal = report.total;
            _loadingMessage = $"Loading {report.fileName}... ({report.current}/{report.total})";
        });

        try
        {
            await _modelBrowser.LoadFromFolderAsync(folderPath, progress);

            // Log summary
            int totalModels = _modelBrowser.PrmFiles.Sum(f => f.Objects.Count);
            _logger.LogInformation("[EDITOR] Loaded {Files} PRM files with {Models} total models",
                _modelBrowser.PrmFiles.Count, totalModels);

            // Show asset browser if hidden
            if (_assetBrowserPanel != null)
                _assetBrowserPanel.IsVisible = true;

            // Add to recent folders
            _recentFiles.AddRecentFile(folderPath, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EDITOR] Failed to load PRM folder: {Path}", folderPath);
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// Load an entire track for visualization.
    /// Loads track.trv + track.trf geometry, scene.prm, sky.prm and their textures, then adds them to the scene.
    /// </summary>
    private void LoadTrack(int trackNumber, string wipoutDataDir)
    {
        _logger.LogWarning("[TRACK] *** LoadTrack called: trackNumber={Track}, dataDir={Dir} ***", trackNumber, wipoutDataDir);

        try
        {
            // Clear existing objects
            var objectsToRemove = _scene.Objects.ToList();
            foreach (var obj in objectsToRemove)
            {
                _scene.RemoveObject(obj);
            }
            _logger.LogInformation("[TRACK] Cleared {Count} existing objects", objectsToRemove.Count);

            // Reset track data and spline navigation for new track
            _scene.TrackLoader = null;
            _scene.ActiveTrack = null;
            _splineNavigationCalculator = null;
            _splineDebugRenderer = null;
            _logger.LogInformation("[TRACK] Reset track data, spline navigation calculator and renderer for new track");

            // Build track directory path
            string trackDir = Path.Combine(wipoutDataDir, $"track{trackNumber:D2}");
            if (!Directory.Exists(trackDir))
            {
                _logger.LogError("[TRACK] Track directory not found: {Dir}", trackDir);
                return;
            }

            string scenePrmPath = Path.Combine(trackDir, "scene.prm");
            string skyPrmPath = Path.Combine(trackDir, "sky.prm");
            string trvPath = Path.Combine(trackDir, "track.trv");
            string trfPath = Path.Combine(trackDir, "track.trf");
            string libraryTtfPath = Path.Combine(trackDir, "library.ttf");
            string libraryCmpPath = Path.Combine(trackDir, "library.cmp");

            _logger.LogWarning("[TRACK] Loading track {Track} from {Dir}", trackNumber, trackDir);

            // Track object to be populated with sections for fly-through navigation
            //var track = new Track($"track{trackNumber:D2}");
            Mesh? trackMesh = null;

            // Load track geometry first (main track mesh)
            if (File.Exists(trvPath) && File.Exists(trfPath))
            {
                _logger.LogInformation("[TRACK] Loading track geometry from {Trv} and {Trf}", trvPath, trfPath);
                try
                {
                    // Create logger for TrackLoader
                    var trackLoaderLogger = LoggerFactory.Create(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Debug);
                        builder.AddConsole();
                    }).CreateLogger<TrackLoader>();

                    var trackLoader = new TrackLoader(trackLoaderLogger);
                    trackLoader.LoadVertices(trvPath);
                    trackLoader.LoadFaces(trfPath);
                    trackMesh = trackLoader.ConvertToMesh();

                    // Store track loader in scene for diagnostic panel access
                    _scene.TrackLoader = trackLoader;

                    _logger.LogInformation("[TRACK] Loaded track mesh with {Vertices} vertices and {Primitives} primitives",
                        trackMesh.Vertices.Length, trackMesh.Primitives.Count);

                    // Initialize track animator for pickup/boost zone animations
                    var animatorLogger = LoggerFactory.Create(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Information);
                    }).CreateLogger<TrackAnimator>();

                    _trackAnimator = new TrackAnimator(animatorLogger);
                    _trackAnimator.RegisterAnimatedFaces(trackMesh, trackLoader.LoadedFaces ?? new());

                    // No scaling or centering - track is rendered at original coordinates like in C
                    // Track uses identity matrix transformation

                    // Load library textures for the track
                    int[]? libraryTextures = null;
                    if (File.Exists(libraryCmpPath) && File.Exists(libraryTtfPath))
                    {
                        _logger.LogInformation("[TRACK] Building track textures from {Ttf} + {Cmp}", libraryTtfPath, libraryCmpPath);
                        libraryTextures = BuildTrackLibraryTextures(libraryTtfPath, libraryCmpPath);
                    }

                    // Create GameObject for track
                    var trackLogger = LoggerFactory.Create(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Debug);
                        builder.AddConsole();
                    }).CreateLogger<GameObject>();

                    var modelLoaderLogger = LoggerFactory.Create(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Debug);
                        builder.AddConsole();
                    }).CreateLogger<ModelLoader>();

                    var assetResolverLogger = LoggerFactory.Create(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Debug);
                        builder.AddConsole();
                    }).CreateLogger<AssetPathResolver>();

                    var trackObject = new GameObject(_renderer, trackLogger, _textureManager, new ModelLoader(modelLoaderLogger), new AssetPathResolver(assetResolverLogger))
                    {
                        Angle = new Vec3(0, 0, 0)
                    };
                    trackObject.SetModel(trackMesh);

                    // Apply library textures
                    if (libraryTextures != null)
                    {
                        trackObject.ApplyTexturesWithNormalization(libraryTextures);
                    }

                    trackObject.IsVisible = true;

                    var trackSceneObject = _scene.AddObject("Track Geometry", trackObject);
                    // Track coordinates are int32 values (tens/hundreds of thousands)
                    // Apply 0.001f scale to convert PSX coordinates to reasonable viewing scale
                    trackSceneObject.Position = new Vec3(0, 0, 0);
                    trackSceneObject.Rotation = new Vec3(0, 0, 0);  // Track uses identity rotation
                    trackSceneObject.Scale = 0.001f;  // Scale down from PSX coordinates
                    trackSceneObject.SourceFilePath = trvPath;

                    _logger.LogInformation("[TRACK] Added track geometry to scene");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[TRACK] Failed to load track geometry: {Message}", ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("[TRACK] Track geometry files not found: {Trv} or {Trf}", trvPath, trfPath);
            }

            // Scene and sky objects use their own origins directly (no track-based transform)
            // Like in C: scene objects have mat4_set_translation(&obj->mat, obj->origin)

            // Load scene first (buildings, track elements)
            if (File.Exists(scenePrmPath))
            {
                _logger.LogInformation("[TRACK] Loading scene.prm: {Path}", scenePrmPath);
                AddAllModelsFromPrmFile(scenePrmPath, false, null, 1.0f);
            }
            else
            {
                _logger.LogWarning("[TRACK] scene.prm not found: {Path}", scenePrmPath);
            }

            // Load sky (skybox elements)
            if (File.Exists(skyPrmPath))
            {
                _logger.LogInformation("[TRACK] Loading sky.prm: {Path}", skyPrmPath);
                float skyYOffset = GetSkyYOffset(trackNumber);  // No scaling - use raw value
                var skyOffsetVec = new Vec3(0, skyYOffset, 0);
                _logger.LogInformation("[TRACK] Sky offset for track {Track}: Y={SkyY}", trackNumber, skyYOffset);
                AddAllModelsFromPrmFile(skyPrmPath, false, null, 1.0f, true, skyOffsetVec);
            }
            else
            {
                _logger.LogWarning("[TRACK] sky.prm not found: {Path}", skyPrmPath);
            }

            _logger.LogInformation("[TRACK] Track {Track} loaded successfully with {Count} objects",
                trackNumber, _scene.Objects.Count);

            // Create a new Track instance for this load to avoid stale data
            var newTrack = _trackFactory.Create() as Track;
            if (newTrack == null)
            {
                _logger.LogError("[TRACK] Failed to create Track instance from factory");
                return;
            }
            
            // Load track sections from track.trs file (pre-calculated by wipeout track editor)
            LoadTrackSections(trackDir, newTrack);

            _scene.ActiveTrack = newTrack;
            _logger.LogInformation("[TRACK] Set ActiveTrack to {TrackName} with {SectionCount} sections",
                newTrack.Name, newTrack.Sections.Count);

            // Make all objects visible and ensure they are properly rendered
            foreach (var obj in _scene.Objects)
            {
                obj.IsVisible = true;
                _logger.LogDebug("[TRACK] Object: {Name}, Visible: {Visible}", obj.Name, obj.IsVisible);
            }

            // Select first object for camera focus and reset grid
            if (_scene.Objects.Count > 0)
            {
                _scene.SelectedObject = _scene.Objects[0];

                // Adjust grid position to ground level
                _worldGrid.GridYPosition = 0.0f;
                _logger.LogInformation("[TRACK] Set grid position to Y=0");

                // Position camera at reasonable distance for full track coordinates (int32 values)
                // Track coordinates are in the tens of thousands range, so use fixed reasonable values
                var trackGeometry = _scene.Objects.FirstOrDefault(o => o.Name == "Track Geometry")?.Ship;
                if (trackGeometry?.Model?.Vertices != null && trackGeometry.Model.Vertices.Length > 0)
                {
                    // Use fixed camera position values that work well with PSX-scale coordinates
                    // Similar to external camera in game: ~1000-2000 units back and ~200-500 units up
                    _camera.Position = new Vector3(0, 2000, 5000);
                    _camera.Target = new Vector3(0, 0, 0);
                    _logger.LogInformation("[TRACK] Camera positioned at fixed distance for PSX coordinates");
                }
            }

            _logger.LogWarning("[TRACK] Track {Track} rendering should start now", trackNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TRACK] Failed to load track {Track}: {Message}", trackNumber, ex.Message);
        }
    }

    /// <summary>
    /// Load track sections from track.trs file (pre-calculated by track editor).
    /// This matches wipeout-rewrite, which loads sections instead of generating synthetically.
    /// File format is big-endian (PSX format).
    /// </summary>
    private void LoadTrackSections(string trackDataPath, ITrack track)
    {
        const int SECTION_DATA_SIZE = 156;  // bytes per section in track.trs
        const float TRACK_SCALE = 0.001f;

        string trsPath = Path.Combine(trackDataPath, "track.trs");
        if (!File.Exists(trsPath))
        {
            _logger.LogWarning("[TRACK] track.trs not found at {Path}, cannot load sections", trsPath);
            return;
        }

        try
        {
            byte[] trsData = File.ReadAllBytes(trsPath);
            int sectionCount = trsData.Length / SECTION_DATA_SIZE;

            _logger.LogInformation("[TRACK] Loading {SectionCount} track sections from track.trs", sectionCount);

            // First pass: load all section data
            for (int i = 0; i < sectionCount; i++)
            {
                int baseOffset = i * SECTION_DATA_SIZE;

                // Read section structure (BIG ENDIAN - PSX format)
                // matching wipeout-rewrite track.c:205-245

                int junctionIndex = ReadInt32BE(trsData, baseOffset + 0);
                int prevIndex = ReadInt32BE(trsData, baseOffset + 4);
                int nextIndex = ReadInt32BE(trsData, baseOffset + 8);

                // Center coordinates (int32 values in PSX coords)
                int centerX = ReadInt32BE(trsData, baseOffset + 12);
                int centerY = ReadInt32BE(trsData, baseOffset + 16);
                int centerZ = ReadInt32BE(trsData, baseOffset + 20);

                // Face data
                short faceStart = ReadInt16BE(trsData, baseOffset + 104);
                short faceCount = ReadInt16BE(trsData, baseOffset + 106);

                // Flags and section number
                short flags = ReadInt16BE(trsData, baseOffset + 132);
                short sectionNum = ReadInt16BE(trsData, baseOffset + 134);

                var section = new TrackSection
                {
                    Center = new Vec3(centerX, centerY, centerZ),
                    SectionNumber = i,
                    FaceStart = faceStart,
                    FaceCount = faceCount,
                    Flags = flags,
                    Prev = null,  // Will be linked in second pass
                    Next = null,
                    Junction = null
                };

                track.Sections.Add(section);

                // Log first, middle and last sections
                if (i == 0 || i == sectionCount - 1 || i == sectionCount / 2)
                {
                    _logger.LogInformation(
                        "[TRACK] Section {Index}: Center=({X}, {Y}, {Z}) World=({WX:F2}, {WY:F2}, {WZ:F2}), Flags={Flags}",
                        i, centerX, centerY, centerZ,
                        centerX * TRACK_SCALE, centerY * TRACK_SCALE, centerZ * TRACK_SCALE, flags);
                }
            }

            // Second pass: link sections using indices
            for (int i = 0; i < sectionCount; i++)
            {
                int baseOffset = i * SECTION_DATA_SIZE;

                int junctionIndex = ReadInt32BE(trsData, baseOffset + 0);
                int prevIndex = ReadInt32BE(trsData, baseOffset + 4);
                int nextIndex = ReadInt32BE(trsData, baseOffset + 8);

                // Link sections
                if (junctionIndex >= 0 && junctionIndex < sectionCount)
                {
                    track.Sections[i].Junction = track.Sections[junctionIndex];
                }
                if (prevIndex >= 0 && prevIndex < sectionCount)
                {
                    track.Sections[i].Prev = track.Sections[prevIndex];
                }
                if (nextIndex >= 0 && nextIndex < sectionCount)
                {
                    track.Sections[i].Next = track.Sections[nextIndex];
                }
            }

            // Third pass: detect and smooth out problem sections
            // Calculate average distance between consecutive sections (for dynamic threshold)
            float totalDistance = 0;
            int distanceCount = 0;

            for (int i = 1; i < sectionCount; i++)
            {
                var prev = track.Sections[i - 1].Center;
                var curr = track.Sections[i].Center;
                float dx = curr.X - prev.X;
                float dy = curr.Y - prev.Y;
                float dz = curr.Z - prev.Z;
                float dist = MathF.Sqrt(dx * dx + dy * dy + dz * dz);
                totalDistance += dist;
                distanceCount++;
            }

            float averageDistance = totalDistance / distanceCount;
            float OUTLIER_THRESHOLD = averageDistance * 3.0f;  // 3x average distance is suspect

            _logger.LogInformation("[TRACK] Average section distance: {AvgDist:F0} PSX units, outlier threshold: {Threshold:F0}",
                averageDistance, OUTLIER_THRESHOLD);

            int smoothedCount = 0;
            for (int i = 1; i < sectionCount - 1; i++)
            {
                var prev = track.Sections[i - 1].Center;
                var curr = track.Sections[i].Center;
                var next = track.Sections[i + 1].Center;

                float dx = curr.X - prev.X;
                float dy = curr.Y - prev.Y;
                float dz = curr.Z - prev.Z;
                float dist = MathF.Sqrt(dx * dx + dy * dy + dz * dz);

                if (dist > OUTLIER_THRESHOLD)
                {
                    // Detected outlier, smooth by averaging with neighbors
                    Vec3 smoothedCenter = new Vec3(
                        (prev.X + next.X) / 2,
                        (prev.Y + next.Y) / 2,
                        (prev.Z + next.Z) / 2
                    );
                    _track.Sections[i].Center = smoothedCenter;
                    smoothedCount++;

                    _logger.LogWarning("[TRACK] Smoothed outlier section {Index} (dist={Dist:F0}) to ({X}, {Y}, {Z})",
                        i, dist,
                        smoothedCenter.X,
                        smoothedCenter.Y,
                        smoothedCenter.Z);
                }
            }

            _logger.LogInformation("[TRACK] Outlier smoothing disabled - preserving original section geometry");

            // Fourth pass: identify all junction sections
            var junctionSections = new List<int>();
            for (int i = 0; i < sectionCount; i++)
            {
                if (track.Sections[i].Junction != null)
                {
                    junctionSections.Add(i);
                }
            }

            if (junctionSections.Count > 0)
            {
                _logger.LogInformation("[TRACK] === Junction sections ({Count} found) ===", junctionSections.Count);
                for (int i = 0; i < junctionSections.Count && i < 20; i++)
                {
                    int idx = junctionSections[i];
                    var sect = track.Sections[idx];
                    var juncSect = sect.Junction;

                    if (juncSect == null) continue;

                    _logger.LogInformation(
                        "[TRACK] Sec[{Index:D3}] has Junction->Sec[{JuncIdx:D3}]: " +
                        "Main Next={NextIdx}, Junc Next={JuncNextIdx}, " +
                        "Pos=({X},{Y},{Z})->{JX},{JY},{JZ}",
                        idx, juncSect.SectionNumber,
                        sect.Next != null ? sect.Next.SectionNumber : -1,
                        juncSect.Next != null ? juncSect.Next.SectionNumber : -1,
                        (int)sect.Center.X, (int)sect.Center.Y, (int)sect.Center.Z,
                        (int)juncSect.Center.X, (int)juncSect.Center.Y, (int)juncSect.Center.Z);
                }

                _logger.LogWarning("[TRACK] Track has {JunctionCount} bifurcation points - spline may diverge here",
                    junctionSections.Count);
            }

            _logger.LogInformation("[TRACK] Loaded {SectionCount} track sections from track.trs (wipeout-rewrite algorithm)", track.Sections.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError("[TRACK] Error loading track.trs: {Error}", ex.Message);
        }
    }

    /// <summary>
    /// Open a folder dialog and load all PRM files from the selected folder.
    /// </summary>
    private void OpenFolderDialog()
    {
        try
        {
            _logger.LogInformation("[EDITOR] Opening folder dialog...");

            string? selectedFolder = ModelFileDialog.OpenFolderDialog("Select folder with PRM files");

            if (!string.IsNullOrEmpty(selectedFolder))
            {
                LoadPrmFolder(selectedFolder);
            }
            else
            {
                // Fallback: Show manual input dialog
                _logger.LogInformation("[EDITOR] Native dialog not available, showing manual input");
                _fileDialogManager?.ShowFolderDialog(ModelFileDialog.GetModelDirectory());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EDITOR] Error opening folder dialog");
            _fileDialogManager?.ShowFolderDialog(ModelFileDialog.GetModelDirectory());
        }
    }

    private static short ReadI16BE(byte[] bytes, ref int p)
    {
        short value = (short)((bytes[p] << 8) | bytes[p + 1]);
        p += 2;
        return value;
    }

    /// <summary>
    /// Helper to read big-endian int16 from byte array.
    /// </summary>
    private static short ReadInt16BE(byte[] data, int offset)
    {
        return (short)((data[offset] << 8) | data[offset + 1]);
    }

    /// <summary>
    /// Helper to read big-endian int32 from byte array.
    /// </summary>
    private static int ReadInt32BE(byte[] data, int offset)
    {
        return (data[offset] << 24) | (data[offset + 1] << 16) |
               (data[offset + 2] << 8) | data[offset + 3];
    }

    /// <summary>
    /// Render loading indicator overlay when async operations are in progress.
    /// </summary>
    private void RenderLoadingIndicator()
    {
        if (!_isLoading) return;

        // Center modal overlay
        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.GetCenter(), ImGuiCond.Always, new System.Numerics.Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 120));

        var flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.NoTitleBar;

        if (ImGui.Begin("##LoadingIndicator", flags))
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);

            // Spinner animation
            var time = (float)(_totalTime * 4.0);
            var spinnerRadius = 20.0f;
            var center = new System.Numerics.Vector2(
                ImGui.GetWindowWidth() / 2,
                40
            );

            var drawList = ImGui.GetWindowDrawList();
            var windowPos = ImGui.GetWindowPos();
            var spinnerCenter = new System.Numerics.Vector2(
                windowPos.X + center.X,
                windowPos.Y + center.Y
            );

            for (int i = 0; i < 8; i++)
            {
                var angle = (time + i * MathF.PI / 4.0f) % (MathF.PI * 2.0f);
                var alpha = (byte)(255 * (1.0f - i / 8.0f));
                var color = ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0.3f, 0.6f, 1.0f, alpha / 255.0f));

                var pos = new System.Numerics.Vector2(
                    spinnerCenter.X + MathF.Cos(angle) * spinnerRadius,
                    spinnerCenter.Y + MathF.Sin(angle) * spinnerRadius
                );

                drawList.AddCircleFilled(pos, 3.0f, color);
            }

            ImGui.SetCursorPosY(60);

            // Progress text
            var text = _loadingTotal > 0
                ? $"{_loadingMessage} ({_loadingProgress}/{_loadingTotal})"
                : _loadingMessage;

            var textSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPosX((ImGui.GetWindowWidth() - textSize.X) / 2);
            ImGui.Text(text);

            // Progress bar
            if (_loadingTotal > 0)
            {
                ImGui.SetCursorPosY(85);
                ImGui.SetCursorPosX(20);
                ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new System.Numerics.Vector4(0.3f, 0.6f, 1.0f, 1.0f));
                ImGui.ProgressBar((float)_loadingProgress / _loadingTotal, new System.Numerics.Vector2(360, 20), "");
                ImGui.PopStyleColor();
            }
        }
        ImGui.End();
    }

    private void RenderMenuBar()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open Model...", "Ctrl+O"))
                {
                    _fileDialogManager?.ShowFileDialog(ModelFileDialog.GetModelDirectory());
                }
                if (ImGui.MenuItem("Open Models in Folder..."))
                {
                    OpenFolderDialog();
                }

                // --- Novas entradas para texturas ---
                if (ImGui.MenuItem("Load Texture (CMP)..."))
                {
                    if (_fileDialogManager != null)
                    {
                        _fileDialogManager.ShowFileDialog(ModelFileDialog.GetModelDirectory(), "*.cmp", "Open CMP Texture File");
                        _fileDialogManager.OnFilesSelected = files =>
                        {
                            if (files != null && files.Length > 0)
                            {
                                // Adiciona CMP ao painel de assets (Asset Browser)
                                _modelBrowser.LoadSingleFile(files[0]);
                            }
                        };
                    }
                }
                if (ImGui.MenuItem("Load Textures (Folder)..."))
                {
                    if (_fileDialogManager != null)
                    {
                        _fileDialogManager.ShowFolderDialog(ModelFileDialog.GetModelDirectory());
                        _fileDialogManager.OnFolderSelected = folderPath =>
                        {
                            if (!string.IsNullOrEmpty(folderPath))
                            {
                                // Load all CMP files from folder into asset browser
                                _modelBrowser.LoadCmpFilesFromFolder(folderPath);
                                _logger.LogInformation($"[TEXTURE] Loaded CMP files from folder {folderPath} into asset browser");
                            }
                        };
                    }
                }

                if (ImGui.MenuItem("Load Texture (TIM)..."))
                {
                    if (_fileDialogManager != null)
                    {
                        _fileDialogManager.ShowFileDialog(ModelFileDialog.GetModelDirectory(), "*.tim", "Open TIM Texture File");
                        _fileDialogManager.OnFilesSelected = files =>
                        {
                            if (files != null && files.Length > 0)
                            {
                                // Adiciona TIM ao painel de assets (Asset Browser)
                                _modelBrowser.LoadSingleFile(files[0]);
                            }
                        };
                    }
                }

                if (ImGui.MenuItem("Load Textures TIM (Folder)..."))
                {
                    if (_fileDialogManager != null)
                    {
                        _fileDialogManager.ShowFolderDialog(ModelFileDialog.GetModelDirectory());
                        _fileDialogManager.OnFolderSelected = folderPath =>
                        {
                            if (!string.IsNullOrEmpty(folderPath))
                            {
                                // Load all TIM files from folder into asset browser
                                _modelBrowser.LoadTimFilesFromFolder(folderPath);
                                _logger.LogInformation($"[TEXTURE] Loaded TIM files from folder {folderPath} into asset browser");
                            }
                        };
                    }
                }
                // --- Fim das novas entradas ---

                // Open Recent submenu
                if (ImGui.BeginMenu("Open Recent", _recentFiles.RecentItems.Count > 0))
                {
                    // Create a copy to avoid collection modified exception
                    var recentItemsCopy = _recentFiles.RecentItems.ToList();

                    foreach (var item in recentItemsCopy)
                    {
                        string icon = item.IsFolder ? "📁" : "📄";
                        string label = $"{icon} {item.DisplayName}";

                        if (ImGui.MenuItem(label))
                        {
                            if (item.IsFolder)
                            {
                                LoadPrmFolder(item.Path);
                            }
                            else
                            {
                                // Load the specific file directly
                                if (File.Exists(item.Path))
                                {
                                    _logger.LogInformation("[RECENT] Loading file into browser: {Path}", item.Path);

                                    // Load only this specific file into the browser (don't auto-load into scene)
                                    _modelBrowser.LoadSingleFile(item.Path);
                                }
                                else
                                {
                                    _logger.LogWarning("[RECENT] File not found: {Path}", item.Path);
                                }
                            }
                        }

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip(item.Path);
                        }
                    }

                    ImGui.Separator();
                    if (ImGui.MenuItem("Clear Recent"))
                    {
                        _recentFiles.ClearRecentFiles();
                    }

                    ImGui.EndMenu();
                }

                ImGui.Separator();
                if (ImGui.MenuItem("Save Scene", "Ctrl+S")) { }
                ImGui.Separator();
                if (ImGui.MenuItem("Exit", "Alt+F4")) { Close(); }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                if (ImGui.MenuItem("Undo", "Ctrl+Z")) { }
                if (ImGui.MenuItem("Redo", "Ctrl+Y")) { }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Reset Camera", "R")) { ResetCamera(); }
                if (ImGui.MenuItem("Frame Selected", "F")) { }
                ImGui.Separator();
                ImGui.MenuItem("Viewport", null, ref _showViewport);
                if (_scenePanel != null)
                {
                    bool show = _scenePanel.IsVisible;
                    if (ImGui.MenuItem("Scene", null, ref show))
                        _scenePanel.IsVisible = show;
                }
                if (_assetBrowserPanel != null)
                {
                    bool show = _assetBrowserPanel.IsVisible;
                    if (ImGui.MenuItem("Asset Browser", null, ref show))
                        _assetBrowserPanel.IsVisible = show;
                }
                if (_trackViewerPanel != null)
                {
                    bool show = _trackViewerPanel.IsVisible;
                    if (ImGui.MenuItem("Track Viewer", null, ref show))
                        _trackViewerPanel.IsVisible = show;
                }
                if (_trackDataPanel != null)
                {
                    bool show = _trackDataPanel.IsVisible;
                    if (ImGui.MenuItem("Track Data Inspector", null, ref show))
                    {
                        _trackDataPanel.IsVisible = show;
                        _settingsManager.Settings.ShowTrackDataInspector = show;
                        _settingsManager.SaveSettings();
                    }
                }
                if (_propertiesPanel != null)
                {
                    bool show = _propertiesPanel.IsVisible;
                    if (ImGui.MenuItem("Properties", null, ref show))
                        _propertiesPanel.IsVisible = show;
                }
                if (_texturePanel != null)
                {
                    bool show = _texturePanel.IsVisible;
                    if (ImGui.MenuItem("Textures", null, ref show))
                        _texturePanel.IsVisible = show;
                }
                if (_transformPanel != null)
                {
                    bool show = _transformPanel.IsVisible;
                    if (ImGui.MenuItem("Transform", null, ref show))
                        _transformPanel.IsVisible = show;
                }
                if (_cameraPanel != null)
                {
                    bool show = _cameraPanel.IsVisible;
                    if (ImGui.MenuItem("Camera", null, ref show))
                        _cameraPanel.IsVisible = show;
                }
                if (_settingsPanel != null)
                {
                    bool show = _settingsPanel.IsVisible;
                    if (ImGui.MenuItem("Settings", null, ref show))
                        _settingsPanel.IsVisible = show;
                }
                if (_viewportInfoPanel != null)
                {
                    bool show = _viewportInfoPanel.IsVisible;
                    if (ImGui.MenuItem("Viewport Info", null, ref show))
                        _viewportInfoPanel.IsVisible = show;
                }
                if (_lightPanel != null)
                {
                    bool show = _lightPanel.IsVisible;
                    if (ImGui.MenuItem("Light Properties", null, ref show))
                        _lightPanel.IsVisible = show;
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Tools"))
            {
                if (ImGui.MenuItem("Refresh Assets")) { _modelBrowser.RefreshModelList(); }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Window"))
            {
                if (ImGui.MenuItem("Reset Layout")) { }
                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }

    /// <summary>
    /// Render rename dialog for scene entitier
    /// <summary>
    /// Render rename dialog for scene entities
    /// </summary>
    private void RenderRenameDialog()
    {
        if (!_showRenameDialog || _renameTarget == null)
            return;

        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize * 0.5f, ImGuiCond.Appearing, new System.Numerics.Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 120), ImGuiCond.Appearing);

        if (ImGui.Begin("Rename", ref _showRenameDialog, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text("New name:");
            ImGui.SetNextItemWidth(-1);

            bool enterPressed = ImGui.InputText("##renameinput", ref _renameBuffer, 256, ImGuiInputTextFlags.EnterReturnsTrue);

            ImGui.Spacing();

            if (ImGui.Button("OK", new System.Numerics.Vector2(120, 0)) || enterPressed)
            {
                if (!string.IsNullOrWhiteSpace(_renameBuffer))
                {
                    // Apply rename based on target type
                    if (_renameTarget is SceneCamera camera)
                    {
                        camera.Name = _renameBuffer;
                        _logger.LogInformation($"[SCENE] Renamed camera to: {_renameBuffer}");
                    }
                    else if (_renameTarget is SceneObject obj)
                    {
                        obj.Name = _renameBuffer;
                        _logger.LogInformation($"[SCENE] Renamed object to: {_renameBuffer}");
                    }
                    else if (_renameTarget is DirectionalLight light)
                    {
                        light.Name = _renameBuffer;
                        _logger.LogInformation($"[SCENE] Renamed light to: {_renameBuffer}");
                    }
                }
                _showRenameDialog = false;
                _renameTarget = null;
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel", new System.Numerics.Vector2(120, 0)))
            {
                _showRenameDialog = false;
                _renameTarget = null;
            }
        }

        if (!_showRenameDialog)
        {
            _renameTarget = null;
        }

        ImGui.End();
    }

    /// <summary>
    /// Render the camera spline as a red line for debug visualization.
    /// </summary>
    private void RenderSplineDebug(ICamera activeCamera)
    {
        // Only render if track is loaded AND spline is enabled in properties
        if (_scene?.ActiveTrack == null || !_propertiesPanel.ShowSpline)
            return;

        // Create navigation calculator if needed (whenever track is available)
        if (_splineNavigationCalculator == null)
        {
            var navLogger = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole();
            }).CreateLogger<TrackNavigationCalculator>();

            _splineNavigationCalculator = new TrackNavigationCalculator(_scene.ActiveTrack, navLogger);
            _logger.LogInformation("[SPLINE] Created navigation calculator with {SectionCount} sections",
                _splineNavigationCalculator.GetSectionCount());
        }

        // Create debug renderer if needed
        if (_splineDebugRenderer == null)
        {
            _splineDebugRenderer = new SplineDebugRenderer();

            // Build from waypoints
            var waypoints = _splineNavigationCalculator.GetWaypoints();
            _splineDebugRenderer.BuildFromWaypoints(waypoints);

            _logger.LogInformation("[SPLINE] Built debug renderer with {WaypointCount} waypoints", waypoints.Count);

            // Debug: Log first and last waypoint coordinates
            if (waypoints.Count > 0)
            {
                var first = waypoints[0];
                var last = waypoints[waypoints.Count - 1];
                _logger.LogInformation("[SPLINE] First waypoint: Position={Position}", first.Position);
                _logger.LogInformation("[SPLINE] Last waypoint: Position={Position}", last.Position);

                // Log camera current position for comparison
                _logger.LogInformation("[SPLINE] Camera position: {Position}", activeCamera.Position);
            }
        }

        // Render the spline
        if (_splineDebugRenderer != null)
        {
            _splineDebugRenderer.Render(
                activeCamera.GetProjectionMatrix(),
                activeCamera.GetViewMatrix()
            );
        }
    }

    /// <summary>
    /// Render ImGui UI (without 3D rendering which is done separately in RenderViewportToFBO).
    /// </summary>
    private void RenderUIWithoutViewport()
    {
        // Main menu bar
        RenderMenuBar();

        // Setup DockSpace to fill the entire window (excluding menu bar)
        SetupDockSpace();

        // Render panels (viewport display only, 3D already rendered to FBO)
        RenderViewportDisplayPanel();
        _scenePanel.Render();
        _assetBrowserPanel.Render();
        _trackViewerPanel.Render();
        _trackDataPanel.Render();
        _viewportInfoPanel.Render();
        _propertiesPanel.Render();
        _texturePanel.Render();
        _transformPanel.Render();
        _cameraPanel.Render();
        _lightPanel.Render();
        _settingsPanel.Render();

        // Render dialogs
        _fileDialogManager.Render();
        RenderRenameDialog();

        // Render loading indicator
        RenderLoadingIndicator();
    }

    /// <summary>
    /// Render the 3D viewport panel showing the selected model.
    /// </summary>
    private void RenderViewportDisplayPanel()
    {
        if (!_showViewport) return;

        // Don't set position/size, let docking system handle it
        // Only set on first use to suggest a good starting size
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 600), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Viewport", ref _showViewport))
        {
            _viewportHovered = ImGui.IsWindowHovered();
            _viewportFocused = ImGui.IsWindowFocused();

            // Camera selection toolbar
            ImGui.Text("Camera:");
            ImGui.SameLine();

            var cameras = _scene.CameraManager.Cameras.ToList();
            var activeCamera = _scene.CameraManager.ActiveCamera;
            int currentIndex = cameras.IndexOf(activeCamera);

            ImGui.SetNextItemWidth(150);
            if (ImGui.BeginCombo("##viewportCamera", activeCamera.Name))
            {
                for (int i = 0; i < cameras.Count; i++)
                {
                    bool isSelected = (i == currentIndex);
                    if (ImGui.Selectable(cameras[i].Name, isSelected))
                    {
                        _scene.CameraManager.SetActiveCamera(cameras[i].Name);
                        _camera = cameras[i].Camera; // Update reference for viewport
                    }
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.Separator();

            var contentRegion = ImGui.GetContentRegionAvail();
            int newWidth = (int)contentRegion.X;
            int newHeight = (int)contentRegion.Y;

            // Resize framebuffer if viewport size changed
            if (newWidth > 0 && newHeight > 0 && (newWidth != _viewportWidth || newHeight != _viewportHeight))
            {
                _viewportWidth = newWidth;
                _viewportHeight = newHeight;

                // Resize texture
                GL.BindTexture(TextureTarget.Texture2D, _viewportTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb,
                    _viewportWidth, _viewportHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

                // Resize renderbuffer
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _viewportRBO);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8,
                    _viewportWidth, _viewportHeight);

                // Update camera aspect ratio
                _camera.SetAspectRatio((float)_viewportWidth / _viewportHeight);
            }

            // Display framebuffer texture in ImGui (no 3D rendering here, done separately)
            if (_viewportTexture != 0)
            {
                var imagePos = ImGui.GetCursorScreenPos();

                ImGui.Image((IntPtr)_viewportTexture,
                    new System.Numerics.Vector2(_viewportWidth, _viewportHeight),
                    new System.Numerics.Vector2(0, 1),  // UV coords flipped vertically
                    new System.Numerics.Vector2(1, 0));

                var io = ImGui.GetIO();

                // Get active camera interface once for all viewport operations
                var activeCam = _scene.CameraManager.ActiveCamera?.Camera;

                // Handle mouse controls for viewport camera
                if (_viewportHovered && activeCam != null)
                {
                    var mousePos = new Vector2(io.MousePos.X, io.MousePos.Y);
                    var mouseDelta = mousePos - _lastMousePosition;

                    // Left mouse button: Orbit camera
                    if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && !io.KeyAlt)
                    {
                        if (!_isDraggingOrbit)
                        {
                            _isDraggingOrbit = true;
                        }
                        else if (mouseDelta.LengthSquared > 0.01f)
                        {
                            // Orbit sensitivity
                            float sensitivity = 0.003f;

                            // Update yaw (horizontal rotation) - full 360° rotation
                            activeCam.Yaw += mouseDelta.X * sensitivity;

                            // Update pitch (vertical rotation) - limited to -89° to 89° to avoid gimbal lock
                            activeCam.Pitch -= mouseDelta.Y * sensitivity;
                        }
                    }
                    else
                    {
                        _isDraggingOrbit = false;
                    }

                    // Middle mouse button OR Alt + Left mouse button: Pan camera
                    if ((ImGui.IsMouseDown(ImGuiMouseButton.Middle) || (ImGui.IsMouseDown(ImGuiMouseButton.Left) && io.KeyAlt)))
                    {
                        if (!_isDraggingPan)
                        {
                            _isDraggingPan = true;
                        }
                        else if (mouseDelta.LengthSquared > 0.01f)
                        {
                            // Pan sensitivity based on distance
                            float sensitivity = 0.001f * activeCam.Distance;

                            // Calculate right and up vectors from camera
                            var viewMatrix = activeCam.GetViewMatrix();
                            var right = new Vector3(viewMatrix.M11, viewMatrix.M21, viewMatrix.M31);
                            var up = new Vector3(viewMatrix.M12, viewMatrix.M22, viewMatrix.M32);

                            // Move target based on mouse delta
                            activeCam.Target -= right * mouseDelta.X * sensitivity;
                            activeCam.Target += up * mouseDelta.Y * sensitivity;
                        }
                    }
                    else
                    {
                        _isDraggingPan = false;
                    }

                    // Mouse wheel: Zoom (supports both vertical scroll and trackpad pinch)
                    float scrollDelta = io.MouseWheel;

                    // Also check horizontal scroll wheel (some trackpads use this)
                    if (io.MouseWheelH != 0)
                    {
                        scrollDelta = io.MouseWheelH;
                    }

                    // Check for mouse scroll delta from OpenTK (alternative method for trackpad)
                    if (scrollDelta == 0 && MouseState.ScrollDelta.Y != 0)
                    {
                        scrollDelta = MouseState.ScrollDelta.Y;
                    }

                    if (scrollDelta != 0)
                    {
                        // Zoom sensitivity based on current distance
                        // Higher sensitivity for trackpad zooming
                        float zoomSpeed = activeCam.Distance * 0.2f;

                        // Invert if needed for more intuitive trackpad control
                        activeCam.Distance -= scrollDelta * zoomSpeed;

                        // Clamp distance to reasonable values
                        activeCam.Distance = MathHelper.Clamp(activeCam.Distance, 0.1f, 5000.0f);
                    }

                    // Keyboard controls for camera movement
                    float moveSpeed = 1.0f;  // Much faster base speed
                    if (io.KeyShift)
                        moveSpeed = 4.0f;  // Much faster with Shift (4x base speed)

                    var moveDirection = Vector3.Zero;

                    // WASD keys for movement
                    if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S))
                        moveDirection += Vector3.UnitZ * moveSpeed;
                    if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W))
                        moveDirection -= Vector3.UnitZ * moveSpeed;
                    if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D))
                        moveDirection -= Vector3.UnitX * moveSpeed;
                    if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A))
                        moveDirection += Vector3.UnitX * moveSpeed;
                    if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.E))
                        moveDirection += Vector3.UnitY * moveSpeed;
                    if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Q))
                        moveDirection -= Vector3.UnitY * moveSpeed;

                    if (moveDirection.LengthSquared > 0.01f)
                    {
                        activeCam.Move(moveDirection);  // Direct movement, no delta time scaling
                    }

                    _lastMousePosition = mousePos;
                }

                // Handle view gizmo input if viewport is hovered
                // Note: Gizmo is rendered in RenderViewportToFBO() directly into the viewport framebuffer
                if (_viewGizmo != null && _settingsManager.Settings.ShowGizmo && _viewportHovered && activeCam != null)
                {
                    var gizmoSize = 100.0f * _currentUIScale;
                    // Gizmo position in viewport FBO coordinates (top-right corner)
                    var gizmoFBOPos = new Vector2(_viewportWidth - gizmoSize - 10, _viewportHeight - gizmoSize - 10);

                    // Convert mouse position from screen to viewport coordinates
                    var mouseInViewport = new Vector2(io.MousePos.X - imagePos.X, io.MousePos.Y - imagePos.Y);

                    // Note: Y is flipped because OpenGL has origin at bottom-left, ImGui at top-left
                    var mouseInFBO = new Vector2(mouseInViewport.X, _viewportHeight - mouseInViewport.Y);

                    // Use active camera from scene
                    _viewGizmo.HandleInput(activeCam, gizmoFBOPos, mouseInFBO, gizmoSize, 5.0f);
                }
            }
        }
        ImGui.End();
    }

    /// <summary>
    /// Render 3D scene to viewport framebuffer (isolated from ImGui).
    /// </summary>
    private void RenderViewportToFBO()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _viewportFBO);
        GL.Viewport(0, 0, _viewportWidth, _viewportHeight);

        // Clear viewport framebuffer
        GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Use active camera from scene
        var activeCamera = _scene.CameraManager.ActiveCamera?.Camera ?? _camera;

        // Setup 3D rendering - perspective mode
        _renderer.SetPassthroughProjection(false);
        _renderer.SetProjectionMatrix(activeCamera.GetProjectionMatrix());
        _renderer.SetViewMatrix(activeCamera.GetViewMatrix());

        // Setup 3D rendering state
        _renderer.SetDepthTest(true);
        _renderer.SetDepthWrite(true);
        _renderer.SetFaceCulling(true);

        // Apply directional lights from scene
        // Combine all enabled lights (for now just use the first enabled light)
        var enabledLight = _scene.LightManager.Lights.FirstOrDefault(l => l.IsEnabled);
        if (enabledLight != null)
        {
            _renderer.SetDirectionalLight(
                new Vector3(enabledLight.Direction.X, enabledLight.Direction.Y, enabledLight.Direction.Z),
                new Vector3(enabledLight.Color.X, enabledLight.Color.Y, enabledLight.Color.Z),
                enabledLight.Intensity
            );
        }
        else
        {
            // Default light if no lights enabled
            var defaultDir = new Vector3(-1, -1, -1).Normalized();
            _renderer.SetDirectionalLight(
                defaultDir,
                new Vector3(1, 1, 1),
                0.7f
            );
        }

        // Render world grid and axes (always visible)
        if (_worldGrid != null)
        {
            _worldGrid.Render(
                activeCamera.GetProjectionMatrix(),
                activeCamera.GetViewMatrix(),
                activeCamera.Position,
                0.1f,  // near plane
                1000.0f  // far plane
            );
        }

        // Render all objects from the scene
        foreach (var sceneObject in _scene.Objects)
        {
            if (!sceneObject.IsVisible || sceneObject.Ship == null)
                continue;

            var shipModel = sceneObject.Ship;

            // Special handling for sky: follow camera and don't use object's position/rotation
            if (sceneObject.IsSky)
            {
                // Sky follows camera with offset
                var skyPos = activeCamera.Position + new Vector3(sceneObject.SkyOffset.X, sceneObject.SkyOffset.Y, sceneObject.SkyOffset.Z);

                // Build transformation: Translation * Scale (no rotation - like in C)
                var positionSky = Matrix4.CreateTranslation(skyPos);
                var scaleSky = Matrix4.CreateScale(sceneObject.Scale);  // Scale for sky
                _renderer.SetModelMatrix(positionSky * scaleSky);

                // Disable depth write and face culling for sky
                _renderer.SetDepthWrite(false);
                GL.Disable(EnableCap.CullFace);

                if (_propertiesPanel?.WireframeMode == true)
                    GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

                shipModel.Draw();

                if (_propertiesPanel?.WireframeMode == true)
                    GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

                _renderer.SetDepthWrite(true);
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(TriangleFace.Back);
                continue;
            }

            // Regular object rendering
            // Set ship transform directly (GameObject will calculate matrix internally)
            shipModel.Position = sceneObject.Position;
            shipModel.Angle = sceneObject.Rotation;
            shipModel.IsVisible = sceneObject.IsVisible;

            // Apply only scale via SetModelMatrix
            // GameObject.Draw() will apply its own position+rotation transform
            var scaling = Matrix4.CreateScale(sceneObject.Scale);
            _renderer.SetModelMatrix(scaling);

            // Conditional face culling based on object type
            // Track geometry should have culling disabled to show both sides
            if (sceneObject.Name == "Track Geometry")
            {
                _renderer.SetFaceCulling(false);
            }
            else
            {
                // Ships and other objects should have culling enabled
                _renderer.SetFaceCulling(true);
                GL.CullFace(TriangleFace.Back);
            }

            // Wireframe toggle
            if (_propertiesPanel?.WireframeMode == true)
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

            // Render this specific ship directly
            shipModel.Draw();

            // Render shadow with blending enabled
            _renderer.SetBlending(true);
            _renderer.SetDepthWrite(false); // Don't write to depth buffer for shadow
            shipModel.RenderShadow();
            _renderer.SetDepthWrite(true); // Restore depth writing
            _renderer.SetBlending(false);

            // Restore polygon mode
            if (_propertiesPanel?.WireframeMode == true)
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        }

        // Flush batched rendering for scene objects
        // This ensures single objects are visible without requiring multiple objects
        _renderer.Flush();

        // Render camera spline debug visualization (if fly-through is enabled)
        RenderSplineDebug(activeCamera);

        // Restore rendering state
        _renderer.SetDepthTest(false);
        _renderer.SetDepthWrite(false);
        _renderer.SetFaceCulling(false);

        // Render view gizmo overlay (top-right corner of viewport)
        // This is a 2D UI widget that shows camera orientation
        if (_viewGizmo != null && _settingsManager.Settings.ShowGizmo)
        {
            var gizmoSize = 100.0f * _currentUIScale;
            // Position in framebuffer coordinates (top-right)
            var gizmoPos = new Vector2(_viewportWidth - gizmoSize - 10, _viewportHeight - gizmoSize - 10);

            // Use active camera from scene
            var gizmoCamera = _scene.CameraManager.ActiveCamera?.Camera;
            if (gizmoCamera != null)
            {
                _viewGizmo.Render(gizmoCamera, gizmoPos, 5.0f, gizmoSize);
            }
        }

        // Unbind framebuffer and restore main viewport
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Viewport(0, 0, Size.X, Size.Y);
    }

    /// <summary>
    /// Reset camera to initial state.
    /// </summary>
    private void ResetCamera()
    {
        // Reset camera view
        _camera.ResetView();

        // Move camera to better viewing position (further back and slightly above)
        _camera.Move(new Vector3(0, 20, 50));
        _camera.Rotate(0, -0.3f);  // Slightly looking down

        // Reset animation state
        _totalTime = 0f;

        if (_viewportInfoPanel != null)
            ((IViewportInfoPanel)_viewportInfoPanel).AutoRotate = false;

        _logger.LogInformation("[CAMERA] Reset to position: {Pos}, Yaw: {Yaw}, Pitch: {Pitch}",
            _camera.Position, _camera.Yaw, _camera.Pitch);
    }

    /// <summary>
    /// Setup a fullscreen docking space that fills the window (below menu bar).
    /// </summary>
    private void SetupDockSpace()
    {
        var viewport = ImGui.GetMainViewport();

        // Get menu bar height
        float menuBarHeight = ImGui.GetFrameHeight();

        // Position dockspace below menu bar
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(viewport.Pos.X, viewport.Pos.Y + menuBarHeight));
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(viewport.Size.X, viewport.Size.Y - menuBarHeight));
        ImGui.SetNextWindowViewport(viewport.ID);

        // Window flags for a background dockspace
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                      ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                                      ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(0.0f, 0.0f));

        ImGui.Begin("DockSpaceWindow", windowFlags);
        ImGui.PopStyleVar(3);

        // Create dockspace
        uint dockspaceID = ImGui.GetID("MainDockSpace");
        ImGui.DockSpace(dockspaceID, new System.Numerics.Vector2(0, 0), ImGuiDockNodeFlags.PassthruCentralNode);

        ImGui.End();
    }

    #endregion 
}