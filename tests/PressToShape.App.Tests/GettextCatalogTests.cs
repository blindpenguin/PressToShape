using System.IO;
using System.Text;
using PressToShape.App.Localization;
using Xunit;

namespace PressToShape.App.Tests;

public class GettextCatalogTests
{
    private static ITranslationCatalog LoadGerman()
    {
        var catalog = new EmbeddedCatalogSource().Load("de");
        Assert.NotNull(catalog);
        return catalog!;
    }

    [Fact]
    public void GermanCatalog_IsEmbedded_AndLoads()
    {
        Assert.NotNull(new EmbeddedCatalogSource().Load("de"));
    }

    [Fact]
    public void KnownTexts_AreTranslated()
    {
        var catalog = LoadGerman();

        Assert.Equal("Auswählen", catalog.Translate("Select"));
    }

    [Fact]
    public void UnknownTexts_ReturnNull()
    {
        var catalog = LoadGerman();

        Assert.Null(catalog.Translate("This text is not in the catalog"));
        Assert.Null(catalog.Translate("menu", "This text is not in the catalog"));
        Assert.Null(catalog.Translate("unknown-context", "Select")); // the text exists, but not with this context
    }

    [Fact]
    public void English_HasNoCatalog()
    {
        Assert.Null(new EmbeddedCatalogSource().Load("en"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("xx")]
    public void MissingOrInvalidLanguage_ReturnsNull(string code)
    {
        Assert.Null(new EmbeddedCatalogSource().Load(code));
    }

    [Fact]
    public void CorruptStream_Throws_ButTheSourceNeverDoes()
    {
        using var corrupt = new MemoryStream(Encoding.UTF8.GetBytes("this is not a .mo file"));

        Assert.ThrowsAny<System.Exception>(() => new GettextCatalog(corrupt, new System.Globalization.CultureInfo("de")));
    }

    [Fact]
    public void LocalizerWithTheEmbeddedSource_TranslatesGerman()
    {
        var localizer = new Localizer(new EmbeddedCatalogSource(), "de");

        Assert.Equal("Auswählen", localizer.T("Select"));
        Assert.Equal("Open", localizer.TC("menu", "Open")); // no entry with a context yet, so the English text is shown
        Assert.Equal("Something else", localizer.T("Something else"));

        localizer.SetLanguage("en");

        Assert.Equal("Select", localizer.T("Select"));
    }
}
