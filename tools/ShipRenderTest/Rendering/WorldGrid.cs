using OpenTK.Graphics.OpenGL4;
using GLPrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using OpenTK.Mathematics;

namespace WipeoutRewrite.Tools.Rendering;

/// <summary>
/// Renders an infinite world grid and coordinate axes for 3D viewport.
/// Uses procedural shader-based rendering for infinite grid effect.
/// </summary>
public class WorldGrid : IWorldGrid
{
    // Size of each grid cell
    public float GridFadeDistance { get; set; } = 100.0f;

    public float GridSize { get; set; } = 1.0f;
    public bool ShowAxes { get; set; } = true;
    public bool ShowGrid { get; set; } = true;

    #region fields
    private int _axesColorLocation;
    private int _axesMVPLocation;
    private int _axesShaderProgram;
    private int _axesVAO;
    private int _axesVBO;
    private int _gridCameraPosLocation;
    private int _gridEBO;
    private int _gridFarLocation;
    private int _gridMVPLocation;
    private int _gridNearLocation;
    private int _gridShaderProgram;
    private int _gridVAO;
    private int _gridVBO;
    private int _gridViewLocation;
    private bool _initialized = false;
    #endregion 

    // Distance at which grid fades out

    public WorldGrid()
    {
        // Lazy initialization - recursos OpenGL criados quando Render() for chamado
    }

    #region methods

    public void Dispose()
    {
        if (_gridVAO != 0)
        {
            GL.DeleteVertexArray(_gridVAO);
            GL.DeleteBuffer(_gridVBO);
            GL.DeleteBuffer(_gridEBO);
        }

        if (_axesVAO != 0)
        {
            GL.DeleteVertexArray(_axesVAO);
            GL.DeleteBuffer(_axesVBO);
        }

        if (_gridShaderProgram != 0)
        {
            GL.DeleteProgram(_gridShaderProgram);
        }

        if (_axesShaderProgram != 0)
        {
            GL.DeleteProgram(_axesShaderProgram);
        }
    }

    public void Render(Matrix4 projection, Matrix4 view, Vector3 cameraPosition, float near = 0.1f, float far = 1000.0f)
    {
        EnsureInitialized();

        // Render infinite grid
        if (ShowGrid)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);

            GL.UseProgram(_gridShaderProgram);

            GL.UniformMatrix4(_gridMVPLocation, false, ref projection);
            GL.UniformMatrix4(_gridViewLocation, false, ref view);
            GL.Uniform3(_gridCameraPosLocation, cameraPosition);
            GL.Uniform1(_gridNearLocation, near);
            GL.Uniform1(_gridFarLocation, far);

            GL.BindVertexArray(_gridVAO);
            GL.DrawElements(GLPrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
        }

        // Render coordinate axes
        if (ShowAxes)
        {
            GL.UseProgram(_axesShaderProgram);
            var mvp = view * projection;
            GL.UniformMatrix4(_axesMVPLocation, false, ref mvp);

            GL.LineWidth(3.0f);
            GL.BindVertexArray(_axesVAO);

            // X axis - Red
            GL.Uniform4(_axesColorLocation, new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            GL.DrawArrays(GLPrimitiveType.Lines, 0, 2);

            // Y axis - Green
            GL.Uniform4(_axesColorLocation, new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
            GL.DrawArrays(GLPrimitiveType.Lines, 2, 2);

            // Z axis - Blue
            GL.Uniform4(_axesColorLocation, new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
            GL.DrawArrays(GLPrimitiveType.Lines, 4, 2);

            GL.BindVertexArray(0);
            GL.LineWidth(1.0f);
        }

        GL.UseProgram(0);
    }

    private void CheckProgramLink(int program)
    {
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(program);
            throw new Exception($"Program linking error: {infoLog}");
        }
    }

    private void CheckShaderCompile(int shader, string type)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"Shader compilation error ({type}): {infoLog}");
        }
    }

    private void CreateShaders()
    {
        // Infinite grid shader - procedural fragment shader
        const string gridVertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec3 aPosition;
            
            uniform mat4 uMVP;
            uniform mat4 uView;
            
            out vec3 nearPoint;
            out vec3 farPoint;
            
            vec3 UnprojectPoint(float x, float y, float z) {
                mat4 viewInv = inverse(uView);
                mat4 projInv = inverse(uMVP);
                vec4 unprojectedPoint = viewInv * projInv * vec4(x, y, z, 1.0);
                return unprojectedPoint.xyz / unprojectedPoint.w;
            }
            
            void main() {
                nearPoint = UnprojectPoint(aPosition.x, aPosition.y, -1.0);
                farPoint = UnprojectPoint(aPosition.x, aPosition.y, 1.0);
                gl_Position = vec4(aPosition.xy, 0.0, 1.0);
            }
        ";

        const string gridFragmentShaderSource = @"
            #version 330 core
            
            in vec3 nearPoint;
            in vec3 farPoint;
            
            out vec4 FragColor;
            
            uniform mat4 uView;
            uniform mat4 uMVP;
            uniform vec3 uCameraPos;
            uniform float uNear;
            uniform float uFar;
            
            vec4 grid(vec3 fragPos3D, float scale) {
                vec2 coord = fragPos3D.xz * scale;
                vec2 derivative = fwidth(coord);
                vec2 grid = abs(fract(coord - 0.5) - 0.5) / derivative;
                float line = min(grid.x, grid.y);
                float minimumz = min(derivative.y, 1.0);
                float minimumx = min(derivative.x, 1.0);
                vec4 color = vec4(0.3, 0.3, 0.35, 1.0 - min(line, 1.0));
                
                // Z axis (blue line)
                if(fragPos3D.x > -0.1 * minimumx && fragPos3D.x < 0.1 * minimumx)
                    color = vec4(0.2, 0.2, 0.8, 1.0);
                // X axis (red line)
                if(fragPos3D.z > -0.1 * minimumz && fragPos3D.z < 0.1 * minimumz)
                    color = vec4(0.8, 0.2, 0.2, 1.0);
                
                return color;
            }
            
            float computeDepth(vec3 pos) {
                vec4 clip_space_pos = uMVP * uView * vec4(pos, 1.0);
                return (clip_space_pos.z / clip_space_pos.w);
            }
            
            float computeLinearDepth(vec3 pos) {
                vec4 clip_space_pos = uMVP * uView * vec4(pos, 1.0);
                float clip_space_depth = (clip_space_pos.z / clip_space_pos.w);
                float linearDepth = (2.0 * uNear * uFar) / (uFar + uNear - clip_space_depth * (uFar - uNear));
                return linearDepth / uFar;
            }
            
            void main() {
                float t = -nearPoint.y / (farPoint.y - nearPoint.y);
                
                // Clip to valid range
                if(t <= 0.0) {
                    discard;
                }
                
                vec3 fragPos3D = nearPoint + t * (farPoint - nearPoint);
                
                // Compute depth for proper z-testing
                gl_FragDepth = ((gl_DepthRange.diff * computeDepth(fragPos3D)) + gl_DepthRange.near + gl_DepthRange.far) / 2.0;
                
                float linearDepth = computeLinearDepth(fragPos3D);
                float fading = max(0.0, (0.8 - linearDepth));
                
                FragColor = grid(fragPos3D, 1.0);
                FragColor.a *= fading;
                
                if(FragColor.a < 0.05) discard;
            }
        ";

        // Axes shader - simple colored lines
        const string axesVertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec3 aPosition;
            uniform mat4 uMVP;
            void main()
            {
                gl_Position = uMVP * vec4(aPosition, 1.0);
            }
        ";

        const string axesFragmentShaderSource = @"
            #version 330 core
            out vec4 FragColor;
            uniform vec4 uColor;
            void main()
            {
                FragColor = uColor;
            }
        ";

        // Compile grid shader
        int gridVertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(gridVertexShader, gridVertexShaderSource);
        GL.CompileShader(gridVertexShader);
        CheckShaderCompile(gridVertexShader, "GRID VERTEX");

        int gridFragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(gridFragmentShader, gridFragmentShaderSource);
        GL.CompileShader(gridFragmentShader);
        CheckShaderCompile(gridFragmentShader, "GRID FRAGMENT");

        _gridShaderProgram = GL.CreateProgram();
        GL.AttachShader(_gridShaderProgram, gridVertexShader);
        GL.AttachShader(_gridShaderProgram, gridFragmentShader);
        GL.LinkProgram(_gridShaderProgram);
        CheckProgramLink(_gridShaderProgram);

        GL.DeleteShader(gridVertexShader);
        GL.DeleteShader(gridFragmentShader);

        _gridMVPLocation = GL.GetUniformLocation(_gridShaderProgram, "uMVP");
        _gridViewLocation = GL.GetUniformLocation(_gridShaderProgram, "uView");
        _gridCameraPosLocation = GL.GetUniformLocation(_gridShaderProgram, "uCameraPos");
        _gridNearLocation = GL.GetUniformLocation(_gridShaderProgram, "uNear");
        _gridFarLocation = GL.GetUniformLocation(_gridShaderProgram, "uFar");

        // Compile axes shader
        int axesVertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(axesVertexShader, axesVertexShaderSource);
        GL.CompileShader(axesVertexShader);
        CheckShaderCompile(axesVertexShader, "AXES VERTEX");

        int axesFragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(axesFragmentShader, axesFragmentShaderSource);
        GL.CompileShader(axesFragmentShader);
        CheckShaderCompile(axesFragmentShader, "AXES FRAGMENT");

        _axesShaderProgram = GL.CreateProgram();
        GL.AttachShader(_axesShaderProgram, axesVertexShader);
        GL.AttachShader(_axesShaderProgram, axesFragmentShader);
        GL.LinkProgram(_axesShaderProgram);
        CheckProgramLink(_axesShaderProgram);

        GL.DeleteShader(axesVertexShader);
        GL.DeleteShader(axesFragmentShader);

        _axesMVPLocation = GL.GetUniformLocation(_axesShaderProgram, "uMVP");
        _axesColorLocation = GL.GetUniformLocation(_axesShaderProgram, "uColor");
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;

        CreateShaders();
        InitializeInfiniteGrid();
        InitializeAxes();
        _initialized = true;
    }

    private void InitializeAxes()
    {
        const float axisLength = 5.0f;

        var vertices = new float[]
        {
            // X axis - Red
            0, 0, 0,  axisLength, 0, 0,
            // Y axis - Green
            0, 0, 0,  0, axisLength, 0,
            // Z axis - Blue
            0, 0, 0,  0, 0, axisLength
        };

        _axesVAO = GL.GenVertexArray();
        _axesVBO = GL.GenBuffer();

        GL.BindVertexArray(_axesVAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _axesVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float),
            vertices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,
            3 * sizeof(float), 0);

        GL.BindVertexArray(0);
    }

    private void InitializeInfiniteGrid()
    {
        // Create a full-screen quad in NDC (Normalized Device Coordinates)
        var vertices = new float[]
        {
            -1.0f, -1.0f, 0.0f,
             1.0f, -1.0f, 0.0f,
             1.0f,  1.0f, 0.0f,
            -1.0f,  1.0f, 0.0f
        };

        var indices = new uint[]
        {
            0, 1, 2,
            2, 3, 0
        };

        _gridVAO = GL.GenVertexArray();
        _gridVBO = GL.GenBuffer();
        _gridEBO = GL.GenBuffer();

        GL.BindVertexArray(_gridVAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _gridVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float),
            vertices, BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _gridEBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint),
            indices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,
            3 * sizeof(float), 0);

        GL.BindVertexArray(0);
    }

    #endregion 
}