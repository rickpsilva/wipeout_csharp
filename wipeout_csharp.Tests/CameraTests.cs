using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Moq;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for Camera class.
/// Tests camera positioning, rotation, zoom, projection matrices, and input handling.
/// </summary>
public class CameraTests
{
    private readonly ILogger<Camera> _logger;
    private readonly Camera _camera;

    public CameraTests()
    {
        _logger = NullLogger<Camera>.Instance;
        _camera = new Camera(_logger);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidLogger_InitializesCamera()
    {
        var camera = new Camera(_logger);

        Assert.NotNull(camera);
        Assert.NotEqual(Vector3.Zero, camera.Position);
        Assert.Equal(Vector3.Zero, camera.Target);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Camera(null!));
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Distance_SetValue_ClampsToValidRange()
    {
        _camera.Distance = 50f;
        Assert.Equal(50f, _camera.Distance);

        // Test minimum clamp
        _camera.Distance = -100f;
        Assert.True(_camera.Distance >= 1f);

        // Test maximum clamp
        _camera.Distance = 999999f;
        Assert.True(_camera.Distance <= 10000f);
    }

    [Fact]
    public void Fov_SetValue_ClampsToValidRange()
    {
        _camera.Fov = 60f;
        Assert.Equal(60f, _camera.Fov);

        // Test minimum clamp
        _camera.Fov = 1f;
        Assert.True(_camera.Fov >= 5f);

        // Test maximum clamp
        _camera.Fov = 200f;
        Assert.True(_camera.Fov <= 120f);
    }

    [Fact]
    public void Pitch_SetValue_ClampsToValidRange()
    {
        _camera.Pitch = 0.5f;
        Assert.Equal(0.5f, _camera.Pitch);

        // Test clamps to approximately ±180 degrees in radians
        _camera.Pitch = 10f;
        Assert.True(_camera.Pitch <= MathHelper.DegreesToRadians(180f));

        _camera.Pitch = -10f;
        Assert.True(_camera.Pitch >= MathHelper.DegreesToRadians(-180f));
    }

    [Fact]
    public void Roll_SetAndGet_WorksCorrectly()
    {
        _camera.Roll = 0.5f;
        Assert.Equal(0.5f, _camera.Roll);

        _camera.Roll = -0.5f;
        Assert.Equal(-0.5f, _camera.Roll);
    }

    [Fact]
    public void Position_SetValue_UpdatesDistanceAutomatically()
    {
        var newPosition = new Vector3(10, 20, 30);
        _camera.Position = newPosition;

        Assert.Equal(newPosition, _camera.Position);
        float expectedDistance = (newPosition - _camera.Target).Length;
        Assert.Equal(expectedDistance, _camera.Distance, 0.01f);
    }

    [Fact]
    public void Target_SetValue_UpdatesPositionInNormalMode()
    {
        var originalPosition = _camera.Position;
        var newTarget = new Vector3(100, 0, 0);

        _camera.Target = newTarget;

        Assert.Equal(newTarget, _camera.Target);
        // Position should be updated (not equal to original)
        Assert.NotEqual(originalPosition, _camera.Position);
    }

    [Fact]
    public void IsFlythroughMode_WhenEnabled_PreventsAutomaticUpdates()
    {
        _camera.IsFlythroughMode = true;
        var originalPosition = _camera.Position;

        _camera.Target = new Vector3(50, 50, 50);

        // In flythrough mode, position should not auto-update
        Assert.Equal(originalPosition, _camera.Position);
    }

    [Fact]
    public void Yaw_SetValue_NormalizesTo2Pi()
    {
        _camera.Yaw = 1.0f;
        Assert.Equal(1.0f, _camera.Yaw, 0.001f);

        // Test normalization for large positive values
        _camera.Yaw = MathF.PI * 3; // 540 degrees
        Assert.True(_camera.Yaw >= 0 && _camera.Yaw < MathF.PI * 2);

        // Test normalization for negative values
        _camera.Yaw = -MathF.PI;
        Assert.True(_camera.Yaw >= 0 && _camera.Yaw < MathF.PI * 2);
    }

    #endregion

    #region Matrix Tests

    [Fact]
    public void GetProjectionMatrix_PerspectiveMode_ReturnsValidMatrix()
    {
        _camera.SetAspectRatio(16f / 9f);
        var matrix = _camera.GetProjectionMatrix();

        Assert.NotEqual(Matrix4.Identity, matrix);
        // Matrix should have non-zero elements
        Assert.NotEqual(0f, matrix.M11);
        Assert.NotEqual(0f, matrix.M22);
    }

    [Fact]
    public void GetProjectionMatrix_IsometricMode_ReturnsOrthographicMatrix()
    {
        _camera.SetIsometricMode(true, 1.0f);
        _camera.SetAspectRatio(16f / 9f);

        var matrix = _camera.GetProjectionMatrix();

        Assert.NotEqual(Matrix4.Identity, matrix);
        // Orthographic projection has M44 = 1
        Assert.Equal(1f, matrix.M44, 0.001f);
    }

    [Fact]
    public void GetViewMatrix_ReturnsValidMatrix()
    {
        _camera.Position = new Vector3(0, 10, 20);
        _camera.Target = new Vector3(0, 0, 0);

        var matrix = _camera.GetViewMatrix();

        Assert.NotEqual(Matrix4.Identity, matrix);
        Assert.NotEqual(0f, matrix.M11);
    }

    [Fact]
    public void GetViewMatrix_WithRoll_AppliesRotation()
    {
        _camera.Position = new Vector3(0, 10, 20);
        _camera.Target = new Vector3(0, 0, 0);
        _camera.Roll = MathF.PI / 4; // 45 degrees

        var matrix = _camera.GetViewMatrix();

        Assert.NotEqual(Matrix4.Identity, matrix);
    }

    [Fact]
    public void GetViewMatrix_WithSamePositionAndTarget_UsesDefaultForward()
    {
        _camera.Position = new Vector3(0, 0, 0);
        _camera.Target = new Vector3(0, 0, 0);

        // Should not throw and should return valid matrix
        var matrix = _camera.GetViewMatrix();
        Assert.NotEqual(Matrix4.Zero, matrix);
    }

    #endregion

    #region Movement Tests

    [Fact]
    public void Move_UpdatesTargetAndPosition()
    {
        var originalTarget = _camera.Target;
        var direction = new Vector3(10, 0, 0);

        _camera.Move(direction);

        Assert.NotEqual(originalTarget, _camera.Target);
        Assert.Equal(originalTarget + direction, _camera.Target);
    }

    [Fact]
    public void Zoom_InPerspectiveMode_UpdatesDistance()
    {
        _camera.SetIsometricMode(false);
        var originalDistance = _camera.Distance;

        _camera.Zoom(10f);

        Assert.NotEqual(originalDistance, _camera.Distance);
    }

    [Fact]
    public void Zoom_InIsometricMode_UpdatesScale()
    {
        _camera.SetIsometricMode(true, 1.0f);

        // Zoom in (positive delta increases scale)
        _camera.Zoom(-10f);
        
        // Should still have valid projection after zoom
        var matrix = _camera.GetProjectionMatrix();
        Assert.NotEqual(Matrix4.Zero, matrix);
    }

    [Fact]
    public void Rotate_UpdatesYawAndPitch()
    {
        var originalYaw = _camera.Yaw;
        var originalPitch = _camera.Pitch;

        _camera.Rotate(100f, 50f);

        Assert.NotEqual(originalYaw, _camera.Yaw);
        Assert.NotEqual(originalPitch, _camera.Pitch);
    }

    [Fact]
    public void Rotate_ClampsPitchToValidRange()
    {
        // Rotate with very large pitch value
        _camera.Rotate(0f, 100000f);

        // Pitch should be clamped
        Assert.InRange(_camera.Pitch, -89f, 89f);
    }

    #endregion

    #region Reset and Configuration Tests

    [Fact]
    public void ResetView_RestoresInitialState()
    {
        var initialTarget = _camera.Target;
        var initialFov = _camera.Fov;

        // Modify camera state
        _camera.Move(new Vector3(100, 100, 100));
        _camera.Rotate(50f, 30f);
        _camera.Fov = 45f;

        // Verify state changed
        Assert.NotEqual(initialTarget, _camera.Target);
        Assert.NotEqual(initialFov, _camera.Fov);

        // Reset
        _camera.ResetView();

        // After reset, target, fov, and angles should be restored
        Assert.Equal(initialTarget, _camera.Target);
        Assert.Equal(initialFov, _camera.Fov);
        Assert.Equal(0f, _camera.Yaw);
        Assert.Equal(0f, _camera.Pitch);
        Assert.Equal(0f, _camera.Roll);
    }

    [Fact]
    public void SetAspectRatio_UpdatesAspectRatio()
    {
        _camera.SetAspectRatio(16f / 9f);

        // Verify by checking projection matrix changes
        var matrix1 = _camera.GetProjectionMatrix();

        _camera.SetAspectRatio(4f / 3f);
        var matrix2 = _camera.GetProjectionMatrix();

        Assert.NotEqual(matrix1, matrix2);
    }

    [Fact]
    public void SetIsometricMode_TogglesProjectionType()
    {
        _camera.SetAspectRatio(16f / 9f);

        // Perspective mode
        _camera.SetIsometricMode(false);
        var perspectiveMatrix = _camera.GetProjectionMatrix();

        // Isometric mode
        _camera.SetIsometricMode(true, 1.5f);
        var isometricMatrix = _camera.GetProjectionMatrix();

        Assert.NotEqual(perspectiveMatrix, isometricMatrix);
    }

    #endregion

    #region Additional Coverage Tests

    [Fact]
    public void Distance_SetVerySmallValue_ClampsToMinimum()
    {
        _camera.Distance = 0.0001f;
        Assert.True(_camera.Distance >= 1f);
    }

    [Fact]
    public void Distance_SetVeryLargeValue_ClampsToMaximum()
    {
        _camera.Distance = 50000f;
        Assert.True(_camera.Distance <= 10000f);
    }

    [Fact]
    public void Fov_SetExtremeValues_ClampsCorrectly()
    {
        _camera.Fov = 0f;
        Assert.Equal(5f, _camera.Fov);

        _camera.Fov = 500f;
        Assert.Equal(120f, _camera.Fov);
    }

    [Fact]
    public void Pitch_InFlythroughMode_DoesNotUpdatePosition()
    {
        _camera.IsFlythroughMode = true;
        var originalPosition = _camera.Position;

        _camera.Pitch = 1.5f;

        // Position should not change in flythrough mode
        Assert.Equal(originalPosition, _camera.Position);
    }

    [Fact]
    public void Yaw_InFlythroughMode_DoesNotUpdatePosition()
    {
        _camera.IsFlythroughMode = true;
        var originalPosition = _camera.Position;

        _camera.Yaw = 1.5f;

        // Position should not change in flythrough mode
        Assert.Equal(originalPosition, _camera.Position);
    }

    [Fact]
    public void Yaw_NegativeValue_WrapsToPositive()
    {
        _camera.Yaw = -MathF.PI;
        Assert.True(_camera.Yaw >= 0 && _camera.Yaw < MathF.PI * 2);
    }

    [Fact]
    public void Yaw_LargePositiveValue_WrapsCorrectly()
    {
        _camera.Yaw = MathF.PI * 5; // 900 degrees
        Assert.True(_camera.Yaw >= 0 && _camera.Yaw < MathF.PI * 2);
    }

    [Fact]
    public void Move_WithZeroDirection_DoesNotChangeTarget()
    {
        var originalTarget = _camera.Target;
        _camera.Move(Vector3.Zero);
        Assert.Equal(originalTarget, _camera.Target);
    }

    [Fact]
    public void Move_WithNegativeDirection_MovesCorrectly()
    {
        var originalTarget = _camera.Target;
        _camera.Move(new Vector3(-5, -5, -5));
        Assert.NotEqual(originalTarget, _camera.Target);
    }

    [Fact]
    public void Zoom_WithZeroDelta_MaintainsDistance()
    {
        var originalDistance = _camera.Distance;
        _camera.Zoom(0f);
        Assert.Equal(originalDistance, _camera.Distance);
    }

    [Fact]
    public void Zoom_NegativeValue_IncreasesDistance()
    {
        _camera.SetIsometricMode(false);
        var originalDistance = _camera.Distance;
        _camera.Zoom(-5f);
        Assert.True(_camera.Distance > originalDistance);
    }

    [Fact]
    public void Zoom_PositiveValue_DecreasesDistance()
    {
        _camera.SetIsometricMode(false);
        _camera.Distance = 100f; // Set initial distance
        var originalDistance = _camera.Distance;
        _camera.Zoom(5f);
        Assert.True(_camera.Distance < originalDistance);
    }

    [Fact]
    public void GetViewMatrix_WithZeroLengthForward_UsesDefaultZ()
    {
        _camera.Position = Vector3.Zero;
        _camera.Target = Vector3.Zero;

        var matrix = _camera.GetViewMatrix();
        
        // Should use default forward (UnitZ) and not crash
        Assert.NotEqual(Matrix4.Zero, matrix);
    }

    [Fact]
    public void GetViewMatrix_WithSmallRoll_AppliesCorrectly()
    {
        _camera.Position = new Vector3(0, 10, 20);
        _camera.Target = new Vector3(0, 0, 0);
        _camera.Roll = 0.1f; // Small roll

        var matrix1 = _camera.GetViewMatrix();
        
        _camera.Roll = 0f;
        var matrix2 = _camera.GetViewMatrix();

        Assert.NotEqual(matrix1, matrix2);
    }

    [Fact]
    public void SetIsometricMode_WithDifferentScales_AffectsProjection()
    {
        _camera.SetAspectRatio(16f / 9f);
        
        _camera.SetIsometricMode(true, 0.5f);
        var matrix1 = _camera.GetProjectionMatrix();

        _camera.SetIsometricMode(true, 2.0f);
        var matrix2 = _camera.GetProjectionMatrix();

        Assert.NotEqual(matrix1, matrix2);
    }

    [Fact]
    public void Rotate_WithZeroValues_MaintainsOrientation()
    {
        var originalYaw = _camera.Yaw;
        var originalPitch = _camera.Pitch;

        _camera.Rotate(0f, 0f);

        Assert.Equal(originalYaw, _camera.Yaw);
        Assert.Equal(originalPitch, _camera.Pitch);
    }

    [Fact]
    public void Position_MultipleUpdates_MaintainsConsistency()
    {
        _camera.Position = new Vector3(10, 10, 10);
        var distance1 = _camera.Distance;

        _camera.Position = new Vector3(20, 20, 20);
        var distance2 = _camera.Distance;

        Assert.NotEqual(distance1, distance2);
    }

    [Fact]
    public void GetProjectionMatrix_AfterFovChange_ReturnsUpdatedMatrix()
    {
        _camera.SetAspectRatio(16f / 9f);
        _camera.Fov = 60f;
        var matrix1 = _camera.GetProjectionMatrix();

        _camera.Fov = 90f;
        var matrix2 = _camera.GetProjectionMatrix();

        Assert.NotEqual(matrix1, matrix2);
    }

    [Fact]
    public void Target_MultipleChanges_UpdatesCorrectly()
    {
        _camera.Target = new Vector3(10, 0, 0);
        Assert.Equal(new Vector3(10, 0, 0), _camera.Target);

        _camera.Target = new Vector3(0, 10, 0);
        Assert.Equal(new Vector3(0, 10, 0), _camera.Target);

        _camera.Target = new Vector3(0, 0, 10);
        Assert.Equal(new Vector3(0, 0, 10), _camera.Target);
    }

    [Fact]
    public void Pitch_SetToZero_WorksCorrectly()
    {
        _camera.Pitch = 0.5f;
        _camera.Pitch = 0f;
        Assert.Equal(0f, _camera.Pitch);
    }

    [Fact]
    public void Yaw_SetToZero_WorksCorrectly()
    {
        _camera.Yaw = 1.5f;
        _camera.Yaw = 0f;
        Assert.Equal(0f, _camera.Yaw, 0.001f);
    }

    [Fact]
    public void Roll_SetToZero_WorksCorrectly()
    {
        _camera.Roll = 1.5f;
        _camera.Roll = 0f;
        Assert.Equal(0f, _camera.Roll);
    }

    [Fact]
    public void Distance_AfterTargetChange_UpdatesAccordingly()
    {
        _camera.IsFlythroughMode = false;
        
        _camera.Target = new Vector3(100, 100, 100);
        
        // Distance should be valid (positive)
        Assert.True(_camera.Distance > 0);
    }

    [Fact]
    public void GetViewMatrix_AfterMove_ReturnsUpdatedMatrix()
    {
        var matrix1 = _camera.GetViewMatrix();
        
        _camera.Move(new Vector3(50, 0, 0));
        var matrix2 = _camera.GetViewMatrix();

        Assert.NotEqual(matrix1, matrix2);
    }

    [Fact]
    public void GetViewMatrix_AfterRotate_ReturnsUpdatedMatrix()
    {
        var matrix1 = _camera.GetViewMatrix();
        
        _camera.Rotate(45f, 30f);
        var matrix2 = _camera.GetViewMatrix();

        Assert.NotEqual(matrix1, matrix2);
    }

    [Fact]
    public void Zoom_MultipleCallsInPerspective_ClampsCorrectly()
    {
        _camera.SetIsometricMode(false);
        
        // Zoom in a lot
        for (int i = 0; i < 100; i++)
        {
            _camera.Zoom(100f);
        }
        
        // Should be at minimum distance
        Assert.True(_camera.Distance >= 1f);
        
        // Zoom out a lot
        for (int i = 0; i < 200; i++)
        {
            _camera.Zoom(-100f);
        }
        
        // Should be at maximum distance
        Assert.True(_camera.Distance <= 10000f);
    }

    [Fact]
    public void GetViewMatrix_WithLargePitch_HandlesCorrectly()
    {
        _camera.Pitch = MathHelper.DegreesToRadians(170f);
        var matrix = _camera.GetViewMatrix();
        Assert.NotEqual(Matrix4.Zero, matrix);
    }

    [Fact]
    public void GetViewMatrix_WithLargeYaw_HandlesCorrectly()
    {
        _camera.Yaw = MathHelper.DegreesToRadians(350f);
        var matrix = _camera.GetViewMatrix();
        Assert.NotEqual(Matrix4.Zero, matrix);
    }

    [Fact]
    public void Position_SetToTarget_ResultsInZeroDistance()
    {
        _camera.Target = new Vector3(100, 200, 300);
        _camera.Position = new Vector3(100, 200, 300);
        
        Assert.Equal(0f, _camera.Distance, 0.01f);
    }

    [Fact]
    public void Rotate_LargePositiveYaw_WorksCorrectly()
    {
        _camera.Rotate(1000f, 0f);
        
        // After rotation, yaw should still be valid (will be normalized)
        // Just verify it's a valid number
        Assert.False(float.IsNaN(_camera.Yaw));
        Assert.False(float.IsInfinity(_camera.Yaw));
    }

    [Fact]
    public void Rotate_LargeNegativePitch_ClampsToMinimum()
    {
        _camera.Rotate(0f, -10000f);
        
        // Pitch should be clamped to -89°
        Assert.Equal(-89f, _camera.Pitch, 0.1f);
    }

    [Fact]
    public void Rotate_LargePositivePitch_ClampsToMaximum()
    {
        _camera.Rotate(0f, 10000f);
        
        // Pitch should be clamped to 89°
        Assert.Equal(89f, _camera.Pitch, 0.1f);
    }

    [Fact]
    public void GetViewMatrix_WithPerpendicularVectors_HandlesCorrectly()
    {
        _camera.Position = new Vector3(10, 0, 0);
        _camera.Target = new Vector3(0, 0, 0);
        
        var matrix = _camera.GetViewMatrix();
        Assert.NotEqual(Matrix4.Zero, matrix);
    }

    [Fact]
    public void GetViewMatrix_WithVerticalLookVector_UsesDefaultRight()
    {
        _camera.Position = new Vector3(0, 100, 0);
        _camera.Target = new Vector3(0, 0, 0);
        
        // This should trigger the fallback for right vector calculation
        var matrix = _camera.GetViewMatrix();
        Assert.NotEqual(Matrix4.Zero, matrix);
    }

    [Fact]
    public void GetViewMatrix_AfterMultipleRotations_RemainStable()
    {
        for (int i = 0; i < 10; i++)
        {
            _camera.Rotate(10f, 5f);
        }
        
        var matrix = _camera.GetViewMatrix();
        Assert.NotEqual(Matrix4.Zero, matrix);
        Assert.False(float.IsNaN(matrix.M11));
    }

    [Fact]
    public void SetIsometricMode_ToggleMultipleTimes_WorksCorrectly()
    {
        _camera.SetAspectRatio(16f / 9f);
        
        _camera.SetIsometricMode(true, 1.0f);
        var iso1 = _camera.GetProjectionMatrix();
        
        _camera.SetIsometricMode(false);
        var persp = _camera.GetProjectionMatrix();
        
        _camera.SetIsometricMode(true, 1.0f);
        var iso2 = _camera.GetProjectionMatrix();
        
        // Iso1 and iso2 should be similar
        Assert.NotEqual(iso1, persp);
    }

    [Fact]
    public void Zoom_InIsometricMode_ClampsScale()
    {
        _camera.SetIsometricMode(true, 1.0f);
        
        // Zoom in a lot (negative delta reduces scale)
        for (int i = 0; i < 100; i++)
        {
            _camera.Zoom(100f);
        }
        
        // Should still produce valid projection matrix
        var matrix = _camera.GetProjectionMatrix();
        Assert.NotEqual(Matrix4.Zero, matrix);
    }

    [Fact]
    public void Move_ChainedCalls_AccumulatesMovement()
    {
        var originalTarget = _camera.Target;
        
        _camera.Move(new Vector3(10, 0, 0));
        _camera.Move(new Vector3(0, 10, 0));
        _camera.Move(new Vector3(0, 0, 10));
        
        var expectedTarget = originalTarget + new Vector3(10, 10, 10);
        Assert.Equal(expectedTarget, _camera.Target);
    }

    [Fact]
    public void Position_SetWhileInFlythroughMode_UpdatesDistanceCorrectly()
    {
        _camera.IsFlythroughMode = true;
        _camera.Target = new Vector3(0, 0, 0);
        _camera.Position = new Vector3(100, 0, 0);
        
        Assert.Equal(100f, _camera.Distance, 0.01f);
    }

    [Fact]
    public void GetViewMatrix_WithNegativeRoll_AppliesCorrectly()
    {
        _camera.Position = new Vector3(0, 10, 20);
        _camera.Target = new Vector3(0, 0, 0);
        _camera.Roll = -MathF.PI / 4; // -45 degrees
        
        var matrix = _camera.GetViewMatrix();
        Assert.NotEqual(Matrix4.Identity, matrix);
    }

    [Fact]
    public void GetViewMatrix_WithFullRotationRoll_HandlesCorrectly()
    {
        _camera.Position = new Vector3(0, 10, 20);
        _camera.Target = new Vector3(0, 0, 0);
        _camera.Roll = MathF.PI * 2; // 360 degrees
        
        var matrix = _camera.GetViewMatrix();
        Assert.NotEqual(Matrix4.Zero, matrix);
    }

    [Fact]
    public void Yaw_MultipleWrapArounds_MaintainsConsistency()
    {
        _camera.Yaw = MathF.PI * 2;
        var firstYaw = _camera.Yaw;
        
        _camera.Yaw = MathF.PI * 4;
        var secondYaw = _camera.Yaw;
        
        // Both should be normalized to same range
        Assert.True(firstYaw >= 0 && firstYaw < MathF.PI * 2);
        Assert.True(secondYaw >= 0 && secondYaw < MathF.PI * 2);
    }

    [Fact]
    public void Rotate_CombinedYawAndPitch_UpdatesBothCorrectly()
    {
        var originalYaw = _camera.Yaw;
        var originalPitch = _camera.Pitch;
        
        _camera.Rotate(50f, 25f);
        
        Assert.NotEqual(originalYaw, _camera.Yaw);
        Assert.NotEqual(originalPitch, _camera.Pitch);
    }

    [Fact]
    public void SetAspectRatio_Zero_ThrowsOnGetProjection()
    {
        _camera.SetAspectRatio(0f);
        
        // OpenTK throws ArgumentOutOfRangeException for aspect <= 0
        Assert.Throws<ArgumentOutOfRangeException>(() => _camera.GetProjectionMatrix());
    }

    [Fact]
    public void SetAspectRatio_NegativeValue_ThrowsOnGetProjection()
    {
        _camera.SetAspectRatio(-1f);
        
        Assert.Throws<ArgumentOutOfRangeException>(() => _camera.GetProjectionMatrix());
    }


    #endregion
}
