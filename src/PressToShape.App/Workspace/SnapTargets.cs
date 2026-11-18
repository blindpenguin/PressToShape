using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>What a snap target is.</summary>
internal enum SnapTargetType
{
    /// <summary>An edge of the bounding box of a shape or group.</summary>
    BoxEdge,

    /// <summary>The center line of the bounding box of a shape or group.</summary>
    BoxCenter,

    /// <summary>The anchor of a path node.</summary>
    PathNode,

    /// <summary>An edge or center line of the artboard.</summary>
    Artboard,
}

/// <summary>
/// One position that a point can snap to on one axis. <paramref name="From"/> and <paramref name="To"/> are the extent of the target along the
/// other axis, so a guide line can start and end at the target.
/// </summary>
internal readonly record struct SnapTarget(double Value, SnapTargetType Type, double From, double To);

/// <summary>
/// The positions a drag can snap to, collected once at the press of a drag so that the pointer moves only search them. For every visible shape
/// and group that is not excluded: the edges and the center of its bounding box (the box of the turned outline) and, for a path, its anchor nodes
/// with the rotation applied. The artboard adds its edges and its center lines. Hidden shapes, shapes of hidden groups and hidden layers are
/// no targets, locked shapes are. A group that is excluded gives no box of its own, its members are still visited.
/// </summary>
internal sealed class SnapTargets
{
    private readonly SnapTarget[] _x;
    private readonly SnapTarget[] _y;

    private SnapTargets(IEnumerable<SnapTarget> x, IEnumerable<SnapTarget> y)
    {
        _x = [.. x.OrderBy(target => target.Value)];
        _y = [.. y.OrderBy(target => target.Value)];
    }

    /// <summary>No targets.</summary>
    public static SnapTargets Empty { get; } = new([], []);

    /// <summary>The targets on the x axis, ordered by value.</summary>
    public IReadOnlyList<SnapTarget> X => _x;

    /// <summary>The targets on the y axis, ordered by value.</summary>
    public IReadOnlyList<SnapTarget> Y => _y;

    /// <summary>Collects the targets of a document.</summary>
    /// <param name="document">The document.</param>
    /// <param name="excludedIds">The shapes and groups that give no target of their own, for example the ones being dragged and the groups around them.</param>
    public static SnapTargets Collect(Document document, IReadOnlySet<Guid> excludedIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(excludedIds);

        var x = new List<SnapTarget>();
        var y = new List<SnapTarget>();
        foreach (var layer in document.Layers)
        {
            if (layer.IsVisible)
            {
                AddShapes(layer.Shapes, excludedIds, x, y);
            }
        }

        var width = document.ArtboardWidth;
        var height = document.ArtboardHeight;
        x.Add(new SnapTarget(0, SnapTargetType.Artboard, 0, height));
        x.Add(new SnapTarget(width / 2, SnapTargetType.Artboard, 0, height));
        x.Add(new SnapTarget(width, SnapTargetType.Artboard, 0, height));
        y.Add(new SnapTarget(0, SnapTargetType.Artboard, 0, width));
        y.Add(new SnapTarget(height / 2, SnapTargetType.Artboard, 0, width));
        y.Add(new SnapTarget(height, SnapTargetType.Artboard, 0, width));
        return new SnapTargets(x, y);
    }

    /// <summary>These targets and the given anchor positions (document units) as node targets: the nodes of a path that is drawn or edited.</summary>
    public SnapTargets WithNodes(IEnumerable<(double X, double Y)> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        var added = nodes.Where(node => double.IsFinite(node.X) && double.IsFinite(node.Y)).ToList();
        return new SnapTargets(
            _x.Concat(added.Select(node => new SnapTarget(node.X, SnapTargetType.PathNode, node.Y, node.Y))),
            _y.Concat(added.Select(node => new SnapTarget(node.Y, SnapTargetType.PathNode, node.X, node.X))));
    }

    /// <summary>
    /// The target on the axis that is nearest to the value, if one is within the tolerance (inclusive). Of two targets at the same
    /// distance the one with the smaller value wins.
    /// </summary>
    public bool TryFindNearest(SnapAxis axis, double value, double tolerance, out SnapTarget nearest)
    {
        nearest = default;
        if (!double.IsFinite(value) || !double.IsFinite(tolerance) || tolerance < 0)
        {
            return false;
        }

        var targets = axis == SnapAxis.X ? _x : _y;
        var index = Array.BinarySearch(targets, new SnapTarget(value, default, 0, 0), ValueComparer.Instance);
        if (index < 0)
        {
            index = ~index;
        }

        var found = false;
        var bestDistance = double.MaxValue;
        for (var i = Math.Max(0, index - 1); i <= Math.Min(targets.Length - 1, index); i++)
        {
            var distance = Math.Abs(targets[i].Value - value);
            if (distance <= tolerance && distance < bestDistance)
            {
                bestDistance = distance;
                nearest = targets[i];
                found = true;
            }
        }

        return found;
    }

    private static void AddShapes(IEnumerable<Shape> shapes, IReadOnlySet<Guid> excludedIds, List<SnapTarget> x, List<SnapTarget> y)
    {
        foreach (var shape in shapes)
        {
            if (!shape.IsVisible)
            {
                continue;
            }

            if (!excludedIds.Contains(shape.Id))
            {
                AddBox(shape, x, y);
                if (shape is PathShape path)
                {
                    AddNodes(path, x, y);
                }
            }

            if (shape is GroupShape group)
            {
                AddShapes(group.Children, excludedIds, x, y);
            }
        }
    }

    private static void AddBox(Shape shape, List<SnapTarget> x, List<SnapTarget> y)
    {
        var box = shape.RotatedBounds;
        var right = box.X + box.Width;
        var bottom = box.Y + box.Height;
        x.Add(new SnapTarget(box.X, SnapTargetType.BoxEdge, box.Y, bottom));
        x.Add(new SnapTarget(box.X + box.Width / 2, SnapTargetType.BoxCenter, box.Y, bottom));
        x.Add(new SnapTarget(right, SnapTargetType.BoxEdge, box.Y, bottom));
        y.Add(new SnapTarget(box.Y, SnapTargetType.BoxEdge, box.X, right));
        y.Add(new SnapTarget(box.Y + box.Height / 2, SnapTargetType.BoxCenter, box.X, right));
        y.Add(new SnapTarget(bottom, SnapTargetType.BoxEdge, box.X, right));
    }

    private static void AddNodes(PathShape path, List<SnapTarget> x, List<SnapTarget> y)
    {
        foreach (var node in path.WithRotationApplied().ToDocumentNodes())
        {
            x.Add(new SnapTarget(node.X, SnapTargetType.PathNode, node.Y, node.Y));
            y.Add(new SnapTarget(node.Y, SnapTargetType.PathNode, node.X, node.X));
        }
    }

    private sealed class ValueComparer : IComparer<SnapTarget>
    {
        public static ValueComparer Instance { get; } = new();

        public int Compare(SnapTarget a, SnapTarget b) => a.Value.CompareTo(b.Value);
    }
}
