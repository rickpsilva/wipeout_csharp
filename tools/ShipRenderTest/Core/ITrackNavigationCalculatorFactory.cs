using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Tools.Core;

/// <summary>
/// Factory for creating TrackNavigationCalculator instances.
/// Provides proper dependency injection for dynamic calculator creation.
/// </summary>
public interface ITrackNavigationCalculatorFactory
{
    /// <summary>
    /// Creates a new TrackNavigationCalculator instance for the given track.
    /// </summary>
    /// <param name="track">The track to navigate</param>
    /// <returns>A new configured TrackNavigationCalculator</returns>
    ITrackNavigationCalculator Create(ITrack track);
}