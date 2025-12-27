using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Infrastructure.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace WipeoutRewrite.Presentation
{
    public class ContentPreview3D : IContentPreview3D
    {
        private readonly ILogger<ContentPreview3D> _logger;
        private readonly IGameObjectCollection _gameObjects;
        private readonly IRenderer _renderer;
        private readonly ICamera _camera;
        
        private bool _initialized = false;
        private bool _cameraConfigured = false;
        private float _rotationAngle = 0f;
        private int _currentModelId = -1;
        
        // Configurações de posicionamento (ajustáveis)
        private Vec3 _shipPosition = new Vec3(0, 0, -15);  // Mais afastado em Z negativo
        private Vec3 _cameraOffset = new Vec3(0, 3, 8);    // Offset da câmera relativo à nave
        private float _rotationSpeed = 0.01f;

        public ContentPreview3D(
            ILogger<ContentPreview3D> logger, 
            IGameObjectCollection gameObjects,
            IRenderer renderer,
            ICamera camera
        )
        {
           _logger = logger ?? throw new ArgumentNullException(nameof(logger));
           _gameObjects = gameObjects ?? throw new ArgumentNullException(nameof(gameObjects));
           _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
           _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        }

        public void SetModel(int modelId)
        {
            _currentModelId = modelId;
            _logger.LogInformation($"Setting 3D model for preview: {modelId}");
        }
        
        /// <summary>
        /// Configura a posição da nave no preview 3D
        /// </summary>
        public void SetShipPosition(float x, float y, float z)
        {
            _shipPosition = new Vec3(x, y, z);
            _logger.LogInformation($"Ship position set to: ({x}, {y}, {z})");
        }
        
        /// <summary>
        /// Configura o offset da câmera relativo à nave
        /// </summary>
        public void SetCameraOffset(float x, float y, float z)
        {
            _cameraOffset = new Vec3(x, y, z);
            _logger.LogInformation($"Camera offset set to: ({x}, {y}, {z})");
        }
        
        /// <summary>
        /// Configura a velocidade de rotação
        /// </summary>
        public void SetRotationSpeed(float speed)
        {
            _rotationSpeed = speed;
        }

        public void Render<T>(int modelId)
        {
            // Configurar câmera se ainda não foi feito
            if (!_cameraConfigured)
            {
                _camera.SetAspectRatio(1280f / 720f); // Aspect ratio padrão
                _camera.SetIsometricMode(false);
                _cameraConfigured = true;
                _logger.LogInformation("Camera configured for ContentPreview3D");
            }

            if (!_initialized)
            {
                Initialize(modelId);
            }

            // Se o modelo mudou, atualizar visibilidade das naves
            if (_currentModelId != modelId)
            {
                _logger.LogInformation("Changing preview model from {OldId} to {NewId}", _currentModelId, modelId);
                
                // Esconder a nave anterior
                var oldShip = _gameObjects.GetAll.Find(s => s.GameObjectId == _currentModelId);
                if (oldShip != null)
                {
                    oldShip.IsVisible = false;
                }
                
                // Mostrar a nova nave
                var newShip = _gameObjects.GetAll.Find(s => s.GameObjectId == modelId);
                if (newShip != null)
                {
                    newShip.IsVisible = true;
                    newShip.Position = _shipPosition;
                    newShip.Angle = new Vec3(0, 0, MathF.PI);
                }
                
                _currentModelId = modelId;
            }

            // Update rotation (da direita para esquerda)
            _rotationAngle += _rotationSpeed;
            if (_rotationAngle > MathF.PI * 2)
                _rotationAngle -= MathF.PI * 2;

            // Atualizar rotação da nave
            var ship = _gameObjects.GetAll.Find(s => s.GameObjectId == _currentModelId);
            if (ship != null)
            {
                // Rotação em Y para girar da direita para esquerda
                ship.Angle = new Vec3(0, _rotationAngle, MathF.PI);
                // Aplicar posição configurada
                ship.Position = _shipPosition;
            }

            // Render 3D ship (apenas a nave selecionada)
            if (ship == null || !ship.IsVisible)
                return;

            try
            {
                // Limpar apenas o depth buffer (manter o color buffer com o background)
                GL.Clear(ClearBufferMask.DepthBufferBit);
                
                _renderer.SetPassthroughProjection(false);
                _renderer.SetProjectionMatrix(_camera.GetProjectionMatrix());
                _renderer.SetViewMatrix(_camera.GetViewMatrix());
                _renderer.SetModelMatrix(OpenTK.Mathematics.Matrix4.Identity);
                
                _renderer.SetDepthTest(true);
                _renderer.SetDepthWrite(true);
                _renderer.SetFaceCulling(true);

                _renderer.SetCurrentTexture(_renderer.WhiteTexture);
                
                // Render apenas a nave selecionada
                ship.Draw();
                
                // Render shadow da nave
                _renderer.SetBlending(true);
                ship.RenderShadow();
                _renderer.SetBlending(false);
                
                _renderer.SetDepthTest(false);
                _renderer.SetDepthWrite(false);
                _renderer.SetFaceCulling(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering 3D preview");
            }
        }

        private void Initialize(int modelId)
        {
            _logger.LogInformation("Initializing ContentPreview3D with model {ModelId}", modelId);
            
            // Inicializar ships se ainda não foi feito
            if (_gameObjects.GetAll.Count == 0)
            {
                _gameObjects.Init(null);
            }

            // Tornar INVISÍVEL todas as naves exceto a selecionada
            foreach (var s in _gameObjects.GetAll)
            {
                s.IsVisible = (s.GameObjectId == modelId);
            }

            // Encontrar e configurar a nave desejada
            var ship = _gameObjects.GetAll.Find(s => s.GameObjectId == modelId);
            if (ship != null)
            {
                ship.Position = _shipPosition;
                ship.Angle = new Vec3(0, 0, MathF.PI);
                _logger.LogInformation("Ship {ModelId} configured for preview at position {Pos}", modelId, ship.Position);
            }

            _initialized = true;
            _currentModelId = modelId;
        }
    }

}
