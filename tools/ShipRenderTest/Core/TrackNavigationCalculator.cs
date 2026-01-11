using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Tools.Core;

/// <summary>
/// Calculates camera position and rotation for fly-through navigation along a track.
/// Handles interpolation between track sections and collision avoidance.
/// </summary>
public class TrackNavigationCalculator : ITrackNavigationCalculator
{
    #region constants
    private const int ARC_LENGTH_DIVISIONS = 20000;

    // Match C rewrite vertical FOV, closer cockpit view
    private const float CAMERA_DAMPING = 0.90f;

    // Y offset above track (slightly higher to look down more)
    private const float DEFAULT_CAMERA_DISTANCE = 0f;

    // Base Y height of track in PSX coordinates

    // Navigation parameters (ported from Wipeout.js fly-through)
    private const float DEFAULT_CAMERA_HEIGHT = 600f;

    // Camera sits on the spline path (no backward offset)
    private const float DEFAULT_FOV = 84.0f;

    // Scale applied when returning positions
    private const float FORWARD_DAMPING = 0.90f;

    // Smooth look-at target (increased for smoother curves: 0.95)
    private const float LOOK_AHEAD_MS = 800f;

    // Smooth camera movement (increased for smoother curves: 0.95)
    private const float LOOK_AT_DAMPING = 0.90f;

    // Damp forward vector to limit yaw/pitch changes
    private const float MAX_ROLL_RAD = (8f * MathF.PI) / 180f;

    // Look-ahead time offset in ms
    private const float POINT_DURATION_MS = 100f;

    // Roll contribution weight (very low to avoid aggressive banking)
    private const float PSX_TO_WORLD_SCALE = 0.001f;

    // Hermite bias (JS uses 0.0)
    private const float ROLL_DAMPING = 0.95f;

    // Roll smoothing factor (very high to smooth out rapid changes)
    private const float ROLL_WEIGHT = 0.10f;

    // Hermite tension (JS uses 0.5)
    private const float SPLINE_BIAS = 0.0f;

    // Each section contributes ~100ms in JS
    private const float SPLINE_TENSION = 0.5f;

    #endregion 

    #region fields
    private readonly List<float> _arcLengthTable = new();
    private readonly List<Vector3> _cameraPoints = new();
    private float _currentRoll = 0f;
    private Vector3 _dampedLookAt = Vector3.Zero;
    private Vector3 _dampedPosition = Vector3.Zero;
    private bool _hasDampedState = false;
    private readonly ILogger<TrackNavigationCalculator> _logger;
    private Vector3 _smoothedForward = Vector3.UnitZ;
    private float _totalArcLength = 0f;
    private readonly ITrack _track;
    private float _trackBaseHeight = 0f;
    private List<NavigationWaypoint> _waypoints = new();
    #endregion 

    public TrackNavigationCalculator(ITrack track, ILogger<TrackNavigationCalculator> logger)
    {
        _track = track ?? throw new ArgumentNullException(nameof(track));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        BuildNavigationWaypoints();
    }

    #region methods

    /// <summary>
    /// Returns the loop time in seconds based on the number of camera points and POINT_DURATION_MS.
    /// Mirrors wipeout.js logic (points.length * 100ms).
    /// </summary>
    public float GetLoopTimeSeconds()
    {
        return (_cameraPoints.Count * POINT_DURATION_MS) / 1000f;
    }

    /// <summary>
    /// Gets navigation data at a specific progress point along the track.
    /// Progress: 0.0 = start (Section 0), 1.0 = end of track
    /// </summary>
    public NavigationData GetNavigationData(float progress)
    {
        progress = Math.Clamp(progress, 0f, 1f);

        if (_cameraPoints.Count < 2)
        {
            return new NavigationData
            {
                Position = Vector3.Zero,
                ForwardDirection = Vector3.UnitZ,
                Target = Vector3.UnitZ,
                Yaw = 0f,
                Pitch = 0f,
                Roll = 0f,
                Distance = DEFAULT_CAMERA_DISTANCE,
                Fov = DEFAULT_FOV
            };
        }

        // Evaluate camera position along Hermite spline (arc-length reparameterized)
        var splinePos = EvaluateSplinePointArcLength(_cameraPoints, progress, SPLINE_TENSION, SPLINE_BIAS, _arcLengthTable, _totalArcLength);

        // Evaluate look-ahead point (time + LOOK_AHEAD_MS)
        float loopTimeMs = _cameraPoints.Count * POINT_DURATION_MS;
        float lookAheadT = progress + (LOOK_AHEAD_MS / Math.Max(loopTimeMs, 1f));
        lookAheadT = lookAheadT % 1.0f;
        var splineLookAt = EvaluateSplinePointArcLength(_cameraPoints, lookAheadT, SPLINE_TENSION, SPLINE_BIAS, _arcLengthTable, _totalArcLength);

        // Apply cockpit height to camera position only.
        // Note: Track data here uses PSX orientation; to place the camera above the spline,
        // use negative offset in our current coordinate setup.
        splinePos.Y -= DEFAULT_CAMERA_HEIGHT;

        // Initialize damping state on first call
        if (!_hasDampedState)
        {
            _dampedPosition = splinePos;
            _dampedLookAt = splineLookAt;
            _hasDampedState = true;
        }

        // Damped position/target (JS style: pos = pos*d + new*(1-d))
        _dampedPosition = _dampedPosition * CAMERA_DAMPING + splinePos * (1f - CAMERA_DAMPING);
        _dampedLookAt = _dampedLookAt * LOOK_AT_DAMPING + splineLookAt * (1f - LOOK_AT_DAMPING);

        // Forward direction
        var forwardDirection = (_dampedLookAt - _dampedPosition).Normalized();
        // Smooth forward to avoid abrupt yaw/pitch changes
        _smoothedForward = (_smoothedForward * FORWARD_DAMPING + forwardDirection * (1f - FORWARD_DAMPING)).Normalized();

        // Yaw/pitch from forward vector
        float calculatedYaw = MathF.Atan2(_smoothedForward.X, _smoothedForward.Z);
        float calculatedPitch = MathF.Asin(Math.Clamp(_smoothedForward.Y, -1f, 1f));

        // Roll based on heading delta (approximate JS roll logic)
        var cn = splinePos - _dampedPosition;
        var tn = splineLookAt - _dampedLookAt;
        float rollInput = MathF.Atan2(cn.Z, cn.X) - MathF.Atan2(tn.Z, tn.X);
        // Wrap to [-pi, pi]
        if (rollInput > MathF.PI) rollInput -= MathF.PI * 2f;
        if (rollInput < -MathF.PI) rollInput += MathF.PI * 2f;
        _currentRoll = _currentRoll * ROLL_DAMPING + rollInput * ROLL_WEIGHT;
        // Clamp roll to small angle to keep banking subtle
        _currentRoll = Math.Clamp(_currentRoll, -MAX_ROLL_RAD, MAX_ROLL_RAD);

        // Convert to world coordinates for rendering
        var worldCameraPos = _dampedPosition * PSX_TO_WORLD_SCALE;
        var worldTargetPos = _dampedLookAt * PSX_TO_WORLD_SCALE;
        // Use smoothed forward to orient target and avoid sharp rotations
        float worldDistance = (worldTargetPos - worldCameraPos).Length;
        worldTargetPos = worldCameraPos + _smoothedForward * worldDistance;

        return new NavigationData
        {
            Position = worldCameraPos,
            ForwardDirection = _smoothedForward,
            Target = worldTargetPos,
            Yaw = calculatedYaw,
            Pitch = calculatedPitch,
            Roll = _currentRoll,
            Distance = (worldTargetPos - worldCameraPos).Length,
            Fov = DEFAULT_FOV
        };
    }

    /// <summary>
    /// Calculates progress value based on track section index.
    /// </summary>
    public float GetProgressFromSection(int sectionIndex)
    {
        if (_waypoints.Count == 0)
            return 0f;

        return Math.Clamp((float)sectionIndex / (_waypoints.Count - 1), 0f, 1f);
    }

    /// <summary>
    /// Recommended base playback speed (progress per second) so that 1x completes a lap in loop time.
    /// </summary>
    public float GetRecommendedBaseSpeed()
    {
        var seconds = GetLoopTimeSeconds();
        if (seconds <= 0f) return 0f;
        return 1f / seconds;
    }

    /// <summary>
    /// Gets total number of sections in the track.
    /// </summary>
    public int GetSectionCount() => _track.Sections.Count;

    /// <summary>
    /// Gets the navigation data for the starting position.
    /// Starts from the beginning (0%) since we now skip the pit lane (first 10%) in section generation.
    /// </summary>
    public NavigationData GetStartingPosition()
    {
        // Start from the beginning - the pit lane is already skipped in GenerateTrackSections
        return GetNavigationData(0.0f);
    }

    /// <summary>
    /// Gets the track this calculator is using.
    /// </summary>
    public ITrack GetTrack() => _track;

    /// <summary>
    /// Gets total waypoints count.
    /// </summary>
    public int GetWaypointCount() => _waypoints.Count;

    /// <summary>
    /// Gets all waypoints for debug visualization.
    /// </summary>
    public IReadOnlyList<NavigationWaypoint> GetWaypoints() => _waypoints.AsReadOnly();

    /// <summary>
    /// Recalculates navigation waypoints after track data changes.
    /// </summary>
    public void RefreshWaypoints()
    {
        BuildNavigationWaypoints();
        _logger.LogInformation("[NAVIGATION] Waypoints refreshed");
    }

    private void BuildArcLengthTable()
    {
        _arcLengthTable.Clear();

        if (_cameraPoints.Count < 2)
        {
            _totalArcLength = 0f;
            return;
        }

        _arcLengthTable.Capacity = ARC_LENGTH_DIVISIONS + 1;
        _arcLengthTable.Add(0f);

        float total = 0f;
        var prev = EvaluateSplinePointUniform(_cameraPoints, 0f, SPLINE_TENSION, SPLINE_BIAS);

        for (int i = 1; i <= ARC_LENGTH_DIVISIONS; i++)
        {
            float u = (float)i / ARC_LENGTH_DIVISIONS;
            var p = EvaluateSplinePointUniform(_cameraPoints, u, SPLINE_TENSION, SPLINE_BIAS);
            total += (p - prev).Length;
            _arcLengthTable.Add(total);
            prev = p;
        }

        _totalArcLength = total;

        if (_totalArcLength > 0f)
        {
            for (int i = 0; i < _arcLengthTable.Count; i++)
            {
                _arcLengthTable[i] /= _totalArcLength;
            }
        }
    }

    /// <summary>
    /// Builds navigation waypoints from track sections for smooth interpolation.
    /// Uses Prev/Next pointers instead of array order, since sections can be out of sequence (junctions).
    /// </summary>
    private void BuildNavigationWaypoints()
    {
        _waypoints.Clear();
        _cameraPoints.Clear();
        _hasDampedState = false;
        _currentRoll = 0f;
        _smoothedForward = Vector3.UnitZ;
        _arcLengthTable.Clear();
        _totalArcLength = 0f;

        if (_track.Sections.Count == 0)
        {
            _logger.LogWarning("[NAVIGATION] Track has no sections");
            return;
        }

        const int FLAG_JUMP = 1;
        const int FLAG_JUNCTION_START = 16;

        var jumpIndices = new List<int>();

        void AddPoint(TrackSection section)
        {
            var pos = new Vector3(section.Center.X, section.Center.Y, section.Center.Z);
            _cameraPoints.Add(pos);
            if ((section.Flags & FLAG_JUMP) != 0)
            {
                jumpIndices.Add(_cameraPoints.Count - 1);
            }
        }

        // Pass 1: follow main path (skip junctions)
        var current = _track.Sections[0];
        int visited = 0;
        while (current != null && visited < _track.Sections.Count)
        {
            AddPoint(current);
            current = current.Next;
            visited++;
        }

        // Pass 2: follow junctions when they start
        current = _track.Sections[0];
        visited = 0;
        while (current != null && visited < _track.Sections.Count)
        {
            AddPoint(current);
            if (current.Junction != null && (current.Junction.Flags & FLAG_JUNCTION_START) != 0)
            {
                current = current.Junction;
            }
            else
            {
                current = current.Next;
            }
            visited++;
        }

        if (_cameraPoints.Count < 2)
        {
            _logger.LogWarning("[NAVIGATION] Not enough camera points to build spline");
            return;
        }

        // Calculate base height of track (average Y of first few sections)
        float sumY = 0f;
        int sampleCount = Math.Min(10, _cameraPoints.Count);
        for (int i = 0; i < sampleCount; i++)
        {
            sumY += _cameraPoints[i].Y;
        }
        _trackBaseHeight = sumY / sampleCount;
        _logger.LogInformation($"[NAVIGATION] Track base height: {_trackBaseHeight} PSX ({_trackBaseHeight * PSX_TO_WORLD_SCALE:F3} world)");

        // Extend tangents around jumps to smooth airborne sections
        for (int i = 0; i < jumpIndices.Count; i++)
        {
            int idx = jumpIndices[i];
            if (_cameraPoints.Count < 3)
                break;

            var prev = _cameraPoints[(idx - 1 + _cameraPoints.Count) % _cameraPoints.Count];
            var curr = _cameraPoints[idx];
            var next = _cameraPoints[(idx + 1) % _cameraPoints.Count];

            var tangent = curr - prev;
            float lengthNext = (next - curr).Length;
            if (tangent.LengthSquared > 0 && lengthNext > 0)
            {
                tangent = tangent.Normalized() * (lengthNext * 0.25f);
                _cameraPoints[idx] = curr + tangent;
            }
        }

        // Populate waypoints list (used by debug renderer)
        foreach (var point in _cameraPoints)
        {
            _waypoints.Add(new NavigationWaypoint
            {
                Position = point,
                LookAtPoint = point,
                Yaw = 0f,
                Pitch = 0f,
                Distance = DEFAULT_CAMERA_DISTANCE,
                Fov = DEFAULT_FOV,
                FloorY = point.Y
            });
        }

        BuildArcLengthTable();
        _logger.LogInformation("[NAVIGATION] Built {PointCount} camera points (two-pass with junctions)", _cameraPoints.Count);
    }

    private static Vector3 EvaluateSplinePointArcLength(
        IReadOnlyList<Vector3> points,
        float t,
        float tension,
        float bias,
        IReadOnlyList<float> arcTable,
        float totalArcLength)
    {
        if (arcTable.Count == 0 || totalArcLength <= 0f)
        {
            return EvaluateSplinePointUniform(points, t, tension, bias);
        }

        t = Math.Clamp(t, 0f, 1f);

        int high = arcTable.Count - 1;
        int low = 0;

        while (high - low > 1)
        {
            int mid = (high + low) >> 1;
            if (arcTable[mid] < t)
            {
                low = mid;
            }
            else
            {
                high = mid;
            }
        }

        float l0 = arcTable[low];
        float l1 = arcTable[high];
        float u0 = (float)low / ARC_LENGTH_DIVISIONS;
        float u1 = (float)high / ARC_LENGTH_DIVISIONS;

        float span = l1 - l0;
        float alpha = span > 1e-6f ? (t - l0) / span : 0f;
        float u = u0 + (u1 - u0) * alpha;

        return EvaluateSplinePointUniform(points, u, tension, bias);
    }

    private static Vector3 EvaluateSplinePointUniform(IReadOnlyList<Vector3> points, float t, float tension, float bias)
    {
        t = Math.Clamp(t, 0f, 1f);

        float point = (points.Count - 1) * t;
        int intPoint = Math.Clamp((int)Math.Floor(point), 0, points.Count - 1);
        float weight = point - intPoint;

        Vector3 p0 = points[intPoint == 0 ? intPoint : intPoint - 1];
        Vector3 p1 = points[intPoint];
        Vector3 p2 = points[intPoint >= points.Count - 2 ? points.Count - 1 : intPoint + 1];
        Vector3 p3 = points[intPoint >= points.Count - 3 ? points.Count - 1 : intPoint + 2];

        return new Vector3(
            HermiteInterpolate(p0.X, p1.X, p2.X, p3.X, weight, tension, bias),
            HermiteInterpolate(p0.Y, p1.Y, p2.Y, p3.Y, weight, tension, bias),
            HermiteInterpolate(p0.Z, p1.Z, p2.Z, p3.Z, weight, tension, bias)
        );
    }

    private static float HermiteInterpolate(float p0, float p1, float p2, float p3, float t, float tension, float bias)
    {
        float m0 = (p1 - p0) * (1 + bias) * (1 - tension) / 2f + (p2 - p1) * (1 - bias) * (1 - tension) / 2f;
        float m1 = (p2 - p1) * (1 + bias) * (1 - tension) / 2f + (p3 - p2) * (1 - bias) * (1 - tension) / 2f;

        float t2 = t * t;
        float t3 = t2 * t;

        float h0 = 2 * t3 - 3 * t2 + 1;
        float h1 = t3 - 2 * t2 + t;
        float h2 = t3 - t2;
        float h3 = -2 * t3 + 3 * t2;

        return h0 * p1 + h1 * m0 + h2 * m1 + h3 * p2;
    }

    #endregion

    public struct NavigationData
    {
        public float Distance { get; set; }
        public Vector3 ForwardDirection { get; set; }
        public float Fov { get; set; }
        public float Pitch { get; set; }
        public Vector3 Position { get; set; }
        public float Roll { get; set; }
        public Vector3 Target { get; set; }
        public float Yaw { get; set; }
    }

    // Limit banking to ±8 degrees
    public struct NavigationWaypoint
    {
        // Rotation around X axis (radians)
        public float Distance { get; set; }

        // Field of view
        public float FloorY { get; set; }

        // Track floor height at this point

        // Distance from target
        public float Fov { get; set; }

        // Camera position
        public Vector3 LookAtPoint { get; set; }

        // Rotation around Y axis (radians)
        public float Pitch { get; set; }

        public Vector3 Position { get; set; }

        // Target point to look at
        public float Yaw { get; set; }
    }
}