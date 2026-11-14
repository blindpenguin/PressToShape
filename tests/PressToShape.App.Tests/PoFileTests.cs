using System.Linq;
using Xunit;

namespace PressToShape.App.Tests;

public class PoFileTests
{
    [Fact]
    public void Parse_ReadsEntries_WithContext_AndSkipsTheHeader()
    {
        var entries = PoFile.Parse("""
            # comment
            msgid ""
            msgstr ""
            "Language: de\n"

            msgid "Select"
            msgstr "Auswählen"

            msgctxt "menu"
            msgid "Open"
            msgstr "Öffnen"
            """);

        Assert.Equal(2, entries.Count);
        Assert.Equal(new PoEntry(null, "Select", "Auswählen", false), entries[0]);
        Assert.Equal(new PoEntry("menu", "Open", "Öffnen", false), entries[1]);
    }

    [Fact]
    public void Parse_JoinsMultiLineStrings_AndResolvesEscapes()
    {
        var entries = PoFile.Parse("msgid \"\"\n\"First \"\n\"line\"\nmsgstr \"A \\\"quoted\\\" word\\nnext\"\n");

        Assert.Equal("First line", entries.Single().Id);
        Assert.Equal("A \"quoted\" word\nnext", entries.Single().Translation);
    }

    [Fact]
    public void Parse_MarksFuzzyEntries()
    {
        var entries = PoFile.Parse("#, fuzzy\nmsgid \"Select\"\nmsgstr \"Wählen\"\n");

        Assert.True(entries.Single().IsFuzzy);
    }

    [Fact]
    public void Parse_IgnoresObsoleteEntries()
    {
        var entries = PoFile.Parse("#~ msgid \"Old\"\n#~ msgstr \"Alt\"\n\nmsgid \"New\"\nmsgstr \"Neu\"\n");

        Assert.Equal("New", entries.Single().Id);
    }

    [Fact]
    public void Parse_EntryWithoutTranslation_HasAnEmptyTranslation()
    {
        var entries = PoFile.Parse("msgid \"Select\"\nmsgstr \"\"\n");

        Assert.Equal(string.Empty, entries.Single().Translation);
    }
}
