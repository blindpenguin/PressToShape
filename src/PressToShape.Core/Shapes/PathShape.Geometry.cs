using System;
using System.Collections.Generic;
using PressToShape.Core.Geometry;

namespace PressToShape.Core.Shapes;

public sealed partial record PathShape
{
    // How far a polyline may be from the curve, in document units.
    private const double FlattenTolerance = 0.25;

    // The band around the outline that counts as a hit, at least. There is no zoom here, so it is in document units.
    private const double MinimumHitMargin = 4;

    /// <summary>
    /// A path with the given nodes (document coordinates, in the frame of this path before the rotation) that keeps the id, the
    /// appearance, the name, the flags and the rotation. The box is recomputed from the nodes. Because the rotation turns the
    /// shape around the center of its box and the center moves when the box changes, the path is shifted so that every node stays
    /// at the same place on the screen as if the old box had been kept.
    /// </summary>
    public PathShape Rebuilt(IReadOnlyList<DocumentNode> nodes, bool isClosed, FillRule fillRule)
    {
        var fresh = Create(nodes, isClosed, fillRule);

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
            Nodes = fresh.Nodes,
            IsClosed = isClosed,
            FillRule = fillRule,
        };
    }

    /// <summary>
    /// Whether the point (before any rotation) hits the path. The inside counts for a closed path and for an open path with a fill,
    /// which is filled as if it were closed. A path that is open or has a stroke is also hit near its outline: within half the
    /// stroke width, but at least a few document units. The closing line of an open path is not part of the outline.
    /// </summary>
    protected override bool ContainsLocal(double x, double y)
    {
        var outline = FlattenedOutline();
        if ((IsClosed || Appearance.Fill is not null) && IsInside(outline, x, y))
        {
            return true;
        }

        var hasStroke = Appearance.Stroke is not null && Appearance.StrokeWidth > 0;
        if (IsClosed && !hasStroke)
        {
            return false;
        }

        var margin = Math.Max(hasStroke ? Appearance.StrokeWidth / 2 : 0, MinimumHitMargin);
        return IsNearOutline(outline, x, y, margin);
    }

    // All segments as one polyline in document coordinates. A closed path ends where it starts.
    private List<(double X, double Y)> FlattenedOutline()
    {
        var points = new List<(double X, double Y)>();
        foreach (var segment in Segments())
        {
            var flat = Bezier.Flatten(segment, FlattenTolerance);
            for (var i = points.Count == 0 ? 0 : 1; i < flat.Count; i++)
            {
                points.Add(flat[i]);
            }
        }

        return points;
    }

    // The polygon is closed implicitly from the last point back to the first.
    private bool IsInside(List<(double X, double Y)> polygon, double x, double y)
    {
        var winding = 0;
        var crossings = 0;
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
