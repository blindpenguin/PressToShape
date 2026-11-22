using System;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasAreaViewModelDoubleTapTests
{
    private static readonly RectangleShape A = new() { X = 100, Y = 100, Width = 30, Height = 30 };
    private static readonly RectangleShape B = new() { X = 160, Y = 100, Width = 30, Height = 30 };
    private static readonly GroupShape Group = new() { Children = [A, B] };

    private sealed record Fixture(EditorState State, CanvasAreaViewModel Canvas, ManualTimeProvider Time)
    {
        // The artboard is centered in the viewport, so a document point is a screen point plus the origin.
        public double ScreenX(double documentX) => Canvas.OriginX + documentX;

        public double ScreenY(double documentY) => Canvas.OriginY + documentY;

        public void Tap(int id, double documentX, double documentY)
        {
            Canvas.TouchPressed(id, ScreenX(documentX), ScreenY(documentY));
            Canvas.TouchReleased(id, ScreenX(documentX), ScreenY(documentY));
        }
    }

    private static Fixture Create()
    {
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [Group, new RectangleShape { X = 400, Width = 30, Height = 30 }])) { ActiveTool = Tool.Select };
        var time = new ManualTimeProvider();
        var canvas = new CanvasAreaViewModel(state, null, time) { ViewportWidth = 1600, ViewportHeight = 900 };
        return new Fixture(state, canvas, time);
    }

    [Fact]
    public void DoubleTap_OnAGroup_EntersItAndSelectsTheChildUnderTheFinger()
    {
        var f = Create();

        f.Tap(1, 170, 110);
        f.Time.Advance(200);
        f.Tap(2, 170, 110);

        Assert.Equal(Group.Id, f.State.EnteredGroupId);
        Assert.Equal(new[] { B.Id }, f.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void SingleTap_OnAGroup_OnlySelectsIt()
    {
        var f = Create();

        f.Tap(1, 170, 110);

        Assert.Null(f.State.EnteredGroupId);
        Assert.Equal(new[] { Group.Id }, f.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void TwoTaps_TooFarApartInTime_DoNotEnter()
    {
        var f = Create();

        f.Tap(1, 170, 110);
        f.Time.Advance(DoubleTapDetector.MaxIntervalMilliseconds + 100);
        f.Tap(2, 170, 110);

        Assert.Null(f.State.EnteredGroupId);
    }

    [Fact]
    public void TwoTaps_OnDifferentSpots_DoNotEnter()
    {
        var f = Create();

        f.Tap(1, 110, 110);
        f.Time.Advance(100);
        f.Tap(2, 170, 110);

        Assert.Null(f.State.EnteredGroupId);
    }

    [Fact]
    public void DoubleTap_OnEmptySpaceInsideAnEnteredGroup_LeavesIt()
    {
        var f = Create();
        f.State.EnterGroup(Group.Id);

        f.Tap(1, 700, 600);
        f.Time.Advance(100);
        f.Tap(2, 700, 600);

        Assert.Null(f.State.EnteredGroupId);
    }

    [Fact]
    public void TapBesideTheEnteredGroup_LeavesItWithoutADoubleTap()
    {
        var f = Create();
        f.State.EnterGroup(Group.Id);

        f.Tap(1, 700, 600);

        Assert.Null(f.State.EnteredGroupId);
        Assert.Equal(new[] { Group.Id }, f.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void TapThenADragOfTheSameShape_StillDragsAndIsNoDoubleTap()
    {
        var f = Create();
        f.Tap(1, 110, 110);
        f.Time.Advance(100);
        var steps = f.State.History.Steps.Count;

        f.Canvas.TouchPressed(2, f.ScreenX(110), f.ScreenY(110));
        f.Canvas.TouchMoved(2, f.ScreenX(140), f.ScreenY(130));
        f.Canvas.TouchReleased(2, f.ScreenX(140), f.ScreenY(130));

        Assert.Null(f.State.EnteredGroupId);
        Assert.Equal(steps + 1, f.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.MoveShape, f.State.History.Steps[^1]);
    }

    [Fact]
    public void DoubleTap_WithAnotherTool_DoesNothing()
    {
        var f = Create();
        f.State.ActiveTool = Tool.Hand;

        f.Tap(1, 170, 110);
        f.Time.Advance(100);
        f.Tap(2, 170, 110);

        Assert.Null(f.State.EnteredGroupId);
    }

    [Fact]
    public void TwoFingerGesture_BetweenTaps_BreaksTheDoubleTap()
    {
        var f = Create();
        f.Tap(1, 170, 110);
        f.Canvas.TouchPressed(5, f.ScreenX(170), f.ScreenY(110));
        f.Canvas.TouchPressed(6, f.ScreenX(300), f.ScreenY(110));
        f.Canvas.TouchMoved(6, f.ScreenX(320), f.ScreenY(110));
        f.Canvas.TouchReleased(5, f.ScreenX(170), f.ScreenY(110));
        f.Canvas.TouchReleased(6, f.ScreenX(320), f.ScreenY(110));
        f.Time.Advance(100);

        f.Tap(2, 170, 110);

        Assert.Null(f.State.EnteredGroupId);
    }

    // A finger reaches a guide from farther away than a mouse or a pen: 8 pixels is in reach of a finger only.
    private static bool DoubleClickReachesTheGuide(InputPointerType inputType)
    {
        var f = Create();
        f.State.ActiveTool = Tool.Rectangle;
        var guide = new Guide { Orientation = GuideOrientation.Vertical, Position = 300 };
        f.State.Edit(HistoryStepType.CreateGuide, d => d.AddGuide(guide));

        f.Canvas.DoubleClick(f.ScreenX(300) + 8, f.ScreenY(500), inputType: inputType);

        return f.Canvas.CursorType == CanvasCursorType.ResizeWestEast;
    }

    [Fact]
    public void DoubleClick_WithAnotherToolByTouch_PressesAsATouch() => Assert.True(DoubleClickReachesTheGuide(InputPointerType.Touch));

    [Fact]
    public void DoubleClick_WithAnotherToolByPen_PressesAsAPen() => Assert.False(DoubleClickReachesTheGuide(InputPointerType.Pen));

    [Fact]
    public void DoubleClick_WithAnotherToolByMouse_PressesAsAMouse() => Assert.False(DoubleClickReachesTheGuide(InputPointerType.Mouse));
}
