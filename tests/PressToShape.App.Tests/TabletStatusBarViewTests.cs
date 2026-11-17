using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Views.Desktop;
using PressToShape.App.Views.Tablet;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.PathOperations;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

// The tablet status bar (R10): where it sits, what it shows, that it is touch sized and that Back closes its zoom list.
[Collection(HeadlessUi.CollectionName)]
public class TabletStatusBarViewTests
{
    private sealed class NoShutdown : IAppShutdown
    {
        public void Shutdown()
        {
        }
    }

    private static TabletShellViewModel CreateShell(PressToShape.App.Localization.ILocalizer? localizer = null)
    {
        localizer ??= FakeLocalizer.Create();
        var host = new MessageDialogHost();
        var services = new FileServices(new MemorySettingsStore(), new FakeFileStore(), new FakeFileDialogs(), new OverlayMessageDialogs(host, localizer));
        return new TabletShellViewModel(new NoShutdown(), localizer, services, host);
    }

    private static (T View, Window Window) Show<T>(T view, bool touch, double width = 1280, double height = 200) where T : Control
    {
        var host = new Border { Child = view };
        if (touch)
        {
            host.Classes.Add("touch");
            TouchSizes.ApplyTo(host);
        }

        var window = new Window { Width = width, Height = height, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (view, window);
    }

    private static T Named<T>(Visual root, string automationName) where T : Control =>
        root.GetVisualDescendants().OfType<T>().First(c => AutomationProperties.GetName(c) == automationName);

    private static Point Origin(Visual c, Visual window) => c.TranslatePoint(new Point(0, 0), window)!.Value;

    private static string[] Texts(Visual view) => view.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible).Select(t => t.Text ?? string.Empty).ToArray();

    // Runs the work when the test lets it, so the test decides when the background operation ends.
    private sealed class GatedRunner
    {
        private readonly TaskCompletionSource _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<PathOperationResult> Run(Func<PathOperationResult> work) => RunAfterGate(work);

        public void Release() => _gate.SetResult();

        private async Task<PathOperationResult> RunAfterGate(Func<PathOperationResult> work)
        {
            await _gate.Task.ConfigureAwait(false);
            return work();
        }
    }

    [Fact]
    public void Shell_TheStatusBar_IsUnderTheToolColumnAndTheCanvas_AcrossTheWholeWidth() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var view = new TabletShellView { DataContext = shell };
        var window = new Window { Width = 1280, Height = 720, Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var bar = view.GetVisualDescendants().OfType<TabletStatusBarView>().Single();
        var column = view.GetVisualDescendants().OfType<TabletToolColumnView>().Single();
        var canvas = view.GetVisualDescendants().OfType<PressToShape.App.Views.Editor.CanvasAreaView>().Single();

        Assert.Same(shell.StatusBar, bar.DataContext);
        Assert.Equal(0, Origin(bar, window).X, 0.5);
        Assert.Equal(1280, bar.Bounds.Width, 0.5);
        Assert.Equal(48, bar.Bounds.Height, 0.5);
        Assert.Equal(Origin(bar, window).Y, Origin(column, window).Y + column.Bounds.Height, 0.5);
        Assert.Equal(Origin(bar, window).Y, Origin(canvas, window).Y + canvas.Bounds.Height, 0.5);

        window.Close();
    });

    [Fact]
    public void Bar_ShowsTheSelectionSummaryAndTheZoom_AndTheZoomControlsWork() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletStatusBarView { DataContext = shell.StatusBar }, touch: true);
        Assert.Contains("Nothing selected", Texts(view));
        Assert.Equal("100%", Assert.IsType<string>(view.GetVisualDescendants().OfType<DropDownButton>().Single().Content));

        Named<Button>(view, "Zoom in").Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("150%", view.GetVisualDescendants().OfType<DropDownButton>().Single().Content);
        Assert.Equal(150, shell.State.ZoomPercent);

        var rectangle = new RectangleShape { Width = 5, Height = 5 };
        shell.State.Edit(
            PressToShape.Core.History.HistoryStepType.DrawRectangle,
            d => d.AddShape(shell.State.ActiveLayerId, rectangle));
        shell.State.SetSelection([rectangle.Id]);
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("1 selected", Texts(view));

        window.Close();
    });

    [Fact]
    public void Bar_TheControlsHaveAccessibleNames() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletStatusBarView { DataContext = shell.StatusBar }, touch: true);

        foreach (var name in new[] { "Zoom out", "Zoom in", "Fit artboard in window" })
        {
            Assert.NotNull(Named<Button>(view, name));
        }

        Assert.NotNull(Named<DropDownButton>(view, "Zoom level"));

        window.Close();
    });

    [Fact]
    public void Bar_WithTheTouchSizes_EveryControlIsAtLeast48() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletStatusBarView { DataContext = shell.StatusBar }, touch: true);

        var controls = view.GetVisualDescendants().OfType<Control>().Where(c => c is Button && c.TemplatedParent is null && c.IsEffectivelyVisible).ToList();
        Assert.Equal(4, controls.Count);
        var tooSmall = controls.Where(c => c.Bounds.Width < 48 || c.Bounds.Height < 48).Select(c => $"{AutomationProperties.GetName(c)} {c.Bounds.Width:0}x{c.Bounds.Height:0}").ToList();
        Assert.True(tooSmall.Count == 0, "Controls under 48 dp: " + string.Join(", ", tooSmall));

        window.Close();
    });

    [Fact]
    public void Bar_AfterALanguageChange_ShowsTheGermanTexts() => HeadlessUi.Run(() =>
    {
        // The texts that come from the view model follow the injected localizer. The XAML texts use the translation of the whole application, tested with the translation files.
        var german = new Dictionary<string, string> { ["Nothing selected"] = "Nichts ausgewählt", ["Working…"] = "Arbeite…" };
        var localizer = FakeLocalizer.Create("en", german);
        var shell = CreateShell(localizer);
        var (view, window) = Show(new TabletStatusBarView { DataContext = shell.StatusBar }, touch: true);
        Assert.Contains("Nothing selected", Texts(view));

        localizer.SetLanguage("de");
        Dispatcher.UIThread.RunJobs();

        Assert.Contains("Nichts ausgewählt", Texts(view));
        Assert.Equal("Arbeite…", shell.StatusBar.BusyText);

        window.Close();
    });

    [Fact]
    public void Bar_TheZoomList_IsClosedByTryCloseMenu_AndSaysSo() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletStatusBarView { DataContext = shell.StatusBar }, touch: true);
        var closed = 0;
        view.MenuClosed += (_, _) => closed++;
        Assert.False(view.TryCloseMenu());

        var button = Named<DropDownButton>(view, "Zoom level");
        button.Flyout!.ShowAt(button);
        Dispatcher.UIThread.RunJobs();
        Assert.True(button.Flyout.IsOpen);

        Assert.True(view.TryCloseMenu());
        Dispatcher.UIThread.RunJobs();

        Assert.False(button.Flyout.IsOpen);
        Assert.Equal(1, closed);
        Assert.False(view.TryCloseMenu());

        window.Close();
    });

    [Fact]
    public void Bar_WhileAnOperationRuns_ShowsWorkingOnTheTabletAndOnTheDesktop() => HeadlessUi.Run(() =>
    {
        var document = Document.Create("x");
        var a = new RectangleShape { Width = 100, Height = 100 };
        var b = new RectangleShape { X = 50, Y = 50, Width = 100, Height = 100 };
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [a, b]));
        state.SetSelection([a.Id, b.Id]);
        var runner = new GatedRunner();
        var localizer = FakeLocalizer.Create();
        var editor = new PathEditor(state, new FakeMessageDialogs(), localizer, backgroundNodeLimit: 0, background: runner.Run);
        var bar = new StatusBarViewModel(state, localizer, () => { }, editor);
        var (tablet, tabletWindow) = Show(new TabletStatusBarView { DataContext = bar }, touch: true);
        var (desktop, desktopWindow) = Show(new StatusBarView { DataContext = bar }, touch: false);
        Assert.DoesNotContain("Working…", Texts(tablet));
        Assert.DoesNotContain("Working…", Texts(desktop));

        var running = editor.BooleanAsync(PathOperationType.Union);
        Dispatcher.UIThread.RunJobs();

        Assert.Contains("Working…", Texts(tablet));
        Assert.Contains("Working…", Texts(desktop));

        // The work ends on another thread and its continuation comes back through the dispatcher, so pump it (never block this thread).
        runner.Release();
        for (var i = 0; i < 1_000_000 && !running.IsCompleted; i++)
        {
            Dispatcher.UIThread.RunJobs();
            System.Threading.Thread.Yield();
        }

        Assert.True(running.IsCompleted);

        Assert.DoesNotContain("Working…", Texts(tablet));
        Assert.DoesNotContain("Working…", Texts(desktop));

        tabletWindow.Close();
        desktopWindow.Close();
    });
}
