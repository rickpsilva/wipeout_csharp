using WipeoutRewrite.Infrastructure.Graphics;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace WipeoutRewrite.Tests.Infrastructure.Graphics;

/// <summary>
/// Command Pattern: Records rendering operations for deferred execution.
/// Enables testing rendering sequences without immediate GL execution.
/// </summary>
public interface IRenderCommand
{
    void Execute(IRenderer renderer);
}

public class BeginFrameCommand : IRenderCommand
{
    public void Execute(IRenderer renderer) => renderer.BeginFrame();
}

public class EndFrameCommand : IRenderCommand
{
    public void Execute(IRenderer renderer) => renderer.EndFrame();
}

public class DrawSpriteCommand : IRenderCommand
{
    private readonly float _x;
    private readonly float _y;
    private readonly float _w;
    private readonly float _h;
    private readonly Vector4 _color;

    public DrawSpriteCommand(float x, float y, float w, float h, Vector4 color)
    {
        _x = x;
        _y = y;
        _w = w;
        _h = h;
        _color = color;
    }

    public void Execute(IRenderer renderer) => renderer.PushSprite(_x, _y, _w, _h, _color);
}

public class RenderCommandQueue
{
    private readonly Queue<IRenderCommand> _commands = new();

    public int CommandCount => _commands.Count;

    public void Enqueue(IRenderCommand command) => _commands.Enqueue(command);

    public void Execute(IRenderer renderer)
    {
        while (_commands.TryDequeue(out var command))
        {
            command.Execute(renderer);
        }
    }

    public void Clear() => _commands.Clear();
}
