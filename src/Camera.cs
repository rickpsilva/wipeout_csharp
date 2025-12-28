namespace WipeoutRewrite;

public class Camera : ICamera
{
    #region properties

    // Public properties for editor/tool access
    public float Distance
    {
        get => distance;
        set
        {
            distance = System.Math.Clamp(value, minDistance, maxDistance);
            UpdatePosition();  // Update camera position when distance changes
        }
    }

    public float Fov
    {
        get => fov;
        set
        {
            fov = MathHelper.Clamp(value, minFov, maxFov);
        }
    }

    public float Pitch
    {
        get => pitch;
        set
        {
            // Limit pitch to approximately -89° to 89° (in radians: -1.553 to 1.553)
            pitch = MathHelper.Clamp(value, MathHelper.DegreesToRadians(-89f), MathHelper.DegreesToRadians(89f));
            UpdatePosition();
        }
    }

    // Mais perto para ver melhor
    public Vector3 Position
    {
        get => position;
        set
        {
            position = value;
            distance = (position - target).Length;
        }
    }

    public Vector3 Target
    {
        get => target;
        set
        {
            target = value;
            UpdatePosition();
        }
    }

    public float Yaw
    {
        get => yaw;
        set
        {
            // Normalize yaw to keep within 0 to 2π (0 to 360 degrees)
            yaw = value % (MathF.PI * 2);
            if (yaw < 0) yaw += MathF.PI * 2;
            UpdatePosition();
        }
    }

    #endregion

    #region fields
    private readonly ILogger<Camera> _logger;
    private float aspectRatio;
    private float distance;

    // NEAR_PLANE do C
    private float farClip = 64000f;

    // Configuração de câmera (baseado em wipeout-rewrite/src/render_gl.c)
    // Vertical FOV de 73.75° (horizontal 90° em 4:3)
    private float fov = 73.75f;

    // FOV vertical do Wipeout original
    private float initialFov;

    private Vector3 initialPosition;

    // Olhando para o centro
    private Vector3 initialTarget;

    private float isometricScale = 1.0f;

    // Escala do volume de visualização

    // Estado do mouse
    private bool isRotating = false;

    private Vector2 lastMousePos;
    private float maxDistance = 150f;
    private float maxFov = 120f;
    private float minDistance = 5f;
    private float minFov = 5f;

    // FAR_PLANE do C (RENDER_FADEOUT_FAR)

    // Controles
    private float moveSpeed = 200f;

    private float nearClip = 16.0f;
    private float pitch = 0f;

    // Posição e orientação
    private Vector3 position = new(0, 15, 30);

    // Aumentei de 50 para ser mais visível
    private float rotationSpeed = 2f;

    private Vector3 target = new(0, 0, 0);
    private Vector3 up = Vector3.UnitY;

    // Modo isométrico
    private bool useIsometricProjection = false;

    private float yaw = 0f;
    private float zoomSpeed = 5f;
    #endregion

    public Camera(ILogger<Camera> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.initialTarget = this.target;
        this.initialPosition = this.position;
        this.initialFov = this.fov;
        // Calcular distância inicial
        this.distance = Vector3.Distance(position, target);
        _logger.LogInformation("Camera initialized at position {0} targeting {1}", position, target);
    }

    #region methods

    public Matrix4 GetProjectionMatrix()
    {
        if (useIsometricProjection)
        {
            // Projeção ortográfica isométrica
            float width = 1280 * isometricScale;  // Assumindo 1280x720
            float height = 720 * isometricScale;
            float left = -width / 2;
            float right = width / 2;
            float bottom = -height / 2;
            float top = height / 2;
            return Matrix4.CreateOrthographic(
                width,
                height,
                nearClip,
                farClip
            );
        }
        else
        {
            // Perspectiva padrão (3D)
            return Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(fov),
                aspectRatio,
                nearClip,
                farClip
            );
        }
    }

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(position, target, up);
    }

    public void Move(Vector3 direction)
    {
        _logger.LogInformation("[CAMERA] Move() called with direction: {Direction}", direction);
        target += direction;
        _logger.LogInformation("[CAMERA] Target now at: {Target}, Position: {Position}", target, position);
        UpdatePosition();
    }

    public void ResetView()
    {
        position = initialPosition;
        target = initialTarget;
        fov = initialFov;
        yaw = 0f;
        pitch = 0f;
        distance = Vector3.Distance(initialPosition, initialTarget);
        _logger.LogInformation("[CAMERA] View reset: pos={Position}, target={Target}, distance={Distance}", position, target, distance);
        UpdatePosition();
    }

    public void Rotate(float yaw, float pitch)
    {
        _logger.LogInformation("[CAMERA] Rotate called: yaw={Yaw}, pitch={Pitch}", yaw, pitch);
        this.yaw += yaw * 0.01f;
        this.pitch = MathHelper.Clamp(this.pitch + pitch * 0.01f, -89f, 89f);
        _logger.LogInformation("[CAMERA] New rotation: yaw={NewYaw}, pitch={NewPitch}", this.yaw, this.pitch);
        UpdatePosition();
    }

    public void SetAspectRatio(float aspectRatio)
    {
        _logger.LogInformation("[CAMERA] SetAspectRatio called: {AspectRatio}", aspectRatio);
        this.aspectRatio = aspectRatio;
        _logger.LogInformation("[CAMERA] Position: {Position}, Target: {Target}, Distance: {Distance}, FOV: {FOV}",
            position, target, distance, fov);
    }

    public void SetIsometricMode(bool useIsometric, float scale = 1.0f)
    {
        useIsometricProjection = useIsometric;
        isometricScale = scale;
        _logger.LogInformation("[CAMERA] Isometric mode: {Mode}, scale: {Scale}", useIsometric, scale);
    }

    public void Update(KeyboardState keyboardState, MouseState mouseState)
    {
        // Controles de movimento
        HandleKeyboardInput(keyboardState);

        // Controles de mouse (rotação e zoom)
        HandleMouseInput(mouseState);
    }

    public void Zoom(float delta)
    {
        if (useIsometricProjection)
        {
            // Em modo isométrico: zoom controla a escala (field of view ortográfico)
            isometricScale = MathHelper.Clamp(isometricScale + delta * 0.1f, 0.5f, 3.0f);
            _logger.LogInformation("[CAMERA] Isometric zoom: scale={Scale}", isometricScale);
        }
        else
        {
            // Em modo perspectiva: zoom controla a distância
            distance = MathHelper.Clamp(distance - delta, minDistance, maxDistance);
            UpdatePosition();
        }
    }

    private void HandleKeyboardInput(KeyboardState keyboard)
    {
        Vector3 moveDirection = Vector3.Zero;

        // W/A/S/D - Movimento da câmera ao redor do alvo
        if (keyboard.IsKeyDown(Keys.W))
        {
            _logger.LogInformation("[CAMERA +Y] W key DOWN");
            moveDirection += Vector3.UnitY;  // Frente
        }
        if (keyboard.IsKeyDown(Keys.S))
        {
            _logger.LogInformation("[CAMERA Y] S key DOWN");
            moveDirection -= Vector3.UnitY;  // Trás
        }
        if (keyboard.IsKeyDown(Keys.A))
        {
            _logger.LogInformation("[CAMERA -X] A key DOWN");
            moveDirection -= Vector3.UnitX;  // Esquerda
        }
        if (keyboard.IsKeyDown(Keys.D))
        {
            _logger.LogInformation("[CAMERA] D key DOWN");
            moveDirection += Vector3.UnitX;  // Direita
        }
        if (keyboard.IsKeyDown(Keys.Q))
        {
            _logger.LogInformation("[CAMERA -Z] Q key DOWN");
            moveDirection -= Vector3.UnitZ;  // Baixo
        }
        if (keyboard.IsKeyDown(Keys.E))
        {
            _logger.LogInformation("[CAMERA +Z] E key DOWN");
            moveDirection += Vector3.UnitZ;  // Cima
        }

        if (moveDirection != Vector3.Zero)
        {
            _logger.LogInformation("[CAMERA] Moving with direction: {Direction}", moveDirection);
            Vector3 normalizedDir = Vector3.Normalize(moveDirection);
            float movementAmount = moveSpeed / 60f;
            Vector3 finalMovement = normalizedDir * movementAmount;
            _logger.LogInformation("[CAMERA] Normalized: {Norm}, Amount: {Amt}, Final: {Final}", normalizedDir, movementAmount, finalMovement);
            Move(finalMovement);
        }

        // R - Reset à vista inicial
        if (keyboard.IsKeyDown(Keys.R))
        {
            _logger.LogInformation("[CAMERA] R key DOWN - resetting view");
            ResetView();
        }
    }

    private void HandleMouseInput(MouseState mouse)
    {
        // DEBUG: Log mouse state first time
        _logger.LogInformation("[CAMERA] MouseState: X={X}, Y={Y}, ScrollDelta={Scroll}",
            mouse.X, mouse.Y, mouse.ScrollDelta.Y);

        // Botão direito do mouse para rodar a câmera
        if (mouse.IsButtonDown(MouseButton.Right))
        {
            _logger.LogInformation("[CAMERA] Right mouse button DOWN at ({X}, {Y})", mouse.X, mouse.Y);

            if (isRotating)
            {
                float deltaX = mouse.X - lastMousePos.X;
                float deltaY = mouse.Y - lastMousePos.Y;

                if (deltaX != 0 || deltaY != 0)
                {
                    _logger.LogInformation("[CAMERA] Mouse delta: ({DeltaX}, {DeltaY})", deltaX, deltaY);
                    Rotate(deltaX * rotationSpeed, -deltaY * rotationSpeed);
                }
            }
            else
            {
                _logger.LogInformation("[CAMERA] Starting rotation tracking");
                isRotating = true;
            }

            lastMousePos = new Vector2(mouse.X, mouse.Y);
        }
        else
        {
            if (isRotating)
            {
                _logger.LogInformation("[CAMERA] Right mouse button released");
            }
            isRotating = false;
        }

        // Scroll do mouse para zoom
        float scrollDelta = mouse.ScrollDelta.Y;
        if (scrollDelta != 0)
        {
            _logger.LogInformation("[CAMERA] Scroll delta: {ScrollDelta}", scrollDelta);
            Zoom(scrollDelta * zoomSpeed / 60f);
        }
    }

    private void UpdatePosition()
    {
        // yaw and pitch are already in radians, no need to convert

        // Calculate new position around the target
        float x = distance * MathF.Cos(pitch) * MathF.Sin(yaw);
        float y = distance * MathF.Sin(pitch);
        float z = distance * MathF.Cos(pitch) * MathF.Cos(yaw);

        position = target + new Vector3(x, y, z);
        _logger.LogInformation("[CAMERA] UpdatePosition: pos={Position}, target={Target}, yaw={Yaw}rad, pitch={Pitch}rad, dist={Distance}",
            position, target, yaw, pitch, distance);
    }

    #endregion 
}