using WipeoutRewrite.Tools.Core;

namespace WipeoutRewrite.Tools.Managers;

/// <summary>
/// Manages directional lights in the scene.
/// Follows Single Responsibility Principle - only handles light lifecycle.
/// </summary>
public class LightManager : ILightManager
{
    public IReadOnlyList<DirectionalLight> Lights => _lights.AsReadOnly();

    private readonly List<DirectionalLight> _lights;

    public LightManager()
    {
        _lights = new List<DirectionalLight>();
    }

    public DirectionalLight AddLight(string name)
    {
        var light = new DirectionalLight(name);
        _lights.Add(light);
        return light;
    }

    public void RemoveLight(DirectionalLight light)
    {
        _lights.Remove(light);
    }
}