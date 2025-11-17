namespace WipeoutRewrite.Infrastructure.Audio;

/// <summary>
/// Interface para reprodução de áudio.
/// Permite trocar implementações (OpenAL, FMOD, XAudio2) e facilita testes.
/// </summary>
public interface IAudioPlayer
{
    /// <summary>
    /// Carrega arquivo de áudio WAV.
    /// </summary>
    /// <returns>True se carregado com sucesso.</returns>
    bool LoadWav(string wavPath);
    
    /// <summary>
    /// Inicia reprodução do áudio.
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
