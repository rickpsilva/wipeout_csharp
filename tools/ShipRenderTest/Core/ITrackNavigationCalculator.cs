using WipeoutRewrite.Core.Entities;
using static WipeoutRewrite.Tools.Core.TrackNavigationCalculator;

namespace WipeoutRewrite.Tools.Core;

/// <summary>
/// Defines methods for calculating and retrieving navigation data along a track.
/// </summary>
public interface ITrackNavigationCalculator
{
    /// <summary>
    /// Returns the loop time in seconds based on the number of camera points and point duration.
    /// </summary>
    float GetLoopTimeSeconds();
    /// <summary>
    /// Gets the navigation data at the specified progress value along the track.
    /// </summary>
    /// <param name="progress">The progress value, typically ranging from 0 to 1, indicating position along the track.</param>
    /// <returns>A <see cref="NavigationData"/> object containing position and orientation information.</returns>
    NavigationData GetNavigationData(float progress);
    /// <summary>
    /// Gets the progress value corresponding to the specified section index.
    /// </summary>
    /// <param name="sectionIndex">The index of the section.</param>
    /// <returns>The progress value for the given section.</returns>
    float GetProgressFromSection(int sectionIndex);
    /// <summary>
    /// Recommended base playback speed (progress per second) so that 1x completes a lap in loop time.
    /// </summary>
    float GetRecommendedBaseSpeed();
    /// <summary>
    /// Gets the total number of sections in the track.
    /// </summary>
    /// <returns>The number of sections.</returns>
    int GetSectionCount();
    /// <summary>
    /// Gets the starting position navigation data for the track.
    /// </summary>
    /// <returns>A <see cref="NavigationData"/> object representing the starting position.</returns>
    NavigationData GetStartingPosition();
    /// <summary>
    /// Gets the track associated with this navigation calculator.
    /// </summary>
    /// <returns>The <see cref="ITrack"/> instance.</returns>
    ITrack GetTrack();
    /// <summary>
    /// Gets the total number of waypoints on the track.
    /// </summary>
    /// <returns>The number of waypoints.</returns>
    int GetWaypointCount();
    /// <summary>
    /// Gets all waypoints defined on the track.
    /// </summary>
    /// <returns>A read-only collection of <see cref="NavigationWaypoint"/> objects.</returns>
    IReadOnlyList<NavigationWaypoint> GetWaypoints();
    /// <summary>
    /// Refreshes the waypoint data, recalculating waypoint positions and properties.
    /// </summary>
    void RefreshWaypoints();
}