namespace WipeoutRewrite.Presentation
{
    /// <summary>
    /// Marker types for category-based rendering in ContentPreview3D.
    /// Each type represents a GameObjectCategory.
    /// Use these as type parameters: Render&lt;CategoryShip&gt;(index)
    /// </summary>
    
    /// <summary>
    /// Marker type for Ship category (racing ships from allsh.prm)
    /// </summary>
    public sealed class CategoryShip { private CategoryShip() { } }
    
    /// <summary>
    /// Marker type for MsDos category (MS-DOS objects from msdos.prm)
    /// </summary>
    public sealed class CategoryMsDos { private CategoryMsDos() { } }
    
    /// <summary>
    /// Marker type for Teams category (team logos from teams.prm)
    /// </summary>
    public sealed class CategoryTeams { private CategoryTeams() { } }
    
    /// <summary>
    /// Marker type for Options category (menu options from alopt.prm)
    /// </summary>
    public sealed class CategoryOptions { private CategoryOptions() { } }
    
    /// <summary>
    /// Marker type for Weapon category (weapons and projectiles)
    /// </summary>
    public sealed class CategoryWeapon { private CategoryWeapon() { } }
    
    /// <summary>
    /// Marker type for Pickup category (pickups and power-ups)
    /// </summary>
    public sealed class CategoryPickup { private CategoryPickup() { } }
    
    /// <summary>
    /// Marker type for Prop category (decorative props)
    /// </summary>
    public sealed class CategoryProp { private CategoryProp() { } }
    
    /// <summary>
    /// Marker type for Obstacle category (track obstacles)
    /// </summary>
    public sealed class CategoryObstacle { private CategoryObstacle() { } }
    
    /// <summary>
    /// Marker type for Pilot category (pilot models)
    /// </summary>
    public sealed class CategoryPilot { private CategoryPilot() { } }
    
    /// <summary>
    /// Marker type for Camera category (camera models)
    /// </summary>
    public sealed class CategoryCamera { private CategoryCamera() { } }
}
