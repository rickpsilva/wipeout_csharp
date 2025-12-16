using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;

namespace WipeoutRewrite.Factory;

/// <summary>
/// Factory implementation for creating camera instances.
/// </summary>
public class CameraFactory : ICameraFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public CameraFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new System.ArgumentNullException(nameof(loggerFactory));
    }

    public ICamera CreateCamera()
    {
        var logger = _loggerFactory.CreateLogger<Camera>();
        return new Camera(logger);
    }

    public ICamera CreateCamera(float aspectRatio, float fov)
    {
        var logger = _loggerFactory.CreateLogger<Camera>();
        var camera = new Camera(logger);
        camera.SetAspectRatio(aspectRatio);
        camera.Fov = MathHelper.DegreesToRadians(fov);
        return camera;
    }
}