using System;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasAreaViewModelTouchTests
{
    private sealed class Fixture
    {
        public Fixture(Tool tool, bool withShape = false)
        {
            var document = Document.Create("L");
            if (withShape)
            {
                var shape = new RectangleShape { X = 100, Y = 100, Width = 200, Height = 100 };
                ShapeId = shape.Id;
                document = document.AddShape(document.Layers[0].Id, shape);
            }

            State = new EditorState(document) { ActiveTool = tool };
            Canvas = new CanvasAreaViewModel(State) { ViewportWidth = 2000, ViewportHeight = 1200 };
            if (withShape)
            {
                State.SelectOnly(ShapeId);
            }
        }

        public Guid ShapeId { get; }

        public EditorState State { get; }

        public CanvasAreaViewModel Canvas { get; }

        public Shape Shape => Assert.Single(State.Document.Layers.SelectMany(l => l.Shapes));

        public double ScreenX(double documentX) => Canvas.OriginX + documentX * Canvas.ZoomPercent / 100;

        public double ScreenY(double documentY) => Canvas.OriginY + documentY * Canvas.ZoomPercent / 100;

        public double DocumentX(double screenX) => (screenX - Canvas.OriginX) * 100 / Canvas.ZoomPercent;

        public double DocumentY(double screenY) => (screenY - Canvas.OriginY) * 100 / Canvas.ZoomPercent;

        public (double X, double Y) HandleOnScreen(HandleType handle)
        {
            var frame = SelectionGeometry.FrameOf(SelectionQueries.SelectedShapes(State.Document, State.SelectedShapeIds))!;
            var (x, y) = SelectionGeometry.HandlePosition(frame, handle, State.ZoomPercent);
            return (ScreenX(x), ScreenY(y));
        }
    }

    // ---- one finger acts like the mouse ----

    [Fact]
    public void OneFinger_DrawsARectangle_AsOneHistoryStep()
    {
        var f = new Fixture(Tool.Rectangle);

        f.Canvas.TouchPressed(1, f.ScreenX(10), f.ScreenY(20));
        f.Canvas.TouchMoved(1, f.ScreenX(60), f.ScreenY(50));
        f.Canvas.TouchReleased(1, f.ScreenX(110), f.ScreenY(70));

        var shape = Assert.IsType<RectangleShape>(f.Shape);
        Assert.Equal((10, 20, 100, 50), (shape.X, shape.Y, shape.Width, shape.Height));
        Assert.Equal(new[] { HistoryStepType.DrawRectangle }, f.State.History.Steps);
        Assert.False(f.State.IsDragInProgress);
    }

    [Fact]
    public void OneFinger_WithTheSelectTool_MovesTheShape()
    {
        var f = new Fixture(Tool.Select, withShape: true);

        f.Canvas.TouchPressed(1, f.ScreenX(200), f.ScreenY(150));
        f.Canvas.TouchMoved(1, f.ScreenX(230), f.ScreenY(160));
        f.Canvas.TouchReleased(1, f.ScreenX(250), f.ScreenY(170));

        Assert.Equal((150, 120), (f.Shape.X, f.Shape.Y));
        Assert.Equal(new[] { HistoryStepType.MoveShape }, f.State.History.Steps);
    }

    [Fact]
    public void OneFinger_WithTheHandTool_PansTheView()
    {
        var f = new Fixture(Tool.Hand);
        var panBefore = (f.State.PanX, f.State.PanY);

        f.Canvas.TouchPressed(1, 500, 500);
        Assert.True(f.Canvas.IsPanning);
        f.Canvas.TouchMoved(1, 530, 540);
        f.Canvas.TouchReleased(1, 530, 540);

        Assert.False(f.Canvas.IsPanning);
        Assert.Equal(panBefore.PanX + 30, f.State.PanX, 6);
        Assert.Equal(panBefore.PanY + 40, f.State.PanY, 6);
    }

    [Fact]
    public void AfterAFingerIsUp_TheNextFingerDrawsAgain_WithAnotherId()
    {
        var f = new Fixture(Tool.Rectangle);
        f.Canvas.TouchPressed(1001, f.ScreenX(0), f.ScreenY(0));
        f.Canvas.TouchReleased(1001, f.ScreenX(50), f.ScreenY(50));

        f.Canvas.TouchPressed(1002, f.ScreenX(100), f.ScreenY(100));
        f.Canvas.TouchReleased(1002, f.ScreenX(180), f.ScreenY(160));

        Assert.Equal(2, f.State.Document.Layers.SelectMany(l => l.Shapes).Count());
    }

    // ---- a second finger undoes the first action ----

    [Fact]
    public void SecondFinger_DuringDrawing_LeavesNoShapeAndNoStep()
    {
        var f = new Fixture(Tool.Rectangle);
        f.Canvas.TouchPressed(1, f.ScreenX(10), f.ScreenY(10));
        f.Canvas.TouchMoved(1, f.ScreenX(110), f.ScreenY(90));
        Assert.NotNull(f.State.Draft);

        f.Canvas.TouchPressed(2, f.ScreenX(300), f.ScreenY(300));
        f.Canvas.TouchReleased(2, f.ScreenX(300), f.ScreenY(300));
        f.Canvas.TouchMoved(1, f.ScreenX(120), f.ScreenY(95));
        f.Canvas.TouchReleased(1, f.ScreenX(120), f.ScreenY(95));

        Assert.Null(f.State.Draft);
        Assert.Empty(f.State.Document.Layers.SelectMany(l => l.Shapes));
        Assert.Empty(f.State.History.Steps);
        Assert.False(f.State.IsDragInProgress);
    }

    [Fact]
    public void SecondFinger_DuringAMove_PutsTheShapeBackAndRecordsNothing()
    {
        var f = new Fixture(Tool.Select, withShape: true);
        var original = f.State.Document;

        f.Canvas.TouchPressed(1, f.ScreenX(200), f.ScreenY(150));
        f.Canvas.TouchMoved(1, f.ScreenX(260), f.ScreenY(190));
        Assert.Equal(160, f.Shape.X);

        f.Canvas.TouchPressed(2, f.ScreenX(500), f.ScreenY(500));
        f.Canvas.TouchReleased(2, f.ScreenX(500), f.ScreenY(500));
        f.Canvas.TouchReleased(1, f.ScreenX(260), f.ScreenY(190));

        Assert.Same(original, f.State.Document);
        Assert.Empty(f.State.History.Steps);
        Assert.False(f.State.CanUndo);
    }

    [Fact]
    public void SecondFinger_DuringAResize_PutsTheShapeBack()
    {
        var f = new Fixture(Tool.Select, withShape: true);
        var original = f.State.Document;
        var (hx, hy) = f.HandleOnScreen(HandleType.BottomRight);

        f.Canvas.TouchPressed(1, hx, hy);
        f.Canvas.TouchMoved(1, hx + 60, hy + 40);
        f.Canvas.TouchPressed(2, 100, 100);

        Assert.Same(original, f.State.Document);
        Assert.Empty(f.State.History.Steps);
    }

    [Fact]
    public void SecondFinger_WithTheHandTool_EndsThePanAndStartsTheGesture()
    {
        var f = new Fixture(Tool.Hand);
        f.Canvas.TouchPressed(1, 500, 500);
        Assert.True(f.Canvas.IsPanning);

        f.Canvas.TouchPressed(2, 700, 500);

        Assert.False(f.Canvas.IsPanning);
        var zoomBefore = f.State.ZoomPercent;
        f.Canvas.TouchMoved(2, 900, 500);
        Assert.True(f.State.ZoomPercent > zoomBefore);
    }

    // ---- two fingers pan and zoom ----

    [Fact]
    public void Pinch_ZoomsAroundTheMidpointOfTheFingers()
    {
        var f = new Fixture(Tool.Select);
        f.Canvas.TouchPressed(1, 500, 400);
        f.Canvas.TouchPressed(2, 700, 400);
        var anchorX = f.DocumentX(600);
        var anchorY = f.DocumentY(400);

        f.Canvas.TouchMoved(2, 900, 400); // distance 200 -> 400, midpoint 600 -> 700

        Assert.Equal(200, f.State.ZoomPercent, 6);
        Assert.Equal(anchorX, f.DocumentX(700), 4);
        Assert.Equal(anchorY, f.DocumentY(400), 4);
    }

    [Fact]
    public void Pinch_MovingTogether_ZoomsOut()
    {
        var f = new Fixture(Tool.Select);
        f.Canvas.TouchPressed(1, 500, 400);
        f.Canvas.TouchPressed(2, 900, 400);

        f.Canvas.TouchMoved(2, 700, 400); // distance 400 -> 200

        Assert.Equal(50, f.State.ZoomPercent, 6);
    }

    [Fact]
    public void TwoFingersMovingTogether_PanTheView()
    {
        var f = new Fixture(Tool.Select);
        f.Canvas.TouchPressed(1, 500, 400);
        f.Canvas.TouchPressed(2, 700, 400);
        var (originX, originY) = (f.Canvas.OriginX, f.Canvas.OriginY);

        f.Canvas.TouchMoved(1, 530, 440);
        f.Canvas.TouchMoved(2, 730, 440);

        Assert.Equal(100, f.State.ZoomPercent, 6);
        Assert.Equal(originX + 30, f.Canvas.OriginX, 6);
        Assert.Equal(originY + 40, f.Canvas.OriginY, 6);
    }

    [Fact]
    public void Pinch_IsKeptInsideTheZoomRange()
    {
        var f = new Fixture(Tool.Select);
        f.Canvas.TouchPressed(1, 500, 400);
        f.Canvas.TouchPressed(2, 600, 400);

        f.Canvas.TouchMoved(2, 500 + 100_000, 400);
        Assert.Equal(6400, f.State.ZoomPercent);

        f.Canvas.TouchMoved(2, 500.5, 400);
        Assert.Equal(10, f.State.ZoomPercent);
    }

    [Fact]
    public void WhileTwoFingersMoveTheView_UndoIsBlocked_AndTheDocumentIsUntouched()
    {
        var f = new Fixture(Tool.Rectangle);
        f.Canvas.TouchPressed(1, f.ScreenX(0), f.ScreenY(0));
        f.Canvas.TouchReleased(1, f.ScreenX(50), f.ScreenY(50));
        Assert.True(f.State.CanUndo);
        var document = f.State.Document;

        f.Canvas.TouchPressed(2, 500, 400);
        f.Canvas.TouchPressed(3, 700, 400);

        Assert.True(f.State.IsDragInProgress);
        Assert.False(f.State.CanUndo);

        f.Canvas.TouchMoved(3, 800, 400);
        f.Canvas.TouchReleased(3, 800, 400);
        f.Canvas.TouchReleased(2, 500, 400);

        Assert.False(f.State.IsDragInProgress);
        Assert.True(f.State.CanUndo);
        Assert.Same(document, f.State.Document);
        Assert.Single(f.State.History.Steps);
    }

    [Fact]
    public void AfterTheGesture_TheLiftingFingerDoesNotDraw_ButTheNextTouchDoes()
    {
        var f = new Fixture(Tool.Rectangle);
        f.Canvas.TouchPressed(1, f.ScreenX(0), f.ScreenY(0));
        f.Canvas.TouchPressed(2, f.ScreenX(100), f.ScreenY(0));
        f.Canvas.TouchReleased(2, f.ScreenX(100), f.ScreenY(0));
        f.Canvas.TouchMoved(1, f.ScreenX(80), f.ScreenY(80));
        f.Canvas.TouchReleased(1, f.ScreenX(80), f.ScreenY(80));
        Assert.Empty(f.State.Document.Layers.SelectMany(l => l.Shapes));

        f.Canvas.TouchPressed(3, f.ScreenX(10), f.ScreenY(10));
        f.Canvas.TouchReleased(3, f.ScreenX(110), f.ScreenY(90));

        Assert.Single(f.State.Document.Layers.SelectMany(l => l.Shapes));
    }

    [Fact]
    public void AThirdFinger_DoesNotDisturbTheGesture()
    {
        var f = new Fixture(Tool.Rectangle);
        f.Canvas.TouchPressed(1, 500, 400);
        f.Canvas.TouchPressed(2, 700, 400);

        f.Canvas.TouchPressed(3, 900, 900);
        f.Canvas.TouchMoved(3, 950, 950);
        f.Canvas.TouchReleased(3, 950, 950);
        f.Canvas.TouchMoved(2, 900, 400);

        Assert.Equal(200, f.State.ZoomPercent, 6);
        Assert.Empty(f.State.Document.Layers.SelectMany(l => l.Shapes));
    }

    // ---- cancel by the system ----

    [Fact]
    public void TouchCancelled_DuringAMove_PutsTheShapeBackAndForgetsTheFinger()
    {
        var f = new Fixture(Tool.Select, withShape: true);
        var original = f.State.Document;
        f.Canvas.TouchPressed(1, f.ScreenX(200), f.ScreenY(150));
        f.Canvas.TouchMoved(1, f.ScreenX(260), f.ScreenY(190));

        f.Canvas.TouchCancelled();
        f.Canvas.TouchMoved(1, f.ScreenX(300), f.ScreenY(200));
        f.Canvas.TouchReleased(1, f.ScreenX(300), f.ScreenY(200));

        Assert.Same(original, f.State.Document);
        Assert.Empty(f.State.History.Steps);
        Assert.False(f.State.IsDragInProgress);
    }

    [Fact]
    public void TouchCancelled_AfterANormalRelease_ChangesNothing()
    {
        var f = new Fixture(Tool.Rectangle);
        f.Canvas.TouchPressed(1, f.ScreenX(10), f.ScreenY(10));
        f.Canvas.TouchReleased(1, f.ScreenX(110), f.ScreenY(90));

        f.Canvas.TouchCancelled();

        Assert.Single(f.State.Document.Layers.SelectMany(l => l.Shapes));
        Assert.Equal(new[] { HistoryStepType.DrawRectangle }, f.State.History.Steps);
    }

    [Fact]
    public void TouchCancelled_DuringAGesture_EndsItAndFreesTheHistory()
    {
        var f = new Fixture(Tool.Select);
        f.Canvas.TouchPressed(1, 500, 400);
        f.Canvas.TouchPressed(2, 700, 400);
        Assert.True(f.State.IsDragInProgress);

        f.Canvas.TouchCancelled();

        Assert.False(f.State.IsDragInProgress);
    }

    // ---- fingers grab handles from a larger area ----

    [Fact]
    public void Finger_FifteenPixelsFromAHandle_GrabsIt_ButTheMouseDoesNot()
    {
        var touch = new Fixture(Tool.Select, withShape: true);
        var (hx, hy) = touch.HandleOnScreen(HandleType.BottomRight);
        touch.Canvas.TouchPressed(1, hx + 15, hy + 15);
        touch.Canvas.TouchMoved(1, hx + 55, hy + 45);
        touch.Canvas.TouchReleased(1, hx + 55, hy + 45);

        var mouse = new Fixture(Tool.Select, withShape: true);
        mouse.Canvas.PointerPressed(hx + 15, hy + 15);
        mouse.Canvas.PointerMoved(hx + 55, hy + 45);
        mouse.Canvas.PointerReleased(hx + 55, hy + 45);

        Assert.Equal(new[] { HistoryStepType.ResizeShapes }, touch.State.History.Steps);
        Assert.True(touch.Shape.Width > 200);
        Assert.Empty(mouse.State.History.Steps);
        Assert.Equal(200, mouse.Shape.Width);
    }

    [Fact]
    public void TouchGrabSize_CanBeChanged()
    {
        var f = new Fixture(Tool.Select, withShape: true);
        Assert.Equal(SelectionGeometry.TouchGrabSize, f.Canvas.TouchGrabSize);

        f.Canvas.TouchGrabSize = 12;

        Assert.Equal(12, f.Canvas.TouchGrabSize);
        var (hx, hy) = f.HandleOnScreen(HandleType.BottomRight);
        f.Canvas.TouchPressed(1, hx + 15, hy + 15);
        f.Canvas.TouchReleased(1, hx + 15, hy + 15);
        Assert.Empty(f.State.History.Steps);
    }

    // ---- mouse stays as it was ----

    [Fact]
    public void Mouse_StillDrawsWithTheExistingMethods()
    {
        var f = new Fixture(Tool.Rectangle);

        f.Canvas.PointerPressed(f.ScreenX(10), f.ScreenY(20));
        f.Canvas.PointerMoved(f.ScreenX(60), f.ScreenY(50));
        f.Canvas.PointerReleased(f.ScreenX(110), f.ScreenY(70));

        Assert.Equal(new[] { HistoryStepType.DrawRectangle }, f.State.History.Steps);
    }
}
