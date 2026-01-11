using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Tools.Core;

namespace WipeoutRewrite.Tools.Rendering;

/// <summary>
/// Service for rendering scene objects.
/// Follows Single Responsibility Principle - only handles scene rendering logic.
/// </summary>
public class SceneRenderer : ISceneRenderer
{
    private readonly ILogger _logger;
    private readonly IRenderer _renderer;
    private readonly ViewGizmo _viewGizmo;
    private readonly WorldGrid _worldGrid;

    public SceneRenderer(
        ILogger logger,
        IRenderer renderer,
        WorldGrid worldGrid,
        ViewGizmo viewGizmo)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _worldGrid = worldGrid ?? throw new ArgumentNullException(nameof(worldGrid));
        _viewGizmo = viewGizmo ?? throw new ArgumentNullException(nameof(viewGizmo));
    }

    public void RenderScene(ICamera camera, Scene scene, bool wireframeMode)
    {
        // Apply directional lights from scene
        var enabledLight = scene.LightManager.Lights.FirstOrDefault(l => l.IsEnabled);
        if (enabledLight != null)
        {
            _renderer.SetDirectionalLight(
                new Vector3(enabledLight.Direction.X, enabledLight.Direction.Y, enabledLight.Direction.Z),
                new Vector3(enabledLight.Color.X, enabledLight.Color.Y, enabledLight.Color.Z),
                enabledLight.Intensity
            );
        }

        // Render all visible objects from the scene
        foreach (var sceneObject in scene.Objects)
        {
            if (!sceneObject.IsVisible || sceneObject.Ship == null)
                continue;

            var shipModel = sceneObject.Ship;

            // Only set visibility (don't override Position/Angle here)
            shipModel.IsVisible = sceneObject.IsVisible;

            // Sky: follow camera and disable depth write so it always wraps the scene
            if (sceneObject.IsSky)
            {
                var skyPos = new Vector3(
                    camera.Position.X + sceneObject.SkyOffset.X,
                    camera.Position.Y + sceneObject.SkyOffset.Y,
                    camera.Position.Z + sceneObject.SkyOffset.Z
                );

                // Build complete transformation matrix for sky:
                // Translation * Scale (no rotation - like in C)
                var positionSky = Matrix4.CreateTranslation(skyPos);
                var scaleSky = Matrix4.CreateScale(sceneObject.Scale);  // Keep object's scale
                _renderer.SetModelMatrix(positionSky * scaleSky);

                _renderer.SetDepthWrite(false);
                // Disable face culling completely so we see sky from both inside and outside
                GL.Disable(EnableCap.CullFace);

                if (wireframeMode)
                    GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

                shipModel.Draw();

                if (wireframeMode)
                    GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

                _renderer.SetDepthWrite(true);
                // Restore normal backface culling for other objects
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(TriangleFace.Back);
                continue;
            }

            // Build complete transformation matrix from SceneObject:
            // Translation * Rotation * Scale
            var position = Matrix4.CreateTranslation(sceneObject.Position.X, sceneObject.Position.Y, sceneObject.Position.Z);

            // Apply rotation (convert from Vec3 euler angles to rotation matrix)
            var rotX = Matrix4.CreateRotationX(sceneObject.Rotation.X);
            var rotY = Matrix4.CreateRotationY(sceneObject.Rotation.Y);
            var rotZ = Matrix4.CreateRotationZ(sceneObject.Rotation.Z);
            var rotation = rotZ * rotY * rotX;  // ZYX order

            var scale = Matrix4.CreateScale(sceneObject.Scale);

            // Complete transformation: Position * Rotation * Scale
            var modelMatrix = position * rotation * scale;
            _renderer.SetModelMatrix(modelMatrix);

            // Reset face culling
            _renderer.SetFaceCulling(false);

            // Wireframe toggle
            if (wireframeMode)
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

            // Render ship
            shipModel.Draw();

            // Render shadow
            _renderer.SetBlending(true);
            _renderer.SetDepthWrite(false);
            shipModel.RenderShadow();
            _renderer.SetDepthWrite(true);
            _renderer.SetBlending(false);

            // Restore polygon mode
            if (wireframeMode)
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        }

        // Flush batched rendering
        _renderer.Flush();
    }

    public void RenderViewGizmo(ICamera camera, float x, float y, float size)
    {
        _viewGizmo.Render(camera, new Vector2(x, y), 5.0f, size);
    }

    public void RenderWorldGrid(ICamera camera)
    {
        if (_worldGrid != null)
        {
            _worldGrid.Render(
                camera.GetProjectionMatrix(),
                camera.GetViewMatrix(),
                camera.Position,
                0.1f,
                1000.0f
            );
        }
    }
}