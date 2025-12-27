namespace WipeoutRewrite.Core.Entities
{
    /// <summary>
    /// Categories for game objects to organize them logically.
    /// </summary>
    public enum GameObjectCategory
    {
        Unknown = 0,
        
        // Racing ships (8 ships from allsh.prm)
        Ship,
        
        // UI and menu elements
        MsDos,
        Teams,
        Options,
        
        // Weapons and pickups
        Weapon,
        Pickup,
        
        // Props and obstacles
        Prop,
        Obstacle,
        
        // Track-specific objects (sky and scene for each track)
        Track01,
        Track02,
        Track03,
        Track04,
        Track05,
        Track06,
        Track07,
        Track08,
        Track09,
        Track10,
        Track11,
        Track12,
        Track13,
        Track14,
        
        // Other
        Pilot,
        Camera
    }
}
