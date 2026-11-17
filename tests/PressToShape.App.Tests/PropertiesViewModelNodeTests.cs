using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class PropertiesViewModelNodeTests
{
    private sealed class Fixture
    {
        public Fixture(string tool = "Node", double rotation = 0)
        {
            var path = PathShape.Create(
                [
                    new DocumentNode(0, 100),
                    new DocumentNode(100, 100, inX: 70, inY: 100, outX: 130, outY: 100, type: NodeType.Symmetric),
                    new DocumentNode(200, 100),
                    new DocumentNode(300, 150),
                ]);
            Path = path with { Rotation = rotation };
            var document = Document.Create("x");
            State = new EditorState(document.AddShapes(document.Layers[0].Id, [Path])) { ActiveTool = Enum.Parse<Tool>(tool) };
            State.Select(Path.Id);
            Properties = new PropertiesViewModel(State, new SelectionEditor(State), FakeLocalizer.Create());
        }

        public EditorState State { get; }

        public PropertiesViewModel Properties { get; }

        public PathShape Path { get; }

        public PathShape Current => (PathShape)State.Document.FindShape(Path.Id)!.Shape;

        public int Steps => State.History.Steps.Count;

        public void Select(params int[] indices) => State.SetNodeSelection(indices.Select(i => new NodeRef(Path.Id, i)));
    }

    [Theory]
    [InlineData("Node", true, true)]
    [InlineData("DirectSelect", true, false)]
    [InlineData("Select", false, false)]
    [InlineData("Pen", false, false)]
    public void ShowNode_AndTheTypeTools_FollowTheTool(string tool, bool showNode, bool showTypeTools)
    {
        var f = new Fixture(tool);

        Assert.Equal(showNode, f.Properties.ShowNode);
        Assert.Equal(showTypeTools, f.Properties.ShowNodeTypeTools);
    }

    [Fact]
    public void ShowNode_ChangingTheTool_RaisesPropertyChanged()
    {
        var f = new Fixture("Select");
        var raised = new List<string?>();
        f.Properties.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        f.State.ActiveTool = Tool.Node;

        Assert.Contains(nameof(PropertiesViewModel.ShowNode), raised);
        Assert.Contains(nameof(PropertiesViewModel.ShowNodeTypeTools), raised);
        Assert.True(f.Properties.ShowNode);
    }

    [Fact]
    public void NodeFields_OneSelectedNode_ShowItsPosition()
    {
        var f = new Fixture();

        f.Select(2);

        Assert.True(f.Properties.NodeX.IsEnabled);
        Assert.True(f.Properties.NodeY.IsEnabled);
        Assert.Equal("200", f.Properties.NodeX.Text);
        Assert.Equal("100", f.Properties.NodeY.Text);
    }

    [Fact]
    public void NodeFields_NoneOrSeveralSelectedNodes_AreDisabled()
    {
        var f = new Fixture();

        Assert.False(f.Properties.NodeX.IsEnabled);

        f.Select(1, 2);
        Assert.False(f.Properties.NodeX.IsEnabled);
        Assert.False(f.Properties.NodeY.IsEnabled);
    }

    [Fact]
    public void NodeX_Typed_MovesTheNodeAsOneStep()
    {
        var f = new Fixture();
        f.Select(2);
        var steps = f.Steps;

        f.Properties.NodeX.Text = "250";
        f.Properties.NodeX.Commit();

        Assert.Equal(250, f.Current.ToDocumentNodes()[2].X, 6);
        Assert.Equal(100, f.Current.ToDocumentNodes()[2].Y, 6);
        Assert.Equal(steps + 1, f.Steps);
        Assert.Equal(HistoryStepType.MovePathPoints, f.State.History.Steps[^1]);
    }

    [Fact]
    public void NodeY_Typed_MovesTheNodeAndTheFieldShowsTheNewValue()
    {
        var f = new Fixture();
        f.Select(2);

        f.Properties.NodeY.Text = "40";
        f.Properties.NodeY.Commit();

        Assert.Equal("40", f.Properties.NodeY.Text);
        Assert.Equal(40, f.Current.ToDocumentNodes()[2].Y, 6);
    }

    [Fact]
    public void NodeFields_TwoTypedEdits_AreTwoSteps()
    {
        var f = new Fixture();
        f.Select(2);
        var steps = f.Steps;

        f.Properties.NodeX.Text = "250";
        f.Properties.NodeX.Commit();
        f.Properties.NodeX.Text = "260";
        f.Properties.NodeX.Commit();

        Assert.Equal(steps + 2, f.Steps);
    }

    [Fact]
    public void NodeFields_RotatedPath_ShowAndSetThePositionOnTheScreen()
    {
        var f = new Fixture(rotation: 90);
        f.Select(2);
        var position = PathNodeEditing.NodePosition(f.Path, 2);

        Assert.Equal(Math.Round(position.X).ToString(), f.Properties.NodeX.Text);

        f.Properties.NodeX.Text = "333";
        f.Properties.NodeX.Commit();

        Assert.Equal(333, PathNodeEditing.NodePosition(f.Current, 2).X, 6);
        Assert.Equal(position.Y, PathNodeEditing.NodePosition(f.Current, 2).Y, 6);
    }

    [Fact]
    public void TypeFlags_OneNode_MarkItsType()
    {
        var f = new Fixture();

        f.Select(1);
        Assert.True(f.Properties.IsNodeSymmetric);
        Assert.False(f.Properties.IsNodeCorner);
        Assert.False(f.Properties.IsNodeSmooth);

        f.Select(2);
        Assert.True(f.Properties.IsNodeCorner);
    }

    [Fact]
    public void TypeFlags_SeveralNodesOfTheSameType_MarkIt_AndMixedNodesMarkNone()
    {
        var f = new Fixture();

        f.Select(0, 2);
        Assert.True(f.Properties.IsNodeCorner);

        f.Select(0, 1);
        Assert.False(f.Properties.IsNodeCorner);
        Assert.False(f.Properties.IsNodeSmooth);
        Assert.False(f.Properties.IsNodeSymmetric);
    }

    [Fact]
    public void TypeCommands_EachChangesTheTypeAsOneStep()
    {
        var f = new Fixture();
        f.Select(2);
        var steps = f.Steps;

        f.Properties.MakeNodeSmoothCommand.Execute(null);
        Assert.Equal(NodeType.Smooth, f.Current.SubPaths[0].Nodes[2].Type);
        Assert.True(f.Properties.IsNodeSmooth);

        f.Properties.MakeNodeSymmetricCommand.Execute(null);
        Assert.Equal(NodeType.Symmetric, f.Current.SubPaths[0].Nodes[2].Type);

        f.Properties.MakeNodeCornerCommand.Execute(null);
        Assert.Equal(NodeType.Corner, f.Current.SubPaths[0].Nodes[2].Type);

        Assert.Equal(steps + 3, f.Steps);
        Assert.Equal(HistoryStepType.ChangeNodeType, f.State.History.Steps[^1]);
    }

    [Fact]
    public void TypeCommands_AreDisabledWithoutSelectedNodes()
    {
        var f = new Fixture();

        Assert.False(f.Properties.HasNodesSelected);
        Assert.False(f.Properties.MakeNodeSmoothCommand.CanExecute(null));

        f.Select(1);

        Assert.True(f.Properties.MakeNodeSmoothCommand.CanExecute(null));
    }

    [Fact]
    public void SegmentCommands_AreEnabledOnlyForTwoNeighboringNodes()
    {
        var f = new Fixture();

        f.Select(1);
        Assert.False(f.Properties.CanChangeSegment);
        Assert.False(f.Properties.MakeSegmentStraightCommand.CanExecute(null));

        f.Select(2, 3);
        Assert.True(f.Properties.CanChangeSegment);
        Assert.True(f.Properties.MakeSegmentCurvedCommand.CanExecute(null));

        f.Select(0, 2);
        Assert.False(f.Properties.CanChangeSegment);
    }

    [Fact]
    public void SegmentCommands_ChangeTheSegmentAsOneStep()
    {
        var f = new Fixture();
        f.Select(2, 3);
        var steps = f.Steps;

        f.Properties.MakeSegmentCurvedCommand.Execute(null);

        Assert.True(f.Current.SubPaths[0].Nodes[2].HasOut);
        Assert.Equal(steps + 1, f.Steps);

        f.Properties.MakeSegmentStraightCommand.Execute(null);

        Assert.False(f.Current.SubPaths[0].Nodes[2].HasOut);
        Assert.Equal(steps + 2, f.Steps);
        Assert.Equal(HistoryStepType.ChangeSegment, f.State.History.Steps[^1]);
    }

    [Fact]
    public void Values_AfterUndo_FollowTheDocument()
    {
        var f = new Fixture();
        f.Select(2);
        f.Properties.MakeNodeSmoothCommand.Execute(null);

        f.State.Undo();

        Assert.True(f.Properties.IsNodeCorner);
        Assert.False(f.Properties.IsNodeSmooth);
    }

    [Fact]
    public void Properties_RaisePropertyChanged_WhenTheNodeSelectionChanges()
    {
        var f = new Fixture();
        var raised = new List<string?>();
        f.Properties.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        f.Select(1);

        Assert.Contains(nameof(PropertiesViewModel.IsNodeSymmetric), raised);
        Assert.Contains(nameof(PropertiesViewModel.HasNodesSelected), raised);
        Assert.Contains(nameof(PropertiesViewModel.CanChangeSegment), raised);
    }

    [Fact]
    public void CommitPendingEdits_AppliesATypedNodeValue()
    {
        var f = new Fixture();
        f.Select(2);
        f.Properties.NodeX.IsEditing = true;
        f.Properties.NodeX.Text = "260";

        f.Properties.CommitPendingEdits();

        Assert.Equal(260, f.Current.ToDocumentNodes()[2].X, 6);
    }

    [Fact]
    public void TypeCommand_OnARectangleCorner_ConvertsItInOneStep()
    {
        var rectangle = new RectangleShape { X = 10, Y = 10, Width = 100, Height = 60 };
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [rectangle])) { ActiveTool = Tool.Node };
        state.Select(rectangle.Id);
        var properties = new PropertiesViewModel(state, new SelectionEditor(state), FakeLocalizer.Create());
        state.SetNodeSelection([new NodeRef(rectangle.Id, 0)]);
        var steps = state.History.Steps.Count;

        properties.MakeNodeSmoothCommand.Execute(null);

        Assert.IsType<PathShape>(state.Document.FindShape(rectangle.Id)!.Shape);
        Assert.Equal(steps + 1, state.History.Steps.Count);
    }
}
