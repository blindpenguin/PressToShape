using System;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class PropertiesViewModelCornersTests
{
    private sealed class Fixture
    {
        public Fixture(Tool tool = Tool.Select, CornerRadii radii = default, Func<RectangleShape, RectangleShape>? change = null)
        {
            Rectangle = new RectangleShape { Id = Guid.NewGuid(), X = 100, Y = 50, Width = 200, Height = 100, Radii = radii };
            if (change is not null)
            {
                Rectangle = change(Rectangle);
            }

            var document = Document.Create("x");
            State = new EditorState(document.AddShape(document.Layers[0].Id, Rectangle)) { ActiveTool = tool };
            State.Select(Rectangle.Id);
            Properties = new PropertiesViewModel(State, new SelectionEditor(State), FakeLocalizer.Create());
        }

        public EditorState State { get; }

        public PropertiesViewModel Properties { get; }

        public RectangleShape Rectangle { get; }

        public RectangleShape Current => (RectangleShape)State.Document.FindShape(Rectangle.Id)!.Shape;

        public int Steps => State.History.Steps.Count;

        public static void Enter(NumberFieldViewModel field, string text)
        {
            field.Text = text;
            field.Commit();
        }
    }

    [Fact]
    public void ShowCorners_OneRectangleWithSelectAndDirectSelect_IsTrue()
    {
        Assert.True(new Fixture(Tool.Select).Properties.ShowCorners);
        Assert.True(new Fixture(Tool.DirectSelect).Properties.ShowCorners);
    }

    [Fact]
    public void ShowCorners_OtherTools_IsFalse()
    {
        foreach (var tool in new[] { Tool.Node, Tool.Pen, Tool.Rectangle, Tool.Hand })
        {
            Assert.False(new Fixture(tool).Properties.ShowCorners);
        }
    }

    [Fact]
    public void ShowCorners_ChangingTheTool_UpdatesTheFlag()
    {
        var fixture = new Fixture(Tool.Select);
        var raised = false;
        fixture.Properties.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(PropertiesViewModel.ShowCorners);

        fixture.State.ActiveTool = Tool.Node;

        Assert.True(raised);
        Assert.False(fixture.Properties.ShowCorners);
    }

    [Fact]
    public void ShowCorners_EllipsePathTwoShapesOrGroup_IsFalse()
    {
        var rectangle = new RectangleShape { Id = Guid.NewGuid(), Width = 100, Height = 100 };
        var ellipse = new EllipseShape { Id = Guid.NewGuid(), X = 300, Width = 100, Height = 100 };
        var path = PathShape.Create([new DocumentNode(0, 500), new DocumentNode(100, 500), new DocumentNode(50, 600)], isClosed: true);
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [rectangle, ellipse, path])) { ActiveTool = Tool.Select };
        var properties = new PropertiesViewModel(state, new SelectionEditor(state), FakeLocalizer.Create());

        state.Select(ellipse.Id);
        Assert.False(properties.ShowCorners);
        state.Select(path.Id);
        Assert.False(properties.ShowCorners);
        state.SetSelection([rectangle.Id, ellipse.Id]);
        Assert.False(properties.ShowCorners);
        Assert.False(properties.LinkedRadius.IsEnabled);

        state.ReplaceDocument(state.Document.Group([rectangle.Id, ellipse.Id], Guid.NewGuid()));
        state.Select(state.Document.Layers[0].Shapes[0].Id);
        Assert.False(properties.ShowCorners);
    }

    [Fact]
    public void ShowCorners_NothingSelected_IsFalse()
    {
        var fixture = new Fixture();

        fixture.State.ClearSelection();

        Assert.False(fixture.Properties.ShowCorners);
        Assert.False(fixture.Properties.HasRectangleSelected);
    }

    [Fact]
    public void Fields_ShowTheEffectiveRadii()
    {
        var fixture = new Fixture(radii: new CornerRadii(5, 10, 500, 0));

        Assert.Equal("5", fixture.Properties.TopLeftRadius.Text);
        Assert.Equal("10", fixture.Properties.TopRightRadius.Text);
        Assert.Equal("50", fixture.Properties.BottomRightRadius.Text);
        Assert.Equal("0", fixture.Properties.BottomLeftRadius.Text);
    }

    [Fact]
    public void LinkedField_AllRadiiEqual_ShowsTheValue()
    {
        var fixture = new Fixture(radii: CornerRadii.Uniform(12));

        Assert.Equal("12", fixture.Properties.LinkedRadius.Text);
        Assert.False(fixture.Properties.LinkedRadius.IsMixed);
    }

    [Fact]
    public void LinkedField_RadiiDiffer_IsMixedAndEmpty()
    {
        var fixture = new Fixture(radii: new CornerRadii(1, 2, 3, 4));

        Assert.True(fixture.Properties.LinkedRadius.IsMixed);
        Assert.Equal(string.Empty, fixture.Properties.LinkedRadius.Text);
        Assert.True(fixture.Properties.LinkedRadius.IsEnabled);
    }

    [Fact]
    public void LinkedField_Entry_SetsAllFourRadiiAsOneStep()
    {
        var fixture = new Fixture(radii: new CornerRadii(1, 2, 3, 4));
        var steps = fixture.Steps;

        Fixture.Enter(fixture.Properties.LinkedRadius, "20");

        Assert.Equal(CornerRadii.Uniform(20), fixture.Current.Radii);
        Assert.Equal(steps + 1, fixture.Steps);
        Assert.Equal(HistoryStepType.ChangeCornerRadius, fixture.State.History.Steps[^1]);
        Assert.Equal("20", fixture.Properties.LinkedRadius.Text);
        Assert.False(fixture.Properties.LinkedRadius.IsMixed);
    }

    [Fact]
    public void CornerField_WithTheToggleOn_SetsAllFour()
    {
        var fixture = new Fixture();
        fixture.State.LinkCornerRadii = true;

        Fixture.Enter(fixture.Properties.BottomRightRadius, "15");

        Assert.Equal(CornerRadii.Uniform(15), fixture.Current.Radii);
        Assert.Equal("15", fixture.Properties.TopLeftRadius.Text);
    }

    [Fact]
    public void CornerField_WithTheToggleOff_SetsOnlyThatCorner()
    {
        var fixture = new Fixture(radii: new CornerRadii(1, 2, 3, 4));
        fixture.Properties.LinkCorners = false;

        Fixture.Enter(fixture.Properties.TopRightRadius, "30");

        Assert.Equal(new CornerRadii(1, 30, 3, 4), fixture.Current.Radii);
        Assert.Equal("30", fixture.Properties.TopRightRadius.Text);
        Assert.Equal("1", fixture.Properties.TopLeftRadius.Text);
    }

    [Fact]
    public void LinkedField_WithTheToggleOff_StillSetsAllFour()
    {
        var fixture = new Fixture();
        fixture.Properties.LinkCorners = false;

        Fixture.Enter(fixture.Properties.LinkedRadius, "8");

        Assert.Equal(CornerRadii.Uniform(8), fixture.Current.Radii);
    }

    [Fact]
    public void LinkCorners_SharesTheStateWithTheEditorState()
    {
        var fixture = new Fixture();
        var raised = false;
        fixture.Properties.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(PropertiesViewModel.LinkCorners);

        fixture.State.LinkCornerRadii = false;

        Assert.False(fixture.Properties.LinkCorners);
        Assert.True(raised);
        fixture.Properties.LinkCorners = true;
        Assert.True(fixture.State.LinkCornerRadii);
    }

    [Fact]
    public void Entry_AboveTheLimit_IsLimitedAndTheFieldShowsTheLimitedValue()
    {
        var fixture = new Fixture();

        Fixture.Enter(fixture.Properties.LinkedRadius, "500");

        Assert.Equal(CornerRadii.Uniform(50), fixture.Current.Radii);
        Assert.Equal("50", fixture.Properties.LinkedRadius.Text);
    }

    [Fact]
    public void Entry_AboveTheLimitWhenAlreadyAtTheLimit_ShowsTheLimitedValueAndMakesNoStep()
    {
        var fixture = new Fixture(radii: CornerRadii.Uniform(50));
        var steps = fixture.Steps;

        Fixture.Enter(fixture.Properties.LinkedRadius, "80");

        Assert.Equal("50", fixture.Properties.LinkedRadius.Text);
        Assert.Equal(steps, fixture.Steps);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-5")]
    [InlineData("")]
    [InlineData("1e999")]
    public void Entry_InvalidText_RevertsWithoutAStep(string text)
    {
        var fixture = new Fixture(radii: CornerRadii.Uniform(7));
        var steps = fixture.Steps;

        Fixture.Enter(fixture.Properties.LinkedRadius, text);

        Assert.Equal("7", fixture.Properties.LinkedRadius.Text);
        Assert.Equal(CornerRadii.Uniform(7), fixture.Current.Radii);
        Assert.Equal(steps, fixture.Steps);
    }

    [Fact]
    public void Entry_ThenUndo_PutsAllRadiiBack()
    {
        var fixture = new Fixture(radii: new CornerRadii(1, 2, 3, 4));

        Fixture.Enter(fixture.Properties.LinkedRadius, "20");
        fixture.State.Undo();

        Assert.Equal(new CornerRadii(1, 2, 3, 4), fixture.Current.Radii);
        Assert.Equal("1", fixture.Properties.TopLeftRadius.Text);
        Assert.True(fixture.Properties.LinkedRadius.IsMixed);
    }

    [Fact]
    public void Fields_FollowAChangeOfTheDocumentLikeADrag()
    {
        var fixture = new Fixture();

        fixture.State.Preview(fixture.State.Document.UpdateShape(fixture.Rectangle.Id, _ => fixture.Rectangle with { Radii = CornerRadii.Uniform(33) }));

        Assert.Equal("33", fixture.Properties.LinkedRadius.Text);
        Assert.Equal("33", fixture.Properties.BottomLeftRadius.Text);
    }

    [Fact]
    public void Fields_AfterAResize_ShowTheLimitedRadius()
    {
        var fixture = new Fixture(radii: CornerRadii.Uniform(40));

        fixture.State.Edit(HistoryStepType.ChangeSize, document => document.UpdateShape(fixture.Rectangle.Id, _ => fixture.Rectangle with { Height = 40 }));

        Assert.Equal("20", fixture.Properties.LinkedRadius.Text);
        Assert.Equal(CornerRadii.Uniform(40), fixture.Current.Radii);
    }

    [Fact]
    public void ShowCorners_RectangleThatGetsLocked_IsHiddenBecauseALockedShapeIsNotSelected()
    {
        var fixture = new Fixture();
        Assert.True(fixture.Properties.ShowCorners);

        fixture.State.Edit(HistoryStepType.LockShapes, document => document.UpdateShape(fixture.Rectangle.Id, shape => shape with { IsLocked = true }));

        Assert.False(fixture.Properties.ShowCorners);
        Assert.False(fixture.Properties.LinkedRadius.IsEnabled);
    }

    [Fact]
    public void CommitPendingEdits_TypedRadius_IsApplied()
    {
        var fixture = new Fixture();
        fixture.Properties.LinkedRadius.IsEditing = true;
        fixture.Properties.LinkedRadius.Text = "9";

        fixture.Properties.CommitPendingEdits();

        Assert.Equal(CornerRadii.Uniform(9), fixture.Current.Radii);
    }

    [Fact]
    public void ChangingTheRadius_DoesNotChangeThePositionOrSizeOrSelection()
    {
        var fixture = new Fixture();

        Fixture.Enter(fixture.Properties.LinkedRadius, "10");

        Assert.Equal(fixture.Rectangle.Bounds, fixture.Current.Bounds);
        Assert.Equal(fixture.Rectangle.Id, Assert.Single(fixture.State.SelectedShapeIds));
    }
}
