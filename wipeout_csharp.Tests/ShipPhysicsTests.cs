using Xunit;
using WipeoutRewrite.Core.Entities;
using System;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for Ship physics and update logic.
/// Tests direction vector calculation, velocity integration, and state updates.
/// </summary>
public class ShipPhysicsTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        var ship = new Ship("Test Ship", 1);
        
        Assert.Equal("Test Ship", ship.Name);
        Assert.Equal(1, ship.ShipId);
        Assert.Equal(0, ship.Speed);
        Assert.True(ship.IsVisible);
        Assert.True(ship.IsRacing);
        Assert.False(ship.IsFlying);
    }

    [Fact]
    public void Constructor_ShouldInitializePositionToZero()
    {
        var ship = new Ship("Test", 1);
        
        Assert.Equal(0, ship.Position.X);
        Assert.Equal(0, ship.Position.Y);
        Assert.Equal(0, ship.Position.Z);
    }

    [Fact]
    public void Constructor_ShouldInitializeDirectionVectorsCorrectly()
    {
        var ship = new Ship("Test", 1);
        
        // Initial forward should be (0, 0, 1) - pointing in Z direction
        Assert.Equal(0f, ship.DirForward.X, 0.01f);
        Assert.Equal(0f, ship.DirForward.Y, 0.01f);
        Assert.Equal(1f, ship.DirForward.Z, 0.01f);
        
        // Initial right should be (1, 0, 0) - pointing in X direction
        Assert.Equal(1f, ship.DirRight.X, 0.01f);
        Assert.Equal(0f, ship.DirRight.Y, 0.01f);
        Assert.Equal(0f, ship.DirRight.Z, 0.01f);
        
        // Initial up should be (0, 1, 0) - pointing in Y direction
        Assert.Equal(0f, ship.DirUp.X, 0.01f);
        Assert.Equal(1f, ship.DirUp.Y, 0.01f);
        Assert.Equal(0f, ship.DirUp.Z, 0.01f);
    }

    [Fact]
    public void Update_WithZeroVelocity_ShouldNotChangePosition()
    {
        var ship = new Ship("Test", 1);
        var initialPos = ship.Position;
        
        ship.Update(1.0f);
        
        Assert.Equal(initialPos.X, ship.Position.X);
        Assert.Equal(initialPos.Y, ship.Position.Y);
        Assert.Equal(initialPos.Z, ship.Position.Z);
    }

    [Fact]
    public void Update_WithVelocity_ShouldUpdatePosition()
    {
        var ship = new Ship("Test", 1);
        ship.Velocity = new Vec3(10, 0, 0); // 10 units/sec in X direction
        
        ship.Update(1.0f); // 1 second
        
        Assert.Equal(10, ship.Position.X, 2);
        Assert.Equal(0, ship.Position.Y, 2);
        Assert.Equal(0, ship.Position.Z, 2);
    }

    [Fact]
    public void Update_WithDeltaTime_ShouldScalePositionChange()
    {
        var ship = new Ship("Test", 1);
        ship.Velocity = new Vec3(100, 0, 0);
        
        ship.Update(0.1f); // 0.1 seconds
        
        Assert.Equal(10, ship.Position.X, 2); // 100 * 0.1 = 10
    }

    [Fact]
    public void Update_WithAngularVelocity_ShouldUpdateAngles()
    {
        var ship = new Ship("Test", 1);
        ship.AngularVelocity = new Vec3(0, 1.0f, 0); // 1 radian/sec yaw
        
        ship.Update(1.0f);
        
        Assert.Equal(1.0f, ship.Angle.Y, 2);
    }

    [Fact]
    public void Update_ShouldCalculateSpeed()
    {
        var ship = new Ship("Test", 1);
        ship.Velocity = new Vec3(3, 4, 0); // 3-4-5 triangle
        
        ship.Update(0.01f);
        
        Assert.Equal(5.0f, ship.Speed, 2);
    }

    [Fact]
    public void Update_WithRotation_ShouldUpdateDirectionVectors()
    {
        var ship = new Ship("Test", 1);
        ship.Angle = new Vec3(0, MathF.PI / 2, 0); // 90 degree yaw rotation
        
        ship.Update(0.01f);
        
        // After 90 degree yaw, forward should point in +X direction
        Assert.Equal(-1, ship.DirForward.X, 1);
        Assert.Equal(0, ship.DirForward.Y, 1);
        Assert.Equal(0, ship.DirForward.Z, 1);
    }

    [Fact]
    public void Update_DirectionVectors_ShouldBeOrthogonal()
    {
        var ship = new Ship("Test", 1);
        ship.Angle = new Vec3(0.5f, 0.3f, 0.2f); // Arbitrary rotation
        
        ship.Update(0.01f);
        
        // Forward and Right should be perpendicular (dot product = 0)
        float dotFR = ship.DirForward.Dot(ship.DirRight);
        Assert.Equal(0, dotFR, 1);
        
        // Forward and Up should be perpendicular
        float dotFU = ship.DirForward.Dot(ship.DirUp);
        Assert.Equal(0, dotFU, 1);
        
        // Right and Up should be perpendicular
        float dotRU = ship.DirRight.Dot(ship.DirUp);
        Assert.Equal(0, dotRU, 1);
    }

    [Fact]
    public void Update_DirectionVectors_ShouldBeUnitLength()
    {
        var ship = new Ship("Test", 1);
        ship.Angle = new Vec3(0.5f, 0.3f, 0.2f);
        
        ship.Update(0.01f);
        
        Assert.Equal(1.0f, ship.DirForward.Length(), 1);
        Assert.Equal(1.0f, ship.DirRight.Length(), 1);
        Assert.Equal(1.0f, ship.DirUp.Length(), 1);
    }

    [Fact]
    public void Update_MultipleFrames_ShouldAccumulatePosition()
    {
        var ship = new Ship("Test", 1);
        ship.Velocity = new Vec3(10, 0, 0);
        
        ship.Update(0.1f);
        ship.Update(0.1f);
        ship.Update(0.1f);
        
        Assert.Equal(3.0f, ship.Position.X, 2); // 10 * 0.1 * 3 frames
    }

    [Fact]
    public void TakeDamage_ShouldReduceShield()
    {
        var ship = new Ship("Test", 1);
        ship.Shield = 100;
        
        ship.TakeDamage(30);
        
        Assert.Equal(70, ship.Shield);
    }

    [Fact]
    public void TakeDamage_WhenShieldReachesZero_ShouldDestroy()
    {
        var ship = new Ship("Test", 1);
        ship.Shield = 50;
        
        ship.TakeDamage(60);
        
        Assert.True(ship.IsDestroyed);
        Assert.True(ship.Shield <= 0);
    }

    [Fact]
    public void Render_WhenNotVisible_ShouldNotCrash()
    {
        var ship = new Ship("Test", 1);
        ship.IsVisible = false;
        
        var exception = Record.Exception(() => ship.Render(null!));
        
        Assert.Null(exception);
    }

    [Fact]
    public void Update_WithComplexVelocity_ShouldUpdatePositionCorrectly()
    {
        var ship = new Ship("Test", 1);
        ship.Position = new Vec3(100, 50, 200);
        ship.Velocity = new Vec3(-20, 10, -30);
        
        ship.Update(2.0f);
        
        Assert.Equal(60, ship.Position.X, 2);  // 100 + (-20 * 2)
        Assert.Equal(70, ship.Position.Y, 2);  // 50 + (10 * 2)
        Assert.Equal(140, ship.Position.Z, 2); // 200 + (-30 * 2)
    }
}
