using System;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.Assets;

namespace WipeoutRewrite.Infrastructure.UI
{
    public enum TextSize
    {
        Size16 = 0,
        Size12 = 1,
        Size8 = 2
    }

    public struct Glyph
    {
        public Vector2i Offset;
        public int Width;

        public Glyph(int offsetX, int offsetY, int width)
        {
            Offset = new Vector2i(offsetX, offsetY);
            Width = width;
        }
    }

    public class CharSet
    {
        public int Texture { get; set; }
        public int Height { get; set; }
        public Glyph[] Glyphs { get; set; } = new Glyph[40];
    }

    public class FontSystem : IFontSystem
    {
        private readonly ILogger<FontSystem> _logger;
        private readonly CmpImageLoader _cmpLoader;
        private readonly TimImageLoader _timLoader;
        private readonly CharSet[] _charSets = new CharSet[3];
        private bool _loaded = false;

        public FontSystem(ILogger<FontSystem> logger, CmpImageLoader cmpLoader, TimImageLoader timLoader)
        {
            _logger = logger;
            _cmpLoader = cmpLoader;
            _timLoader = timLoader;
            // Initialize char sets with data from C code
            _charSets[(int)TextSize.Size16] = new CharSet
            {
                Height = 16,
                Glyphs = new Glyph[]
                {
                    new(  0,   0, 25), new( 25,   0, 24), new( 49,   0, 17), new( 66,   0, 24), new( 90,   0, 24), new(114,   0, 17), new(131,   0, 25), new(156,   0, 18),
                    new(174,   0,  7), new(181,   0, 17), new(  0,  16, 17), new( 17,  16, 17), new( 34,  16, 28), new( 62,  16, 17), new( 79,  16, 24), new(103,  16, 24),
                    new(127,  16, 26), new(153,  16, 24), new(177,  16, 18), new(195,  16, 17), new(  0,  32, 17), new( 17,  32, 17), new( 34,  32, 29), new( 63,  32, 24),
                    new( 87,  32, 17), new(104,  32, 18), new(122,  32, 24), new(146,  32, 10), new(156,  32, 18), new(174,  32, 17), new(191,  32, 18), new(  0,  48, 18),
                    new( 18,  48, 18), new( 36,  48, 18), new( 54,  48, 22), new( 76,  48, 25), new(101,  48,  7), new(108,  48,  7), new(198,   0,  0), new(198,   0,  0)
                }
            };

            _charSets[(int)TextSize.Size12] = new CharSet
            {
                Height = 12,
                Glyphs = new Glyph[]
                {
                    new(  0,   0, 19), new( 19,   0, 19), new( 38,   0, 14), new( 52,   0, 19), new( 71,   0, 19), new( 90,   0, 13), new(103,   0, 19), new(122,   0, 14),
                    new(136,   0,  6), new(142,   0, 13), new(155,   0, 14), new(169,   0, 14), new(  0,  12, 22), new( 22,  12, 14), new( 36,  12, 19), new( 55,  12, 18),
                    new( 73,  12, 20), new( 93,  12, 19), new(112,  12, 15), new(127,  12, 14), new(141,  12, 13), new(154,  12, 13), new(167,  12, 22), new(  0,  24, 19),
                    new( 19,  24, 13), new( 32,  24, 14), new( 46,  24, 19), new( 65,  24,  8), new( 73,  24, 15), new( 88,  24, 13), new(101,  24, 14), new(115,  24, 15),
                    new(130,  24, 14), new(144,  24, 15), new(159,  24, 18), new(177,  24, 19), new(196,  24,  5), new(201,  24,  5), new(183,   0,  0), new(183,   0,  0)
                }
            };

            _charSets[(int)TextSize.Size8] = new CharSet
            {
                Height = 8,
                Glyphs = new Glyph[]
                {
                    new(  0,   0, 13), new( 13,   0, 13), new( 26,   0, 10), new( 36,   0, 13), new( 49,   0, 13), new( 62,   0,  9), new( 71,   0, 13), new( 84,   0, 10),
                    new( 94,   0,  4), new( 98,   0,  9), new(107,   0, 10), new(117,   0, 10), new(127,   0, 16), new(143,   0, 10), new(153,   0, 13), new(166,   0, 13),
                    new(179,   0, 14), new(  0,   8, 13), new( 13,   8, 10), new( 23,   8,  9), new( 32,   8,  9), new( 41,   8,  9), new( 50,   8, 16), new( 66,   8, 14),
                    new( 80,   8,  9), new( 89,   8, 10), new( 99,   8, 13), new(112,   8,  6), new(118,   8, 11), new(129,   8, 10), new(139,   8, 10), new(149,   8, 11),
                    new(160,   8, 10), new(170,   8, 10), new(180,   8, 12), new(192,   8, 14), new(206,   8,  4), new(210,   8,  4), new(193,   0,  0), new(193,   0,  0)
                }
            };
        }

        public void LoadFonts(string assetsPath)
        {
            if (_loaded) return;

            try
            {
                string cmpPath = $"{assetsPath}/wipeout/textures/drfonts.cmp";
                _logger.LogInformation("Loading fonts from: {CmpPath}", cmpPath);

                byte[][] images = _cmpLoader.LoadCompressed(cmpPath);
                
                if (images.Length < 3)
                {
                    _logger.LogError("drfonts.cmp should have at least 3 images, got {ImageCount}", images.Length);
                    return;
                }

                // Load the 3 font textures (Size16, Size12, Size8)
                for (int i = 0; i < 3 && i < images.Length; i++)
                {
                    (byte[] pixels, int width, int height) = _timLoader.LoadTimFromBytes(images[i], false);
                    if (pixels != null)
                    {
                        _charSets[i].Texture = CreateTexture(pixels, width, height);
                        _logger.LogInformation("Loaded font texture {TextureIndex}: {Width}x{Height}", i, width, height);
                    }
                }

                _loaded = true;
                _logger.LogInformation("Fonts loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fonts");
            }
        }

        private int CreateTexture(byte[] pixels, int width, int height)
        {
            int textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 
                width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return textureId;
        }

        private int CharToGlyphIndex(char c)
        {
            if (c >= '0' && c <= '9')
                return (c - '0') + 26;
            if (c >= 'A' && c <= 'Z')
                return c - 'A';
            if (c >= 'a' && c <= 'z')
                return c - 'a'; // Convert to uppercase
            return 0; // Default to 'A'
        }

        public int GetTextWidth(string text, TextSize size)
        {
            int width = 0;
            CharSet cs = _charSets[(int)size];

            foreach (char c in text)
            {
                if (c == ' ')
                    width += 8;
                else
                    width += cs.Glyphs[CharToGlyphIndex(c)].Width;
            }

            return width;
        }

    public void DrawText(IRenderer renderer, string text, Vector2 pos, TextSize size, Color4 color)
    {
        if (!_loaded)
        {
            _logger.LogWarning("Fonts not loaded, skipping text draw");
            return;
        }

        CharSet cs = _charSets[(int)size];
        float x = pos.X;

        // Get texture dimensions based on actually loaded texture
        // From logs: Size16=212x64, Size12=208x36, Size8=216x16
        int texWidth = size == TextSize.Size16 ? 212 : (size == TextSize.Size12 ? 208 : 216);
        int texHeight = size == TextSize.Size16 ? 64 : (size == TextSize.Size12 ? 36 : 16);

        foreach (char c in text.ToUpper()) // Convert to uppercase
        {
            if (c != ' ')
            {
                Glyph glyph = cs.Glyphs[CharToGlyphIndex(c)];
                
                // Calculate UV coordinates for the glyph in the font texture
                float u0 = glyph.Offset.X / (float)texWidth;
                float v0 = glyph.Offset.Y / (float)texHeight;
                float u1 = (glyph.Offset.X + glyph.Width) / (float)texWidth;
                float v1 = (glyph.Offset.Y + cs.Height) / (float)texHeight;
                
                // Draw glyph using custom texture with UV coordinates
                if (renderer is GLRenderer glRenderer)
                {
                    glRenderer.PushSpriteWithTexture(x, pos.Y, glyph.Width, cs.Height, color, cs.Texture,
                        u0, v0, u1, v1);
                }
                x += glyph.Width;
            }
            else
            {
                x += 8; // Space width
            }
        }
    }        public void DrawTextCentered(IRenderer renderer, string text, Vector2 pos, TextSize size, Color4 color)
        {
            int width = GetTextWidth(text, size);
            pos.X -= width / 2.0f;
            DrawText(renderer, text, pos, size, color);
        }
    }
}
