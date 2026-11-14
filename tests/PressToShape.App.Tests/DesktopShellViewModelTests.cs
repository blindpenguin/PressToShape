using System.Linq;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class DesktopShellViewModelTests
{
    private sealed class FakeShutdown : IAppShutdown
    {
        public int Calls { get; private set; }

        public void Shutdown() => Calls++;
    }

    private static MenuItemViewModel FindItem(DesktopShellViewModel shell, string header)
    {
        static System.Collections.Generic.IEnumerable<MenuItemViewModel> Flatten(System.Collections.Generic.IEnumerable<MenuItemViewModel> items) =>
            items.SelectMany(i => new[] { i }.Concat(Flatten(i.Children)));

        return Flatten(shell.MenuBar.Menus).Single(i => i.Header.Replace("_", string.Empty) == header);
    }

    [Fact]
    public void Shell_CreatesAllRegions()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown());

        Assert.NotNull(shell.MenuBar);
        Assert.NotNull(shell.Toolbar);
        Assert.NotNull(shell.Toolbox);
        Assert.NotNull(shell.Canvas);
        Assert.NotNull(shell.Inspector);
        Assert.NotNull(shell.StatusBar);
    }

    [Fact]
    public void ToolSelectedInToolbox_IsReflectedInToolbar()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown());

        shell.Toolbox.Tools.Single(t => t.Definition.Tool == Tool.Node).SelectCommand.Execute(null);

        Assert.True(shell.Toolbar.IsNodeToolActive);
        Assert.False(shell.Toolbar.IsSelectToolActive);
    }

    [Fact]
    public void ToolToggledInToolbar_IsReflectedInToolbox()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown());

        shell.Toolbar.IsNodeToolActive = true;

        Assert.Equal(Tool.Node, shell.Toolbox.ActiveTool);
        Assert.Equal(Tool.Node, shell.Toolbox.Tools.Single(t => t.IsActive).Definition.Tool);
    }

    [Fact]
    public void ZoomFromEveryRoute_ShowsTheSameValue()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown());

        // menu route
        FindItem(shell, "Zoom In").Command!.Execute(null);
        Assert.Equal("150%", shell.StatusBar.ZoomText);
        Assert.Equal(150, shell.Canvas.ZoomPercent);

        // status bar route
        shell.StatusBar.ZoomOutCommand.Execute(null);
        shell.StatusBar.ZoomOutCommand.Execute(null);
        Assert.Equal("75%", shell.StatusBar.ZoomText);
        Assert.Equal(75, shell.Canvas.ZoomPercent);
        Assert.Equal(75, shell.State.ZoomPercent);

        // shortcut-style route through the state
        shell.State.SetZoom(200);
        Assert.Equal("200%", shell.StatusBar.ZoomText);
        Assert.Equal(200, shell.Canvas.ZoomPercent);
    }

    [Fact]
    public void FitToWindow_UsesTheCanvasViewport_FromMenuAndStatusBar()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown());
        shell.Canvas.ViewportWidth = 648;  // available 600 x 400 with the default 24 px margin
        shell.Canvas.ViewportHeight = 448;

        shell.StatusBar.FitToWindowCommand.Execute(null);
        Assert.Equal(50, shell.State.ZoomPercent, precision: 6);

        shell.State.SetZoom(300);
        FindItem(shell, "Fit Artboard in Window").Command!.Execute(null);
        Assert.Equal(50, shell.State.ZoomPercent, precision: 6);
    }

    [Fact]
    public void FitToWindow_WithoutViewport_LeavesZoomUnchanged()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown());
        shell.State.SetZoom(200);

        shell.FitToWindow();

        Assert.Equal(200, shell.State.ZoomPercent);
    }

    [Fact]
    public void WindowMenu_SwitchesTheInspectorTab()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown());

        FindItem(shell, "Layers").Command!.Execute(null);

        Assert.Equal(InspectorTab.Layers, shell.Inspector.SelectedTab);
    }

    [Fact]
    public void Quit_CallsTheInjectedShutdown()
    {
        var shutdown = new FakeShutdown();
        var shell = new DesktopShellViewModel(shutdown);

        FindItem(shell, "Quit").Command!.Execute(null);

        Assert.Equal(1, shutdown.Calls);
    }

    [Fact]
    public void SelectionState_FlowsToAllRegions()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown());

        shell.State.HasSelection = true;
        shell.State.SelectionSummary = "1 selected";

        Assert.True(shell.Toolbar.HasSelection);
        Assert.True(shell.Inspector.Properties.HasSelection);
        Assert.Equal("1 selected", shell.StatusBar.SelectionSummary);
    }

    [Fact]
    public void ShortcutCommands_DoWhatTheMenuItemsDo()
    {
        var shutdown = new FakeShutdown();
        var shell = new DesktopShellViewModel(shutdown);
        shell.Canvas.ViewportWidth = 648;
        shell.Canvas.ViewportHeight = 448;

        shell.ZoomInCommand.Execute(null);
        Assert.Equal(150, shell.State.ZoomPercent);

        shell.ZoomOutCommand.Execute(null);
        Assert.Equal(100, shell.State.ZoomPercent);

        shell.FitArtboardCommand.Execute(null);
        Assert.Equal(50, shell.State.ZoomPercent, precision: 6);

        shell.ActualSizeCommand.Execute(null);
        Assert.Equal(100, shell.State.ZoomPercent);

        shell.QuitCommand.Execute(null);
        Assert.Equal(1, shutdown.Calls);
    }
}
