namespace WipeoutRewrite.Factory;

/// <summary>
/// Factory for creating camera instances.
/// Allows multiple camera creation while maintaining dependency injection.
/// </summary>
public interface ICameraFactory
{
    /// <summary>
    /// Creates a new camera instance with default settings.
    /// </summary>
    ICamera CreateCamera();
    /// <summary>
    /// Creates a new camera instance with custom viewport settings.
    /// </summary>
    ICamera CreateCamera(float aspectRatio, float fov);
}