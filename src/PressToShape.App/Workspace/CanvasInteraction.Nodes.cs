using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

// Direct Select and the Node tool: what a press does (select a node, pick a shape, start a selection box). The drags of nodes, handles and
// segments are in CanvasInteraction.NodeDrags.cs.
internal sealed partial class CanvasInteraction
{
    // Whether the selection box that is being dragged selects nodes (a shape with nodes is selected) or shapes.
    private bool _nodeMarquee;

    // A press on a node that was already selected, without Shift. If the pointer does not move before the button goes up, the click
    // makes that node the only selected one. A drag moves all selected nodes instead.
    private NodeRef? _pendingNodeClick;
    private bool _nodeDragMoved;

    private void PressWithNodeTool(double x, double y, bool shift, InputPointerType inputType)
    {
        _pendingNodeClick = null;
        _nodeDragMoved = false;

        // Direct Select shows the radius handles of a selected rectangle. They win over its nodes and handles, as over the resize handles.
        if (_state.ActiveTool == Tool.DirectSelect && TryBeginRadiusDrag(x, y, inputType))
        {
            return;
        }

        var scale = _state.ZoomPercent / 100;
        var grab = inputType == InputPointerType.Touch ? TouchGrabSize : SelectionGeometry.GrabSize;
        var paths = EditablePathsOnScreen();
        var hit = NodeHits.Find(paths, _state.SelectedNodes, x, y, grab / 2 / scale, includeSegments: _state.ActiveTool == Tool.Node);

        switch (hit.Type)
        {
            case NodeHitType.Node:
                // Shift only changes the selection: nothing moves.
                if (SelectNodeByPress(new NodeRef(hit.ShapeId, hit.Index), shift))
                {
                    BeginNodeDrag(DragType.MovingNodes, x, y, hit);
                }

                return;
            case NodeHitType.Handle:
                BeginNodeDrag(DragType.MovingHandle, x, y, hit);
                return;
            case NodeHitType.Segment:
                // The two nodes of the segment become the selected nodes, so the segment can be made straight or curved afterwards.
                if (!shift)
                {
                    _state.SetNodeSelection([new NodeRef(hit.ShapeId, hit.Index), new NodeRef(hit.ShapeId, (hit.Index + 1) % PathNodeCount(hit.ShapeId))]);
                }

                BeginNodeDrag(DragType.Bending, x, y, hit);
                return;
        }

        if (LeafAt(x, y) is { } leaf)
        {
            // The shape under the pointer is picked without entering its group, and a shape is not moved by dragging it.
            if (shift)
            {
                _state.ToggleSelected(leaf.Id);
            }
            else
            {
                _state.SelectOnly(leaf.Id);
                _state.ClearNodeSelection();
            }

            return;
        }

        // Empty space: a box starts. It selects nodes if shapes with nodes are selected, otherwise shapes. A tiny box is a click.
        _startShift = shift;
        _nodeMarquee = paths.Count > 0;
        BeginDrag(DragType.Marquee, x, y);
    }

    private bool IsNodeTool => _state.ActiveTool is Tool.DirectSelect or Tool.Node;

    // A double click or double tap on a node with the Node tool: a corner becomes smooth, a smooth or symmetric node becomes a corner, as if
    // the matching button had been pressed. The node becomes the only selected node.
    private void NodeDoubleClicked(double x, double y, InputPointerType inputType)
    {
        var scale = _state.ZoomPercent / 100;
        var grab = inputType == InputPointerType.Touch ? TouchGrabSize : SelectionGeometry.GrabSize;
        var hit = NodeHits.Find(EditablePathsOnScreen(), _state.SelectedNodes, x, y, grab / 2 / scale, includeSegments: false);
        if (hit.Type != NodeHitType.Node
            || _state.Document.FindShape(hit.ShapeId)?.Shape is not { } shape
            || EditablePath.For(shape) is not { } path)
        {
            return;
        }

        _state.SetNodeSelection([new NodeRef(hit.ShapeId, hit.Index)]);
        Nodes.SetType(path.Nodes[hit.Index].Type == NodeType.Corner ? NodeType.Smooth : NodeType.Corner);
    }

    /// <summary>
    /// Whether the position is over something the node tools can grab: a node, a shown handle or (Node tool) a segment of a selected shape.
    /// Always <c>false</c> during a drag and for the other tools. Used for the cursor.
    /// </summary>
    public bool IsOverNodeTarget(double x, double y)
    {
        if (!IsNodeTool || IsDragging)
        {
            return false;
        }

        var scale = _state.ZoomPercent / 100;
        var hit = NodeHits.Find(EditablePathsOnScreen(), _state.SelectedNodes, x, y, SelectionGeometry.GrabSize / 2 / scale, includeSegments: _state.ActiveTool == Tool.Node);
        return hit.Type != NodeHitType.None;
    }

    // A press on a node: Shift toggles it, a node that is not selected becomes the only selected one, a selected node keeps the selection
    // so that a drag moves all selected nodes.
    // Returns whether a drag of the selected nodes may start (not after a Shift-click).
    private bool SelectNodeByPress(NodeRef node, bool shift)
    {
        if (shift)
        {
            _state.ToggleNode(node);
            return false;
        }

        if (_state.SelectedNodes.Contains(node))
        {
            _pendingNodeClick = node;
            return true;
        }

        _state.SetNodeSelection([node]);
        return true;
    }

    private int PathNodeCount(System.Guid shapeId) =>
        _state.Document.FindShape(shapeId)?.Shape is { } shape && EditablePath.For(shape) is { } path ? path.Nodes.Length : 1;

    // The button went up: a click on one of several selected nodes, without a move, selects only that node.
    private void NodeToolReleased()
    {
        if (_pendingNodeClick is { } node && !_nodeDragMoved)
        {
            _state.SetNodeSelection([node]);
        }

        _pendingNodeClick = null;
    }

    // The nodes inside the box, in document units, of the paths that have selected shapes. A tiny box clears the node selection.
    private void SelectNodesInBox(SelectionBox box)
    {
        var scale = _state.ZoomPercent / 100;
        if (box.Width * scale < ClickPixels && box.Height * scale < ClickPixels)
        {
            if (!_startShift)
            {
                _state.ClearNodeSelection();
            }

            return;
        }

        var inside = new List<NodeRef>();
        foreach (var path in EditablePathsOnScreen())
        {
            var nodes = path.ToDocumentNodes();
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].X >= box.X && nodes[i].X <= box.X + box.Width && nodes[i].Y >= box.Y && nodes[i].Y <= box.Y + box.Height)
                {
                    inside.Add(new NodeRef(path.Id, i));
                }
            }
        }

        _state.SetNodeSelection(_startShift ? _state.SelectedNodes.Concat(inside) : inside);
    }

    private IReadOnlyList<PathShape> EditablePathsOnScreen() => EditablePath.OnScreenOfSelection(_state.Document, _state.SelectedShapeIds);

    // The topmost shape under the position that can be selected, taken as it is: a shape inside a group counts, the group is not entered.
    private Shape? LeafAt(double x, double y) =>
        _state.Document.HitTest(x, y).FirstOrDefault(hit => SelectionQueries.IsSelectable(_state.Document, hit.Id));
}
