using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using WipeoutRewrite.Factory;
using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Tests.Factory;

public class GameObjectFactoryTests
{
    [Fact]
    public void CreateModel_ReturnsGameObject()
    {
        var mockGameObject = new Mock<IGameObject>();
        var serviceProvider = new ServiceCollection()
            .AddSingleton(mockGameObject.Object)
            .BuildServiceProvider();

        var factory = new GameObjectFactory(serviceProvider);

        var result = factory.CreateModel();

        Assert.NotNull(result);
        Assert.Same(mockGameObject.Object, result);
    }

    [Fact]
    public void CreateModel_CallsServiceProvider()
    {
        var mockGameObject = new Mock<IGameObject>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IGameObject)))
            .Returns(mockGameObject.Object);

        var factory = new GameObjectFactory(mockServiceProvider.Object);

        var result = factory.CreateModel();

        mockServiceProvider.Verify(
            sp => sp.GetService(typeof(IGameObject)),
            Times.Once
        );
    }
}
