using WipeoutRewrite.Infrastructure.Input;
using Xunit;

namespace WipeoutRewrite.Tests.Infrastructure.Input;

public class KeyNameMappingTests
{
    [Theory]
    [InlineData(4, "A")]
    [InlineData(5, "B")]
    [InlineData(29, "Z")]
    [InlineData(30, "1")]
    [InlineData(39, "0")]
    public void GetKeyName_WithLettersAndNumbers_ReturnsCorrectName(int code, string expected)
    {
        var result = KeyNameMapping.GetKeyName(code);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(40, "RETURN")]
    [InlineData(41, "ESCAPE")]
    [InlineData(42, "BACKSP")]
    [InlineData(44, "SPACE")]
    public void GetKeyName_WithSpecialKeys_ReturnsCorrectName(int code, string expected)
    {
        var result = KeyNameMapping.GetKeyName(code);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(58, "F1")]
    [InlineData(65, "F8")]
    [InlineData(69, "F12")]
    public void GetKeyName_WithFunctionKeys_ReturnsCorrectName(int code, string expected)
    {
        var result = KeyNameMapping.GetKeyName(code);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(79, "RIGHT")]
    [InlineData(80, "LEFT")]
    [InlineData(81, "DOWN")]
    [InlineData(82, "UP")]
    public void GetKeyName_WithArrowKeys_ReturnsCorrectName(int code, string expected)
    {
        var result = KeyNameMapping.GetKeyName(code);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(89, "KP1")]
    [InlineData(94, "KP6")]
    [InlineData(98, "KP0")]
    public void GetKeyName_WithNumpadKeys_ReturnsCorrectName(int code, string expected)
    {
        var result = KeyNameMapping.GetKeyName(code);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(100, "LCTRL")]
    [InlineData(101, "LSHIFT")]
    [InlineData(102, "LALT")]
    [InlineData(104, "RCTRL")]
    public void GetKeyName_WithModifierKeys_ReturnsCorrectName(int code, string expected)
    {
        var result = KeyNameMapping.GetKeyName(code);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(119, "A")]
    [InlineData(120, "B")]
    [InlineData(121, "X")]
    [InlineData(123, "Y")]
    public void GetKeyName_WithGamepadButtons_ReturnsCorrectName(int code, string expected)
    {
        var result = KeyNameMapping.GetKeyName(code);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(124, "LSHLDR")]
    [InlineData(125, "RSHLDR")]
    [InlineData(128, "LTRIG")]
    [InlineData(129, "RTRIG")]
    public void GetKeyName_WithGamepadShoulders_ReturnsCorrectName(int code, string expected)
    {
        var result = KeyNameMapping.GetKeyName(code);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(126, "SELECT")]
    [InlineData(127, "START")]
    public void GetKeyName_WithGamepadStartSelect_ReturnsCorrectName(int code, string expected)
    {
        var result = KeyNameMapping.GetKeyName(code);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(132, "DPUP")]
    [InlineData(133, "DPDOWN")]
    [InlineData(134, "DPLEFT")]
    [InlineData(135, "DPRIGHT")]
    public void GetKeyName_WithGamepadDpad_ReturnsCorrectName(int code, string expected)
    {
        var result = KeyNameMapping.GetKeyName(code);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    [InlineData(-1)]
    [InlineData(200)]
    public void GetKeyName_WithUnknownCode_ReturnsFormattedCode(int code)
    {
        var result = KeyNameMapping.GetKeyName(code);

        Assert.Equal($"KEY{code}", result);
    }
}
