using System.Linq;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class PropertiesViewModelPathTests
{
    private sealed class Fixture
    {
        public Fixture()
        {
            State = new EditorState { ActiveTool = Tool.Select };
            Properties = new PropertiesViewModel(State, new SelectionEditor(State), FakeLocalizer.Create("en", null));
        }

        public EditorState State { get; }

        public PropertiesViewModel Properties { get; }

        public int Steps => State.History.Steps.Count;

        public void Add(params Shape[] shapes)
        {
            State.Edit(HistoryStepType.DrawPath, d => shapes.Aggregate(d, (doc, s) => doc.AddShape(State.ActiveLayerId, s)));
            State.SetSelection(shapes.Select(s => s.Id));
        }

        public Shape Get(Shape shape) => State.Document.FindShape(shape.Id)!.Shape;
    }

    private static PathShape Triangle(bool closed = false, FillRule rule = FillRule.NonZero) =>
        PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(50, 100)], closed, rule);

    [Fact]
    public void SelectingAPath_ShowsThePathSectionEvenWithTheSelectTool()
    {
        var f = new Fixture();
        Assert.False(f.Properties.ShowPath);

        f.Add(Triangle());

        Assert.True(f.Properties.HasPathSelected);
        Assert.True(f.Properties.ShowPath);
    }

    [Fact]
    public void SelectingARectangle_DoesNotShowThePathSection()
    {
        var f = new Fixture();

        f.Add(new RectangleShape { Width = 10, Height = 10 });

        Assert.False(f.Properties.HasPathSelected);
        Assert.False(f.Properties.ShowPath);
    }

    [Fact]
    public void ThePenTool_ShowsThePathSectionWithoutASelection()
    {
        var f = new Fixture();

        f.State.ActiveTool = Tool.Pen;

        Assert.True(f.Properties.ShowPath);
        Assert.False(f.Properties.HasPathSelected);
    }

    [Fact]
    public void ToolsWithoutProperties_ShowNoPathSectionEvenForAPath()
    {
        var f = new Fixture();
        f.Add(Triangle());

        f.State.ActiveTool = Tool.Hand;

        Assert.False(f.Properties.ShowPath);
    }

    [Fact]
    public void ThePathInsideAGroup_CountsAsSelected()
    {
        var f = new Fixture();
        var group = new GroupShape { Children = [Triangle(), new RectangleShape { Width = 5, Height = 5 }] };

        f.Add(group);

        Assert.True(f.Properties.HasPathSelected);
    }

    [Fact]
    public void Values_OfASinglePath_ShowItsState()
    {
        var f = new Fixture();

        f.Add(Triangle(closed: true, FillRule.EvenOdd));

        Assert.True(f.Properties.IsPathClosed);
        Assert.False(f.Properties.IsPathOpen);
        Assert.True(f.Properties.IsFillRuleEvenOdd);
        Assert.False(f.Properties.IsFillRuleNonZero);
    }

    [Fact]
    public void Values_OfMixedPaths_SetNeitherChoice()
    {
        var f = new Fixture();

        f.Add(Triangle(closed: true, FillRule.EvenOdd), Triangle(closed: false, FillRule.NonZero));

        Assert.False(f.Properties.IsPathClosed);
        Assert.False(f.Properties.IsPathOpen);
        Assert.False(f.Properties.IsFillRuleEvenOdd);
        Assert.False(f.Properties.IsFillRuleNonZero);
    }

    [Fact]
    public void ClosePath_OpenPath_ClosesItAsOneStep()
    {
        var f = new Fixture();
        var path = Triangle();
        f.Add(path);
        var steps = f.Steps;

        f.Properties.ClosePathCommand.Execute(null);

        Assert.True(((PathShape)f.Get(path)).IsClosed);
        Assert.Equal(steps + 1, f.Steps);
        Assert.Equal(HistoryStepType.ChangePath, f.State.History.Steps[^1]);
        Assert.True(f.Properties.IsPathClosed);
    }

    [Fact]
    public void OpenPath_ClosedPath_OpensIt()
    {
        var f = new Fixture();
        var path = Triangle(closed: true);
        f.Add(path);

        f.Properties.OpenPathCommand.Execute(null);

        Assert.False(((PathShape)f.Get(path)).IsClosed);
        Assert.True(f.Properties.IsPathOpen);
    }

    [Fact]
    public void ClosePath_PathWithACurvedClosingSegment_GrowsTheBoxToTheCurve()
    {
        var f = new Fixture();
        var path = PathShape.Create(
            [new DocumentNode(0, 0, inX: -50, inY: 50), new DocumentNode(100, 0), new DocumentNode(100, 100, outX: -50, outY: 150)]);
        f.Add(path);

        f.Properties.ClosePathCommand.Execute(null);

        var closed = (PathShape)f.Get(path);
        Assert.True(closed.X < 0);
        Assert.True(closed.Y + closed.Height > 100);
    }

    [Fact]
    public void ClosePath_RotatedPath_KeepsTheNodesWhereTheyAre()
    {
        var f = new Fixture();
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 50)]) with { Rotation = 30 };
        f.Add(path);

        f.Properties.ClosePathCommand.Execute(null);

        var closed = (PathShape)f.Get(path);
        Assert.Equal(30, closed.Rotation);
        // A closing line does not change the box of straight segments, so the box is the same.
        Assert.Equal(path.Bounds, closed.Bounds);
    }

    [Fact]
    public void FillRuleCommands_ChangeTheRuleAsOneStep()
    {
        var f = new Fixture();
        var path = Triangle();
        f.Add(path);
        var steps = f.Steps;

        f.Properties.UseEvenOddFillCommand.Execute(null);

        Assert.Equal(FillRule.EvenOdd, ((PathShape)f.Get(path)).FillRule);
        Assert.Equal(steps + 1, f.Steps);
        Assert.True(f.Properties.IsFillRuleEvenOdd);

        f.Properties.UseNonZeroFillCommand.Execute(null);

        Assert.Equal(FillRule.NonZero, ((PathShape)f.Get(path)).FillRule);
    }

    [Fact]
    public void Commands_ThatChangeNothing_MakeNoStep()
    {
        var f = new Fixture();
        f.Add(Triangle(closed: true, FillRule.EvenOdd));
        var steps = f.Steps;

        f.Properties.ClosePathCommand.Execute(null);
        f.Properties.UseEvenOddFillCommand.Execute(null);

        Assert.Equal(steps, f.Steps);
    }

    [Fact]
    public void Commands_OnAMixedSelection_ChangeThePathsOnly()
    {
        var f = new Fixture();
        var path = Triangle();
        var rectangle = new RectangleShape { Width = 10, Height = 10 };
        f.Add(path, rectangle);

        f.Properties.ClosePathCommand.Execute(null);

        Assert.True(((PathShape)f.Get(path)).IsClosed);
        Assert.Equal(rectangle, f.Get(rectangle));
    }

    [Fact]
    public void Commands_OnSeveralPaths_ApplyToAllInOneStep()
    {
        var f = new Fixture();
        var first = Triangle();
        var second = Triangle();
        f.Add(first, second);
        var steps = f.Steps;

        f.Properties.ClosePathCommand.Execute(null);

        Assert.All(new[] { first, second }, p => Assert.True(((PathShape)f.Get(p)).IsClosed));
        Assert.Equal(steps + 1, f.Steps);
        Assert.True(f.Properties.IsPathClosed);
    }

    [Fact]
    public void Commands_AreDisabledWithoutAPath()
    {
        var f = new Fixture();

        Assert.False(f.Properties.ClosePathCommand.CanExecute(null));

        f.Add(Triangle());

        Assert.True(f.Properties.ClosePathCommand.CanExecute(null));
        Assert.True(f.Properties.UseEvenOddFillCommand.CanExecute(null));
    }

    [Fact]
    public void Values_AfterUndo_FollowTheDocument()
    {
        var f = new Fixture();
        f.Add(Triangle());
        f.Properties.ClosePathCommand.Execute(null);

        f.State.Undo();

        Assert.True(f.Properties.IsPathOpen);
        Assert.False(f.Properties.IsPathClosed);
    }

    [Fact]
    public void PropertyChanged_WhenTheSelectionChanges_IsRaisedForThePathValues()
    {
        var f = new Fixture();
        var raised = new System.Collections.Generic.List<string?>();
        f.Properties.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        f.Add(Triangle());

        Assert.Contains(nameof(PropertiesViewModel.HasPathSelected), raised);
        Assert.Contains(nameof(PropertiesViewModel.IsPathOpen), raised);
        Assert.Contains(nameof(PropertiesViewModel.ShowPath), raised);
    }
}
