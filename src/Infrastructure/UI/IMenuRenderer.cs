using WipeoutRewrite.Core.Services;

namespace WipeoutRewrite.Infrastructure.UI;

public interface IMenuRenderer
{
    void RenderMenu(IMenuManager menu);
    void DrawText(string text, Vec2i position, UIAnchor anchor, int size, UIColor color);
    void DrawTextCentered(string text, Vec2i position, int size, UIColor color);
    int GetTextWidth(string text, int size);
    void SetWindowSize(int width, int height);
}

public enum UIColor
{
    Default,
    Accent,
    Disabled
}

public enum UISize
{
    Size8 = 8,
    Size12 = 12,
    Size16 = 16,
    Size20 = 20
}
