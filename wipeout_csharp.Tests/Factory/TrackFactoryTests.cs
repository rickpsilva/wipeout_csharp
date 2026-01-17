using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using WipeoutRewrite.Factory;
using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Tests.Factory;

public class TrackFactoryTests
{
    [Fact]
    public void Constructor_WithNullLoggerFactory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TrackFactory(null!));
    }

    [Fact]
    public void Create_ReturnsTrack()
    {
        var factory = new TrackFactory(NullLoggerFactory.Instance);

        var track = factory.Create();

        Assert.NotNull(track);
        Assert.IsAssignableFrom<ITrack>(track);
    }

    [Fact]
    public void Create_CreatesDifferentInstances()
    {
        var factory = new TrackFactory(NullLoggerFactory.Instance);

        var track1 = factory.Create();
        var track2 = factory.Create();

        Assert.NotSame(track1, track2);
    }

    [Fact]
    public void Create_CreatesTrackWithEmptySections()
    {
        var factory = new TrackFactory(NullLoggerFactory.Instance);

        var track = factory.Create();

        Assert.Empty(track.Sections);
    }
}
