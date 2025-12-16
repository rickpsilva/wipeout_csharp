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
        private int _whiteTexture; // 1x1 white texture for solid color rendering
        private int _vbo;
        private int _vao;
        private int _shaderProgram;        // For normal rendering (ships)
        private int _videoShaderProgram;    // For video rendering (no alpha discard)
        private int _screenWidth, _screenHeight;
        private bool _usePassthroughProjection = false; // For 3D meshes already projected to screen space
        private bool _is2DMode = false; // Track if we're in 2D rendering mode
        
        // Guardar matrizes para usar no Flush
        private Matrix4 _projectionMatrix = Matrix4.Identity;
        private Matrix4 _viewMatrix = Matrix4.Identity;
        private Matrix4 _modelMatrix = Matrix4.Identity;

        public int WhiteTexture => _whiteTexture;
        public int ScreenWidth => _screenWidth;
        public int ScreenHeight => _screenHeight;

        public void SetDepthTest(bool enabled)
        {
            if (enabled)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Less);
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);
            }
        }

        public void SetProjectionMatrix(Matrix4 projection)
        {
            _projectionMatrix = projection;
            GL.UseProgram(_shaderProgram);
            int loc = GL.GetUniformLocation(_shaderProgram, "projection");
            GL.UniformMatrix4(loc, false, ref projection);
        }

        public void SetViewMatrix(Matrix4 view)
        {
            _viewMatrix = view;
            GL.UseProgram(_shaderProgram);
            int loc = GL.GetUniformLocation(_shaderProgram, "view");
            GL.UniformMatrix4(loc, false, ref view);
        }

        public void SetModelMatrix(Matrix4 model)
        {
            _modelMatrix = model;
            GL.UseProgram(_shaderProgram);
            int loc = GL.GetUniformLocation(_shaderProgram, "model");
            GL.UniformMatrix4(loc, false, ref model);
        }

        public void SetDepthWrite(bool enabled)
        {
            // DepthMask controls writing to the depth buffer while leaving depth testing enabled.
            GL.DepthMask(enabled);
        }

        // Alpha test state (fragment discard based on texture alpha)
        private bool _alphaTestEnabled = false;
        private float _alphaTestThreshold = 0.5f;

        public void SetAlphaTest(bool enabled)
        {
            _alphaTestEnabled = enabled;
        }

        public void SetBlending(bool enabled)
        {
            if (enabled) GL.Enable(EnableCap.Blend);
            else GL.Disable(EnableCap.Blend);
        }

    public void SetFaceCulling(bool enabled)
    {
        if (enabled)
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Back);
        }
        else
        {
            GL.Disable(EnableCap.CullFace);
        }
    }        public void SetPassthroughProjection(bool enabled)
        {
            _usePassthroughProjection = enabled;
        }

        public void SetDirectionalLight(Vector3 direction, Vector3 color, float intensity)
        {
            GL.UseProgram(_shaderProgram);
            
            int lightDirLoc = GL.GetUniformLocation(_shaderProgram, "lightDir");
            int lightColorLoc = GL.GetUniformLocation(_shaderProgram, "lightColor");
            int lightIntensityLoc = GL.GetUniformLocation(_shaderProgram, "lightIntensity");
            
            GL.Uniform3(lightDirLoc, direction.Normalized());
            GL.Uniform3(lightColorLoc, color);
            GL.Uniform1(lightIntensityLoc, intensity);
        }

        public void UpdateScreenSize(int width, int height)
        {
            _screenWidth = width;
            _screenHeight = height;
        }

        public void Init(int width, int height)
        {
            _screenWidth = width;
            _screenHeight = height;

            // Shader setup - normal shader for ships
            _shaderProgram = CreateProgram(VertexShaderSource, FragmentShaderSource);
            
            // Shader setup - video shader (no alpha discard)
            _videoShaderProgram = CreateProgram(VertexShaderSource, VideoFragmentShaderSource);
            
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
            
            // Create 1x1 white texture for solid color rendering
            _whiteTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _whiteTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            byte[] whitePixel = { 255, 255, 255, 255 };
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1, 1, 0, PixelFormat.Rgba, PixelType.UnsignedByte, whitePixel);
            
            // Set white texture as default
            _spriteTexture = _whiteTexture;
            
            // Enable depth testing (like C original - always ON)
            // This prevents seeing through the ship interior
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.DepthMask(true);
            
            // Enable face culling by default (like C original)
            // Individual objects can disable if needed
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Back);
            
            // Enable blending for transparency
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            // Set default directional light (white light from top-left-front)
            SetDirectionalLight(
                new Vector3(-1.0f, -1.0f, -1.0f).Normalized(),
                new Vector3(1.0f, 1.0f, 1.0f),
                0.7f
            );
        }

        /// <summary>
        /// Create an OpenGL texture from raw RGBA pixels and return the GL handle.
        /// Exposed so other systems (FontSystem, TextureManager) can reuse it.
        /// </summary>
        public int CreateTexture(byte[] pixels, int width, int height)
        {
            int textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return textureId;
        }

        private static void SetupVertexAttributes()
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
        private readonly float[] _vertexBuffer = new float[MaxTris * 3 * 9]; // 3 vertices x 9 floats (pos[3], uv[2], color[4])
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

        public void PushSpriteWithTexture(float x, float y, float w, float h, OpenTK.Mathematics.Color4 color, int textureId, float u0, float v0, float u1, float v1)
        {
            // Check if we need to switch textures (flush if different texture)
            if (_spriteTexture != textureId && _trisLen > 0)
            {
                // Use appropriate flush based on current mode
                if (_is2DMode)
                    FlushVideo();
                else
                    Flush();
            }

            // Check if batch is full
            if (_trisLen >= MaxTris - 2)
            {
                // Use appropriate flush based on current mode
                if (_is2DMode)
                    FlushVideo();
                else
                    Flush();
            }

            // Set the texture (will be bound in EndFrame/Flush)
            _spriteTexture = textureId;

            Vector4 col = new(color.R, color.G, color.B, color.A);
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
            // Validate texture ID - must be positive and not zero
            if (textureId <= 0)
            {
                Console.WriteLine($"[RENDERER] WARNING: Invalid texture ID {textureId}, using white texture instead");
                textureId = _whiteTexture;
            }
            
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

        public void RenderVideoFrame(byte[] frameData, int videoWidth, int videoHeight, int windowWidth, int windowHeight)
        {
            // Create a temporary OpenGL texture from the frame data
            if (frameData == null || frameData.Length == 0)
                return;
            
            int expectedSize = videoWidth * videoHeight * 4;
            if (frameData.Length != expectedSize)
            {
                // Log warning but continue - may just be padding issues
                System.Diagnostics.Debug.WriteLine($"Warning: Frame data size mismatch. Expected {expectedSize}, got {frameData.Length}");
            }
            
            int tempTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tempTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                         videoWidth, videoHeight, 0,
                         PixelFormat.Rgba, PixelType.UnsignedByte, frameData);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            
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
            
            // Salvar estado atual
            int oldTexture = _spriteTexture;
            
            // Use temporary video texture
            _spriteTexture = tempTexture;
            
            // Sprite centrado com aspect ratio correto
            PushSprite(offsetX, offsetY, renderWidth, renderHeight, new Vector4(1, 1, 1, 1));
            
            // Use video shader to render (already bound from Setup2DRendering in caller)
            FlushVideo();
            
            // Restaurar estado
            _spriteTexture = oldTexture;
            
            // Clean up temporary texture AFTER rendering is complete
            GL.DeleteTexture(tempTexture);
        }

        public void BeginFrame()
        {
            GL.Viewport(0, 0, _screenWidth, _screenHeight);
            GL.ClearColor(0.05f, 0.07f, 0.12f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            // Enable depth testing for 3D ship rendering and enable depth writing
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.DepthMask(true);
            
            // Enable alpha blending for transparent text rendering
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            // Disable face culling by default - will be enabled for 3D rendering when needed
            // This ensures 2D sprites (video, menu, UI) are always visible
            GL.Disable(EnableCap.CullFace);
            
            _trisLen = 0;
        }
        
        // Setup for 2D sprite rendering (menus, UI, video)
        public void Setup2DRendering()
        {
            _is2DMode = true;
            
            // Setup 2D orthographic projection
            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, _screenWidth, _screenHeight, 0, -1, 1);
            _viewMatrix = Matrix4.Identity;
            _modelMatrix = Matrix4.Identity;
            
            // Ativar blending para transparência (importante para fontes e UI)
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            // Use simple video shader for 2D (no ambient lighting, no color multiplication)
            GL.UseProgram(_videoShaderProgram);
            int projLoc = GL.GetUniformLocation(_videoShaderProgram, "projection");
            GL.UniformMatrix4(projLoc, false, ref _projectionMatrix);
            int viewLoc = GL.GetUniformLocation(_videoShaderProgram, "view");
            GL.UniformMatrix4(viewLoc, false, ref _viewMatrix);
            int modelLoc = GL.GetUniformLocation(_videoShaderProgram, "model");
            GL.UniformMatrix4(modelLoc, false, ref _modelMatrix);
        }
        
        // End frame for 2D rendering
        public void EndFrame2D()
        {
            _is2DMode = false;
            
            // Bind sprite texture se houver
            if (_spriteTexture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _spriteTexture);
                int loc = GL.GetUniformLocation(_videoShaderProgram, "texture0");
                GL.Uniform1(loc, 0);
            }
            FlushVideo();
        }

        public void EndFrame()
        {
            _is2DMode = false;
            
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

            // Bind the current texture - validate first
            if (_spriteTexture > 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _spriteTexture);
                
                int loc = GL.GetUniformLocation(_shaderProgram, "texture0");
                if (loc >= 0)
                {
                    GL.Uniform1(loc, 0);
                }
            }
            else
            {
                // Use white texture as fallback
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _whiteTexture);
                Console.WriteLine($"[RENDERER] WARNING: Invalid sprite texture {_spriteTexture}, using white texture");
            }

            // Setup projection matrix - NÃO sobrescrever! As matrizes já foram setadas em SetProjectionMatrix
            // Apenas usar alpha test
            
            // Push alpha test uniform
            int alphaLoc = GL.GetUniformLocation(_shaderProgram, "alphaTest");
            if (alphaLoc >= 0)
            {
                GL.Uniform1(alphaLoc, _alphaTestEnabled ? 1 : 0);
            }
            int thrLoc = GL.GetUniformLocation(_shaderProgram, "alphaThreshold");
            if (thrLoc >= 0)
            {
                GL.Uniform1(thrLoc, _alphaTestThreshold);
            }

            GL.DrawArrays(PrimitiveType.Triangles, 0, _trisLen * 3);
            _trisLen = 0;
        }
        
        // Flush using video shader (no alpha discard)
        private void FlushVideo()
        {
            if (_trisLen == 0) return;
            
            GL.UseProgram(_videoShaderProgram);
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _trisLen * 3 * 9, _vertexBuffer, BufferUsageHint.DynamicDraw);

            // Bind the current texture
            if (_spriteTexture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _spriteTexture);
                
                int loc = GL.GetUniformLocation(_videoShaderProgram, "texture0");
                if (loc >= 0)
                {
                    GL.Uniform1(loc, 0);
                }
            }

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
            GL.DeleteTexture(_whiteTexture);
            GL.DeleteProgram(_shaderProgram);
            GL.DeleteProgram(_videoShaderProgram);
        }

        private static int CompileShader(ShaderType type, string source)
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

        private static int CreateProgram(string vsSource, string fsSource)
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
            out vec3 v_fragPos;
            uniform mat4 projection;
            uniform mat4 view;
            uniform mat4 model;
            void main() {
                vec4 worldPos = model * vec4(pos, 1.0);
                gl_Position = projection * view * worldPos;
                v_color = color;
                v_uv = uv;
                v_fragPos = worldPos.xyz;
            }
        ";

        private const string FragmentShaderSource = @"
            #version 330 core
            in vec4 v_color;
            in vec2 v_uv;
            in vec3 v_fragPos;
            out vec4 FragColor;
            uniform sampler2D texture0;
            // Alpha test control: when alphaTest==1, discard fragments with alpha <= alphaThreshold
            uniform int alphaTest;
            uniform float alphaThreshold;
            // Directional light uniforms
            uniform vec3 lightDir;
            uniform vec3 lightColor;
            uniform float lightIntensity;
            void main() {
                vec4 texColor = texture(texture0, v_uv);
                vec4 color = texColor * v_color;
                if (alphaTest == 1 && texColor.a <= alphaThreshold) discard;
                if (color.a == 0.0) discard;
                
                // PSX colors are typically 128,128,128 (half intensity) so multiply by 2.0 like original
                color.rgb = color.rgb * 2.0;
                
                // Calculate normal from screen-space derivatives
                vec3 normal = normalize(cross(dFdx(v_fragPos), dFdy(v_fragPos)));
                
                // Calculate lighting
                float diffuse = max(dot(normal, -lightDir), 0.0);
                vec3 lighting = lightColor * lightIntensity * diffuse;
                
                // Add ambient light so objects aren't completely black
                vec3 ambient = vec3(0.3, 0.3, 0.3);
                vec3 finalLighting = ambient + lighting;
                
                color.rgb *= finalLighting;
                
                // Ambient lighting: darken backfaces to simulate interior shadowing
                // gl_FrontFacing is true for front faces, false for back faces
                if (!gl_FrontFacing) {
                    color.rgb *= 0.08; // Darken backfaces to 8% brightness (92% darker)
                }
                FragColor = color;
            }
        ";
        
        // Fragment shader for video rendering (no alpha discard, no color multiplication)
        private const string VideoFragmentShaderSource = @"
            #version 330 core
            in vec4 v_color;
            in vec2 v_uv;
            out vec4 FragColor;
            uniform sampler2D texture0;
            void main() {
                vec4 texColor = texture(texture0, v_uv);
                // Simply output the texture color multiplied by vertex color (for brightness control)
                FragColor = texColor * v_color;
            }
        ";
    }
}
