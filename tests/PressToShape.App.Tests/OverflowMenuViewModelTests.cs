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

        public SwitchableCommand Preferences { get; } = new(true);

        public FileMenuCommands File => new(New, Open, Save, SaveAs, ExportSvg, _ => new SwitchableCommand(true));

        public EditMenuCommands Edit => new(Cut, Copy, Paste, PasteInPlace, Duplicate, Delete, SelectAll, Deselect);

        public SwitchableCommand Group { get; } = new(true);

        public SwitchableCommand Ungroup { get; } = new(true);

        public SwitchableCommand Front { get; } = new(true);

        public SwitchableCommand Forward { get; } = new(true);

        public SwitchableCommand Backward { get; } = new(true);

        public SwitchableCommand Back { get; } = new(true);

        public SwitchableCommand Lock { get; } = new(true);

        public SwitchableCommand UnlockAll { get; } = new(true);

        public SwitchableCommand Hide { get; } = new(true);

        public SwitchableCommand ShowAll { get; } = new(true);

        public ObjectMenuCommands Objects => new(Group, Ungroup, Front, Forward, Backward, Back, Lock, UnlockAll, Hide, ShowAll);

        public SwitchableCommand Union { get; } = new(true);

        public SwitchableCommand Difference { get; } = new(true);

        public SwitchableCommand Intersection { get; } = new(true);

        public SwitchableCommand Exclusion { get; } = new(true);

        public SwitchableCommand Combine { get; } = new(true);

        public SwitchableCommand BreakApart { get; } = new(true);

        public SwitchableCommand ConvertToPath { get; } = new(true);

        public SwitchableCommand ReverseDirection { get; } = new(true);

        public PathMenuCommands Paths => new(Union, Difference, Intersection, Exclusion, Combine, BreakApart, ConvertToPath, ReverseDirection);
    }

    private static OverflowMenuViewModel Create(Commands commands, ILocalizer? localizer = null) =>
        new(localizer ?? FakeLocalizer.Create(), commands.File, commands.Edit, commands.Objects, commands.Paths, commands.Preferences);

    private static MenuItemViewModel Find(OverflowMenuViewModel menu, string plainHeader) => menu.Items.Single(i => i.PlainHeader == plainHeader);

    [Fact]
    public void Items_AreTheFileActions_TheEditActions_TheObjectAndPathActions_AndTheLanguage_InOrder()
    {
        var menu = Create(new Commands());

        Assert.Equal(
            new[]
            {
                "New", "Open…", "Save", "Save As…", "Export as SVG…", "-",
                "Cut", "Copy", "Paste", "Paste in Place", "Duplicate", "Delete", "Select All", "Deselect", "-",
                "Group", "Ungroup", "Arrange", "Path", "Lock", "Unlock All", "Hide", "Show All", "-",
                "Preferences…", "Language",
            },
            menu.Items.Select(i => i.PlainHeader));
    }

    [Fact]
    public void TheArrangeSubmenu_HasTheFourArrangeCommands()
    {
        var commands = new Commands();
        var arrange = Find(Create(commands), "Arrange");

        Assert.Equal(["Bring to Front", "Bring Forward", "Send Backward", "Send to Back"], arrange.Children.Select(c => c.PlainHeader));
        arrange.Children[0].Command!.Execute(null);
        arrange.Children[3].Command!.Execute(null);
        Assert.Equal(1, commands.Front.Executions);
        Assert.Equal(1, commands.Back.Executions);
        Assert.True(arrange.IsEnabled);
    }

    [Fact]
    public void ThePathSubmenu_HasTheEightPathCommandsInOrderAndRunsThem()
    {
        var commands = new Commands();
        var path = Find(Create(commands), "Path");

        Assert.Equal(
            ["Convert Object to Path", "-", "Combine", "Break Apart", "-", "Union", "Difference", "Intersection", "Exclusion", "-", "Reverse Direction"],
            path.Children.Select(c => c.IsSeparator ? "-" : c.PlainHeader));
        foreach (var item in path.Children.Where(c => !c.IsSeparator))
        {
            item.Command!.Execute(null);
        }

        Assert.Equal(
            [1, 1, 1, 1, 1, 1, 1, 1],
            new[] { commands.ConvertToPath, commands.Combine, commands.BreakApart, commands.Union, commands.Difference, commands.Intersection, commands.Exclusion, commands.ReverseDirection }.Select(c => c.Executions));
        Assert.True(path.IsEnabled);
    }

    [Fact]
    public void ThePathSubmenu_ConvertAndReverseItems_FollowTheirCommands_AndAreDisabledWithoutCommands()
    {
        var commands = new Commands();
        var path = Find(Create(commands), "Path");
        var convert = path.Children.Single(c => c.PlainHeader == "Convert Object to Path");
        var reverse = path.Children.Single(c => c.PlainHeader == "Reverse Direction");
        Assert.True(convert.IsEnabled);
        Assert.True(reverse.IsEnabled);

        commands.ConvertToPath.Set(false);
        commands.ReverseDirection.Set(false);

        Assert.False(convert.IsEnabled);
        Assert.False(reverse.IsEnabled);

        var plain = new PathMenuCommands(commands.Union, commands.Difference, commands.Intersection, commands.Exclusion, commands.Combine, commands.BreakApart);
        var withoutThem = Find(new OverflowMenuViewModel(FakeLocalizer.Create(), commands.File, commands.Edit, commands.Objects, plain), "Path");
        Assert.False(withoutThem.Children.Single(c => c.PlainHeader == "Convert Object to Path").IsEnabled);
        Assert.False(withoutThem.Children.Single(c => c.PlainHeader == "Reverse Direction").IsEnabled);
    }

    [Fact]
    public void ThePathSubmenu_ItemsFollowTheirCommands()
    {
        var commands = new Commands();
        var path = Find(Create(commands), "Path");
        var union = path.Children.Single(c => c.PlainHeader == "Union");

        commands.Union.Set(false);

        Assert.False(union.IsEnabled);
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
            ("Group", commands.Group), ("Ungroup", commands.Ungroup), ("Lock", commands.Lock), ("Unlock All", commands.UnlockAll),
            ("Hide", commands.Hide), ("Show All", commands.ShowAll),
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
    public void Preferences_Item_RunsTheCommandAndFollowsItsAvailability()
    {
        var commands = new Commands();
        var item = Find(Create(commands), "Preferences…");

        Assert.True(item.IsEnabled);
        item.Command!.Execute(null);
        Assert.Equal(1, commands.Preferences.Executions);

        commands.Preferences.Set(false);
        Assert.False(item.IsEnabled);
    }

    [Fact]
    public void Preferences_WithoutACommand_IsDisabled()
    {
        var commands = new Commands();
        var menu = new OverflowMenuViewModel(FakeLocalizer.Create(), commands.File, commands.Edit, commands.Objects, commands.Paths);

        Assert.False(Find(menu, "Preferences…").IsEnabled);
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

        Assert.Throws<System.ArgumentNullException>(() => new OverflowMenuViewModel(null!, commands.File, commands.Edit, commands.Objects, commands.Paths));
        Assert.Throws<System.ArgumentNullException>(() => new OverflowMenuViewModel(FakeLocalizer.Create(), null!, commands.Edit, commands.Objects, commands.Paths));
        Assert.Throws<System.ArgumentNullException>(() => new OverflowMenuViewModel(FakeLocalizer.Create(), commands.File, null!, commands.Objects, commands.Paths));
        Assert.Throws<System.ArgumentNullException>(() => new OverflowMenuViewModel(FakeLocalizer.Create(), commands.File, commands.Edit, null!, commands.Paths));
        Assert.Throws<System.ArgumentNullException>(() => new OverflowMenuViewModel(FakeLocalizer.Create(), commands.File, commands.Edit, commands.Objects, null!));
    }
}
