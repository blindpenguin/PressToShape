using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class EyedropperToolTests
{
    private static readonly Color Red = Color.FromRgb(255, 0, 0);
    private static readonly Color Green = Color.FromRgb(0, 255, 0);

    private sealed class Fixture
    {
        public Fixture()
        {
            State = new EditorState(Document.Create("L"));
            Source = new RectangleShape { X = 0, Y = 0, Width = 50, Height = 50, Appearance = Appearance.Default with { Fill = Red, Stroke = Green } };
            Target = new RectangleShape { X = 100, Y = 0, Width = 50, Height = 50 };
            State.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(State.ActiveLayerId, Source).AddShape(State.ActiveLayerId, Target));
            Properties = new PropertiesViewModel(State, new SelectionEditor(State), FakeLocalizer.Create());
            Interaction = new CanvasInteraction(State);
            State.ActiveTool = Tool.Eyedropper;
        }

        public EditorState State { get; }

        public RectangleShape Source { get; }

        public RectangleShape Target { get; }

        public PropertiesViewModel Properties { get; }

        public CanvasInteraction Interaction { get; }

        public Appearance TargetAppearance => State.Document.FindShape(Target.Id)!.Shape.Appearance;

        public int Steps => State.History.Steps.Count;
    }

    [Fact]
    public void Click_OnAShape_SetsTheFillOfTheSelectionAsOneStep()
    {
        var f = new Fixture();
        f.State.SelectOnly(f.Target.Id);
        var steps = f.Steps;

        f.Interaction.PointerPressed(10, 10);

        Assert.Equal(Red, f.TargetAppearance.Fill);
        Assert.Equal(Appearance.Default.Stroke, f.TargetAppearance.Stroke);
        Assert.Equal(steps + 1, f.Steps);
        Assert.Equal(HistoryStepType.ChangeFill, f.State.History.Steps[^1]);
    }

    [Fact]
    public void ShiftClick_OnAShape_SetsTheStrokeToTheShapesFillColor()
    {
        var f = new Fixture();
        f.State.SelectOnly(f.Target.Id);

        f.Interaction.PointerPressed(10, 10, shift: true);

        // Shift only chooses the target. The color is the shape's fill, as for every pick (R3.4).
        Assert.Equal(Red, f.TargetAppearance.Stroke);
        Assert.Equal(Appearance.Default.Fill, f.TargetAppearance.Fill);
        Assert.Equal(HistoryStepType.ChangeStroke, f.State.History.Steps[^1]);
    }

    [Fact]
    public void Click_WithNothingSelected_ChangesTheDefaultsAndNoStep()
    {
        var f = new Fixture();
        var steps = f.Steps;

        f.Interaction.PointerPressed(10, 10);

        Assert.Equal(Red, f.State.DefaultAppearance.Fill);
        Assert.Equal(steps, f.Steps);
    }

    [Fact]
    public void Click_OnEmptyCanvas_ChangesNothing()
    {
        var f = new Fixture();
        f.State.SelectOnly(f.Target.Id);
        var document = f.State.Document;

        f.Interaction.PointerPressed(600, 600);

        Assert.Same(document, f.State.Document);
        Assert.Same(Appearance.Default, f.State.DefaultAppearance);
    }

    [Fact]
    public void Click_StaysOnTheEyedropperToolAndKeepsTheSelection()
    {
        var f = new Fixture();
        f.State.SelectOnly(f.Target.Id);

        f.Interaction.PointerPressed(10, 10);
        f.Interaction.PointerPressed(10, 10, shift: true);

        Assert.Equal(Tool.Eyedropper, f.State.ActiveTool);
        Assert.Equal([f.Target.Id], f.State.SelectedShapeIds.ToArray());
        Assert.Equal(ColorPickTarget.None, f.State.ColorPick);
        Assert.False(f.Interaction.IsDragging);
    }

    [Fact]
    public void FlyoutPick_StillWorksWithAnotherToolActive()
    {
        var f = new Fixture();
        f.State.ActiveTool = Tool.Select;
        f.State.SelectOnly(f.Target.Id);

        f.Properties.StartPick(ColorPickTarget.Fill);
        f.Interaction.PointerPressed(10, 10);

        Assert.Equal(Red, f.TargetAppearance.Fill);
        Assert.Equal(Tool.Select, f.State.ActiveTool);
    }
}
