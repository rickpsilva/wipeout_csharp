using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace WipeoutRewrite.Infrastructure.Graphics
{
    /// <summary>
    /// Implementação OpenGL do IRenderer.
    /// </summary>
    public class GLRenderer : IRenderer
    {
        private int _atlasTexture;
        private int _vbo;
        private int _vao;
        private int _shaderProgram;
        private int _screenWidth, _screenHeight;

        public void UpdateScreenSize(int width, int height)
        {
            _screenWidth = width;
            _screenHeight = height;
        }

        public void Init(int width, int height)
        {
            _screenWidth = width;
            _screenHeight = height;

            // Shader setup
            _shaderProgram = CreateProgram(VertexShaderSource, FragmentShaderSource);
            GL.UseProgram(_shaderProgram);

            // VAO/VBO setup
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _vertexBuffer.Length, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // Setup vertex attributes
            SetupVertexAttributes();

            // Texture setup
            _atlasTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _atlasTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            // Allocate empty atlas (placeholder)
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 2048, 2048, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        }

        private void SetupVertexAttributes()
        {
            const int stride = 9 * sizeof(float); // 9 floats per vertex
            
            // Position attribute (3 floats)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            // UV attribute (2 floats)
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Color attribute (4 floats)
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));
            GL.EnableVertexAttribArray(2);
        }


        // Buffer for triangles (vertex data)
        private const int MaxTris = 2048;
        private float[] _vertexBuffer = new float[MaxTris * 3 * 9]; // 3 vertices x 9 floats (pos[3], uv[2], color[4])
        private int _trisLen = 0;

        // Textura simples para sprites
        private int _spriteTexture = 0;

        public void LoadSpriteTexture(string path)
        {
            // Carregar PNG multiplataforma usando ImageSharp
            using var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(path);
            var pixels = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixels);

            _spriteTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _spriteTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        public void PushSprite(float x, float y, float w, float h, OpenTK.Mathematics.Vector4 color)
        {
            // Sprite quad (duas tris)
            float z = 0;
            PushTri(
                new Vector3(x, y, z), new Vector2(0, 0), color,
                new Vector3(x + w, y, z), new Vector2(1, 0), color,
                new Vector3(x, y + h, z), new Vector2(0, 1), color
            );
            PushTri(
                new Vector3(x + w, y, z), new Vector2(1, 0), color,
                new Vector3(x + w, y + h, z), new Vector2(1, 1), color,
                new Vector3(x, y + h, z), new Vector2(0, 1), color
            );
        }

        public void RenderFullscreenTexture(int textureId, int windowWidth, int windowHeight)
        {
            int oldTexture = _spriteTexture;
            
            // Clear screen
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            BeginFrame();
            _spriteTexture = textureId;
            
            // Render fullscreen quad
            PushSprite(0, 0, windowWidth, windowHeight, new OpenTK.Mathematics.Vector4(1, 1, 1, 1));
            
            EndFrame();
            
            _spriteTexture = oldTexture;
        }

        public void PushSpriteWithTexture(float x, float y, float w, float h, OpenTK.Mathematics.Color4 color, int textureId, float u0, float v0, float u1, float v1)
        {
            // Check if we need to switch textures (flush if different texture)
            if (_spriteTexture != textureId && _trisLen > 0)
            {
                Flush();
            }

            // Check if batch is full
            if (_trisLen >= MaxTris - 2)
            {
                Flush();
            }

            // Set the texture (will be bound in EndFrame/Flush)
            _spriteTexture = textureId;

            Vector4 col = new Vector4(color.R, color.G, color.B, color.A);
            const float z = 0;

            // Push two triangles with custom UV coordinates
            PushTri(
                new Vector3(x, y, z), new Vector2(u0, v0), col,
                new Vector3(x + w, y, z), new Vector2(u1, v0), col,
                new Vector3(x, y + h, z), new Vector2(u0, v1), col
            );
            PushTri(
                new Vector3(x + w, y, z), new Vector2(u1, v0), col,
                new Vector3(x + w, y + h, z), new Vector2(u1, v1), col,
                new Vector3(x, y + h, z), new Vector2(u0, v1), col
            );
        }

        public int GetCurrentTexture()
        {
            return _spriteTexture;
        }

        public void SetCurrentTexture(int textureId)
        {
            if (_spriteTexture != textureId && _trisLen > 0)
            {
                Flush();
            }
            _spriteTexture = textureId;
        }

        public void RenderVideoFrame(int videoTextureId, int videoWidth, int videoHeight, int windowWidth, int windowHeight)
        {
            // Calculate video and window aspect ratio
            float videoAspect = (float)videoWidth / videoHeight;
            float windowAspect = (float)windowWidth / windowHeight;
            
            float renderWidth, renderHeight;
            float offsetX = 0, offsetY = 0;
            
            // COVER MODE: Fill entire window (may crop video edges)
            if (windowAspect > videoAspect)
            {
                // Janela mais larga - escalar pela largura
                renderWidth = windowWidth;
                renderHeight = windowWidth / videoAspect;
                offsetY = (windowHeight - renderHeight) / 2;
            }
            else
            {
                // Janela mais alta - escalar pela altura
                renderHeight = windowHeight;
                renderWidth = windowHeight * videoAspect;
                offsetX = (windowWidth - renderWidth) / 2;
            }
            
            // Salvar textura atual
            int oldTexture = _spriteTexture;
            
            // Use video texture temporarily
            _spriteTexture = videoTextureId;
            
            // Desenhar usando o sistema que JÁ FUNCIONA
            BeginFrame();
            
            // Sprite centrado com aspect ratio correto
            PushSprite(offsetX, offsetY, renderWidth, renderHeight, new Vector4(1, 1, 1, 1));
            
            EndFrame();
            
            // Restaurar textura original
            _spriteTexture = oldTexture;
        }

        public void BeginFrame()
        {
            GL.Viewport(0, 0, _screenWidth, _screenHeight);
            GL.ClearColor(0.05f, 0.07f, 0.12f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            // Enable alpha blending for transparent text rendering
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            _trisLen = 0;
        }

        public void EndFrame()
        {
            // Bind sprite texture se houver
            if (_spriteTexture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _spriteTexture);
                int loc = GL.GetUniformLocation(_shaderProgram, "texture0");
                GL.Uniform1(loc, 0);
            }
            Flush();
        }

        // Migrado de render_flush
        public void Flush()
        {
            if (_trisLen == 0) return;
            
            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _trisLen * 3 * 9, _vertexBuffer, BufferUsageHint.DynamicDraw);

            // Bind the current texture
            if (_spriteTexture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _spriteTexture);
                int loc = GL.GetUniformLocation(_shaderProgram, "texture0");
                GL.Uniform1(loc, 0);
            }

            // Setup projection matrix (2D orthographic)
            var projection = Matrix4.CreateOrthographicOffCenter(0, _screenWidth, _screenHeight, 0, -1, 1);
            var projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
            GL.UniformMatrix4(projLoc, false, ref projection);

            GL.DrawArrays(PrimitiveType.Triangles, 0, _trisLen * 3);
            _trisLen = 0;
        }

        // Migrado de render_push_tris
        public void PushTri(Vector3 p1, Vector2 uv1, Vector4 color1,
                            Vector3 p2, Vector2 uv2, Vector4 color2,
                            Vector3 p3, Vector2 uv3, Vector4 color3)
        {
            if (_trisLen >= MaxTris) Flush();
            int baseIdx = _trisLen * 3 * 9;
            // Vertex 1
            _vertexBuffer[baseIdx + 0] = p1.X;
            _vertexBuffer[baseIdx + 1] = p1.Y;
            _vertexBuffer[baseIdx + 2] = p1.Z;
            _vertexBuffer[baseIdx + 3] = uv1.X;
            _vertexBuffer[baseIdx + 4] = uv1.Y;
            _vertexBuffer[baseIdx + 5] = color1.X;
            _vertexBuffer[baseIdx + 6] = color1.Y;
            _vertexBuffer[baseIdx + 7] = color1.Z;
            _vertexBuffer[baseIdx + 8] = color1.W;
            // Vertex 2
            _vertexBuffer[baseIdx + 9] = p2.X;
            _vertexBuffer[baseIdx + 10] = p2.Y;
            _vertexBuffer[baseIdx + 11] = p2.Z;
            _vertexBuffer[baseIdx + 12] = uv2.X;
            _vertexBuffer[baseIdx + 13] = uv2.Y;
            _vertexBuffer[baseIdx + 14] = color2.X;
            _vertexBuffer[baseIdx + 15] = color2.Y;
            _vertexBuffer[baseIdx + 16] = color2.Z;
            _vertexBuffer[baseIdx + 17] = color2.W;
            // Vertex 3
            _vertexBuffer[baseIdx + 18] = p3.X;
            _vertexBuffer[baseIdx + 19] = p3.Y;
            _vertexBuffer[baseIdx + 20] = p3.Z;
            _vertexBuffer[baseIdx + 21] = uv3.X;
            _vertexBuffer[baseIdx + 22] = uv3.Y;
            _vertexBuffer[baseIdx + 23] = color3.X;
            _vertexBuffer[baseIdx + 24] = color3.Y;
            _vertexBuffer[baseIdx + 25] = color3.Z;
            _vertexBuffer[baseIdx + 26] = color3.W;
            _trisLen++;
        }

        public void Cleanup()
        {
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteTexture(_atlasTexture);
            GL.DeleteProgram(_shaderProgram);
        }

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status != (int)All.True)
            {
                string log = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error compiling {type}: {log}");
            }
            return shader;
        }

        private int CreateProgram(string vsSource, string fsSource)
        {
            int vs = CompileShader(ShaderType.VertexShader, vsSource);
            int fs = CompileShader(ShaderType.FragmentShader, fsSource);
            int program = GL.CreateProgram();
            GL.AttachShader(program, vs);
            GL.AttachShader(program, fs);
            GL.LinkProgram(program);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);
            return program;
        }

        // Minimal shader sources (expand as needed)
        private const string VertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 pos;
            layout(location = 1) in vec2 uv;
            layout(location = 2) in vec4 color;
            out vec4 v_color;
            out vec2 v_uv;
            uniform mat4 projection;
            void main() {
                gl_Position = projection * vec4(pos, 1.0);
                v_color = color;
                v_uv = uv;
            }
        ";

        private const string FragmentShaderSource = @"
            #version 330 core
            in vec4 v_color;
            in vec2 v_uv;
            out vec4 FragColor;
            uniform sampler2D texture0;
            void main() {
                vec4 texColor = texture(texture0, v_uv);
                FragColor = texColor * v_color;
            }
        ";
    }
}
