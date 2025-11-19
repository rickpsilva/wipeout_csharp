namespace WipeoutRewrite.Infrastructure.Assets;

/// <summary>
/// Interface para carregamento de assets.
/// Allows swapping implementations (disk, network, embedded) and facilitates testing.
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
    /// Loads binary file.
    /// </summary>
    byte[]? LoadBinaryFile(string relativePath);
    
    /// <summary>
    /// Lista arquivos em uma pasta.
    /// </summary>
    List<string> ListFiles(string relativePath, string pattern = "*");
    
    /// <summary>
    /// Loads available track list.
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
