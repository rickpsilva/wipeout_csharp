using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Infrastructure.UI;

public class MenuRenderer : IMenuRenderer
{
    private readonly int _windowWidth;
    private readonly int _windowHeight;
    private readonly IRenderer _renderer;
    private readonly IFontSystem? _fontSystem;
    
    public MenuRenderer(int windowWidth, int windowHeight, IRenderer renderer, IFontSystem? fontSystem = null)
    {
        _windowWidth = windowWidth;
        _windowHeight = windowHeight;
        _renderer = renderer;
        _fontSystem = fontSystem;
    }
    
    public void RenderMenu(IMenuManager menu)
    {
        var page = menu.CurrentPage;
        if (page == null)
            return;
        
        // Draw title
        DrawTextCentered(page.Title, GetScaledPosition(page.TitleAnchor, page.TitlePos), 16, UIColor.Accent);
        
        // Draw items
        int itemY = page.ItemsPos.Y;
        for (int i = 0; i < page.Items.Count; i++)
        {
            var item = page.Items[i];
            bool isSelected = i == page.SelectedIndex;
            var color = isSelected && menu.ShouldBlink() ? UIColor.Accent : UIColor.Default;
            
            if (!item.IsEnabled)
                color = UIColor.Disabled;
            
            Vec2i itemPos = new Vec2i(page.ItemsPos.X, itemY);
            
            if (item is MenuButton button)
            {
                DrawText(button.Label, GetScaledPosition(page.ItemsAnchor, itemPos), page.ItemsAnchor, 12, color);
            }
            else if (item is MenuToggle toggle)
            {
                string label = $"{toggle.Label}: {toggle.CurrentValue}";
                DrawText(label, GetScaledPosition(page.ItemsAnchor, itemPos), page.ItemsAnchor, 12, color);
            }
            
            itemY += page.LayoutFlags.HasFlag(MenuLayoutFlags.Vertical) ? 24 : 0;
        }
        
        // Custom draw callback for 3D models, etc.
        page.DrawCallback?.Invoke(0f);
    }
    
    public void DrawText(string text, Vec2i position, UIAnchor anchor, int size, UIColor color)
    {
        if (_fontSystem != null)
        {
            // Use proper font rendering
            TextSize textSize = size switch
            {
                16 => TextSize.Size16,
                12 => TextSize.Size12,
                _ => TextSize.Size8
            };
            
            var glColor = GetColor4(color);
            Vector2 pos = new Vector2(position.X, position.Y);
            _fontSystem.DrawText(_renderer, text, pos, textSize, glColor);
        }
        else
        {
            // Fallback: Draw rectangles (old method)
            var glColor = GetGLColor(color);
            float charWidth = size * 1.2f;
            float charHeight = size * 1.5f;
            
            float x = position.X;
            float y = position.Y;
            
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != ' ')
                {
                    _renderer.PushSprite(x, y, charWidth, charHeight, glColor);
                }
                x += charWidth + 4;
            }
        }
    }
    
    public void DrawTextCentered(string text, Vec2i position, int size, UIColor color)
    {
        int textWidth = GetTextWidth(text, size);
        Vec2i centeredPos = new Vec2i(position.X - textWidth / 2, position.Y);
        DrawText(text, centeredPos, UIAnchor.TopLeft, size, color);
    }
    
    public int GetTextWidth(string text, int size)
    {
        if (_fontSystem != null)
        {
            TextSize textSize = size switch
            {
                16 => TextSize.Size16,
                12 => TextSize.Size12,
                _ => TextSize.Size8
            };
            return _fontSystem.GetTextWidth(text, textSize);
        }
        
        // Fallback: Approximate
        return (int)(text.Length * size * 0.6f);
    }
    
    private Vec2i GetScaledPosition(UIAnchor anchor, Vec2i offset)
    {
        int baseX = 0, baseY = 0;
        
        switch (anchor)
        {
            case UIAnchor.TopLeft:
                baseX = 0;
                baseY = 0;
                break;
            case UIAnchor.TopCenter:
                baseX = _windowWidth / 2;
                baseY = 0;
                break;
            case UIAnchor.TopRight:
                baseX = _windowWidth;
                baseY = 0;
                break;
            case UIAnchor.MiddleLeft:
                baseX = 0;
                baseY = _windowHeight / 2;
                break;
            case UIAnchor.MiddleCenter:
                baseX = _windowWidth / 2;
                baseY = _windowHeight / 2;
                break;
            case UIAnchor.MiddleRight:
                baseX = _windowWidth;
                baseY = _windowHeight / 2;
                break;
            case UIAnchor.BottomLeft:
                baseX = 0;
                baseY = _windowHeight;
                break;
            case UIAnchor.BottomCenter:
                baseX = _windowWidth / 2;
                baseY = _windowHeight;
                break;
            case UIAnchor.BottomRight:
                baseX = _windowWidth;
                baseY = _windowHeight;
                break;
        }
        
        return new Vec2i(baseX + offset.X, baseY + offset.Y);
    }
    
    private Vector4 GetGLColor(UIColor color)
    {
        return color switch
        {
            UIColor.Accent => new Vector4(1.0f, 0.8f, 0.0f, 1.0f),  // Yellow/gold
            UIColor.Disabled => new Vector4(0.5f, 0.5f, 0.5f, 1.0f), // Gray
            _ => new Vector4(1.0f, 1.0f, 1.0f, 1.0f)  // White
        };
    }
    
    private Color4 GetColor4(UIColor color)
    {
        return color switch
        {
            UIColor.Accent => new Color4(1.0f, 0.8f, 0.0f, 1.0f),  // Yellow/gold
            UIColor.Disabled => new Color4(0.5f, 0.5f, 0.5f, 1.0f), // Gray
            _ => new Color4(1.0f, 1.0f, 1.0f, 1.0f)  // White
        };
    }
}
