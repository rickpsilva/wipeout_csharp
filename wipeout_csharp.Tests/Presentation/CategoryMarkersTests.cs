using System;
using System.Linq;
using System.Reflection;
using WipeoutRewrite.Presentation;
using Xunit;

namespace WipeoutRewrite.Tests.Presentation;

public class CategoryMarkersTests
{
    private static readonly Type[] MarkerTypes = new[]
    {
        typeof(CategoryShip),
        typeof(CategoryMsDos),
        typeof(CategoryTeams),
        typeof(CategoryOptions),
        typeof(CategoryWeapon),
        typeof(CategoryPickup),
        typeof(CategoryProp),
        typeof(CategoryObstacle),
        typeof(CategoryPilot),
        typeof(CategoryCamera)
    };

    [Fact]
    public void AllMarkerTypes_AreSealed()
    {
        Assert.All(MarkerTypes, t => Assert.True(t.IsSealed, $"{t.Name} should be sealed"));
    }

    [Fact]
    public void AllMarkerTypes_HavePrivateParameterlessConstructor()
    {
        foreach (var type in MarkerTypes)
        {
            var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, binder: null, Type.EmptyTypes, modifiers: null);
            Assert.NotNull(ctor);
            Assert.True(ctor!.IsPrivate, $"{type.Name} constructor should be private");
        }
    }

    [Fact]
    public void PrivateConstructors_CanBeInvokedViaReflection()
    {
        foreach (var type in MarkerTypes)
        {
            var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, binder: null, Type.EmptyTypes, modifiers: null);
            Assert.NotNull(ctor);
            var instance = ctor!.Invoke(null);
            Assert.IsType(type, instance);
        }
    }
}
