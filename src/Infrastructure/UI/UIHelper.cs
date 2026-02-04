using OpenTK.Mathematics;
using WipeoutRewrite.Infrastructure.Graphics;
using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Infrastructure.UI;

/// <summary>
/// Static UI helper methods for text rendering and positioning.
/// Equivalent to wipeout-rewrite ui.c - provides low-level UI primitives.
/// </summary>
public static class UIHelper
{
    private static IFontSystem? _fontSystem;
    private static IRenderer? _renderer;
    private static int _windowWidth;
    private static int _windowHeight;
    private static int _uiScale = 2;  // Default UI scale (matching C's ui_scale = 2)

    public static void Initialize(IFontSystem fontSystem, IRenderer renderer, int width, int height)
    {
        _fontSystem = fontSystem;
        _renderer = renderer;
        _windowWidth = width;
        _windowHeight = height;
    }

    public static void SetWindowSize(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;
    }

    public static void SetUIScale(int scale)
    {
        _uiScale = scale;
    }

    public static int GetUIScale()
    {
        return _uiScale;
    }

    /// <summary>
    /// Get text width in pixels for given size.
    /// </summary>
    public static int GetTextWidth(string text, int size)
    {
        if (_fontSystem == null || string.IsNullOrEmpty(text))
            return 0;

        TextSize textSize = size switch
        {
            >= 16 => TextSize.Size16,
            >= 12 => TextSize.Size12,
            _ => TextSize.Size8
        };
        return _fontSystem.GetTextWidth(text, textSize);
    }

    /// <summary>
    /// Convert UI position with anchor to screen coordinates.
    /// Equivalent to ui_scaled_pos() in C.
    /// </summary>
    /// <summary>
    /// Get scaled screen position from anchor point and offset.
    /// Equivalent to ui_scaled_pos() in C - IMPORTANT: offsets are multiplied by ui_scale!
    /// </summary>
    public static Vec2i ScaledPos(UIAnchor anchor, Vec2i offset)
    {
        int baseX = anchor switch
        {
            UIAnchor.TopLeft or UIAnchor.MiddleLeft or UIAnchor.BottomLeft => 0,
            UIAnchor.TopCenter or UIAnchor.MiddleCenter or UIAnchor.BottomCenter => _windowWidth / 2,
            UIAnchor.TopRight or UIAnchor.MiddleRight or UIAnchor.BottomRight => _windowWidth,
            _ => 0
        };

        int baseY = anchor switch
        {
            UIAnchor.TopLeft or UIAnchor.TopCenter or UIAnchor.TopRight => 0,
            UIAnchor.MiddleLeft or UIAnchor.MiddleCenter or UIAnchor.MiddleRight => _windowHeight / 2,
            UIAnchor.BottomLeft or UIAnchor.BottomCenter or UIAnchor.BottomRight => _windowHeight,
            _ => 0
        };

        // CRITICAL: offset is multiplied by ui_scale (like C code)
        return new Vec2i(baseX + offset.X * _uiScale, baseY + offset.Y * _uiScale);
    }

    /// <summary>
    /// Draw text at position with size and color.
    /// Equivalent to ui_draw_text() in C.
    /// </summary>
    public static void DrawText(string text, Vec2i pos, int size, UIColor color)
    {
        if (_fontSystem == null || _renderer == null || string.IsNullOrEmpty(text))
            return;

        TextSize textSize = size switch
        {
            >= 16 => TextSize.Size16,
            >= 12 => TextSize.Size12,
            _ => TextSize.Size8
        };

        Color4 glColor = GetColor4(color);
        Vector2 position = new(pos.X, pos.Y);
        _fontSystem.DrawText(_renderer, text, position, textSize, glColor);
    }

    /// <summary>
    /// Draw text centered horizontally at position.
    /// Equivalent to ui_draw_text_centered() in C.
    /// </summary>
    /// <summary>
    /// Draw text centered at position.
    /// Equivalent to ui_draw_text_centered() in C.
    /// </summary>
    public static void DrawTextCentered(string text, Vec2i pos, int size, UIColor color)
    {
        // CRITICAL: GetTextWidth returns unscaled width, FontSystem.DrawText will scale it
        // So we need to get the SCALED width for centering calculation
        // Formula from C: pos.x -= (ui_text_width(text, size) * ui_scale) >> 1;
        int textWidth = GetTextWidth(text, size);
        int scaledWidth = textWidth * _uiScale;
        Vec2i centeredPos = new(pos.X - scaledWidth / 2, pos.Y);
        DrawText(text, centeredPos, size, color);
    }

    /// <summary>
    /// Draw number at position.
    /// Equivalent to ui_draw_number() in C.
    /// </summary>
    public static void DrawNumber(int number, Vec2i pos, int size, UIColor color)
    {
        DrawText(number.ToString(), pos, size, color);
    }

    private static Color4 GetColor4(UIColor color)
    {
        return color switch
        {
            UIColor.Accent => UIConstants.Colors.MenuItemSelected,
            UIColor.Disabled => UIConstants.Colors.MenuItemDisabled,
            _ => UIConstants.Colors.MenuItemDefault
        };
    }
    
}

