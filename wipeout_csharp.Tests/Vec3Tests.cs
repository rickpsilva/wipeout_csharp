using Xunit;
using WipeoutRewrite;
using System;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for Vec3 mathematical operations.
/// Tests vector math, plane projections, and distance calculations.
/// </summary>
public class Vec3Tests
{
    [Fact]
    public void Constructor_ShouldInitializeComponents()
    {
        var v = new Vec3(1, 2, 3);
        
        Assert.Equal(1, v.X);
        Assert.Equal(2, v.Y);
        Assert.Equal(3, v.Z);
    }

    [Fact]
    public void Add_ShouldAddVectors()
    {
        var v1 = new Vec3(1, 2, 3);
        var v2 = new Vec3(4, 5, 6);
        
        var result = v1.Add(v2);
        
        Assert.Equal(5, result.X);
        Assert.Equal(7, result.Y);
        Assert.Equal(9, result.Z);
    }

    [Fact]
    public void Subtract_ShouldSubtractVectors()
    {
        var v1 = new Vec3(5, 7, 9);
        var v2 = new Vec3(1, 2, 3);
        
        var result = v1.Subtract(v2);
        
        Assert.Equal(4, result.X);
        Assert.Equal(5, result.Y);
        Assert.Equal(6, result.Z);
    }

    [Fact]
    public void Multiply_ShouldScaleVector()
    {
        var v = new Vec3(1, 2, 3);
        
        var result = v.Multiply(2);
        
        Assert.Equal(2, result.X);
        Assert.Equal(4, result.Y);
        Assert.Equal(6, result.Z);
    }

    [Fact]
    public void Length_ShouldCalculateMagnitude()
    {
        var v = new Vec3(3, 4, 0);
        
        Assert.Equal(5f, v.Length(), 0.01f);
    }

    [Fact]
    public void Normalize_ShouldCreateUnitVector()
    {
        var v = new Vec3(3, 4, 0);
        
        var normalized = v.Normalize();
        
        Assert.Equal(1f, normalized.Length(), 0.01f);
    }

    [Fact]
    public void Dot_ShouldCalculateDotProduct()
    {
        var v1 = new Vec3(1, 2, 3);
        var v2 = new Vec3(4, 5, 6);
        
        float dot = v1.Dot(v2);
        
        Assert.Equal(32, dot); // 1*4 + 2*5 + 3*6 = 32
    }

    [Fact]
    public void Cross_ShouldCalculateCrossProduct()
    {
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0, 1, 0);
        
        var cross = v1.Cross(v2);
        
        Assert.Equal(0, cross.X);
        Assert.Equal(0, cross.Y);
        Assert.Equal(1, cross.Z);
    }

    [Fact]
    public void DistanceTo_ShouldCalculateDistance()
    {
        var v1 = new Vec3(0, 0, 0);
        var v2 = new Vec3(3, 4, 0);
        
        float distance = v1.DistanceTo(v2);
        
        Assert.Equal(5f, distance, 0.01f);
    }

    [Fact]
    public void DistanceToPlane_WithPointOnPlane_ShouldReturnZero()
    {
        var point = new Vec3(5, 10, 5);
        var planePoint = new Vec3(0, 10, 0);
        var planeNormal = new Vec3(0, 1, 0); // Horizontal plane at Y=10
        
        float distance = point.DistanceToPlane(planePoint, planeNormal);
        
        Assert.Equal(0f, distance, 0.01f);
    }

    [Fact]
    public void DistanceToPlane_WithPointAbovePlane_ShouldReturnPositive()
    {
        var point = new Vec3(0, 15, 0);
        var planePoint = new Vec3(0, 10, 0);
        var planeNormal = new Vec3(0, 1, 0); // Up normal
        
        float distance = point.DistanceToPlane(planePoint, planeNormal);
        
        Assert.Equal(5f, distance, 0.01f);
    }

    [Fact]
    public void DistanceToPlane_WithPointBelowPlane_ShouldReturnNegative()
    {
        var point = new Vec3(0, 5, 0);
        var planePoint = new Vec3(0, 10, 0);
        var planeNormal = new Vec3(0, 1, 0);
        
        float distance = point.DistanceToPlane(planePoint, planeNormal);
        
        Assert.Equal(-5f, distance, 0.01f);
    }

    [Fact]
    public void ProjectOntoPlane_ShouldProjectPointOntoPlane()
    {
        var point = new Vec3(10, 50, 20);
        var planePoint = new Vec3(0, 0, 0);
        var planeNormal = new Vec3(0, 1, 0); // XZ plane at Y=0
        
        var projected = point.ProjectOntoPlane(planePoint, planeNormal);
        
        // X and Z should remain unchanged, Y should be 0
        Assert.Equal(10f, projected.X, 0.01f);
        Assert.Equal(0f, projected.Y, 0.01f);
        Assert.Equal(20f, projected.Z, 0.01f);
    }

    [Fact]
    public void ProjectOntoPlane_WithAngle_ShouldProjectCorrectly()
    {
        // Point above a slanted plane
        var point = new Vec3(0, 10, 0);
        var planePoint = new Vec3(0, 0, 0);
        var planeNormal = new Vec3(0, 1, 1).Normalize(); // 45 degree slant
        
        var projected = point.ProjectOntoPlane(planePoint, planeNormal);
        
        // Projected point should be closer to origin than original
        float originalDistance = point.DistanceTo(planePoint);
        float projectedDistance = projected.DistanceTo(planePoint);
        
        Assert.True(projectedDistance < originalDistance);
    }

    [Fact]
    public void ProjectOntoPlane_PointAlreadyOnPlane_ShouldRemainUnchanged()
    {
        var point = new Vec3(5, 0, 10);
        var planePoint = new Vec3(0, 0, 0);
        var planeNormal = new Vec3(0, 1, 0); // XZ plane
        
        var projected = point.ProjectOntoPlane(planePoint, planeNormal);
        
        Assert.Equal(5f, projected.X, 0.01f);
        Assert.Equal(0f, projected.Y, 0.01f);
        Assert.Equal(10f, projected.Z, 0.01f);
    }

    [Fact]
    public void OperatorPlus_ShouldAddVectors()
    {
        var v1 = new Vec3(1, 2, 3);
        var v2 = new Vec3(4, 5, 6);
        
        var result = v1 + v2;
        
        Assert.Equal(5, result.X);
        Assert.Equal(7, result.Y);
        Assert.Equal(9, result.Z);
    }

    [Fact]
    public void OperatorMinus_ShouldSubtractVectors()
    {
        var v1 = new Vec3(5, 7, 9);
        var v2 = new Vec3(1, 2, 3);
        
        var result = v1 - v2;
        
        Assert.Equal(4, result.X);
        Assert.Equal(5, result.Y);
        Assert.Equal(6, result.Z);
    }

    [Fact]
    public void OperatorMultiply_ShouldScaleVector()
    {
        var v = new Vec3(1, 2, 3);
        
        var result = v * 3;
        
        Assert.Equal(3, result.X);
        Assert.Equal(6, result.Y);
        Assert.Equal(9, result.Z);
    }

    [Fact]
    public void OperatorDivide_ShouldDivideVector()
    {
        var v = new Vec3(6, 9, 12);
        
        var result = v / 3;
        
        Assert.Equal(2, result.X);
        Assert.Equal(3, result.Y);
        Assert.Equal(4, result.Z);
    }
}
