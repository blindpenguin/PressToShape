using System;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasAreaViewModelRadiusTests
{
    // The viewport has the size of the artboard at 100 percent, so a screen position is a document position. The radius handles of the
    // rectangle are at (124, 74), (276, 74), (276, 126) and (124, 126).
    private static (EditorState State, CanvasAreaViewModel Canvas, RectangleShape Rectangle) Create()
    {
        var rectangle = new RectangleShape { Id = Guid.NewGuid(), X = 100, Y = 50, Width = 200, Height = 100 };
        var document = Document.Create("x");
        var state = new EditorState(document.AddShape(document.Layers[0].Id, rectangle)) { ActiveTool = Tool.Select };
        state.Select(rectangle.Id);
        var canvas = new CanvasAreaViewModel(state) { ViewportWidth = 1200, ViewportHeight = 800 };
        return (state, canvas, rectangle);
    }

    private static RectangleShape Current(EditorState state, Shape shape) => Assert.IsType<RectangleShape>(state.Document.FindShape(shape.Id)!.Shape);

    [Fact]
    public void Escape_DuringTheDragOfARadiusHandle_PutsTheRadiiBackAndRecordsNoStep()
    {
        var (state, canvas, rectangle) = Create();
        var steps = state.History.Steps.Count;

        canvas.PointerPressed(124, 74);
        canvas.PointerMoved(150, 100);
        Assert.False(Current(state, rectangle).Radii.IsZero);

        var handled = canvas.KeyPressed(CanvasKey.Escape, shift: false);
        canvas.PointerReleased(150, 100);

        Assert.True(handled);
        Assert.True(Current(state, rectangle).Radii.IsZero);
        Assert.Equal(steps, state.History.Steps.Count);
        Assert.False(state.IsDragInProgress);
        Assert.Equal(rectangle.Id, Assert.Single(state.SelectedShapeIds));
    }

    [Fact]
    public void Escape_AfterTheRadiusDragEnded_KeepsTheRadii()
    {
        var (state, canvas, rectangle) = Create();
        canvas.PointerPressed(124, 74);
        canvas.PointerMoved(150, 100);
        canvas.PointerReleased(150, 100);
        var radii = Current(state, rectangle).Radii;

        canvas.KeyPressed(CanvasKey.Escape, shift: false);

        Assert.False(radii.IsZero);
        Assert.Equal(radii, Current(state, rectangle).Radii);
    }
}
