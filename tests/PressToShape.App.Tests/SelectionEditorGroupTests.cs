using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class SelectionEditorGroupTests
{
    private static readonly Color Red = new(255, 0, 0, 255);
    private static readonly Color Blue = new(0, 0, 255, 255);

    private static RectangleShape Rect(double x, double rotation = 0, Color? fill = null) =>
        new() { X = x, Y = 0, Width = 10, Height = 10, Rotation = rotation, Appearance = Appearance.Default with { Fill = fill ?? Red } };

    private static GroupShape Group(params Shape[] children) => new() { Children = [.. children] };

    private static (EditorState State, SelectionEditor Editor) Create(Shape[] shapes, params Guid[] selected)
    {
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, shapes));
        state.SetSelection(selected);
        var counter = 1000;
        return (state, new SelectionEditor(state, () => new Guid(counter++, 0, 0, new byte[8])));
    }

    private static IReadOnlyList<Shape> Leaves(EditorState state) =>
        state.Document.Layers.SelectMany(layer => layer.Shapes).SelectMany(shape => shape.Leaves()).ToList();

    private static Shape Find(EditorState state, Guid id) => state.Document.FindShape(id)!.Shape;

    [Fact]
    public void Nudge_Group_MovesAllLeavesAsOneStep()
    {
        var a = Rect(0);
        var b = Rect(20);
        var group = Group(a, b);
        var (state, editor) = Create([group], group.Id);
        var steps = state.History.Steps.Count;

        editor.Nudge(5, 3);

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(5, Find(state, a.Id).X);
        Assert.Equal(25, Find(state, b.Id).X);
        Assert.Equal(3, Find(state, a.Id).Y);
        Assert.Equal(new Bounds(5, 3, 30, 10), Find(state, group.Id).Bounds);
        Assert.Equal(group.Id, state.SelectedShapeIds.Single());
    }

    [Fact]
    public void Apply_Group_ChangesTheShapesBelowAndNotTheGroup()
    {
        var a = Rect(0);
        var b = Rect(20);
        var group = Group(a, b);
        var (state, editor) = Create([group], group.Id);

        editor.Apply(HistoryStepType.ChangeFill, shape => shape with { Appearance = shape.Appearance with { Fill = Blue } });

        Assert.Equal(Blue, Find(state, a.Id).Appearance.Fill);
        Assert.Equal(Blue, Find(state, b.Id).Appearance.Fill);
        Assert.Equal(Appearance.Default, Find(state, group.Id).Appearance);
        Assert.Equal(HistoryStepType.ChangeFill, state.History.Steps[^1]);
    }

    [Fact]
    public void Apply_GroupAndPlainShape_ChangesAllLeaves()
    {
        var a = Rect(0);
        var plain = Rect(50);
        var group = Group(a);
        var (state, editor) = Create([group, plain], group.Id, plain.Id);

        editor.Apply(HistoryStepType.ChangeFill, shape => shape with { Appearance = shape.Appearance with { Fill = Blue } });

        Assert.All(Leaves(state), leaf => Assert.Equal(Blue, leaf.Appearance.Fill));
    }

    [Fact]
    public void ApplyTransformWithFrame_ResizeGroup_ScalesLeavesAndKeepsTheLayout()
    {
        var a = Rect(0);
        var b = Rect(20);
        var group = Group(a, b);
        var (state, editor) = Create([group], group.Id);
        var steps = state.History.Steps.Count;

        editor.ApplyTransformWithFrame(HistoryStepType.ResizeShapes, (leaves, frame) =>
            SelectionTransforms.Resize(leaves, frame, HandleType.Right, 60, 5, keepProportions: false, fromCenter: false));

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal((0.0, 20.0), (Find(state, a.Id).X, Find(state, a.Id).Width));
        Assert.Equal((40.0, 20.0), (Find(state, b.Id).X, Find(state, b.Id).Width));
        Assert.Equal(10, Find(state, a.Id).Height);
        Assert.Equal(new Bounds(0, 0, 60, 10), Find(state, group.Id).Bounds);
    }

    [Fact]
    public void ApplyTransformWithFrame_ResizeGroupWithTurnedLeaf_ScalesUniformly()
    {
        var turned = Rect(0, rotation: 45);
        var other = Rect(40);
        var group = Group(turned, other);
        var (state, editor) = Create([group], group.Id);
        var before = Find(state, group.Id).Bounds;

        editor.ApplyTransformWithFrame(HistoryStepType.ResizeShapes, (leaves, frame) =>
            SelectionTransforms.Resize(leaves, frame, HandleType.Right, before.X + before.Width * 2, 0, keepProportions: false, fromCenter: false));

        var after = Find(state, group.Id).Bounds;
        Assert.Equal(before.Width * 2, after.Width, 6);
        Assert.Equal(before.Height * 2, after.Height, 6);
        Assert.Equal(20, Find(state, other.Id).Width, 6);
        Assert.Equal(20, Find(state, other.Id).Height, 6);
    }

    [Fact]
    public void ApplyTransformWithFrame_RotateGroup_TurnsTheLeavesAndTheGroupStaysUpright()
    {
        var a = Rect(0);
        var b = Rect(20);
        var group = Group(a, b);
        var (state, editor) = Create([group], group.Id);

        editor.ApplyTransformWithFrame(HistoryStepType.RotateShapes, (leaves, frame) =>
            SelectionTransforms.Rotate(leaves, frame, 15, -20, 40, 5, snap: false));

        Assert.Equal(90, Find(state, a.Id).Rotation, 6);
        Assert.Equal(90, Find(state, b.Id).Rotation, 6);
        Assert.Equal(0, Find(state, group.Id).Rotation);
        Assert.Equal(15, Find(state, group.Id).Bounds.X + Find(state, group.Id).Bounds.Width / 2, 6);
    }

    [Fact]
    public void ApplyTransform_ReturningOtherShapes_Throws()
    {
        var group = Group(Rect(0), Rect(20));
        var (_, editor) = Create([group], group.Id);

        Assert.Throws<InvalidOperationException>(() => editor.ApplyTransform(HistoryStepType.ResizeShapes, leaves => [leaves[0]]));
    }

    [Fact]
    public void FrameOf_GroupWithOneTurnedLeaf_IsUprightAndNotSingle()
    {
        var group = Group(Rect(0, rotation: 30));

        var frame = SelectionGeometry.FrameOf([group]);

        Assert.False(frame!.IsSingle);
        Assert.Equal(0, frame.Rotation);
        Assert.Equal(group.RotatedBounds.Width, frame.Width, 6);
    }

    [Fact]
    public void FrameOf_SinglePlainTurnedShape_StaysSingle()
    {
        Assert.True(SelectionGeometry.FrameOf([Rect(0, rotation: 30)])!.IsSingle);
    }

    [Fact]
    public void SelectionValues_Group_GivesFrameAndMixedAppearanceOfTheLeaves()
    {
        var group = Group(Rect(0, fill: Red), Rect(20, fill: Blue));

        var values = SelectionValues.From([group])!;

        Assert.Equal(new[] { 0.0, 0.0, 30.0, 10.0, 0.0 }, new[] { values.X, values.Y, values.Width, values.Height, values.Rotation });
        Assert.False(values.IsSingle);
        Assert.True(values.Fill.IsMixed);
        Assert.False(values.Opacity.IsMixed);
    }

    [Fact]
    public void Paste_CopiedGroup_GetsNewIdsForTheWholeSubtree()
    {
        var a = Rect(0);
        var inner = Group(Rect(20));
        var group = Group(a, inner);
        var (state, editor) = Create([group], group.Id);
        var originalIds = new Shape[] { group, a, inner }.Select(s => s.Id).Concat(inner.Children.Select(c => c.Id)).ToHashSet();

        editor.Copy();
        editor.Paste(inPlace: false);

        var shapes = state.Document.Layers[0].Shapes;
        Assert.Equal(2, shapes.Length);
        var copy = Assert.IsType<GroupShape>(shapes[1]);
        var copyIds = new[] { copy.Id }.Concat(copy.Children.Select(c => c.Id)).Concat(((GroupShape)copy.Children[1]).Children.Select(c => c.Id)).ToList();
        Assert.Equal(4, copyIds.Distinct().Count());
        Assert.DoesNotContain(copyIds, originalIds.Contains);
        Assert.Equal(new Bounds(10, 10, 30, 10), copy.Bounds);
        Assert.Equal(copy.Id, state.SelectedShapeIds.Single());
    }

    [Fact]
    public void Paste_InPlace_KeepsPosition()
    {
        var group = Group(Rect(0), Rect(20));
        var (state, editor) = Create([group], group.Id);

        editor.Copy();
        editor.Paste(inPlace: true);

        Assert.Equal(group.Bounds, state.Document.Layers[0].Shapes[1].Bounds);
    }

    [Fact]
    public void Duplicate_Group_AddsAMovedCopyWithNewIds()
    {
        var group = Group(Rect(0), Rect(20));
        var (state, editor) = Create([group], group.Id);

        editor.Duplicate();

        var shapes = state.Document.Layers[0].Shapes;
        Assert.Equal(2, shapes.Length);
        Assert.NotEqual(group.Id, shapes[1].Id);
        Assert.Equal(new Bounds(10, 10, 30, 10), shapes[1].Bounds);
        Assert.Equal(HistoryStepType.DuplicateShapes, state.History.Steps[^1]);
    }

    [Fact]
    public void Duplicate_MemberOfAnEnteredGroup_LandsInTheSameGroup()
    {
        var a = Rect(0);
        var group = Group(a, Rect(20));
        var (state, editor) = Create([group]);
        state.EnterGroup(group.Id);
        state.SelectOnly(a.Id);

        editor.Duplicate();

        var changed = (GroupShape)state.Document.Layers[0].Shapes.Single();
        Assert.Equal(3, changed.Children.Length);
        Assert.Equal(group.Id, changed.Id);
        Assert.Equal(10, changed.Children[2].X);
    }

    [Fact]
    public void Cut_GroupMembers_RemovesTheGroupWhenItBecomesEmpty()
    {
        var a = Rect(0);
        var b = Rect(20);
        var group = Group(a, b);
        var (state, editor) = Create([group, Rect(100)]);
        state.EnterGroup(group.Id);
        state.SetSelection([a.Id, b.Id]);

        editor.Cut();

        Assert.Single(state.Document.Layers[0].Shapes);
        Assert.Null(state.Document.FindShape(group.Id));
        Assert.Equal(2, state.Clipboard.Shapes.Count);
    }

    [Fact]
    public void Cut_Group_PutsTheWholeGroupOnTheClipboard()
    {
        var group = Group(Rect(0), Rect(20));
        var (state, editor) = Create([group], group.Id);

        editor.Cut();

        Assert.Empty(state.Document.Layers[0].Shapes);
        Assert.IsType<GroupShape>(Assert.Single(state.Clipboard.Shapes));
    }
}
