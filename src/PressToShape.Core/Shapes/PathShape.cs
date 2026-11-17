using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Geometry;

namespace PressToShape.Core.Shapes;

/// <summary>
/// A path of line and cubic bezier segments made of one or more sub-paths, each open or closed. The box (<see cref="Shape.X"/>, <see cref="Shape.Y"/>,
/// <see cref="Shape.Width"/>, <see cref="Shape.Height"/>) is the tight bounds of all the curves, and the nodes are stored as fractions of
/// that box (see <see cref="PathNode"/>), so changing the box with <c>with</c> moves, resizes and (with the rotation) turns the
/// whole path, like any other shape. Build one from document coordinates with <see cref="Create(IReadOnlyList{DocumentSubPath}, FillRule)"/>.
/// A node or a segment is addressed by one flat index that counts across the sub-paths, the first sub-path first (see <see cref="Locate"/>).
/// Immutable: change it with <c>with</c>.
/// </summary>
public sealed partial record PathShape : Shape
{
    /// <summary>The most sub-paths one path may have.</summary>
    public const int MaxSubPaths = 10_000;

    private ImmutableArray<SubPath> _subPaths = ImmutableArray<SubPath>.Empty;

    /// <summary>The sub-paths in drawing order, from 1 to <see cref="MaxSubPaths"/>.</summary>
    public required ImmutableArray<SubPath> SubPaths
    {
        get => _subPaths;
        init
        {
            if (value.IsDefaultOrEmpty || value.Length > MaxSubPaths)
            {
                throw new ArgumentException($"A path needs from 1 to {MaxSubPaths} sub-paths.", nameof(SubPaths));
            }

            _subPaths = value;
        }
    }

    /// <summary>How the inside is decided where the path crosses itself and across the sub-paths. The default is <see cref="FillRule.NonZero"/>.</summary>
    public FillRule FillRule { get; init; }

    /// <summary>The number of nodes in all sub-paths together.</summary>
    public int NodeCount => _subPaths.Sum(subPath => subPath.Nodes.Length);

    /// <summary>The number of segments in all sub-paths together.</summary>
    public int SegmentCount => _subPaths.Sum(subPath => subPath.SegmentCount);

    /// <summary>
    /// Creates a path from one sub-path of nodes in document coordinates, in the frame before any rotation. See
    /// <see cref="Create(IReadOnlyList{DocumentSubPath}, FillRule)"/>.
    /// </summary>
    /// <exception cref="ArgumentException">There are fewer than two nodes.</exception>
    public static PathShape Create(IReadOnlyList<DocumentNode> nodes, bool isClosed = false, FillRule fillRule = FillRule.NonZero)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        return Create([new DocumentSubPath(nodes, isClosed)], fillRule);
    }

    /// <summary>
    /// Creates a path from sub-paths in document coordinates, in the frame before any rotation. The box becomes the tight bounds of the
    /// curves (the end points and the extremes of the curved segments, not the handle points). A handle point that no segment uses,
    /// such as the in handle of the first node of an open sub-path, does not count. If one side of the box has size 0, every node
    /// stores 0 on that side.
    /// </summary>
    /// <exception cref="ArgumentException">There is no sub-path or there are more than <see cref="MaxSubPaths"/>.</exception>
    public static PathShape Create(IReadOnlyList<DocumentSubPath> subPaths, FillRule fillRule = FillRule.NonZero)
    {
        ArgumentNullException.ThrowIfNull(subPaths);
        if (subPaths.Count < 1 || subPaths.Count > MaxSubPaths)
        {
            throw new ArgumentException($"A path needs from 1 to {MaxSubPaths} sub-paths.", nameof(subPaths));
        }

        var box = TightBounds(subPaths);
        var stored = ImmutableArray.CreateBuilder<SubPath>(subPaths.Count);
        foreach (var subPath in subPaths)
        {
            var nodes = ImmutableArray.CreateBuilder<PathNode>(subPath.Nodes.Count);
            foreach (var node in subPath.Nodes)
            {
                nodes.Add(new PathNode(
                    Fraction(node.X, box.X, box.Width),
                    Fraction(node.Y, box.Y, box.Height),
                    node.InX is { } inX ? Fraction(inX, box.X, box.Width) : null,
                    node.InY is { } inY ? Fraction(inY, box.Y, box.Height) : null,
                    node.OutX is { } outX ? Fraction(outX, box.X, box.Width) : null,
                    node.OutY is { } outY ? Fraction(outY, box.Y, box.Height) : null,
                    node.Type));
            }

            stored.Add(new SubPath(nodes.MoveToImmutable(), subPath.IsClosed));
        }

        return new PathShape
        {
            X = box.X,
            Y = box.Y,
            Width = box.Width,
            Height = box.Height,
            SubPaths = stored.MoveToImmutable(),
            FillRule = fillRule,
        };
    }

    /// <summary>The sub-paths in document coordinates, in the frame before the rotation. The inverse of <see cref="Create(IReadOnlyList{DocumentSubPath}, FillRule)"/> for the current box.</summary>
    public IReadOnlyList<DocumentSubPath> ToDocumentSubPaths()
    {
        var result = new List<DocumentSubPath>(_subPaths.Length);
        foreach (var subPath in _subPaths)
        {
            result.Add(new DocumentSubPath(ToDocumentNodes(subPath), subPath.IsClosed));
        }

        return result;
    }

    /// <summary>
    /// The nodes of all sub-paths in document coordinates, in the frame before the rotation, as one list in flat index order (see <see cref="Locate"/>).
    /// </summary>
    public IReadOnlyList<DocumentNode> ToDocumentNodes() => _subPaths.SelectMany(ToDocumentNodes).ToList();

    /// <summary>The segments of all sub-paths in document coordinates, in the frame before the rotation, in flat segment index order. A line is a cubic with its handles on its end points.</summary>
    public IReadOnlyList<Cubic> Segments()
    {
        var segments = new List<Cubic>(SegmentCount);
        foreach (var subPath in _subPaths)
        {
            segments.AddRange(BuildSegments(ToDocumentNodes(subPath), subPath.IsClosed));
        }

        return segments;
    }

    /// <summary>Turns a flat node index into the sub-path that holds the node and the index of the node inside it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is below 0 or not below <see cref="NodeCount"/>.</exception>
    public (int SubPath, int Node) Locate(int flatIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(flatIndex);

        var remaining = flatIndex;
        for (var i = 0; i < _subPaths.Length; i++)
        {
            var count = _subPaths[i].Nodes.Length;
            if (remaining < count)
            {
                return (i, remaining);
            }

            remaining -= count;
        }

        throw new ArgumentOutOfRangeException(nameof(flatIndex), flatIndex, "The index is not the index of a node.");
    }

    /// <summary>The node with the flat index, as stored (fractions of the box).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is below 0 or not below <see cref="NodeCount"/>.</exception>
    public PathNode NodeAt(int flatIndex)
    {
        var (subPath, node) = Locate(flatIndex);
        return _subPaths[subPath].Nodes[node];
    }

    /// <summary>The flat index of the first node of a sub-path.</summary>
    /// <exception cref="ArgumentOutOfRangeException">There is no such sub-path.</exception>
    public int FirstNodeIndex(int subPathIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(subPathIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(subPathIndex, _subPaths.Length);

        var index = 0;
        for (var i = 0; i < subPathIndex; i++)
        {
            index += _subPaths[i].Nodes.Length;
        }

        return index;
    }

    /// <summary>Turns a flat segment index into the sub-path that holds the segment and the index of the segment inside it (the segment starts at the node with that index).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is below 0 or not below <see cref="SegmentCount"/>.</exception>
    public (int SubPath, int Segment) LocateSegment(int flatIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(flatIndex);

        var remaining = flatIndex;
        for (var i = 0; i < _subPaths.Length; i++)
        {
            var count = _subPaths[i].SegmentCount;
            if (remaining < count)
            {
                return (i, remaining);
            }

            remaining -= count;
        }

        throw new ArgumentOutOfRangeException(nameof(flatIndex), flatIndex, "The index is not the index of a segment.");
    }

    /// <summary>The flat index of the first segment of a sub-path.</summary>
    /// <exception cref="ArgumentOutOfRangeException">There is no such sub-path.</exception>
    public int FirstSegmentIndex(int subPathIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(subPathIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(subPathIndex, _subPaths.Length);

        var index = 0;
        for (var i = 0; i < subPathIndex; i++)
        {
            index += _subPaths[i].SegmentCount;
        }

        return index;
    }

    /// <summary>
    /// The same shape (same id, appearance, name and flags) with its rotation applied to every node and handle point, so it has no rotation itself
    /// and its positions are the ones on the screen. For hit tests, drawing and the boolean operations, not for storing as the edited shape.
    /// </summary>
    public PathShape WithRotationApplied()
    {
        if (Rotation == 0)
        {
            return this;
        }

        var turned = new List<DocumentSubPath>(_subPaths.Length);
        foreach (var subPath in ToDocumentSubPaths())
        {
            var nodes = new List<DocumentNode>(subPath.Nodes.Count);
            foreach (var node in subPath.Nodes)
            {
                var anchor = FromLocal(node.X, node.Y);
                var handleIn = node.HasIn ? FromLocal(node.InX!.Value, node.InY!.Value) : ((double X, double Y)?)null;
                var handleOut = node.HasOut ? FromLocal(node.OutX!.Value, node.OutY!.Value) : ((double X, double Y)?)null;
                nodes.Add(new DocumentNode(anchor.X, anchor.Y, handleIn?.X, handleIn?.Y, handleOut?.X, handleOut?.Y, node.Type));
            }

            turned.Add(new DocumentSubPath(nodes, subPath.IsClosed));
        }

        var fresh = Create(turned, FillRule);
        return this with
        {
            X = fresh.X,
            Y = fresh.Y,
            Width = fresh.Width,
            Height = fresh.Height,
            SubPaths = fresh.SubPaths,
            Rotation = 0,
        };
    }

    private List<DocumentNode> ToDocumentNodes(SubPath subPath)
    {
        var result = new List<DocumentNode>(subPath.Nodes.Length);
        foreach (var node in subPath.Nodes)
        {
            result.Add(new DocumentNode(
                X + node.X * Width,
                Y + node.Y * Height,
                node.InX is { } inX ? X + inX * Width : null,
                node.InY is { } inY ? Y + inY * Height : null,
                node.OutX is { } outX ? X + outX * Width : null,
                node.OutY is { } outY ? Y + outY * Height : null,
                node.Type));
        }

        return result;
    }

    private static Bounds TightBounds(IReadOnlyList<DocumentSubPath> subPaths)
    {
        var left = double.PositiveInfinity;
        var top = double.PositiveInfinity;
        var right = double.NegativeInfinity;
        var bottom = double.NegativeInfinity;
        foreach (var subPath in subPaths)
        {
            foreach (var segment in BuildSegments(subPath.Nodes, subPath.IsClosed))
            {
                var bounds = Bezier.ExtremaBounds(segment);
                left = Math.Min(left, bounds.X);
                top = Math.Min(top, bounds.Y);
                right = Math.Max(right, bounds.X + bounds.Width);
                bottom = Math.Max(bottom, bounds.Y + bounds.Height);
            }
        }

        return new Bounds(left, top, right - left, bottom - top);
    }

    internal static List<Cubic> BuildSegments(IReadOnlyList<DocumentNode> nodes, bool isClosed)
    {
        var count = isClosed ? nodes.Count : nodes.Count - 1;
        var segments = new List<Cubic>(count);
        for (var i = 0; i < count; i++)
        {
            var from = nodes[i];
            var to = nodes[(i + 1) % nodes.Count];
            segments.Add(new Cubic(
                (from.X, from.Y),
                from.HasOut ? (from.OutX!.Value, from.OutY!.Value) : (from.X, from.Y),
                to.HasIn ? (to.InX!.Value, to.InY!.Value) : (to.X, to.Y),
                (to.X, to.Y)));
        }

        return segments;
    }

    private static double Fraction(double value, double origin, double size) => size == 0 ? 0 : (value - origin) / size;
}
