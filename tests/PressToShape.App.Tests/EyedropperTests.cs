using System.Collections.Generic;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class EyedropperTests
{
    private static readonly Color Red = Color.FromRgb(255, 0, 0);
    private static readonly Color Green = Color.FromRgb(0, 255, 0);
    private static readonly Color Blue = new(0, 0, 255, 128);

    private static (EditorState State, CanvasInteraction Interaction, List<(ColorPickTarget Target, Color Color)> Picked) Create(params Shape[] shapes)
    {
        var state = new EditorState(Document.Create("L"));
        state.Edit(PressToShape.Core.History.HistoryStepType.DrawRectangle, d => shapes.Aggregate(d, (doc, s) => doc.AddShape(state.ActiveLayerId, s)));
        var picked = new List<(ColorPickTarget, Color)>();
        state.ColorPicked += (target, color) => picked.Add((target, color));
        return (state, new CanvasInteraction(state), picked);
    }

    private static RectangleShape Rect(double x, Appearance appearance) =>
        new() { X = x, Y = 0, Width = 50, Height = 50, Appearance = appearance };

    [Fact]
    public void Click_OnAShapeWithFill_ReportsTheFillWithAlphaAndEndsThePick()
    {
        var (state, interaction, picked) = Create(Rect(0, Appearance.Default with { Fill = Blue, Opacity = 0.2 }));
        state.BeginColorPick(ColorPickTarget.Stroke);

        interaction.PointerPressed(10, 10);

        Assert.Equal([(ColorPickTarget.Stroke, Blue)], picked);
        Assert.Equal(ColorPickTarget.None, state.ColorPick);
    }

    [Fact]
    public void Click_OnAShapeWithoutFill_ReportsTheStroke()
    {
        var (state, interaction, picked) = Create(Rect(0, Appearance.Default with { Fill = null, Stroke = Green }));
        state.BeginColorPick(ColorPickTarget.Fill);

        interaction.PointerPressed(10, 10);

        Assert.Equal([(ColorPickTarget.Fill, Green)], picked);
    }

    [Fact]
    public void Click_OnAShapeWithNoFillAndNoStroke_ReportsNothingAndEndsThePick()
    {
        var (state, interaction, picked) = Create(Rect(0, Appearance.Default with { Fill = null, Stroke = null }));
        state.BeginColorPick(ColorPickTarget.Fill);

        interaction.PointerPressed(10, 10);

        Assert.Empty(picked);
        Assert.Equal(ColorPickTarget.None, state.ColorPick);
    }

    [Fact]
    public void Click_OverlappingShapes_TakesTheTopmost()
    {
        var (state, interaction, picked) = Create(
            Rect(0, Appearance.Default with { Fill = Red }),
            Rect(0, Appearance.Default with { Fill = Green }));
        state.BeginColorPick(ColorPickTarget.Fill);

        interaction.PointerPressed(10, 10);

        Assert.Equal(Green, Assert.Single(picked).Color);
    }

    [Fact]
    public void Click_OnEmptyCanvas_ChangesNothingAndEndsThePick()
    {
        var (state, interaction, picked) = Create(Rect(0, Appearance.Default));
        state.BeginColorPick(ColorPickTarget.Fill);
        var document = state.Document;

        interaction.PointerPressed(500, 500);

        Assert.Empty(picked);
        Assert.Equal(ColorPickTarget.None, state.ColorPick);
        Assert.Same(document, state.Document);
    }

    [Fact]
    public void Click_DuringAPickWithTheSelectTool_DoesNotSelectOrDrag()
    {
        var (state, interaction, _) = Create(Rect(0, Appearance.Default));
        state.ActiveTool = Tool.Select;
        state.BeginColorPick(ColorPickTarget.Fill);

        interaction.PointerPressed(10, 10);
        interaction.PointerMoved(40, 40);
        interaction.PointerReleased(40, 40);

        Assert.False(state.HasSelection);
        Assert.False(interaction.IsDragging);
        Assert.Equal(0, state.Document.Layers[0].Shapes[0].X);
    }

    [Fact]
    public void Click_AfterThePickEnded_WorksAsBefore()
    {
        var (state, interaction, _) = Create(Rect(0, Appearance.Default));
        state.ActiveTool = Tool.Select;
        state.BeginColorPick(ColorPickTarget.Fill);
        interaction.PointerPressed(10, 10);

        interaction.PointerPressed(10, 10);

        Assert.True(state.HasSelection);
    }

    [Fact]
    public void Escape_DuringAPick_EndsItWithoutAColorAndWithoutClearingTheSelection()
    {
        var (state, _, picked) = Create(Rect(0, Appearance.Default));
        var canvas = new CanvasAreaViewModel(state);
        state.SelectOnly(state.Document.Layers[0].Shapes[0].Id);
        state.BeginColorPick(ColorPickTarget.Fill);

        var handled = canvas.KeyPressed(CanvasKey.Escape, shift: false);

        Assert.True(handled);
        Assert.Empty(picked);
        Assert.Equal(ColorPickTarget.None, state.ColorPick);
        Assert.True(state.HasSelection);
    }

    [Theory]
    [InlineData("Hand")]
    [InlineData("Zoom")]
    [InlineData("Rectangle")]
    public void CanvasClick_DuringAPick_IsTakenByThePickWithAnyTool(string toolName)
    {
        var tool = System.Enum.Parse<Tool>(toolName);
        var (state, _, picked) = Create(Rect(0, Appearance.Default with { Fill = Red }));
        var canvas = new CanvasAreaViewModel(state);
        state.ActiveTool = tool;
        state.BeginColorPick(ColorPickTarget.Fill);
        var zoom = state.ZoomPercent;
        var origin = (canvas.OriginX, canvas.OriginY);

        canvas.PointerPressed(origin.OriginX + 10 * zoom / 100, origin.OriginY + 10 * zoom / 100);

        Assert.Equal(Red, Assert.Single(picked).Color);
        Assert.Equal(zoom, state.ZoomPercent);
    }
}
