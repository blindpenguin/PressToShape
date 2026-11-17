using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.PathOperations;

/// <summary>
/// Turns the polygons of a clipped result back into nodes. An edge of the polygon that is a piece of a flattened original curve becomes
/// that piece of the original curve again (a cubic cut out of it), and neighboring edges on the same curve become one cubic. Every
/// other edge is a straight line, which is within the tolerance of the exact result because the polygon is.
/// </summary>
internal static class CurveFitting
{
    // Two t values closer than this are the same point of a curve.
    private const double SameT = 1e-9;

    /// <summary>The closed sub-path of a polygon.</summary>
    /// <param name="polygon">The polygon, at least three points.</param>
    /// <param name="sources">The flattened original curves.</param>
    public static DocumentSubPath Fit(IReadOnlyList<(double X, double Y)> polygon, SourceCurves sources)
    {
        var count = polygon.Count;
        var matches = polygon.Select(point => sources.Matches(point.X, point.Y)).ToList();

        var pieces = new List<Piece>(count);
        for (var i = 0; i < count; i++)
        {
            pieces.Add(Classify(polygon, matches, sources, i, (i + 1) % count));
        }

        pieces = Merge(pieces);
        return pieces.Count < 2 ? Lines(polygon) : Build(polygon, pieces, sources);
    }

    // An edge between two points that lie on the same curve segment, with the stretch between them as flat as a flattened curve is.
    private static Piece Classify(IReadOnlyList<(double X, double Y)> polygon, List<IReadOnlyList<(int Operand, int Segment, double T)>> matches, SourceCurves sources, int from, int to)
    {
        foreach (var start in matches[from])
        {
            foreach (var end in matches[to])
            {
                if (start.Operand != end.Operand || start.Segment != end.Segment || Math.Abs(end.T - start.T) <= SameT)
                {
                    continue;
                }

                if (sources.Curve(start.Operand, start.Segment) is not { } curve)
                {
                    continue;
                }

                var middle = Bezier.Point(curve, (start.T + end.T) / 2);
                var chordX = (polygon[from].X + polygon[to].X) / 2;
                var chordY = (polygon[from].Y + polygon[to].Y) / 2;
                if (Math.Sqrt(Math.Pow(middle.X - chordX, 2) + Math.Pow(middle.Y - chordY, 2)) <= BooleanEngine.Tolerance + 1e-3)
                {
                    return new Piece(from, to, start.Operand, start.Segment, start.T, end.T);
                }
            }
        }

        return new Piece(from, to, -1, -1, 0, 0);
    }

    // Neighboring curve pieces of one segment that run the same way are one piece. The last piece may join the first one.
    private static List<Piece> Merge(List<Piece> pieces)
    {
        var merged = new List<Piece>(pieces.Count);
        foreach (var piece in pieces)
        {
            if (merged.Count > 0 && merged[^1].Joins(piece))
            {
                merged[^1] = merged[^1] with { To = piece.To, EndT = piece.EndT };
            }
            else
            {
                merged.Add(piece);
            }
        }

        if (merged.Count > 1 && merged[^1].Joins(merged[0]))
        {
            merged[0] = merged[0] with { From = merged[^1].From, StartT = merged[^1].StartT };
            merged.RemoveAt(merged.Count - 1);
        }

        return merged;
    }

    private static DocumentSubPath Build(IReadOnlyList<(double X, double Y)> polygon, List<Piece> pieces, SourceCurves sources)
    {
        var curves = pieces.Select(piece => Curve(polygon, piece, sources)).ToList();
        var nodes = new List<DocumentNode>(pieces.Count);
        for (var k = 0; k < pieces.Count; k++)
        {
            var point = polygon[pieces[k].From];
            var before = curves[(k - 1 + pieces.Count) % pieces.Count];
            var after = curves[k];
            var handleIn = before is { } b && !Same(b.P2, (point.X, point.Y)) ? b.P2 : ((double X, double Y)?)null;
            var handleOut = after is { } a && !Same(a.P1, (point.X, point.Y)) ? a.P1 : ((double X, double Y)?)null;
            var node = new DocumentNode(point.X, point.Y, handleIn?.X, handleIn?.Y, handleOut?.X, handleOut?.Y);
            nodes.Add(new DocumentNode(node.X, node.Y, node.InX, node.InY, node.OutX, node.OutY, NodeTypes.Derive(node)));
        }

        return new DocumentSubPath(nodes, isClosed: true);
    }

    // The piece of the original curve between the two t values, running from the first to the last point of the piece, or null for a line.
    private static Cubic? Curve(IReadOnlyList<(double X, double Y)> polygon, Piece piece, SourceCurves sources)
    {
        if (piece.Segment < 0 || sources.Curve(piece.Operand, piece.Segment) is not { } source)
        {
            return null;
        }

        var low = Math.Min(piece.StartT, piece.EndT);
        var high = Math.Max(piece.StartT, piece.EndT);
        var (head, _) = Bezier.Split(source, high);
        var part = low <= 0 ? head : Bezier.Split(head, low / high).Right;
        if (piece.EndT < piece.StartT)
        {
            part = new Cubic(part.P3, part.P2, part.P1, part.P0);
        }

        // The ends are the points of the polygon, so neighboring pieces share exactly one node.
        var start = polygon[piece.From];
        var end = polygon[piece.To];
        return new Cubic((start.X, start.Y), part.P1, part.P2, (end.X, end.Y));
    }

    private static DocumentSubPath Lines(IReadOnlyList<(double X, double Y)> polygon) =>
        new(polygon.Select(point => new DocumentNode(point.X, point.Y)).ToList(), isClosed: true);

    private static bool Same((double X, double Y) a, (double X, double Y) b) => Math.Abs(a.X - b.X) < 1e-9 && Math.Abs(a.Y - b.Y) < 1e-9;

    // Segment is -1 for a line.
    private readonly record struct Piece(int From, int To, int Operand, int Segment, double StartT, double EndT)
    {
        // The next piece continues this one on the same curve segment in the same direction.
        public bool Joins(Piece next) =>
            Segment >= 0
            && next.Segment == Segment
            && next.Operand == Operand
            && Math.Abs(next.StartT - EndT) <= SameT
            && Math.Sign(next.EndT - next.StartT) == Math.Sign(EndT - StartT);
    }
}
