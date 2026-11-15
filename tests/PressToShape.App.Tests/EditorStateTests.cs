using System;
using System.Collections.Generic;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class EditorStateTests
{
    [Fact]
    public void NewState_HasExpectedDefaults()
    {
        var state = new EditorState();

        Assert.Equal(Tool.Select, state.ActiveTool);
        Assert.Equal(100, state.ZoomPercent);
        Assert.False(state.HasSelection);
        Assert.Equal(string.Empty, state.SelectionSummary);
        Assert.Equal(1200, state.ArtboardWidth);
        Assert.Equal(800, state.ArtboardHeight);
    }

    [Fact]
    public void SetZoom_OutsideRange_IsClamped()
    {
        var state = new EditorState();

        state.SetZoom(1);
        Assert.Equal(ZoomLevels.Min, state.ZoomPercent);

        state.SetZoom(99999);
        Assert.Equal(ZoomLevels.Max, state.ZoomPercent);
    }

    [Fact]
    public void ZoomIn_AndZoomOut_StepThroughPresets()
    {
        var state = new EditorState();

        state.ZoomIn();
        Assert.Equal(150, state.ZoomPercent);

        state.ZoomOut();
        state.ZoomOut();
        Assert.Equal(75, state.ZoomPercent);
    }

    [Fact]
    public void ZoomIn_AtMaximum_StaysAtMaximum()
    {
        var state = new EditorState();
        state.SetZoom(ZoomLevels.Max);

        state.ZoomIn();

        Assert.Equal(ZoomLevels.Max, state.ZoomPercent);
    }

    [Fact]
    public void ZoomToActualSize_Sets100()
    {
        var state = new EditorState();
        state.SetZoom(400);

        state.ZoomToActualSize();

        Assert.Equal(100, state.ZoomPercent);
    }

    [Fact]
    public void ZoomToFit_UsesArtboardAndViewport()
    {
        var state = new EditorState();

        state.ZoomToFit(1248, 848); // available 1200 x 800 -> exactly the artboard size

        Assert.Equal(100, state.ZoomPercent, precision: 6);
    }

    [Fact]
    public void ZoomToFit_WithoutViewportSize_DoesNothing()
    {
        var state = new EditorState();
        state.SetZoom(200);

        state.ZoomToFit(0, 0);

        Assert.Equal(200, state.ZoomPercent);
    }

    [Fact]
    public void Changes_RaisePropertyChanged()
    {
        var state = new EditorState();
        var changed = new List<string?>();
        state.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.ActiveTool = Tool.Pen;
        state.SetZoom(200);
        state.HasSelection = true;
        state.SelectionSummary = "1 selected";

        Assert.Contains(nameof(EditorState.ActiveTool), changed);
        Assert.Contains(nameof(EditorState.ZoomPercent), changed);
        Assert.Contains(nameof(EditorState.HasSelection), changed);
        Assert.Contains(nameof(EditorState.SelectionSummary), changed);
    }

    [Fact]
    public void SetZoom_ToSameValue_DoesNotRaisePropertyChanged()
    {
        var state = new EditorState();
        var count = 0;
        state.PropertyChanged += (_, _) => count++;

        state.SetZoom(100);

        Assert.Equal(0, count);
    }

    [Fact]
    public void NewState_WithoutDocument_HasAnEmptyDefaultDocument()
    {
        var state = new EditorState();

        Assert.Single(state.Document.Layers);
        Assert.Empty(state.Document.Layers[0].Shapes);
    }

    [Fact]
    public void NewState_WithDocument_KeepsThatDocument()
    {
        var document = Document.Create("Start");

        var state = new EditorState(document);

        Assert.Same(document, state.Document);
    }

    [Fact]
    public void Document_Replaced_RaisesDocumentAndArtboardNotifications()
    {
        var state = new EditorState();
        var changed = new List<string?>();
        state.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.Document = state.Document.WithArtboardSize(500, 300);

        Assert.Contains(nameof(EditorState.Document), changed);
        Assert.Contains(nameof(EditorState.ArtboardWidth), changed);
        Assert.Contains(nameof(EditorState.ArtboardHeight), changed);
    }

    [Fact]
    public void ArtboardSize_FollowsTheDocument()
    {
        var state = new EditorState();

        state.Document = state.Document.WithArtboardSize(500, 300);

        Assert.Equal(500, state.ArtboardWidth);
        Assert.Equal(300, state.ArtboardHeight);
    }

    [Fact]
    public void Zoom_Changed_LeavesTheDocumentUntouched()
    {
        var state = new EditorState();
        var document = state.Document;
        var changed = new List<string?>();
        state.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.SetZoom(300);
        state.ZoomToFit(1000, 700);

        Assert.Same(document, state.Document);
        Assert.DoesNotContain(nameof(EditorState.Document), changed);
    }

    [Fact]
    public void UpdateLayer_ChangesTheLayerAndKeepsTheOldDocumentUnchanged()
    {
        var state = new EditorState(Document.Create("Layer"));
        var before = state.Document;
        var layerId = before.Layers[0].Id;

        state.UpdateLayer(layerId, l => l with { IsVisible = false }, HistoryStepType.HideLayer);

        Assert.False(state.Document.Layers[0].IsVisible);
        Assert.True(before.Layers[0].IsVisible);
        Assert.NotSame(before, state.Document);
    }

    [Fact]
    public void UpdateLayer_UnknownLayer_ThrowsAndKeepsTheDocument()
    {
        var state = new EditorState();
        var before = state.Document;

        Assert.Throws<KeyNotFoundException>(() => state.UpdateLayer(Guid.NewGuid(), l => l, HistoryStepType.HideLayer));

        Assert.Same(before, state.Document);
    }

    [Fact]
    public void ZoomToFit_UsesTheArtboardOfTheDocument()
    {
        var state = new EditorState();
        state.Document = state.Document.WithArtboardSize(600, 400);

        state.ZoomToFit(648, 448); // available 600 x 400 -> exactly the artboard size

        Assert.Equal(100, state.ZoomPercent, precision: 6);
    }

    private static (EditorState State, Guid Bottom, Guid Top, Guid ShapeId) TwoLayerState()
    {
        var shape = new RectangleShape { Width = 10, Height = 10 };
        var document = Document.Create("Bottom");
        var bottom = document.Layers[0].Id;
        document = document.AddLayer(new Layer { Name = "Top" });
        var top = document.Layers[1].Id;
        document = document.AddShape(bottom, shape);
        return (new EditorState(document), bottom, top, shape.Id);
    }

    [Fact]
    public void NewState_ActiveLayerIsTheTopmost()
    {
        var (state, _, top, _) = TwoLayerState();

        Assert.Equal(top, state.ActiveLayerId);
    }

    [Fact]
    public void NewState_HasNoSelectionAndNoDraft()
    {
        var state = new EditorState();

        Assert.Null(state.SelectedShapeId);
        Assert.Null(state.Draft);
    }

    [Fact]
    public void Document_ActiveLayerRemoved_FallsBackToTheTopmostLayer()
    {
        var (state, bottom, top, _) = TwoLayerState();

        state.Document = state.Document.RemoveLayer(top);

        Assert.Equal(bottom, state.ActiveLayerId);
    }

    [Fact]
    public void Document_OtherLayerRemoved_KeepsTheActiveLayer()
    {
        var (state, bottom, top, _) = TwoLayerState();
        state.ActiveLayerId = bottom;

        state.Document = state.Document.RemoveLayer(top);
        Assert.Equal(bottom, state.ActiveLayerId);

        var (state2, bottom2, top2, _) = TwoLayerState();
        state2.Document = state2.Document.RemoveLayer(bottom2);
        Assert.Equal(top2, state2.ActiveLayerId);
    }

    [Fact]
    public void Select_Shape_SetsSelectionAndHasSelection()
    {
        var (state, _, _, shapeId) = TwoLayerState();

        state.Select(shapeId);

        Assert.Equal(shapeId, state.SelectedShapeId);
        Assert.True(state.HasSelection);
        Assert.Equal(string.Empty, state.SelectionSummary);
    }

    [Fact]
    public void Select_Null_ClearsSelection()
    {
        var (state, _, _, shapeId) = TwoLayerState();
        state.Select(shapeId);

        state.Select(null);

        Assert.Null(state.SelectedShapeId);
        Assert.False(state.HasSelection);
    }

    [Fact]
    public void Document_UnrelatedEdit_KeepsTheSelection()
    {
        var (state, _, top, shapeId) = TwoLayerState();
        state.Select(shapeId);

        state.Document = state.Document.UpdateLayer(top, l => l with { IsLocked = true });

        Assert.Equal(shapeId, state.SelectedShapeId);
        Assert.True(state.HasSelection);
    }

    [Fact]
    public void Document_SelectedShapeRemoved_ClearsTheSelection()
    {
        var (state, _, _, shapeId) = TwoLayerState();
        state.Select(shapeId);

        state.Document = state.Document.RemoveShape(shapeId);

        Assert.Null(state.SelectedShapeId);
        Assert.False(state.HasSelection);
    }

    [Fact]
    public void Document_LayerOfSelectedShapeRemoved_ClearsTheSelection()
    {
        var (state, bottom, _, shapeId) = TwoLayerState();
        state.Select(shapeId);

        state.Document = state.Document.RemoveLayer(bottom);

        Assert.Null(state.SelectedShapeId);
        Assert.False(state.HasSelection);
    }

    [Fact]
    public void Draft_Set_RaisesPropertyChanged()
    {
        var state = new EditorState();
        var raised = new List<string?>();
        state.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        state.Draft = new RectangleShape { Width = 1, Height = 1 };

        Assert.Contains(nameof(EditorState.Draft), raised);
    }

    private static RectangleShape Rect() => new() { Width = 5, Height = 5 };

    [Fact]
    public void Edit_RecordsAStepAndSetsTheDocument()
    {
        var state = new EditorState(Document.Create("L"));
        var before = state.Document;

        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, Rect()));

        Assert.NotSame(before, state.Document);
        Assert.Same(state.Document, state.History.Current);
        Assert.Equal(new[] { HistoryStepType.DrawRectangle }, state.History.Steps);
        Assert.True(state.CanUndo);
        Assert.False(state.CanRedo);
    }

    [Fact]
    public void Edit_ReturningTheSameInstance_RecordsNothing()
    {
        var state = new EditorState(Document.Create("L"));
        var raised = 0;
        state.HistoryChanged += (_, _) => raised++;

        state.Edit(HistoryStepType.MoveShape, d => d);

        Assert.Empty(state.History.Steps);
        Assert.Equal(0, raised);
        Assert.False(state.CanUndo);
    }

    [Fact]
    public void Edit_WhenTheChangeThrows_ChangesNothing()
    {
        var state = new EditorState(Document.Create("L"));
        var before = state.Document;

        Assert.Throws<KeyNotFoundException>(() => state.Edit(HistoryStepType.DeleteShape, d => d.RemoveShape(Guid.NewGuid())));

        Assert.Same(before, state.Document);
        Assert.Empty(state.History.Steps);
    }

    [Fact]
    public void PreviewThenCommit_RecordsOneStep()
    {
        var state = new EditorState(Document.Create("L"));
        var layer = state.ActiveLayerId;
        var shape = Rect();
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(layer, shape));

        for (var i = 1; i <= 20; i++)
        {
            state.Preview(state.Document.UpdateShape(shape.Id, s => s is RectangleShape r ? r with { X = i } : s));
        }

        Assert.Single(state.History.Steps);
        state.Commit(HistoryStepType.MoveShape);

        Assert.Equal(new[] { HistoryStepType.DrawRectangle, HistoryStepType.MoveShape }, state.History.Steps);
        Assert.Same(state.Document, state.History.Current);
    }

    [Fact]
    public void Commit_WithoutAChange_RecordsNothing()
    {
        var state = new EditorState(Document.Create("L"));
        var raised = 0;
        state.HistoryChanged += (_, _) => raised++;

        state.Commit(HistoryStepType.MoveShape);

        Assert.Empty(state.History.Steps);
        Assert.Equal(0, raised);
    }

    [Fact]
    public void UndoRedoAndJump_SetTheDocumentAndRaiseTheNotifications()
    {
        var state = new EditorState(Document.Create("L"));
        var layer = state.ActiveLayerId;
        var d0 = state.Document;
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(layer, Rect()));
        var d1 = state.Document;
        state.Edit(HistoryStepType.DrawEllipse, d => d.AddShape(layer, new EllipseShape { Width = 3, Height = 3 }));
        var d2 = state.Document;
        var raised = new List<string?>();
        state.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        var historyEvents = 0;
        state.HistoryChanged += (_, _) => historyEvents++;

        state.Undo();
        Assert.Same(d1, state.Document);
        Assert.True(state.CanRedo);

        state.Undo();
        Assert.Same(d0, state.Document);
        Assert.False(state.CanUndo);

        state.Redo();
        Assert.Same(d1, state.Document);

        state.JumpToStep(2);
        Assert.Same(d2, state.Document);

        state.JumpToStep(0);
        Assert.Same(d0, state.Document);

        Assert.Equal(5, historyEvents);
        Assert.Contains(nameof(EditorState.CanUndo), raised);
        Assert.Contains(nameof(EditorState.CanRedo), raised);
        Assert.Contains(nameof(EditorState.Document), raised);
    }

    [Fact]
    public void UndoRedoAndJump_WithNothingToDo_DoNothing()
    {
        var state = new EditorState(Document.Create("L"));
        var before = state.Document;
        var raised = 0;
        state.HistoryChanged += (_, _) => raised++;

        state.Undo();
        state.Redo();
        state.JumpToStep(0);
        state.JumpToStep(9);
        state.JumpToStep(-1);

        Assert.Same(before, state.Document);
        Assert.Equal(0, raised);
    }

    [Fact]
    public void Undo_KeepsTheSelectionWhileTheShapeExists_AndClearsItOtherwise()
    {
        var state = new EditorState(Document.Create("L"));
        var layer = state.ActiveLayerId;
        var first = Rect();
        var second = Rect();
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(layer, first));
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(layer, second));

        state.Select(first.Id);
        state.Undo();
        Assert.Equal(first.Id, state.SelectedShapeId);
        Assert.True(state.HasSelection);

        state.Undo();
        Assert.Null(state.SelectedShapeId);
        Assert.False(state.HasSelection);
    }

    [Fact]
    public void Undo_OfADeletedShape_BringsItBackWithoutSelection()
    {
        var state = new EditorState(Document.Create("L"));
        var shape = Rect();
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, shape));
        state.Select(shape.Id);
        state.Edit(HistoryStepType.DeleteShape, d => d.RemoveShape(shape.Id));
        state.Select(null);

        state.Undo();

        Assert.NotNull(state.Document.FindShape(shape.Id));
        Assert.Null(state.SelectedShapeId);
    }

    [Fact]
    public void CanUndoAndCanRedo_FollowTheDragFlag()
    {
        var state = new EditorState(Document.Create("L"));
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, Rect()));
        state.Undo();
        state.Redo();
        state.Undo();
        var raised = new List<string?>();
        state.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        Assert.False(state.CanUndo);
        Assert.True(state.CanRedo);

        state.Redo();
        Assert.True(state.CanUndo);

        state.IsDragInProgress = true;
        Assert.False(state.CanUndo);
        Assert.False(state.CanRedo);
        Assert.Contains(nameof(EditorState.CanUndo), raised);

        var document = state.Document;
        state.Undo();
        state.Redo();
        state.JumpToStep(0);
        Assert.Same(document, state.Document);

        state.IsDragInProgress = false;
        Assert.True(state.CanUndo);
    }

    [Fact]
    public void ReplaceDocument_ResetsTheHistoryAndShowsTheDocument()
    {
        var state = new EditorState(Document.Create("L"));
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, Rect()));
        var other = Document.Create("Other");
        var raised = 0;
        state.HistoryChanged += (_, _) => raised++;

        state.ReplaceDocument(other);

        Assert.Same(other, state.Document);
        Assert.Same(other, state.History.Current);
        Assert.Empty(state.History.Steps);
        Assert.False(state.CanUndo);
        Assert.Equal(1, raised);
        Assert.Equal(other.Layers[0].Id, state.ActiveLayerId);
    }

    [Fact]
    public void UndoBeyondTheLimit_IsNotPossible()
    {
        var state = new EditorState(Document.Create("L"));
        for (var i = 0; i < 130; i++)
        {
            state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, Rect()));
        }

        var undone = 0;
        while (state.CanUndo)
        {
            state.Undo();
            undone++;
        }

        Assert.Equal(100, undone);
        Assert.Equal(30, state.Document.Layers[0].Shapes.Length);
    }

    [Fact]
    public void Arguments_Invalid_Throw()
    {
        var state = new EditorState(Document.Create("L"));

        Assert.Throws<ArgumentNullException>(() => state.Edit(HistoryStepType.MoveShape, null!));
        Assert.Throws<ArgumentNullException>(() => state.Preview(null!));
        Assert.Throws<ArgumentNullException>(() => state.ReplaceDocument(null!));
    }

    // ---- view: pan, viewport and zoom around a point ----

    private static EditorState WithViewport(double width = 1000, double height = 700)
    {
        var state = new EditorState(Document.Create("L"));
        state.SetViewport(width, height);
        return state;
    }

    private static double DocumentPointUnder(EditorState state, double screenX)
    {
        var origin = ViewGeometry.Origin(state.ViewportWidth, state.ArtboardWidth * state.ZoomPercent / 100, state.PanX);
        return CanvasMapping.ToDocument(screenX, origin, state.ZoomPercent);
    }

    [Fact]
    public void NewState_HasNoOffsetAndNoViewport()
    {
        var state = new EditorState();

        Assert.Equal(0, state.PanX);
        Assert.Equal(0, state.PanY);
        Assert.Equal(0, state.ViewportWidth);
        Assert.Equal(0, state.ViewportHeight);
    }

    [Fact]
    public void SetPan_StoresTheOffset_AndNotifies()
    {
        var state = WithViewport();
        var raised = new List<string?>();
        state.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        state.SetPan(120, -80);

        Assert.Equal((120, -80), (state.PanX, state.PanY));
        Assert.Contains(nameof(EditorState.PanX), raised);
        Assert.Contains(nameof(EditorState.PanY), raised);
    }

    [Fact]
    public void SetPan_BeyondTheLimit_IsLimited()
    {
        var state = WithViewport(1000, 700);

        state.SetPan(100000, -100000);

        Assert.Equal(ViewGeometry.PanLimit(1000, 1200), state.PanX);
        Assert.Equal(-ViewGeometry.PanLimit(700, 800), state.PanY);
    }

    [Fact]
    public void SetPan_NonFiniteValues_LeaveThatAxisUnchanged()
    {
        var state = WithViewport();
        state.SetPan(50, 60);

        state.SetPan(double.NaN, 70);
        Assert.Equal((50, 70), (state.PanX, state.PanY));

        state.SetPan(double.PositiveInfinity, double.NegativeInfinity);
        Assert.Equal((50, 70), (state.PanX, state.PanY));
    }

    [Fact]
    public void PanBy_AddsTheDistance_AndIgnoresNonFiniteDistances()
    {
        var state = WithViewport();

        state.PanBy(30, 40);
        state.PanBy(-10, double.NaN);

        Assert.Equal((20, 40), (state.PanX, state.PanY));
    }

    [Fact]
    public void SetViewport_SmallerThanTheOffsetAllows_LimitsTheOffsetAgain()
    {
        var state = WithViewport(2000, 1500);
        state.SetPan(1400, 0);
        Assert.Equal(1400, state.PanX);

        state.SetViewport(400, 1500);

        Assert.Equal(ViewGeometry.PanLimit(400, 1200), state.PanX);
    }

    [Fact]
    public void SetViewport_NonFiniteOrNegative_BecomesUnknown()
    {
        var state = WithViewport();

        state.SetViewport(double.NaN, -5);

        Assert.Equal((0, 0), (state.ViewportWidth, state.ViewportHeight));
    }

    [Fact]
    public void ZoomAround_KeepsTheDocumentPointUnderTheAnchor()
    {
        var state = WithViewport();
        state.SetPan(90, 0);
        var before = DocumentPointUnder(state, 300);

        state.ZoomAround(250, 300, 200);

        Assert.Equal(250, state.ZoomPercent);
        Assert.Equal(before, DocumentPointUnder(state, 300), precision: 6);
    }

    [Theory]
    [InlineData(150)]
    [InlineData(50)]
    [InlineData(800)]
    public void CenterAnchoredRoutes_KeepTheCenterOfTheCanvasFixed(double target)
    {
        var routes = new Action<EditorState>[]
        {
            s => s.SetZoom(target),
            s => s.ZoomPercent = target,
            s => s.ZoomAround(target, s.ViewportWidth / 2, s.ViewportHeight / 2),
        };

        foreach (var route in routes)
        {
            var state = WithViewport();
            state.SetPan(110, -40);
            var before = DocumentPointUnder(state, 500);

            route(state);

            Assert.Equal(target, state.ZoomPercent);
            Assert.Equal(before, DocumentPointUnder(state, 500), precision: 6);
        }
    }

    [Fact]
    public void ZoomInZoomOutAndActualSize_KeepTheCenter()
    {
        var state = WithViewport();
        state.SetPan(75, 0);
        var before = DocumentPointUnder(state, 500);

        state.ZoomIn();
        Assert.Equal(before, DocumentPointUnder(state, 500), precision: 6);

        state.ZoomOut();
        state.ZoomOut();
        Assert.Equal(before, DocumentPointUnder(state, 500), precision: 6);

        state.ZoomToActualSize();
        Assert.Equal(100, state.ZoomPercent);
        Assert.Equal(before, DocumentPointUnder(state, 500), precision: 6);
    }

    [Theory]
    [InlineData(1, ZoomLevels.Min)]
    [InlineData(100000, ZoomLevels.Max)]
    public void ZoomAround_OutsideTheRange_StopsAtTheLimit(double target, double expected)
    {
        var state = WithViewport();

        state.ZoomAround(target, 10, 10);

        Assert.Equal(expected, state.ZoomPercent);
        Assert.True(double.IsFinite(state.PanX) && double.IsFinite(state.PanY));
    }

    [Fact]
    public void ZoomAround_AtTheLimitAlready_ChangesNothingAndRaisesNothing()
    {
        var state = WithViewport();
        state.SetZoom(ZoomLevels.Max);
        state.SetPan(33, 44);
        var raised = new List<string?>();
        state.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        state.ZoomAround(ZoomLevels.Max * 2, 100, 100);

        Assert.Empty(raised);
        Assert.Equal((33, 44), (state.PanX, state.PanY));
    }

    [Fact]
    public void ZoomAround_NaN_IsTreatedAsTheDefaultZoomByTheClamp_AndStaysFinite()
    {
        var state = WithViewport();

        state.ZoomAround(double.NaN, 10, 10);

        Assert.True(double.IsFinite(state.ZoomPercent));
        Assert.True(double.IsFinite(state.PanX));
    }

    [Fact]
    public void ZoomAround_AnchorAtTheCanvasEdge_StillLimitsTheOffset()
    {
        var state = WithViewport();

        state.ZoomAround(6400, 1000, 700);

        var limitX = ViewGeometry.PanLimit(1000, 1200 * 64);
        Assert.InRange(state.PanX, -limitX, limitX);
    }

    [Fact]
    public void ZoomToFit_ResetsTheOffset_AndFits()
    {
        var state = WithViewport(648, 448);
        state.SetZoom(300);
        state.SetPan(200, 100);

        state.ZoomToFit(648, 448);

        Assert.Equal(50, state.ZoomPercent, precision: 6);
        Assert.Equal((0, 0), (state.PanX, state.PanY));
    }

    [Fact]
    public void ZoomToFit_WithoutAViewport_KeepsTheZoomButResetsTheOffset()
    {
        var state = new EditorState(Document.Create("L"));
        state.SetZoom(200);
        state.SetPan(30, 40);

        state.ZoomToFit(0, 0);

        Assert.Equal(200, state.ZoomPercent);
        Assert.Equal((0, 0), (state.PanX, state.PanY));
    }

    [Fact]
    public void ViewChanges_NeverTouchTheDocumentTheHistoryOrTheDragFlag()
    {
        var state = WithViewport();
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, Rect()));
        var document = state.Document;
        var steps = state.History.Steps.Count;
        var historyEvents = 0;
        state.HistoryChanged += (_, _) => historyEvents++;

        state.SetPan(100, 50);
        state.PanBy(-20, 5);
        state.ZoomAround(300, 100, 100);
        state.SetViewport(500, 400);
        state.ZoomToFit(500, 400);

        Assert.Same(document, state.Document);
        Assert.Equal(steps, state.History.Steps.Count);
        Assert.Equal(0, historyEvents);
        Assert.False(state.IsDragInProgress);
        Assert.True(state.CanUndo);
    }

    [Fact]
    public void ArtboardSizeChange_LimitsTheOffsetAgain()
    {
        var state = WithViewport(1000, 700);
        state.SetPan(ViewGeometry.PanLimit(1000, 1200), 0);

        state.Document = state.Document.WithArtboardSize(200, 800);

        Assert.True(state.PanX <= ViewGeometry.PanLimit(1000, 200) + 1e-9);
    }
}
