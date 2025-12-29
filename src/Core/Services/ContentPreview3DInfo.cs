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
    /// Optional custom Z position for the model. If null, uses default based on category.
    /// </summary>
    public float? CustomZPosition { get; set; }

    public ContentPreview3DInfo(Type categoryType, int modelIndex, float? customZPosition = null)
    {
        CategoryType = categoryType ?? throw new ArgumentNullException(nameof(categoryType));
        ModelIndex = modelIndex;
        CustomZPosition = customZPosition;
    }
}