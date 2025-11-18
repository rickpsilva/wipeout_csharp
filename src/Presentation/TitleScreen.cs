using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Infrastructure.Assets;
using WipeoutRewrite.Infrastructure.UI;

namespace WipeoutRewrite.Presentation;

public class TitleScreen
{
    private float _timer;
    private float _blinkTimer;
    private bool _attractShown;
    private const float AttractDelayFirst = 10.0f; // 10 segundos antes dos créditos
    private const float AttractDelaySubsequent = 10.0f;
    private const float BlinkInterval = 0.5f; // Pisca a cada 0.5s
    
    private int _titleTexture;
    private int _titleWidth;
    private int _titleHeight;
    private bool _textureLoaded;
    private readonly IFontSystem? _fontSystem;
    private readonly TimImageLoader _timLoader;
    
    public TitleScreen(IFontSystem? fontSystem, TimImageLoader timLoader)
    {
        _fontSystem = fontSystem;
        _timLoader = timLoader;
        _timer = 0f;
        _blinkTimer = 0f;
        _attractShown = false;
        _textureLoaded = false;
        _fontSystem = fontSystem;
        LoadTitleTexture();
    }
    
    private void LoadTitleTexture()
    {
        try
        {
            string timPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "wipeout", "textures", "wiptitle.tim");
            if (File.Exists(timPath))
            {
                var (pixels, width, height) = _timLoader.LoadTim(timPath, false);
                _titleWidth = width;
                _titleHeight = height;
                
                // Create OpenGL texture
                _titleTexture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, _titleTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                
                _textureLoaded = true;
                Console.WriteLine($"✓ Title texture loaded: {width}x{height}");
            }
            else
            {
                Console.WriteLine($"✗ Title texture not found: {timPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error loading title texture: {ex.Message}");
        }
    }
    
    public void Reset()
    {
        _timer = 0f;
        _blinkTimer = 0f;
    }
    
    public void Update(float deltaTime, out bool shouldStartAttract, out bool shouldStartMenu)
    {
        _timer += deltaTime;
        _blinkTimer += deltaTime;
        
        if (_blinkTimer >= BlinkInterval * 2)
        {
            _blinkTimer = 0f;
        }
        
        float attractDelay = _attractShown ? AttractDelaySubsequent : AttractDelayFirst;
        shouldStartAttract = _timer >= attractDelay;
        shouldStartMenu = false;
    }
    
    public void OnAttractComplete()
    {
        _attractShown = true;
        Reset();
    }
    
    public void OnStartPressed()
    {
        // Will trigger menu
    }
    
    public void Render(IRenderer renderer, int screenWidth, int screenHeight)
    {
        // Begin 2D rendering (BeginFrame já faz o Clear)
        renderer.BeginFrame();
        
        // Render wiptitle.tim texture fullscreen - SEMPRE VISÍVEL
        if (_textureLoaded)
        {
            renderer.SetCurrentTexture(_titleTexture);
            // rgba(128, 128, 128, 255) from C code
            renderer.PushSprite(0, 0, screenWidth, screenHeight, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
        }
        
        // Render "PRESS ENTER" text - PISCA SEMPRE
        if (_fontSystem != null)
        {
            // Piscar continuamente
            bool shouldShowText = _blinkTimer < BlinkInterval;
            
            if (shouldShowText)
            {
                string text = "PRESS ENTER";
                // UI_COLOR_DEFAULT = rgba(128, 128, 128, 255) = gray
                var color = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
                Vector2 pos = new Vector2(screenWidth / 2, screenHeight - 40);
                
                _fontSystem.DrawTextCentered(renderer, text, pos, TextSize.Size8, color);
            }
        }
        
        renderer.EndFrame();
    }
}
