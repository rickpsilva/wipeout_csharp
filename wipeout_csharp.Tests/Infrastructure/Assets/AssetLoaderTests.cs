using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using WipeoutRewrite.Infrastructure.Assets;

namespace WipeoutRewrite.Tests.Infrastructure.Assets;

public class AssetLoaderTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<ILogger<AssetLoader>> _mockLogger;

    public AssetLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"assets_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _mockLogger = new Mock<ILogger<AssetLoader>>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Constructor_WithNullLogger_DoesNotThrow()
    {
        // Act & Assert - AssetLoader allows null logger
        var loader = new AssetLoader(null!);
        Assert.NotNull(loader);
    }

    [Fact]
    public void Initialize_WithValidPath_SetsBasePath()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);

        // Act
        loader.Initialize(_tempDir);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("initialized")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Initialize_WithNonExistentPath_LogsWarning()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        var nonExistentPath = Path.Combine(_tempDir, "nonexistent");

        // Act
        loader.Initialize(nonExistentPath);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("does not exist")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LoadTextFile_WithExistingFile_ReturnsContent()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        loader.Initialize(_tempDir);
        var testFile = Path.Combine(_tempDir, "test.txt");
        var testContent = "Test content";
        File.WriteAllText(testFile, testContent);

        // Act
        var result = loader.LoadTextFile("test.txt");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testContent, result);
    }

    [Fact]
    public void LoadTextFile_WithMissingFile_ReturnsNull()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        loader.Initialize(_tempDir);

        // Act
        var result = loader.LoadTextFile("nonexistent.txt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void LoadTextFile_WithMissingFile_LogsWarning()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        loader.Initialize(_tempDir);

        // Act
        loader.LoadTextFile("missing.txt");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("File not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LoadBinaryFile_WithExistingFile_ReturnsBytes()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        loader.Initialize(_tempDir);
        var testFile = Path.Combine(_tempDir, "test.bin");
        var testData = new byte[] { 1, 2, 3, 4, 5 };
        File.WriteAllBytes(testFile, testData);

        // Act
        var result = loader.LoadBinaryFile("test.bin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testData, result);
    }

    [Fact]
    public void LoadBinaryFile_WithMissingFile_ReturnsNull()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        loader.Initialize(_tempDir);

        // Act
        var result = loader.LoadBinaryFile("nonexistent.bin");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void LoadTextFile_WithEmptyFile_ReturnsEmptyString()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        loader.Initialize(_tempDir);
        var testFile = Path.Combine(_tempDir, "empty.txt");
        File.WriteAllText(testFile, "");

        // Act
        var result = loader.LoadTextFile("empty.txt");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void LoadBinaryFile_WithEmptyFile_ReturnsEmptyArray()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        loader.Initialize(_tempDir);
        var testFile = Path.Combine(_tempDir, "empty.bin");
        File.WriteAllBytes(testFile, new byte[] { });

        // Act
        var result = loader.LoadBinaryFile("empty.bin");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ListFiles_WithValidPath_ReturnsFiles()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        var subDir = Path.Combine(_tempDir, "subdir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "file1.txt"), "");
        File.WriteAllText(Path.Combine(subDir, "file2.txt"), "");
        loader.Initialize(_tempDir);

        // Act
        var result = loader.ListFiles("subdir");

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ListFiles_WithPattern_FiltersCorrectly()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        var subDir = Path.Combine(_tempDir, "subdir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "file1.txt"), "");
        File.WriteAllText(Path.Combine(subDir, "file2.bin"), "");
        loader.Initialize(_tempDir);

        // Act
        var result = loader.ListFiles("subdir", "*.txt");

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, f => Assert.EndsWith(".txt", f));
    }

    [Fact]
    public void LoadTextFile_WithMultilineContent_PreservesNewlines()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        loader.Initialize(_tempDir);
        var testFile = Path.Combine(_tempDir, "multiline.txt");
        var testContent = "Line 1\nLine 2\nLine 3";
        File.WriteAllText(testFile, testContent);

        // Act
        var result = loader.LoadTextFile("multiline.txt");

        // Assert
        Assert.Equal(testContent, result);
    }

    [Fact]
    public void LoadBinaryFile_WithLargeFile_ReturnsCorrectData()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        loader.Initialize(_tempDir);
        var testFile = Path.Combine(_tempDir, "large.bin");
        var largeData = new byte[10000];
        for (int i = 0; i < largeData.Length; i++)
            largeData[i] = (byte)(i % 256);
        File.WriteAllBytes(testFile, largeData);

        // Act
        var result = loader.LoadBinaryFile("large.bin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(largeData.Length, result.Length);
        Assert.Equal(largeData, result);
    }

    [Fact]
    public void LoadTextFile_WithSpecialCharacters_PreservesContent()
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        loader.Initialize(_tempDir);
        var testFile = Path.Combine(_tempDir, "special.txt");
        var testContent = "Special chars: !@#$%^&*()";
        File.WriteAllText(testFile, testContent);

        // Act
        var result = loader.LoadTextFile("special.txt");

        // Assert
        Assert.Equal(testContent, result);
    }

    [Theory]
    [InlineData("test.txt")]
    [InlineData("subfolder/test.txt")]
    [InlineData("deep/nested/folder/test.txt")]
    public void LoadTextFile_WithNestedPaths_WorksCorrectly(string relativePath)
    {
        // Arrange
        var loader = new AssetLoader(_mockLogger.Object);
        loader.Initialize(_tempDir);
        var fullPath = Path.Combine(_tempDir, relativePath);
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        var testContent = $"Content for {relativePath}";
        File.WriteAllText(fullPath, testContent);

        // Act
        var result = loader.LoadTextFile(relativePath);

        // Assert
        Assert.Equal(testContent, result);
    }
}
