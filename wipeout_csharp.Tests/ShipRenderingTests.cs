using Xunit;
using WipeoutRewrite.Core.Entities;
using Moq;
using WipeoutRewrite.Infrastructure.Graphics;
using System;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for Ship rendering and transformation matrix calculations.
/// Tests matrix creation, helper position methods, and rendering logic.
/// </summary>
public class ShipRenderingTests
{
    [Fact]
    public void CalculateTransformMatrix_WithZeroAngles_ShouldReturnIdentityLikeMatrix()
    {
        var ship = new Ship("Test", 1);
        ship.Position = new Vec3(0, 0, 0);
        ship.Angle = new Vec3(0, 0, 0);
        
        Mat4 matrix = ship.CalculateTransformMatrix();
        
        Assert.NotNull(matrix.M);
        Assert.Equal(16, matrix.M.Length);
    }

    [Fact]
    public void CalculateTransformMatrix_WithPosition_ShouldIncludeTranslation()
    {
        var ship = new Ship("Test", 1);
        ship.Position = new Vec3(100, 200, 300);
        ship.Angle = new Vec3(0, 0, 0);
        
        Mat4 matrix = ship.CalculateTransformMatrix();
        
        // Translation is in elements [12], [13], [14] (column-major)
        Assert.Equal(100f, matrix.M[12], 0.1f);
        Assert.Equal(200f, matrix.M[13], 0.1f);
        Assert.Equal(300f, matrix.M[14], 0.1f);
    }

    [Fact]
    public void GetCockpitPosition_ShouldBeForwardAndUp()
    {
        var ship = new Ship("Test", 1);
        ship.Position = new Vec3(0, 0, 0);
        ship.Update(0.01f); // Update direction vectors
        
        Vec3 cockpit = ship.GetCockpitPosition();
        
        // Cockpit is 150 forward + 60 up
        // Initial forward is (0,0,1), up is (0,1,0)
        Assert.Equal(0f, cockpit.X, 0.1f);
        Assert.Equal(60f, cockpit.Y, 0.1f);
        Assert.Equal(150f, cockpit.Z, 0.1f);
    }

    [Fact]
    public void GetNosePosition_ShouldBeForward384Units()
    {
        var ship = new Ship("Test", 1);
        ship.Position = new Vec3(100, 100, 100);
        ship.Update(0.01f);
        
        Vec3 nose = ship.GetNosePosition();
        
        // Nose is 384 units forward from position
        Assert.Equal(100f, nose.X, 0.1f);
        Assert.Equal(100f, nose.Y, 0.1f);
        Assert.Equal(484f, nose.Z, 0.1f); // 100 + 384
    }

    [Fact]
    public void GetWingLeftPosition_ShouldBeLeftAndBack()
    {
        var ship = new Ship("Test", 1);
        ship.Position = new Vec3(0, 0, 0);
        ship.Update(0.01f);
        
        Vec3 wingLeft = ship.GetWingLeftPosition();
        
        // Wing left is 256 left and 384 back
        // Initial right is (1,0,0), forward is (0,0,1)
        Assert.Equal(-256f, wingLeft.X, 0.1f);
        Assert.Equal(0f, wingLeft.Y, 0.1f);
        Assert.Equal(-384f, wingLeft.Z, 0.1f);
    }

    [Fact]
    public void GetWingRightPosition_ShouldBeRightAndBack()
    {
        var ship = new Ship("Test", 1);
        ship.Position = new Vec3(0, 0, 0);
        ship.Update(0.01f);
        
        Vec3 wingRight = ship.GetWingRightPosition();
        
        // Wing right is 256 right and 384 back
        Assert.Equal(256f, wingRight.X, 0.1f);
        Assert.Equal(0f, wingRight.Y, 0.1f);
        Assert.Equal(-384f, wingRight.Z, 0.1f);
    }

    [Fact]
    public void GetWingPositions_ShouldBeSymmetric()
    {
        var ship = new Ship("Test", 1);
        ship.Position = new Vec3(50, 50, 50);
        ship.Update(0.01f);
        
        Vec3 wingLeft = ship.GetWingLeftPosition();
        Vec3 wingRight = ship.GetWingRightPosition();
        
        // Wings should be symmetric around ship center
        float leftDistance = ship.Position.DistanceTo(wingLeft);
        float rightDistance = ship.Position.DistanceTo(wingRight);
        
        Assert.Equal(leftDistance, rightDistance, 0.1f);
    }

    [Fact]
    public void Render_WhenNotVisible_ShouldNotCrash()
    {
        var ship = new Ship("Test", 1);
        ship.IsVisible = false;
        var mockRenderer = new Mock<IRenderer>();
        
        var exception = Record.Exception(() => ship.Render(mockRenderer.Object));
        
        Assert.Null(exception);
    }

    [Fact]
    public void Render_WhenVisible_ShouldNotCrash()
    {
        var ship = new Ship("Test", 1);
        ship.IsVisible = true;
        var mockRenderer = new Mock<IRenderer>();
        
        var exception = Record.Exception(() => ship.Render(mockRenderer.Object));
        
        Assert.Null(exception);
    }

    [Fact]
    public void RenderShadow_WhenNotVisible_ShouldNotCrash()
    {
        var ship = new Ship("Test", 1);
        ship.IsVisible = false;
        var mockRenderer = new Mock<IRenderer>();
        
        var exception = Record.Exception(() => ship.RenderShadow(mockRenderer.Object));
        
        Assert.Null(exception);
    }

    [Fact]
    public void RenderShadow_WhenFlying_ShouldNotCrash()
    {
        var ship = new Ship("Test", 1);
        ship.IsVisible = true;
        ship.IsFlying = true;
        var mockRenderer = new Mock<IRenderer>();
        
        var exception = Record.Exception(() => ship.RenderShadow(mockRenderer.Object));
        
        Assert.Null(exception);
    }

    [Fact]
    public void RenderShadow_WhenOnGround_ShouldNotCrash()
    {
        var ship = new Ship("Test", 1);
        ship.IsVisible = true;
        ship.IsFlying = false;
        var mockRenderer = new Mock<IRenderer>();
        
        var exception = Record.Exception(() => ship.RenderShadow(mockRenderer.Object));
        
        Assert.Null(exception);
    }

    [Fact]
    public void GetNosePosition_WithRotation_ShouldFollowForwardVector()
    {
        var ship = new Ship("Test", 1);
        ship.Position = new Vec3(0, 0, 0);
        ship.Angle = new Vec3(0, MathF.PI / 2, 0); // 90 degree yaw
        ship.Update(0.01f); // Update direction vectors
        
        Vec3 nose = ship.GetNosePosition();
        
        // After 90 degree yaw, forward points in -X direction
        Assert.Equal(-384f, nose.X, 10.0f);
        Assert.Equal(0f, nose.Y, 10.0f);
        Assert.Equal(0f, nose.Z, 10.0f);
    }

    [Fact]
    public void HelperPositions_ShouldUpdateWithRotation()
    {
        var ship = new Ship("Test", 1);
        ship.Position = new Vec3(0, 0, 0);
        ship.Angle = new Vec3(MathF.PI / 4, 0, 0); // 45 degree pitch
        ship.Update(0.01f);
        
        Vec3 cockpit = ship.GetCockpitPosition();
        Vec3 nose = ship.GetNosePosition();
        
        // Positions should be affected by pitch
        Assert.NotEqual(0, cockpit.Y); // Cockpit moved vertically
        Assert.NotEqual(0, nose.Y);    // Nose moved vertically
    }

    [Fact]
    public void CalculateTransformMatrix_MultipleTimes_ShouldBeConsistent()
    {
        var ship = new Ship("Test", 1);
        ship.Position = new Vec3(100, 200, 300);
        ship.Angle = new Vec3(0.5f, 0.3f, 0.2f);
        
        Mat4 matrix1 = ship.CalculateTransformMatrix();
        Mat4 matrix2 = ship.CalculateTransformMatrix();
        
        // Should produce identical results
        for (int i = 0; i < 16; i++)
        {
            Assert.Equal(matrix1.M[i], matrix2.M[i], 5.0f);
        }
    }
}
