using Microsoft.Extensions.DependencyInjection;
using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Factory;

public class GameObjectFactory : IGameObjectFactory
{
    private readonly IServiceProvider _serviceProvider;

    public GameObjectFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IGameObject CreateModel() => _serviceProvider.GetRequiredService<IGameObject>();
}