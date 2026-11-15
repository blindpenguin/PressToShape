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

        var scale = zoomPercent / 100;
        var shapes = new List<ScreenShape>();
        foreach (var layer in document.Layers)
        {
            if (!layer.IsVisible)
            {
                continue;
            }

            foreach (var shape in layer.Shapes)
            {
                shapes.Add(ToScreen(shape, scale, originX, originY));
            }
        }

        return shapes;
    }

    private static ScreenShape ToScreen(Shape shape, double scale, double originX, double originY)
    {
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
            originX + shape.X * scale,
            originY + shape.Y * scale,
            shape.Width * scale,
            shape.Height * scale,
            appearance.Fill,
            hasStroke ? appearance.Stroke : null,
            hasStroke ? appearance.StrokeWidth * scale : 0,
            appearance.Opacity);
    }
}
