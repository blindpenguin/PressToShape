using System;
using System.Collections.Generic;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// Turns a document into the shapes to draw on the canvas, so the drawing control needs no geometry rules.
/// </summary>
internal static class CanvasScene
{
    /// <summary>
    /// Builds the shapes in drawing order: layers bottom first, shapes bottom first, so later shapes cover earlier ones.
    /// Hidden layers are left out. Locked layers are drawn like any other.
    /// </summary>
    /// <param name="document">The document to draw.</param>
    /// <param name="zoomPercent">Zoom in percent. 100 means one document unit per screen pixel.</param>
    /// <param name="originX">Screen position of the artboard's left edge.</param>
    /// <param name="originY">Screen position of the artboard's top edge.</param>
    public static IReadOnlyList<ScreenShape> Build(Document document, double zoomPercent, double originX, double originY)
    {
        ArgumentNullException.ThrowIfNull(document);

        var shapes = new List<ScreenShape>();
        foreach (var layer in document.Layers)
        {
            if (!layer.IsVisible)
            {
                continue;
            }

            foreach (var shape in layer.Shapes)
            {
                shapes.Add(BuildShape(shape, zoomPercent, originX, originY));
            }
        }

        return shapes;
    }

    /// <summary>Converts one shape, for example a shape that is still being drawn and is not in a document.</summary>
    public static ScreenShape BuildShape(Shape shape, double zoomPercent, double originX, double originY)
    {
        ArgumentNullException.ThrowIfNull(shape);

        var scale = zoomPercent / 100;
        var type = shape switch
        {
            RectangleShape => ScreenShapeType.Rectangle,
            EllipseShape => ScreenShapeType.Ellipse,
            _ => throw new NotSupportedException($"Shape type {shape.GetType().Name} cannot be drawn."),
        };

        var appearance = shape.Appearance;
        var hasStroke = appearance.Stroke is not null && appearance.StrokeWidth > 0;

        return new ScreenShape(
            type,
            CanvasMapping.ToScreen(shape.X, originX, zoomPercent),
            CanvasMapping.ToScreen(shape.Y, originY, zoomPercent),
            shape.Width * scale,
            shape.Height * scale,
            appearance.Fill,
            hasStroke ? appearance.Stroke : null,
            hasStroke ? appearance.StrokeWidth * scale : 0,
            appearance.Opacity);
    }

    /// <summary>
    /// The screen rectangle of the selected shape's bounds (the stroke width is not included).
    /// Returns <c>null</c> without an id, if no shape has the id, or if its layer is hidden.
    /// </summary>
    public static ScreenRect? SelectionOutline(Document document, Guid? shapeId, double zoomPercent, double originX, double originY)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (shapeId is not { } id || document.FindShape(id) is not { } location || !location.Layer.IsVisible)
        {
            return null;
        }

        var shape = location.Shape;
        var scale = zoomPercent / 100;
        return new ScreenRect(
            CanvasMapping.ToScreen(shape.X, originX, zoomPercent),
            CanvasMapping.ToScreen(shape.Y, originY, zoomPercent),
            shape.Width * scale,
            shape.Height * scale);
    }
}
