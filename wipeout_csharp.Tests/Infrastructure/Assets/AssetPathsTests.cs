using System.IO;
using WipeoutRewrite.Infrastructure.Assets;
using Xunit;

namespace WipeoutRewrite.Tests.Infrastructure.Assets;

public class AssetPathsTests
{
    [Fact]
    public void AssetSearchPaths_ContainsThreePaths()
    {
        var paths = AssetPaths.AssetSearchPaths;
        
        Assert.NotNull(paths);
        Assert.Equal(3, paths.Count());
    }

    [Fact]
    public void AssetSearchPaths_FirstPath_IsAssetsWipeout()
    {
        var paths = AssetPaths.AssetSearchPaths.ToList();
        
        Assert.Equal(Path.Combine("assets", "wipeout"), paths[0]);
    }

    [Fact]
    public void AssetSearchPaths_SecondPath_IsTwoLevelsUp()
    {
        var paths = AssetPaths.AssetSearchPaths.ToList();
        
        Assert.Equal(Path.Combine("..", "..", "assets", "wipeout"), paths[1]);
    }

    [Fact]
    public void AssetSearchPaths_ThirdPath_IsThreeLevelsUp()
    {
        var paths = AssetPaths.AssetSearchPaths.ToList();
        
        Assert.Equal(Path.Combine("..", "..", "..", "assets", "wipeout"), paths[2]);
    }

    [Fact]
    public void GetCommonPath_ReturnsCorrectPath()
    {
        var path = AssetPaths.GetCommonPath("/root/assets");
        
        Assert.Equal(Path.Combine("/root/assets", "common"), path);
    }

    [Fact]
    public void GetCommonPath_WithTrailingSlash()
    {
        var path = AssetPaths.GetCommonPath("/root/assets/");
        
        Assert.Contains("common", path);
    }

    [Fact]
    public void GetTexturesPath_ReturnsCorrectPath()
    {
        var path = AssetPaths.GetTexturesPath("/root/assets");
        
        Assert.Equal(Path.Combine("/root/assets", "textures"), path);
    }

    [Fact]
    public void GetTrackPath_WithTrackNumber_ReturnsCorrectPath()
    {
        var path = AssetPaths.GetTrackPath("/root/assets", 1);
        
        Assert.Equal(Path.Combine("/root/assets", "track01"), path);
    }

    [Fact]
    public void GetTrackPath_WithDoubleDigitTrack_FormatsCorrectly()
    {
        var path = AssetPaths.GetTrackPath("/root/assets", 12);
        
        Assert.Equal(Path.Combine("/root/assets", "track12"), path);
    }

    [Fact]
    public void GetTrackPath_WithSingleDigit_PadsWithZero()
    {
        var path = AssetPaths.GetTrackPath("/root/assets", 5);
        
        Assert.Contains("track05", path);
    }

    [Fact]
    public void GetMusicPath_ReturnsCorrectPath()
    {
        var path = AssetPaths.GetMusicPath("/root/assets");
        
        Assert.Equal(Path.Combine("/root/assets", "music"), path);
    }

    [Theory]
    [InlineData("/assets", "common")]
    [InlineData("/test/path", "common")]
    [InlineData("C:\\assets", "common")]
    public void GetCommonPath_WorksWithDifferentBasePaths(string basePath, string expected)
    {
        var path = AssetPaths.GetCommonPath(basePath);
        
        Assert.EndsWith(expected, path);
    }

    [Theory]
    [InlineData(1, "track01")]
    [InlineData(9, "track09")]
    [InlineData(10, "track10")]
    [InlineData(99, "track99")]
    public void GetTrackPath_FormatsTrackNumberCorrectly(int trackNum, string expected)
    {
        var path = AssetPaths.GetTrackPath("/root", trackNum);
        
        Assert.Contains(expected, path);
    }
}
