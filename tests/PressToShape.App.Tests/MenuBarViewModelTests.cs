using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class MenuBarViewModelTests
{
    private sealed class FakeShutdown : IAppShutdown
    {
        public int Calls { get; private set; }

        public void Shutdown() => Calls++;
    }

    private sealed class Fixture
    {
        public EditorState State { get; } = new();

        public FakeShutdown Shutdown { get; } = new();

        public List<InspectorTab> ShownTabs { get; } = [];

        public int FitCalls { get; private set; }

        public List<string> FileCalls { get; } = [];

        public RecentFiles Recent { get; } = new(new MemorySettingsStore());

        public SwitchableCommand Undo { get; } = new(false);

        public SwitchableCommand Redo { get; } = new(false);

        public MenuBarViewModel Menu { get; }

        public Localizer Localizer { get; }

        public LayerEditor Layers { get; }

        public Dictionary<string, SwitchableCommand> EditCommandsByName { get; } = new[] { "cut", "copy", "paste", "paste in place", "duplicate", "delete", "select all", "deselect" }
            .ToDictionary(name => name, _ => new SwitchableCommand(false));

        public Dictionary<string, SwitchableCommand> ObjectCommandsByName { get; } = new[]
            {
                "group", "ungroup", "front", "forward", "backward", "back", "lock", "unlock all", "hide", "show all",
            }
            .ToDictionary(name => name, _ => new SwitchableCommand(false));

        public Dictionary<string, SwitchableCommand> PathCommandsByName { get; } = new[] { "union", "difference", "intersection", "exclusion", "combine", "break apart", "convert", "reverse" }
            .ToDictionary(name => name, _ => new SwitchableCommand(false));

        private PathMenuCommands PathCommands() => new(
            PathCommandsByName["union"],
            PathCommandsByName["difference"],
            PathCommandsByName["intersection"],
            PathCommandsByName["exclusion"],
            PathCommandsByName["combine"],
            PathCommandsByName["break apart"],
            PathCommandsByName["convert"],
            PathCommandsByName["reverse"]);

        private ObjectMenuCommands ObjectCommands() => new(
            ObjectCommandsByName["group"],
            ObjectCommandsByName["ungroup"],
            ObjectCommandsByName["front"],
            ObjectCommandsByName["forward"],
            ObjectCommandsByName["backward"],
            ObjectCommandsByName["back"],
            ObjectCommandsByName["lock"],
            ObjectCommandsByName["unlock all"],
            ObjectCommandsByName["hide"],
            ObjectCommandsByName["show all"]);

        private EditMenuCommands EditCommands() => new(
            EditCommandsByName["cut"],
            EditCommandsByName["copy"],
            EditCommandsByName["paste"],
            EditCommandsByName["paste in place"],
            EditCommandsByName["duplicate"],
            EditCommandsByName["delete"],
            EditCommandsByName["select all"],
            EditCommandsByName["deselect"]);

        public Fixture()
        {
            Localizer = FakeLocalizer.Create("en", new Dictionary<string, string>
            {
                ["_File"] = "_Datei",
                ["_View"] = "_Ansicht",
                ["_New"] = "_Neu",
                ["Zoom _In"] = "_Vergrößern",
            });
            var files = new FileMenuCommands(
                new RelayCommand(() => FileCalls.Add("new")),
                new RelayCommand(() => FileCalls.Add("open")),
                new RelayCommand(() => FileCalls.Add("save")),
                new RelayCommand(() => FileCalls.Add("save as")),
                new RelayCommand(() => FileCalls.Add("export")),
                path => new RelayCommand(() => FileCalls.Add("recent " + path)));
            Layers = new LayerEditor(State, Localizer);
            Menu = new MenuBarViewModel(State, Shutdown, Localizer, ShownTabs.Add, () => FitCalls++, files, EditCommands(), ObjectCommands(), new LayerMenuCommands(Layers), PathCommands(), Recent, Undo, Redo);
        }
    }

    private static string Clean(string header) => header.Replace("_", string.Empty);

    private static IEnumerable<MenuItemViewModel> Flatten(IEnumerable<MenuItemViewModel> items) =>
        items.SelectMany(i => new[] { i }.Concat(Flatten(i.Children)));

    private static MenuItemViewModel Find(Fixture f, string header) =>
        Flatten(f.Menu.Menus).Single(i => Clean(i.Header) == header);

    [Fact]
    public void TopLevelMenus_AreInMockupOrder()
    {
        var f = new Fixture();

        Assert.Equal(
            new[] { "File", "Edit", "Object", "Layer", "Path", "Text", "View", "Window", "Help" },
            f.Menu.Menus.Select(m => Clean(m.Header)));
    }

    [Fact]
    public void OnlyTheImplementedItems_AreEnabledLeafItems()
    {
        var f = new Fixture();

        var enabledLeaves = Flatten(f.Menu.Menus)
            .Where(i => !i.IsSeparator && i.Children.Count == 0 && i.IsEnabled)
            .Select(i => Clean(i.Header))
            .OrderBy(h => h)
            .ToList();

        var expected = new[]
        {
            "New", "Open…", "Save", "Save As…", "Export as SVG…",
            "Quit", "Zoom In", "Zoom Out", "Fit Artboard in Window", "Actual Size (100%)",
            "Properties", "Layers", "Assets", "History",
            "English", "Deutsch",
            "Add Layer", "Duplicate Layer",
            "Show Rulers", "Show Grid", "Snap to Grid", "Snap to Objects", "Smart Guides", "Show Guides",
        }.OrderBy(h => h);

        Assert.Equal(expected, enabledLeaves);
    }

    [Fact]
    public void PathMenu_BooleanCombineAndBreakApartItems_FollowTheirCommands()
    {
        var f = new Fixture();
        var items = new Dictionary<string, string>
        {
            ["Union"] = "union",
            ["Difference"] = "difference",
            ["Intersection"] = "intersection",
            ["Exclusion"] = "exclusion",
            ["Combine"] = "combine",
            ["Break Apart"] = "break apart",
        };

        foreach (var (header, command) in items)
        {
            Assert.False(Find(f, header).IsEnabled);
            f.PathCommandsByName[command].Set(true);
            Assert.True(Find(f, header).IsEnabled);
            Find(f, header).Command!.Execute(null);
            Assert.Equal(1, f.PathCommandsByName[command].Executions);
        }
    }

    [Fact]
    public void PathMenu_ConvertAndReverse_FollowTheirCommands_AndConvertShowsItsShortcut()
    {
        var f = new Fixture();
        var convert = Find(f, "Convert Object to Path");
        var reverse = Find(f, "Reverse Direction");

        Assert.Equal("Ctrl+Shift+O", convert.Gesture);
        Assert.False(convert.IsEnabled);
        Assert.False(reverse.IsEnabled);

        f.PathCommandsByName["convert"].Set(true);
        f.PathCommandsByName["reverse"].Set(true);
        convert.Command!.Execute(null);
        reverse.Command!.Execute(null);

        Assert.True(convert.IsEnabled);
        Assert.True(reverse.IsEnabled);
        Assert.Equal(1, f.PathCommandsByName["convert"].Executions);
        Assert.Equal(1, f.PathCommandsByName["reverse"].Executions);
    }

    [Fact]
    public void PathMenu_StrokeToPathAndSimplify_StayDisabledPlaceholders()
    {
        var f = new Fixture();

        Assert.False(Find(f, "Stroke to Path").IsEnabled);
        Assert.False(Find(f, "Simplify").IsEnabled);
    }

    [Fact]
    public void LayerMenu_HasTheLayerCommandsInOrder()
    {
        var f = new Fixture();

        var menu = f.Menu.Menus.Single(m => Clean(m.Header) == "Layer");

        Assert.Equal(
            new[] { "Add Layer", "Duplicate Layer", "Delete Layer", "-", "Merge Down", "-", "Move Up", "Move Down", "-", "Move Selection" },
            menu.Children.Select(i => i.IsSeparator ? "-" : Clean(i.Header)));
        Assert.Equal(["To Layer Above", "To Layer Below"], Find(f, "Move Selection").Children.Select(i => Clean(i.Header)));
    }

    [Fact]
    public void LayerMenu_InOneLayerDocument_OnlyAddAndDuplicateAreEnabled()
    {
        var f = new Fixture();

        var enabled = f.Menu.Menus.Single(m => Clean(m.Header) == "Layer").Descendants()
            .Where(i => !i.IsSeparator && i.Children.Count == 0 && i.IsEnabled)
            .Select(i => Clean(i.Header));

        Assert.Equal(["Add Layer", "Duplicate Layer"], enabled);
    }

    [Fact]
    public void LayerMenu_AddLayer_RunsTheCommandAndEnablesTheOthers()
    {
        var f = new Fixture();

        f.Menu.Menus.Single(m => Clean(m.Header) == "Layer").Children[0].Command!.Execute(null);

        Assert.Equal(2, f.State.Document.Layers.Length);
        Assert.True(f.Layers.CanDelete);
        Assert.True(f.Layers.CanMerge);
        Assert.True(Find(f, "Delete Layer").Command!.CanExecute(null));
        Assert.True(Find(f, "Merge Down").Command!.CanExecute(null));
    }

    [Fact]
    public void EnabledItems_HaveACommandOrAreSubmenus()
    {
        var f = new Fixture();

        foreach (var item in Flatten(f.Menu.Menus).Where(i => i.IsEnabled))
        {
            Assert.True(item.Command is not null || item.Children.Count > 0, item.Header);
        }
    }

    [Fact]
    public void ItemsWithoutCommand_AreDisabled()
    {
        var f = new Fixture();

        Assert.All(
            Flatten(f.Menu.Menus).Where(i => i.Command is null && i.Children.Count == 0),
            i => Assert.False(i.IsEnabled, i.Header));
    }

    [Fact]
    public void Separators_AreMarked_AndDisabled()
    {
        var f = new Fixture();

        var separators = Flatten(f.Menu.Menus).Where(i => i.IsSeparator).ToList();

        Assert.NotEmpty(separators);
        Assert.All(separators, s =>
        {
            Assert.Equal("-", s.Header);
            Assert.False(s.IsEnabled);
        });
    }

    [Fact]
    public void Submenus_AreEnabled_SoTheyCanBeOpened()
    {
        var f = new Fixture();

        foreach (var name in new[] { "File", "Edit", "Object", "Layer", "Path", "Text", "View", "Window", "Help" })
        {
            Assert.True(Find(f, name).IsEnabled, name);
        }

        // Their leaf items are still disabled until a feature exists.
        Assert.False(Find(f, "Undo").IsEnabled);
        Assert.False(Find(f, "About PressToShape").IsEnabled);
    }

    [Fact]
    public void Quit_CallsShutdown()
    {
        var f = new Fixture();

        Find(f, "Quit").Command!.Execute(null);

        Assert.Equal(1, f.Shutdown.Calls);
    }

    [Fact]
    public void ZoomItems_ChangeEditorState()
    {
        var f = new Fixture();

        Find(f, "Zoom In").Command!.Execute(null);
        Assert.Equal(150, f.State.ZoomPercent);

        Find(f, "Zoom Out").Command!.Execute(null);
        Find(f, "Zoom Out").Command!.Execute(null);
        Assert.Equal(75, f.State.ZoomPercent);

        Find(f, "Actual Size (100%)").Command!.Execute(null);
        Assert.Equal(100, f.State.ZoomPercent);
    }

    [Fact]
    public void FitItem_InvokesFitCallback()
    {
        var f = new Fixture();

        Find(f, "Fit Artboard in Window").Command!.Execute(null);

        Assert.Equal(1, f.FitCalls);
    }

    [Fact]
    public void WindowItems_ShowTheMatchingInspectorTab()
    {
        var f = new Fixture();

        Find(f, "Layers").Command!.Execute(null);
        Find(f, "History").Command!.Execute(null);
        Find(f, "Properties").Command!.Execute(null);
        Find(f, "Assets").Command!.Execute(null);

        Assert.Equal(
            new[] { InspectorTab.Layers, InspectorTab.History, InspectorTab.Properties, InspectorTab.Assets },
            f.ShownTabs);
    }

    [Fact]
    public void ViewMenu_ZoomItems_ShowGestures()
    {
        var f = new Fixture();

        Assert.Equal("Ctrl+0", Find(f, "Fit Artboard in Window").Gesture);
        Assert.Equal("Ctrl+1", Find(f, "Actual Size (100%)").Gesture);
        Assert.Equal("Ctrl+Q", Find(f, "Quit").Gesture);
    }

    [Fact]
    public void Headers_ChangeWithTheLanguage()
    {
        var f = new Fixture();
        var file = f.Menu.Menus.First();
        var newItem = file.Children.First();
        Assert.Equal("_File", file.Header);
        Assert.Equal("_New", newItem.Header);

        f.Localizer.SetLanguage("de");

        Assert.Equal("_Datei", file.Header);
        Assert.Equal("_Neu", newItem.Header);
    }

    [Fact]
    public void UntranslatedHeaders_FallBackToEnglish()
    {
        var f = new Fixture();

        f.Localizer.SetLanguage("de");

        Assert.Equal("_Edit", f.Menu.Menus[1].Header);
    }

    [Fact]
    public void LanguageChange_NotifiesAllProperties_SoBindingsRefresh()
    {
        var f = new Fixture();
        var item = f.Menu.Menus.First();
        var names = new List<string?>();
        item.PropertyChanged += (_, e) => names.Add(e.PropertyName);

        f.Localizer.SetLanguage("de");

        Assert.Contains(string.Empty, names);
    }

    [Fact]
    public void Separators_StayUntranslated()
    {
        var f = new Fixture();
        f.Localizer.SetLanguage("de");

        Assert.All(Flatten(f.Menu.Menus).Where(i => i.IsSeparator), s => Assert.Equal("-", s.Header));
    }

    private static MenuItemViewModel LanguageMenu(Fixture f) =>
        Flatten(f.Menu.Menus).Single(i => Clean(i.Header) == "Language");

    [Fact]
    public void LanguageSubmenu_IsInTheViewMenu_WithBothLanguages()
    {
        var f = new Fixture();

        var view = f.Menu.Menus.Single(m => Clean(m.Header) == "View");
        var language = LanguageMenu(f);

        Assert.Contains(language, view.Children);
        Assert.True(language.IsEnabled);
        Assert.Equal(new[] { "English", "Deutsch" }, language.Children.Select(c => c.Header));
        Assert.All(language.Children, c =>
        {
            Assert.True(c.IsCheckable);
            Assert.True(c.IsEnabled);
        });
    }

    [Fact]
    public void LanguageItems_AreNeverTranslated()
    {
        var f = new Fixture();
        f.Localizer.SetLanguage("de");

        Assert.Equal(new[] { "English", "Deutsch" }, LanguageMenu(f).Children.Select(c => c.Header));
    }

    [Fact]
    public void CurrentLanguage_IsChecked()
    {
        var f = new Fixture();
        var items = LanguageMenu(f).Children;

        Assert.True(items[0].IsChecked);
        Assert.False(items[1].IsChecked);
    }

    [Fact]
    public void SelectingGerman_SwitchesTheLanguage_AndMovesTheCheckMark()
    {
        var f = new Fixture();
        var items = LanguageMenu(f).Children;
        var checkChanged = false;
        items[1].PropertyChanged += (_, e) => checkChanged |= e.PropertyName is "IsChecked" or "";

        items[1].Command!.Execute(null);

        Assert.Equal("de", f.Localizer.LanguageCode);
        Assert.False(items[0].IsChecked);
        Assert.True(items[1].IsChecked);
        Assert.True(checkChanged);
    }

    [Fact]
    public void SelectingTheActiveLanguage_ChangesNothing()
    {
        var f = new Fixture();
        var raised = 0;
        f.Localizer.LanguageChanged += (_, _) => raised++;

        LanguageMenu(f).Children[0].Command!.Execute(null);

        Assert.Equal(0, raised);
        Assert.Equal("en", f.Localizer.LanguageCode);
    }

    [Fact]
    public void SettingIsChecked_DoesNotChangeTheState()
    {
        var f = new Fixture();
        var german = LanguageMenu(f).Children[1];

        german.IsChecked = true; // what a radio menu item does when clicked; the command changes the language

        Assert.False(german.IsChecked);
        Assert.Equal("en", f.Localizer.LanguageCode);
    }

    [Fact]
    public void OtherItems_AreNotCheckable()
    {
        var f = new Fixture();

        // The language items and the six switches of the View menu show a check state.
        var switches = new[] { "Show Rulers", "Show Grid", "Snap to Grid", "Snap to Objects", "Smart Guides", "Show Guides" };
        Assert.All(
            Flatten(f.Menu.Menus).Where(i => !LanguageMenu(f).Children.Contains(i) && !switches.Contains(Clean(i.Header))),
            i => Assert.False(i.IsCheckable, i.Header));
    }

    [Theory]
    [InlineData("New", "new")]
    [InlineData("Open…", "open")]
    [InlineData("Save", "save")]
    [InlineData("Save As…", "save as")]
    [InlineData("Export as SVG…", "export")]
    public void FileItems_ExecuteTheirCommands(string header, string expected)
    {
        var f = new Fixture();

        Find(f, header).Command!.Execute(null);

        Assert.Equal(new[] { expected }, f.FileCalls);
    }

    [Fact]
    public void FileItems_KeepTheirShortcutHints()
    {
        var f = new Fixture();

        Assert.Equal("Ctrl+N", Find(f, "New").Gesture);
        Assert.Equal("Ctrl+O", Find(f, "Open…").Gesture);
        Assert.Equal("Ctrl+S", Find(f, "Save").Gesture);
        Assert.Equal("Ctrl+Shift+S", Find(f, "Save As…").Gesture);
        Assert.Equal("Ctrl+Shift+E", Find(f, "Export as SVG…").Gesture);
    }

    [Fact]
    public void OpenRecent_WithoutFiles_ShowsOneDisabledTranslatedItem()
    {
        var f = new Fixture();

        var item = Assert.Single(Find(f, "Open Recent").Children);

        Assert.Equal("(No recent files)", item.Header);
        Assert.False(item.IsEnabled);
        Assert.True(Find(f, "Open Recent").IsEnabled);
    }

    [Fact]
    public void OpenRecent_AfterFilesWereUsed_ListsThemMostRecentFirstWithTheFolderAsToolTip()
    {
        var f = new Fixture();
        var submenu = Find(f, "Open Recent");

        f.Recent.Add("/work/one.p2s");
        f.Recent.Add("/other/two.p2s");

        Assert.Equal(new[] { "two.p2s", "one.p2s" }, submenu.Children.Select(c => c.Header));
        Assert.Equal(new[] { "/other", "/work" }, submenu.Children.Select(c => c.ToolTip));
        Assert.All(submenu.Children, c => Assert.True(c.IsEnabled));
    }

    [Fact]
    public void OpenRecent_ChoosingAnEntry_RunsTheCommandWithTheFullPath()
    {
        var f = new Fixture();
        f.Recent.Add("/work/one.p2s");

        Find(f, "Open Recent").Children[0].Command!.Execute(null);

        Assert.Equal(new[] { "recent /work/one.p2s" }, f.FileCalls);
    }

    [Fact]
    public void OpenRecent_AfterTheLastEntryIsRemoved_ShowsTheEmptyItemAgain()
    {
        var f = new Fixture();
        f.Recent.Add("/a.p2s");

        f.Recent.Remove("/a.p2s");

        var item = Assert.Single(Find(f, "Open Recent").Children);
        Assert.Equal("(No recent files)", item.Header);
    }

    [Fact]
    public void OpenRecent_AtMostTenEntries()
    {
        var f = new Fixture();

        for (var i = 0; i < 15; i++)
        {
            f.Recent.Add($"/f{i}.p2s");
        }

        Assert.Equal(RecentFiles.MaxCount, Find(f, "Open Recent").Children.Count);
    }

    [Fact]
    public void OpenRecent_LanguageSwitch_KeepsTheEntryNamesAndTranslatesTheEmptyItem()
    {
        var f = new Fixture();
        f.Recent.Add("/work/one.p2s");

        f.Localizer.SetLanguage("de");

        Assert.Equal("one.p2s", Find(f, "Open Recent").Children[0].Header);
    }

    [Fact]
    public void MenuItem_FollowsTheCommandsCanExecute()
    {
        var command = new SwitchableCommand(false);
        var item = MenuItemViewModel.Item(FakeLocalizer.Create(), "_Undo", "Ctrl+Z", command);
        var raised = new List<string?>();
        item.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        Assert.False(item.IsEnabled);

        command.Set(true);
        Assert.True(item.IsEnabled);
        Assert.Contains(nameof(MenuItemViewModel.IsEnabled), raised);

        command.Set(false);
        Assert.False(item.IsEnabled);
    }

    [Fact]
    public void MenuItem_WithAnAlwaysExecutableCommand_StaysEnabled_AndSubmenusAndSeparatorsKeepTheirRules()
    {
        var localizer = FakeLocalizer.Create();
        var enabled = MenuItemViewModel.Item(localizer, "_Quit", null, new RelayCommand(() => { }));
        var withoutCommand = MenuItemViewModel.Item(localizer, "_Cut");
        var submenu = MenuItemViewModel.Submenu(localizer, "_Edit", withoutCommand);
        var separator = MenuItemViewModel.Separator(localizer);

        Assert.True(enabled.IsEnabled);
        Assert.False(withoutCommand.IsEnabled);
        Assert.True(submenu.IsEnabled);
        Assert.False(separator.IsEnabled);
    }

    [Fact]
    public void SubmenuWithACommandThatCannotExecute_StaysEnabled()
    {
        var submenu = MenuItemViewModel.Submenu(FakeLocalizer.Create(), "_Edit", MenuItemViewModel.Item(FakeLocalizer.Create(), "_Cut"));

        Assert.True(submenu.IsEnabled);
    }

    [Fact]
    public void UndoAndRedoItems_FollowTheirCommands_AndKeepTheirShortcutHints()
    {
        var f = new Fixture();

        Assert.False(Find(f, "Undo").IsEnabled);
        Assert.False(Find(f, "Redo").IsEnabled);
        Assert.Equal("Ctrl+Z", Find(f, "Undo").Gesture);
        Assert.Equal("Ctrl+Shift+Z", Find(f, "Redo").Gesture);

        f.Undo.Set(true);
        f.Redo.Set(true);
        Assert.True(Find(f, "Undo").IsEnabled);
        Assert.True(Find(f, "Redo").IsEnabled);

        Find(f, "Undo").Command!.Execute(null);
        Find(f, "Redo").Command!.Execute(null);
        Assert.Equal(1, f.Undo.Executions);
        Assert.Equal(1, f.Redo.Executions);
    }

    [Fact]
    public void EditItems_AreDisabledUntilTheirCommandCanRun_AndRunTheirCommand()
    {
        var f = new Fixture();
        var items = new (string Header, string Command)[]
        {
            ("Cut", "cut"), ("Copy", "copy"), ("Paste", "paste"), ("Paste in Place", "paste in place"),
            ("Duplicate", "duplicate"), ("Delete", "delete"), ("Select All", "select all"), ("Deselect", "deselect"),
        };

        foreach (var (header, command) in items)
        {
            var item = Find(f, header);
            Assert.False(item.IsEnabled, header);

            f.EditCommandsByName[command].Set(true);
            Assert.True(item.IsEnabled, header);
            item.Command!.Execute(null);

            Assert.Equal(1, f.EditCommandsByName[command].Executions);
        }
    }

    [Fact]
    public void EditItems_KeepTheirGestureHints()
    {
        var f = new Fixture();

        Assert.Equal("Ctrl+X", Find(f, "Cut").Gesture);
        Assert.Equal("Ctrl+C", Find(f, "Copy").Gesture);
        Assert.Equal("Ctrl+V", Find(f, "Paste").Gesture);
        Assert.Equal("Ctrl+Shift+V", Find(f, "Paste in Place").Gesture);
        Assert.Equal("Ctrl+D", Find(f, "Duplicate").Gesture);
        Assert.Equal("Delete", Find(f, "Delete").Gesture);
        Assert.Equal("Ctrl+A", Find(f, "Select All").Gesture);
        Assert.Equal("Ctrl+Shift+A", Find(f, "Deselect").Gesture);
    }
}

