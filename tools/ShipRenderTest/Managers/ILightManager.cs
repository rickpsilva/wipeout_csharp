using WipeoutRewrite.Tools.Core;

namespace WipeoutRewrite.Tools.Managers;

/// <summary>
/// Interface for managing directional lights in a scene.
/// Follows Interface Segregation Principle.
/// </summary>
public interface ILightManager
{
    IReadOnlyList<DirectionalLight> Lights { get; }

    DirectionalLight AddLight(string name);
    void RemoveLight(DirectionalLight light);
}