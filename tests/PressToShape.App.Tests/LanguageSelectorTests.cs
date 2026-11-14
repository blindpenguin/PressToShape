using System.Globalization;
using PressToShape.App.Localization;
using Xunit;

namespace PressToShape.App.Tests;

public class LanguageSelectorTests
{
    [Theory]
    [InlineData("de", "en-US", "de")]
    [InlineData("en", "de-DE", "en")]
    [InlineData("DE", "en-US", "de")]
    [InlineData(" de ", "fr-FR", "de")]
    public void SavedChoice_WinsOverSystemLanguage(string saved, string system, string expected)
    {
        Assert.Equal(expected, LanguageSelector.Choose(saved, new CultureInfo(system)));
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("de-AT")]
    [InlineData("de-CH")]
    [InlineData("de")]
    public void NoSavedChoice_GermanSystem_StartsInGerman(string system)
    {
        Assert.Equal("de", LanguageSelector.Choose(null, new CultureInfo(system)));
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    [InlineData("ja-JP")]
    public void NoSavedChoice_OtherSystemLanguage_StartsInEnglish(string system)
    {
        Assert.Equal("en", LanguageSelector.Choose(null, new CultureInfo(system)));
    }

    [Theory]
    [InlineData("fr", "de-DE", "de")]
    [InlineData("xx", "en-US", "en")]
    [InlineData("", "de-DE", "de")]
    [InlineData("  ", "en-US", "en")]
    public void UnsupportedSavedChoice_IsIgnored(string saved, string system, string expected)
    {
        Assert.Equal(expected, LanguageSelector.Choose(saved, new CultureInfo(system)));
    }

    [Fact]
    public void InvariantCulture_FallsBackToEnglish()
    {
        Assert.Equal("en", LanguageSelector.Choose(null, CultureInfo.InvariantCulture));
    }
}
