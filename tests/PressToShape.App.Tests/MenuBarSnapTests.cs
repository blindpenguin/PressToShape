using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class MenuBarSnapTests
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
    [InlineData("Show Rulers")]
    [InlineData("Show Grid")]
    [InlineData("Snap to Grid")]
    [InlineData("Snap to Objects")]
    public void Item_IsEnabledAndCheckable(string header)
    {
        var item = Find(CreateShell(), header);

        Assert.True(item.IsEnabled);
        Assert.True(item.IsCheckable);
        Assert.NotNull(item.Command);
    }

    [Fact]
    public void Item_CheckStateFollowsThePreferences()
    {
        var shell = CreateShell();

        Assert.True(Find(shell, "Show Rulers").IsChecked);
        Assert.False(Find(shell, "Show Grid").IsChecked);
        Assert.False(Find(shell, "Snap to Grid").IsChecked);
        Assert.False(Find(shell, "Snap to Objects").IsChecked);
    }

    [Fact]
    public void Item_Execute_TogglesTheSetting()
    {
        var shell = CreateShell();

        Find(shell, "Show Grid").Command!.Execute(null);
        Find(shell, "Snap to Grid").Command!.Execute(null);
        Find(shell, "Snap to Objects").Command!.Execute(null);
        Find(shell, "Show Rulers").Command!.Execute(null);

        var snapping = shell.State.Snapping;
        Assert.True(snapping.ShowGrid);
        Assert.True(snapping.SnapToGrid);
        Assert.True(snapping.SnapToObjects);
        Assert.False(snapping.ShowRulers);
        Assert.True(Find(shell, "Show Grid").IsChecked);
        Assert.False(Find(shell, "Show Rulers").IsChecked);
    }

    [Fact]
    public void Item_ExecuteTwice_TogglesBack()
    {
        var shell = CreateShell();

        Find(shell, "Show Grid").Command!.Execute(null);
        Find(shell, "Show Grid").Command!.Execute(null);

        Assert.False(shell.State.Snapping.ShowGrid);
    }

    [Fact]
    public void Item_ChangeFromElsewhere_RaisesIsChecked()
    {
        var shell = CreateShell();
        var item = Find(shell, "Snap to Grid");
        var raised = new List<string?>();
        item.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        shell.State.Snapping.SnapToGrid = true;

        Assert.Contains(nameof(MenuItemViewModel.IsChecked), raised);
        Assert.True(item.IsChecked);
    }

    [Fact]
    public void Item_Toggling_DoesNotChangeTheDocumentOrTheHistory()
    {
        var shell = CreateShell();
        var document = shell.State.Document;
        var steps = shell.State.History.Steps.Count;

        Find(shell, "Show Grid").Command!.Execute(null);

        Assert.Same(document, shell.State.Document);
        Assert.Equal(steps, shell.State.History.Steps.Count);
    }

    [Fact]
    public void Item_KeepsItsGestureAndMnemonic()
    {
        var shell = CreateShell();

        Assert.Equal("Ctrl+R", Find(shell, "Show Rulers").Gesture);
        Assert.Contains("_Rulers", Find(shell, "Show Rulers").Header);
    }
}
