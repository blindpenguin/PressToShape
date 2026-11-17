using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Geometry;

namespace PressToShape.Core.Shapes;

public sealed partial record PathShape
{
    // How far a polyline may be from the curve, in document units.
    private const double FlattenTolerance = 0.25;

    // The band around the outline that counts as a hit, at least. There is no zoom here, so it is in document units.
    private const double MinimumHitMargin = 4;

    /// <summary>
    /// A path with one sub-path of the given nodes (document coordinates, in the frame of this path before the rotation) that keeps the id, the
    /// appearance, the name, the flags and the rotation. See <see cref="Rebuilt(IReadOnlyList{DocumentSubPath}, FillRule)"/>.
    /// </summary>
    public PathShape Rebuilt(IReadOnlyList<DocumentNode> nodes, bool isClosed, FillRule fillRule) =>
        Rebuilt([new DocumentSubPath(nodes, isClosed)], fillRule);

    /// <summary>
    /// A path with the given sub-paths (document coordinates, in the frame of this path before the rotation) that keeps the id, the
    /// appearance, the name, the flags and the rotation. The box is recomputed from the nodes. Because the rotation turns the
    /// shape around the center of its box and the center moves when the box changes, the path is shifted so that every node stays
    /// at the same place on the screen as if the old box had been kept.
    /// </summary>
    public PathShape Rebuilt(IReadOnlyList<DocumentSubPath> subPaths, FillRule fillRule)
    {
        var fresh = Create(subPaths, fillRule);

        // Where the new box center appears on the screen: turn it around the old center, the way the old shape was turned.
        var (sin, cos) = SinCos(Rotation);
        var dx = fresh.CenterX - CenterX;
        var dy = fresh.CenterY - CenterY;
        var visibleX = CenterX + dx * cos - dy * sin;
        var visibleY = CenterY + dx * sin + dy * cos;

        // The new shape turns around its own center, so put that center where the old turning put it.
        return this with
        {
            X = fresh.X + (visibleX - fresh.CenterX),
            Y = fresh.Y + (visibleY - fresh.CenterY),
            Width = fresh.Width,
            Height = fresh.Height,
            SubPaths = fresh.SubPaths,
            FillRule = fillRule,
        };
    }

    /// <summary>
    /// Turns a position on the screen into the frame of the path before its rotation (the frame <see cref="Create"/> and
    /// <see cref="ToDocumentNodes"/> use): the inverse of the turn around the center of the box.
    /// </summary>
    public (double X, double Y) ToLocal(double x, double y)
    {
        var (sin, cos) = SinCos(-Rotation);
        var dx = x - CenterX;
        var dy = y - CenterY;
        return (CenterX + dx * cos - dy * sin, CenterY + dx * sin + dy * cos);
    }

    /// <summary>Turns a position in the frame of the path before its rotation into the position on the screen: the turn around the center of the box.</summary>
    public (double X, double Y) FromLocal(double x, double y)
    {
        var (sin, cos) = SinCos(Rotation);
        var dx = x - CenterX;
        var dy = y - CenterY;
        return (CenterX + dx * cos - dy * sin, CenterY + dx * sin + dy * cos);
    }

    /// <summary>Turns a distance on the screen into a distance in the frame of the path before its rotation.</summary>
    public (double Dx, double Dy) ToLocalDelta(double dx, double dy)
    {
        var (sin, cos) = SinCos(-Rotation);
        return (dx * cos - dy * sin, dx * sin + dy * cos);
    }

    /// <summary>
    /// Whether the point (before any rotation) hits the path. The inside counts for the closed sub-paths, and for all sub-paths if the path has
    /// a fill (an open sub-path with a fill is filled as if it were closed). The sub-paths are counted together with the fill rule, so a
    /// sub-path inside another one is a hole. An open sub-path, or any sub-path of a path with a stroke, is also hit near its outline: within half the
    /// stroke width, but at least a few document units. The closing line of an open sub-path is not part of the outline.
    /// </summary>
    protected override bool ContainsLocal(double x, double y)
    {
        var outlines = FlattenedOutlines();
        var filled = outlines.Where((_, index) => _subPaths[index].IsClosed || Appearance.Fill is not null).ToList();
        if (filled.Count > 0 && IsInside(filled, x, y))
        {
            return true;
        }

        var hasStroke = Appearance.Stroke is not null && Appearance.StrokeWidth > 0;
        var margin = Math.Max(hasStroke ? Appearance.StrokeWidth / 2 : 0, MinimumHitMargin);
        for (var i = 0; i < outlines.Count; i++)
        {
            if ((!_subPaths[i].IsClosed || hasStroke) && IsNearOutline(outlines[i], x, y, margin))
            {
                return true;
            }
        }

        return false;
    }

    // Every sub-path as one polyline in document coordinates. A closed sub-path ends where it starts.
    private List<List<(double X, double Y)>> FlattenedOutlines()
    {
        var outlines = new List<List<(double X, double Y)>>(_subPaths.Length);
        foreach (var subPath in _subPaths)
        {
            var points = new List<(double X, double Y)>();
            foreach (var segment in BuildSegments(ToDocumentNodes(subPath), subPath.IsClosed))
            {
                var flat = Bezier.Flatten(segment, FlattenTolerance);
                for (var i = points.Count == 0 ? 0 : 1; i < flat.Count; i++)
                {
                    points.Add(flat[i]);
                }
            }

            outlines.Add(points);
        }

        return outlines;
    }

    // Each polygon is closed implicitly from its last point back to its first, and all of them count together.
    private bool IsInside(List<List<(double X, double Y)>> polygons, double x, double y)
    {
        var winding = 0;
        var crossings = 0;
        foreach (var polygon in polygons)
        {
            for (var i = 0; i < polygon.Count; i++)
            {
                var a = polygon[i];
                var b = polygon[(i + 1) % polygon.Count];

                if ((a.Y > y) != (b.Y > y))
                {
                    var crossX = a.X + (y - a.Y) * (b.X - a.X) / (b.Y - a.Y);
                    if (x < crossX)
                    {
                        crossings++;
                        winding += b.Y > a.Y ? 1 : -1;
                    }
                }
            }
        }

        return FillRule == FillRule.EvenOdd ? crossings % 2 == 1 : winding != 0;
    }

    private static bool IsNearOutline(List<(double X, double Y)> outline, double x, double y, double margin)
    {
        var limit = margin * margin;
        for (var i = 0; i + 1 < outline.Count; i++)
        {
            if (DistanceSquaredToSegment(outline[i], outline[i + 1], x, y) <= limit)
            {
                return true;
            }
        }

        return false;
    }

    private static double DistanceSquaredToSegment((double X, double Y) a, (double X, double Y) b, double x, double y)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var lengthSquared = dx * dx + dy * dy;
        var t = lengthSquared == 0 ? 0 : Math.Clamp(((x - a.X) * dx + (y - a.Y) * dy) / lengthSquared, 0, 1);
        var nearX = a.X + t * dx - x;
        var nearY = a.Y + t * dy - y;
        return nearX * nearX + nearY * nearY;
    }
}
