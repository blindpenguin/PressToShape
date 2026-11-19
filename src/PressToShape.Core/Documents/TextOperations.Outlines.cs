using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.Documents;

public static partial class TextOperations
{
    /// <summary>
    /// The ids of the texts that <see cref="ConvertToOutlines"/> would change: the texts among the selected shapes and among the shapes
    /// inside selected groups (at any depth), each once, that are shown and not locked and that the outliner gives at least one and at
    /// most <see cref="PathShape.MaxSubPaths"/> contours for. Ids that are not shapes of the document are ignored.
    /// </summary>
    public static IReadOnlyList<Guid> ConvertibleTextIds(Document document, IEnumerable<Guid> ids, ITextOutliner outliner)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentNullException.ThrowIfNull(outliner);

        return [.. AffectedTexts(document, ids).Where(text => Contours(text, outliner) is not null).Select(text => text.Id)];
    }

    /// <summary>
    /// Replaces the texts found by <see cref="ConvertibleTextIds"/> with one compound path each (even-odd, so the counters of letters are holes).
    /// The path keeps id, name, flags, appearance, rotation and place in the stack, and it looks like the text. Returns the same document if
    /// there is nothing to convert.
    /// </summary>
    public static Document ConvertToOutlines(Document document, IEnumerable<Guid> ids, ITextOutliner outliner)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentNullException.ThrowIfNull(outliner);

        Dictionary<Guid, Shape> replacements = [];
        foreach (var text in AffectedTexts(document, ids))
        {
            if (Contours(text, outliner) is { } contours)
            {
                replacements[text.Id] = ToPath(text, contours);
            }
        }

        return document.UpdateShapes(replacements);
    }

    private static IReadOnlyList<DocumentSubPath>? Contours(TextShape text, ITextOutliner outliner) =>
        outliner.Outline(text) is { Count: > 0 and <= PathShape.MaxSubPaths } contours ? contours : null;

    // The contours are in the frame before the rotation, the text turns around the center of its box and a path around the center of its own
    // box. The path is moved so that its turning gives the same picture: d = (I - R) * (textCenter - pathCenter).
    private static PathShape ToPath(TextShape text, IReadOnlyList<DocumentSubPath> contours)
    {
        var path = PathShape.Create(contours, FillRule.EvenOdd);
        if (text.Rotation != 0)
        {
            var radians = text.Rotation * Math.PI / 180;
            var sin = Math.Sin(radians);
            var cos = Math.Cos(radians);
            var vx = text.CenterX - path.CenterX;
            var vy = text.CenterY - path.CenterY;
            path = path with { X = path.X + vx - (vx * cos - vy * sin), Y = path.Y + vy - (vx * sin + vy * cos) };
        }

        return path with
        {
            Id = text.Id,
            Name = text.Name,
            IsVisible = text.IsVisible,
            IsLocked = text.IsLocked,
            Appearance = text.Appearance,
            Rotation = text.Rotation,
        };
    }
}
