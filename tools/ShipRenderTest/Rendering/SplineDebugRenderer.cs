using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using WipeoutRewrite.Tools.Core;

namespace WipeoutRewrite.Tools.Rendering;

/// <summary>
/// Simple debug renderer for camera spline visualization.
/// Draws the spline path as a thick red line for debugging fly-through navigation.
/// Uses a simple shader to render with red color.
/// </summary>
public class SplineDebugRenderer
{
    private const string FragmentShaderSource = @"
        #version 410
        out vec4 FragColor;
        
        uniform vec3 color;
        
        void main()
        {
            FragColor = vec4(color, 1.0);
        }
    ";

    private const string VertexShaderSource = @"
        #version 410
        layout (location = 0) in vec3 position;
        
        uniform mat4 projection;
        uniform mat4 view;
        
        void main()
        {
            gl_Position = projection * view * vec4(position, 1.0);
        }
    ";

    #region fields
    private int _colorLoc = 0;
    private bool _isInitialized = false;
    private int _lineCount = 0;
    private int _projLoc = 0;
    private int _shaderProgram = 0;
    private int _vao = 0;
    private int _vbo = 0;
    private int _viewLoc = 0;
    #endregion 

    public SplineDebugRenderer()
    {
    }

    /// <summary>
    /// Build the spline geometry from waypoints.
    /// IMPORTANT: Waypoints are in PSX coordinates. Track is rendered with 0.001f scale.
    /// We need to apply the same scale to spline vertices.
    /// Generates smooth interpolation between waypoints using Catmull-Rom spline.
    /// </summary>
    public void BuildFromWaypoints(IReadOnlyList<TrackNavigationCalculator.NavigationWaypoint> waypoints)
    {
        if (waypoints.Count < 2)
            return;

        // Initialize shader if not already done
        if (_shaderProgram == 0)
        {
            InitializeShader();
        }

        const float PSX_TO_WORLD_SCALE = 0.001f;  // Same scale as track rendering

        // Create line vertices by connecting waypoints with smooth interpolation
        var vertices = new List<Vector3>();

        // Draw line segments between consecutive waypoints with smooth interpolation
        for (int i = 0; i < waypoints.Count; i++)
        {
            // Add current waypoint
            var scaledPos = waypoints[i].Position * PSX_TO_WORLD_SCALE;
            vertices.Add(scaledPos);

            // Add intermediate points for smoother visualization (between this and next waypoint)
            if (i < waypoints.Count - 1)
            {
                var current = waypoints[i].Position * PSX_TO_WORLD_SCALE;
                var next = waypoints[i + 1].Position * PSX_TO_WORLD_SCALE;

                // Add more intermediate points (20 instead of 4) for smoother curves
                for (int j = 1; j <= 20; j++)
                {
                    float t = j / 21.0f;  // 0.047 to 0.952
                    // Use Catmull-Rom interpolation for smoother curves
                    Vector3 p0 = i > 0 ? waypoints[i - 1].Position * PSX_TO_WORLD_SCALE : current;
                    Vector3 p1 = current;
                    Vector3 p2 = next;
                    Vector3 p3 = i < waypoints.Count - 2 ? waypoints[i + 2].Position * PSX_TO_WORLD_SCALE : next;

                    var interpolated = CatmullRom(p0, p1, p2, p3, t);
                    vertices.Add(interpolated);
                }
            }
        }

        _lineCount = vertices.Count;

        // Debug: Log spline bounds
        if (vertices.Count > 0)
        {
            var first = vertices[0];
            var last = vertices[vertices.Count - 1];
            Console.WriteLine($"[SPLINE] Built spline with {vertices.Count} vertices (SCALED by 0.001)");
            Console.WriteLine($"[SPLINE] First vertex (SCALED): X={first.X:F2}, Y={first.Y:F2}, Z={first.Z:F2}");
            Console.WriteLine($"[SPLINE] Last vertex (SCALED): X={last.X:F2}, Y={last.Y:F2}, Z={last.Z:F2}");
        }

        // Setup VAO and VBO
        if (!_isInitialized)
        {
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _isInitialized = true;
        }

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

        // Upload vertex data
        var vertexArray = vertices.SelectMany(v => new[] { v.X, v.Y, v.Z }).ToArray();
        GL.BufferData(BufferTarget.ArrayBuffer, vertexArray.Length * sizeof(float), vertexArray, BufferUsageHint.DynamicDraw);

        // Setup vertex attributes (position only, using a generic attribute location)
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    /// <summary>
    /// Clean up GPU resources.
    /// </summary>
    public void Dispose()
    {
        if (_isInitialized)
        {
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            _isInitialized = false;
        }
    }

    /// <summary>
    /// Render the spline as a thick red line.
    /// Uses a custom shader to render with bright red color.
    /// Vertices are already scaled to world coordinates (PSX * 0.001).
    /// Disables depth test so spline is always visible on top.
    /// </summary>
    public void Render(Matrix4 projectionMatrix, Matrix4 viewMatrix)
    {
        if (_lineCount == 0 || !_isInitialized || _shaderProgram == 0)
            return;

        // Save current OpenGL state
        GL.GetFloat(GetPName.LineWidth, out float prevLineWidth);
        var prevDepthTest = GL.IsEnabled(EnableCap.DepthTest);
        var prevBlend = GL.IsEnabled(EnableCap.Blend);
        var prevCullFace = GL.IsEnabled(EnableCap.CullFace);
        int prevProgram;
        GL.GetInteger(GetPName.CurrentProgram, out prevProgram);

        // Setup for thick red line rendering
        GL.LineWidth(15.0f);  // VERY thick lines for visibility
        GL.Disable(EnableCap.DepthTest);  // Always visible on top
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);

        // Use our shader program
        GL.UseProgram(_shaderProgram);

        // Set uniforms
        GL.UniformMatrix4(_projLoc, false, ref projectionMatrix);
        GL.UniformMatrix4(_viewLoc, false, ref viewMatrix);

        // Set color to bright red
        GL.Uniform3(_colorLoc, 1.0f, 0.0f, 0.0f);  // Pure red (R, G, B)

        // Draw the line strip
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.LineStrip, 0, _lineCount);
        GL.BindVertexArray(0);

        // Restore OpenGL state
        GL.UseProgram(prevProgram);
        GL.LineWidth(prevLineWidth);
        if (prevDepthTest)
            GL.Enable(EnableCap.DepthTest);
        if (!prevBlend)
            GL.Disable(EnableCap.Blend);
        if (prevCullFace)
            GL.Enable(EnableCap.CullFace);
    }

    /// <summary>
    /// Catmull-Rom spline interpolation between 4 points.
    /// Provides smooth curves through control points.
    /// </summary>
    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        // Catmull-Rom coefficients
        float c0 = -0.5f * t3 + 1.0f * t2 - 0.5f * t;
        float c1 = 1.5f * t3 - 2.5f * t2 + 1.0f;
        float c2 = -1.5f * t3 + 2.0f * t2 + 0.5f * t;
        float c3 = 0.5f * t3 - 0.5f * t2;

        return c0 * p0 + c1 * p1 + c2 * p2 + c3 * p3;
    }

    /// <summary>
    /// Initialize the shader program for rendering the spline.
    /// </summary>
    private void InitializeShader()
    {
        // Compile vertex shader
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, VertexShaderSource);
        GL.CompileShader(vertexShader);

        // Check for vertex shader compile errors
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int vertexSuccess);
        if (vertexSuccess == 0)
        {
            GL.GetShaderInfoLog(vertexShader, out string vertexInfoLog);
            Console.WriteLine($"[SPLINE] Vertex shader compilation failed: {vertexInfoLog}");
        }

        // Compile fragment shader
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, FragmentShaderSource);
        GL.CompileShader(fragmentShader);

        // Check for fragment shader compile errors
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out int fragmentSuccess);
        if (fragmentSuccess == 0)
        {
            GL.GetShaderInfoLog(fragmentShader, out string fragmentInfoLog);
            Console.WriteLine($"[SPLINE] Fragment shader compilation failed: {fragmentInfoLog}");
        }

        // Link shaders
        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);
        GL.LinkProgram(_shaderProgram);

        // Check for linking errors
        GL.GetProgram(_shaderProgram, GetProgramParameterName.LinkStatus, out int linkSuccess);
        if (linkSuccess == 0)
        {
            GL.GetProgramInfoLog(_shaderProgram, out string linkInfoLog);
            Console.WriteLine($"[SPLINE] Shader program linking failed: {linkInfoLog}");
        }

        // Delete the shaders as they're linked now
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        // Get uniform locations
        _projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
        _viewLoc = GL.GetUniformLocation(_shaderProgram, "view");
        _colorLoc = GL.GetUniformLocation(_shaderProgram, "color");

        Console.WriteLine($"[SPLINE] Shader initialized successfully");
    }
}