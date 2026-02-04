using System;

namespace WipeoutRewrite.Infrastructure.Graphics
{
    public interface ITextureManager
    {
        // Create an OpenGL texture from raw RGBA pixels
        int CreateTexture(byte[] pixels, int width, int height);

        // Load a compressed CMP file (array of TIM images) and create textures
        // Returns an array of texture handles
        int[] LoadTexturesFromCmp(string cmpPath);

        // Load a single TIM file and create a texture
        // Returns the texture handle
        int LoadTextureFromTim(string timPath);

        // Returns true if the given GL texture handle contains any alpha<255 pixels
        bool HasAlpha(int textureHandle);

        // Texture alpha mode: whether texture is fully opaque, contains binary (cutout)
        // alpha or contains fractional alpha values (translucent).
        TextureAlphaMode GetTextureAlphaMode(int textureHandle);

        // Get the size of a texture for UV normalization
        (int width, int height) GetTextureSize(int textureHandle);
    }

    public enum TextureAlphaMode
    {
        Opaque = 0,
        Cutout = 1,
        Translucent = 2
    }
}
