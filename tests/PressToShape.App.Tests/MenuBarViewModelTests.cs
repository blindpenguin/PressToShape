using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
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

        public MenuBarViewModel Menu { get; }

        public Fixture()
        {
            Menu = new MenuBarViewModel(State, Shutdown, ShownTabs.Add, () => FitCalls++);
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
            new[] { "File", "Edit", "Object", "Path", "Text", "View", "Window", "Help" },
            f.Menu.Menus.Select(m => Clean(m.Header)));
    }

    [Fact]
    public void OnlyQuitZoomAndWindowItems_AreEnabledLeafItems()
    {
        var f = new Fixture();

        var enabledLeaves = Flatten(f.Menu.Menus)
            .Where(i => !i.IsSeparator && i.Children.Count == 0 && i.IsEnabled)
            .Select(i => Clean(i.Header))
            .OrderBy(h => h)
            .ToList();

        var expected = new[]
        {
            "Quit", "Zoom In", "Zoom Out", "Fit Artboard in Window", "Actual Size (100%)",
            "Properties", "Layers", "Assets", "History",
        }.OrderBy(h => h);

        Assert.Equal(expected, enabledLeaves);
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

        foreach (var name in new[] { "File", "Edit", "Object", "Path", "Text", "View", "Window", "Help" })
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
}
