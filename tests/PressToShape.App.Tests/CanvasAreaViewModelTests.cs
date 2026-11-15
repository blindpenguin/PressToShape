using System;
using System.Collections.Generic;
using PressToShape.App.ViewModels.Editor;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasAreaViewModelTests
{
    private static (EditorState State, CanvasAreaViewModel Canvas) Create(double width, double height)
    {
        var state = new EditorState();
        var canvas = new CanvasAreaViewModel(state) { ViewportWidth = width, ViewportHeight = height };
        return (state, canvas);
    }

    [Fact]
    public void ArtboardScreenSize_FollowsZoom()
    {
        var (state, canvas) = Create(2000, 1200);

        Assert.Equal(1200, canvas.ArtboardScreenWidth);
        Assert.Equal(800, canvas.ArtboardScreenHeight);

        state.SetZoom(50);

        Assert.Equal(600, canvas.ArtboardScreenWidth);
        Assert.Equal(400, canvas.ArtboardScreenHeight);
    }

    [Fact]
    public void Artboard_IsCentered_WhenSmallerThanViewport()
    {
        var (_, canvas) = Create(2000, 1200);

        Assert.Equal(400, canvas.OriginX);
        Assert.Equal(200, canvas.OriginY);
    }

    [Fact]
    public void Origin_StaysCentered_WhenViewportOrZoomChanges()
    {
        var (state, canvas) = Create(1000, 700);
        state.SetZoom(50); // artboard 600 x 400

        Assert.Equal(200, canvas.OriginX);
        Assert.Equal(150, canvas.OriginY);

        canvas.ViewportWidth = 1400;

        Assert.Equal(400, canvas.OriginX);
    }

    [Fact]
    public void Origin_IsCenteredWithOverflowOnBothSides_WhenArtboardIsLargerThanViewport()
    {
        var (_, canvas) = Create(800, 500);

        Assert.Equal(-200, canvas.OriginX);
        Assert.Equal(-150, canvas.OriginY);
    }

    [Fact]
    public void ZeroViewport_GivesFiniteValues_WithoutErrors()
    {
        var (state, canvas) = Create(0, 0);

        state.SetZoom(200);

        Assert.True(double.IsFinite(canvas.OriginX));
        Assert.True(double.IsFinite(canvas.OriginY));
        Assert.Equal(0, state.PanX);
        Assert.Equal(0, state.PanY);
        Assert.Equal(2400, canvas.ArtboardScreenWidth);
    }

    [Fact]
    public void ZoomChange_RaisesNotificationsForDerivedValues()
    {
        var (state, canvas) = Create(2000, 1200);
        var changed = new List<string?>();
        canvas.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.SetZoom(200);

        Assert.Contains(nameof(CanvasAreaViewModel.ZoomPercent), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.ArtboardScreenWidth), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.OriginX), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.OriginY), changed);
    }

    [Fact]
    public void ViewportChange_RaisesOriginNotifications()
    {
        var (_, canvas) = Create(2000, 1200);
        var changed = new List<string?>();
        canvas.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        canvas.ViewportWidth = 2500;
        canvas.ViewportHeight = 1500;

        Assert.Contains(nameof(CanvasAreaViewModel.OriginX), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.OriginY), changed);
    }

    [Fact]
    public void ArtboardSizeChange_UpdatesScreenSize()
    {
        var (state, canvas) = Create(2000, 1200);

        state.Document = state.Document.WithArtboardSize(1920, 1080);

        Assert.Equal(1920, canvas.ArtboardScreenWidth);
        Assert.Equal(1080, canvas.ArtboardScreenHeight);
    }

    [Fact]
    public void Document_IsTheDocumentOfTheState()
    {
        var (state, canvas) = Create(2000, 1200);

        Assert.Same(state.Document, canvas.Document);
    }

    [Fact]
    public void DocumentChange_RaisesDocumentNotification()
    {
        var (state, canvas) = Create(2000, 1200);
        var changed = new List<string?>();
        canvas.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        var newDocument = state.Document.UpdateLayer(state.Document.Layers[0].Id, l => l with { IsVisible = false });

        state.Document = newDocument;

        Assert.Contains(nameof(CanvasAreaViewModel.Document), changed);
        Assert.Same(newDocument, canvas.Document);
    }

    [Fact]
    public void NewArtboardSize_ChangesArtboardPropertiesAndOrigins()
    {
        var (state, canvas) = Create(2000, 1200);
        var changed = new List<string?>();
        canvas.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.Document = state.Document.WithArtboardSize(1000, 600);

        Assert.Equal(1000, canvas.ArtboardWidth);
        Assert.Equal(600, canvas.ArtboardHeight);
        Assert.Equal(500, canvas.OriginX);
        Assert.Equal(300, canvas.OriginY);
        Assert.Contains(nameof(CanvasAreaViewModel.ArtboardWidth), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.OriginX), changed);
        Assert.Contains(nameof(CanvasAreaViewModel.OriginY), changed);
    }

    [Fact]
    public void ZoomChange_DoesNotRaiseDocumentNotification()
    {
        var (state, canvas) = Create(2000, 1200);
        var changed = new List<string?>();
        canvas.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.SetZoom(200);

        Assert.DoesNotContain(nameof(CanvasAreaViewModel.Document), changed);
    }

    private static List<string?> Record(CanvasAreaViewModel canvas)
    {
        var raised = new List<string?>();
        canvas.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        return raised;
    }

    [Fact]
    public void Pointer_DrawAtZoom200WithAnOrigin_CreatesTheShapeAtTheDocumentPosition()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Rectangle;
        state.SetZoom(200);
        var (originX, originY) = (canvas.OriginX, canvas.OriginY);

        canvas.PointerPressed(originX + 100, originY + 60);
        canvas.PointerMoved(originX + 300, originY + 160);
        canvas.PointerReleased(originX + 500, originY + 260);

        var shape = Assert.Single(state.Document.Layers.SelectMany(l => l.Shapes));
        Assert.Equal((50, 30, 200, 100), (shape.X, shape.Y, shape.Width, shape.Height));
    }

    [Fact]
    public void Draft_DuringADrag_IsInScreenSpaceAndNullAfterwards()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Ellipse;
        state.SetZoom(200);
        var origin = (canvas.OriginX, canvas.OriginY);
        var raised = Record(canvas);

        canvas.PointerPressed(origin.OriginX + 20, origin.OriginY + 20);
        canvas.PointerMoved(origin.OriginX + 120, origin.OriginY + 70);

        var draft = canvas.Draft!.Value;
        Assert.Equal(ScreenShapeType.Ellipse, draft.Type);
        Assert.Equal((origin.OriginX + 20, origin.OriginY + 20, 100, 50), (draft.X, draft.Y, draft.Width, draft.Height));
        Assert.Contains(nameof(CanvasAreaViewModel.Draft), raised);

        canvas.PointerReleased(origin.OriginX + 120, origin.OriginY + 70);

        Assert.Null(canvas.Draft);
    }

    [Fact]
    public void DeleteSelection_SelectedShape_RemovesIt()
    {
        var (state, canvas) = Create(2000, 1200);
        var shape = new RectangleShape { Width = 10, Height = 10 };
        state.Document = state.Document.AddShape(state.ActiveLayerId, shape);
        state.Select(shape.Id);

        canvas.DeleteSelection();

        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
        Assert.Empty(canvas.Overlay.Outlines);
    }

    [Fact]
    public void Cancel_DuringADrag_ClearsTheDraft()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Rectangle;
        canvas.PointerPressed(canvas.OriginX + 10, canvas.OriginY + 10);
        canvas.PointerMoved(canvas.OriginX + 90, canvas.OriginY + 90);

        canvas.Cancel();

        Assert.Null(canvas.Draft);
    }

    [Fact]
    public void Origin_FollowsTheViewOffset()
    {
        var (state, canvas) = Create(2000, 1200);
        var raised = new List<string?>();
        canvas.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        state.SetPan(150, -70);

        Assert.Equal(400 + 150, canvas.OriginX);
        Assert.Equal(200 - 70, canvas.OriginY);
        Assert.Contains(nameof(CanvasAreaViewModel.OriginX), raised);
        Assert.Contains(nameof(CanvasAreaViewModel.OriginY), raised);
        Assert.Contains(nameof(CanvasAreaViewModel.Overlay), raised);
    }

    [Fact]
    public void ViewportChanges_ReachTheStateAndLimitTheOffset()
    {
        var (state, canvas) = Create(2000, 1200);
        state.SetPan(ViewGeometry.PanLimit(2000, 1200), 0);

        canvas.ViewportWidth = 300;

        Assert.Equal(300, state.ViewportWidth);
        Assert.Equal(1200, state.ViewportHeight);
        Assert.Equal(ViewGeometry.PanLimit(300, 1200), state.PanX);
    }

    [Fact]
    public void Pointer_DrawAtAViewOffsetAndZoom_LandsUnderThePointer()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Rectangle;
        state.SetZoom(200);
        state.SetPan(-120, 80);
        var (originX, originY) = (canvas.OriginX, canvas.OriginY);

        canvas.PointerPressed(originX + 100, originY + 60);
        canvas.PointerReleased(originX + 300, originY + 160);

        var shape = Assert.Single(state.Document.Layers.SelectMany(l => l.Shapes));
        Assert.Equal((50, 30, 100, 50), (shape.X, shape.Y, shape.Width, shape.Height));
    }

    // ---- wheel, panning and the Zoom tool ----

    private static double DocumentXUnder(CanvasAreaViewModel canvas, double screenX) =>
        CanvasMapping.ToDocument(screenX, canvas.OriginX, canvas.ZoomPercent);

    private static void AssertNoEditing(EditorState state)
    {
        Assert.Empty(state.History.Steps);
        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
    }

    [Theory]
    [InlineData(1, 0, 0, 48)]
    [InlineData(-1, 0, 0, -48)]
    [InlineData(0.25, 0, 0, 12)]
    [InlineData(2, 0, 0, 96)]
    [InlineData(0, 1, 48, 0)]
    [InlineData(0, -2, -96, 0)]
    public void Wheel_WithoutModifier_MovesTheView(double deltaY, double deltaX, double expectedPanX, double expectedPanY)
    {
        var (state, canvas) = Create(2000, 1200);

        var handled = canvas.Wheel(deltaX, deltaY, control: false, shift: false, 500, 500);

        Assert.True(handled);
        Assert.Equal((expectedPanX, expectedPanY), (state.PanX, state.PanY));
    }

    [Fact]
    public void Wheel_WithShift_MovesTheViewHorizontally()
    {
        var (state, canvas) = Create(2000, 1200);

        var handled = canvas.Wheel(0, 1, control: false, shift: true, 500, 500);

        Assert.True(handled);
        Assert.Equal((48, 0), (state.PanX, state.PanY));
    }

    [Fact]
    public void Wheel_WithShiftAndAHorizontalDelta_KeepsTheHorizontalDelta()
    {
        var (state, canvas) = Create(2000, 1200);

        canvas.Wheel(2, 5, control: false, shift: true, 500, 500);

        Assert.Equal((96, 240), (state.PanX, state.PanY));
    }

    [Fact]
    public void Wheel_WithControl_ZoomsAroundThePointer()
    {
        var (state, canvas) = Create(2000, 1200);
        var before = DocumentXUnder(canvas, 700);

        var handled = canvas.Wheel(0, 1, control: true, shift: false, 700, 300);

        Assert.True(handled);
        Assert.Equal(110, state.ZoomPercent, precision: 9);
        Assert.Equal(before, DocumentXUnder(canvas, 700), precision: 6);
    }

    [Fact]
    public void Wheel_WithControlDown_ZoomsOut()
    {
        var (state, canvas) = Create(2000, 1200);

        canvas.Wheel(0, -1, control: true, shift: false, 700, 300);

        Assert.Equal(100 / 1.1, state.ZoomPercent, precision: 9);
    }

    [Fact]
    public void Wheel_WithControlAtTheZoomLimit_ChangesNothing()
    {
        var (state, canvas) = Create(2000, 1200);
        state.SetZoom(ZoomLevels.Max);

        Assert.False(canvas.Wheel(0, 1, control: true, shift: false, 700, 300));
        Assert.Equal(ZoomLevels.Max, state.ZoomPercent);
    }

    [Fact]
    public void Wheel_AtThePanLimit_ReturnsFalse()
    {
        var (state, canvas) = Create(2000, 1200);
        state.SetPan(ViewGeometry.PanLimit(2000, 1200), 0);

        Assert.False(canvas.Wheel(1, 0, control: false, shift: false, 500, 500));
    }

    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity, 1)]
    [InlineData(0, 0)]
    public void Wheel_NonFiniteOrZeroDelta_IsIgnored(double deltaX, double deltaY)
    {
        var (state, canvas) = Create(2000, 1200);

        Assert.False(canvas.Wheel(deltaX, deltaY, control: false, shift: false, 500, 500));
        Assert.False(canvas.Wheel(deltaX, deltaY, control: true, shift: false, 500, 500));
        Assert.Equal((0, 0), (state.PanX, state.PanY));
        Assert.Equal(100, state.ZoomPercent);
    }

    [Fact]
    public void Wheel_NonFinitePointerPosition_IsIgnored()
    {
        var (state, canvas) = Create(2000, 1200);

        Assert.False(canvas.Wheel(0, 1, control: true, shift: false, double.NaN, 5));
        Assert.Equal(100, state.ZoomPercent);
    }

    [Fact]
    public void Wheel_WhileADragIsInProgress_IsIgnored()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Rectangle;
        canvas.PointerPressed(canvas.OriginX + 10, canvas.OriginY + 10);

        Assert.False(canvas.Wheel(0, 1, control: false, shift: false, 500, 500));
        Assert.False(canvas.Wheel(0, 1, control: true, shift: false, 500, 500));

        Assert.Equal((0, 0), (state.PanX, state.PanY));
        Assert.Equal(100, state.ZoomPercent);
    }

    [Fact]
    public void Wheel_NeverTouchesTheDocumentOrTheHistory()
    {
        var (state, canvas) = Create(2000, 1200);
        var document = state.Document;

        canvas.Wheel(0, 3, control: false, shift: false, 500, 500);
        canvas.Wheel(0, 2, control: true, shift: false, 500, 500);

        Assert.Same(document, state.Document);
        AssertNoEditing(state);
        Assert.False(state.IsDragInProgress);
    }

    [Fact]
    public void HandTool_Drag_FollowsThePointerOneToOne_AndCreatesNothing()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Hand;
        var before = DocumentXUnder(canvas, 500);

        canvas.PointerPressed(500, 400);
        Assert.True(canvas.IsPanning);
        Assert.True(state.IsDragInProgress);
        canvas.PointerMoved(560, 450);
        Assert.Equal((60, 50), (state.PanX, state.PanY));
        canvas.PointerMoved(580, 470);
        canvas.PointerReleased(580, 470);

        Assert.Equal((80, 70), (state.PanX, state.PanY));
        Assert.Equal(before, DocumentXUnder(canvas, 580), precision: 6);
        Assert.False(canvas.IsPanning);
        Assert.False(state.IsDragInProgress);
        AssertNoEditing(state);
        Assert.False(state.HasSelection);
    }

    [Fact]
    public void HandTool_PressOnAShape_DoesNotSelectOrMoveIt()
    {
        var (state, canvas) = Create(2000, 1200);
        var shape = new RectangleShape { X = 100, Y = 100, Width = 300, Height = 300 };
        state.Edit(PressToShape.Core.History.HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, shape));
        state.Select(null);
        state.ActiveTool = Tool.Hand;

        canvas.PointerPressed(canvas.OriginX + 200, canvas.OriginY + 200);
        canvas.PointerMoved(canvas.OriginX + 260, canvas.OriginY + 240);
        canvas.PointerReleased(canvas.OriginX + 260, canvas.OriginY + 240);

        Assert.False(state.HasSelection);
        Assert.Equal(100, state.Document.FindShape(shape.Id)!.Shape.X);
        Assert.Single(state.History.Steps);
    }

    [Fact]
    public void HandTool_DragFarAway_IsLimited_AndRecoversWhenTheDragComesBack()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Hand;
        var limit = ViewGeometry.PanLimit(2000, 1200);

        canvas.PointerPressed(1000, 600);
        canvas.PointerMoved(1000 + 50000, 600);
        Assert.Equal(limit, state.PanX);

        canvas.PointerMoved(1000 + 30, 600);
        Assert.Equal(30, state.PanX);
        canvas.PointerReleased(1000 + 30, 600);
    }

    [Fact]
    public void Pan_Cancel_EndsItAndFurtherMovesDoNothing()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Hand;
        canvas.PointerPressed(500, 400);
        canvas.PointerMoved(540, 400);

        canvas.Cancel();
        canvas.PointerMoved(900, 400);

        Assert.False(canvas.IsPanning);
        Assert.False(state.IsDragInProgress);
        Assert.Equal(40, state.PanX);
    }

    [Fact]
    public void MiddleDrag_PansWithAnyTool_AndTheToolStays()
    {
        foreach (var tool in new[] { Tool.Select, Tool.Rectangle, Tool.Ellipse, Tool.Zoom })
        {
            var (state, canvas) = Create(2000, 1200);
            state.ActiveTool = tool;

            canvas.MiddlePressed(500, 400);
            canvas.PointerMoved(530, 380);
            canvas.PointerReleased(530, 380);

            Assert.Equal((30, -20), (state.PanX, state.PanY));
            Assert.Equal(tool, state.ActiveTool);
            AssertNoEditing(state);
        }
    }

    [Fact]
    public void MiddlePress_WhileALeftDragIsInProgress_IsIgnored()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Rectangle;
        canvas.PointerPressed(canvas.OriginX + 10, canvas.OriginY + 10);

        canvas.MiddlePressed(500, 400);
        canvas.PointerMoved(canvas.OriginX + 110, canvas.OriginY + 110);

        Assert.False(canvas.IsPanning);
        Assert.Equal((0, 0), (state.PanX, state.PanY));
        Assert.NotNull(state.Draft);
        canvas.PointerReleased(canvas.OriginX + 110, canvas.OriginY + 110);
        Assert.Single(state.Document.Layers.SelectMany(l => l.Shapes));
    }

    [Fact]
    public void Pan_BlocksUndoAndTheHistoryUntilItEnds()
    {
        var (state, canvas) = Create(2000, 1200);
        state.Edit(PressToShape.Core.History.HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, new RectangleShape { Width = 5, Height = 5 }));
        state.ActiveTool = Tool.Hand;
        var document = state.Document;

        canvas.PointerPressed(500, 400);
        Assert.False(state.CanUndo);
        state.Undo();
        state.JumpToStep(0);
        Assert.Same(document, state.Document);

        canvas.PointerReleased(500, 400);
        Assert.True(state.CanUndo);
        Assert.Single(state.History.Steps);
    }

    [Fact]
    public void ChangingTheToolDuringAPan_EndsIt()
    {
        var (state, canvas) = Create(2000, 1200);
        canvas.MiddlePressed(500, 400);
        Assert.True(canvas.IsPanning);

        state.ActiveTool = Tool.Rectangle;

        Assert.False(canvas.IsPanning);
        Assert.False(state.IsDragInProgress);
    }

    [Fact]
    public void ZoomTool_Click_ZoomsInOneStepAroundTheClickedPoint()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Zoom;
        var before = DocumentXUnder(canvas, 800);

        canvas.PointerPressed(800, 300);
        canvas.PointerReleased(800, 300);

        Assert.Equal(150, state.ZoomPercent);
        Assert.Equal(before, DocumentXUnder(canvas, 800), precision: 6);
        AssertNoEditing(state);
        Assert.False(state.IsDragInProgress);
    }

    [Fact]
    public void ZoomTool_AltClick_ZoomsOutOneStep()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Zoom;
        var before = DocumentXUnder(canvas, 800);

        canvas.PointerPressed(800, 300, alt: true);

        Assert.Equal(75, state.ZoomPercent);
        Assert.Equal(before, DocumentXUnder(canvas, 800), precision: 6);
    }

    [Fact]
    public void ZoomTool_AtTheLimits_ChangesNothing()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Zoom;
        state.SetZoom(ZoomLevels.Max);
        canvas.PointerPressed(800, 300);
        Assert.Equal(ZoomLevels.Max, state.ZoomPercent);

        state.SetZoom(ZoomLevels.Min);
        canvas.PointerPressed(800, 300, alt: true);
        Assert.Equal(ZoomLevels.Min, state.ZoomPercent);
    }

    [Fact]
    public void ZoomTool_Click_DoesNotSelectOrDrawAnything()
    {
        var (state, canvas) = Create(2000, 1200);
        var shape = new RectangleShape { X = 0, Y = 0, Width = 1000, Height = 700 };
        state.Edit(PressToShape.Core.History.HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, shape));
        state.Select(null);
        state.ActiveTool = Tool.Zoom;

        canvas.PointerPressed(canvas.OriginX + 100, canvas.OriginY + 100);
        canvas.PointerReleased(canvas.OriginX + 100, canvas.OriginY + 100);

        Assert.False(state.HasSelection);
        Assert.Single(state.History.Steps);
    }

    [Fact]
    public void CursorType_FollowsTheActiveTool()
    {
        var expected = new Dictionary<Tool, CanvasCursorType>
        {
            [Tool.Select] = CanvasCursorType.Arrow,
            [Tool.Rectangle] = CanvasCursorType.Crosshair,
            [Tool.Ellipse] = CanvasCursorType.Crosshair,
            [Tool.Hand] = CanvasCursorType.OpenHand,
            [Tool.Zoom] = CanvasCursorType.Magnifier,
            [Tool.Pen] = CanvasCursorType.Arrow,
        };
        var (state, canvas) = Create(2000, 1200);
        var raised = new List<string?>();
        canvas.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        foreach (var (tool, cursor) in expected.Where(kv => kv.Key != Tool.Select))
        {
            state.ActiveTool = tool;
            Assert.Equal(cursor, canvas.CursorType);
        }

        state.ActiveTool = Tool.Select;
        Assert.Equal(CanvasCursorType.Arrow, canvas.CursorType);
        Assert.Contains(nameof(CanvasAreaViewModel.CursorType), raised);
    }

    [Fact]
    public void CursorType_EyedropperToolOrAPick_IsTheEyedropper_AndRaisesPropertyChanged()
    {
        var (state, canvas) = Create(2000, 1200);
        var raised = new List<string?>();
        canvas.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        state.ActiveTool = Tool.Eyedropper;
        Assert.Equal(CanvasCursorType.Eyedropper, canvas.CursorType);

        state.ActiveTool = Tool.Select;
        Assert.Equal(CanvasCursorType.Arrow, canvas.CursorType);

        raised.Clear();
        state.BeginColorPick(ColorPickTarget.Fill);
        Assert.Equal(CanvasCursorType.Eyedropper, canvas.CursorType);
        Assert.Contains(nameof(CanvasAreaViewModel.CursorType), raised);

        raised.Clear();
        state.CancelColorPick();
        Assert.Equal(CanvasCursorType.Arrow, canvas.CursorType);
        Assert.Contains(nameof(CanvasAreaViewModel.CursorType), raised);
    }

    [Fact]
    public void CursorType_ZoomToolWithAlt_IsTheZoomOutMagnifier_AndBackWhenAltIsReleased()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Zoom;
        var raised = Record(canvas);

        canvas.IsAltHeld = true;
        Assert.Equal(CanvasCursorType.MagnifierMinus, canvas.CursorType);
        Assert.Contains(nameof(CanvasAreaViewModel.CursorType), raised);

        raised.Clear();
        canvas.IsAltHeld = false;
        Assert.Equal(CanvasCursorType.Magnifier, canvas.CursorType);
        Assert.Contains(nameof(CanvasAreaViewModel.CursorType), raised);
    }

    [Fact]
    public void IsAltHeld_SetToTheSameValue_RaisesNothing()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Zoom;
        canvas.IsAltHeld = true;
        var raised = Record(canvas);

        canvas.IsAltHeld = true;

        Assert.Empty(raised);
    }

    [Theory]
    [InlineData("Select", "Arrow")]
    [InlineData("Rectangle", "Crosshair")]
    [InlineData("Ellipse", "Crosshair")]
    [InlineData("Hand", "OpenHand")]
    public void CursorType_OtherToolsWithAlt_IgnoreAlt(string tool, string expected)
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Enum.Parse<Tool>(tool);

        canvas.IsAltHeld = true;

        Assert.Equal(Enum.Parse<CanvasCursorType>(expected), canvas.CursorType);
    }

    [Fact]
    public void CursorType_AHandleUnderThePointerWithAlt_IgnoresAlt()
    {
        var (_, canvas, _) = WithShapes();
        var (x, y) = ScreenOf(canvas, HandleType.Rotate);
        canvas.PointerHover(x, y);

        canvas.IsAltHeld = true;

        Assert.Equal(CanvasCursorType.Rotate, canvas.CursorType);
    }

    [Fact]
    public void CursorType_SelectingTheZoomToolWhileAltIsHeld_ShowsTheMinusAtOnce()
    {
        var (state, canvas) = Create(2000, 1200);
        canvas.IsAltHeld = true;

        state.ActiveTool = Tool.Zoom;

        Assert.Equal(CanvasCursorType.MagnifierMinus, canvas.CursorType);
    }

    [Fact]
    public void Pointer_MovePressAndRelease_TakeTheAltStateFromTheirArgument()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Zoom;

        canvas.PointerMoved(100, 100, alt: true);
        Assert.True(canvas.IsAltHeld);
        canvas.PointerMoved(100, 100, alt: false);
        Assert.False(canvas.IsAltHeld);

        canvas.PointerPressed(100, 100, alt: true);
        Assert.True(canvas.IsAltHeld);
        canvas.PointerReleased(100, 100, alt: false);
        Assert.False(canvas.IsAltHeld);
    }

    [Fact]
    public void Pointer_ReleaseAfterAPanWithAlt_KeepsAltHeld()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Hand;
        canvas.PointerPressed(500, 400, alt: true);

        canvas.PointerReleased(520, 410, alt: true);

        Assert.True(canvas.IsAltHeld);
    }

    [Fact]
    public void ZoomTool_AltHeld_StillZoomsOutOnClick()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Zoom;
        canvas.IsAltHeld = true;

        canvas.PointerPressed(800, 300, alt: true);

        Assert.Equal(75, state.ZoomPercent);
        Assert.Equal(CanvasCursorType.MagnifierMinus, canvas.CursorType);
    }

    [Fact]
    public void CursorType_DuringAPan_IsAClosedHand_AndBackAfterwards()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Hand;
        var raised = new List<string?>();
        canvas.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        canvas.PointerPressed(500, 400);
        Assert.Equal(CanvasCursorType.ClosedHand, canvas.CursorType);

        canvas.PointerReleased(500, 400);
        Assert.Equal(CanvasCursorType.OpenHand, canvas.CursorType);
        Assert.Contains(nameof(CanvasAreaViewModel.CursorType), raised);
    }

    [Fact]
    public void Drawing_AfterPanningAndZooming_StillLandsUnderThePointer()
    {
        var (state, canvas) = Create(2000, 1200);
        state.ActiveTool = Tool.Hand;
        canvas.PointerPressed(500, 400);
        canvas.PointerReleased(560, 330);
        canvas.Wheel(0, 2, control: true, shift: false, 900, 500);
        state.ActiveTool = Tool.Rectangle;
        var (ox, oy, zoom) = (canvas.OriginX, canvas.OriginY, canvas.ZoomPercent);

        canvas.PointerPressed(ox + 40, oy + 20);
        canvas.PointerReleased(ox + 240, oy + 220);

        var shape = Assert.Single(state.Document.Layers.SelectMany(l => l.Shapes));
        Assert.Equal(40 / (zoom / 100), shape.X, precision: 6);
        Assert.Equal(200 / (zoom / 100), shape.Width, precision: 6);
    }

    // ---- scroll bars ----

    [Fact]
    public void ScrollRange_IsTheAllowedOffsetRange_AndFollowsZoomViewportAndArtboard()
    {
        var (state, canvas) = Create(2000, 1200);

        Assert.Equal(ViewGeometry.PanLimit(2000, 1200), canvas.HorizontalScrollMaximum);
        Assert.Equal(-canvas.HorizontalScrollMaximum, canvas.HorizontalScrollMinimum);
        Assert.Equal(ViewGeometry.PanLimit(1200, 800), canvas.VerticalScrollMaximum);
        Assert.Equal(2000, canvas.HorizontalScrollViewportSize);
        Assert.Equal(1200, canvas.VerticalScrollViewportSize);
        Assert.True(canvas.CanScrollHorizontally);
        Assert.True(canvas.CanScrollVertically);

        state.SetZoom(400);
        Assert.Equal(ViewGeometry.PanLimit(2000, 4800), canvas.HorizontalScrollMaximum);

        canvas.ViewportWidth = 900;
        Assert.Equal(ViewGeometry.PanLimit(900, 4800), canvas.HorizontalScrollMaximum);
        Assert.Equal(900, canvas.HorizontalScrollViewportSize);

        state.Document = state.Document.WithArtboardSize(300, 800);
        Assert.Equal(ViewGeometry.PanLimit(900, 300 * 4), canvas.HorizontalScrollMaximum);
    }

    [Fact]
    public void ScrollValue_IsTheNegatedOffset_AndSettingItPansTheOtherWay()
    {
        var (state, canvas) = Create(2000, 1200);

        state.SetPan(120, -60);
        Assert.Equal(-120, canvas.HorizontalScrollValue);
        Assert.Equal(60, canvas.VerticalScrollValue);

        canvas.HorizontalScrollValue = 200; // the thumb moves right: the view moves right, the artboard moves left
        canvas.VerticalScrollValue = -30;

        Assert.Equal((-200, 30), (state.PanX, state.PanY));
        Assert.Equal(canvas.OriginX, ViewGeometry.Origin(2000, 1200, -200));
    }

    [Fact]
    public void ScrollValue_SetOutsideTheRange_IsLimited()
    {
        var (state, canvas) = Create(2000, 1200);

        canvas.HorizontalScrollValue = 1e9;
        canvas.VerticalScrollValue = -1e9;

        Assert.Equal(canvas.HorizontalScrollMaximum, canvas.HorizontalScrollValue);
        Assert.Equal(canvas.VerticalScrollMinimum, canvas.VerticalScrollValue);
        Assert.Equal(-canvas.HorizontalScrollMaximum, state.PanX);
    }

    [Fact]
    public void ScrollValue_AtTheOffsetLimit_IsAtTheEndOfTheRange()
    {
        var (state, canvas) = Create(2000, 1200);

        state.SetPan(1e9, 1e9);

        Assert.Equal(canvas.HorizontalScrollMinimum, canvas.HorizontalScrollValue);
        Assert.Equal(canvas.VerticalScrollMinimum, canvas.VerticalScrollValue);
    }

    [Fact]
    public void ScrollBars_WithoutACanvasSize_CannotScroll()
    {
        var (_, canvas) = Create(0, 0);

        Assert.False(canvas.CanScrollHorizontally);
        Assert.False(canvas.CanScrollVertically);
        Assert.Equal(0, canvas.HorizontalScrollMaximum);
        Assert.Equal(0, canvas.VerticalScrollMinimum);
    }

    [Fact]
    public void ScrollProperties_RaiseNotificationsForWheelDragZoomAndResize()
    {
        var names = new[]
        {
            nameof(CanvasAreaViewModel.HorizontalScrollValue), nameof(CanvasAreaViewModel.VerticalScrollValue),
            nameof(CanvasAreaViewModel.HorizontalScrollMaximum), nameof(CanvasAreaViewModel.VerticalScrollMaximum),
            nameof(CanvasAreaViewModel.HorizontalScrollViewportSize), nameof(CanvasAreaViewModel.CanScrollHorizontally),
        };
        var (state, canvas) = Create(2000, 1200);
        var raised = new List<string?>();
        canvas.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        canvas.Wheel(0, 1, control: false, shift: false, 500, 500);
        Assert.Contains(nameof(CanvasAreaViewModel.VerticalScrollValue), raised);

        raised.Clear();
        state.ActiveTool = Tool.Hand;
        canvas.PointerPressed(100, 100);
        canvas.PointerMoved(150, 120);
        canvas.PointerReleased(150, 120);
        Assert.Contains(nameof(CanvasAreaViewModel.HorizontalScrollValue), raised);

        raised.Clear();
        canvas.Wheel(0, 1, control: true, shift: false, 500, 500);
        Assert.All(names, n => Assert.Contains(n, raised));

        raised.Clear();
        canvas.ViewportWidth = 1500;
        Assert.All(names, n => Assert.Contains(n, raised));
    }

    [Fact]
    public void ScrollValue_SetToTheCurrentValue_ChangesNothing()
    {
        var (state, canvas) = Create(2000, 1200);
        state.SetPan(40, 50);
        var raised = new List<string?>();
        state.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        canvas.HorizontalScrollValue = canvas.HorizontalScrollValue;
        canvas.VerticalScrollValue = canvas.VerticalScrollValue;

        Assert.Empty(raised);
    }

    [Fact]
    public void ScrollBars_NeverCreateHistoryOrDirtyState()
    {
        var (state, canvas) = Create(2000, 1200);
        var document = state.Document;

        canvas.HorizontalScrollValue = 100;
        canvas.VerticalScrollValue = -100;

        Assert.Same(document, state.Document);
        AssertNoEditing(state);
    }

    // ---- selection overlay, modifiers, hover, cursors, keys ----

    private static (EditorState State, CanvasAreaViewModel Canvas, Guid[] Ids) WithShapes(int count = 1, bool rotated = false)
    {
        var (state, canvas) = Create(2000, 1200);
        var shapes = Enumerable.Range(0, count)
            .Select(i => (Shape)new RectangleShape { X = 100 + i * 300, Y = 100, Width = 200, Height = 100, Rotation = rotated ? 90 : 0 })
            .ToArray();
        state.Edit(HistoryStepType.DrawRectangle, d => shapes.Aggregate(d, (doc, sh) => doc.AddShape(state.ActiveLayerId, sh)));
        state.SetSelection(shapes.Select(sh => sh.Id));
        return (state, canvas, shapes.Select(sh => sh.Id).ToArray());
    }

    private static (double X, double Y) ScreenOf(CanvasAreaViewModel canvas, HandleType handle) =>
        (canvas.Overlay.Handles.Single(h => h.Type == handle).Center.X, canvas.Overlay.Handles.Single(h => h.Type == handle).Center.Y);

    [Fact]
    public void Overlay_OneShape_HasOutlineFrameAndHandles_AndFollowsZoomAndOrigin()
    {
        var (state, canvas, _) = WithShapes();

        Assert.Single(canvas.Overlay.Outlines);
        Assert.Equal(9, canvas.Overlay.Handles.Count);
        Assert.Equal(canvas.OriginX + 100, canvas.Overlay.Outlines[0].Points[0].X, 6);

        state.SetZoom(50);

        Assert.Equal(canvas.OriginX + 50, canvas.Overlay.Outlines[0].Points[0].X, 6);
    }

    [Fact]
    public void Overlay_SeveralOrNoShapesOrOtherTools()
    {
        var (state, canvas, _) = WithShapes(3);

        Assert.Equal(3, canvas.Overlay.Outlines.Count);
        Assert.Equal(9, canvas.Overlay.Handles.Count);

        state.ActiveTool = Tool.Rectangle;
        Assert.Equal(3, canvas.Overlay.Outlines.Count);
        Assert.Empty(canvas.Overlay.Handles);

        state.ActiveTool = Tool.Select;
        state.ClearSelection();
        Assert.Empty(canvas.Overlay.Outlines);
    }

    [Fact]
    public void Overlay_Notifies_OnSelectionToolSelectionBoxViewportAndDocumentChange()
    {
        var (state, canvas, ids) = WithShapes(2);
        var raised = Record(canvas);

        state.SetSelection(ids.Take(1));
        Assert.Contains(nameof(CanvasAreaViewModel.Overlay), raised);

        raised.Clear();
        state.ActiveTool = Tool.Hand;
        Assert.Contains(nameof(CanvasAreaViewModel.Overlay), raised);

        raised.Clear();
        state.ActiveTool = Tool.Select;
        state.SelectionBox = new SelectionBox(0, 0, 10, 10);
        Assert.Contains(nameof(CanvasAreaViewModel.Overlay), raised);
        Assert.NotNull(canvas.Overlay.SelectionBox);

        raised.Clear();
        canvas.ViewportWidth = 1500;
        Assert.Contains(nameof(CanvasAreaViewModel.Overlay), raised);

        raised.Clear();
        state.Edit(HistoryStepType.MoveShape, d => d.UpdateShape(ids[0], sh => sh with { X = 5 }));
        Assert.Contains(nameof(CanvasAreaViewModel.Overlay), raised);
    }

    [Fact]
    public void Modifiers_ReachTheInteraction_ShiftAddsToTheSelection_AndShiftKeepsTheRatio()
    {
        var (state, canvas, ids) = WithShapes(2);
        state.SetSelection(ids.Take(1));

        // Shift+click on the second shape adds it.
        canvas.PointerPressed(canvas.OriginX + 450, canvas.OriginY + 150, shift: true);
        canvas.PointerReleased(canvas.OriginX + 450, canvas.OriginY + 150, shift: true);
        Assert.Equal(ids, state.SelectedShapeIds);

        // Alt on a handle drag resizes around the center.
        state.SetSelection(ids.Take(1));
        var (hx, hy) = ScreenOf(canvas, HandleType.BottomRight);
        canvas.PointerPressed(hx, hy);
        canvas.PointerMoved(hx + 40, hy + 20, alt: true);
        canvas.PointerReleased(hx + 40, hy + 20, alt: true);
        var shape = state.Document.FindShape(ids[0])!.Shape;
        Assert.Equal(200, shape.CenterX, 6);
        Assert.Equal(280, shape.Width, 6);
    }

    [Fact]
    public void Hover_OverEachHandle_SetsTheMatchingCursor_AndAwayResetsIt()
    {
        var (_, canvas, _) = WithShapes();
        var expected = new (HandleType Handle, CanvasCursorType Cursor)[]
        {
            (HandleType.Top, CanvasCursorType.ResizeNorthSouth),
            (HandleType.Bottom, CanvasCursorType.ResizeNorthSouth),
            (HandleType.Left, CanvasCursorType.ResizeWestEast),
            (HandleType.Right, CanvasCursorType.ResizeWestEast),
            (HandleType.TopLeft, CanvasCursorType.ResizeNorthWestSouthEast),
            (HandleType.BottomRight, CanvasCursorType.ResizeNorthWestSouthEast),
            (HandleType.TopRight, CanvasCursorType.ResizeNorthEastSouthWest),
            (HandleType.BottomLeft, CanvasCursorType.ResizeNorthEastSouthWest),
            (HandleType.Rotate, CanvasCursorType.Rotate),
        };
        var raised = Record(canvas);

        foreach (var (handle, cursor) in expected)
        {
            var (x, y) = ScreenOf(canvas, handle);
            canvas.PointerHover(x, y);
            Assert.Equal(cursor, canvas.CursorType);
        }

        Assert.Contains(nameof(CanvasAreaViewModel.CursorType), raised);
        canvas.PointerHover(5, 5);
        Assert.Equal(CanvasCursorType.Arrow, canvas.CursorType);
    }

    [Fact]
    public void Hover_OverARotatedShapesHandle_TurnsTheResizeCursor()
    {
        var (_, canvas, _) = WithShapes(rotated: true);

        // Turned by 90 degrees the Right handle points down.
        var (x, y) = ScreenOf(canvas, HandleType.Right);
        canvas.PointerHover(x, y);
        Assert.Equal(CanvasCursorType.ResizeNorthSouth, canvas.CursorType);

        var (cx, cy) = ScreenOf(canvas, HandleType.TopRight);
        canvas.PointerHover(cx, cy);
        Assert.Equal(CanvasCursorType.ResizeNorthWestSouthEast, canvas.CursorType); // the corner that pointed up-right now points down-right
    }

    [Fact]
    public void Cursor_DuringAHandleDrag_StaysTheHandlesCursor_EvenAwayFromIt()
    {
        var (_, canvas, _) = WithShapes();
        var (hx, hy) = ScreenOf(canvas, HandleType.Right);

        canvas.PointerPressed(hx, hy);
        canvas.PointerMoved(hx + 200, hy + 200);

        Assert.Equal(CanvasCursorType.ResizeWestEast, canvas.CursorType);
        canvas.PointerReleased(hx + 200, hy + 200);
        Assert.Equal(CanvasCursorType.Arrow, canvas.CursorType);
    }

    [Fact]
    public void Cursor_OtherToolsNeverShowHandleCursors()
    {
        var (state, canvas, _) = WithShapes();
        var (x, y) = ScreenOf(canvas, HandleType.Right);
        state.ActiveTool = Tool.Rectangle;

        canvas.PointerHover(x, y);

        Assert.Equal(CanvasCursorType.Crosshair, canvas.CursorType);
    }

    [Fact]
    public void PointerLeft_ClearsTheHover()
    {
        var (_, canvas, _) = WithShapes();
        var (x, y) = ScreenOf(canvas, HandleType.Right);
        canvas.PointerHover(x, y);

        canvas.PointerLeft();

        Assert.Equal(CanvasCursorType.Arrow, canvas.CursorType);
    }

    [Fact]
    public void ArrowKeys_NudgeByOne_ShiftByTen_AndRepeatedKeysMergeIntoOneStep()
    {
        var (state, canvas, ids) = WithShapes();
        var steps = state.History.Steps.Count;

        Assert.True(canvas.KeyPressed(CanvasKey.Right, shift: false));
        Assert.True(canvas.KeyPressed(CanvasKey.Right, shift: false));
        Assert.True(canvas.KeyPressed(CanvasKey.Down, shift: true));
        Assert.True(canvas.KeyPressed(CanvasKey.Left, shift: false));
        Assert.True(canvas.KeyPressed(CanvasKey.Up, shift: true));

        var shape = state.Document.FindShape(ids[0])!.Shape;
        Assert.Equal((101, 100), (shape.X, shape.Y));
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.NudgeShapes, state.History.Steps[^1]);
    }

    [Fact]
    public void Escape_ClearsTheSelection()
    {
        var (state, canvas, _) = WithShapes();

        Assert.True(canvas.KeyPressed(CanvasKey.Escape, shift: false));

        Assert.False(state.HasSelection);
    }

    [Fact]
    public void Keys_DoNothingWithoutASelectionOrDuringADrag()
    {
        var (state, canvas, ids) = WithShapes();
        var (hx, hy) = ScreenOf(canvas, HandleType.Right);
        canvas.PointerPressed(hx, hy);

        Assert.False(canvas.KeyPressed(CanvasKey.Right, shift: false));
        canvas.Cancel();
        state.ClearSelection();
        Assert.False(canvas.KeyPressed(CanvasKey.Right, shift: false));
        Assert.False(canvas.KeyPressed(CanvasKey.Escape, shift: false));
        Assert.Equal(100, state.Document.FindShape(ids[0])!.Shape.X);
    }

    [Fact]
    public void DeleteSelection_OfSeveralShapes_RemovesAllOfThem()
    {
        var (state, canvas, _) = WithShapes(3);

        canvas.DeleteSelection();

        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
        Assert.Empty(canvas.Overlay.Outlines);
    }

    [Fact]
    public void SharedEditor_IsUsedForKeys()
    {
        var state = new EditorState();
        var editor = new SelectionEditor(state);
        var canvas = new CanvasAreaViewModel(state, editor) { ViewportWidth = 1000, ViewportHeight = 800 };
        var ids = TestSelection.AddAndSelect(state);

        canvas.KeyPressed(CanvasKey.Right, shift: false);
        Assert.Equal(1, state.Document.FindShape(ids[0])!.Shape.X);
    }
}

