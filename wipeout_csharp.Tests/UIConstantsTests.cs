using Xunit;
using OpenTK.Mathematics;
using WipeoutRewrite.Infrastructure.UI;

namespace WipeoutRewrite.Tests;

/// <summary>
/// Unit tests for UIConstants.
/// Validates that all UI constants are properly defined and have expected values.
/// </summary>
public class UIConstantsTests
{
    [Fact]
    public void FontSizes_MenuTitle_ShouldBe16()
    {
        Assert.Equal(16, UIConstants.FontSizes.MenuTitle);
    }

    [Fact]
    public void FontSizes_MenuItem_ShouldBe16()
    {
        Assert.Equal(16, UIConstants.FontSizes.MenuItem);
    }

    [Fact]
    public void FontSizes_SplashText_ShouldBe16()
    {
        Assert.Equal(16, UIConstants.FontSizes.SplashText);
    }

    [Fact]
    public void FontSizes_CreditsText_ShouldBe8()
    {
        Assert.Equal(8, UIConstants.FontSizes.CreditsText);
    }

    [Fact]
    public void FontSizes_CreditsTitle_ShouldBe16()
    {
        Assert.Equal(16, UIConstants.FontSizes.CreditsTitle);
    }

    [Fact]
    public void Colors_MenuTitleDefault_ShouldBeWhite()
    {
        var white = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
        Assert.Equal(white, UIConstants.Colors.MenuTitleDefault);
    }

    [Fact]
    public void Colors_MenuItemDefault_ShouldBeWhite()
    {
        var white = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
        Assert.Equal(white, UIConstants.Colors.MenuItemDefault);
    }

    [Fact]
    public void Colors_MenuItemSelected_ShouldBeYellow()
    {
        var yellow = new Color4(1.0f, 0.8f, 0.0f, 1.0f);
        Assert.Equal(yellow, UIConstants.Colors.MenuItemSelected);
    }

    [Fact]
    public void Colors_MenuItemDisabled_ShouldBeGray()
    {
        var gray = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
        Assert.Equal(gray, UIConstants.Colors.MenuItemDisabled);
    }

    [Fact]
    public void Colors_SplashText_ShouldBeGray()
    {
        var gray = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
        Assert.Equal(gray, UIConstants.Colors.SplashText);
    }

    [Fact]
    public void Colors_CreditsTitle_ShouldBeWhite()
    {
        var white = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
        Assert.Equal(white, UIConstants.Colors.CreditsTitle);
    }

    [Fact]
    public void Colors_CreditsText_ShouldBeLightGray()
    {
        var lightGray = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
        Assert.Equal(lightGray, UIConstants.Colors.CreditsText);
    }

    [Fact]
    public void Spacing_MenuTitleLineHeight_ShouldBe24()
    {
        Assert.Equal(24, UIConstants.Spacing.MenuTitleLineHeight);
    }

    [Fact]
    public void Spacing_MenuItemVerticalSpacing_ShouldBe24()
    {
        Assert.Equal(24, UIConstants.Spacing.MenuItemVerticalSpacing);
    }

    [Fact]
    public void Spacing_MenuItemHorizontalSpacing_ShouldBe80()
    {
        Assert.Equal(80, UIConstants.Spacing.MenuItemHorizontalSpacing);
    }

    [Fact]
    public void Spacing_CreditsLineHeight_ShouldBe30()
    {
        Assert.Equal(30, UIConstants.Spacing.CreditsLineHeight);
    }

    [Fact]
    public void Strings_SplashPressEnter_ShouldBeCorrect()
    {
        Assert.Equal("PRESS ENTER", UIConstants.Strings.SplashPressEnter);
    }

    [Fact]
    public void Strings_MenuOptions_ShouldBeCorrect()
    {
        Assert.Equal("START GAME", UIConstants.Strings.MenuStartGame);
        Assert.Equal("OPTIONS", UIConstants.Strings.MenuOptions);
        Assert.Equal("QUIT", UIConstants.Strings.MenuQuit);
    }

    [Fact]
    public void Strings_QuitConfirmation_ShouldBeCorrect()
    {
        Assert.Equal("ARE YOU SURE YOU\nWANT TO QUIT", UIConstants.Strings.QuitTitle);
        Assert.Equal("YES", UIConstants.Strings.QuitYes);
        Assert.Equal("NO", UIConstants.Strings.QuitNo);
    }

    [Fact]
    public void Strings_RaceClass_ShouldBeCorrect()
    {
        Assert.Equal("SELECT RACE CLASS", UIConstants.Strings.RaceClassTitle);
        Assert.Equal("VENOM", UIConstants.Strings.RaceClassVenom);
        Assert.Equal("RAPIER", UIConstants.Strings.RaceClassRapier);
    }

    [Fact]
    public void Strings_Teams_ShouldBeCorrect()
    {
        Assert.Equal("AG SYSTEMS", UIConstants.Strings.TeamAGSystems);
        Assert.Equal("AURICOM", UIConstants.Strings.TeamAuricom);
        Assert.Equal("QIREX", UIConstants.Strings.TeamQirex);
        Assert.Equal("FEISAR", UIConstants.Strings.TeamFeisar);
    }

    [Fact]
    public void Strings_Circuits_ShouldBeCorrect()
    {
        Assert.Equal("ALTIMA VII", UIConstants.Strings.CircuitAltima);
        Assert.Equal("KARBONIS V", UIConstants.Strings.CircuitKarbonis);
        Assert.Equal("TERRAMAX", UIConstants.Strings.CircuitTerramax);
        Assert.Equal("KORODERA", UIConstants.Strings.CircuitKorodera);
        Assert.Equal("ARRIDOS IV", UIConstants.Strings.CircuitArridos);
        Assert.Equal("SILVERSTREAM", UIConstants.Strings.CircuitSilverstream);
        Assert.Equal("FIRESTAR", UIConstants.Strings.CircuitFirestar);
    }

    [Fact]
    public void Strings_CreditsLines_ShouldNotBeEmpty()
    {
        Assert.NotNull(UIConstants.Strings.CreditsLines);
        Assert.NotEmpty(UIConstants.Strings.CreditsLines);
        Assert.True(UIConstants.Strings.CreditsLines.Length > 20);
    }

    [Fact]
    public void Strings_CreditsLines_ShouldContainWipeout()
    {
        Assert.Contains("WIPEOUT", UIConstants.Strings.CreditsLines);
    }

    [Fact]
    public void Strings_CreditsTitles_ShouldNotBeEmpty()
    {
        Assert.NotNull(UIConstants.Strings.CreditsTitles);
        Assert.NotEmpty(UIConstants.Strings.CreditsTitles);
        Assert.Equal(7, UIConstants.Strings.CreditsTitles.Length);
    }

    [Fact]
    public void Strings_CreditsTitles_ShouldContainExpectedTitles()
    {
        Assert.Contains("WIPEOUT", UIConstants.Strings.CreditsTitles);
        Assert.Contains("PROGRAMMING", UIConstants.Strings.CreditsTitles);
        Assert.Contains("GRAPHICS", UIConstants.Strings.CreditsTitles);
        Assert.Contains("MUSIC", UIConstants.Strings.CreditsTitles);
    }

    [Fact]
    public void Colors_AllColors_ShouldHaveValidAlpha()
    {
        Assert.Equal(1.0f, UIConstants.Colors.MenuTitleDefault.A);
        Assert.Equal(1.0f, UIConstants.Colors.MenuItemDefault.A);
        Assert.Equal(1.0f, UIConstants.Colors.MenuItemSelected.A);
        Assert.Equal(1.0f, UIConstants.Colors.MenuItemDisabled.A);
        Assert.Equal(1.0f, UIConstants.Colors.SplashText.A);
        Assert.Equal(1.0f, UIConstants.Colors.CreditsTitle.A);
        Assert.Equal(1.0f, UIConstants.Colors.CreditsText.A);
    }
}
