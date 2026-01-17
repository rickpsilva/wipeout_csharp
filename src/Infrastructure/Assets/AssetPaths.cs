using System;
using System.Collections.Generic;
using System.IO;

namespace WipeoutRewrite.Infrastructure.Assets;

/// <summary>
/// Centralized asset path configuration.
/// Defines standard search paths for game assets across the codebase.
/// Eliminates duplication of path logic in multiple locations.
/// </summary>
public static class AssetPaths
{
    /// <summary>
    /// Standard relative paths to search for assets, in priority order.
    /// These paths are relative to the application's current working directory.
    /// </summary>
    public static readonly IEnumerable<string> AssetSearchPaths = new[]
    {
        Path.Combine("assets", "wipeout"),
        Path.Combine("..", "..", "assets", "wipeout"),
        Path.Combine("..", "..", "..", "assets", "wipeout")
    };

    /// <summary>
    /// Gets the common folder path for standard game assets.
    /// </summary>
    public static string GetCommonPath(string assetRoot) => 
        Path.Combine(assetRoot, "common");

    /// <summary>
    /// Gets the textures folder path.
    /// </summary>
    public static string GetTexturesPath(string assetRoot) => 
        Path.Combine(assetRoot, "textures");

    /// <summary>
    /// Gets the track folder path for a specific track number.
    /// </summary>
    public static string GetTrackPath(string assetRoot, int trackNumber) => 
        Path.Combine(assetRoot, $"track{trackNumber:D2}");

    /// <summary>
    /// Gets the music folder path.
    /// </summary>
    public static string GetMusicPath(string assetRoot) => 
        Path.Combine(assetRoot, "music");
}
