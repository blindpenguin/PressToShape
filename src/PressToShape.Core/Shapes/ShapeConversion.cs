using System.Collections.Generic;

namespace PressToShape.Core.Shapes;

/// <summary>Turns shapes into paths without changing how they look, so they can be edited node by node.</summary>
public static class ShapeConversion
{
    // The distance of the handle points of the usual four-segment circle from the node, in radii.
    private const double Circle = 0.5522847498307936;

    /// <summary>
    /// A path that looks like the shape and keeps its id, name, flags, box, rotation and appearance. A rectangle becomes four corner nodes
    /// (closed), plus one more node for each rounded corner, which is two nodes joined by a curve. An ellipse becomes four symmetric nodes
    /// at the top, right, bottom and left with handles of 0.2761 of the width or the height, the usual circle approximation. Returns <c>null</c> for a path, a group and every other type, which need no conversion.
    /// </summary>
    public static PathShape? ToPath(Shape shape)
    {
        return shape switch
        {
            RectangleShape rectangle => Carried(rectangle, FromRectangle(rectangle)),
            EllipseShape ellipse => Carried(ellipse, FromEllipse(ellipse)),
            _ => null,
        };
    }

    // A corner with a radius becomes two nodes at the ends of its arc, joined by a curve with the circle handles. Square corners stay one
    // node, so a rectangle without radii gives the same four nodes as ever.
    private static PathShape FromRectangle(RectangleShape rectangle)
    {
        List<DocumentNode> nodes = [];
        foreach (var corner in RoundedRectangleGeometry.Corners(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, rectangle.EffectiveRadii))
        {
            if (corner.Radius <= 0)
            {
                nodes.Add(new DocumentNode(corner.Point.X, corner.Point.Y));
                continue;
            }

            nodes.Add(new DocumentNode(
                corner.Start.X,
                corner.Start.Y,
                outX: corner.Start.X + (corner.Point.X - corner.Start.X) * Circle,
                outY: corner.Start.Y + (corner.Point.Y - corner.Start.Y) * Circle));
            nodes.Add(new DocumentNode(
                corner.End.X,
                corner.End.Y,
                inX: corner.End.X + (corner.Point.X - corner.End.X) * Circle,
                inY: corner.End.Y + (corner.Point.Y - corner.End.Y) * Circle));
        }

        return PathShape.Create(nodes, isClosed: true);
    }

    private static PathShape FromEllipse(EllipseShape ellipse)
    {
        var radiusX = ellipse.Width / 2;
        var radiusY = ellipse.Height / 2;
        var centerX = ellipse.CenterX;
        var centerY = ellipse.CenterY;
        var handleX = Circle * radiusX;
        var handleY = Circle * radiusY;
        List<DocumentNode> nodes =
        [
            new(centerX, centerY - radiusY, centerX - handleX, centerY - radiusY, centerX + handleX, centerY - radiusY, NodeType.Symmetric),
            new(centerX + radiusX, centerY, centerX + radiusX, centerY - handleY, centerX + radiusX, centerY + handleY, NodeType.Symmetric),
            new(centerX, centerY + radiusY, centerX + handleX, centerY + radiusY, centerX - handleX, centerY + radiusY, NodeType.Symmetric),
            new(centerX - radiusX, centerY, centerX - radiusX, centerY + handleY, centerX - radiusX, centerY - handleY, NodeType.Symmetric),
        ];
        return PathShape.Create(nodes, isClosed: true);
    }

    // The box is set again from the shape, so it is exactly the same however the curve extremes round.
    private static PathShape Carried(Shape shape, PathShape path) =>
        path with
        {
            Id = shape.Id,
            Name = shape.Name,
            IsVisible = shape.IsVisible,
            IsLocked = shape.IsLocked,
            X = shape.X,
            Y = shape.Y,
            Width = shape.Width,
            Height = shape.Height,
            Rotation = shape.Rotation,
            Appearance = shape.Appearance,
        };
}
