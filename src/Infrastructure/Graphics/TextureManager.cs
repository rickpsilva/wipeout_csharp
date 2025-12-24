    
using System;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using WipeoutRewrite.Infrastructure.Assets;

namespace WipeoutRewrite.Infrastructure.Graphics
{
    public class TextureManager : ITextureManager
    {
        private readonly ILogger<TextureManager> _logger;
        private readonly ICmpImageLoader _cmpLoader;
        private readonly ITimImageLoader _timLoader;

        // Track whether created textures contain non-opaque alpha
        private readonly Dictionary<int, bool> _textureHasAlpha = new();
        // Track alpha modes for created textures
        private readonly Dictionary<int, TextureAlphaMode> _textureAlphaMode = new();
        // Track texture sizes (width, height) for UV normalization
        private readonly Dictionary<int, (int width, int height)> _textureSizes = new();
        // Cache CMP file texture handles to avoid loading same CMP multiple times
        private readonly Dictionary<string, int[]> _cmpCache = new();

        public TextureManager(
            ILogger<TextureManager> logger, 
            ICmpImageLoader cmpLoader,
            ITimImageLoader timLoader)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cmpLoader = cmpLoader ?? throw new ArgumentNullException(nameof(cmpLoader));
            _timLoader = timLoader ?? throw new ArgumentNullException(nameof(timLoader));
        }

        public int CreateTexture(byte[] pixels, int width, int height)
        {
            int textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // Detect if texture contains alpha != 255 anywhere and whether alpha is
            // binary (cutout) or fractional (translucent).
            bool hasAlpha = false;
            bool hasFractionalAlpha = false;
            if (pixels != null && pixels.Length >= 4)
            {
                for (int i = 3; i < pixels.Length; i += 4)
                {
                    byte a = pixels[i];
                    if (a != 0xFF)
                    {
                        hasAlpha = true;
                        if (a != 0x00) hasFractionalAlpha = true;
                    }
                }
            }
            _textureHasAlpha[textureId] = hasAlpha;
            var mode = TextureAlphaMode.Opaque;
            if (hasAlpha)
            {
                mode = hasFractionalAlpha ? TextureAlphaMode.Translucent : TextureAlphaMode.Cutout;
            }
            _textureAlphaMode[textureId] = mode;
            _textureSizes[textureId] = (width, height);
            _logger?.LogInformation("Created texture {TextureId} ({Width}x{Height}) hasAlpha={HasAlpha} alphaMode={Mode}", textureId, width, height, hasAlpha, mode);
            _logger?.LogDebug("CreateTexture: pixels length={Len}", pixels?.Length ?? 0);

            return textureId;
        }

        public int[] LoadTexturesFromCmp(string cmpPath)
        {
            // Check cache first - reuse textures if this CMP was already loaded
            if (_cmpCache.TryGetValue(cmpPath, out var cachedHandles))
            {
                _logger?.LogInformation("Reusing cached textures from CMP: {CmpPath} ({Count} textures)", 
                    cmpPath, cachedHandles.Length);
                return cachedHandles;
            }
            
            try
            {
                _logger?.LogInformation("Attempting to load CMP {CmpPath}", cmpPath);
                byte[][] images = _cmpLoader.LoadCompressed(cmpPath);
                if (images == null || images.Length == 0)
                {
                    _logger?.LogWarning("No images found in CMP: {CmpPath}", cmpPath);
                    return Array.Empty<int>();
                }

                _logger?.LogInformation("CMP contains {Count} images", images.Length);
                int[] handles = new int[images.Length];
                for (int i = 0; i < images.Length; i++)
                {
                    _logger?.LogDebug("Loading TIM #{Index} from CMP (byte length={Len})", i, images[i]?.Length ?? 0);
                    
                    // WORKAROUND: TIM 10 and TIM 11 are identical duplicates (same exhaust texture).
                    // TIM 11 causes rendering overlap issues with wing texture (TIM 49).
                    // Skip loading TIM 11 and replace with a fully transparent dummy texture.
                    if (i == 11)
                    {
                        _logger?.LogWarning("Skipping TIM 11 (duplicate of TIM 10) - replacing with transparent dummy to avoid overlap");
                        byte[] dummyPixels = new byte[4 * 4 * 4]; // 4x4 fully transparent
                        for (int p = 0; p < dummyPixels.Length; p += 4)
                        {
                            dummyPixels[p + 3] = 0; // Alpha = 0 (fully transparent)
                        }
                        handles[i] = CreateTexture(dummyPixels, 4, 4);
                        continue;
                    }
                    
                    // Match C behavior: pass false for transparent parameter for CMP textures
                    // (see wipeout-rewrite/src/wipeout/image.c:image_get_compressed_textures)
                    var (pixels, w, h) = _timLoader.LoadTimFromBytes(images[i], false);
                    
                    if (i == 9)
                    {
                        _logger?.LogWarning("TIM 9 (exhaust flame): size={W}x{H}, first pixel RGBA=({R},{G},{B},{A})", 
                            w, h, pixels[0], pixels[1], pixels[2], pixels[3]);
                    }
                    else
                    {
                        _logger?.LogInformation("TIM #{Index}: size={W}x{H}, first pixel RGBA=({R},{G},{B},{A})", 
                            i, w, h, pixels[0], pixels[1], pixels[2], pixels[3]);
                    }
                    
                    handles[i] = CreateTexture(pixels, w, h);
                    
                    if (i == 9)
                    {
                        _logger?.LogWarning("TIM 9 mapped -> GL handle {Handle}", handles[i]);
                    }
                    else
                    {
                        _logger?.LogInformation("Mapped TIM #{Index} -> GL handle {Handle}", i, handles[i]);
                    }
                }

                // Cache the handles for reuse
                _cmpCache[cmpPath] = handles;
                _logger?.LogInformation("Cached {Count} texture handles from {CmpPath}", handles.Length, cmpPath);

                return handles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load CMP textures from {CmpPath}", cmpPath);
                return Array.Empty<int>();
            }
        }

        public bool HasAlpha(int textureHandle)
        {
            if (_textureHasAlpha.TryGetValue(textureHandle, out var v)) return v;
            return false;
        }

        public TextureAlphaMode GetTextureAlphaMode(int textureHandle)
        {
            if (_textureAlphaMode.TryGetValue(textureHandle, out var v)) return v;
            return TextureAlphaMode.Opaque;
        }

        public (int width, int height) GetTextureSize(int textureHandle)
        {
            if (_textureSizes.TryGetValue(textureHandle, out var size)) return size;
            return (256, 256); // Default PSX texture size
        }
    }
}
