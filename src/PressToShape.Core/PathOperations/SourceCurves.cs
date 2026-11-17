using System;
using System.Collections.Generic;
using PressToShape.Core.Geometry;

namespace PressToShape.Core.PathOperations;

/// <summary>
/// The curves of the operands of a boolean operation, flattened, with an index by position. After clipping, a point of the result is
/// looked up here to find out whether it lies on an original curve segment and at which <c>t</c>, so unchanged curve pieces of the
/// result can be written as the original curve again. Straight segments are not indexed, because they need no curve.
/// </summary>
internal sealed class SourceCurves
{
    // How close a point has to be to a flattened edge to count as lying on it. Clipper2 rounds to 4 decimals.
    private const double Reach = 1e-3;

    private const double CellSize = 16;

    // An edge that covers more cells than this is checked for every lookup instead of being put in the cells.
    private const int MaximumCells = 64;

    private readonly Dictionary<(int Operand, int Segment), Cubic> _curves = [];
    private readonly List<Edge> _edges = [];
    private readonly Dictionary<(long X, long Y), List<int>> _cells = [];
    private readonly List<int> _long = [];

    /// <summary>The cubic of a curved segment, or <c>null</c> if the segment is straight or unknown.</summary>
    public Cubic? Curve(int operand, int segment) => _curves.TryGetValue((operand, segment), out var curve) ? curve : null;

    /// <summary>Adds one sub-path: its segments (see <see cref="Flattening.Flatten"/>) and the flattened points of them.</summary>
    public void Add(int operand, int firstSegment, IReadOnlyList<Cubic> segments, IReadOnlyList<FlatPoint> points)
    {
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if (!(segment.P1 == segment.P0 && segment.P2 == segment.P3))
            {
                _curves[(operand, firstSegment + i)] = segment;
            }
        }

        for (var k = 0; k < points.Count; k++)
        {
            var from = points[k];
            if (!_curves.ContainsKey((from.Operand, from.Segment)))
            {
                continue;
            }

            // The edge after the last point of a closed sub-path runs back to the first point, which is the end of the last segment.
            var next = k + 1 < points.Count ? points[k + 1] : points[0];
            if (k + 1 == points.Count && from.T == 1)
            {
                continue;
            }

            var endT = next.Segment == from.Segment && next.T > from.T ? next.T : 1;
            Insert(new Edge(from.X, from.Y, next.X, next.Y, from.Operand, from.Segment, from.T, endT));
        }
    }

    /// <summary>
    /// Where a point lies on the flattened curves: for every curved segment that has an edge within reach of the point, the segment and the
    /// <c>t</c> of the nearest such point of the edge.
    /// </summary>
    public IReadOnlyList<(int Operand, int Segment, double T)> Matches(double x, double y)
    {
        var best = new Dictionary<(int Operand, int Segment), (double Distance, double T)>();
        foreach (var index in _long)
        {
            Consider(index, x, y, best);
        }

        if (_cells.TryGetValue(Cell(x, y), out var inCell))
        {
            foreach (var index in inCell)
            {
                Consider(index, x, y, best);
            }
        }

        var result = new List<(int Operand, int Segment, double T)>(best.Count);
        foreach (var (key, value) in best)
        {
            result.Add((key.Operand, key.Segment, value.T));
        }

        // A dictionary has no defined order, so sort to keep the result the same for the same input.
        result.Sort((a, b) => a.Operand != b.Operand ? a.Operand.CompareTo(b.Operand) : a.Segment.CompareTo(b.Segment));
        return result;
    }

    private void Consider(int index, double x, double y, Dictionary<(int Operand, int Segment), (double Distance, double T)> best)
    {
        var edge = _edges[index];
        var dx = edge.Bx - edge.Ax;
        var dy = edge.By - edge.Ay;
        var lengthSquared = dx * dx + dy * dy;
        var along = lengthSquared == 0 ? 0 : Math.Clamp(((x - edge.Ax) * dx + (y - edge.Ay) * dy) / lengthSquared, 0, 1);
        var nearX = edge.Ax + along * dx - x;
        var nearY = edge.Ay + along * dy - y;
        var distance = Math.Sqrt(nearX * nearX + nearY * nearY);
        if (distance > Reach)
        {
            return;
        }

        var key = (edge.Operand, edge.Segment);
        if (!best.TryGetValue(key, out var existing) || distance < existing.Distance)
        {
            best[key] = (distance, edge.StartT + along * (edge.EndT - edge.StartT));
        }
    }

    private void Insert(Edge edge)
    {
        var index = _edges.Count;
        _edges.Add(edge);

        var (minX, minY) = Cell(Math.Min(edge.Ax, edge.Bx) - Reach, Math.Min(edge.Ay, edge.By) - Reach);
        var (maxX, maxY) = Cell(Math.Max(edge.Ax, edge.Bx) + Reach, Math.Max(edge.Ay, edge.By) + Reach);
        if ((maxX - minX + 1) * (maxY - minY + 1) > MaximumCells)
        {
            _long.Add(index);
            return;
        }

        for (var cx = minX; cx <= maxX; cx++)
        {
            for (var cy = minY; cy <= maxY; cy++)
            {
                if (!_cells.TryGetValue((cx, cy), out var list))
                {
                    list = [];
                    _cells[(cx, cy)] = list;
                }

                list.Add(index);
            }
        }
    }

    private static (long X, long Y) Cell(double x, double y) => ((long)Math.Floor(x / CellSize), (long)Math.Floor(y / CellSize));

    private readonly record struct Edge(double Ax, double Ay, double Bx, double By, int Operand, int Segment, double StartT, double EndT);
}
