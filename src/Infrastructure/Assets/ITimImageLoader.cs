namespace WipeoutRewrite.Infrastructure.Assets
{
    /// <summary>
    /// Interface para carregamento de assets.
    /// Allows swapping implementations (disk, network, embedded) and facilitates testing.
    /// </summary>
    public interface ITimImageLoader
    {
        /// <summary>
        /// Loads TIM image from file.
        /// param name="filePath">Path to TIM file.</param>
        /// param name="transparent">Whether to treat a specific color as transparent.</param>
        /// <returns>Tuple of pixel data (RGBA), width, and height.</returns>    
        /// </summary>
        (byte[] pixels, int width, int height) LoadTim(string filePath, bool transparent = false);

        /// <summary>
        /// Loads TIM image from byte array.
        /// param name="bytes">Byte array containing TIM data.</param>
        /// param name="transparent">Whether to treat a specific color as transparent.</param>
        /// <returns>Tuple of pixel data (RGBA), width, and height.</returns>
        /// </summary>
        (byte[] pixels, int width, int height) LoadTimFromBytes(byte[] bytes, bool transparent = false);
    }
}