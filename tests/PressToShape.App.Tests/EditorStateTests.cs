using System;
using System.Collections.Generic;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
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

        state.UpdateLayer(layerId, l => l with { IsVisible = false });

        Assert.False(state.Document.Layers[0].IsVisible);
        Assert.True(before.Layers[0].IsVisible);
        Assert.NotSame(before, state.Document);
    }

    [Fact]
    public void UpdateLayer_UnknownLayer_ThrowsAndKeepsTheDocument()
    {
        var state = new EditorState();
        var before = state.Document;

        Assert.Throws<KeyNotFoundException>(() => state.UpdateLayer(Guid.NewGuid(), l => l));

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
}
