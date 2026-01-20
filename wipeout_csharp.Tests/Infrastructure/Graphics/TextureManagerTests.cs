using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.Graphics;
using Xunit;

namespace WipeoutRewrite.Tests.Infrastructure.Graphics;

public class TextureManagerTests
{
    private static TextureManager CreateTextureManager(
        out Mock<ICmpImageLoader> cmpLoader,
        out Mock<ITimImageLoader> timLoader,
        out Mock<ILogger<TextureManager>> logger)
    {
        cmpLoader = new Mock<ICmpImageLoader>(MockBehavior.Strict);
        timLoader = new Mock<ITimImageLoader>(MockBehavior.Strict);
        logger = new Mock<ILogger<TextureManager>>();
        return new TextureManager(logger.Object, cmpLoader.Object, timLoader.Object);
    }

    private static T GetPrivateField<T>(TextureManager manager, string fieldName)
    {
        var field = typeof(TextureManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        var value = field!.GetValue(manager);
        Assert.NotNull(value);
        return (T)value;
    }

    [Fact]
    public void LoadTexturesFromCmp_WithCacheHit_ReturnsCachedHandles()
    {
        // Arrange
        var manager = CreateTextureManager(out var cmpLoader, out _, out _);
        var cache = GetPrivateField<Dictionary<string, int[]>>(manager, "_cmpCache");
        var handles = new[] { 10, 20, 30 };
        cache["/tmp/allsh.cmp"] = handles;

        // Act
        var result = manager.LoadTexturesFromCmp("/tmp/allsh.cmp");

        // Assert
        Assert.Same(handles, result);
        cmpLoader.Verify(x => x.LoadCompressed(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void LoadTexturesFromCmp_WithEmptyImages_ReturnsEmptyArray()
    {
        // Arrange
        var manager = CreateTextureManager(out var cmpLoader, out var timLoader, out _);
        cmpLoader.Setup(x => x.LoadCompressed("/tmp/empty.cmp")).Returns(Array.Empty<byte[]>());

        // Act
        var result = manager.LoadTexturesFromCmp("/tmp/empty.cmp");

        // Assert
        Assert.Empty(result);
        cmpLoader.Verify(x => x.LoadCompressed("/tmp/empty.cmp"), Times.Once);
        timLoader.VerifyNoOtherCalls();
    }

    [Fact]
    public void LoadTexturesFromCmp_WhenLoaderThrows_ReturnsEmptyArray()
    {
        // Arrange
        var manager = CreateTextureManager(out var cmpLoader, out _, out _);
        cmpLoader.Setup(x => x.LoadCompressed("/tmp/fail.cmp")).Throws(new InvalidOperationException("boom"));

        // Act
        var result = manager.LoadTexturesFromCmp("/tmp/fail.cmp");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Getters_ReturnStoredValuesOrDefaults()
    {
        // Arrange
        var manager = CreateTextureManager(out _, out _, out _);
        var alphaModes = GetPrivateField<Dictionary<int, TextureAlphaMode>>(manager, "_textureAlphaMode");
        var sizes = GetPrivateField<Dictionary<int, (int width, int height)>>(manager, "_textureSizes");
        var hasAlpha = GetPrivateField<Dictionary<int, bool>>(manager, "_textureHasAlpha");

        alphaModes[5] = TextureAlphaMode.Cutout;
        sizes[5] = (64, 32);
        hasAlpha[5] = true;

        // Act + Assert
        Assert.Equal(TextureAlphaMode.Cutout, manager.GetTextureAlphaMode(5));
        Assert.Equal((64, 32), manager.GetTextureSize(5));
        Assert.True(manager.HasAlpha(5));

        Assert.Equal(TextureAlphaMode.Opaque, manager.GetTextureAlphaMode(999));
        Assert.Equal((256, 256), manager.GetTextureSize(999));
        Assert.False(manager.HasAlpha(999));
    }
}
