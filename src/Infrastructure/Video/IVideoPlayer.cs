namespace WipeoutRewrite.Infrastructure.Video;

/// <summary>
/// Interface para reprodução de vídeo.
/// Permite trocar implementações (FFmpeg, LibVLC, MediaFoundation) e facilita testes.
/// </summary>
public interface IVideoPlayer : IDisposable
{
    /// <summary>
    /// Verifica se o vídeo está tocando.
    /// </summary>
    bool IsPlaying { get; }
    
    /// <summary>
    /// Inicia reprodução do vídeo.
    /// </summary>
    void Play();
    
    /// <summary>
    /// Para reprodução do vídeo (skip).
    /// </summary>
    void Skip();
    
    /// <summary>
    /// Atualiza frame do vídeo (deve ser chamado a cada frame do jogo).
    /// </summary>
    void Update();
    
    /// <summary>
    /// Retorna ID da textura OpenGL do frame atual.
    /// </summary>
    int GetTextureId();
    
    /// <summary>
    /// Retorna largura do vídeo.
    /// </summary>
    int GetWidth();
    
    /// <summary>
    /// Retorna altura do vídeo.
    /// </summary>
    int GetHeight();
}
