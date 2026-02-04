namespace WipeoutRewrite.Core.Services;

/// <summary>
/// Represents the 3D content preview information for a menu item.
/// Stores the category type and model index to be rendered.
/// </summary>
public class ContentPreview3DInfo
{
    /// <summary>
    /// The category type of the model to render (e.g., CategoryShip, CategoryMsDos, CategoryTeams).
    /// </summary>
    public Type CategoryType { get; set; }

    /// <summary>
    /// The index of the model within the category to render.
    /// </summary>
    public int ModelIndex { get; set; }

    /// <summary>
    /// Optional custom uniform scale for the model. If null, auto-scaling is applied.
    /// </summary>
    public float? CustomScale { get; set; }

    /// <summary>
    /// If true, renders a 2D track image instead of a 3D model.
    /// </summary>
    public bool IsTrackImage { get; set; }

    public ContentPreview3DInfo(Type categoryType, int modelIndex, float? customScale = null)
    {
        CategoryType = categoryType ?? throw new ArgumentNullException(nameof(categoryType));
        ModelIndex = modelIndex;
        CustomScale = customScale;
        IsTrackImage = false;
    }

    /// <summary>
    /// Creates a ContentPreview3DInfo for a track image preview
    /// </summary>
    public static ContentPreview3DInfo CreateTrackImagePreview(int trackIndex)
    {
        return new ContentPreview3DInfo(typeof(object), trackIndex)
        {
            IsTrackImage = true
        };
    }
}