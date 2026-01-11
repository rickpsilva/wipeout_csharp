using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Tools.Managers;

namespace WipeoutRewrite.Tools.Rendering;

/// <summary>
/// Renders 3D viewport to a framebuffer.
/// Follows Single Responsibility Principle - only handles 3D rendering.
/// </summary>
public class ViewportRenderer : IViewportRenderer
{
    public int ViewportTextureId => _viewportTexture;

    #region fields
    private readonly ILogger _logger;
    private readonly IRenderer _renderer;
    private readonly ISettingsService _settings;
    private readonly ViewGizmo _viewGizmo;
    private int _viewportFBO = 0;
    private int _viewportHeight = 600;
    private int _viewportRBO = 0;
    private int _viewportTexture = 0;
    private int _viewportWidth = 800;
    private readonly WorldGrid _worldGrid;
    #endregion 

    public ViewportRenderer(
        ILogger logger,
        IRenderer renderer,
        ISettingsService settings,
        WorldGrid worldGrid,
        ViewGizmo viewGizmo)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _worldGrid = worldGrid ?? throw new ArgumentNullException(nameof(worldGrid));
        _viewGizmo = viewGizmo ?? throw new ArgumentNullException(nameof(viewGizmo));
    }

    public void InitializeFramebuffer(int width, int height)
    {
        _viewportWidth = width;
        _viewportHeight = height;

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

    public void RenderToFramebuffer(ICamera camera, int width, int height)
    {
        // Resize if needed
        if (width != _viewportWidth || height != _viewportHeight)
        {
            ResizeFramebuffer(width, height);
            camera.SetAspectRatio((float)width / height);
        }

        // Bind framebuffer
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _viewportFBO);
        GL.Viewport(0, 0, _viewportWidth, _viewportHeight);

        // Clear viewport framebuffer
        GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Setup 3D rendering - perspective mode
        _renderer.SetPassthroughProjection(false);
        _renderer.SetProjectionMatrix(camera.GetProjectionMatrix());
        _renderer.SetViewMatrix(camera.GetViewMatrix());

        // Setup 3D rendering state
        _renderer.SetDepthTest(true);
        _renderer.SetDepthWrite(true);
        _renderer.SetFaceCulling(true);

        // Set default directional light
        var defaultDir = new Vector3(-1, -1, -1).Normalized();
        _renderer.SetDirectionalLight(defaultDir, new Vector3(1, 1, 1), 0.7f);

        // Render world grid and axes
        if (_worldGrid != null)
        {
            _worldGrid.Render(
                camera.GetProjectionMatrix(),
                camera.GetViewMatrix(),
                camera.Position,
                0.1f,  // near plane
                1000.0f  // far plane
            );
        }

        // Scene objects will be rendered by SceneRenderer
        // This is just the base viewport setup

        // Restore rendering state
        _renderer.SetDepthTest(false);
        _renderer.SetDepthWrite(false);
        _renderer.SetFaceCulling(false);

        // Render view gizmo overlay (top-right corner)
        if (_viewGizmo != null && _settings.Settings.ShowGizmo)
        {
            var gizmoSize = 100.0f * _settings.Settings.UIScale;
            var gizmoPos = new Vector2(_viewportWidth - gizmoSize - 10, _viewportHeight - gizmoSize - 10);
            _viewGizmo.Render(camera, gizmoPos, 5.0f, gizmoSize);
        }

        // Unbind framebuffer and restore main viewport
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void ResizeFramebuffer(int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        _viewportWidth = width;
        _viewportHeight = height;

        // Resize texture
        GL.BindTexture(TextureTarget.Texture2D, _viewportTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb,
            _viewportWidth, _viewportHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

        // Resize renderbuffer
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _viewportRBO);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8,
            _viewportWidth, _viewportHeight);
    }
}