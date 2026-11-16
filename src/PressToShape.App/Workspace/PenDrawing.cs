using System;
using System.Collections.Generic;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// The state of a path that the Pen is drawing: the nodes so far and the gesture in progress. It knows nothing about the document,
/// the zoom or the pointer device, so it is tested alone. Distances are in document units, the caller converts them from screen pixels.
/// A press adds a corner node, a drag after it pulls out the handles (smooth node), a press on the first node closes the path
/// and a press on the last node finishes it.
/// </summary>
internal sealed class PenDrawing
{
    private readonly List<DocumentNode> _nodes = [];
    private bool _isPressing;
    private double? _pointerX;
    private double? _pointerY;
    private bool _highlightFirst;

    /// <summary>Whether the path has at least one node.</summary>
    public bool IsDrawing => _nodes.Count > 0;

    /// <summary>Whether the button is down after a press that added a node.</summary>
    public bool IsPressing => _isPressing;

    /// <summary>The nodes so far, in document coordinates.</summary>
    public IReadOnlyList<DocumentNode> Nodes => _nodes;

    /// <summary>
    /// The path as it would be if it ended at the pointer: the nodes so far plus a corner node at the last known pointer position,
    /// which gives a line, or a curve if the last node has an out handle. While the button is down the pointer is dragging a handle,
    /// so only the nodes are shown. <c>null</c> while there are fewer than two nodes to show.
    /// </summary>
    public PathShape? PreviewShape
    {
        get
        {
            if (_nodes.Count == 0)
            {
                return null;
            }

            var shown = new List<DocumentNode>(_nodes);
            // A pointer exactly on the last node (right after a click) would only add a segment without length.
            if (!_isPressing && _pointerX is { } x && _pointerY is { } y && (x != _nodes[^1].X || y != _nodes[^1].Y))
            {
                shown.Add(new DocumentNode(x, y));
            }

            return shown.Count >= 2 ? PathShape.Create(shown) : null;
        }
    }

    /// <summary>What to show over the unfinished path, or <c>null</c> when nothing is drawn.</summary>
    public PenOverlay? Overlay => _nodes.Count == 0 ? null : new PenOverlay([.. _nodes], _highlightFirst);

    /// <summary>
    /// Starts from the nodes of an existing open path, to continue it. The last node is the one the new nodes follow.
    /// </summary>
    public void Begin(IReadOnlyList<DocumentNode> existing)
    {
        ArgumentNullException.ThrowIfNull(existing);

        Discard();
        _nodes.AddRange(existing);
    }

    /// <summary>
    /// The button went down at this position. A press within <paramref name="lastNodeDistance"/> of the last node means finish (a
    /// double click or double tap). A press within <paramref name="closeDistance"/> of the first node of a path with at least three
    /// nodes means close. Otherwise a corner node is added.
    /// </summary>
    public PenPressResult Press(double x, double y, double lastNodeDistance, double closeDistance)
    {
        if (_nodes.Count > 0 && Distance(_nodes[^1], x, y) <= lastNodeDistance)
        {
            return PenPressResult.FinishedPath;
        }

        if (_nodes.Count >= 3 && Distance(_nodes[0], x, y) <= closeDistance)
        {
            return PenPressResult.ClosedPath;
        }

        _nodes.Add(new DocumentNode(x, y));
        _isPressing = true;
        _highlightFirst = false;

        // The preview shows the way to the pointer, so the old position goes until the pointer moves again.
        _pointerX = null;
        _pointerY = null;
        return PenPressResult.AddedNode;
    }

    /// <summary>
    /// The pointer moved with the button down. Once it is farther than <paramref name="threshold"/> from the node that was just
    /// added, the node becomes smooth: the out handle follows the pointer and the in handle is the mirror image on the other side.
    /// A shorter drag leaves a corner. Does nothing when no press added a node.
    /// </summary>
    public void Drag(double x, double y, double threshold)
    {
        if (!_isPressing)
        {
            return;
        }

        var node = _nodes[^1];
        if (Distance(node, x, y) <= threshold)
        {
            _nodes[^1] = new DocumentNode(node.X, node.Y);
            return;
        }

        _nodes[^1] = new DocumentNode(node.X, node.Y, 2 * node.X - x, 2 * node.Y - y, x, y);
    }

    /// <summary>Takes back the node that the press in progress added, for a press that turns out to be a mistake. Does nothing when no press is in progress.</summary>
    public void AbortPress()
    {
        if (!_isPressing)
        {
            return;
        }

        _nodes.RemoveAt(_nodes.Count - 1);
        _isPressing = false;
    }

    /// <summary>The button went up. The node that was added keeps the handles the drag gave it.</summary>
    public void Release()
    {
        _isPressing = false;
    }

    /// <summary>The pointer moved without a button. Remembers the position for the preview and checks whether a click would close the path.</summary>
    public void Hover(double x, double y, double closeDistance)
    {
        _pointerX = x;
        _pointerY = y;
        _highlightFirst = !_isPressing && _nodes.Count >= 3 && Distance(_nodes[0], x, y) <= closeDistance;
    }

    /// <summary>
    /// Ends the drawing and returns the path with the nodes so far, or <c>null</c> if there are fewer than two. The state is
    /// empty afterwards in both cases.
    /// </summary>
    public PathShape? Finish(bool close)
    {
        var path = _nodes.Count >= 2 ? PathShape.Create(_nodes, close) : null;
        Discard();
        return path;
    }

    /// <summary>Drops everything that was drawn.</summary>
    public void Discard()
    {
        _nodes.Clear();
        _isPressing = false;
        _pointerX = null;
        _pointerY = null;
        _highlightFirst = false;
    }

    private static double Distance(DocumentNode node, double x, double y)
    {
        var dx = node.X - x;
        var dy = node.Y - y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
