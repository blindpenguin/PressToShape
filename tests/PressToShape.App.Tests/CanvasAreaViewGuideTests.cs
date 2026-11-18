using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public sealed class CanvasAreaViewGuideTests
{
    private static (CanvasAreaView View, EditorState State, CanvasAreaViewModel Canvas, Window Window) Show()
    {
        var state = new EditorState();
        var canvas = new CanvasAreaViewModel(state);
        var view = new CanvasAreaView { DataContext = canvas };
        var window = new Window { Width = 900, Height = 700, Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (view, state, canvas, window);
    }

    private static Point InWindow(Visual visual, Window window, double x, double y) => visual.TranslatePoint(new Point(x, y), window)!.Value;

    private static RulerControl Ruler(CanvasAreaView view, string name) => view.FindControl<RulerControl>(name)!;

    [Fact]
    public void DragFromTheHorizontalRuler_IntoTheCanvas_MakesAHorizontalGuide() => HeadlessUi.Run(() =>
    {
        var (view, state, canvas, window) = Show();
        var ruler = Ruler(view, "HorizontalRuler");
        var viewport = view.FindControl<Panel>("Viewport")!;

        window.MouseDown(InWindow(ruler, window, 100, 5), MouseButton.Left);
        window.MouseMove(InWindow(viewport, window, 120, 150));
        window.MouseMove(InWindow(viewport, window, 120, 200));
        window.MouseUp(InWindow(viewport, window, 120, 200), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        var guide = Assert.Single(state.Document.Guides);
        Assert.Equal(GuideOrientation.Horizontal, guide.Orientation);
        Assert.Equal(200 - canvas.OriginY, guide.Position, 0);
        window.Close();
    });

    [Fact]
    public void DragFromTheVerticalRuler_MakesAVerticalGuide() => HeadlessUi.Run(() =>
    {
        var (view, state, canvas, window) = Show();
        var ruler = Ruler(view, "VerticalRuler");
        var viewport = view.FindControl<Panel>("Viewport")!;

        window.MouseDown(InWindow(ruler, window, 5, 100), MouseButton.Left);
        window.MouseMove(InWindow(viewport, window, 250, 120));
        window.MouseUp(InWindow(viewport, window, 250, 120), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        var guide = Assert.Single(state.Document.Guides);
        Assert.Equal(GuideOrientation.Vertical, guide.Orientation);
        Assert.Equal(250 - canvas.OriginX, guide.Position, 0);
        window.Close();
    });

    [Fact]
    public void ReleaseOverTheRuler_MakesNoGuide() => HeadlessUi.Run(() =>
    {
        var (view, state, _, window) = Show();
        var ruler = Ruler(view, "HorizontalRuler");
        var viewport = view.FindControl<Panel>("Viewport")!;

        window.MouseDown(InWindow(ruler, window, 100, 5), MouseButton.Left);
        window.MouseMove(InWindow(viewport, window, 120, 150));
        window.MouseMove(InWindow(ruler, window, 130, 8));
        window.MouseUp(InWindow(ruler, window, 130, 8), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.Empty(state.Document.Guides);
        window.Close();
    });

    [Fact]
    public void TheGuideReachesTheControl_AndStaysAfterThePointerLeaves() => HeadlessUi.Run(() =>
    {
        var (view, state, _, window) = Show();
        var ruler = Ruler(view, "HorizontalRuler");
        var viewport = view.FindControl<Panel>("Viewport")!;
        var control = view.GetVisualDescendants().OfType<DocumentCanvasControl>().Single();

        window.MouseDown(InWindow(ruler, window, 100, 5), MouseButton.Left);
        window.MouseMove(InWindow(viewport, window, 120, 200));
        Dispatcher.UIThread.RunJobs();
        Assert.Single(control.RulerGuides!);
        window.MouseUp(InWindow(viewport, window, 120, 200), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.Single(control.RulerGuides!);
        Assert.True(control.RulerGuides![0].IsSelected);
        Assert.Single(state.Document.Guides);
        window.Close();
    });

    [Fact]
    public void Escape_DuringTheDrag_CancelsIt() => HeadlessUi.Run(() =>
    {
        var (view, state, _, window) = Show();
        var ruler = Ruler(view, "HorizontalRuler");
        var viewport = view.FindControl<Panel>("Viewport")!;

        window.MouseDown(InWindow(ruler, window, 100, 5), MouseButton.Left);
        window.MouseMove(InWindow(viewport, window, 120, 200));
        window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
        window.MouseUp(InWindow(viewport, window, 120, 200), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.Empty(state.Document.Guides);
        window.Close();
    });

    [Fact]
    public void ARightClickOnTheRuler_DoesNothing() => HeadlessUi.Run(() =>
    {
        var (view, state, _, window) = Show();
        var ruler = Ruler(view, "HorizontalRuler");
        var viewport = view.FindControl<Panel>("Viewport")!;

        window.MouseDown(InWindow(ruler, window, 100, 5), MouseButton.Right);
        window.MouseMove(InWindow(viewport, window, 120, 200));
        window.MouseUp(InWindow(viewport, window, 120, 200), MouseButton.Right);
        Dispatcher.UIThread.RunJobs();

        Assert.Empty(state.Document.Guides);
        window.Close();
    });

    [Fact]
    public void WithTheRulersOff_ThereIsNothingToDragFrom() => HeadlessUi.Run(() =>
    {
        var (view, state, _, window) = Show();
        state.Snapping.ShowRulers = false;
        Dispatcher.UIThread.RunJobs();

        Assert.False(Ruler(view, "HorizontalRuler").IsEffectivelyVisible);
        Assert.False(Ruler(view, "VerticalRuler").IsEffectivelyVisible);
        window.Close();
    });

    [Fact]
    public void ADragOnTheCanvas_MovesAnExistingGuide_AndTheDeleteKeyRemovesTheSelectedOne() => HeadlessUi.Run(() =>
    {
        var (view, state, canvas, window) = Show();
        var guide = new Guide { Orientation = GuideOrientation.Vertical, Position = 400 };
        state.Edit(PressToShape.Core.History.HistoryStepType.CreateGuide, d => d.AddGuide(guide));
        Dispatcher.UIThread.RunJobs();
        var viewport = view.FindControl<Panel>("Viewport")!;

        window.MouseDown(InWindow(viewport, window, canvas.OriginX + 400, 200), MouseButton.Left);
        window.MouseMove(InWindow(viewport, window, canvas.OriginX + 450, 210));
        window.MouseUp(InWindow(viewport, window, canvas.OriginX + 450, 210), MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(450, state.Document.FindGuide(guide.Id)!.Position, 0);

        window.KeyPress(Key.Delete, RawInputModifiers.None, PhysicalKey.Delete, null);
        Dispatcher.UIThread.RunJobs();

        Assert.Empty(state.Document.Guides);
        window.Close();
    });
}
