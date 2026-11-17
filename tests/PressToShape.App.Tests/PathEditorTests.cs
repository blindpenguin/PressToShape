using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.PathOperations;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class PathEditorTests
{
    private sealed class Fixture
    {
        private int _next;

        public Fixture(params Shape[] shapes)
        {
            var document = Document.Create("x");
            State = new EditorState(document.AddShapes(document.Layers[0].Id, shapes));
            Messages = new FakeMessageDialogs();
            Editor = new PathEditor(State, Messages, FakeLocalizer.Create(), () => new Guid(++_next, 0, 0, new byte[8]));
        }

        public EditorState State { get; }

        public FakeMessageDialogs Messages { get; }

        public PathEditor Editor { get; }

        public IReadOnlyList<Shape> Shapes => State.Document.Layers[0].Shapes;

        public void Select(params Shape[] shapes) => State.SetSelection(shapes.Select(shape => shape.Id));
    }

    private static RectangleShape Rect(double x, double y, double size = 10) => new() { X = x, Y = y, Width = size, Height = size };

    private static PathShape Compound() =>
        PathShape.Create(
            [
                new DocumentSubPath([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)], true),
                new DocumentSubPath([new DocumentNode(200, 0), new DocumentNode(300, 0)], false),
            ]);

    [Fact]
    public void CanBoolean_TwoRectanglesSelected_IsTrue()
    {
        var a = Rect(0, 0);
        var b = Rect(5, 5);
        var f = new Fixture(a, b);
        f.Select(a, b);

        Assert.True(f.Editor.CanBoolean);
        Assert.True(f.Editor.CanCombine);
        Assert.False(f.Editor.CanBreakApart);
    }

    [Fact]
    public void CanBoolean_NothingOrOneShapeSelected_IsFalseForEveryCommand()
    {
        var a = Rect(0, 0);
        var f = new Fixture(a, Rect(5, 5));

        Assert.False(f.Editor.CanBoolean);
        Assert.False(f.Editor.CanCombine);
        Assert.False(f.Editor.CanBreakApart);

        f.Select(a);

        Assert.False(f.Editor.CanBoolean);
        Assert.False(f.Editor.CanCombine);
    }

    [Fact]
    public void CanBoolean_GroupInTheSelection_IsFalse()
    {
        var group = new GroupShape { Children = [Rect(0, 0)] };
        var other = Rect(5, 5);
        var f = new Fixture(group, other);
        f.Select(group, other);

        Assert.False(f.Editor.CanBoolean);
        Assert.False(f.Editor.CanCombine);
    }

    [Fact]
    public void CanBoolean_OpenPath_IsFalseButCombineIsTrue()
    {
        var open = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]);
        var rectangle = Rect(5, 5);
        var f = new Fixture(open, rectangle);
        f.Select(open, rectangle);

        Assert.False(f.Editor.CanBoolean);
        Assert.True(f.Editor.CanCombine);
    }

    [Fact]
    public void CanBoolean_HiddenOrLockedShape_IsFalse()
    {
        var a = Rect(0, 0);
        var b = Rect(5, 5);
        var f = new Fixture(a, b);
        f.Select(a, b);

        f.State.Edit(HistoryStepType.LockShapes, document => document.SetLocked(new HashSet<Guid> { a.Id }, true));
        f.Select(a, b);

        Assert.False(f.Editor.CanBoolean);
        Assert.False(f.Editor.CanCombine);
    }

    [Fact]
    public void CanBreakApart_CompoundPathSelected_IsTrueAndSinglePathIsFalse()
    {
        var compound = Compound();
        var single = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]);
        var f = new Fixture(compound, single);

        f.Select(single);
        Assert.False(f.Editor.CanBreakApart);

        f.Select(compound);
        Assert.True(f.Editor.CanBreakApart);
    }

    [Theory]
    [InlineData(PathOperationType.Union, HistoryStepType.Union)]
    [InlineData(PathOperationType.Difference, HistoryStepType.Difference)]
    [InlineData(PathOperationType.Intersection, HistoryStepType.Intersection)]
    [InlineData(PathOperationType.Exclusion, HistoryStepType.Exclusion)]
    public async Task BooleanAsync_Success_IsOneStepWithTheMatchingNameAndSelectsTheResult(PathOperationType type, HistoryStepType step)
    {
        var a = Rect(0, 0, 100);
        var b = Rect(50, 50, 100);
        var f = new Fixture(a, b);
        f.Select(a, b);
        var steps = f.State.History.Steps.Count;

        await f.Editor.BooleanAsync(type);

        Assert.Equal(steps + 1, f.State.History.Steps.Count);
        Assert.Equal(step, f.State.History.Steps[^1]);
        var result = Assert.IsType<PathShape>(Assert.Single(f.Shapes));
        Assert.Equal([result.Id], f.State.SelectedShapeIds.ToList());
        Assert.Empty(f.Messages.Errors);
    }

    [Fact]
    public async Task BooleanAsync_ThenUndo_RestoresOperandsAndSelection()
    {
        var a = Rect(0, 0, 100);
        var b = Rect(50, 50, 100);
        var f = new Fixture(a, b);
        f.Select(a, b);

        await f.Editor.BooleanAsync(PathOperationType.Union);
        f.State.Undo();

        Assert.Equal([a.Id, b.Id], f.Shapes.Select(s => s.Id));
        Assert.Equal([a.Id, b.Id], f.State.SelectedShapeIds.OrderBy(id => id == a.Id ? 0 : 1).ToList());
    }

    [Fact]
    public async Task BooleanAsync_ThenUndoAndRedo_ReselectsTheResult()
    {
        var a = Rect(0, 0, 100);
        var b = Rect(50, 50, 100);
        var f = new Fixture(a, b);
        f.Select(a, b);

        await f.Editor.BooleanAsync(PathOperationType.Union);
        var resultId = Assert.Single(f.Shapes).Id;
        f.State.Undo();
        f.State.Redo();

        Assert.Equal([resultId], f.State.SelectedShapeIds.ToList());
    }

    [Fact]
    public async Task BooleanAsync_EmptyResult_ChangesNothingAndShowsTheMessage()
    {
        var a = Rect(0, 0);
        var b = Rect(100, 100);
        var f = new Fixture(a, b);
        f.Select(a, b);
        var steps = f.State.History.Steps.Count;

        await f.Editor.BooleanAsync(PathOperationType.Intersection);

        Assert.Equal(steps, f.State.History.Steps.Count);
        Assert.Equal([a.Id, b.Id], f.Shapes.Select(s => s.Id));
        Assert.Equal([a.Id, b.Id], f.State.SelectedShapeIds.ToList());
        Assert.Equal(["The operation left nothing, so nothing was changed."], f.Messages.Errors);
    }

    [Fact]
    public async Task BooleanAsync_WhenItCannotRun_DoesNothingAndShowsNoMessage()
    {
        var a = Rect(0, 0);
        var f = new Fixture(a, Rect(5, 5));
        f.Select(a);
        var steps = f.State.History.Steps.Count;

        await f.Editor.BooleanAsync(PathOperationType.Union);

        Assert.Equal(steps, f.State.History.Steps.Count);
        Assert.Empty(f.Messages.Errors);
    }

    [Fact]
    public async Task CombineAsync_TwoShapes_IsOneCombineStepAndSelectsTheResult()
    {
        var a = Rect(0, 0);
        var b = Rect(50, 0);
        var f = new Fixture(a, b);
        f.Select(a, b);

        await f.Editor.CombineAsync();

        Assert.Equal(HistoryStepType.Combine, f.State.History.Steps[^1]);
        var result = Assert.IsType<PathShape>(Assert.Single(f.Shapes));
        Assert.Equal(2, result.SubPaths.Length);
        Assert.Equal([result.Id], f.State.SelectedShapeIds.ToList());
    }

    [Fact]
    public async Task BreakApartAsync_CompoundPath_IsOneStepAndSelectsThePieces()
    {
        var compound = Compound();
        var f = new Fixture(compound);
        f.Select(compound);
        var steps = f.State.History.Steps.Count;

        await f.Editor.BreakApartAsync();

        Assert.Equal(steps + 1, f.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.BreakApart, f.State.History.Steps[^1]);
        Assert.Equal(2, f.Shapes.Count);
        Assert.Equal(f.Shapes.Select(s => s.Id), f.State.SelectedShapeIds.ToList());
    }

    [Fact]
    public async Task BreakApartAsync_TwoCompoundPathsAndAPlainOne_SplitsOnlyTheCompoundOnes()
    {
        var first = Compound();
        var second = Compound();
        var plain = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]);
        var f = new Fixture(first, second, plain);
        f.Select(first, second, plain);

        await f.Editor.BreakApartAsync();

        Assert.Equal(5, f.Shapes.Count);
        Assert.Contains(f.Shapes, shape => shape.Id == plain.Id);
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        var state = new EditorState(Document.Create("x"));

        Assert.Throws<ArgumentNullException>(() => new PathEditor(null!, new FakeMessageDialogs(), FakeLocalizer.Create()));
        Assert.Throws<ArgumentNullException>(() => new PathEditor(state, null!, FakeLocalizer.Create()));
        Assert.Throws<ArgumentNullException>(() => new PathEditor(state, new FakeMessageDialogs(), null!));
    }
}
