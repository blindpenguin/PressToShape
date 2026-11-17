using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CompoundPathNodeToolTests
{
    private static readonly Guid LayerId = new("aaaaaaaa-0000-0000-0000-000000000001");

    // Sub-path 0: a closed square (flat nodes 0 to 3). Sub-path 1: an open line with two nodes (flat nodes 4 and 5) beside it.
    private static PathShape Compound() =>
        PathShape.Create(
            [
                new DocumentSubPath([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100), new DocumentNode(0, 100)], true),
                new DocumentSubPath([new DocumentNode(200, 0), new DocumentNode(300, 0)], false),
            ]);

    private static (EditorState State, NodeEditor Editor, PathShape Path) CreateNodeTool()
    {
        var path = Compound();
        var document = (Document.Create("x") with { Layers = [new Layer { Id = LayerId, Name = "A" }] }).AddShape(LayerId, path);
        var state = new EditorState(document) { ActiveTool = Tool.Node };
        return (state, new NodeEditor(state), path);
    }

    private static void Select(EditorState state, PathShape path, params int[] indices)
    {
        state.Select(path.Id);
        state.SetNodeSelection(indices.Select(i => new NodeRef(path.Id, i)));
    }

    private static PathShape Current(EditorState state, PathShape path) => Assert.IsType<PathShape>(state.Document.FindShape(path.Id)!.Shape);

    [Fact]
    public void SetType_NodesOfTwoSubPaths_ChangesBothInOneStep()
    {
        var (state, editor, path) = CreateNodeTool();
        Select(state, path, 1, 4);
        var steps = state.History.Steps.Count;

        editor.SetType(NodeType.Smooth);

        var result = Current(state, path);
        Assert.Equal(NodeType.Smooth, result.SubPaths[0].Nodes[1].Type);
        Assert.Equal(NodeType.Smooth, result.SubPaths[1].Nodes[0].Type);
        Assert.Equal(steps + 1, state.History.Steps.Count);
    }

    [Fact]
    public void Nudge_NodeOfSecondSubPath_MovesOnlyThatNode()
    {
        var (state, editor, path) = CreateNodeTool();
        Select(state, path, 5);

        editor.Nudge(0, 40);

        var result = Current(state, path);
        Assert.Equal(40, PathNodeEditing.NodePosition(result, 5).Y, 6);
        Assert.Equal(0, PathNodeEditing.NodePosition(result, 4).Y, 6);
        Assert.Equal(0, PathNodeEditing.NodePosition(result, 0).Y, 6);
        Assert.Equal(100, PathNodeEditing.NodePosition(result, 2).Y, 6);
    }

    [Fact]
    public void DeleteSelected_NodeOfFirstSubPath_KeepsBothSubPaths()
    {
        var (state, editor, path) = CreateNodeTool();
        Select(state, path, 1);

        editor.DeleteSelected();

        var result = Current(state, path);
        Assert.Equal(3, result.SubPaths[0].Nodes.Length);
        Assert.Equal(2, result.SubPaths[1].Nodes.Length);
    }

    [Fact]
    public void DeleteSelected_NodeOfATwoNodeSubPath_RemovesThatSubPath()
    {
        var (state, editor, path) = CreateNodeTool();
        Select(state, path, 4);

        editor.DeleteSelected();

        var result = Current(state, path);
        Assert.Single(result.SubPaths);
        Assert.Equal(4, result.SubPaths[0].Nodes.Length);
    }

    [Fact]
    public void DeleteSelected_ThenUndo_BringsTheSubPathBack()
    {
        var (state, editor, path) = CreateNodeTool();
        Select(state, path, 4);

        editor.DeleteSelected();
        state.Undo();

        Assert.Equal(2, Current(state, path).SubPaths.Length);
    }

    [Fact]
    public void CanChangeSegment_NeighborsInOneSubPath_IsTrue()
    {
        var (state, editor, path) = CreateNodeTool();
        Select(state, path, 4, 5);

        Assert.True(editor.CanChangeSegment);
    }

    [Fact]
    public void CanChangeSegment_LastNodeOfOneSubPathAndFirstOfTheNext_IsFalse()
    {
        var (state, editor, path) = CreateNodeTool();
        Select(state, path, 3, 4);

        Assert.False(editor.CanChangeSegment);
    }

    [Fact]
    public void CanChangeSegment_ClosingSegmentOfAClosedSubPath_IsTrue()
    {
        var (state, editor, path) = CreateNodeTool();
        Select(state, path, 0, 3);

        Assert.True(editor.CanChangeSegment);
    }

    [Fact]
    public void CurveSelectedSegment_SegmentOfSecondSubPath_GivesThatSegmentHandles()
    {
        var (state, editor, path) = CreateNodeTool();
        Select(state, path, 4, 5);

        editor.CurveSelectedSegment();

        var result = Current(state, path);
        Assert.True(result.SubPaths[1].Nodes[0].HasOut);
        Assert.True(result.SubPaths[1].Nodes[1].HasIn);
        Assert.All(result.SubPaths[0].Nodes, node => Assert.False(node.HasIn || node.HasOut));
    }

    [Fact]
    public void VisibleHandles_NodeAtTheEndOfFirstSubPath_DoesNotReachIntoTheNext()
    {
        var path = PathShape.Create(
            [
                new DocumentSubPath([new DocumentNode(0, 0, outX: 10, outY: 10), new DocumentNode(100, 0, inX: 90, inY: 10)], false),
                new DocumentSubPath([new DocumentNode(200, 0, inX: 190, inY: 10, outX: 210, outY: 10), new DocumentNode(300, 0)], false),
            ]);

        var handles = NodeHits.VisibleHandles(path, new System.Collections.Generic.HashSet<int> { 1 });

        Assert.Contains((1, NodeHandleSide.In), handles);
        Assert.Contains((0, NodeHandleSide.Out), handles);
        Assert.DoesNotContain(handles, handle => handle.Index >= 2);
    }

    [Fact]
    public void PenContinue_OnTheEndOfSecondSubPath_AppendsThereAndKeepsTheFirstSubPath()
    {
        var path = Compound();
        var document = (Document.Create("ignored") with { Layers = [new Layer { Id = LayerId, Name = "A" }] }).AddShape(LayerId, path);
        var state = new EditorState(document) { ActiveTool = Tool.Pen };
        state.Select(path.Id);
        var interaction = new CanvasInteraction(state);

        interaction.PointerPressed(300, 0, inputType: InputPointerType.Mouse);
        interaction.PointerReleased(300, 0);
        interaction.PointerPressed(300, 100, inputType: InputPointerType.Mouse);
        interaction.PointerReleased(300, 100);
        interaction.FinishPen();

        var result = Assert.IsType<PathShape>(Assert.Single(state.Document.Layers.SelectMany(l => l.Shapes)));
        Assert.Equal(path.Id, result.Id);
        Assert.Equal(2, result.SubPaths.Length);
        Assert.Equal(4, result.SubPaths[0].Nodes.Length);
        Assert.True(result.SubPaths[0].IsClosed);
        Assert.Equal(3, result.SubPaths[1].Nodes.Length);
        Assert.False(result.SubPaths[1].IsClosed);
    }

    [Fact]
    public void BuildShape_Ring_HasTwoSubPathsAndTheFillRule()
    {
        var ring = PathShape.Create(
            [
                new DocumentSubPath([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(100, 100), new DocumentNode(0, 100)], true),
                new DocumentSubPath([new DocumentNode(25, 25), new DocumentNode(75, 25), new DocumentNode(75, 75), new DocumentNode(25, 75)], true),
            ],
            FillRule.EvenOdd);

        var screen = CanvasScene.BuildShape(ring, 100, 0, 0);

        Assert.Equal(2, screen.Path!.SubPaths.Length);
        Assert.Equal(FillRule.EvenOdd, screen.Path.FillRule);
        Assert.Equal(2, PathOutline.Commands(screen.Path, 0, 0).Count(command => command.Type == PathCommandType.Move));
        Assert.Equal(2, PathOutline.Commands(screen.Path, 0, 0).Count(command => command.Type == PathCommandType.Close));
    }
}
