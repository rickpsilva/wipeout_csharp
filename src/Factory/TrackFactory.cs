using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Factory;

/// <summary>
/// Factory implementation for creating fresh Track instances.
/// Each call to Create() returns a new Track object with empty sections list.
/// </summary>
public class TrackFactory : ITrackFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public TrackFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public ITrack Create()
    {
        var logger = _loggerFactory.CreateLogger<Track>();
        return new Track(logger);
    }
}