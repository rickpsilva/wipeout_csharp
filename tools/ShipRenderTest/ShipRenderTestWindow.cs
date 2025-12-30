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
    private readonly ITextureManager _textureManager;
    private readonly ITexturePanel _texturePanel;
    private float _totalTime = 0f;
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
        FileDialogManager fileDialogManager)
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
        _fileDialogManager = fileDialogManager ?? throw new ArgumentNullException(nameof(fileDialogManager));
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
    }

    /// <summary>
    /// Add a model from Asset Browser to the Scene
    /// </summary>
    private void AddModelToScene(string modelPath, int objectIndex)
    {
        try
        {
            // Create a new ship instance for this object
            var shipLogger = new NullLogger<GameObject>();
            var modelLoaderLogger = new NullLogger<ModelLoader>();
            var modelLoader = new ModelLoader(modelLoaderLogger);
            var newShip = new GameObject(_renderer, shipLogger, _textureManager, modelLoader);
            newShip.LoadModelFromPath(modelPath, objectIndex);
            newShip.IsVisible = true;  // Enable rendering

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
            sceneObject.Rotation = new Vec3(0, 0, MathF.PI); // 180° rotation in Z to orient correctly
            sceneObject.Scale = 0.1f;
            _scene.SelectedObject = sceneObject;

            _logger.LogInformation($"[SCENE] Added {sceneObject.Name} to scene");

            // Add to recent files
            _recentFiles.AddRecentFile(modelPath, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SCENE] Failed to add model to scene: {Path}", modelPath);
        }
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
                            float sensitivity = 0.005f;

                            // Update yaw (horizontal rotation)
                            activeCam.Yaw += mouseDelta.X * sensitivity;

                            // Update pitch (vertical rotation)
                            activeCam.Pitch -= mouseDelta.Y * sensitivity;
                        }
                    }
                    else
                    {
                        _isDraggingOrbit = false;
                    }

                    // Alt + Left mouse button: Pan camera
                    if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && io.KeyAlt)
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

                    // Mouse wheel: Zoom
                    if (io.MouseWheel != 0)
                    {
                        // Zoom sensitivity based on current distance
                        float zoomSpeed = activeCam.Distance * 0.1f;
                        activeCam.Distance -= io.MouseWheel * zoomSpeed;

                        // Clamp distance to reasonable values
                        activeCam.Distance = MathHelper.Clamp(activeCam.Distance, 0.1f, 100.0f);
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

            // Set ship transform directly (GameObject will calculate matrix internally)
            shipModel.Position = sceneObject.Position;
            shipModel.Angle = sceneObject.Rotation;
            shipModel.IsVisible = sceneObject.IsVisible;

            // Apply only scale via SetModelMatrix
            // GameObject.Draw() will apply its own position+rotation transform
            var scaling = Matrix4.CreateScale(sceneObject.Scale);
            _renderer.SetModelMatrix(scaling);

            // Reset face culling for each object
            _renderer.SetFaceCulling(false);

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