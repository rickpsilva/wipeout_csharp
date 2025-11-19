using System;

namespace WipeoutRewrite.Infrastructure.Audio;

/// <summary>
/// Interface for music playback management.
/// </summary>
public interface IMusicPlayer : IDisposable
{
    /// <summary>
    /// Loads track list from directory.
    /// </summary>
    void LoadTracks(string musicPath);

    /// <summary>
    /// Sets playback mode.
    /// </summary>
    void SetMode(MusicMode mode);

    /// <summary>
    /// Plays a random track.
    /// </summary>
    void PlayRandomTrack();

    /// <summary>
    /// Plays a specific track by index.
    /// </summary>
    void PlayTrack(int index);

    /// <summary>
    /// Stops current playback.
    /// </summary>
    void Stop();

    /// <summary>
    /// Atualiza o estado do player (deve ser chamado no game loop).
    /// </summary>
    void Update(float deltaTime);
}
