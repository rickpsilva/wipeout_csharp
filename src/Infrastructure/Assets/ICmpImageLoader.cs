namespace WipeoutRewrite.Infrastructure.Assets
{
    /// <summary>
    /// Interface para carregamento de assets.
    /// Allows swapping implementations (disk, network, embedded) and facilitates testing.
    /// </summary>
    public interface ICmpImageLoader
    {
        /// <summary>
        /// Loads and decompresses CMP image file.
        /// param name="filePath">Path to CMP file.</param>
        /// returns>Array of decompressed images as byte arrays.</returns>
        /// </summary>  
        byte[][] LoadCompressed(string filePath);
    }
}