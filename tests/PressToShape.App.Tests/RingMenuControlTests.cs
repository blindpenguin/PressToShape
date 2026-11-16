using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using CoreShape = PressToShape.Core.Shapes.Shape;
using Xunit;

namespace PressToShape.App.Tests;

// What the view models cannot show about the ring menu: that the control draws it, that real pointer and key input reaches it, and that it
// stays inside the canvas area and uses the touch sizes in the tablet layout.
[Collection(HeadlessUi.CollectionName)]
public class RingMenuControlTests
{
    private sealed class BareShell : EditorShellViewModel
    {
        public BareShell(ILocalizer localizer)
            : base(localizer, TestFileServices.Create())
        {
        }
    }

    private sealed class Scene
    {
        public Scene(bool touch, ILocalizer? localizer = null)
        {
            Shell = new BareShell(localizer ?? FakeLocalizer.Create());
            Shapes = Enumerable.Range(0, 3).Select(i => (CoreShape)new RectangleShape { X = 100 + i * 150, Y = 100, Width = 100, Height = 100 }).ToArray();
            Shell.State.Edit(HistoryStepType.DrawRectangle, d => d.AddShapes(Shell.State.ActiveLayerId, Shapes));
            View = new CanvasAreaView { DataContext = Shell.Canvas };
            var host = new Border { Child = View };
            if (touch)
            {
                host.Classes.Add("touch");
                TouchSizes.ApplyTo(host);
            }

            Window = new Window { Width = 1100, Height = 800, Content = host };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
            Viewport = View.FindControl<Panel>("Viewport")!;
            Ring = View.GetVisualDescendants().OfType<RingMenuControl>().Single();
        }

        public BareShell Shell { get; }

        public CoreShape[] Shapes { get; }

        public CanvasAreaView View { get; }

        public Window Window { get; }

        public Panel Viewport { get; }

        public RingMenuControl Ring { get; }

        public RingMenuViewModel RingModel => Shell.Canvas.Ring!;

        public int Steps => Shell.State.History.Steps.Count;

        public IReadOnlyList<Path> Wedges => Ring.GetVisualDescendants().OfType<Path>().Where(p => p.Classes.Contains("ringWedge")).ToList();

        public TextBlock CenterText => Ring.GetVisualDescendants().OfType<TextBlock>().Single();

        // The point of a place in the viewport, in window coordinates, which is what the headless input takes.
        public Point InWindow(double viewportX, double viewportY) => Viewport.TranslatePoint(new Point(viewportX, viewportY), Window)!.Value;

        public Point ShapeInWindow(int index)
        {
            var canvas = Shell.Canvas;
            var x = canvas.OriginX + (Shapes[index].X + 50) * canvas.ZoomPercent / 100;
            var y = canvas.OriginY + (Shapes[index].Y + 50) * canvas.ZoomPercent / 100;
            return InWindow(x, y);
        }

        public Point WedgeInWindow(int index)
        {
            var (dx, dy) = RingGeometry.SlotCenter(index, RingModel.Slots.Count, Ring.InnerRadius, Ring.OuterRadius);
            return InWindow(RingModel.CenterX + dx, RingModel.CenterY + dy);
        }

        public Point CenterInWindow() => InWindow(RingModel.CenterX, RingModel.CenterY);

        public void RightClickShape(int index)
        {
            var point = ShapeInWindow(index);
            Window.MouseDown(point, MouseButton.Right);
            Window.MouseUp(point, MouseButton.Right);
            Pump();
        }

        public void Click(Point point)
        {
            Window.MouseDown(point, MouseButton.Left);
            Window.MouseUp(point, MouseButton.Left);
            Pump();
        }

        public void Press(Key key, RawInputModifiers modifiers = RawInputModifiers.None)
        {
            Window.KeyPress(key, modifiers, PhysicalKey.None, null);
            Window.KeyRelease(key, modifiers, PhysicalKey.None, null);
            Pump();
        }

        public static void Pump() => Dispatcher.UIThread.RunJobs();

        public void SelectFirstTwo() => Shell.State.SetSelection([Shapes[0].Id, Shapes[1].Id]);
    }

    [Fact]
    public void RightClickOnAShape_ShowsTheSevenWedgesOfASelection() => HeadlessUi.Run(() =>
    {
        var scene = new Scene(touch: false);

        scene.RightClickShape(0);

        Assert.True(scene.Ring.IsVisible);
        Assert.Equal(7, scene.Wedges.Count);
        Assert.Equal([scene.Shapes[0].Id], scene.Shell.State.SelectedShapeIds.ToArray());
        scene.Window.Close();
    });

    [Fact]
    public void RightClickOnEmptyCanvas_ShowsTheTwoWedgesOfTheEmptyCanvas() => HeadlessUi.Run(() =>
    {
        var scene = new Scene(touch: false);

        var empty = scene.InWindow(300, 600);
        scene.Window.MouseDown(empty, MouseButton.Right);
        scene.Window.MouseUp(empty, MouseButton.Right);
        Scene.Pump();

        Assert.Equal(2, scene.Wedges.Count);
        scene.Window.Close();
    });

    [Fact]
    public void ClickOnAWedge_RunsItsCommandAndClosesTheRing() => HeadlessUi.Run(() =>
    {
        var scene = new Scene(touch: false);
        scene.SelectFirstTwo();
        scene.RightClickShape(0);
        var steps = scene.Steps;
        var groupIndex = scene.RingModel.Slots.ToList().FindIndex(s => s.Name == "Group");

        scene.Click(scene.WedgeInWindow(groupIndex));

        Assert.Equal(steps + 1, scene.Steps);
        Assert.Equal(HistoryStepType.GroupShapes, scene.Shell.State.History.Steps[^1]);
        Assert.False(scene.Ring.IsVisible);
        scene.Window.Close();
    });

    [Fact]
    public void ClickOnAnUnavailableWedge_DoesNothingAndKeepsTheRingOpen() => HeadlessUi.Run(() =>
    {
        var scene = new Scene(touch: false);
        scene.RightClickShape(0);
        var steps = scene.Steps;
        var groupIndex = scene.RingModel.Slots.ToList().FindIndex(s => s.Name == "Group");

        scene.Click(scene.WedgeInWindow(groupIndex));

        Assert.Equal(steps, scene.Steps);
        Assert.True(scene.Ring.IsVisible);
        scene.Window.Close();
    });

    [Fact]
    public void ArrangeWedge_SwapsToFourWedges_AndTheCenterGoesBack() => HeadlessUi.Run(() =>
    {
        var scene = new Scene(touch: false);
        scene.RightClickShape(0);
        var arrangeIndex = scene.RingModel.Slots.ToList().FindIndex(s => s.Name == "Arrange");

        scene.Click(scene.WedgeInWindow(arrangeIndex));
        Assert.Equal(4, scene.Wedges.Count);
        Assert.True(scene.Ring.IsVisible);

        scene.Click(scene.CenterInWindow());
        Assert.Equal(7, scene.Wedges.Count);
        Assert.True(scene.Ring.IsVisible);
        scene.Window.Close();
    });

    [Fact]
    public void Escape_InASubmenuGoesBack_ThenCloses() => HeadlessUi.Run(() =>
    {
        var scene = new Scene(touch: false);
        scene.RightClickShape(0);
        scene.Click(scene.WedgeInWindow(scene.RingModel.Slots.ToList().FindIndex(s => s.Name == "Arrange")));
        Assert.Equal(4, scene.Wedges.Count);

        scene.Press(Key.Escape);
        Assert.Equal(7, scene.Wedges.Count);
        scene.Press(Key.Escape);

        Assert.False(scene.Ring.IsVisible);
        Assert.True(scene.Shell.State.HasSelection);
        scene.Window.Close();
    });

    [Fact]
    public void PressOutsideTheRing_ClosesItWithoutChangingTheDocument() => HeadlessUi.Run(() =>
    {
        var scene = new Scene(touch: false);
        scene.RightClickShape(0);
        var steps = scene.Steps;

        scene.Click(scene.InWindow(900, 650));

        Assert.False(scene.Ring.IsVisible);
        Assert.Equal(steps, scene.Steps);
        Assert.Equal([scene.Shapes[0].Id], scene.Shell.State.SelectedShapeIds.ToArray());
        scene.Window.Close();
    });

    [Fact]
    public void MenuKey_OpensTheRingAndTheArrowKeysAndEnterUseASlot() => HeadlessUi.Run(() =>
    {
        var scene = new Scene(touch: false);
        scene.SelectFirstTwo();
        scene.Viewport.Focus();

        scene.Press(Key.Apps);
        Assert.True(scene.Ring.IsVisible);
        Assert.Equal(0, scene.RingModel.FocusedIndex);

        scene.Press(Key.Right);
        scene.Press(Key.Right);
        Assert.Equal("Duplicate", scene.CenterText.Text);
        scene.Press(Key.Left);
        scene.Press(Key.Left);
        scene.Press(Key.Enter);

        Assert.Equal(HistoryStepType.GroupShapes, scene.Shell.State.History.Steps[^1]);
        Assert.False(scene.Ring.IsVisible);
        scene.Window.Close();
    });

    [Fact]
    public void Wedges_CarryTheirTranslatedNameAndTheirDisabledState() => HeadlessUi.Run(() =>
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["Group"] = "Gruppieren", ["Delete"] = "Löschen", ["Close"] = "Schließen" });
        var scene = new Scene(touch: false, localizer);
        scene.RightClickShape(0);

        var center = scene.Ring.GetVisualDescendants().OfType<Ellipse>().Single();
        Assert.Equal("Close", AutomationProperties.GetName(center));
        var names = scene.Wedges.Select(AutomationProperties.GetName).ToList();
        Assert.Equal(["Group", "Ungroup", "Duplicate", "Delete", "Lock", "Hide", "Arrange"], names);
        Assert.False(scene.Wedges[0].IsEnabled);
        Assert.True(scene.Wedges[3].IsEnabled);

        localizer.SetLanguage("de");
        Scene.Pump();

        Assert.Equal("Gruppieren", AutomationProperties.GetName(scene.Wedges[0]));
        Assert.Equal("Löschen", AutomationProperties.GetName(scene.Wedges[3]));
        Assert.Equal("Schließen", AutomationProperties.GetName(scene.Ring.GetVisualDescendants().OfType<Ellipse>().Single()));
        scene.Window.Close();
    });

    [Fact]
    public void LanguageChange_WhileOpen_RefreshesTheCenterText() => HeadlessUi.Run(() =>
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["Delete"] = "Löschen" });
        var scene = new Scene(touch: false, localizer);
        scene.RightClickShape(0);
        scene.RingModel.Hover(3);
        Scene.Pump();
        Assert.Equal("Delete", scene.CenterText.Text);

        localizer.SetLanguage("de");
        Scene.Pump();

        Assert.Equal("Löschen", scene.CenterText.Text);
        scene.Window.Close();
    });

    [Fact]
    public void RingOpenedNearTheCorner_StaysInsideTheCanvasArea() => HeadlessUi.Run(() =>
    {
        var scene = new Scene(touch: false);

        scene.Shell.Canvas.OpenContextMenu(4, 4);
        Scene.Pump();

        var area = scene.Ring.Bounds;
        Assert.All(scene.Wedges, wedge =>
        {
            Assert.True(wedge.Bounds.Left >= 0 && wedge.Bounds.Top >= 0, $"wedge starts at {wedge.Bounds.TopLeft}");
            Assert.True(wedge.Bounds.Right <= area.Width && wedge.Bounds.Bottom <= area.Height);
        });
        scene.Window.Close();
    });

    [Fact]
    public void RingRadii_AreLargerInTheTabletLayout() => HeadlessUi.Run(() =>
    {
        var desktop = new Scene(touch: false);
        var tablet = new Scene(touch: true);

        Assert.True(tablet.Ring.OuterRadius > desktop.Ring.OuterRadius);
        Assert.True(tablet.Ring.InnerRadius > desktop.Ring.InnerRadius);
        Assert.Equal(150, tablet.Ring.OuterRadius);
        desktop.Window.Close();
        tablet.Window.Close();
    });

    [Fact]
    public void CanvasBelowTheRing_DoesNotReactToThePointerWhileItIsOpen() => HeadlessUi.Run(() =>
    {
        var scene = new Scene(touch: false);
        scene.RightClickShape(0);
        var before = scene.Shell.State.Document;
        var start = scene.ShapeInWindow(2);

        // A drag that starts on another shape would move it if the canvas got the press, but the press closes the ring and is swallowed.
        scene.Window.MouseDown(start, MouseButton.Left);
        scene.Window.MouseMove(new Point(start.X + 40, start.Y + 40));
        scene.Window.MouseUp(new Point(start.X + 40, start.Y + 40), MouseButton.Left);
        Scene.Pump();

        Assert.Same(before, scene.Shell.State.Document);
        Assert.False(scene.Ring.IsVisible);
        scene.Window.Close();
    });
}
