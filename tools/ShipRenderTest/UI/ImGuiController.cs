using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// ImGui controller for OpenTK integration.
/// Manages ImGui rendering and input in OpenGL context.
/// </summary>
public class ImGuiController : IDisposable
{
    #region fields
    private int _elementBuffer;
    private int _fontTextureHandle = 1;
    private bool _frameBegun = false;
    private int _projMatrixLocation;
    private int _shaderHandle;
    private int _vertexArray;
    private int _vertexBuffer;
    private int _windowHeight;
    private int _windowWidth;
    #endregion

    public ImGuiController(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;

        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        var io = ImGui.GetIO();
        io.Fonts.AddFontDefault();

        CreateDeviceResources();
        BuildFontAtlas();

        SetPerFrameImGuiData(1f / 60f);
    }

    #region methods

    public void BeginFrame()
    {
        if (_frameBegun)
            throw new InvalidOperationException("ImGui frame already begun");

        SetPerFrameImGuiData(1f / 60f);

        // Clear mouse wheel delta from previous frame
        var io = ImGui.GetIO();
        io.MouseWheel = 0;
        io.MouseWheelH = 0;

        ImGui.NewFrame();
        _frameBegun = true;

        // Enable scissor test for ImGui
        GL.Enable(EnableCap.ScissorTest);
    }

    public void EndFrame()
    {
        if (!_frameBegun)
            throw new InvalidOperationException("ImGui frame not begun");

        ImGui.Render();

        // Save GL state
        GL.GetInteger(GetPName.ActiveTexture, out int lastActiveTexture);
        GL.GetInteger(GetPName.CurrentProgram, out int lastProgram);
        GL.GetInteger(GetPName.TextureBinding2D, out int lastTexture);
        GL.GetInteger(GetPName.ArrayBufferBinding, out int lastArrayBuffer);
        GL.GetInteger(GetPName.VertexArrayBinding, out int lastVertexArray);
        GL.GetInteger(GetPName.BlendSrcRgb, out int lastBlendSrcRgb);
        GL.GetInteger(GetPName.BlendDstRgb, out int lastBlendDstRgb);
        GL.GetBoolean(GetPName.Blend, out bool lastBlendEnabled);
        GL.GetBoolean(GetPName.ScissorTest, out bool lastScissorTestEnabled);

        // Setup GL state for ImGui rendering
        GL.UseProgram(_shaderHandle);
        GL.BindVertexArray(_vertexArray);
        GL.Enable(EnableCap.Blend);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.ScissorTest);

        // Setup orthographic projection matrix
        var io = ImGui.GetIO();
        var L = 0.0f;
        var R = io.DisplaySize.X;
        var T = 0.0f;
        var B = io.DisplaySize.Y;

        var orthoProjection = new float[]
        {
                2.0f/(R-L),   0.0f,         0.0f,   0.0f,
                0.0f,         2.0f/(T-B),   0.0f,   0.0f,
                0.0f,         0.0f,        -1.0f,   0.0f,
                (R+L)/(L-R),  (T+B)/(B-T),  0.0f,   1.0f,
        };

        GL.UniformMatrix4(_projMatrixLocation, 1, false, orthoProjection);

        RenderImDrawData(ImGui.GetDrawData());

        // Restore GL state
        GL.UseProgram(lastProgram);
        GL.BindVertexArray(lastVertexArray);
        GL.ActiveTexture((TextureUnit)lastActiveTexture);
        GL.BindTexture(TextureTarget.Texture2D, lastTexture);
        GL.BindBuffer(BufferTarget.ArrayBuffer, lastArrayBuffer);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc((BlendingFactor)lastBlendSrcRgb, (BlendingFactor)lastBlendDstRgb);
        if (lastBlendEnabled) GL.Enable(EnableCap.Blend);
        else GL.Disable(EnableCap.Blend);
        if (lastScissorTestEnabled) GL.Enable(EnableCap.ScissorTest);
        else GL.Disable(EnableCap.ScissorTest);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);

        // Ensure scissor test is disabled for next frame
        GL.Disable(EnableCap.ScissorTest);

        _frameBegun = false;
    }

    public void PressChar(char keyChar)
    {
        var io = ImGui.GetIO();
        io.AddInputCharacter(keyChar);
    }

    public void RenderImDrawData(ImDrawDataPtr draw_data)
    {
        if (draw_data.CmdListsCount == 0)
            return;

        for (int i = 0; i < draw_data.CmdListsCount; i++)
        {
            ImDrawListPtr cmd_list = draw_data.CmdLists[i];

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, cmd_list.VtxBuffer.Size * 20, cmd_list.VtxBuffer.Data, BufferUsageHint.StreamDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data, BufferUsageHint.StreamDraw);

            for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
            {
                ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];

                if (pcmd.UserCallback != IntPtr.Zero)
                {
                    continue;
                }

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);

                // Clip rectangle
                var clip = pcmd.ClipRect;
                GL.Scissor(
                    (int)clip.X,
                    (int)(_windowHeight - clip.W),
                    (int)(clip.Z - clip.X),
                    (int)(clip.W - clip.Y)
                );

                GL.DrawElements(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)));
            }
        }
    }

    public void UpdateKeyboard(OpenTK.Windowing.GraphicsLibraryFramework.KeyboardState keyboardState)
    {
        var io = ImGui.GetIO();

        // Map common keys
        io.AddKeyEvent(ImGuiKey.Tab, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Tab));
        io.AddKeyEvent(ImGuiKey.LeftArrow, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left));
        io.AddKeyEvent(ImGuiKey.RightArrow, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right));
        io.AddKeyEvent(ImGuiKey.UpArrow, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up));
        io.AddKeyEvent(ImGuiKey.DownArrow, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down));
        io.AddKeyEvent(ImGuiKey.PageUp, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.PageUp));
        io.AddKeyEvent(ImGuiKey.PageDown, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.PageDown));
        io.AddKeyEvent(ImGuiKey.Home, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Home));
        io.AddKeyEvent(ImGuiKey.End, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.End));
        io.AddKeyEvent(ImGuiKey.Delete, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Delete));
        io.AddKeyEvent(ImGuiKey.Backspace, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backspace));
        io.AddKeyEvent(ImGuiKey.Enter, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter));
        io.AddKeyEvent(ImGuiKey.Escape, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape));
        io.AddKeyEvent(ImGuiKey.Space, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space));
        io.AddKeyEvent(ImGuiKey.A, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A));
        io.AddKeyEvent(ImGuiKey.C, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.C));
        io.AddKeyEvent(ImGuiKey.V, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.V));
        io.AddKeyEvent(ImGuiKey.X, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.X));
        io.AddKeyEvent(ImGuiKey.Y, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Y));
        io.AddKeyEvent(ImGuiKey.Z, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Z));

        // Modifiers
        io.AddKeyEvent(ImGuiKey.ModCtrl, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftControl) || keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightControl));
        io.AddKeyEvent(ImGuiKey.ModShift, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift) || keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightShift));
        io.AddKeyEvent(ImGuiKey.ModAlt, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftAlt) || keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightAlt));
        io.AddKeyEvent(ImGuiKey.ModSuper, keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftSuper) || keyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightSuper));
    }

    public void UpdateMouseButton(int button, bool pressed)
    {
        var io = ImGui.GetIO();
        if (button >= 0 && button < 5)
        {
            io.MouseDown[button] = pressed;
            // Debug: Log mouse button events
            if (pressed)
            {
                System.Console.WriteLine($"[ImGui] Mouse button {button} pressed at ({io.MousePos.X}, {io.MousePos.Y})");
            }
        }
    }

    public void UpdateMousePosition(float x, float y)
    {
        // OpenTK MouseState origin is top-left; ImGui expects top-left, so no flip is needed.
        var io = ImGui.GetIO();
        io.MousePos = new System.Numerics.Vector2(x, y);
    }

    public void UpdateMouseScroll(float deltaY)
    {
        var io = ImGui.GetIO();
        io.MouseWheel += deltaY;
    }

    public void WindowResized(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }

    private unsafe void BuildFontAtlas()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out byte* pixel_data, out int width, out int height, out int bytes_per_pixel);

        _fontTextureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fontTextureHandle);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)pixel_data);

        io.Fonts.SetTexID((IntPtr)_fontTextureHandle);
        io.Fonts.ClearTexData();
    }

    private void CheckCompileErrors(int shader, string type)
    {
        if (type == "PROGRAM")
        {
            GL.GetProgram(shader, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                GL.GetProgramInfoLog(shader, out string infoLog);
                throw new Exception($"Program linking failed: {infoLog}");
            }
        }
        else
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                GL.GetShaderInfoLog(shader, out string infoLog);
                throw new Exception($"Shader compilation failed ({type}): {infoLog}");
            }
        }
    }

    private void CreateDeviceResources()
    {
        _vertexArray = GL.GenVertexArray();
        _vertexBuffer = GL.GenBuffer();
        _elementBuffer = GL.GenBuffer();

        GL.BindVertexArray(_vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, 10000 * 20, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer);
        GL.BufferData(BufferTarget.ElementArrayBuffer, 10000 * 2, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
        var stride = 20;
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        GL.EnableVertexAttribArray(2);

        _shaderHandle = CreateProgram();
        _projMatrixLocation = GL.GetUniformLocation(_shaderHandle, "ProjMtx");

        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    private int CreateProgram()
    {
        var vertexSrc = @"
#version 330 core
layout (location = 0) in vec2 Position;
layout (location = 1) in vec2 UV;
layout (location = 2) in vec4 Color;

out vec2 Frag_UV;
out vec4 Frag_Color;

uniform mat4 ProjMtx;

void main()
{
    Frag_UV = UV;
    Frag_Color = Color;
    gl_Position = ProjMtx * vec4(Position.xy,0,1);
}
";
        var fragmentSrc = @"
#version 330 core
in vec2 Frag_UV;
in vec4 Frag_Color;
uniform sampler2D Texture;
layout (location = 0) out vec4 Out_Color;

void main()
{
    Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
}
";

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexSrc);
        GL.CompileShader(vertexShader);
        CheckCompileErrors(vertexShader, "VERTEX");

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentSrc);
        GL.CompileShader(fragmentShader);
        CheckCompileErrors(fragmentShader, "FRAGMENT");

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertexShader);
        GL.AttachShader(program, fragmentShader);
        GL.LinkProgram(program);
        CheckCompileErrors(program, "PROGRAM");

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        return program;
    }

    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new System.Numerics.Vector2(_windowWidth, _windowHeight);
        io.DisplayFramebufferScale = System.Numerics.Vector2.One;
        io.DeltaTime = deltaSeconds;
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        GL.DeleteBuffer(_vertexBuffer);
        GL.DeleteBuffer(_elementBuffer);
        GL.DeleteVertexArray(_vertexArray);
        GL.DeleteProgram(_shaderHandle);
        GL.DeleteTexture(_fontTextureHandle);
    }

    #endregion 
}