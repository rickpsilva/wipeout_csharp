using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Infrastructure.Assets;

/// <summary>
/// Implementation of IAssetPathResolver.
/// Centralizes asset path resolution logic (PRM, CMP, shadow textures).
/// </summary>
public class AssetPathResolver : IAssetPathResolver
{
    private readonly ILogger<AssetPathResolver> _logger;

    public AssetPathResolver(ILogger<AssetPathResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Attempts to resolve the path of a PRM file.
    /// Search order:
    /// 1. SHIPRENDER_PRM environment variable (if set and file exists)
    /// 2. Multiple relative locations: assets/wipeout, ../../assets/wipeout, ../../../assets/wipeout
    /// </summary>
    public string? ResolvePrmPath(string prmFileName)
    {
        if (string.IsNullOrWhiteSpace(prmFileName))
        {
            _logger.LogError("[AssetPathResolver] PRM filename is empty");
            return null;
        }

        // 1. Check SHIPRENDER_PRM env var
        string? envPrm = Environment.GetEnvironmentVariable("SHIPRENDER_PRM");
        if (!string.IsNullOrEmpty(envPrm) && File.Exists(envPrm))
        {
            _logger.LogInformation("[AssetPathResolver] Using SHIPRENDER_PRM from environment: {Path}", envPrm);
            return envPrm;
        }

        // 2. Search in multiple relative locations
        foreach (var basePath in AssetPaths.AssetSearchPaths)
        {
            string candidate = Path.Combine(basePath, "common", prmFileName);
            if (File.Exists(candidate))
            {
                string fullPath = Path.GetFullPath(candidate);
                _logger.LogInformation("[AssetPathResolver] Found PRM at: {Path}", fullPath);
                return fullPath;
            }
        }

        _logger.LogDebug("[AssetPathResolver] PRM file not found: {FileName}", prmFileName);
        return null;
    }

    /// <summary>
    /// Attempts to resolve the path of the CMP texture file associated with a PRM.
    /// Searches in the same directory as the PRM with .cmp extension.
    /// </summary>
    public string? ResolveCmpPath(string prmPath)
    {
        if (string.IsNullOrWhiteSpace(prmPath))
        {
            _logger.LogError("[AssetPathResolver] PRM path is empty");
            return null;
        }

        string cmpCandidate = Path.ChangeExtension(prmPath, ".cmp");
        if (File.Exists(cmpCandidate))
        {
            _logger.LogInformation("[AssetPathResolver] Found CMP texture file: {Path}", cmpCandidate);
            return cmpCandidate;
        }

        _logger.LogDebug("[AssetPathResolver] CMP file not found for PRM: {PrmPath}", prmPath);
        return null;
    }

    /// <summary>
    /// Attempts to resolve the path of a shadow texture.
    /// Shadow textures are located in: ../textures/shad{index}.tim
    /// Relative to the PRM directory.
    /// </summary>
    public string? ResolveShadowTexturePath(string prmPath, int shadowIndex)
    {
        if (string.IsNullOrWhiteSpace(prmPath))
        {
            _logger.LogError("[AssetPathResolver] PRM path is empty");
            return null;
        }

        if (shadowIndex < 1 || shadowIndex > 4)
        {
            _logger.LogWarning("[AssetPathResolver] Invalid shadow index: {Index} (expected 1-4)", shadowIndex);
            return null;
        }

        string prmDir = Path.GetDirectoryName(prmPath) ?? "";
        string shadowPath = Path.Combine(prmDir, "..", "textures", $"shad{shadowIndex}.tim");
        string fullPath = Path.GetFullPath(shadowPath);

        if (File.Exists(fullPath))
        {
            _logger.LogInformation("[AssetPathResolver] Found shadow texture: {Path}", fullPath);
            return fullPath;
        }

        _logger.LogDebug("[AssetPathResolver] Shadow texture not found: {Path}", fullPath);
        return null;
    }
}
