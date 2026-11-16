using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using PressToShape.Core.Geometry;

namespace PressToShape.Core.Shapes;

/// <summary>
/// A path of line and cubic bezier segments, open or closed. The box (<see cref="Shape.X"/>, <see cref="Shape.Y"/>,
/// <see cref="Shape.Width"/>, <see cref="Shape.Height"/>) is the tight bounds of the curve, and the nodes are stored as fractions of
/// that box (see <see cref="PathNode"/>), so changing the box with <c>with</c> moves, resizes and (with the rotation) turns the
/// whole path, like any other shape. Build one from document coordinates with <see cref="Create"/>.
/// Immutable: change it with <c>with</c>.
/// </summary>
public sealed partial record PathShape : Shape
{
    private ImmutableArray<PathNode> _nodes = ImmutableArray<PathNode>.Empty;

    /// <summary>The nodes in drawing order, at least two. The segment from the last node back to the first exists only if <see cref="IsClosed"/>.</summary>
    public required ImmutableArray<PathNode> Nodes
    {
        get => _nodes;
        init
        {
            if (value.IsDefaultOrEmpty || value.Length < 2)
            {
                throw new ArgumentException("A path needs at least two nodes.", nameof(Nodes));
            }

            _nodes = value;
        }
    }

    /// <summary>Whether the last node connects back to the first one.</summary>
    public bool IsClosed { get; init; }

    /// <summary>How the inside is decided where the path crosses itself. The default is <see cref="FillRule.NonZero"/>.</summary>
    public FillRule FillRule { get; init; }

    /// <summary>
    /// Creates a path from nodes in document coordinates, in the frame before any rotation. The box becomes the tight bounds of the
    /// curve (the end points and the extremes of the curved segments, not the handle points). A handle point that no segment uses,
    /// such as the in handle of the first node of an open path, does not count. If one side of the box has size 0, every node
    /// stores 0 on that side.
    /// </summary>
    /// <exception cref="ArgumentException">There are fewer than two nodes.</exception>
    public static PathShape Create(IReadOnlyList<DocumentNode> nodes, bool isClosed = false, FillRule fillRule = FillRule.NonZero)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        if (nodes.Count < 2)
        {
            throw new ArgumentException("A path needs at least two nodes.", nameof(nodes));
        }

        var box = TightBounds(nodes, isClosed);
        var stored = ImmutableArray.CreateBuilder<PathNode>(nodes.Count);
        foreach (var node in nodes)
        {
            stored.Add(new PathNode(
                Fraction(node.X, box.X, box.Width),
                Fraction(node.Y, box.Y, box.Height),
                node.InX is { } inX ? Fraction(inX, box.X, box.Width) : null,
                node.InY is { } inY ? Fraction(inY, box.Y, box.Height) : null,
                node.OutX is { } outX ? Fraction(outX, box.X, box.Width) : null,
                node.OutY is { } outY ? Fraction(outY, box.Y, box.Height) : null));
        }

        return new PathShape
        {
            X = box.X,
            Y = box.Y,
            Width = box.Width,
            Height = box.Height,
            Nodes = stored.MoveToImmutable(),
            IsClosed = isClosed,
            FillRule = fillRule,
        };
    }

    /// <summary>The nodes in document coordinates, in the frame before the rotation. The inverse of <see cref="Create"/> for the current box.</summary>
    public IReadOnlyList<DocumentNode> ToDocumentNodes()
    {
        var result = new List<DocumentNode>(_nodes.Length);
        foreach (var node in _nodes)
        {
            result.Add(new DocumentNode(
                X + node.X * Width,
                Y + node.Y * Height,
                node.InX is { } inX ? X + inX * Width : null,
                node.InY is { } inY ? Y + inY * Height : null,
                node.OutX is { } outX ? X + outX * Width : null,
                node.OutY is { } outY ? Y + outY * Height : null));
        }

        return result;
    }

    /// <summary>The segments in document coordinates, in the frame before the rotation. A line is a cubic with its handles on its end points.</summary>
    public IReadOnlyList<Cubic> Segments() => BuildSegments(ToDocumentNodes(), IsClosed);

    private static Bounds TightBounds(IReadOnlyList<DocumentNode> nodes, bool isClosed)
    {
        var left = double.PositiveInfinity;
        var top = double.PositiveInfinity;
        var right = double.NegativeInfinity;
        var bottom = double.NegativeInfinity;
        foreach (var segment in BuildSegments(nodes, isClosed))
        {
            var bounds = Bezier.ExtremaBounds(segment);
            left = Math.Min(left, bounds.X);
            top = Math.Min(top, bounds.Y);
            right = Math.Max(right, bounds.X + bounds.Width);
            bottom = Math.Max(bottom, bounds.Y + bounds.Height);
        }

        return new Bounds(left, top, right - left, bottom - top);
    }

    private static List<Cubic> BuildSegments(IReadOnlyList<DocumentNode> nodes, bool isClosed)
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
