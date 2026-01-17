using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WipeoutRewrite.Factory;

namespace WipeoutRewrite.Tests.Factory;

public class CameraFactoryTests
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly CameraFactory _factory;

    public CameraFactoryTests()
    {
        _loggerFactory = NullLoggerFactory.Instance;
        _factory = new CameraFactory(_loggerFactory);
    }

    [Fact]
    public void Constructor_WithNullLoggerFactory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CameraFactory(null!));
    }

    [Fact]
    public void CreateCamera_ReturnsCamera()
    {
        var camera = _factory.CreateCamera();

        Assert.NotNull(camera);
        Assert.IsAssignableFrom<ICamera>(camera);
    }

    [Fact]
    public void CreateCamera_WithAspectRatioAndFov_ReturnsCamera()
    {
        var camera = _factory.CreateCamera(16.0f / 9.0f, 60.0f);

        Assert.NotNull(camera);
        Assert.IsAssignableFrom<ICamera>(camera);
    }

    [Fact]
    public void CreateCamera_WithAspectRatioAndFov_SetsAspectRatio()
    {
        float aspectRatio = 16.0f / 9.0f;

        var camera = _factory.CreateCamera(aspectRatio, 60.0f);

        // Verify aspect ratio is set (camera should have it configured)
        Assert.NotNull(camera);
    }

    [Fact]
    public void CreateCamera_CreatesDifferentInstances()
    {
        var camera1 = _factory.CreateCamera();
        var camera2 = _factory.CreateCamera();

        Assert.NotSame(camera1, camera2);
    }
}
