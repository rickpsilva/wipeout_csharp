using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace WipeoutRewrite;

/// <summary>
/// Abstraction for keyboard and mouse input state.
/// Allows testing Camera without depending on OpenTK's KeyboardState and MouseState.
/// </summary>
public interface IInputState
{
    /// <summary>
    /// Check if a key is currently pressed.
    /// </summary>
    bool IsKeyDown(Keys key);

    /// <summary>
    /// Check if a mouse button is currently pressed.
    /// </summary>
    bool IsMouseButtonDown(MouseButton button);

    /// <summary>
    /// Get the current mouse position in screen coordinates.
    /// </summary>
    Vector2 MousePosition { get; }

    /// <summary>
    /// Get the mouse scroll wheel delta (positive = scroll up, negative = scroll down).
    /// </summary>
    Vector2 ScrollDelta { get; }
}

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
            // Limit pitch to approximately -89° to 89° (in radians: -1.553 to 1.553) to avoid gimbal lock
            //pitch = MathHelper.Clamp(value, MathHelper.DegreesToRadians(-89f), MathHelper.DegreesToRadians(89f));
            pitch = MathHelper.Clamp(value, MathHelper.DegreesToRadians(-180f), MathHelper.DegreesToRadians(180f));
            if (!_isFlythroughMode)
                UpdatePosition();
        }
    }

    public float Roll
    {
        get => roll;
        set
        {
            roll = value;
            // Roll only affects view matrix; no position update needed.
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
            // Don't call UpdatePosition here if we're in fly-through mode
            // The fly-through navigation sets both Position and Target explicitly
            if (!_isFlythroughMode)
            {
                UpdatePosition();
            }
        }
    }

    /// <summary>
    /// Enable/disable fly-through mode where Position and Target are set directly.
    /// When enabled, UpdatePosition() won't be called automatically.
    /// </summary>
    public bool IsFlythroughMode
    {
        get => _isFlythroughMode;
        set => _isFlythroughMode = value;
    }

    public float Yaw
    {
        get => yaw;
        set
        {
            // Normalize yaw to keep within 0 to 2π (0 to 360 degrees)
            yaw = value % (MathF.PI * 2);
            if (yaw < 0) yaw += MathF.PI * 2;
            if (!_isFlythroughMode)
                UpdatePosition();
        }
    }

    #endregion

    #region fields
    private readonly ILogger<Camera> _logger;
    private float aspectRatio;
    private float distance;
    private bool _isFlythroughMode = false;  // Flag to disable UpdatePosition() calls

    // NEAR_PLANE do C
    private float farClip = 2048576f;

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
    private float maxDistance = 10000f;
    private float maxFov = 120f;
    private float minDistance = 1f;  // Allow very close camera with 0.001f scale
    private float minFov = 5f;

    // FAR_PLANE do C (RENDER_FADEOUT_FAR)

    // Controles
    private float moveSpeed = 200f;

    private float nearClip = 0.01f;
    private float pitch = 0f;
    private float roll = 0f;

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
        // Use look-at so Position/Target define the view (matches JS fly-through expectation).
        var forward = target - position;
        if (forward.LengthSquared < 1e-6f)
            forward = Vector3.UnitZ;
        forward = forward.Normalized();

        // Build an orthonormal basis
        var upBase = -Vector3.UnitY; // flip up to match scene's orientation
        var right = Vector3.Cross(forward, upBase);
        if (right.LengthSquared < 1e-6f)
            right = Vector3.UnitX; // fallback if looking straight up/down
        else
            right = right.Normalized();

        var up = Vector3.Cross(right, forward);

        // Apply base orientation: original pipeline had a +π around Z; emulate via base roll
        float effectiveRoll = roll;

        // Apply roll by rotating right/up around forward (Rodrigues)
        if (MathF.Abs(effectiveRoll) > 1e-6f)
        {
            float cosR = MathF.Cos(effectiveRoll);
            float sinR = MathF.Sin(effectiveRoll);

            var rightRolled = right * cosR + up * sinR;
            var upRolled = -right * sinR + up * cosR;

            right = rightRolled;
            up = upRolled;

            _logger?.LogDebug(
                "[CAMERA ROLL] effectiveRoll={Roll:F4}rad({RollDeg:F2}°) | " +
                "right=({RX:F3},{RY:F3},{RZ:F3}) | up=({UX:F3},{UY:F3},{UZ:F3})",
                effectiveRoll, effectiveRoll * 180f / MathF.PI,
                rightRolled.X, rightRolled.Y, rightRolled.Z,
                upRolled.X, upRolled.Y, upRolled.Z);
        }

        // Build view matrix using the rolled up vector
        var viewMatrix = new Matrix4(
            right.X, up.X, -forward.X, 0,
            right.Y, up.Y, -forward.Y, 0,
            right.Z, up.Z, -forward.Z, 0,
            -Vector3.Dot(right, position), -Vector3.Dot(up, position), Vector3.Dot(forward, position), 1
        );

        return viewMatrix;
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
        roll = 0f;
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

    public void Update(IInputState input)
    {
        // Controles de movimento
        HandleKeyboardInput(input);

        // Controles de mouse (rotação e zoom)
        HandleMouseInput(input);
    }

    /// <summary>
    /// Overload for backward compatibility with OpenTK KeyboardState and MouseState.
    /// </summary>
    public void Update(KeyboardState keyboardState, MouseState mouseState)
    {
        Update(new InputStateAdapter(keyboardState, mouseState));
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

    private void HandleKeyboardInput(IInputState input)
    {
        Vector3 moveDirection = Vector3.Zero;

        // W/A/S/D - Movimento da câmera ao redor do alvo
        if (input.IsKeyDown(Keys.W))
        {
            _logger.LogInformation("[CAMERA +Y] W key DOWN");
            moveDirection += Vector3.UnitY;  // Frente
        }
        if (input.IsKeyDown(Keys.S))
        {
            _logger.LogInformation("[CAMERA Y] S key DOWN");
            moveDirection -= Vector3.UnitY;  // Trás
        }
        if (input.IsKeyDown(Keys.A))
        {
            _logger.LogInformation("[CAMERA -X] A key DOWN");
            moveDirection -= Vector3.UnitX;  // Esquerda
        }
        if (input.IsKeyDown(Keys.D))
        {
            _logger.LogInformation("[CAMERA] D key DOWN");
            moveDirection += Vector3.UnitX;  // Direita
        }
        if (input.IsKeyDown(Keys.Q))
        {
            _logger.LogInformation("[CAMERA -Z] Q key DOWN");
            moveDirection -= Vector3.UnitZ;  // Baixo
        }
        if (input.IsKeyDown(Keys.E))
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
        if (input.IsKeyDown(Keys.R))
        {
            _logger.LogInformation("[CAMERA] R key DOWN - resetting view");
            ResetView();
        }
    }

    private void HandleMouseInput(IInputState input)
    {
        // DEBUG: Log mouse state first time
        _logger.LogInformation("[CAMERA] MouseState: X={X}, Y={Y}, ScrollDelta={Scroll}",
            input.MousePosition.X, input.MousePosition.Y, input.ScrollDelta.Y);

        // Botão direito do mouse para rodar a câmera
        if (input.IsMouseButtonDown(MouseButton.Right))
        {
            _logger.LogInformation("[CAMERA] Right mouse button DOWN at ({X}, {Y})", input.MousePosition.X, input.MousePosition.Y);

            if (isRotating)
            {
                float deltaX = input.MousePosition.X - lastMousePos.X;
                float deltaY = input.MousePosition.Y - lastMousePos.Y;

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

            lastMousePos = new Vector2(input.MousePosition.X, input.MousePosition.Y);
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
        float scrollDelta = input.ScrollDelta.Y;
        if (scrollDelta != 0)
        {
            _logger.LogInformation("[CAMERA] Scroll delta: {ScrollDelta}", scrollDelta);
            Zoom(scrollDelta * zoomSpeed / 60f);
        }
    }

    private void UpdatePosition()
    {
        // Orbit-style positioning: compute forward from yaw/pitch and place camera
        // at 'distance' behind the target along that forward vector.
        float cy = MathF.Cos(pitch);
        float sy = MathF.Sin(pitch);
        float sx = MathF.Sin(yaw);
        float cx = MathF.Cos(yaw);

        var forward = new Vector3(sx * cy, sy, cx * cy);
        if (forward.LengthSquared > 1e-6f)
            forward = forward.Normalized();

        position = target - forward * distance;
        
        _logger.LogInformation("[CAMERA] UpdatePosition: pos={Position}, target={Target}, yaw={Yaw}rad, pitch={Pitch}rad, dist={Distance}",
            position, target, yaw, pitch, distance);
    }

    #endregion 
}

/// <summary>
/// Adapter that converts OpenTK KeyboardState and MouseState to IInputState.
/// Allows backward compatibility while using the abstraction internally.
/// </summary>
internal class InputStateAdapter : IInputState
{
    private readonly KeyboardState _keyboardState;
    private readonly MouseState _mouseState;

    public InputStateAdapter(KeyboardState keyboardState, MouseState mouseState)
    {
        _keyboardState = keyboardState;
        _mouseState = mouseState;
    }

    public bool IsKeyDown(Keys key) => _keyboardState.IsKeyDown(key);

    public bool IsMouseButtonDown(MouseButton button) => _mouseState.IsButtonDown(button);

    public Vector2 MousePosition => new(_mouseState.X, _mouseState.Y);

    public Vector2 ScrollDelta => _mouseState.ScrollDelta;
}