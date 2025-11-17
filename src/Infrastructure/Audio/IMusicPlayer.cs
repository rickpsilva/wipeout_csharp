using System;

namespace WipeoutRewrite.Infrastructure.Audio;

/// <summary>
/// Interface para gestão de reprodução de música.
/// </summary>
public interface IMusicPlayer : IDisposable
{
    /// <summary>
    /// Carrega lista de faixas de um diretório.
    /// </summary>
    void LoadTracks(string musicPath);

    /// <summary>
    /// Define o modo de reprodução.
    /// </summary>
    void SetMode(MusicMode mode);

    /// <summary>
    /// Reproduz uma faixa aleatória.
    /// </summary>
    void PlayRandomTrack();

    /// <summary>
    /// Reproduz uma faixa específica pelo índice.
    /// </summary>
    void PlayTrack(int index);

    /// <summary>
    /// Para a reprodução atual.
    /// </summary>
    void Stop();

    /// <summary>
    /// Atualiza o estado do player (deve ser chamado no game loop).
    /// </summary>
    void Update(float deltaTime);
}
