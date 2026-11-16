using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.Documents;

/// <summary>Hide, lock and rename shapes and groups. A group's flags are its own: its children keep theirs. Layers are not touched.</summary>
internal static class FlagOperations
{
    /// <summary>Shows or hides the shapes. Returns the same document if nothing changes.</summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    public static Document SetHidden(Document document, IReadOnlySet<Guid> ids, bool hidden) =>
        Change(document, ids, shape => shape.IsVisible == hidden ? shape with { IsVisible = !hidden } : shape);

    /// <summary>Locks or unlocks the shapes. Returns the same document if nothing changes.</summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    public static Document SetLocked(Document document, IReadOnlySet<Guid> ids, bool locked) =>
        Change(document, ids, shape => shape.IsLocked != locked ? shape with { IsLocked = locked } : shape);

    /// <summary>Shows every shape and group, nested ones included. Returns the same document if everything is shown.</summary>
    public static Document ShowAll(Document document) =>
        MapAll(document, shape => shape.IsVisible ? shape : shape with { IsVisible = true });

    /// <summary>Unlocks every shape and group, nested ones included. Returns the same document if nothing is locked.</summary>
    public static Document UnlockAll(Document document) =>
        MapAll(document, shape => shape.IsLocked ? shape with { IsLocked = false } : shape);

    /// <summary>
    /// Gives the shape a name. A name that is empty or only spaces becomes the empty string, which means "no name". Returns the same
    /// document if the name is the same.
    /// </summary>
    /// <exception cref="KeyNotFoundException">No shape has this id.</exception>
    public static Document Rename(Document document, Guid id, string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var trimmed = name.Trim();
        return Change(document, new HashSet<Guid> { id }, shape => shape.Name == trimmed ? shape : shape with { Name = trimmed });
    }

    private static Document Change(Document document, IReadOnlySet<Guid> ids, Func<Shape, Shape> change)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(ids);

        foreach (var id in ids)
        {
            if (ShapeTree.Find(document.Layers, id) is null)
            {
                throw new KeyNotFoundException($"No shape with id {id}.");
            }
        }

        return ids.Count == 0 ? document : MapAll(document, shape => ids.Contains(shape.Id) ? change(shape) : shape);
    }

    private static Document MapAll(Document document, Func<Shape, Shape> change)
    {
        ArgumentNullException.ThrowIfNull(document);

        var changed = false;
        var layers = document.Layers.Select(layer =>
        {
            var shapes = ShapeTree.Map(layer.Shapes, change);
            if (shapes == layer.Shapes)
            {
                return layer;
            }

            changed = true;
            return layer with { Shapes = shapes };
        }).ToImmutableArray();

        return changed ? document with { Layers = layers } : document;
    }
}
