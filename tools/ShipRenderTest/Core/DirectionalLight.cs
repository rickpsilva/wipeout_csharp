using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Tools.Core;

/// <summary>
/// Represents a directional light in the scene.
/// Follows Single Responsibility Principle - only handles light properties.
/// </summary>
public class DirectionalLight
{
    public Vec3 Color { get; set; }
    public Vec3 Direction { get; set; }
    public float Intensity { get; set; }
    public bool IsEnabled { get; set; }
    public string Name { get; set; }

    public DirectionalLight(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Direction = new Vec3(0, -1, 0);
        Color = new Vec3(1, 1, 1);
        Intensity = 1.0f;
        IsEnabled = true;
    }

    public override string ToString() => Name;
}
