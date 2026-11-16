using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class NodeOverlayBuilderTests
{
    private static readonly IReadOnlySet<NodeRef> NoNodes = ImmutableHashSet<NodeRef>.Empty;

    private static PathShape Row(double rotation = 0)
    {
        var path = PathShape.Create(
            [
                new DocumentNode(0, 100, outX: 30, outY: 60),
                new DocumentNode(100, 100, inX: 70, inY: 100, outX: 130, outY: 100, type: NodeType.Smooth),
                new DocumentNode(200, 100, inX: 170, inY: 60),
            ]);
        return path with { Rotation = rotation };
    }

    private static Document DocumentWith(params Shape[] shapes)
    {
        var document = Document.Create("x");
        return document.AddShapes(document.Layers[0].Id, shapes);
    }

    private static ImmutableArray<Guid> Ids(params Shape[] shapes) => [.. shapes.Select(s => s.Id)];

    private static ScreenNodeOverlay? Build(Document document, ImmutableArray<Guid> ids, IReadOnlySet<NodeRef> nodes, Tool tool = Tool.Node, double zoom = 100, double originX = 0, double originY = 0) =>
        NodeOverlayBuilder.Build(document, ids, nodes, tool, zoom, originX, originY);

    [Theory]
    [InlineData("Select")]
    [InlineData("Pen")]
    [InlineData("Rectangle")]
    [InlineData("Hand")]
    public void Build_OtherTools_ReturnsNull(string toolName)
    {
        var path = Row();

        Assert.Null(Build(DocumentWith(path), Ids(path), NoNodes, Enum.Parse<Tool>(toolName)));
    }

    [Fact]
    public void Build_NothingSelected_ReturnsNull()
    {
        Assert.Null(Build(DocumentWith(Row()), [], NoNodes));
    }

    [Theory]
    [InlineData("DirectSelect")]
    [InlineData("Node")]
    public void Build_SelectedPath_ShowsEveryNodeAndNoHandlesWithoutSelectedNodes(string toolName)
    {
        var path = Row();

        var overlay = Build(DocumentWith(path), Ids(path), NoNodes, Enum.Parse<Tool>(toolName))!;

        Assert.Equal(3, overlay.Nodes.Count);
        Assert.All(overlay.Nodes, n => Assert.False(n.IsSelected));
        Assert.All(overlay.Nodes, n => Assert.False(n.HasIn || n.HasOut));
        Assert.Equal([0.0, 100.0, 200.0], overlay.Nodes.Select(n => n.X));
    }

    [Fact]
    public void Build_SelectedNode_IsMarkedAndShowsItsHandlesAndTheNeighborHandlesTowardIt()
    {
        var path = Row();
        var selected = new[] { new NodeRef(path.Id, 1) }.ToImmutableHashSet();

        var overlay = Build(DocumentWith(path), Ids(path), selected)!;

        Assert.False(overlay.Nodes[0].IsSelected);
        Assert.True(overlay.Nodes[1].IsSelected);
        Assert.Equal((70, 100), (overlay.Nodes[1].InX!.Value, overlay.Nodes[1].InY!.Value));
        Assert.Equal((130, 100), (overlay.Nodes[1].OutX!.Value, overlay.Nodes[1].OutY!.Value));
        // The out handle of node 0 and the in handle of node 2 belong to the segments next to the selected node.
        Assert.Equal((30, 60), (overlay.Nodes[0].OutX!.Value, overlay.Nodes[0].OutY!.Value));
        Assert.Equal((170, 60), (overlay.Nodes[2].InX!.Value, overlay.Nodes[2].InY!.Value));
    }

    [Fact]
    public void Build_SelectedEndNode_ShowsOnlyTheHandlesOfItsSegment()
    {
        var path = Row();
        var selected = new[] { new NodeRef(path.Id, 0) }.ToImmutableHashSet();

        var overlay = Build(DocumentWith(path), Ids(path), selected)!;

        Assert.True(overlay.Nodes[0].HasOut);
        Assert.True(overlay.Nodes[1].HasIn);
        Assert.False(overlay.Nodes[1].HasOut);
        Assert.False(overlay.Nodes[2].HasIn);
    }

    [Fact]
    public void Build_RotatedPath_IsInScreenPixelsWhereItAppears()
    {
        var path = Row(90);
        var selected = new[] { new NodeRef(path.Id, 2) }.ToImmutableHashSet();

        var overlay = Build(DocumentWith(path), Ids(path), selected)!;

        var expected = PathNodeEditing.NodePosition(path, 2);
        Assert.Equal(expected.X, overlay.Nodes[2].X, 6);
        Assert.Equal(expected.Y, overlay.Nodes[2].Y, 6);
    }

    [Fact]
    public void Build_ZoomAndOrigin_ConvertDocumentUnitsToScreenPixels()
    {
        var path = Row();

        var overlay = Build(DocumentWith(path), Ids(path), NoNodes, zoom: 200, originX: 50, originY: 20)!;

        Assert.Equal((50 + 2 * 100, 20 + 2 * 100), (overlay.Nodes[1].X, overlay.Nodes[1].Y));
    }

    [Fact]
    public void Build_Handles_FollowZoomAndOriginToo()
    {
        var path = Row();
        var selected = new[] { new NodeRef(path.Id, 1) }.ToImmutableHashSet();

        var overlay = Build(DocumentWith(path), Ids(path), selected, zoom: 50, originX: 10, originY: 10)!;

        Assert.Equal(10 + 0.5 * 130, overlay.Nodes[1].OutX!.Value, 6);
    }

    [Fact]
    public void Build_Rectangle_HasFourCornerNodes()
    {
        var rectangle = new RectangleShape { X = 10, Y = 20, Width = 100, Height = 60 };

        var overlay = Build(DocumentWith(rectangle), Ids(rectangle), NoNodes, Tool.DirectSelect)!;

        Assert.Equal([(10.0, 20.0), (110.0, 20.0), (110.0, 80.0), (10.0, 80.0)], overlay.Nodes.Select(n => (n.X, n.Y)));
    }

    [Fact]
    public void Build_Ellipse_HasFourNodesAndShowsHandlesWhenSelected()
    {
        var ellipse = new EllipseShape { X = 0, Y = 0, Width = 100, Height = 100 };
        var selected = new[] { new NodeRef(ellipse.Id, 0) }.ToImmutableHashSet();

        var overlay = Build(DocumentWith(ellipse), Ids(ellipse), selected)!;

        Assert.Equal(4, overlay.Nodes.Count);
        Assert.True(overlay.Nodes[0].HasIn && overlay.Nodes[0].HasOut);
    }

    [Fact]
    public void Build_SeveralSelectedShapes_ListsTheNodesShapeByShape()
    {
        var first = Row();
        var second = new RectangleShape { X = 500, Y = 500, Width = 10, Height = 10 };

        var overlay = Build(DocumentWith(first, second), Ids(first, second), NoNodes)!;

        Assert.Equal(7, overlay.Nodes.Count);
    }

    [Fact]
    public void Build_PathInsideASelectedGroup_IsShown()
    {
        var path = Row();
        var group = new GroupShape { Children = [path, new RectangleShape { X = 500, Y = 500, Width = 10, Height = 10 }] };

        var overlay = Build(DocumentWith(group), Ids(group), NoNodes)!;

        Assert.Equal(7, overlay.Nodes.Count);
    }

    [Fact]
    public void Build_HiddenAndLockedShapes_ShowNothing()
    {
        var hidden = Row() with { IsVisible = false };
        var locked = Row() with { IsLocked = true };
        var document = DocumentWith(hidden, locked);

        Assert.Null(Build(document, Ids(hidden, locked), NoNodes));
    }

    [Fact]
    public void Build_GroupWithOnlyARectangle_ShowsItsFourCorners()
    {
        var group = new GroupShape { Children = [new RectangleShape { X = 0, Y = 0, Width = 10, Height = 10 }] };

        var overlay = Build(DocumentWith(group), Ids(group), NoNodes);

        Assert.Equal(4, overlay!.Nodes.Count);
    }

    [Fact]
    public void Build_NodesOfAnotherShape_DoNotMarkTheseNodes()
    {
        var path = Row();
        var other = new NodeRef(Guid.NewGuid(), 1);

        var overlay = Build(DocumentWith(path), Ids(path), new[] { other }.ToImmutableHashSet())!;

        Assert.All(overlay.Nodes, n => Assert.False(n.IsSelected));
    }

    // ---- the view model property ----

    private static (EditorState State, CanvasAreaViewModel Canvas, PathShape Path) Create(Tool tool)
    {
        var path = Row();
        var state = new EditorState(DocumentWith(path)) { ActiveTool = tool };
        state.Select(path.Id);
        return (state, new CanvasAreaViewModel(state) { ViewportWidth = 1200, ViewportHeight = 800 }, path);
    }

    [Fact]
    public void NodeOverlay_NodeTool_ShowsTheNodes_AndOtherToolsNothing()
    {
        var (state, canvas, _) = Create(Tool.Node);
        Assert.NotNull(canvas.NodeOverlay);

        state.ActiveTool = Tool.Select;

        Assert.Null(canvas.NodeOverlay);
    }

    [Fact]
    public void NodeOverlay_RaisesPropertyChanged_OnNodeSelectionToolDocumentAndZoom()
    {
        var (state, canvas, path) = Create(Tool.Node);
        var raised = new List<string?>();
        canvas.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        state.SetNodeSelection([new NodeRef(path.Id, 1)]);
        Assert.Contains(nameof(CanvasAreaViewModel.NodeOverlay), raised);

        raised.Clear();
        state.SetZoom(200);
        Assert.Contains(nameof(CanvasAreaViewModel.NodeOverlay), raised);

        raised.Clear();
        state.Edit(HistoryStepType.MovePathPoints, d => d.UpdateShapes(new Dictionary<Guid, Shape> { [path.Id] = path with { X = 5 } }));
        Assert.Contains(nameof(CanvasAreaViewModel.NodeOverlay), raised);

        raised.Clear();
        state.ActiveTool = Tool.DirectSelect;
        Assert.Contains(nameof(CanvasAreaViewModel.NodeOverlay), raised);
    }

    [Fact]
    public void NodeOverlay_InScreenPixelsOfTheCanvas_UsesTheViewModelOrigin()
    {
        var (state, canvas, path) = Create(Tool.Node);
        state.SetNodeSelection([new NodeRef(path.Id, 1)]);

        var node = canvas.NodeOverlay!.Nodes[1];

        Assert.Equal(canvas.OriginX + 100, node.X, 6);
        Assert.Equal(canvas.OriginY + 100, node.Y, 6);
        Assert.True(node.IsSelected);
    }
}
