using Microsoft.Extensions.Logging;
using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Tools.Core;

/// <summary>
/// Factory implementation for creating TrackNavigationCalculator instances.
/// </summary>
public class TrackNavigationCalculatorFactory : ITrackNavigationCalculatorFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public TrackNavigationCalculatorFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public ITrackNavigationCalculator Create(ITrack track)
    {
        if (track == null)
            throw new ArgumentNullException(nameof(track));

        var logger = _loggerFactory.CreateLogger<TrackNavigationCalculator>();
        return new TrackNavigationCalculator(track, logger);
    }
}