using System;
using System.Collections.Generic;
using PressToShape.Core.Shapes;
using SkiaSharp;

namespace PressToShape.App.Services;

/// <summary>Reads the contours of a Skia path (the outline of a glyph) into sub-paths of the document.</summary>
internal static class GlyphOutlines
{
    private const double Tolerance = 1e-6;

    // A quadratic curve becomes a cubic one with the handles at two thirds of the way to the control point.
    private const double TwoThirds = 2.0 / 3.0;

    /// <summary>
    /// The contours of the path, moved by the offset. Every contour is closed: it ends at its start, or a closing line joins it. Quadratic
    /// curves become cubic ones, conic curves are cut into quadratic ones. A contour with fewer than two points is left out.
    /// </summary>
    public static IReadOnlyList<DocumentSubPath> FromPath(SKPath path, double offsetX, double offsetY)
    {
        ArgumentNullException.ThrowIfNull(path);

        List<DocumentSubPath> contours = [];
        List<NodeBuilder> nodes = [];
        var points = new SKPoint[4];
        using var iterator = path.CreateRawIterator();
        SKPathVerb verb;
        while ((verb = iterator.Next(points)) != SKPathVerb.Done)
        {
            switch (verb)
            {
                case SKPathVerb.Move:
                    Finish(contours, nodes);
                    nodes = [new NodeBuilder(points[0].X + offsetX, points[0].Y + offsetY)];
                    break;
                case SKPathVerb.Line:
                    nodes.Add(new NodeBuilder(points[1].X + offsetX, points[1].Y + offsetY));
                    break;
                case SKPathVerb.Quad:
                    AddQuad(nodes, points[1], points[2], offsetX, offsetY);
                    break;
                case SKPathVerb.Conic:
                    foreach (var (control, end) in ConicQuads(points[0], points[1], points[2], iterator.ConicWeight()))
                    {
                        AddQuad(nodes, control, end, offsetX, offsetY);
                    }

                    break;
                case SKPathVerb.Cubic:
                    AddCubic(nodes, points[1], points[2], points[3], offsetX, offsetY);
                    break;
                case SKPathVerb.Close:
                    Finish(contours, nodes);
                    nodes = [];
                    break;
            }
        }

        Finish(contours, nodes);
        return contours;
    }

    private static void AddQuad(List<NodeBuilder> nodes, SKPoint control, SKPoint end, double offsetX, double offsetY)
    {
        var start = nodes[^1];
        double cx = control.X + offsetX;
        double cy = control.Y + offsetY;
        double ex = end.X + offsetX;
        double ey = end.Y + offsetY;
        start.OutX = start.X + TwoThirds * (cx - start.X);
        start.OutY = start.Y + TwoThirds * (cy - start.Y);
        nodes.Add(new NodeBuilder(ex, ey) { InX = ex + TwoThirds * (cx - ex), InY = ey + TwoThirds * (cy - ey) });
    }

    private static void AddCubic(List<NodeBuilder> nodes, SKPoint control1, SKPoint control2, SKPoint end, double offsetX, double offsetY)
    {
        var start = nodes[^1];
        start.OutX = control1.X + offsetX;
        start.OutY = control1.Y + offsetY;
        nodes.Add(new NodeBuilder(end.X + offsetX, end.Y + offsetY) { InX = control2.X + offsetX, InY = control2.Y + offsetY });
    }

    private static IEnumerable<(SKPoint Control, SKPoint End)> ConicQuads(SKPoint start, SKPoint control, SKPoint end, float weight)
    {
        var quads = SKPath.ConvertConicToQuads(start, control, end, weight, 1);
        for (var i = 1; i + 1 < quads.Length; i += 2)
        {
            yield return (quads[i], quads[i + 1]);
        }
    }

    // The contour is closed. When the last point is the first one, the two are the same node and the handle that arrived at the last one
    // belongs to the first one.
    private static void Finish(List<DocumentSubPath> contours, List<NodeBuilder> nodes)
    {
        if (nodes.Count > 1 && Math.Abs(nodes[^1].X - nodes[0].X) < Tolerance && Math.Abs(nodes[^1].Y - nodes[0].Y) < Tolerance)
        {
            nodes[0].InX = nodes[^1].InX;
            nodes[0].InY = nodes[^1].InY;
            nodes.RemoveAt(nodes.Count - 1);
        }

        if (nodes.Count >= 2)
        {
            contours.Add(new DocumentSubPath([.. nodes.ConvertAll(node => node.ToNode())], isClosed: true));
        }
    }

    private sealed class NodeBuilder(double x, double y)
    {
        public double X { get; } = x;

        public double Y { get; } = y;

        public double? InX { get; set; }

        public double? InY { get; set; }

        public double? OutX { get; set; }

        public double? OutY { get; set; }

        public DocumentNode ToNode() => new(X, Y, InX, InY, OutX, OutY);
    }
}
