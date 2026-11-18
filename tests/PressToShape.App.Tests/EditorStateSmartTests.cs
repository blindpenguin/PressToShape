using System.Collections.Generic;
using System.Collections.Immutable;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class EditorStateSmartTests
{
    private static (EditorState State, Guide Guide, RectangleShape Shape) Create()
    {
        var guide = new Guide { Orientation = GuideOrientation.Horizontal, Position = 50 };
        var shape = new RectangleShape { X = 10, Y = 10, Width = 20, Height = 20 };
        var document = Document.Create("L");
        document = document.AddShape(document.Layers[0].Id, shape).AddGuide(guide);
        return (new EditorState(document), guide, shape);
    }

    private static List<string?> Watch(EditorState state)
    {
        var raised = new List<string?>();
        state.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        return raised;
    }

    [Fact]
    public void Defaults_AreEmpty()
    {
        var (state, _, _) = Create();

        Assert.Empty(state.SmartMarkers);
        Assert.Empty(state.ActiveGuideIds);
        Assert.Null(state.SelectedGuideId);
    }

    [Fact]
    public void Properties_RaisePropertyChanged()
    {
        var (state, guide, _) = Create();
        var raised = Watch(state);

        state.SmartMarkers = [new SmartMarker(SmartMarkerType.Distance, SnapAxis.X, 0, 10, 5, 10)];
        state.ActiveGuideIds = [guide.Id];
        state.SelectGuide(guide.Id);

        Assert.Contains(nameof(EditorState.SmartMarkers), raised);
        Assert.Contains(nameof(EditorState.ActiveGuideIds), raised);
        Assert.Contains(nameof(EditorState.SelectedGuideId), raised);
    }

    [Fact]
    public void SelectGuide_ClearsTheShapeSelection()
    {
        var (state, guide, shape) = Create();
        state.Select(shape.Id);

        state.SelectGuide(guide.Id);

        Assert.Equal(guide.Id, state.SelectedGuideId);
        Assert.False(state.HasSelection);
    }

    [Fact]
    public void SelectingAShape_ClearsTheGuideSelection()
    {
        var (state, guide, shape) = Create();
        state.SelectGuide(guide.Id);

        state.Select(shape.Id);

        Assert.Null(state.SelectedGuideId);
        Assert.True(state.HasSelection);
    }

    [Fact]
    public void ClearingTheShapeSelection_KeepsTheGuideSelection()
    {
        var (state, guide, _) = Create();
        state.SelectGuide(guide.Id);

        state.ClearSelection();

        Assert.Equal(guide.Id, state.SelectedGuideId);
    }

    [Fact]
    public void SelectGuide_UnknownId_DoesNothing()
    {
        var (state, _, shape) = Create();
        state.Select(shape.Id);

        state.SelectGuide(System.Guid.NewGuid());

        Assert.Null(state.SelectedGuideId);
        Assert.True(state.HasSelection);
    }

    [Fact]
    public void ClearGuideSelection_Clears()
    {
        var (state, guide, _) = Create();
        state.SelectGuide(guide.Id);

        state.ClearGuideSelection();

        Assert.Null(state.SelectedGuideId);
    }

    [Fact]
    public void UndoOfAGuideCreate_ClearsAStaleSelection()
    {
        var (state, _, _) = Create();
        var created = new Guide { Orientation = GuideOrientation.Vertical, Position = 7 };
        state.Edit(HistoryStepType.CreateGuide, document => document.AddGuide(created));
        state.SelectGuide(created.Id);

        state.Undo();

        Assert.Null(state.SelectedGuideId);
    }

    [Fact]
    public void DeletingTheSelectedGuide_ClearsTheSelection()
    {
        var (state, guide, _) = Create();
        state.SelectGuide(guide.Id);

        state.Edit(HistoryStepType.DeleteGuide, document => document.RemoveGuide(guide.Id));

        Assert.Null(state.SelectedGuideId);
    }

    [Fact]
    public void SmartMarker_PathPointType_IsAPoint()
    {
        var marker = new SmartMarker(SmartMarkerType.PathPoint, SnapAxis.X, 3, 3, 4, double.NaN);

        Assert.Equal(3, marker.From);
        Assert.Equal(4, marker.Cross);
        Assert.True(double.IsNaN(marker.Value));
    }

    [Fact]
    public void SmartMarkers_Cleared_AreEmptyAgain()
    {
        var (state, _, _) = Create();
        state.SmartMarkers = [new SmartMarker(SmartMarkerType.EqualGap, SnapAxis.Y, 0, 5, 1, 5)];

        state.SmartMarkers = ImmutableArray<SmartMarker>.Empty;

        Assert.Empty(state.SmartMarkers);
    }
}
