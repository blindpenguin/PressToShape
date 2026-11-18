using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class MenuBarSmartTests
{
    private sealed class NoShutdown : IAppShutdown
    {
        public void Shutdown()
        {
        }
    }

    private static DesktopShellViewModel CreateShell() => new(new NoShutdown(), FakeLocalizer.Create(), TestFileServices.Create());

    private static MenuItemViewModel Find(DesktopShellViewModel shell, string header)
    {
        static IEnumerable<MenuItemViewModel> Flatten(IEnumerable<MenuItemViewModel> items) => items.SelectMany(i => new[] { i }.Concat(Flatten(i.Children)));

        return Flatten(shell.MenuBar.Menus).Single(i => i.Header.Replace("_", string.Empty) == header);
    }

    [Theory]
    [InlineData("Smart Guides")]
    [InlineData("Show Guides")]
    public void Item_IsEnabledAndCheckable(string header)
    {
        var item = Find(CreateShell(), header);

        Assert.True(item.IsEnabled);
        Assert.True(item.IsCheckable);
        Assert.NotNull(item.Command);
    }

    [Fact]
    public void Item_CheckStateFollowsThePreferences_SmartGuidesOffShowGuidesOn()
    {
        var shell = CreateShell();

        Assert.False(Find(shell, "Smart Guides").IsChecked);
        Assert.True(Find(shell, "Show Guides").IsChecked);
    }

    [Fact]
    public void Item_Execute_TogglesTheSetting_AndBack()
    {
        var shell = CreateShell();

        Find(shell, "Smart Guides").Command!.Execute(null);
        Find(shell, "Show Guides").Command!.Execute(null);

        Assert.True(shell.State.Snapping.SmartGuides);
        Assert.False(shell.State.Snapping.ShowGuides);
        Assert.True(Find(shell, "Smart Guides").IsChecked);
        Assert.False(Find(shell, "Show Guides").IsChecked);

        Find(shell, "Smart Guides").Command!.Execute(null);
        Assert.False(shell.State.Snapping.SmartGuides);
    }

    [Fact]
    public void Item_ChangeFromTheStatusBar_UpdatesTheMenu_AndTheReverse()
    {
        var shell = CreateShell();
        var raised = new List<string?>();
        Find(shell, "Smart Guides").PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        shell.StatusBar.SmartGuides = true;

        Assert.Contains(nameof(MenuItemViewModel.IsChecked), raised);
        Assert.True(Find(shell, "Smart Guides").IsChecked);

        Find(shell, "Show Guides").Command!.Execute(null);
        Assert.False(shell.StatusBar.ShowGuides);
    }

    [Fact]
    public void StatusBar_PropertiesRaiseOnChangesFromElsewhere()
    {
        var shell = CreateShell();
        var raised = new List<string?>();
        shell.StatusBar.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        shell.State.Snapping.SmartGuides = true;
        shell.State.Snapping.ShowGuides = false;

        Assert.Contains(nameof(PressToShape.App.ViewModels.Editor.StatusBarViewModel.SmartGuides), raised);
        Assert.Contains(nameof(PressToShape.App.ViewModels.Editor.StatusBarViewModel.ShowGuides), raised);
        Assert.True(shell.StatusBar.SmartGuides);
        Assert.False(shell.StatusBar.ShowGuides);
    }

    [Fact]
    public void Toggling_DoesNotChangeTheDocumentOrTheHistory()
    {
        var shell = CreateShell();
        var document = shell.State.Document;
        var steps = shell.State.History.Steps.Count;

        Find(shell, "Smart Guides").Command!.Execute(null);
        Find(shell, "Show Guides").Command!.Execute(null);

        Assert.Same(document, shell.State.Document);
        Assert.Equal(steps, shell.State.History.Steps.Count);
    }

    [Fact]
    public void Items_HaveTheirAltKeyMnemonics()
    {
        var shell = CreateShell();
        var headers = shell.MenuBar.Menus.SelectMany(m => m.Children).Select(i => i.Header).ToList();

        Assert.Contains("S_mart Guides", headers);
        Assert.Contains("Show G_uides", headers);
    }
}
