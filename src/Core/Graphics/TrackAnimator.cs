using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Graphics;

namespace WipeoutRewrite.Core.Graphics;

/// <summary>
/// Handles animation of track face colors (pickups, boost zones, etc).
/// Updates colors based on face flags and elapsed time.
/// </summary>
public class TrackAnimator
{
    private readonly ILogger<TrackAnimator> _logger;
    private float _totalTime = 0f;
    
    // Track face flags from wipeout-rewrite (exact bit positions)
    private const byte FACE_TRACK = 1 << 0;             // 0x01 - Base track surface
    private const byte FACE_PICKUP_LEFT = 1 << 1;       // 0x02 - Pickup on left side
    private const byte FACE_FLIP_TEXTURE = 1 << 2;      // 0x04 - Flip texture
    private const byte FACE_PICKUP_RIGHT = 1 << 3;      // 0x08 - Pickup on right side
    private const byte FACE_START_GRID = 1 << 4;        // 0x10 - Starting grid
    private const byte FACE_BOOST = 1 << 5;             // 0x20 - Boost pad
    private const byte FACE_PICKUP_COLLECTED = 1 << 6;  // 0x40 - Pickup collected
    private const byte FACE_PICKUP_ACTIVE = 1 << 7;     // 0x80 - Pickup active

    /// <summary>
    /// Data structure for animated faces
    /// </summary>
    private class AnimatedFace
    {
        public int PrimitiveIndex0 { get; set; }  // First triangle primitive index
        public int PrimitiveIndex1 { get; set; }  // Second triangle primitive index
        public byte Flags { get; set; }
        public (byte R, byte G, byte B, byte A) BaseColor { get; set; }
        public int FaceIndex { get; set; }
    }

    private List<AnimatedFace> _animatedFaces = new();
    private Mesh? _trackMesh;

    public TrackAnimator(ILogger<TrackAnimator>? logger = null)
    {
        _logger = logger ?? LoggerFactory.Create(b => {}).CreateLogger<TrackAnimator>();
    }

    /// <summary>
    /// Register animated faces from track mesh.
    /// Must be called after mesh is created but before animation starts.
    /// </summary>
    public void RegisterAnimatedFaces(Mesh trackMesh, List<TrackLoader.TrackFace> originalFaces)
    {
        _trackMesh = trackMesh;
        _animatedFaces.Clear();

        int primitiveIndex = 0;
        int pickupCount = 0;
        int boostCount = 0;

        for (int faceIndex = 0; faceIndex < originalFaces.Count; faceIndex++)
        {
            var face = originalFaces[faceIndex];
            byte flags = face.Flags;

            // Check if this face is animated (pickup or boost zone)
            bool isPickup = (flags & (FACE_PICKUP_LEFT | FACE_PICKUP_RIGHT)) != 0;
            bool isBoost = (flags & FACE_BOOST) != 0;

            if (isPickup || isBoost)
            {
                // This face has 2 triangles (primitives)
                var animatedFace = new AnimatedFace
                {
                    PrimitiveIndex0 = primitiveIndex,
                    PrimitiveIndex1 = primitiveIndex + 1,
                    Flags = flags,
                    BaseColor = face.Color,
                    FaceIndex = faceIndex
                };

                _animatedFaces.Add(animatedFace);

                if (isPickup) pickupCount++;
                if (isBoost) boostCount++;

                _logger.LogInformation("[TRACK] Face {Index}: pickup={Pickup}, boost={Boost}, flags=0x{Flags:X2}, " +
                    "color=RGB({R},{G},{B}), primitives=[{P0},{P1}]",
                    faceIndex, isPickup, isBoost, flags, face.Color.R, face.Color.G, face.Color.B, 
                    primitiveIndex, primitiveIndex + 1);
            }

            // Each face creates 2 triangles
            primitiveIndex += 2;
        }

        _logger.LogInformation("[TRACK] Registered {Total} animated faces: {Pickups} pickups, {Boosts} boosts", 
            _animatedFaces.Count, pickupCount, boostCount);
    }

    /// <summary>
    /// Update animated face colors based on elapsed time.
    /// Call this every frame to animate pickup colors.
    /// </summary>
    public void Update(float deltaTime)
    {
        _totalTime += deltaTime;

        if (_trackMesh == null || _animatedFaces.Count == 0)
            return;

        foreach (var animFace in _animatedFaces)
        {
            var prim0 = _trackMesh.Primitives[animFace.PrimitiveIndex0] as FT3;
            var prim1 = _trackMesh.Primitives[animFace.PrimitiveIndex1] as FT3;

            if (prim0 == null || prim1 == null)
                continue;

            (byte r, byte g, byte b, byte a) newColor;

            // Pickup zones: animated RGB color cycling
            if ((animFace.Flags & (FACE_PICKUP_LEFT | FACE_PICKUP_RIGHT)) != 0)
            {
                // Calculate animated color based on time
                // Each pickup cycles through colors at different phases
                float phase = animFace.FaceIndex * 0.5f;  // Different phase for each pickup

                float r_f = MathF.Sin(1.5f * _totalTime + phase) * 127f + 128f;
                float g_f = MathF.Cos(1.5f * _totalTime + phase) * 127f + 128f;
                float b_f = MathF.Sin(-1.5f * _totalTime - phase) * 127f + 128f;

                newColor = (
                    (byte)Math.Clamp(r_f, 0, 255),
                    (byte)Math.Clamp(g_f, 0, 255),
                    (byte)Math.Clamp(b_f, 0, 255),
                    255  // Alpha always opaque
                );
            }
            // Boost zones: static blue color
            else if ((animFace.Flags & FACE_BOOST) != 0)
            {
                newColor = (0, 0, 255, 255);  // Blue
            }
            else
            {
                continue;
            }

            // Apply new color to both triangles of this quad face
            prim0.Color = newColor;
            prim1.Color = newColor;
        }
    }

    /// <summary>
    /// Reset animation timer to start.
    /// </summary>
    public void Reset()
    {
        _totalTime = 0f;
    }
}
