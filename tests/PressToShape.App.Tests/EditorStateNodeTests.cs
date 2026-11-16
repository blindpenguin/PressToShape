using System;
using System.Collections.Generic;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class EditorStateNodeTests
{
    private static PathShape Triangle() =>
        PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(50, 100)], isClosed: true);

    private static (EditorState State, PathShape Path) Create(Tool tool = Tool.Node)
    {
        var path = Triangle();
        var document = Document.Create("x");
        var state = new EditorState(document.AddShape(document.Layers[0].Id, path)) { ActiveTool = tool };
        state.Select(path.Id);
        return (state, path);
    }

    [Fact]
    public void NewState_HasNoSelectedNodes()
    {
        var state = new EditorState();

        Assert.Empty(state.SelectedNodes);
        Assert.False(state.HasSelectedNodes);
    }

    [Fact]
    public void SetNodeSelection_Nodes_AreSelectedOnceEach()
    {
        var (state, path) = Create();

        state.SetNodeSelection([new NodeRef(path.Id, 0), new NodeRef(path.Id, 2), new NodeRef(path.Id, 0)]);

        Assert.Equal(2, state.SelectedNodes.Count);
        Assert.True(state.HasSelectedNodes);
    }

    [Fact]
    public void ToggleNode_TwiceAddsThenRemoves()
    {
        var (state, path) = Create();
        var node = new NodeRef(path.Id, 1);

        state.ToggleNode(node);
        Assert.Contains(node, state.SelectedNodes);

        state.ToggleNode(node);
        Assert.DoesNotContain(node, state.SelectedNodes);
    }

    [Fact]
    public void ClearNodeSelection_RemovesEveryNode()
    {
        var (state, path) = Create();
        state.SetNodeSelection([new NodeRef(path.Id, 0), new NodeRef(path.Id, 1)]);

        state.ClearNodeSelection();

        Assert.Empty(state.SelectedNodes);
    }

    [Fact]
    public void Changes_RaisePropertyChanged()
    {
        var (state, path) = Create();
        var raised = new List<string?>();
        state.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        state.SetNodeSelection([new NodeRef(path.Id, 0)]);

        Assert.Contains(nameof(EditorState.SelectedNodes), raised);
        Assert.Contains(nameof(EditorState.HasSelectedNodes), raised);
    }

    [Fact]
    public void SetNodeSelection_SameNodes_RaisesNothing()
    {
        var (state, path) = Create();
        state.SetNodeSelection([new NodeRef(path.Id, 0)]);
        var raised = new List<string?>();
        state.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        state.SetNodeSelection([new NodeRef(path.Id, 0)]);

        Assert.Empty(raised);
    }

    [Fact]
    public void ChangingTheShapeSelection_ClearsTheNodes()
    {
        var (state, path) = Create();
        state.SetNodeSelection([new NodeRef(path.Id, 0)]);

        state.ClearSelection();

        Assert.Empty(state.SelectedNodes);
    }

    [Fact]
    public void SelectingAnotherShape_ClearsTheNodes()
    {
        var (state, path) = Create();
        var other = new RectangleShape { Width = 5, Height = 5 };
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, other));
        state.Select(path.Id);
        state.SetNodeSelection([new NodeRef(path.Id, 1)]);

        state.Select(other.Id);

        Assert.Empty(state.SelectedNodes);
    }

    [Fact]
    public void ChangingTheTool_ClearsTheNodes()
    {
        var (state, path) = Create();
        state.SetNodeSelection([new NodeRef(path.Id, 0)]);

        state.ActiveTool = Tool.DirectSelect;

        Assert.Empty(state.SelectedNodes);
    }

    [Fact]
    public void SettingTheSameTool_KeepsTheNodes()
    {
        var (state, path) = Create();
        state.SetNodeSelection([new NodeRef(path.Id, 0)]);

        state.ActiveTool = Tool.Node;

        Assert.Single(state.SelectedNodes);
    }

    [Fact]
    public void ReplaceDocument_ClearsTheNodes()
    {
        var (state, path) = Create();
        state.SetNodeSelection([new NodeRef(path.Id, 0)]);

        state.ReplaceDocument(Document.Create("Other"));

        Assert.Empty(state.SelectedNodes);
    }

    [Fact]
    public void AnOrdinaryEdit_KeepsTheNodes()
    {
        var (state, path) = Create();
        state.SetNodeSelection([new NodeRef(path.Id, 0), new NodeRef(path.Id, 2)]);

        state.Edit(HistoryStepType.MovePathPoints, d => d.UpdateShapes(new Dictionary<Guid, Shape> { [path.Id] = path with { X = path.X + 10 } }));

        Assert.Equal(2, state.SelectedNodes.Count);
    }

    [Fact]
    public void UndoOfADeletedNode_DropsTheReferenceToTheNodeThatIsGone()
    {
        var (state, path) = Create();
        var fewer = PathNodeEditing.RemoveNodes(path, [2])!;
        state.Edit(HistoryStepType.DeletePathPoints, d => d.UpdateShapes(new Dictionary<Guid, Shape> { [path.Id] = fewer }));
        state.SetNodeSelection([new NodeRef(path.Id, 0), new NodeRef(path.Id, 1)]);
        // A redo-like state: the document comes back to three nodes and then goes forward again to two, so index 2 must not stay.
        state.Undo();
        state.SetNodeSelection([new NodeRef(path.Id, 2)]);

        state.Redo();

        Assert.Empty(state.SelectedNodes);
    }

    [Fact]
    public void UndoOfADeletedShape_DropsTheReferenceToTheShape()
    {
        var (state, path) = Create();
        state.SetNodeSelection([new NodeRef(path.Id, 0)]);
        state.Edit(HistoryStepType.DeleteShape, d => d.RemoveShapes(new HashSet<Guid> { path.Id }));
        state.SetNodeSelection([new NodeRef(path.Id, 0)]);

        state.Undo();
        state.Redo();

        Assert.Empty(state.SelectedNodes);
    }

    [Fact]
    public void ARectangleAllowsTheFourCornerIndices()
    {
        var rectangle = new RectangleShape { Width = 10, Height = 10 };
        var document = Document.Create("x");
        var state = new EditorState(document.AddShape(document.Layers[0].Id, rectangle)) { ActiveTool = Tool.Node };
        state.Select(rectangle.Id);
        state.SetNodeSelection([new NodeRef(rectangle.Id, 0), new NodeRef(rectangle.Id, 3)]);

        state.Edit(HistoryStepType.MoveShape, d => d.UpdateShapes(new Dictionary<Guid, Shape> { [rectangle.Id] = rectangle with { X = 5 } }));

        Assert.Equal(2, state.SelectedNodes.Count);
    }

    [Fact]
    public void ANodeIndexBeyondThePathIsDroppedWhenTheDocumentChanges()
    {
        var (state, path) = Create();
        state.SetNodeSelection([new NodeRef(path.Id, 7), new NodeRef(path.Id, 1)]);

        state.Edit(HistoryStepType.MovePathPoints, d => d.UpdateShapes(new Dictionary<Guid, Shape> { [path.Id] = path with { X = path.X + 1 } }));

        Assert.Equal([new NodeRef(path.Id, 1)], state.SelectedNodes);
    }
}
