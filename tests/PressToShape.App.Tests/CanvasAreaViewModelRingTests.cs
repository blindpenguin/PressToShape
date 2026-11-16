using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasAreaViewModelRingTests
{
    // The ring of the fixture has two slots for a selection and one for the empty canvas, so a test can tell the modes apart.
    private sealed class Fixture
    {
        public Fixture(Tool tool = Tool.Select, bool withRing = true)
        {
            First = new RectangleShape { X = 100, Y = 100, Width = 200, Height = 100 };
            Second = new RectangleShape { X = 500, Y = 100, Width = 100, Height = 100 };
            var document = Document.Create("L");
            document = document.AddShapes(document.Layers[0].Id, [First, Second]);
            State = new EditorState(document) { ActiveTool = tool };
            Ring = withRing ? new RingMenuViewModel(FakeLocalizer.Create()) { Content = Content } : null;
            Canvas = new CanvasAreaViewModel(State, ring: Ring) { ViewportWidth = 2000, ViewportHeight = 1200 };
        }

        public RectangleShape First { get; }

        public RectangleShape Second { get; }

        public EditorState State { get; }

        public RingMenuViewModel? Ring { get; }

        public CanvasAreaViewModel Canvas { get; }

        public double ScreenX(double documentX) => Canvas.OriginX + documentX * Canvas.ZoomPercent / 100;

        public double ScreenY(double documentY) => Canvas.OriginY + documentY * Canvas.ZoomPercent / 100;

        public bool RightClick(double documentX, double documentY) => Canvas.OpenContextMenu(ScreenX(documentX), ScreenY(documentY));

        private static IReadOnlyList<RingSlotDefinition> Content(RingMenuMode mode) =>
            mode == RingMenuMode.Selection
                ? [Slot("One"), Slot("Two")]
                : [Slot("Only")];

        private static RingSlotDefinition Slot(string name) =>
            RingSlotDefinition.WithCommand(name, MaterialIconKind.Layers, new RelayCommand(() => { }));
    }

    [Fact]
    public void OpenContextMenu_OnASelectedShape_OpensTheSelectionRingAndKeepsTheSelection()
    {
        var f = new Fixture();
        f.State.SetSelection([f.First.Id, f.Second.Id]);

        var opened = f.RightClick(150, 150);

        Assert.True(opened);
        Assert.True(f.Ring!.IsOpen);
        Assert.Equal(2, f.Ring.Slots.Count);
        Assert.Equal([f.First.Id, f.Second.Id], f.State.SelectedShapeIds.ToArray());
        Assert.Equal(f.ScreenX(150), f.Ring.CenterX);
        Assert.Equal(f.ScreenY(150), f.Ring.CenterY);
    }

    [Fact]
    public void OpenContextMenu_OnAnUnselectedShape_SelectsItFirst()
    {
        var f = new Fixture();
        f.State.SelectOnly(f.First.Id);

        f.RightClick(550, 150);

        Assert.Equal([f.Second.Id], f.State.SelectedShapeIds.ToArray());
        Assert.Equal(2, f.Ring!.Slots.Count);
    }

    [Fact]
    public void OpenContextMenu_OnEmptyCanvas_ClearsTheSelectionAndOpensTheEmptyCanvasRing()
    {
        var f = new Fixture();
        f.State.SelectOnly(f.First.Id);

        f.RightClick(400, 600);

        Assert.False(f.State.HasSelection);
        Assert.True(f.Ring!.IsOpen);
        Assert.Single(f.Ring.Slots);
    }

    [Fact]
    public void OpenContextMenu_WhileOpen_MovesTheRingToTheNewPoint()
    {
        var f = new Fixture();
        f.RightClick(150, 150);

        f.RightClick(550, 150);

        Assert.Equal(f.ScreenX(550), f.Ring!.CenterX);
        Assert.Equal([f.Second.Id], f.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void OpenContextMenu_WithAnotherTool_DoesNothing()
    {
        foreach (var tool in new[] { Tool.Rectangle, Tool.Hand, Tool.Zoom })
        {
            var f = new Fixture(tool);
            f.State.SelectOnly(f.First.Id);

            var opened = f.RightClick(550, 150);

            Assert.False(opened);
            Assert.False(f.Ring!.IsOpen);
            Assert.Equal([f.First.Id], f.State.SelectedShapeIds.ToArray());
        }
    }

    [Fact]
    public void OpenContextMenu_DuringADrag_DoesNothing()
    {
        var f = new Fixture();
        f.State.SelectOnly(f.First.Id);
        f.Canvas.PointerPressed(f.ScreenX(150), f.ScreenY(150));
        Assert.True(f.State.IsDragInProgress);

        var opened = f.RightClick(550, 150);

        Assert.False(opened);
        Assert.False(f.Ring!.IsOpen);
    }

    [Fact]
    public void OpenContextMenu_DuringAPan_DoesNothing()
    {
        var f = new Fixture();
        f.Canvas.MiddlePressed(300, 300);
        Assert.True(f.Canvas.IsPanning);

        var opened = f.RightClick(150, 150);

        Assert.False(opened);
        Assert.False(f.Ring!.IsOpen);
    }

    [Fact]
    public void OpenContextMenu_DuringAColorPick_DoesNothing()
    {
        var f = new Fixture();
        f.State.BeginColorPick(ColorPickTarget.Fill);

        var opened = f.RightClick(150, 150);

        Assert.False(opened);
        Assert.False(f.Ring!.IsOpen);
    }

    [Fact]
    public void OpenContextMenu_WithoutARing_DoesNothing()
    {
        var f = new Fixture(withRing: false);

        Assert.Null(f.Canvas.Ring);
        Assert.False(f.RightClick(150, 150));
        Assert.False(f.Canvas.KeyPressed(CanvasKey.ContextMenu, shift: false));
    }

    [Fact]
    public void KeyPressed_ContextMenuWithASelection_OpensInTheMiddleOfTheSelectionWithTheFirstSlotFocused()
    {
        var f = new Fixture();
        f.State.SelectOnly(f.First.Id);

        var handled = f.Canvas.KeyPressed(CanvasKey.ContextMenu, shift: false);

        Assert.True(handled);
        Assert.True(f.Ring!.IsOpen);
        Assert.Equal(2, f.Ring.Slots.Count);
        Assert.Equal(f.ScreenX(200), f.Ring.CenterX, 6);
        Assert.Equal(f.ScreenY(150), f.Ring.CenterY, 6);
        Assert.Equal(0, f.Ring.FocusedIndex);
    }

    [Fact]
    public void KeyPressed_ContextMenuWithoutASelection_OpensTheEmptyCanvasRingInTheMiddleOfTheViewport()
    {
        var f = new Fixture();

        var handled = f.Canvas.KeyPressed(CanvasKey.ContextMenu, shift: false);

        Assert.True(handled);
        Assert.Single(f.Ring!.Slots);
        Assert.Equal(1000, f.Ring.CenterX);
        Assert.Equal(600, f.Ring.CenterY);
    }

    [Fact]
    public void KeyPressed_ContextMenuWithAnotherTool_IsNotHandled()
    {
        var f = new Fixture(Tool.Rectangle);

        Assert.False(f.Canvas.KeyPressed(CanvasKey.ContextMenu, shift: false));
        Assert.False(f.Ring!.IsOpen);
    }

    [Fact]
    public void SelectionChange_WhileOpen_ClosesTheRing()
    {
        var f = new Fixture();
        f.RightClick(150, 150);

        f.State.ClearSelection();

        Assert.False(f.Ring!.IsOpen);
    }

    [Fact]
    public void DocumentChange_WhileOpen_ClosesTheRing()
    {
        var f = new Fixture();
        f.RightClick(150, 150);

        f.State.Document = Document.Create("Other");

        Assert.False(f.Ring!.IsOpen);
    }

    [Fact]
    public void OpenContextMenu_AddsNoHistoryStep()
    {
        var f = new Fixture();
        var steps = f.State.History.Steps.Count;

        f.RightClick(150, 150);
        f.RightClick(400, 600);

        Assert.Equal(steps, f.State.History.Steps.Count);
    }
}
