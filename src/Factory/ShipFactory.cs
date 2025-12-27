
using Microsoft.Extensions.DependencyInjection;
using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Factory
{
   public class ShipFactory : IGameObjectFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public ShipFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IGameObject CreateModel() => _serviceProvider.GetRequiredService<IGameObject>();

    }
}