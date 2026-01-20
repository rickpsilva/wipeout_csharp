using System;
using System.IO;

namespace WipeoutRewrite.Infrastructure.Assets;

/// <summary>
/// Interface for resolving asset paths (PRM, CMP, shadow textures).
/// Centralizes search logic across multiple locations and environment variables.
/// </summary>
public interface IAssetPathResolver
{
    /// <summary>
    /// Attempts to resolve the path of a PRM file.
    /// Searches in multiple relative locations and respects the SHIPRENDER_PRM environment variable.
    /// </summary>
    /// <returns>Absolute path of the PRM file found, or null if not found.</returns>
    string? ResolvePrmPath(string prmFileName);

    /// <summary>
    /// Attempts to resolve the path of the CMP texture file associated with a PRM.
    /// </summary>
    /// <returns>Absolute path of the CMP file found, or null if not found.</returns>
    string? ResolveCmpPath(string prmPath);

    /// <summary>
    /// Attempts to resolve the path of a shadow texture (shad1.tim - shad4.tim).
    /// </summary>
    /// <param name="prmPath">Path of the PRM file to determine the base folder.</param>
    /// <param name="shadowIndex">Shadow index (1-4).</param>
    /// <returns>Absolute path of the shadow texture found, or null if not found.</returns>
    string? ResolveShadowTexturePath(string prmPath, int shadowIndex);
}
