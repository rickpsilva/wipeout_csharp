using OpenTK.Mathematics;
using WipeoutRewrite.Core.Services;
using WipeoutRewrite.Infrastructure.Graphics;
using static WipeoutRewrite.Infrastructure.UI.UIConstants;

namespace WipeoutRewrite.Infrastructure.UI;

/// <summary>
/// Menu renderer - equivalent to menu.c menu_update() rendering logic.
/// Uses UIHelper for all actual drawing operations.
/// </summary>
public class MenuRenderer : IMenuRenderer
{
    private readonly IFontSystem _fontSystem;
    private readonly IRenderer _renderer;

    public MenuRenderer(IRenderer renderer, IFontSystem fontSystem)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _fontSystem = fontSystem ?? throw new ArgumentNullException(nameof(fontSystem));
    }

    public void SetWindowSize(int width, int height)
    {
        UIHelper.SetWindowSize(width, height);
        
        // Also initialize UIHelper with services if not done
        UIHelper.Initialize(_fontSystem, _renderer, width, height);
    }

    public void RenderMenu(IMenuManager menu)
    {
        var page = menu.CurrentPage;
        if (page == null)
            return;

        bool isHorizontal = page.LayoutFlags.HasFlag(MenuLayoutFlags.Horizontal);

        // Horizontal menus (confirmation dialogs) have special rendering logic
        if (isHorizontal)
        {
            RenderHorizontalMenu(page);
            page.DrawCallback?.Invoke(this);
            return;
        }

        // Calculate positions for VERTICAL menus (matching C code exactly)
        Vec2i titlePos, itemsPos;
        bool isFixed = page.LayoutFlags.HasFlag(MenuLayoutFlags.Fixed);
        
        if (!isFixed)
        {
            // Dynamic positioning for non-FIXED menus (matching C: height = 20 + entries_len * 12)
            int height = 20 + page.Items.Count * 12;
            titlePos = new Vec2i(0, -height / 2);
            itemsPos = new Vec2i(0, -height / 2 + 20);
        }
        else
        {
            // Use specified positions for FIXED menus
            titlePos = page.TitlePos;
            itemsPos = page.ItemsPos;
        }

        // Draw title if not empty (VERTICAL menus)
        if (!string.IsNullOrEmpty(page.Title))
        {
            Vec2i titleScreen = UIHelper.ScaledPos(page.TitleAnchor, titlePos);
            
            // Use centered text if AlignCenter flag is set (matching C code)
            if (page.LayoutFlags.HasFlag(MenuLayoutFlags.AlignCenter))
                UIHelper.DrawTextCentered(page.Title, titleScreen, FontSizes.MenuTitle, UIColor.Accent);
            else
                UIHelper.DrawText(page.Title, titleScreen, FontSizes.MenuTitle, UIColor.Accent);
        }

        // Draw items (VERTICAL menus)
        bool isVertical = page.LayoutFlags.HasFlag(MenuLayoutFlags.Vertical);

        int itemX = itemsPos.X;
        int itemY = itemsPos.Y;

        for (int i = 0; i < page.Items.Count; i++)
        {
            var item = page.Items[i];
            if (string.IsNullOrEmpty(item.Label))
                continue; // Skip invisible items

            bool isSelected = i == page.SelectedIndex;
            var color = isSelected ? UIColor.Accent : (item.IsEnabled ? UIColor.Default : UIColor.Disabled);

            Vec2i itemPos = new(itemX, itemY);
            Vec2i screenPos = UIHelper.ScaledPos(page.ItemsAnchor, itemPos);

            // Render item label (always just the label, not "LABEL: VALUE")
            string label = item.Label;

            // Use centered text if AlignCenter flag is set (matching C code)
            bool shouldCenter = page.LayoutFlags.HasFlag(MenuLayoutFlags.AlignCenter);
            
            if (shouldCenter)
                UIHelper.DrawTextCentered(label, screenPos, FontSizes.MenuItem, color);
            else
                UIHelper.DrawText(label, screenPos, FontSizes.MenuItem, color);

            // For toggles, draw the value right-aligned (matching C code)
            if (item is MenuToggle toggle)
            {
                string value = toggle.CurrentValue;
                int valueWidth = UIHelper.GetTextWidth(value, FontSizes.MenuItem);
                
                // Calculate right-aligned position: items_pos.x + block_width - text_width
                Vec2i togglePos = new(itemX + page.BlockWidth - valueWidth, itemY);
                Vec2i toggleScreenPos = UIHelper.ScaledPos(page.ItemsAnchor, togglePos);
                UIHelper.DrawText(value, toggleScreenPos, FontSizes.MenuItem, color);
            }

            // Move to next position
            if (isVertical)
                itemY += Spacing.MenuItemVerticalSpacing;
        }

        // Call custom draw callback if page has one
        page.DrawCallback?.Invoke(this);
    }

    // Render HORIZONTAL menu (confirmation dialogs) - matching C code exactly
    private void RenderHorizontalMenu(MenuPage page)
    {
        // Title and subtitle rendering
        Vec2i pos = new(0, -20);
        
        // Split title by newline for title + subtitle (matching C code)
        string[] titleLines = page.Title?.Split('\n') ?? Array.Empty<string>();
        if (titleLines.Length > 0)
        {
            Vec2i titleScreen = UIHelper.ScaledPos(page.TitleAnchor, pos);
            UIHelper.DrawTextCentered(titleLines[0], titleScreen, FontSizes.MenuItem, UIColor.Default);
            
            if (titleLines.Length > 1)
            {
                pos.Y += 12;
                Vec2i subtitleScreen = UIHelper.ScaledPos(page.TitleAnchor, pos);
                UIHelper.DrawTextCentered(titleLines[1], subtitleScreen, FontSizes.MenuItem, UIColor.Default);
            }
        }
        
        pos.Y += 16;

        // Items rendering - hardcoded positions like C: -50, 60
        pos.X = -50;
        for (int i = 0; i < page.Items.Count; i++)
        {
            var item = page.Items[i];
            if (string.IsNullOrEmpty(item.Label))
                continue;

            bool isSelected = i == page.SelectedIndex;
            var color = isSelected ? UIColor.Accent : UIColor.Default;

            Vec2i itemScreen = UIHelper.ScaledPos(page.ItemsAnchor, pos);
            UIHelper.DrawTextCentered(item.Label, itemScreen, FontSizes.MenuTitle, color);  // UI_SIZE_16 in C
            
            pos.X = 60;  // Second item position
        }
    }

    // Legacy interface methods - delegate to UIHelper
    public void DrawText(string text, Vec2i position, UIAnchor anchor, int size, UIColor color)
    {
        Vec2i screenPos = UIHelper.ScaledPos(anchor, position);
        UIHelper.DrawText(text, screenPos, size, color);
    }

    public void DrawTextCentered(string text, Vec2i position, int size, UIColor color)
    {
        UIHelper.DrawTextCentered(text, position, size, color);
    }

    public int GetTextWidth(string text, int size)
    {
        return UIHelper.GetTextWidth(text, size);
    }

    public Vec2i ScalePosition(UIAnchor anchor, Vec2i offset)
    {
        return UIHelper.ScaledPos(anchor, offset);
    }
}