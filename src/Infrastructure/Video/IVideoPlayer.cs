namespace WipeoutRewrite.Infrastructure.Video;

/// <summary>
/// Interface for video playback.
/// Allows swapping implementations (FFmpeg, LibVLC, MediaFoundation) and facilitates testing.
/// </summary>
public interface IVideoPlayer : IDisposable
{
    /// <summary>
    /// Checks if video is playing.
    /// </summary>
    bool IsPlaying { get; }
    
    /// <summary>
    /// Starts video playback.
    /// </summary>
    void Play();
    
    /// <summary>
    /// Stops video playback (skip).
    /// </summary>
    void Skip();
    
    /// <summary>
    /// Updates video frame (should be called every game frame).
    /// </summary>
    void Update();
    
    /// <summary>
    /// Retorna ID da textura OpenGL do frame atual.
    /// </summary>
    int GetTextureId();
    
    /// <summary>
    /// Returns video width.
    /// </summary>
    int GetWidth();
    
    /// <summary>
    /// Returns video height.
    /// </summary>
    int GetHeight();
}
