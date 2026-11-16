using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

/// <summary>Guards the German catalog against the texts in the code. These tests read de.po directly and do not need msgfmt.</summary>
public class TranslationConsistencyTests
{
    private static readonly IReadOnlyList<PoEntry> German = PoFile.Read(RepoPaths.Combine("src/PressToShape.App/Translations/de.po"));

    private static string Key(string? context, string text) => context is null ? text : context + "|" + text;

    private static Dictionary<string, string> GermanDictionary() =>
        German.Where(e => !e.IsFuzzy && e.Translation.Length > 0).ToDictionary(e => Key(e.Context, e.Id), e => e.Translation);

    [Fact]
    public void SourcesContainTranslatableTexts()
    {
        Assert.True(SourceTexts.FindAll().Count > 100);
    }

    [Fact]
    public void EveryTextInTheCode_HasAGermanTranslation()
    {
        var german = GermanDictionary();

        var missing = SourceTexts.FindAll()
            .Where(t => !german.ContainsKey(Key(t.Context, t.Text)))
            .Select(t => $"{t.Text} ({t.File})")
            .Distinct()
            .ToList();

        Assert.True(missing.Count == 0, "Missing or empty or fuzzy German translations for: " + string.Join("; ", missing));
    }

    [Fact]
    public void EveryGermanEntry_IsUsedInTheCode()
    {
        var used = SourceTexts.FindAll().Select(t => Key(t.Context, t.Text)).ToHashSet();

        var obsolete = German.Where(e => !used.Contains(Key(e.Context, e.Id))).Select(e => e.Id).ToList();

        Assert.True(obsolete.Count == 0, "Obsolete entries in de.po: " + string.Join("; ", obsolete));
    }

    [Fact]
    public void GermanCatalog_HasNoDuplicateEntries()
    {
        var duplicates = German.GroupBy(e => Key(e.Context, e.Id)).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        Assert.True(duplicates.Count == 0, "Duplicate entries in de.po: " + string.Join("; ", duplicates));
    }

    [Fact]
    public void GermanCatalog_HasNoFuzzyOrEmptyEntries()
    {
        var bad = German.Where(e => e.IsFuzzy || e.Translation.Length == 0).Select(e => e.Id).ToList();

        Assert.True(bad.Count == 0, "Fuzzy or empty entries: " + string.Join("; ", bad));
    }

    [Fact]
    public void KeyHints_AndPlaceholders_AreKeptInTheTranslation()
    {
        var hint = new Regex(@"\([A-Z0-9]\)|\{\d+\}|\(\d+%\)", RegexOptions.Compiled);

        foreach (var entry in German)
        {
            var expected = hint.Matches(entry.Id).Select(m => m.Value).OrderBy(v => v).ToList();
            var actual = hint.Matches(entry.Translation).Select(m => m.Value).OrderBy(v => v).ToList();

            Assert.True(expected.SequenceEqual(actual), $"'{entry.Id}' -> '{entry.Translation}' must keep {string.Join(" ", expected)}");
        }
    }

    [Fact]
    public void Ellipses_AreKeptInTheTranslation()
    {
        foreach (var entry in German.Where(e => e.Id.EndsWith('…')))
        {
            Assert.EndsWith("…", entry.Translation);
        }
    }

    [Fact]
    public void AltMnemonics_ArePresentInBothLanguages_WhenTheEnglishTextHasOne()
    {
        foreach (var entry in German.Where(e => e.Id.Contains('_')))
        {
            Assert.Contains('_', entry.Translation);
        }
    }

    [Theory]
    [InlineData("en")]
    [InlineData("de")]
    public void MenuMnemonics_AreUniquePerMenuLevel(string language)
    {
        var localizer = new Localizer(new FakeCatalogSource().Add("de", GermanDictionary()), language);
        var menu = new MenuBarViewModel(
            new EditorState(), new NoShutdown(), localizer, _ => { }, () => { }, NoFileCommands(), NoEditCommands(), NoObjectCommands(), new RecentFiles(new MemorySettingsStore()), new SwitchableCommand(false), new SwitchableCommand(false));

        var problems = new List<string>();
        CheckLevel("menu bar", menu.Menus, problems);
        foreach (var item in menu.Menus.SelectMany(m => new[] { m }.Concat(m.Descendants())).Where(i => i.Children.Count > 0))
        {
            CheckLevel(item.Header, item.Children, problems);
        }

        Assert.True(problems.Count == 0, string.Join("; ", problems));
    }

    private static ObjectMenuCommands NoObjectCommands()
    {
        var none = new CommunityToolkit.Mvvm.Input.RelayCommand(() => { });
        return new ObjectMenuCommands(none, none, none, none, none, none, none, none, none, none);
    }

    private static EditMenuCommands NoEditCommands()
    {
        var none = new CommunityToolkit.Mvvm.Input.RelayCommand(() => { });
        return new EditMenuCommands(none, none, none, none, none, none, none, none);
    }

    private static FileMenuCommands NoFileCommands()
    {
        var none = new CommunityToolkit.Mvvm.Input.RelayCommand(() => { });
        return new FileMenuCommands(none, none, none, none, none, _ => none);
    }

    private static void CheckLevel(string level, IReadOnlyList<MenuItemViewModel> items, List<string> problems)
    {
        var letters = items
            .Where(i => !i.IsSeparator)
            .Select(i => (Header: i.Header, Index: i.Header.IndexOf('_')))
            .Where(x => x.Index >= 0 && x.Index + 1 < x.Header.Length)
            .Select(x => (x.Header, Letter: char.ToUpperInvariant(x.Header[x.Index + 1])))
            .ToList();

        foreach (var group in letters.GroupBy(x => x.Letter).Where(g => g.Count() > 1))
        {
            problems.Add($"Mnemonic '{group.Key}' is used twice in '{level}': {string.Join(" / ", group.Select(x => x.Header))}");
        }
    }

    private sealed class NoShutdown : IAppShutdown
    {
        public void Shutdown()
        {
        }
    }
}
