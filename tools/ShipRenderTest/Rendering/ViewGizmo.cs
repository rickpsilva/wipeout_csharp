using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace WipeoutRewrite.Tools.Rendering;

// Extension method for Quaternion to Euler conversion
public static class QuaternionExtensions
{
    public static Vector3 ToEulerAngles(this Quaternion q)
    {
        Vector3 angles;

        // Pitch (X-axis rotation)
        float sinp = 2 * (q.W * q.X - q.Z * q.Y);
        if (Math.Abs(sinp) >= 1)
            angles.X = MathF.CopySign(MathF.PI / 2, sinp);
        else
            angles.X = MathF.Asin(sinp);

        // Yaw (Y-axis rotation)
        float siny_cosp = 2 * (q.W * q.Y + q.X * q.Z);
        float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.X * q.X);
        angles.Y = MathF.Atan2(siny_cosp, cosy_cosp);

        // Roll (Z-axis rotation)
        float sinr_cosp = 2 * (q.W * q.Z + q.Y * q.X);
        float cosr_cosp = 1 - 2 * (q.Z * q.Z + q.X * q.X);
        angles.Z = MathF.Atan2(sinr_cosp, cosr_cosp);

        return angles;
    }
}

/// <summary>
/// 3D orientation gizmo widget inspired by erhe's ImViewGuizmo.
/// Displays axis indicators (X=Red, Y=Green, Z=Blue) with mouse interaction
/// for quick camera orientation snapping.
/// </summary>
public class ViewGizmo : IViewGizmo
{
    #region constants
    private const double AnimationDuration = 0.2;
    private const float AxisLineWidth = 3.0f;
    private const float CircleRadius = 12.0f;

    // 200ms

    // Visual configuration
    private const float GizmoSize = 80.0f;

    private const float HighlightLineWidth = 5.0f;
    private const float LineLength = 0.5f;
    #endregion 

    #region fields
    private int _activeAxisId = -1;
    private double _animationStartTime = 0;
    private int _colorLocation;

    // Interaction state
    private int _hoveredAxisId = -1;

    private bool _initialized = false;

    // Animation state
    private bool _isAnimating = false;

    private bool _isDragging = false;
    private Vector2 _lastMousePos;
    private int _lineVao;
    private int _lineVbo;

    // Uniform locations
    private int _mvpLocation;

    // Rendering resources
    private int _shaderProgram;

    private float _snapDistance;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private int _vao;
    private int _vbo;

    private static readonly Vector4[] AxisColors =
    {
            new Vector4(0.9f, 0.2f, 0.2f, 1.0f),  // +X Red
            new Vector4(0.6f, 0.1f, 0.1f, 1.0f),  // -X Dark Red
            new Vector4(0.2f, 0.9f, 0.2f, 1.0f),  // +Y Green
            new Vector4(0.1f, 0.6f, 0.1f, 1.0f),  // -Y Dark Green
            new Vector4(0.2f, 0.5f, 1.0f, 1.0f),  // +Z Blue
            new Vector4(0.1f, 0.3f, 0.6f, 1.0f)   // -Z Dark Blue
        };

    // Axis definitions (aligned with Blender/erhe conventions)
    private static readonly Vector3[] AxisDirections =
    {
            new Vector3(1, 0, 0),   // +X (Red)
            new Vector3(-1, 0, 0),  // -X
            new Vector3(0, 1, 0),   // +Y (Green)
            new Vector3(0, -1, 0),  // -Y
            new Vector3(0, 0, 1),   // +Z (Blue)
            new Vector3(0, 0, -1)   // -Z
        };

    private static readonly string[] AxisLabels = { "X", "X", "Y", "Y", "Z", "Z" };

    // Predefined orientations for snapping
    private static readonly Quaternion[] SnapOrientations;

    #endregion 

    public ViewGizmo()
    {
        // Lazy initialization - recursos OpenGL criados quando Render() for chamado
    }

    static ViewGizmo()
    {
        // Pre-compute quaternions for snapping to axis-aligned views
        SnapOrientations = new Quaternion[6];

        // +X view (looking along +X axis)
        SnapOrientations[0] = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(-90));

        // -X view
        SnapOrientations[1] = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(90));

        // +Y view (top-down)
        SnapOrientations[2] = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(-90));

        // -Y view (bottom-up)
        SnapOrientations[3] = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(90));

        // +Z view (front)
        SnapOrientations[4] = Quaternion.Identity;

        // -Z view (back)
        SnapOrientations[5] = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(180));
    }

    #region methods

    public void Dispose()
    {
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_lineVao);
        GL.DeleteBuffer(_lineVbo);
        GL.DeleteProgram(_shaderProgram);
    }

    public bool HandleInput(ICamera camera, Vector2 gizmoScreenPos, Vector2 mousePos,
                           float size = GizmoSize, float snapDistance = 5.0f)
    {
        if (_isAnimating) return true;

        bool modified = false;
        float halfSize = size / 2.0f;
        Vector2 gizmoCenter = gizmoScreenPos + new Vector2(halfSize, halfSize);

        // Check if mouse is over gizmo area
        Vector2 mouseOffset = mousePos - gizmoCenter;
        float distToCenter = mouseOffset.Length;

        if (distToCenter > halfSize)
        {
            _hoveredAxisId = -1;
            if (!_isDragging) _activeAxisId = -1;
            return false;
        }

        // Check hover over axes
        _hoveredAxisId = GetAxisUnderMouse(camera, gizmoScreenPos, mousePos, size);

        // Handle mouse click for snapping
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && _hoveredAxisId >= 0)
        {
            _activeAxisId = _hoveredAxisId;
            _isDragging = true;
            _lastMousePos = mousePos;

            // Start snap animation
            StartSnapAnimation(camera, _hoveredAxisId, snapDistance);
            modified = true;
        }

        // Handle dragging for free rotation
        if (_isDragging && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            if (_activeAxisId == -1) // Free rotate (center sphere)
            {
                Vector2 delta = mousePos - _lastMousePos;
                RotateCamera(camera, delta * 0.005f);
                modified = true;
            }
            _lastMousePos = mousePos;
        }

        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            _isDragging = false;
        }

        return modified;
    }

    public void Render(ICamera camera, Vector2 screenPosition, float snapDistance = 5.0f,
                      float size = GizmoSize)
    {
        EnsureInitialized();

        // Update animation if active
        if (_isAnimating)
        {
            UpdateAnimation(camera);
        }

        // Create orthographic projection for gizmo
        var gizmoView = Matrix4.LookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);
        var gizmoProj = Matrix4.CreateOrthographic(2.0f, 2.0f, -100.0f, 100.0f);

        // Transform from world camera rotation
        var cameraRotation = GetCameraRotation(camera);
        var invertedRotation = cameraRotation.Inverted();
        var gizmoRotation = Matrix4.CreateFromQuaternion(invertedRotation);
        gizmoView = gizmoRotation * gizmoView;

        GL.UseProgram(_shaderProgram);

        // Save current GL state
        int[] viewport = new int[4];
        GL.GetInteger(GetPName.Viewport, viewport);
        int x = viewport[0], y = viewport[1], width = viewport[2], height = viewport[3];
        bool depthTestEnabled = GL.IsEnabled(EnableCap.DepthTest);
        bool blendEnabled = GL.IsEnabled(EnableCap.Blend);

        // Setup gizmo viewport 
        // OpenGL viewport uses bottom-left origin
        // screenPosition is in framebuffer coordinates (also bottom-left origin)
        int gizmoSize = (int)size;
        GL.Viewport((int)screenPosition.X, (int)screenPosition.Y, gizmoSize, gizmoSize);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Don't clear color buffer (we want to render over existing viewport content)
        // Only clear depth buffer for gizmo rendering
        GL.Clear(ClearBufferMask.DepthBufferBit);

        // Render axes with depth sorting (back-to-front)
        var sortedAxes = SortAxesByDepth(gizmoView);

        foreach (var (axisId, depth) in sortedAxes)
        {
            RenderAxis(axisId, gizmoView, gizmoProj, size);
        }

        // Restore GL state
        GL.Viewport(x, y, width, height);
        if (!depthTestEnabled) GL.Disable(EnableCap.DepthTest);
        if (blendEnabled) GL.Enable(EnableCap.Blend);
    }

    private void CheckProgramLinking(int program)
    {
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string log = GL.GetProgramInfoLog(program);
            Console.WriteLine($"ViewGizmo Shader Program Linking Error: {log}");
        }
    }

    private void CheckShaderCompilation(int shader, string type)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string log = GL.GetShaderInfoLog(shader);
            Console.WriteLine($"ViewGizmo {type} Shader Compilation Error: {log}");
        }
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;

        InitializeShaders();
        InitializeGeometry();
        _initialized = true;
    }

    private float[] GenerateSphere(float radius, int segments, int rings)
    {
        var vertices = new System.Collections.Generic.List<float>();

        for (int ring = 0; ring <= rings; ring++)
        {
            float phi = MathF.PI * ring / rings;
            for (int seg = 0; seg <= segments; seg++)
            {
                float theta = 2.0f * MathF.PI * seg / segments;

                float x = radius * MathF.Sin(phi) * MathF.Cos(theta);
                float y = radius * MathF.Cos(phi);
                float z = radius * MathF.Sin(phi) * MathF.Sin(theta);

                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(z);
            }
        }

        return vertices.ToArray();
    }

    private int GetAxisUnderMouse(ICamera camera, Vector2 gizmoPos, Vector2 mousePos, float size)
    {
        // Simple hit testing - check if mouse is near axis endpoint in screen space
        var gizmoView = Matrix4.LookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);
        var cameraRotation = GetCameraRotation(camera);
        var invertedRotation = cameraRotation.Inverted();
        var gizmoRotation = Matrix4.CreateFromQuaternion(invertedRotation);
        gizmoView = gizmoRotation * gizmoView;

        var gizmoProj = Matrix4.CreateOrthographic(2.0f, 2.0f, -100.0f, 100.0f);

        Vector2 gizmoCenter = gizmoPos + new Vector2(size / 2, size / 2);

        for (int i = 0; i < 6; i++)
        {
            Vector3 endPoint = AxisDirections[i] * LineLength;
            Vector4 clipPos = new Vector4(endPoint, 1.0f);
            var mvp = gizmoView * gizmoProj;
            clipPos = mvp * clipPos;

            if (Math.Abs(clipPos.W) < 0.001f) continue;

            Vector3 ndc = new Vector3(clipPos.X, clipPos.Y, clipPos.Z) / clipPos.W;
            Vector2 screenPos = gizmoCenter + new Vector2(ndc.X, -ndc.Y) * (size / 2);

            float dist = (mousePos - screenPos).Length;
            if (dist < CircleRadius * 1.5f)
            {
                return i;
            }
        }

        return -1;
    }

    private Quaternion GetCameraRotation(ICamera camera)
    {
        // Extract rotation from camera orientation
        // Assumes camera uses Yaw/Pitch or looks at origin
        float yaw = MathHelper.DegreesToRadians(camera.Yaw);
        float pitch = MathHelper.DegreesToRadians(camera.Pitch);

        var qYaw = Quaternion.FromAxisAngle(Vector3.UnitY, yaw);
        var qPitch = Quaternion.FromAxisAngle(Vector3.UnitX, pitch);

        return qYaw * qPitch;
    }

    private float GetRotationForDirection(Vector3 dir)
    {
        // Simple rotation calculation for line orientation
        if (Math.Abs(dir.X) > 0.9f) return dir.X > 0 ? 0 : MathF.PI;
        if (Math.Abs(dir.Y) > 0.9f) return dir.Y > 0 ? MathF.PI / 2 : -MathF.PI / 2;
        return 0;
    }

    private void InitializeGeometry()
    {
        // Create sphere for axis endpoint circles
        var sphereVertices = GenerateSphere(CircleRadius / 100.0f, 12, 12);

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, sphereVertices.Length * sizeof(float),
                     sphereVertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // Create line geometry
        _lineVao = GL.GenVertexArray();
        GL.BindVertexArray(_lineVao);

        _lineVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVbo);

        float[] lineVertices = { 0, 0, 0, 1, 0, 0 }; // Simple line from origin
        GL.BufferData(BufferTarget.ArrayBuffer, lineVertices.Length * sizeof(float),
                     lineVertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0);
    }

    private void InitializeShaders()
    {
        string vertexShaderSource = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                
                uniform mat4 uMVP;
                
                void main()
                {
                    gl_Position = uMVP * vec4(aPosition, 1.0);
                }
            ";

        string fragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;
                
                uniform vec4 uColor;
                
                void main()
                {
                    FragColor = uColor;
                }
            ";

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);
        CheckShaderCompilation(vertexShader, "Vertex");

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);
        CheckShaderCompilation(fragmentShader, "Fragment");

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);
        GL.LinkProgram(_shaderProgram);
        CheckProgramLinking(_shaderProgram);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        _mvpLocation = GL.GetUniformLocation(_shaderProgram, "uMVP");
        _colorLocation = GL.GetUniformLocation(_shaderProgram, "uColor");
    }

    private void RenderAxis(int axisId, Matrix4 view, Matrix4 proj, float size)
    {
        Vector3 direction = AxisDirections[axisId];
        Vector4 color = AxisColors[axisId];

        bool isHovered = (_hoveredAxisId == axisId);
        bool isActive = (_activeAxisId == axisId);

        // Brighten color if hovered or active
        if (isHovered || isActive)
        {
            color *= 1.3f;
            color.W = 1.0f;
        }

        // Render line
        float rotation = GetRotationForDirection(direction);
        var lineTransform = Matrix4.CreateScale(LineLength) *
                           Matrix4.CreateFromAxisAngle(Vector3.UnitY, rotation);
        var mvp = lineTransform * view * proj;

        GL.UseProgram(_shaderProgram);
        GL.UniformMatrix4(_mvpLocation, false, ref mvp);
        GL.Uniform4(_colorLocation, color);

        GL.LineWidth(isHovered || isActive ? HighlightLineWidth : AxisLineWidth);
        GL.BindVertexArray(_lineVao);
        GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Lines, 0, 2);

        // Render sphere at endpoint
        var spherePos = direction * LineLength;
        var sphereTransform = Matrix4.CreateTranslation(spherePos);
        mvp = sphereTransform * view * proj;

        GL.UniformMatrix4(_mvpLocation, false, ref mvp);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.TriangleFan, 0, 156); // Sphere vertices
    }

    private void RotateCamera(ICamera camera, Vector2 delta)
    {
        // Free rotation around camera
        var rotation = GetCameraRotation(camera);

        var yaw = Quaternion.FromAxisAngle(Vector3.UnitY, -delta.X);
        var pitch = Quaternion.FromAxisAngle(Vector3.UnitX, -delta.Y);

        rotation = yaw * rotation * pitch;
        SetCameraRotation(camera, rotation);

        // Position updates automatically via camera's Distance property
    }

    private void SetCameraRotation(ICamera camera, Quaternion rotation)
    {
        // Convert quaternion back to Yaw/Pitch
        var euler = rotation.ToEulerAngles();
        camera.Yaw = MathHelper.RadiansToDegrees(euler.Y);
        camera.Pitch = MathHelper.RadiansToDegrees(euler.X);
    }

    private (int axisId, float depth)[] SortAxesByDepth(Matrix4 gizmoView)
    {
        var axes = new (int, float)[6];

        for (int i = 0; i < 6; i++)
        {
            var dir = new Vector4(AxisDirections[i], 0);
            var viewDir = gizmoView * dir;
            axes[i] = (i, viewDir.Z);
        }

        Array.Sort(axes, (a, b) => a.Item2.CompareTo(b.Item2));
        return axes;
    }

    private void StartSnapAnimation(ICamera camera, int axisId, float distance)
    {
        _isAnimating = true;
        _animationStartTime = ImGui.GetTime();
        _snapDistance = distance;

        // Store current state
        _startRotation = GetCameraRotation(camera);
        _startPosition = camera.Position;

        // Calculate target orientation
        _targetRotation = SnapOrientations[axisId];

        // Calculate target position (maintain distance from origin)
        Vector3 forward = Vector3.Transform(-Vector3.UnitZ, _targetRotation);
        _targetPosition = -forward * distance;
    }

    private void UpdateAnimation(ICamera camera)
    {
        double elapsed = ImGui.GetTime() - _animationStartTime;
        float t = Math.Clamp((float)(elapsed / AnimationDuration), 0.0f, 1.0f);

        // Smooth interpolation (ease-out)
        t = 1.0f - (1.0f - t) * (1.0f - t);

        // Interpolate rotation (SLERP)
        var currentRotation = Quaternion.Slerp(_startRotation, _targetRotation, t);

        // Interpolate position
        var currentPosition = Vector3.Lerp(_startPosition, _targetPosition, t);

        // Apply to camera
        // Note: ICamera.Position is read-only, so we update via SetCameraRotation which adjusts camera orbit
        SetCameraRotation(camera, currentRotation);
        // Position will be updated based on rotation and distance

        if (t >= 1.0f)
        {
            _isAnimating = false;
        }
    }

    #endregion 
}