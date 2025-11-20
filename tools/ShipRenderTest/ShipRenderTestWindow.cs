using System;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using WipeoutRewrite.Infrastructure.Graphics;
using Microsoft.Extensions.Logging.Abstractions;
using WipeoutRewrite.Core.Entities;
using System.Linq;
using System.Collections.Generic;

namespace WipeoutRewrite.Tools
{
    /// <summary>
    /// Ship Render Test Tool - Dedicated tool to debug ship rendering.
    /// Tests different scales, positions, and rendering modes.
    /// </summary>
    public class ShipRenderWindow : GameWindow
    {
        private readonly ILogger<ShipRenderWindow> _logger;
        private readonly IShips _ships;
        private readonly IRenderer _renderer;
        private ICamera _camera;
        private ShipV2? _ship;
        private int _testMode = 0;
        private float _scale;
        private bool _autoRotate = false;  // Modo de rotação automática ativado (F)
        private float _totalTime = 0f;  // Tempo acumulado para animação
        private bool _rotateY = false;  // Rotação em Y (tecla 3)

        // Test configurations - 3D world coordinates (not screen pixels!)
        // Camera is at (0, 15, 30) looking at (0, 0, 0)
        // Ship is placed at origin in 3D space
        private readonly (float scale, Vec3 position, string description)[] _testConfigs = new[]
        {
            (1.0f, new Vec3(0, 0, 0), "Full scale - Origin (centered in camera view)"),
            (0.5f, new Vec3(0, 0, 0), "Half scale - Origin"),
            (0.3f, new Vec3(5, 0, 0), "0.3x scale - Slightly right"),
            (0.2f, new Vec3(-5, 0, 0), "0.2x scale - Slightly left"),
            (1.0f, new Vec3(0, 5, 0), "Full scale - Above origin"),
            (1.0f, new Vec3(0, 0, 10), "Full scale - Forward in Z"),
        };

        public ShipRenderWindow(
            GameWindowSettings gws,
            NativeWindowSettings nws,
            IShips ships,   
            ILogger<ShipRenderWindow> logger,
            IRenderer renderer,
            ICamera camera)
            : base(gws, nws)
        {
            _ships = ships ?? throw new ArgumentNullException(nameof(ships));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            _renderer.Init(Size.X, Size.Y);
            
            // Inicializar câmera em modo 3D perspectiva (FOV 73.75° como no C original)
            var aspectRatio = (float)Size.X / Size.Y;
            _camera.SetAspectRatio(aspectRatio);
            _camera.SetIsometricMode(false);  // Usar projeção 3D perspectiva (não isométrica)
            _logger.LogInformation("[RENDER] Window initialized: {Width}x{Height}, AspectRatio: {Aspect}, FOV: 73.75°", Size.X, Size.Y, aspectRatio);
            _logger.LogInformation("[RENDER] Using 3D perspective projection (matching C original)");
            LoadTestShip();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            
            _renderer.UpdateScreenSize(e.Width, e.Height);
            _camera.SetAspectRatio((float)e.Width / e.Height);
            
            _logger.LogInformation("[RENDER] Window resized to: {Width}x{Height}", e.Width, e.Height);
        }

        private void LoadTestShip()
        {
            var (scale, position, description) = _testConfigs[_testMode];

            _logger.LogInformation("\n===========================================");
            _logger.LogInformation("Test Configuration {Index}/{Total}", _testMode + 1, _testConfigs.Length);
            _logger.LogInformation("===========================================");
            _logger.LogInformation("Description: {Description}", description);
            _logger.LogInformation("Scale: {Scale}x", scale);
            _logger.LogInformation("Position: {Position}", position);


            _ships.ShipsInit(null);
            _ship = _ships.AllShips.FirstOrDefault(s=>s.ShipId == 7);
            if (_ship == null)
            {
               throw new InvalidOperationException("No ship loaded to test rendering.");
            };

            _ship.Position = position;
            _ship.IsVisible = true;

            _logger.LogInformation("\nModel loaded:");
            _logger.LogInformation("  Vertices: {Count}", _ship.Model!.Vertices.Length);
            _logger.LogInformation("  Primitives: {Count}", _ship.Model!.Primitives.Count);

            _logger.LogInformation("\nVertex positions (relative to ship position):");
            for (int i = 0; i < _ship.Model!.Vertices.Length; i++)
            {
                var v = _ship.Model!.Vertices[i];
                _logger.LogInformation("  Vertex {Index}: ({X:F1}, {Y:F1}, {Z:F1})",
                    i, v.X, v.Y, v.Z);
            }

            _logger.LogInformation("\nAbsolute screen positions (ship pos + vertex):");
            for (int i = 0; i < Math.Min(3, _ship.Model!.Vertices.Length); i++)
            {
                var v = _ship.Model!.Vertices[i];
                _logger.LogInformation("  Vertex {Index} on screen: ({X:F1}, {Y:F1}, {Z:F1})",
                    i,
                    position.X + v.X,
                    position.Y + v.Y,
                    position.Z + v.Z);
            }

            _logger.LogInformation("===========================================\n");

            _scale = scale;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            {
                Close();
            }

            // Tecla R para resetar rotação da nave
            var rKey = OpenTK.Windowing.GraphicsLibraryFramework.Keys.R;
            if (KeyboardState.IsKeyPressed(rKey))
            {
                if (_ships != null)
                {
                    if (_ship != null)
                    {
                        _ship.Angle = new Vec3(0, 0, MathF.PI);  // PI em Z para orientação correta
                        _totalTime = 0f;
                        _autoRotate = false;
                        _logger.LogInformation("[RENDER] Reset: Rotation disabled, angles reset to initial orientation");
                    }
                }
                System.Threading.Thread.Sleep(200);
            }

            // Tecla F para ativar/desativar rotação automática
            var fKey = OpenTK.Windowing.GraphicsLibraryFramework.Keys.F;
            if (KeyboardState.IsKeyPressed(fKey))
            {
                _autoRotate = !_autoRotate;
                _logger.LogInformation("[RENDER] Auto-rotate mode: {AutoRotate}", _autoRotate);
                System.Threading.Thread.Sleep(800);  // Debounce para evitar toggle rápido
            }

            // Controles de rotação (apenas quando auto-rotate está ativo)
            if (_autoRotate)
            {
                // Tecla 1 para ativar/desativar rotação Z
                // var key1 = OpenTK.Windowing.GraphicsLibraryFramework.Keys.D1;
                // if (KeyboardState.IsKeyPressed(key1))
                // {
                //     _rotateZ = !_rotateZ;
                //     _logger.LogInformation("[RENDER] Rotate Z: {RotateZ}", _rotateZ);
                //     System.Threading.Thread.Sleep(200);
                // }

                // // Tecla 2 para ativar/desativar rotação X
                // var key2 = OpenTK.Windowing.GraphicsLibraryFramework.Keys.D2;
                // if (KeyboardState.IsKeyPressed(key2))
                // {
                //     _rotateX = !_rotateX;
                //     _logger.LogInformation("[RENDER] Rotate X: {RotateX}", _rotateX);
                //     System.Threading.Thread.Sleep(200);
                // }

                // Tecla 3 para ativar/desativar rotação Y
                var key3 = OpenTK.Windowing.GraphicsLibraryFramework.Keys.D3;
                if (KeyboardState.IsKeyPressed(key3))
                {
                    _rotateY = !_rotateY;
                    _logger.LogInformation("[RENDER] Rotate Y: {RotateY}", _rotateY);
                    System.Threading.Thread.Sleep(200);
                }
            }

            // Se auto-rotate ativado, rotaciona a nave automaticamente
            if (_autoRotate && _ships != null)
            {
                _totalTime += (float)args.Time;
                
                if (_ship != null)
                {
                    var angle = _ship.Angle;
                    
                    // // Rotação em Z (yaw) - se ativada
                    // if (_rotateZ)
                    // {
                    //     angle.Z += 2.0f * (float)args.Time;  // 2 rad/s
                        
                    //     // Normalizar ângulo Z (0 a 2π)
                    //     if (angle.Z > MathF.PI * 2)
                    //         angle.Z -= MathF.PI * 2;
                    // }
                    
                    // Rotação em X (pitch) - se ativada
                    // if (_rotateX)
                    // {
                    //     angle.X = MathF.Sin(_totalTime * 0.5f) * 0.5f;  // Oscila entre -0.5 e 0.5 rad
                    // }
                    
                    // Rotação em Y (roll) - se ativada
                    if (_rotateY)
                    {
                        angle.Y += 2.0f * (float)args.Time;  // 2 rad/s (360° ao redor do eixo Y)
                        
                        // Normalizar ângulo Y (0 a 2π)
                        if (angle.Y > MathF.PI * 2)
                            angle.Y -= MathF.PI * 2;
                    }
                    
                    _ship.Angle = angle;
                }
            }

            // Atualizar câmera com input (teclado e mouse)
            if (_camera != null)
            {
                _camera.Update(KeyboardState, MouseState);
            }

            _ships?.ShipsUpdate();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            _renderer.BeginFrame();

            // Clear to dark background
            GL.ClearColor(0.1f, 0.1f, 0.2f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Render the test ship (if loaded)
            if (_ships != null)
            {
                try
                {
                    // MODO 3D PERSPECTIVA - Renderização com câmera orbital (FOV 73.75° como C original)
                    _renderer.SetPassthroughProjection(false);  // Use camera matrices (view + projection)
                    
                    // Configurar matrizes de câmera 3D
                    _renderer.SetProjectionMatrix(_camera.GetProjectionMatrix());
                    _renderer.SetViewMatrix(_camera.GetViewMatrix());
                    
                    // Model matrix identidade (nave na origem, sem rotação extra)
                    var modelMatrix = Matrix4.Identity;
                    _renderer.SetModelMatrix(modelMatrix);
                    
                    // Ativar depth testing e face culling (como no C original)
                    _renderer.SetDepthTest(true);
                    _renderer.SetDepthWrite(true);
                    _renderer.SetFaceCulling(true);  // Culling ATIVADO (backface removal)
                    
                    // Posicionar nave no centro (0,0,0) em coordenadas 3D world space
                    foreach (var ship in _ships.AllShips)
                    {
                        ship.Position = new Vec3(0, 0, 0);
                    }

                    // Ensure a valid texture is bound
                    _renderer.SetCurrentTexture(_renderer.WhiteTexture);

                    _logger.LogInformation("[RENDER] 3D PERSPECTIVE MODE - Camera pos: {Pos}, yaw: {Yaw}, pitch: {Pitch}", 
                        _camera.Position, _camera.Yaw, _camera.Pitch);

                    // Render ship com face culling ativo
                    _ships.ShipsRenderer();
                    
                    // Render shadow (semi-transparent, blended)
                    _renderer.SetBlending(true);
                    foreach (var ship in _ships.AllShips)
                    {
                        ship.RenderShadow();
                    }
                    _renderer.SetBlending(false);
                    
                    // Restaurar estado de renderização
                    _renderer.SetDepthTest(false);
                    _renderer.SetDepthWrite(false);
                    _renderer.SetFaceCulling(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error rendering test ship");
                }
            }

            _renderer.EndFrame();
            SwapBuffers();
        }
    }
}
