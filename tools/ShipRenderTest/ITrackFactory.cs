using WipeoutRewrite.Core.Entities;

namespace WipeoutRewrite.Tools;

/// <summary>
/// Factory for creating fresh Track instances.
/// Ensures each track load gets a new Track object with empty sections list.
/// </summary>
public interface ITrackFactory
{
    /// <summary>
    /// Create a new Track instance.
    /// </summary>
    ITrack Create();
}