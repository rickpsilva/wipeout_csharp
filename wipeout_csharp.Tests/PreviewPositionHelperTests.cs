using System;
using Xunit;
using OpenTK.Mathematics;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Presentation;

namespace WipeoutRewrite.Tests;

public class PreviewPositionHelperTests
{
    private static int[] CaptureViewport(PreviewPosition position, int screenWidth, int screenHeight)
    {
        int[] captured = Array.Empty<int>();
        try
        {
            PreviewPositionHelper.ViewportOverride = (x, y, w, h) => { captured = new[] { x, y, w, h }; };
            PreviewPositionHelper.ApplyPositionLayout(position, screenWidth, screenHeight);
        }
        finally
        {
            PreviewPositionHelper.ViewportOverride = null;
        }
        return captured;
    }

    [Fact]
    public void ApplyPositionLayout_Center_SetsFullViewport()
    {
        var viewport = CaptureViewport(PreviewPosition.Center, 1920, 1080);
        Assert.Equal(new[] { 0, 0, 1920, 1080 }, viewport);
    }

    [Fact]
    public void ApplyPositionLayout_LeftBottom_SetsExpectedViewport()
    {
        var viewport = CaptureViewport(PreviewPosition.LeftBottom, 1920, 1080);
        Assert.Equal(new[] { 0, 0, 640, 360 }, viewport);
    }

    [Fact]
    public void ApplyPositionLayout_RightBottom_SetsExpectedViewport()
    {
        var viewport = CaptureViewport(PreviewPosition.RightBottom, 1920, 1080);
        Assert.Equal(new[] { 1280, 0, 640, 360 }, viewport);
    }

    [Fact]
    public void ApplyPositionLayout_TopCenter_SetsExpectedViewport()
    {
        var viewport = CaptureViewport(PreviewPosition.TopCenter, 1920, 1080);
        Assert.Equal(new[] { 0, 720, 1920, 360 }, viewport);
    }

    [Theory]
    [InlineData(PreviewPosition.Center)]
    [InlineData(PreviewPosition.LeftBottom)]
    [InlineData(PreviewPosition.RightBottom)]
    [InlineData(PreviewPosition.TopCenter)]
    public void GetScaleForPosition_ReturnsOne(PreviewPosition position)
    {
        var scale = PreviewPositionHelper.GetScaleForPosition(position);
        Assert.Equal(1.0f, scale);
    }

    [Fact]
    public void GetCameraOffsetForPosition_ReturnsDefaultOffset()
    {
        var offset = PreviewPositionHelper.GetCameraOffsetForPosition(PreviewPosition.Center);
        Assert.Equal(0, offset.X);
        Assert.Equal(3, offset.Y);
        Assert.Equal(8, offset.Z);
    }

    [Fact]
    public void CalculateViewport_Center_ReturnsFullScreenDimensions()
    {
        var (x, y, width, height) = PreviewPositionHelper.CalculateViewport(PreviewPosition.Center, 1920, 1080);
        
        Assert.Equal(0, x);
        Assert.Equal(0, y);
        Assert.Equal(1920, width);
        Assert.Equal(1080, height);
    }

    [Fact]
    public void CalculateViewport_LeftBottom_ReturnsCorrectDimensions()
    {
        var (x, y, width, height) = PreviewPositionHelper.CalculateViewport(PreviewPosition.LeftBottom, 1920, 1080);
        
        Assert.Equal(0, x);
        Assert.Equal(0, y);
        Assert.Equal(640, width);
        Assert.Equal(360, height);
    }

    [Fact]
    public void CalculateViewport_RightBottom_ReturnsCorrectDimensions()
    {
        var (x, y, width, height) = PreviewPositionHelper.CalculateViewport(PreviewPosition.RightBottom, 1920, 1080);
        
        Assert.Equal(1280, x);
        Assert.Equal(0, y);
        Assert.Equal(640, width);
        Assert.Equal(360, height);
    }

    [Fact]
    public void GetConfig_ValidPosition_ReturnsConfiguration()
    {
        var config = PreviewPositionHelper.GetConfig(PreviewPosition.Center);
        
        Assert.Equal(0.0f, config.XRatio);
        Assert.Equal(0.0f, config.YRatio);
        Assert.Equal(1.0f, config.WidthRatio);
        Assert.Equal(1.0f, config.HeightRatio);
        Assert.Equal(1.0f, config.Scale);
    }

    [Fact]
    public void GetConfig_InvalidPosition_ReturnsCenterAsDefault()
    {
        var config = PreviewPositionHelper.GetConfig((PreviewPosition)999);
        
        // Should fallback to Center configuration
        Assert.Equal(1.0f, config.WidthRatio);
        Assert.Equal(1.0f, config.HeightRatio);
    }

    [Fact]
    public void SetCustomConfig_UpdatesConfiguration()
    {
        var customConfig = new ViewportConfig(
            XRatio: 0.25f,
            YRatio: 0.25f,
            WidthRatio: 0.5f,
            HeightRatio: 0.5f,
            CameraOffset: new Vec3(1, 2, 3),
            Scale: 2.0f
        );

        try
        {
            PreviewPositionHelper.SetCustomConfig(PreviewPosition.Center, customConfig);
            var config = PreviewPositionHelper.GetConfig(PreviewPosition.Center);
            
            Assert.Equal(0.25f, config.XRatio);
            Assert.Equal(0.25f, config.YRatio);
            Assert.Equal(0.5f, config.WidthRatio);
            Assert.Equal(0.5f, config.HeightRatio);
            Assert.Equal(2.0f, config.Scale);
            Assert.Equal(1, config.CameraOffset.X);
            Assert.Equal(2, config.CameraOffset.Y);
            Assert.Equal(3, config.CameraOffset.Z);
        }
        finally
        {
            // Restore original configuration
            PreviewPositionHelper.SetCustomConfig(PreviewPosition.Center, new ViewportConfig(
                0.0f, 0.0f, 1.0f, 1.0f, new Vec3(0, 3, 8), 1.0f
            ));
        }
    }

    [Fact]
    public void SetCustomConfig_AffectsCalculateViewport()
    {
        var customConfig = new ViewportConfig(
            XRatio: 0.5f,
            YRatio: 0.5f,
            WidthRatio: 0.25f,
            HeightRatio: 0.25f,
            CameraOffset: new Vec3(0, 3, 8),
            Scale: 1.0f
        );

        try
        {
            PreviewPositionHelper.SetCustomConfig(PreviewPosition.Center, customConfig);
            var (x, y, width, height) = PreviewPositionHelper.CalculateViewport(PreviewPosition.Center, 1000, 1000);
            
            Assert.Equal(500, x);
            Assert.Equal(500, y);
            Assert.Equal(250, width);
            Assert.Equal(250, height);
        }
        finally
        {
            // Restore original
            PreviewPositionHelper.SetCustomConfig(PreviewPosition.Center, new ViewportConfig(
                0.0f, 0.0f, 1.0f, 1.0f, new Vec3(0, 3, 8), 1.0f
            ));
        }
    }
}
