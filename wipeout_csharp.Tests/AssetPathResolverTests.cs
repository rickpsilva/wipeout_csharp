using Xunit;
using System.IO;
using WipeoutRewrite.Infrastructure.Assets;
using Microsoft.Extensions.Logging.Abstractions;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for AssetPathResolver.
/// Tests path resolution for PRM, CMP, and shadow texture files.
/// </summary>
public class AssetPathResolverTests
{
    private readonly IAssetPathResolver _resolver;

    public AssetPathResolverTests()
    {
        _resolver = new AssetPathResolver(NullLogger<AssetPathResolver>.Instance);
    }

    #region PRM Path Resolution Tests

    [Fact]
    public void ResolvePrmPath_WithNullFileName_ReturnsNull()
    {
        var result = _resolver.ResolvePrmPath(null!);
        
        Assert.Null(result);
    }

    [Fact]
    public void ResolvePrmPath_WithEmptyFileName_ReturnsNull()
    {
        var result = _resolver.ResolvePrmPath("");
        
        Assert.Null(result);
    }

    [Fact]
    public void ResolvePrmPath_WithWhitespaceFileName_ReturnsNull()
    {
        var result = _resolver.ResolvePrmPath("   ");
        
        Assert.Null(result);
    }

    [Fact]
    public void ResolvePrmPath_WithNonexistentFile_ReturnsNull()
    {
        var result = _resolver.ResolvePrmPath("nonexistent.prm");
        
        Assert.Null(result);
    }

    [Fact]
    public void ResolvePrmPath_RespectsSHIPRENDER_PRMEnvVar()
    {
        // Create a temporary file
        var tempFile = Path.GetTempFileName();
        try
        {
            Environment.SetEnvironmentVariable("SHIPRENDER_PRM", tempFile);
            
            var result = _resolver.ResolvePrmPath("anyname.prm");
            
            Assert.NotNull(result);
            Assert.Equal(tempFile, result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SHIPRENDER_PRM", null);
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ResolvePrmPath_IgnoresMissingEnvVarFile()
    {
        Environment.SetEnvironmentVariable("SHIPRENDER_PRM", "/nonexistent/path/file.prm");
        
        var result = _resolver.ResolvePrmPath("test.prm");
        
        // Should return null since the nonexistent env var path is invalid
        Assert.Null(result);
        
        Environment.SetEnvironmentVariable("SHIPRENDER_PRM", null);
    }

    #endregion

    #region CMP Path Resolution Tests

    [Fact]
    public void ResolveCmpPath_WithNullPath_ReturnsNull()
    {
        var result = _resolver.ResolveCmpPath(null!);
        
        Assert.Null(result);
    }

    [Fact]
    public void ResolveCmpPath_WithEmptyPath_ReturnsNull()
    {
        var result = _resolver.ResolveCmpPath("");
        
        Assert.Null(result);
    }

    [Fact]
    public void ResolveCmpPath_WithNonexistentPrmPath_ReturnsNull()
    {
        var result = _resolver.ResolveCmpPath("/nonexistent/path/model.prm");
        
        Assert.Null(result);
    }

    [Fact]
    public void ResolveCmpPath_WithValidPrmPath_ReturnsCmpIfExists()
    {
        // Create temporary PRM and CMP files
        var tempPrmFile = Path.GetTempFileName();
        var tempCmpFile = Path.ChangeExtension(tempPrmFile, ".cmp");
        
        try
        {
            File.WriteAllText(tempCmpFile, "");
            
            var result = _resolver.ResolveCmpPath(tempPrmFile);
            
            Assert.NotNull(result);
            Assert.Equal(tempCmpFile, result);
        }
        finally
        {
            if (File.Exists(tempPrmFile))
                File.Delete(tempPrmFile);
            if (File.Exists(tempCmpFile))
                File.Delete(tempCmpFile);
        }
    }

    #endregion

    #region Shadow Texture Path Resolution Tests

    [Fact]
    public void ResolveShadowTexturePath_WithNullPath_ReturnsNull()
    {
        var result = _resolver.ResolveShadowTexturePath(null!, 1);
        
        Assert.Null(result);
    }

    [Fact]
    public void ResolveShadowTexturePath_WithInvalidShadowIndex_ReturnsNull()
    {
        var result = _resolver.ResolveShadowTexturePath("/some/path.prm", 0);
        Assert.Null(result);
        
        result = _resolver.ResolveShadowTexturePath("/some/path.prm", 5);
        Assert.Null(result);
        
        result = _resolver.ResolveShadowTexturePath("/some/path.prm", -1);
        Assert.Null(result);
    }

    [Fact]
    public void ResolveShadowTexturePath_WithValidIndices()
    {
        for (int i = 1; i <= 4; i++)
        {
            // Should not throw for valid indices 1-4
            var result = _resolver.ResolveShadowTexturePath("/nonexistent/path.prm", i);
            Assert.Null(result); // File doesn't exist, but call should not throw
        }
    }

    [Fact]
    public void ResolveShadowTexturePath_WithValidFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "test_assets", "textures");
        Directory.CreateDirectory(tempDir);
        
        var tempPrmFile = Path.Combine(Path.GetTempPath(), "test_assets", "common", "test.prm");
        Directory.CreateDirectory(Path.GetDirectoryName(tempPrmFile)!);
        File.WriteAllText(tempPrmFile, "");
        
        var shadowFile = Path.Combine(tempDir, "shad1.tim");
        File.WriteAllText(shadowFile, "");
        
        try
        {
            var result = _resolver.ResolveShadowTexturePath(tempPrmFile, 1);
            
            Assert.NotNull(result);
            Assert.True(File.Exists(result));
        }
        finally
        {
            if (File.Exists(tempPrmFile))
                File.Delete(tempPrmFile);
            if (File.Exists(shadowFile))
                File.Delete(shadowFile);
            try
            {
                Directory.Delete(Path.GetDirectoryName(tempPrmFile)!);
                Directory.Delete(tempDir);
                Directory.Delete(Path.Combine(Path.GetTempPath(), "test_assets"));
            }
            catch { }
        }
    }

    #endregion
}
