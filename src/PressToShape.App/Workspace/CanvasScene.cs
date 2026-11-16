using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// Turns a document into the shapes to draw on the canvas, so the drawing control needs no geometry rules.
/// </summary>
internal static class CanvasScene
{
    /// <summary>
    /// Builds the shapes in drawing order: layers bottom first, shapes bottom first, so later shapes cover earlier ones. Groups are
    /// flattened: only the shapes in them are drawn, in order. Hidden layers, hidden shapes and the shapes of hidden groups are left out.
    /// Locked shapes are drawn like any other.
    /// </summary>
    /// <param name="document">The document to draw.</param>
    /// <param name="zoomPercent">Zoom in percent. 100 means one document unit per screen pixel.</param>
    /// <param name="originX">Screen position of the artboard's left edge.</param>
    /// <param name="originY">Screen position of the artboard's top edge.</param>
    /// <param name="enteredGroupId">The group that was entered. Shapes that are not inside it are marked as dimmed. <c>null</c> dims nothing.</param>
    /// <param name="hiddenShapeId">A shape that is left out because a draft takes its place, for example a path that is being continued. <c>null</c> leaves nothing out.</param>
    public static IReadOnlyList<ScreenShape> Build(Document document, double zoomPercent, double originX, double originY, Guid? enteredGroupId = null, Guid? hiddenShapeId = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        var shapes = new List<ScreenShape>();
        foreach (var layer in document.Layers)
        {
            if (layer.IsVisible)
            {
                AddShapes(layer.Shapes, shapes, zoomPercent, originX, originY, dimmed: enteredGroupId is not null, enteredGroupId, hiddenShapeId);
            }
        }

        return shapes;
    }

    private static void AddShapes(
        IEnumerable<Shape> source,
        List<ScreenShape> target,
        double zoomPercent,
        double originX,
        double originY,
        bool dimmed,
        Guid? enteredGroupId,
        Guid? hiddenShapeId)
    {
        foreach (var shape in source)
        {
            if (!shape.IsVisible || shape.Id == hiddenShapeId)
            {
                continue;
            }

            if (shape is GroupShape group)
            {
                // Inside the entered group nothing is dimmed, also not in the groups below it.
                AddShapes(group.Children, target, zoomPercent, originX, originY, dimmed && group.Id != enteredGroupId, enteredGroupId, hiddenShapeId);
            }
            else
            {
                target.Add(BuildShape(shape, zoomPercent, originX, originY) with { IsDimmed = dimmed });
            }
        }
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
            PathShape => ScreenShapeType.Path,
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
            appearance.Opacity,
            shape.Rotation,
            Path: shape is PathShape path ? BuildPath(path, scale) : null);
    }

    // Fractions of the box become screen pixels from the box's top left corner, so the control needs no knowledge of the model.
    private static ScreenPath BuildPath(PathShape path, double scale)
    {
        var width = path.Width * scale;
        var height = path.Height * scale;
        var nodes = ImmutableArray.CreateBuilder<ScreenPathNode>(path.Nodes.Length);
        foreach (var node in path.Nodes)
        {
            nodes.Add(new ScreenPathNode(
                node.X * width,
                node.Y * height,
                node.InX * width,
                node.InY * height,
                node.OutX * width,
                node.OutY * height));
        }

        return new ScreenPath(nodes.MoveToImmutable(), path.IsClosed, path.FillRule);
    }
}
