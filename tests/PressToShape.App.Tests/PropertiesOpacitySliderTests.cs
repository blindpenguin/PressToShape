using System;
using System.Linq;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using Xunit;

namespace PressToShape.App.Tests;

public class PropertiesOpacitySliderTests
{
    private static (EditorState State, PropertiesViewModel Properties, Guid[] Ids) Create(int shapes = 1)
    {
        var state = new EditorState();
        var properties = new PropertiesViewModel(state, new SelectionEditor(state), FakeLocalizer.Create());
        var ids = TestSelection.AddAndSelect(state, shapes);
        return (state, properties, ids);
    }

    private static double Opacity(EditorState state, Guid id) => state.Document.FindShape(id)!.Shape.Appearance.Opacity;

    [Fact]
    public void SliderValue_FollowsTheShapes()
    {
        var (state, properties, ids) = Create();
        Assert.Equal(100, properties.OpacitySliderValue);

        state.Edit(HistoryStepType.ChangeOpacity, d => d.UpdateShape(ids[0], s => s with { Appearance = s.Appearance with { Opacity = 0.3 } }));

        Assert.Equal(30, properties.OpacitySliderValue, 6);
    }

    [Fact]
    public void ADragWithManyChanges_IsOneChangeOpacityStep_AndPreviewsLive()
    {
        var (state, properties, ids) = Create(2);
        var steps = state.History.Steps.Count;

        properties.BeginOpacityDrag();
        properties.PreviewOpacity(80);
        properties.PreviewOpacity(55);
        Assert.Equal(0.55, Opacity(state, ids[0]), 6);
        Assert.Equal(0.55, Opacity(state, ids[1]), 6);
        Assert.Equal(steps, state.History.Steps.Count);
        properties.EndOpacityDrag();

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.ChangeOpacity, state.History.Steps[^1]);
        state.Undo();
        Assert.Equal(1, Opacity(state, ids[0]));
    }

    [Fact]
    public void ChangesWithoutADragStart_AreIgnored_AndAnUnchangedDragMakesNoStep()
    {
        var (state, properties, ids) = Create();
        var steps = state.History.Steps.Count;

        properties.PreviewOpacity(10);
        properties.BeginOpacityDrag();
        properties.EndOpacityDrag();

        Assert.Equal(1, Opacity(state, ids[0]));
        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void ValuesAreClamped_AndNonFiniteValuesIgnored()
    {
        var (state, properties, ids) = Create();

        properties.BeginOpacityDrag();
        properties.PreviewOpacity(150);
        Assert.Equal(1, Opacity(state, ids[0]));
        properties.PreviewOpacity(-20);
        Assert.Equal(0, Opacity(state, ids[0]));
        properties.PreviewOpacity(double.NaN);
        Assert.Equal(0, Opacity(state, ids[0]));
        properties.EndOpacityDrag();
    }

    [Fact]
    public void NothingSelected_NoDragStarts()
    {
        var state = new EditorState();
        var properties = new PropertiesViewModel(state, new SelectionEditor(state), FakeLocalizer.Create());

        properties.BeginOpacityDrag();
        properties.PreviewOpacity(50);
        properties.EndOpacityDrag();

        Assert.Empty(state.History.Steps);
    }
}
