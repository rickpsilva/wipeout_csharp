using Xunit;
using System;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for Mat4 transformation matrix.
/// Tests identity, translation, rotation, and matrix operations.
/// </summary>
public class Mat4Tests
{
    [Fact]
    public void Constructor_WithCorrectSize_ShouldSucceed()
    {
        float[] data = new float[16];
        var matrix = new Mat4(data);
        
        Assert.NotNull(matrix.M);
        Assert.Equal(16, matrix.M.Length);
    }

    [Fact]
    public void Constructor_WithWrongSize_ShouldThrowException()
    {
        float[] data = new float[12]; // Wrong size
        
        Assert.Throws<ArgumentException>(() => new Mat4(data));
    }

    [Fact]
    public void Identity_ShouldCreateIdentityMatrix()
    {
        Mat4 identity = Mat4.Identity();
        
        // Diagonal should be 1
        Assert.Equal(1f, identity.M[0]);  // m00
        Assert.Equal(1f, identity.M[5]);  // m11
        Assert.Equal(1f, identity.M[10]); // m22
        Assert.Equal(1f, identity.M[15]); // m33
        
        // Off-diagonal should be 0
        Assert.Equal(0f, identity.M[1]);
        Assert.Equal(0f, identity.M[2]);
        Assert.Equal(0f, identity.M[4]);
    }

    [Fact]
    public void Translation_ShouldCreateTranslationMatrix()
    {
        Vec3 position = new Vec3(10, 20, 30);
        Mat4 matrix = Mat4.Translation(position);
        
        // Translation is in column 3 (indices 12, 13, 14)
        Assert.Equal(10f, matrix.M[12]);
        Assert.Equal(20f, matrix.M[13]);
        Assert.Equal(30f, matrix.M[14]);
        
        // Should still have identity rotation part
        Assert.Equal(1f, matrix.M[0]);
        Assert.Equal(1f, matrix.M[5]);
        Assert.Equal(1f, matrix.M[10]);
    }

    [Fact]
    public void FromEulerAngles_WithZeroAngles_ShouldBeIdentity()
    {
        Vec3 angles = new Vec3(0, 0, 0);
        Mat4 matrix = Mat4.FromEulerAngles(angles);
        
        // Should be close to identity
        Assert.Equal(1f, matrix.M[0], 0.001f);
        Assert.Equal(1f, matrix.M[5], 0.001f);
        Assert.Equal(1f, matrix.M[10], 0.001f);
        Assert.Equal(1f, matrix.M[15], 0.001f);
    }

    [Fact]
    public void FromEulerAngles_WithYaw90_ShouldRotateCorrectly()
    {
        Vec3 angles = new Vec3(0, MathF.PI / 2, 0); // 90 degree yaw
        Mat4 matrix = Mat4.FromEulerAngles(angles);
        
        // After 90 degree yaw, X and Z axes should swap
        Assert.Equal(0f, matrix.M[0], 0.01f);  // cos(90) = 0
        Assert.Equal(-1f, matrix.M[2], 0.01f); // -sin(90) = -1
    }

    [Fact]
    public void FromPositionAndAngles_ShouldCombineTranslationAndRotation()
    {
        Vec3 position = new Vec3(100, 200, 300);
        Vec3 angles = new Vec3(0, 0, 0);
        
        Mat4 matrix = Mat4.FromPositionAndAngles(position, angles);
        
        // Should have translation
        Assert.Equal(100f, matrix.M[12]);
        Assert.Equal(200f, matrix.M[13]);
        Assert.Equal(300f, matrix.M[14]);
        
        // Should have rotation (identity in this case)
        Assert.Equal(1f, matrix.M[0], 0.01f);
        Assert.Equal(1f, matrix.M[5], 0.01f);
    }

    [Fact]
    public void Multiply_IdentityByIdentity_ShouldBeIdentity()
    {
        Mat4 id1 = Mat4.Identity();
        Mat4 id2 = Mat4.Identity();
        
        Mat4 result = Mat4.Multiply(id1, id2);
        
        Assert.Equal(1f, result.M[0], 0.001f);
        Assert.Equal(1f, result.M[5], 0.001f);
        Assert.Equal(1f, result.M[10], 0.001f);
        Assert.Equal(1f, result.M[15], 0.001f);
    }

    [Fact]
    public void Multiply_OperatorOverload_ShouldWork()
    {
        Mat4 a = Mat4.Identity();
        Mat4 b = Mat4.Translation(new Vec3(10, 20, 30));
        
        Mat4 result = a * b;
        
        Assert.NotNull(result.M);
        Assert.Equal(16, result.M.Length);
    }

    [Fact]
    public void Translation_WithNegativeValues_ShouldWork()
    {
        Vec3 position = new Vec3(-50, -100, -150);
        Mat4 matrix = Mat4.Translation(position);
        
        Assert.Equal(-50f, matrix.M[12]);
        Assert.Equal(-100f, matrix.M[13]);
        Assert.Equal(-150f, matrix.M[14]);
    }

    [Fact]
    public void FromEulerAngles_WithMultipleRotations_ShouldNotCrash()
    {
        Vec3 angles = new Vec3(0.5f, 0.3f, 0.2f);
        
        var exception = Record.Exception(() => Mat4.FromEulerAngles(angles));
        
        Assert.Null(exception);
    }

    [Fact]
    public void FromPositionAndAngles_WithComplexValues_ShouldNotCrash()
    {
        Vec3 position = new Vec3(123.45f, -67.89f, 101.23f);
        Vec3 angles = new Vec3(1.2f, 0.8f, 0.4f);
        
        var exception = Record.Exception(() => Mat4.FromPositionAndAngles(position, angles));
        
        Assert.Null(exception);
    }

    [Fact]
    public void Multiply_WithTranslations_ShouldCombine()
    {
        Mat4 t1 = Mat4.Translation(new Vec3(10, 0, 0));
        Mat4 t2 = Mat4.Translation(new Vec3(5, 0, 0));
        
        Mat4 result = t1 * t2;
        
        // Combined translation should be 15 in X
        Assert.Equal(15f, result.M[12], 0.01f);
    }
}
