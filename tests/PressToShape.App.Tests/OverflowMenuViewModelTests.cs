using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.ViewModels.Tablet;
using Xunit;

namespace PressToShape.App.Tests;

public class OverflowMenuViewModelTests
{
    private sealed class Commands
    {
        public SwitchableCommand New { get; } = new(true);

        public SwitchableCommand Open { get; } = new(true);

        public SwitchableCommand Save { get; } = new(true);

        public SwitchableCommand SaveAs { get; } = new(true);

        public SwitchableCommand ExportSvg { get; } = new(true);

        public SwitchableCommand Cut { get; } = new(true);

        public SwitchableCommand Copy { get; } = new(true);

        public SwitchableCommand Paste { get; } = new(true);

        public SwitchableCommand PasteInPlace { get; } = new(true);

        public SwitchableCommand Duplicate { get; } = new(true);

        public SwitchableCommand Delete { get; } = new(true);

        public SwitchableCommand SelectAll { get; } = new(true);

        public SwitchableCommand Deselect { get; } = new(true);

        public FileMenuCommands File => new(New, Open, Save, SaveAs, ExportSvg, _ => new SwitchableCommand(true));

        public EditMenuCommands Edit => new(Cut, Copy, Paste, PasteInPlace, Duplicate, Delete, SelectAll, Deselect);
    }

    private static OverflowMenuViewModel Create(Commands commands, ILocalizer? localizer = null) =>
        new(localizer ?? FakeLocalizer.Create(), commands.File, commands.Edit);

    private static MenuItemViewModel Find(OverflowMenuViewModel menu, string plainHeader) => menu.Items.Single(i => i.PlainHeader == plainHeader);

    [Fact]
    public void Items_AreTheFileActions_TheEditActions_AndTheLanguage_InOrder()
    {
        var menu = Create(new Commands());

        Assert.Equal(
            new[]
            {
                "New", "Open…", "Save", "Save As…", "Export as SVG…", "-",
                "Cut", "Copy", "Paste", "Paste in Place", "Duplicate", "Delete", "Select All", "Deselect", "-",
                "Language",
            },
            menu.Items.Select(i => i.PlainHeader));
    }

    [Fact]
    public void Items_HaveNoQuitAndNoRecentFiles()
    {
        var menu = Create(new Commands());
        var all = menu.Items.SelectMany(i => new[] { i }.Concat(i.Descendants())).Select(i => i.PlainHeader).ToList();

        Assert.DoesNotContain(all, h => h.Contains("Quit"));
        Assert.DoesNotContain(all, h => h.Contains("Recent"));
    }

    [Fact]
    public void Items_ShowNoShortcutTexts()
    {
        var menu = Create(new Commands());

        Assert.All(menu.Items, i => Assert.Null(i.Gesture));
    }

    [Fact]
    public void Items_RunTheMatchingCommand()
    {
        var commands = new Commands();
        var menu = Create(commands);
        var expected = new (string Header, SwitchableCommand Command)[]
        {
            ("New", commands.New), ("Open…", commands.Open), ("Save", commands.Save), ("Save As…", commands.SaveAs),
            ("Export as SVG…", commands.ExportSvg), ("Cut", commands.Cut), ("Copy", commands.Copy), ("Paste", commands.Paste),
            ("Paste in Place", commands.PasteInPlace), ("Duplicate", commands.Duplicate), ("Delete", commands.Delete),
            ("Select All", commands.SelectAll), ("Deselect", commands.Deselect),
        };

        foreach (var (header, command) in expected)
        {
            Find(menu, header).Command!.Execute(null);

            Assert.Equal(1, command.Executions);
        }
    }

    [Fact]
    public void AnItemWhoseCommandCannotRun_IsDisabled_AndFollowsTheCommand()
    {
        var commands = new Commands();
        commands.Paste.Set(false);
        var menu = Create(commands);
        var paste = Find(menu, "Paste");
        var raised = new List<string?>();
        paste.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        Assert.False(paste.IsEnabled);

        commands.Paste.Set(true);

        Assert.True(paste.IsEnabled);
        Assert.Contains(nameof(MenuItemViewModel.IsEnabled), raised);
    }

    [Fact]
    public void TheLanguageSubmenu_IsAlwaysEnabled_AndListsBothLanguages()
    {
        var language = Find(Create(new Commands()), "Language");

        Assert.True(language.IsEnabled);
        Assert.Equal(new[] { "English", "Deutsch" }, language.Children.Select(c => c.Header));
        Assert.All(language.Children, c => Assert.True(c.IsCheckable));
    }

    [Fact]
    public void ChoosingALanguage_SwitchesIt_MovesTheCheckMark_AndTranslatesTheHeaders()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["_Save"] = "_Speichern", ["_Language"] = "_Sprache" });
        var menu = Create(new Commands(), localizer);
        var language = Find(menu, "Language");
        var english = language.Children.Single(c => c.Header == "English");
        var german = language.Children.Single(c => c.Header == "Deutsch");
        Assert.True(english.IsChecked);
        Assert.False(german.IsChecked);

        german.Command!.Execute(null);

        Assert.Equal("de", localizer.LanguageCode);
        Assert.True(german.IsChecked);
        Assert.False(english.IsChecked);
        Assert.Equal("Speichern", menu.Items[2].PlainHeader);
        Assert.Equal("Sprache", language.PlainHeader);
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        var commands = new Commands();

        Assert.Throws<System.ArgumentNullException>(() => new OverflowMenuViewModel(null!, commands.File, commands.Edit));
        Assert.Throws<System.ArgumentNullException>(() => new OverflowMenuViewModel(FakeLocalizer.Create(), null!, commands.Edit));
        Assert.Throws<System.ArgumentNullException>(() => new OverflowMenuViewModel(FakeLocalizer.Create(), commands.File, null!));
    }
}
