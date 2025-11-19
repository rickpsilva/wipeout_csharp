namespace WipeoutRewrite.Infrastructure.Audio;

/// <summary>
/// Interface for audio playback.
/// Allows swapping implementations (OpenAL, FMOD, XAudio2) and facilitates testing.
/// </summary>
public interface IAudioPlayer
{
    /// <summary>
    /// Loads WAV audio file.
    /// </summary>
    /// <returns>True se carregado com sucesso.</returns>
    bool LoadWav(string wavPath);
    
    /// <summary>
    /// Starts audio playback.
    /// </summary>
    void Play();
    
    /// <summary>
    /// Para reprodução do áudio.
    /// </summary>
    void Stop();
    
    /// <summary>
    /// Pausa reprodução do áudio.
    /// </summary>
    void Pause();
    
    /// <summary>
    /// Verifica se o áudio está tocando.
    /// </summary>
    bool IsPlaying();
    
    /// <summary>
    /// Retorna a posição atual de playback em segundos.
    /// </summary>
    float GetPlaybackPosition();
    
    /// <summary>
    /// Define a posição de playback em segundos.
    /// </summary>
    void SetPlaybackPosition(float seconds);
}
