using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.Documents;

/// <summary>The rules a document's layers must follow. A broken value never becomes a document.</summary>
internal static class DocumentValidation
{
    /// <summary>
    /// Returns the layers if they are valid: at least one layer, no null layer or shape, layer ids unique, shape and group ids unique
    /// over the whole tree, groups nested at most <see cref="ShapeTree.MaxDepth"/> levels, and every group with the box and the
    /// rotation that follow from its children.
    /// </summary>
    /// <exception cref="ArgumentException">A rule is broken.</exception>
    public static ImmutableArray<Layer> Validated(ImmutableArray<Layer> layers)
    {
        if (layers.IsDefaultOrEmpty)
        {
            throw new ArgumentException("A document needs at least one layer.", nameof(Document.Layers));
        }

        var layerIds = new HashSet<Guid>();
        var shapeIds = new HashSet<Guid>();
        foreach (var layer in layers)
        {
            if (layer is null)
            {
                throw new ArgumentException("A layer must not be null.", nameof(Document.Layers));
            }

            if (!layerIds.Add(layer.Id))
            {
                throw new ArgumentException($"Layer id {layer.Id} is used more than once.", nameof(Document.Layers));
            }

            // Layers and shapes share one id space, so a layer must not have the id of a shape of an earlier layer either.
            if (shapeIds.Contains(layer.Id))
            {
                throw new ArgumentException(MixedMessage(layer.Id), nameof(Document.Layers));
            }

            ValidateShapes(layer.Shapes, shapeIds, layerIds, 0);
        }

        return layers;
    }

    /// <summary>Returns the guides if they are valid: none null and the ids unique. A default array means no guides.</summary>
    /// <exception cref="ArgumentException">A rule is broken.</exception>
    public static ImmutableArray<Guide> ValidatedGuides(ImmutableArray<Guide> guides)
    {
        if (guides.IsDefault)
        {
            return [];
        }

        var ids = new HashSet<Guid>();
        foreach (var guide in guides)
        {
            if (guide is null)
            {
                throw new ArgumentException("A guide must not be null.", nameof(Document.Guides));
            }

            if (!ids.Add(guide.Id))
            {
                throw new ArgumentException($"Guide id {guide.Id} is used more than once.", nameof(Document.Guides));
            }
        }

        return guides;
    }

    private static string MixedMessage(Guid id) => $"Id {id} is used by a layer and by a shape.";

    private static void ValidateShapes(ImmutableArray<Shape> shapes, HashSet<Guid> shapeIds, HashSet<Guid> layerIds, int depth)
    {
        foreach (var shape in shapes)
        {
            if (shape is null)
            {
                throw new ArgumentException("A shape must not be null.", nameof(Document.Layers));
            }

            if (!shapeIds.Add(shape.Id))
            {
                throw new ArgumentException($"Shape id {shape.Id} is used more than once.", nameof(Document.Layers));
            }

            if (layerIds.Contains(shape.Id))
            {
                throw new ArgumentException(MixedMessage(shape.Id), nameof(Document.Layers));
            }

            if (shape is not GroupShape group)
            {
                continue;
            }

            if (depth + 1 > ShapeTree.MaxDepth)
            {
                throw new ArgumentException($"Groups are nested deeper than {ShapeTree.MaxDepth} levels.", nameof(Document.Layers));
            }

            // Setting the children again gives the box that follows from them. A group made with another box or rotation differs.
            var derived = group with { Children = group.Children };
            if (derived.Bounds != group.Bounds || group.Rotation != 0)
            {
                throw new ArgumentException($"The box of group {group.Id} does not follow from its children.", nameof(Document.Layers));
            }

            ValidateShapes(group.Children, shapeIds, layerIds, depth + 1);
        }
    }
}
