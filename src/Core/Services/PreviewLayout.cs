using System.Collections.Generic;

namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Defines the position and layout for preview rendering on screen
/// </summary>
public enum PreviewPosition
{
    /// <summary>Center of screen (default)</summary>
    Center,
    
    /// <summary>Left side, bottom area</summary>
    LeftBottom,
    
    /// <summary>Right side, bottom area</summary>
    RightBottom,
    
    /// <summary>Top center (future expansion)</summary>
    TopCenter,
    /// <summary>Bottom center</summary>
    BottomCenter
}

/// <summary>
/// Represents a single preview in a layout
/// </summary>
public class PreviewItem
{
    public PreviewPosition Position { get; set; }
    public ContentPreview3DInfo Info { get; set; }
    
    public PreviewItem(PreviewPosition position, ContentPreview3DInfo info)
    {
        Position = position;
        Info = info;
    }
}

/// <summary>
/// Container for multiple 3D previews to be rendered on the same screen
/// </summary>
public class PreviewLayout
{
    public List<PreviewItem> Previews { get; set; } = new();
    
    public void AddPreview(PreviewPosition position, ContentPreview3DInfo info)
    {
        Previews.Add(new PreviewItem(position, info));
    }
    
    public void Clear()
    {
        Previews.Clear();
    }
}
