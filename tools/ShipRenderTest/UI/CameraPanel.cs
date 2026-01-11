using ImGuiNET;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using SysVector2 = System.Numerics.Vector2;
using SysVector3 = System.Numerics.Vector3;
using SysVector4 = System.Numerics.Vector4;
using WipeoutRewrite.Core.Entities;
using WipeoutRewrite.Tools.Core;
using WipeoutRewrite.Tools.Managers;

namespace WipeoutRewrite.Tools.UI;

/// <summary>
/// UI Panel for camera settings and control.
/// Follows Single Responsibility Principle - only handles camera UI.
/// </summary>
public class CameraPanel : ICameraPanel
{
    private const float DEFAULT_PLAYBACK_SPEED = 0.1f;

    public bool IsVisible
    {
        get => _isVisible;
        set => _isVisible = value;
    }

    #region fields
    private ITrack? _currentTrack;
    private bool _flyThroughMode = false;
    private bool _isPlaying = false;
    private bool _isVisible = true;
    private readonly ILogger<CameraPanel> _logger;

    // 0 = Section 0, 1 = end of track
    private ITrackNavigationCalculator? _navigationCalculator;

    // Track that the current calculator is bound to

    private readonly ITrackNavigationCalculatorFactory _navigationFactory;

    // Progress per second (0.1 = 10 seconds for full lap)
    private float _playbackSpeedMultiplier = 1.0f;

    private readonly IScene _scene;
    private float _trackProgress = 0f;
    #endregion 

    // Speed multiplier (0.1x, 0.2x, 0.5x, 1x, 2x, 4x)

    public CameraPanel(
        ILogger<CameraPanel> logger,
        IScene scene,
        ITrackNavigationCalculatorFactory navigationFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _navigationFactory = navigationFactory ?? throw new ArgumentNullException(nameof(navigationFactory));
    }

    public void Render()
    {
        if (!_isVisible) return;

        // Detect if track changed - if so, reset fly-through mode
        if (_currentTrack != _scene.ActiveTrack)
        {
            if (_flyThroughMode)
            {
                _flyThroughMode = false;
                _isPlaying = false;
                _navigationCalculator = null;
                _logger.LogInformation("[CAMERA] Track changed - fly-through mode disabled");
            }
            _currentTrack = _scene.ActiveTrack;
        }

        ImGui.SetNextWindowSize(new SysVector2(300, 300), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Camera", ref _isVisible))
        {
            // Use selected camera if available, otherwise use active camera
            var cameraToDisplay = _scene.SelectedCamera ?? _scene.CameraManager.ActiveCamera;
            if (cameraToDisplay != null)
            {
                // Update fly-through navigation if active
                UpdateFlyThroughNavigation(cameraToDisplay);

                RenderCameraSettings(cameraToDisplay);
            }
            else
            {
                ImGui.TextDisabled("No camera selected");
                ImGui.Text("Create or select a camera from the Scene panel");
            }
        }
        ImGui.End();
    }

    private void RenderCameraSettings(SceneCamera sceneCamera)
    {
        ImGui.Text($"Camera: {sceneCamera.Name}");

        bool isActive = _scene.CameraManager.ActiveCamera == sceneCamera;
        bool isSelected = _scene.SelectedCamera == sceneCamera;

        if (isActive)
        {
            ImGui.SameLine();
            ImGui.TextColored(new SysVector4(0.3f, 1.0f, 0.3f, 1.0f), "(Active)");
        }

        if (isSelected && !isActive)
        {
            ImGui.SameLine();
            ImGui.TextColored(new SysVector4(0.8f, 0.8f, 0.3f, 1.0f), "(Selected)");
        }

        ImGui.Separator();

        var camera = sceneCamera.Camera;
        var pos = camera.Position;

        // Fly-through mode toggle
        if (ImGui.Checkbox("Fly-through (Pilot Vision)", ref _flyThroughMode))
        {
            if (_flyThroughMode)
            {
                // Initialize fly-through mode
                _isPlaying = false;
                _trackProgress = 0f;

                // ALWAYS create a fresh calculator for the current track
                // This ensures we get the correct spline even if track was reloaded
                if (_scene.ActiveTrack != null)
                {
                    _navigationCalculator = _navigationFactory.Create(_scene.ActiveTrack);
                    _currentTrack = _scene.ActiveTrack; // Track the track that the calculator is bound to
                    _logger.LogInformation("[CAMERA] Created NEW fly-through calculator for track, sections: {SectionCount}",
                        _navigationCalculator.GetSectionCount());

                    // Position camera at starting position (section 0)
                    var startingData = _navigationCalculator.GetStartingPosition();
                    camera.Position = startingData.Position;
                    camera.Target = startingData.Target;
                    camera.Yaw = startingData.Yaw;
                    camera.Pitch = startingData.Pitch;
                    camera.Roll = startingData.Roll;
                    camera.Distance = startingData.Distance;
                    camera.Fov = startingData.Fov;

                    _logger.LogInformation("[CAMERA] Positioned at section 0:");
                    _logger.LogInformation("[CAMERA]   Position: ({PosX:F2}, {PosY:F2}, {PosZ:F2})",
                        startingData.Position.X, startingData.Position.Y, startingData.Position.Z);
                    _logger.LogInformation("[CAMERA]   Target: ({TgtX:F2}, {TgtY:F2}, {TgtZ:F2})",
                        startingData.Target.X, startingData.Target.Y, startingData.Target.Z);
                    _logger.LogInformation("[CAMERA]   Distance: {Distance:F2}, FOV: {Fov:F2}°, Yaw: {Yaw:F4}rad, Pitch: {Pitch:F4}rad",
                        startingData.Distance, startingData.Fov, startingData.Yaw, startingData.Pitch);
                }
                else
                {
                    _logger.LogWarning("[CAMERA] Fly-through enabled but no active track loaded");
                }

                _logger.LogInformation("[CAMERA] Fly-through mode enabled");
            }
            else
            {
                // Exit fly-through mode
                _isPlaying = false;
                _navigationCalculator = null;
                _currentTrack = null;
                _logger.LogInformation("[CAMERA] Fly-through mode disabled");
            }
        }

        // Always show camera position info
        ImGui.Separator();
        ImGui.Text("Camera Info:");
        ImGui.Text($"Position: ({pos.X:F2}, {pos.Y:F2}, {pos.Z:F2})");
        ImGui.Text($"Distance: {camera.Distance:F2}");
        ImGui.Text($"FOV: {camera.Fov:F2}°");
        ImGui.Text($"Yaw: {MathHelper.RadiansToDegrees(camera.Yaw):F2}°");
        ImGui.Text($"Pitch: {MathHelper.RadiansToDegrees(camera.Pitch):F2}°");

        // If fly-through mode is enabled, show play/pause/restart controls
        if (_flyThroughMode)
        {
            ImGui.Separator();
            ImGui.Text("Navigation Controls:");

            if (_isPlaying)
            {
                if (ImGui.Button("Pause##flythroughPause", new SysVector2(100, 0)))
                {
                    _isPlaying = false;
                    _logger.LogInformation("[CAMERA] Fly-through paused at progress {Progress:F2}", _trackProgress);
                }
            }
            else
            {
                if (ImGui.Button("Play##flythroughPlay", new SysVector2(100, 0)))
                {
                    _isPlaying = true;
                    _logger.LogInformation("[CAMERA] Fly-through started");
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Restart##flythroughRestart", new SysVector2(100, 0)))
            {
                _trackProgress = 0f;
                _isPlaying = false;
                _logger.LogInformation("[CAMERA] Fly-through restarted");
            }

            ImGui.ProgressBar(_trackProgress, new SysVector2(-1, 0), $"{(_trackProgress * 100):F1}%");

            ImGui.Separator();
            ImGui.Text("Playback Speed:");

            float buttonWidth = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X * 5) / 6;

            if (ImGui.Button("0.1x", new SysVector2(buttonWidth, 0)))
            {
                _playbackSpeedMultiplier = 0.1f;
                _logger.LogInformation("[CAMERA] Playback speed: 0.1x (very slow - 100s per lap)");
            }
            ImGui.SameLine();

            if (ImGui.Button("0.2x", new SysVector2(buttonWidth, 0)))
            {
                _playbackSpeedMultiplier = 0.2f;
                _logger.LogInformation("[CAMERA] Playback speed: 0.2x (slow - 50s per lap)");
            }
            ImGui.SameLine();

            if (ImGui.Button("0.5x", new SysVector2(buttonWidth, 0)))
            {
                _playbackSpeedMultiplier = 0.5f;
                _logger.LogInformation("[CAMERA] Playback speed: 0.5x (slower - 20s per lap)");
            }
            ImGui.SameLine();

            if (ImGui.Button("1x", new SysVector2(buttonWidth, 0)))
            {
                _playbackSpeedMultiplier = 1.0f;
                _logger.LogInformation("[CAMERA] Playback speed: 1x (normal - 10s per lap)");
            }
            ImGui.SameLine();

            if (ImGui.Button("2x", new SysVector2(buttonWidth, 0)))
            {
                _playbackSpeedMultiplier = 2.0f;
                _logger.LogInformation("[CAMERA] Playback speed: 2x (fast - 5s per lap)");
            }
            ImGui.SameLine();

            if (ImGui.Button("4x", new SysVector2(buttonWidth, 0)))
            {
                _playbackSpeedMultiplier = 4.0f;
                _logger.LogInformation("[CAMERA] Playback speed: 4x (very fast - 2.5s per lap)");
            }

            ImGui.Text($"Current speed: {_playbackSpeedMultiplier:F1}x");

            ImGui.Separator();
            ImGui.TextDisabled("(Other camera settings are disabled in Fly-through mode)");
        }
        else
        {
            // Normal camera controls (disabled in fly-through mode)
            ImGui.Separator();
            ImGui.Text("Orbit Settings:");

            // FOV (already in degrees in Camera.cs)
            float fovDeg = camera.Fov;
            if (ImGui.SliderFloat("FOV (degrees)", ref fovDeg, 1.0f, 120.0f))
            {
                camera.Fov = fovDeg;
            }

            float distance = camera.Distance;
            if (ImGui.DragFloat("Distance (units)", ref distance, 0.5f, 0.1f, 10000.0f))
            {
                camera.Distance = distance;
            }

            // Yaw and Pitch in degrees
            float yawDeg = MathHelper.RadiansToDegrees(camera.Yaw);
            float pitchDeg = MathHelper.RadiansToDegrees(camera.Pitch);

            if (ImGui.DragFloat("Yaw (degrees)", ref yawDeg, 1.0f))
            {
                camera.Yaw = MathHelper.DegreesToRadians(yawDeg);
            }

            if (ImGui.DragFloat("Pitch (degrees)", ref pitchDeg, 1.0f, -89.0f, 89.0f))
            {
                camera.Pitch = MathHelper.DegreesToRadians(pitchDeg);
            }

            // Look target
            var target = camera.Target;
            var targetVec = new SysVector3(target.X, target.Y, target.Z);
            if (ImGui.DragFloat3("Target", ref targetVec, 0.1f))
            {
                camera.Target = new Vector3(targetVec.X, targetVec.Y, targetVec.Z);
            }

            ImGui.Separator();

            // Quick actions
            if (ImGui.Button("Reset Camera"))
            {
                camera.Target = Vector3.Zero;
                camera.Yaw = 0;
                camera.Pitch = MathHelper.DegreesToRadians(-26.5f);
                camera.Distance = 30;
            }

            if (!isActive)
            {
                if (ImGui.Button("Set as Active Camera"))
                {
                    _scene.CameraManager.SetActiveCamera(sceneCamera.Name);
                    _logger.LogInformation("[CAMERA] Set {Name} as active", sceneCamera.Name);
                }
            }
        }
    }

    private void UpdateFlyThroughNavigation(SceneCamera sceneCamera)
    {
        var camera = sceneCamera.Camera;

        // CRITICAL: Validate that calculator is still valid for current track
        // This handles the case where track was changed/reloaded
        if (_navigationCalculator != null && _currentTrack != _scene.ActiveTrack)
        {
            _navigationCalculator = null;
            _currentTrack = null;
            _flyThroughMode = false;
            _isPlaying = false;
            _logger.LogWarning("[CAMERA] Track changed - invalidating navigation calculator");
            return;
        }

        // Update camera position during playback
        if (_flyThroughMode && _isPlaying && _navigationCalculator != null)
        {
            float deltaTime = ImGui.GetIO().DeltaTime;

            // Calculate actual playback speed with multiplier
            // Base speed: prefer calculator-recommended value (JS-equivalent), fallback to DEFAULT_PLAYBACK_SPEED
            float baseSpeed = DEFAULT_PLAYBACK_SPEED;
            if (_navigationCalculator != null)
            {
                float recommended = _navigationCalculator.GetRecommendedBaseSpeed();
                if (recommended > 0f)
                {
                    baseSpeed = recommended;
                }
            }
            float actualSpeed = baseSpeed * _playbackSpeedMultiplier;
            _trackProgress += deltaTime * actualSpeed;

            // Stop at end
            if (_trackProgress >= 1.0f)
            {
                _trackProgress = 1.0f;
                _isPlaying = false;
                _logger.LogInformation("[CAMERA] Fly-through completed");
            }

            // Get navigation data and update camera
            var navData = _navigationCalculator.GetNavigationData(_trackProgress);

            // IMPORTANT: In fly-through mode, set values directly without recalculations
            camera.Target = navData.Target;
            camera.Position = navData.Position;
            camera.Distance = navData.Distance;
            camera.Yaw = navData.Yaw;
            camera.Pitch = navData.Pitch;
            camera.Fov = navData.Fov;
            camera.Roll = navData.Roll;

            _logger.LogInformation("[CAMERA] Fly-through update: progress={Progress:F2}, pos={Position}, target={Target}",
                _trackProgress, navData.Position, navData.Target);
        }

        // Clear calculator and disable fly-through mode if disabled
        if (!_flyThroughMode || _scene.ActiveTrack == null)
        {
            camera.IsFlythroughMode = false;  // Disable fly-through mode
        }
    }
}