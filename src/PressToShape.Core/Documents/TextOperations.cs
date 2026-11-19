using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.Documents;

/// <summary>
/// The operations on texts in a document: create, change the content, change the formatting and take over a measured size. The result is a
/// new document, the input is never changed. A result that equals the input is the same instance, so the caller can tell that nothing changed.
/// </summary>
public static partial class TextOperations
{
    /// <summary>Returns a document with the text added above the existing shapes of a layer or of a group.</summary>
    /// <param name="document">The document.</param>
    /// <param name="containerId">The id of a layer or of a group.</param>
    /// <param name="text">The text.</param>
    /// <exception cref="KeyNotFoundException">No layer or group has this id.</exception>
    /// <exception cref="ArgumentException">A shape with the id of the text already exists.</exception>
    public static Document Create(Document document, Guid containerId, TextShape text)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(text);

        return document.AddShapes(containerId, [text]);
    }

    /// <summary>
    /// Replaces the content of a text. Locks and visibility are not checked here, the tool decides what a lock means.
    /// Returns the same document if the content is already this.
    /// </summary>
    /// <exception cref="KeyNotFoundException">No shape has this id.</exception>
    /// <exception cref="ArgumentException">The shape is not a text, or the content is longer than <see cref="TextShape.MaxTextLength"/>.</exception>
    public static Document SetText(Document document, Guid id, string content)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(content);

        var text = Find(document, id);
        return text.Text == content ? document : document.UpdateShape(id, _ => text with { Text = content });
    }

    /// <summary>
    /// Applies a format change to the texts among the selected shapes and among the shapes inside selected groups (at any depth), each once,
    /// that are shown and not locked (the layer and the groups around them count). Other shapes and ids that are not shapes are ignored.
    /// Returns the same document if no text changes.
    /// </summary>
    /// <exception cref="ArgumentException">A value of the change is outside the limits of <see cref="TextShape"/>.</exception>
    public static Document SetFormat(Document document, IEnumerable<Guid> ids, TextFormatChange change)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentNullException.ThrowIfNull(change);

        Dictionary<Guid, Shape> replacements = [];
        foreach (var text in AffectedTexts(document, ids))
        {
            var changed = change.ApplyTo(text);
            if (changed != text)
            {
                replacements[text.Id] = changed;
            }
        }

        return document.UpdateShapes(replacements);
    }

    /// <summary>
    /// Takes over the size of a measured layout for a point text, which has no box of its own. The top left corner of the turned text stays
    /// where it is on the screen, so a rotated text does not drift while it grows. An area text keeps its box, the same document is returned,
    /// and so it is if the size is already this.
    /// </summary>
    /// <exception cref="KeyNotFoundException">No shape has this id.</exception>
    /// <exception cref="ArgumentException">The shape is not a text.</exception>
    public static Document ApplyLayout(Document document, Guid id, TextLayoutResult layout)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(layout);

        var text = Find(document, id);
        var fitted = Fit(text, layout);
        return fitted == text ? document : document.UpdateShape(id, _ => fitted);
    }

    /// <summary>
    /// The text with the size of a measured layout, if it is a point text (it has no box of its own). The top left corner of the turned text
    /// stays where it is on the screen, so a rotated text does not drift while it grows. An area text keeps its box and is returned as it is,
    /// and so is a point text that already has this size.
    /// </summary>
    public static TextShape Fit(TextShape text, TextLayoutResult layout)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(layout);

        if (text.IsArea || (text.Width == layout.Width && text.Height == layout.Height))
        {
            return text;
        }

        var (x, y) = text.Rotation == 0 ? (text.X, text.Y) : TopLeftKeepingCorner(text, layout.Width, layout.Height);
        return text with { X = x, Y = y, Width = layout.Width, Height = layout.Height };
    }

    private static TextShape Find(Document document, Guid id)
    {
        var location = document.FindShape(id) ?? throw new KeyNotFoundException($"No shape with id {id}.");
        return location.Shape as TextShape ?? throw new ArgumentException("The shape is not a text.", nameof(id));
    }

    // The turned top left corner is center + R * (-w/2, -h/2). It must stay, so the new center is that corner + R * (w'/2, h'/2).
    private static (double X, double Y) TopLeftKeepingCorner(TextShape text, double width, double height)
    {
        var radians = text.Rotation * Math.PI / 180;
        var sin = Math.Sin(radians);
        var cos = Math.Cos(radians);
        var cornerX = text.CenterX + (-text.Width / 2) * cos - (-text.Height / 2) * sin;
        var cornerY = text.CenterY + (-text.Width / 2) * sin + (-text.Height / 2) * cos;
        var centerX = cornerX + (width / 2) * cos - (height / 2) * sin;
        var centerY = cornerY + (width / 2) * sin + (height / 2) * cos;
        return (centerX - width / 2, centerY - height / 2);
    }

    // The text leaves of the selected shapes (a group gives its leaves), each once, that are shown and not locked.
    private static List<TextShape> AffectedTexts(Document document, IEnumerable<Guid> ids)
    {
        var seen = new HashSet<Guid>();
        var texts = new List<TextShape>();
        foreach (var id in ids)
        {
            if (ShapeTree.Find(document.Layers, id) is not { } found)
            {
                continue;
            }

            foreach (var leaf in found.Shape.Leaves().OfType<TextShape>())
            {
                if (seen.Add(leaf.Id) && document.FindShape(leaf.Id) is { IsEffectivelyVisible: true, IsEffectivelyLocked: false })
                {
                    texts.Add(leaf);
                }
            }
        }

        return texts;
    }
}
