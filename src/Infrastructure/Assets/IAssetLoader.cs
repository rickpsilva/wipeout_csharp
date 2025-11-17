namespace WipeoutRewrite.Infrastructure.Assets;

/// <summary>
/// Interface para carregamento de assets.
/// Permite trocar implementações (disco, rede, embedded) e facilita testes.
/// </summary>
public interface IAssetLoader
{
    /// <summary>
    /// Inicializa o loader com caminho base.
    /// </summary>
    void Initialize(string basePath);
    
    /// <summary>
    /// Carrega arquivo de texto.
    /// </summary>
    string? LoadTextFile(string relativePath);
    
    /// <summary>
    /// Carrega arquivo binário.
    /// </summary>
    byte[]? LoadBinaryFile(string relativePath);
    
    /// <summary>
    /// Lista arquivos em uma pasta.
    /// </summary>
    List<string> ListFiles(string relativePath, string pattern = "*");
    
    /// <summary>
    /// Carrega lista de tracks disponíveis.
    /// </summary>
    List<string> LoadTrackList();
    
    /// <summary>
    /// Carrega imagem (PNG/JPG).
    /// </summary>
    byte[]? LoadImage(string relativePath);
    
    /// <summary>
    /// Verifica se um asset existe.
    /// </summary>
    bool AssetExists(string relativePath);
}
