using WipeoutRewrite.Tools.Core;

namespace WipeoutRewrite.Tools.Managers;

/// <summary>
/// Manages directional lights in the scene.
/// </summary>
public interface ILightManager
{
    /// <summary>
    /// Gets a read-only collection of all directional lights managed by this manager.
    /// </summary>
    IReadOnlyList<DirectionalLight> Lights { get; }

    /// <summary>
    /// Adds a new directional light with the specified name.
    /// </summary>
    /// <param name="name">The name identifier for the light.</param>
    /// <returns>The newly created directional light.</returns>
    DirectionalLight AddLight(string name);

    /// <summary>
    /// Removes the specified directional light from the manager.
    /// </summary>
    /// <param name="light">The directional light to remove.</param>
    void RemoveLight(DirectionalLight light);
}