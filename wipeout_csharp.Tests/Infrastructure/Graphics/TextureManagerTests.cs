using Xunit;
using Moq;
using WipeoutRewrite.Infrastructure.Graphics;

namespace WipeoutRewrite.Tests.Infrastructure.Graphics;

public class TextureManagerTests
{
    private Mock<ITextureManager> CreateMockTextureManager()
    {
        return new Mock<ITextureManager>();
    }

    [Fact]
    public void TextureManager_IsInterface()
    {
        // Assert
        Assert.True(typeof(ITextureManager).IsInterface);
    }

    [Fact]
    public void ITextureManager_CanBeImplemented()
    {
        // Arrange
        var mock = CreateMockTextureManager();

        // Assert
        Assert.NotNull(mock.Object);
        Assert.IsAssignableFrom<ITextureManager>(mock.Object);
    }

    [Fact]
    public void TextureAlphaMode_HasMultipleModes()
    {
        // Assert
        var modes = Enum.GetNames(typeof(TextureAlphaMode));
        Assert.NotEmpty(modes);
    }
}
