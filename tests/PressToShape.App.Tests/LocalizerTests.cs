using System.Collections.Generic;
using System.Globalization;
using PressToShape.App.Localization;
using Xunit;

namespace PressToShape.App.Tests;

public class LocalizerTests
{
    private static readonly Dictionary<string, string> German = new()
    {
        ["Select"] = "Auswählen",
        ["Empty"] = "",
        ["menu|Open"] = "Öffnen",
        ["toolbar|Open"] = "Öffnen (Werkzeugleiste)",
    };

    [Fact]
    public void English_ReturnsTheSourceText()
    {
        var localizer = FakeLocalizer.Create("en", German);

        Assert.Equal("Select", localizer.T("Select"));
        Assert.Equal("Open", localizer.TC("menu", "Open"));
        Assert.Equal("en", localizer.LanguageCode);
        Assert.Equal("en", localizer.Culture.TwoLetterISOLanguageName);
    }

    [Fact]
    public void English_DoesNotLoadACatalog()
    {
        var source = new FakeCatalogSource().Add("de", German);

        _ = new Localizer(source, "en");

        Assert.Equal(0, source.LoadCount);
    }

    [Fact]
    public void German_TranslatesKnownTexts()
    {
        var localizer = FakeLocalizer.Create("de", German);

        Assert.Equal("Auswählen", localizer.T("Select"));
        Assert.Equal("de", localizer.LanguageCode);
        Assert.Equal("de", localizer.Culture.TwoLetterISOLanguageName);
    }

    [Fact]
    public void German_MissingTranslation_FallsBackToEnglish()
    {
        var localizer = FakeLocalizer.Create("de", German);

        Assert.Equal("Unknown text", localizer.T("Unknown text"));
    }

    [Fact]
    public void German_UsesTheContext()
    {
        var localizer = FakeLocalizer.Create("de", German);

        Assert.Equal("Öffnen", localizer.TC("menu", "Open"));
        Assert.Equal("Öffnen (Werkzeugleiste)", localizer.TC("toolbar", "Open"));
        Assert.Equal("Open", localizer.TC("other", "Open"));
    }

    [Fact]
    public void CatalogWithoutEntries_FallsBackToEnglish()
    {
        var localizer = new Localizer(new FakeCatalogSource(), "de"); // no catalog for de at all

        Assert.Equal("Select", localizer.T("Select"));
        Assert.Equal("de", localizer.LanguageCode);
    }

    [Fact]
    public void SetLanguage_SwitchesAndRaisesTheEventOnce()
    {
        var localizer = FakeLocalizer.Create("en", German);
        var raised = 0;
        localizer.LanguageChanged += (_, _) => raised++;

        localizer.SetLanguage("de");

        Assert.Equal(1, raised);
        Assert.Equal("Auswählen", localizer.T("Select"));
        Assert.Equal("de", localizer.Culture.TwoLetterISOLanguageName);
    }

    [Fact]
    public void SetLanguage_BackToEnglish_ReturnsSourceText()
    {
        var localizer = FakeLocalizer.Create("de", German);

        localizer.SetLanguage("en");

        Assert.Equal("Select", localizer.T("Select"));
        Assert.Equal("en", localizer.LanguageCode);
    }

    [Fact]
    public void SetLanguage_ToTheCurrentLanguage_DoesNothing()
    {
        var localizer = FakeLocalizer.Create("de", German);
        var raised = 0;
        localizer.LanguageChanged += (_, _) => raised++;

        localizer.SetLanguage("de");
        localizer.SetLanguage("DE");

        Assert.Equal(0, raised);
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("xx-YY")]
    public void SetLanguage_UnknownCode_IsIgnored(string code)
    {
        var localizer = FakeLocalizer.Create("en", German);
        var raised = 0;
        localizer.LanguageChanged += (_, _) => raised++;

        localizer.SetLanguage(code);

        Assert.Equal(0, raised);
        Assert.Equal("en", localizer.LanguageCode);
    }

    [Fact]
    public void UnknownInitialLanguage_StartsInEnglish()
    {
        var localizer = FakeLocalizer.Create("xx", German);

        Assert.Equal("en", localizer.LanguageCode);
    }

    [Fact]
    public void LanguageCodes_AreCaseInsensitive()
    {
        var localizer = FakeLocalizer.Create("en", German);

        localizer.SetLanguage("De");

        Assert.Equal("de", localizer.LanguageCode);
        Assert.Equal("Auswählen", localizer.T("Select"));
    }

    [Fact]
    public void Languages_ListEnglishAndGerman_WithOwnNames()
    {
        var localizer = FakeLocalizer.Create();

        Assert.Collection(
            localizer.Languages,
            l => { Assert.Equal("en", l.Code); Assert.Equal("English", l.DisplayName); },
            l => { Assert.Equal("de", l.Code); Assert.Equal("Deutsch", l.DisplayName); });
    }

    [Fact]
    public void N_ReturnsItsArgument()
    {
        Assert.Equal("Select", Localizer.N("Select"));
    }

    [Fact]
    public void CultureUsedForFormatting_FollowsTheLanguage()
    {
        var localizer = FakeLocalizer.Create("en", German);
        Assert.Equal("80.75", 80.75.ToString("0.##", localizer.Culture));

        localizer.SetLanguage("de");

        Assert.Equal("80,75", 80.75.ToString("0.##", localizer.Culture));
        Assert.Equal(CultureInfo.GetCultureInfo("de"), localizer.Culture);
    }
}
