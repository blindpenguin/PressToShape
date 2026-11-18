using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Views.Editor;
using PressToShape.App.Views.Tablet;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public sealed class CanvasAreaViewSnapTests
{
    private sealed class NoShutdown : IAppShutdown
    {
        public void Shutdown()
        {
        }
    }

    private static (CanvasAreaView View, EditorState State, Window Window) ShowCanvas()
    {
        var state = new EditorState();
        var view = new CanvasAreaView { DataContext = new CanvasAreaViewModel(state) };
        var window = new Window { Width = 800, Height = 600, Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (view, state, window);
    }

    private static System.Collections.Generic.List<RulerControl> Rulers(Control view) => view.GetVisualDescendants().OfType<RulerControl>().ToList();

    [Fact]
    public void Rulers_FollowTheSetting() => HeadlessUi.Run(() =>
    {
        var (view, state, window) = ShowCanvas();
        var rulers = Rulers(view);
        Assert.Equal(2, rulers.Count);
        Assert.All(rulers, r => Assert.True(r.IsEffectivelyVisible));

        state.Snapping.ShowRulers = false;
        Dispatcher.UIThread.RunJobs();
        Assert.All(rulers, r => Assert.False(r.IsEffectivelyVisible));

        state.Snapping.ShowRulers = true;
        Dispatcher.UIThread.RunJobs();
        Assert.All(rulers, r => Assert.True(r.IsEffectivelyVisible));
        window.Close();
    });

    [Fact]
    public void Grid_AppearsWhenShowGridIsSet() => HeadlessUi.Run(() =>
    {
        var (view, state, window) = ShowCanvas();
        var control = view.GetVisualDescendants().OfType<DocumentCanvasControl>().Single();
        Assert.False(control.ShowGrid);

        state.Snapping.ShowGrid = true;
        state.Snapping.GridSpacing = 25;
        Dispatcher.UIThread.RunJobs();

        Assert.True(control.ShowGrid);
        Assert.Equal(25, control.GridSpacing);
        Assert.NotNull(control.GridBrush);
        window.Close();
    });

    [Fact]
    public void Guides_ReachTheControl() => HeadlessUi.Run(() =>
    {
        var (view, state, window) = ShowCanvas();
        var control = view.GetVisualDescendants().OfType<DocumentCanvasControl>().Single();

        state.SnapGuides = [new SnapGuide(SnapAxis.X, 100, 0, 50)];
        Dispatcher.UIThread.RunJobs();

        Assert.Single(control.SnapGuides!);
        Assert.NotNull(control.GuideBrush);
        window.Close();
    });

    [Fact]
    public void Desktop_ShowsTheRulersByDefault() => HeadlessUi.Run(() =>
    {
        var shell = new DesktopShellViewModel(new NoShutdown(), FakeLocalizer.Create(), TestFileServices.Create());
        var view = new CanvasAreaView { DataContext = shell.Canvas };
        var window = new Window { Width = 800, Height = 600, Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Assert.All(Rulers(view), r => Assert.True(r.IsEffectivelyVisible));
        window.Close();
    });

    [Fact]
    public void Tablet_HidesTheRulersByDefault_AndShowsThemWhenTheSettingSaysSo() => HeadlessUi.Run(() =>
    {
        var host = new MessageDialogHost();
        var localizer = FakeLocalizer.Create();
        var services = new FileServices(new MemorySettingsStore(), new FakeFileStore(), new FakeFileDialogs(), new OverlayMessageDialogs(host, localizer));
        var shell = new TabletShellViewModel(new NoShutdown(), localizer, services, host);
        var view = new TabletShellView { DataContext = shell };
        var window = new Window { Width = 1280, Height = 700, Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var rulers = Rulers(view);
        Assert.Equal(2, rulers.Count);
        Assert.All(rulers, r => Assert.False(r.IsEffectivelyVisible));

        shell.State.Snapping.ShowRulers = true;
        Dispatcher.UIThread.RunJobs();

        Assert.All(rulers, r => Assert.True(r.IsEffectivelyVisible));
        window.Close();
    });
}
