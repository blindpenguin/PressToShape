using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class NodeEditorTests
{
    private static PathShape Row(double rotation = 0)
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 100),
                new DocumentNode(100, 100, inX: 70, inY: 100, outX: 130, outY: 100, type: NodeType.Symmetric),
                new DocumentNode(200, 100),
                new DocumentNode(300, 150),
            ]);
        return path with { Rotation = rotation };
    }

    private static (EditorState State, NodeEditor Editor) Create(params Shape[] shapes)
    {
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, shapes)) { ActiveTool = Tool.Node };
        return (state, new NodeEditor(state));
    }

    private static void Select(EditorState state, Shape shape, params int[] indices)
    {
        state.Select(shape.Id);
        state.SetNodeSelection(indices.Select(i => new NodeRef(shape.Id, i)));
    }

    private static Shape Current(EditorState state, Shape shape) => state.Document.FindShape(shape.Id)!.Shape;

    private static (double X, double Y) OnScreen(PathShape path, int index)
    {
        var node = path.ToDocumentNodes()[index];
        return path.FromLocal(node.X, node.Y);
    }

    [Fact]
    public void SetType_SelectedNode_ChangesItAsOneStep()
    {
        var path = Row();
        var (state, editor) = Create(path);
        Select(state, path, 2);
        var steps = state.History.Steps.Count;

        editor.SetType(NodeType.Smooth);

        var node = ((PathShape)Current(state, path)).Nodes[2];
        Assert.Equal(NodeType.Smooth, node.Type);
        Assert.True(node.HasIn && node.HasOut);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.ChangeNodeType, state.History.Steps[^1]);
    }

    [Fact]
    public void SetType_KeepsTheNodeSelection()
    {
        var path = Row();
        var (state, editor) = Create(path);
        Select(state, path, 2);

        editor.SetType(NodeType.Smooth);

        Assert.Equal([new NodeRef(path.Id, 2)], state.SelectedNodes);
    }

    [Fact]
    public void SetType_OnARectangleCorner_ConvertsItToAPathInTheSameStep()
    {
        var rectangle = new RectangleShape { X = 10, Y = 10, Width = 100, Height = 60, Name = "Box" };
        var (state, editor) = Create(rectangle);
        Select(state, rectangle, 0);
        var steps = state.History.Steps.Count;

        editor.SetType(NodeType.Smooth);

        var path = Assert.IsType<PathShape>(Current(state, rectangle));
        Assert.Equal(rectangle.Id, path.Id);
        Assert.Equal("Box", path.Name);
        Assert.Equal(NodeType.Smooth, path.Nodes[0].Type);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        state.Undo();
        Assert.IsType<RectangleShape>(Current(state, rectangle));
    }

    [Fact]
    public void SetType_ThatChangesNothingOnARectangle_DoesNotConvertIt()
    {
        var rectangle = new RectangleShape { Width = 100, Height = 60 };
        var (state, editor) = Create(rectangle);
        Select(state, rectangle, 0, 1);
        var steps = state.History.Steps.Count;

        editor.SetType(NodeType.Corner);

        Assert.IsType<RectangleShape>(Current(state, rectangle));
        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void SetType_OnSeveralPaths_ChangesAllOfThemInOneStep()
    {
        var first = Row();
        var second = Row();
        var (state, editor) = Create(first, second);
        state.Select(first.Id);
        state.SetNodeSelection([new NodeRef(first.Id, 2), new NodeRef(second.Id, 3)]);
        var steps = state.History.Steps.Count;

        editor.SetType(NodeType.Smooth);

        Assert.Equal(NodeType.Smooth, ((PathShape)Current(state, first)).Nodes[2].Type);
        Assert.Equal(NodeType.Smooth, ((PathShape)Current(state, second)).Nodes[3].Type);
        Assert.Equal(steps + 1, state.History.Steps.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(90)]
    public void SetType_RotatedPath_KeepsEveryAnchorOnTheScreen(double rotation)
    {
        var path = Row(rotation);
        var (state, editor) = Create(path);
        Select(state, path, 2);
        var before = Enumerable.Range(0, 4).Select(i => OnScreen(path, i)).ToList();

        editor.SetType(NodeType.Symmetric);

        var after = (PathShape)Current(state, path);
        for (var i = 0; i < 4; i++)
        {
            Assert.Equal(before[i].X, OnScreen(after, i).X, 6);
            Assert.Equal(before[i].Y, OnScreen(after, i).Y, 6);
        }
    }

    [Fact]
    public void SetType_ThenUndo_RestoresThePath()
    {
        var path = Row();
        var (state, editor) = Create(path);
        Select(state, path, 2);

        editor.SetType(NodeType.Smooth);
        state.Undo();

        Assert.Equal(NodeType.Corner, ((PathShape)Current(state, path)).Nodes[2].Type);
    }

    [Fact]
    public void CanChangeSegment_TwoNeighboringNodes_IsTrue()
    {
        var path = Row();
        var (state, editor) = Create(path);

        Select(state, path, 1, 2);
        Assert.True(editor.CanChangeSegment);

        Select(state, path, 2, 1);
        Assert.True(editor.CanChangeSegment);
    }

    [Fact]
    public void CanChangeSegment_OneNodeNonNeighborsOrThreeNodes_IsFalse()
    {
        var path = Row();
        var (state, editor) = Create(path);

        Select(state, path, 1);
        Assert.False(editor.CanChangeSegment);
        Select(state, path, 0, 2);
        Assert.False(editor.CanChangeSegment);
        Select(state, path, 0, 1, 2);
        Assert.False(editor.CanChangeSegment);
        Select(state, path, 0, 3);
        Assert.False(editor.CanChangeSegment);
    }

    [Fact]
    public void CanChangeSegment_FirstAndLastNodeOfAClosedPath_IsTrue()
    {
        var closed = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100)], isClosed: true);
        var (state, editor) = Create(closed);

        Select(state, closed, 0, 2);

        Assert.True(editor.CanChangeSegment);
    }

    [Fact]
    public void StraightenSelectedSegment_RemovesItsHandlesAsOneStep()
    {
        var path = Row();
        var (state, editor) = Create(path);
        Select(state, path, 1, 2);
        var steps = state.History.Steps.Count;

        editor.StraightenSelectedSegment();

        var nodes = ((PathShape)Current(state, path)).Nodes;
        Assert.False(nodes[1].HasOut);
        Assert.Equal(NodeType.Corner, nodes[1].Type);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.ChangeSegment, state.History.Steps[^1]);
    }

    [Fact]
    public void CurveSelectedSegment_GivesAStraightSegmentHandlesAsOneStep()
    {
        var path = Row();
        var (state, editor) = Create(path);
        Select(state, path, 2, 3);
        var steps = state.History.Steps.Count;

        editor.CurveSelectedSegment();

        var nodes = ((PathShape)Current(state, path)).Nodes;
        Assert.True(nodes[2].HasOut);
        Assert.True(nodes[3].HasIn);
        Assert.Equal(steps + 1, state.History.Steps.Count);
    }

    [Fact]
    public void SegmentCommands_WithoutASelectedSegment_DoNothing()
    {
        var path = Row();
        var (state, editor) = Create(path);
        Select(state, path, 0);
        var steps = state.History.Steps.Count;

        editor.StraightenSelectedSegment();
        editor.CurveSelectedSegment();

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void CurveSelectedSegment_OnAStraightRectangleSide_ConvertsAndCurvesInOneStep()
    {
        var rectangle = new RectangleShape { Width = 100, Height = 60 };
        var (state, editor) = Create(rectangle);
        Select(state, rectangle, 0, 1);
        var steps = state.History.Steps.Count;

        editor.CurveSelectedSegment();

        var path = Assert.IsType<PathShape>(Current(state, rectangle));
        Assert.True(path.Nodes[0].HasOut);
        Assert.Equal(steps + 1, state.History.Steps.Count);
    }

    [Fact]
    public void DeleteSelected_RemovesTheNodesAsOneStepAndClearsTheNodeSelection()
    {
        var path = Row();
        var (state, editor) = Create(path);
        Select(state, path, 1, 2);
        var steps = state.History.Steps.Count;

        editor.DeleteSelected();

        Assert.Equal(2, ((PathShape)Current(state, path)).Nodes.Length);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.DeletePathPoints, state.History.Steps[^1]);
        Assert.Empty(state.SelectedNodes);
        Assert.Equal(path.Id, state.SelectedShapeId);
    }

    [Fact]
    public void DeleteSelected_DownToOneNode_DeletesTheShape()
    {
        var path = Row();
        var (state, editor) = Create(path);
        Select(state, path, 0, 1, 2);

        editor.DeleteSelected();

        Assert.Empty(state.Document.Layers.SelectMany(l => l.Shapes));
        Assert.False(state.HasSelection);
        Assert.Empty(state.SelectedNodes);
    }

    [Fact]
    public void DeleteSelected_OnARectangleCorner_LeavesATrianglePath()
    {
        var rectangle = new RectangleShape { Width = 100, Height = 60 };
        var (state, editor) = Create(rectangle);
        Select(state, rectangle, 0);

        editor.DeleteSelected();

        var path = Assert.IsType<PathShape>(Current(state, rectangle));
        Assert.Equal(3, path.Nodes.Length);
        Assert.True(path.IsClosed);
    }

    [Fact]
    public void DeleteSelected_ThenUndo_BringsTheNodesBack()
    {
        var path = Row();
        var (state, editor) = Create(path);
        Select(state, path, 1);

        editor.DeleteSelected();
        state.Undo();

        Assert.Equal(4, ((PathShape)Current(state, path)).Nodes.Length);
    }

    [Fact]
    public void Nudge_MovesTheSelectedNodesOnTheScreen()
    {
        var path = Row();
        var (state, editor) = Create(path);
        Select(state, path, 0, 3);

        editor.Nudge(5, -3);

        var after = (PathShape)Current(state, path);
        Assert.Equal(5, OnScreen(after, 0).X - OnScreen(path, 0).X, 6);
        Assert.Equal(-3, OnScreen(after, 3).Y - OnScreen(path, 3).Y, 6);
        Assert.Equal(OnScreen(path, 1).X, OnScreen(after, 1).X, 6);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(90)]
    public void Nudge_RotatedPath_MovesTheNodeByTheDistanceOnTheScreen(double rotation)
    {
        var path = Row(rotation);
        var (state, editor) = Create(path);
        Select(state, path, 1);
        var before = OnScreen(path, 1);

        editor.Nudge(10, 0);

        var after = OnScreen((PathShape)Current(state, path), 1);
        Assert.Equal(before.X + 10, after.X, 6);
        Assert.Equal(before.Y, after.Y, 6);
    }

    [Fact]
    public void Nudge_Repeated_IsOneStep()
    {
        var path = Row();
        var (state, editor) = Create(path);
        Select(state, path, 1);
        var steps = state.History.Steps.Count;

        editor.Nudge(1, 0);
        editor.Nudge(1, 0);
        editor.Nudge(1, 0);

        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(HistoryStepType.MovePathPoints, state.History.Steps[^1]);
    }

    [Fact]
    public void Nudge_AfterAnotherEdit_StartsANewStep()
    {
        var path = Row();
        var (state, editor) = Create(path);
        Select(state, path, 1);
        editor.Nudge(1, 0);
        var steps = state.History.Steps.Count;

        editor.SetType(NodeType.Corner);
        editor.Nudge(1, 0);

        Assert.True(state.History.Steps.Count > steps);
    }

    [Fact]
    public void Nudge_NothingSelectedOrNoDistance_DoesNothing()
    {
        var path = Row();
        var (state, editor) = Create(path);
        var steps = state.History.Steps.Count;

        editor.Nudge(5, 5);
        Select(state, path, 1);
        editor.Nudge(0, 0);

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void Nudge_OnAnEllipse_ConvertsItAndMovesTheNode()
    {
        var ellipse = new EllipseShape { X = 0, Y = 0, Width = 100, Height = 100 };
        var (state, editor) = Create(ellipse);
        Select(state, ellipse, 0);

        editor.Nudge(0, -20);

        var path = Assert.IsType<PathShape>(Current(state, ellipse));
        // The top node moved up by 20, so the box grew upward.
        Assert.Equal(-20, path.Y, 6);
        Assert.Equal(120, path.Height, 6);
    }

    [Fact]
    public void SetPosition_OneNode_MovesItToThePositionOnTheScreen()
    {
        var path = Row(30);
        var (state, editor) = Create(path);
        Select(state, path, 2);

        editor.SetPosition(250, 80);

        var position = OnScreen((PathShape)Current(state, path), 2);
        Assert.Equal(250, position.X, 6);
        Assert.Equal(80, position.Y, 6);
    }

    [Fact]
    public void SetPosition_OneCoordinateOnly_KeepsTheOther()
    {
        var path = Row();
        var (state, editor) = Create(path);
        Select(state, path, 2);
        var before = OnScreen(path, 2);

        editor.SetPosition(null, 60);

        var after = OnScreen((PathShape)Current(state, path), 2);
        Assert.Equal(before.X, after.X, 6);
        Assert.Equal(60, after.Y, 6);
    }

    [Fact]
    public void SetPosition_NoneOrSeveralNodes_DoesNothing()
    {
        var path = Row();
        var (state, editor) = Create(path);
        var steps = state.History.Steps.Count;

        Select(state, path);
        editor.SetPosition(1, 1);
        Select(state, path, 1, 2);
        editor.SetPosition(1, 1);

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void LockedOrHiddenShapes_AreNotEdited()
    {
        var locked = Row() with { IsLocked = true };
        var hidden = Row() with { IsVisible = false };
        var (state, editor) = Create(locked, hidden);
        state.SetNodeSelection([new NodeRef(locked.Id, 1), new NodeRef(hidden.Id, 1)]);
        var steps = state.History.Steps.Count;

        editor.SetType(NodeType.Corner);
        editor.Nudge(5, 5);
        editor.DeleteSelected();

        Assert.False(editor.HasEditableNodes);
        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void Operations_WithoutSelectedNodes_DoNothing()
    {
        var path = Row();
        var (state, editor) = Create(path);
        state.Select(path.Id);
        var steps = state.History.Steps.Count;

        editor.SetType(NodeType.Smooth);
        editor.DeleteSelected();
        editor.StraightenSelectedSegment();

        Assert.Equal(steps, state.History.Steps.Count);
        Assert.False(editor.HasEditableNodes);
    }

    [Fact]
    public void TypeOfSelection_SameMixedAndNone()
    {
        var path = Row();
        var (state, editor) = Create(path);

        Assert.Null(editor.TypeOfSelection);

        Select(state, path, 0, 2);
        Assert.Equal(NodeType.Corner, editor.TypeOfSelection);

        Select(state, path, 0, 1);
        Assert.Null(editor.TypeOfSelection);

        Select(state, path, 1);
        Assert.Equal(NodeType.Symmetric, editor.TypeOfSelection);
    }

    [Fact]
    public void SingleNodeAndItsPosition_OnlyForExactlyOneNode()
    {
        var path = Row();
        var (state, editor) = Create(path);

        Select(state, path, 2);
        Assert.Equal(new NodeRef(path.Id, 2), editor.SingleNode);
        Assert.Equal(200, editor.PositionOfSingleNode!.Value.X, 6);

        Select(state, path, 1, 2);
        Assert.Null(editor.SingleNode);
        Assert.Null(editor.PositionOfSingleNode);
    }

    [Fact]
    public void SelectedNodeIndicesBeyondThePath_AreIgnored()
    {
        var path = Row();
        var (state, editor) = Create(path);
        state.Select(path.Id);
        state.SetNodeSelection([new NodeRef(path.Id, 9)]);

        Assert.False(editor.HasEditableNodes);
    }
}
