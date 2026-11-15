using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.Documents;

/// <summary>
/// A drawing: an artboard and a stack of layers. Immutable: every edit method returns a new document and
/// leaves this one unchanged. Unchanged layers are shared between the two. A document always has at least one layer.
/// </summary>
public sealed record Document
{
    private readonly double _artboardWidth = 1200;
    private readonly double _artboardHeight = 800;
    private readonly ImmutableArray<Layer> _layers;

    // Private: a document needs a first layer, so it is created through Create.
    private Document()
    {
    }

    /// <summary>Artboard width in document units. Finite and greater than zero.</summary>
    public double ArtboardWidth
    {
        get => _artboardWidth;
        init => _artboardWidth = Guard.FinitePositive(value, nameof(ArtboardWidth));
    }

    /// <summary>Artboard height in document units. Finite and greater than zero.</summary>
    public double ArtboardHeight
    {
        get => _artboardHeight;
        init => _artboardHeight = Guard.FinitePositive(value, nameof(ArtboardHeight));
    }

    /// <summary>
    /// The layers in stacking order, bottom first. At least one layer, and layer and shape ids are unique in the document.
    /// </summary>
    public ImmutableArray<Layer> Layers
    {
        get => _layers;
        init => _layers = Validated(value);
    }

    /// <summary>Creates a 1200 x 800 document with one visible, unlocked layer.</summary>
    /// <param name="firstLayerName">The name of the first layer. Core has no default text, so the caller provides it.</param>
    public static Document Create(string firstLayerName)
    {
        ArgumentNullException.ThrowIfNull(firstLayerName);

        return new Document { Layers = [new Layer { Name = firstLayerName }] };
    }

    /// <summary>Returns a document with a new artboard size.</summary>
    public Document WithArtboardSize(double width, double height) =>
        this with { ArtboardWidth = width, ArtboardHeight = height };

    /// <summary>Returns a document with the layer added above all existing layers.</summary>
    /// <exception cref="ArgumentException">A layer or shape in <paramref name="layer"/> has an id that already exists.</exception>
    public Document AddLayer(Layer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);

        return this with { Layers = Layers.Add(layer) };
    }

    /// <summary>Returns a document without the layer and its shapes.</summary>
    /// <exception cref="KeyNotFoundException">No layer has this id.</exception>
    /// <exception cref="InvalidOperationException">The layer is the last one.</exception>
    public Document RemoveLayer(Guid layerId)
    {
        var index = IndexOfLayer(layerId);
        if (Layers.Length == 1)
        {
            throw new InvalidOperationException("The last layer cannot be removed.");
        }

        return this with { Layers = Layers.RemoveAt(index) };
    }

    /// <summary>Returns a document in which the layer is replaced by the result of <paramref name="change"/>.</summary>
    /// <exception cref="KeyNotFoundException">No layer has this id.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="change"/> returned nothing or a layer with another id.</exception>
    public Document UpdateLayer(Guid layerId, Func<Layer, Layer> change)
    {
        ArgumentNullException.ThrowIfNull(change);

        var index = IndexOfLayer(layerId);
        var changed = change(Layers[index]) ?? throw new InvalidOperationException("The change must return a layer.");
        if (changed.Id != layerId)
        {
            throw new InvalidOperationException("A layer's id cannot be changed.");
        }

        return this with { Layers = Layers.SetItem(index, changed) };
    }

    /// <summary>Returns a document with the shape added above the existing shapes of the layer.</summary>
    /// <exception cref="KeyNotFoundException">No layer has this id.</exception>
    /// <exception cref="ArgumentException">A shape with this id already exists in the document.</exception>
    public Document AddShape(Guid layerId, Shape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);

        var index = IndexOfLayer(layerId);
        var layer = Layers[index];

        return this with { Layers = Layers.SetItem(index, layer with { Shapes = layer.Shapes.Add(shape) }) };
    }

    /// <summary>Returns a document in which the shape is replaced by the result of <paramref name="change"/>. Its place in the stack stays.</summary>
    /// <exception cref="KeyNotFoundException">No shape has this id.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="change"/> returned nothing or a shape with another id.</exception>
    public Document UpdateShape(Guid shapeId, Func<Shape, Shape> change)
    {
        ArgumentNullException.ThrowIfNull(change);

        var (layerIndex, shapeIndex) = IndexOfShape(shapeId);
        var layer = Layers[layerIndex];
        var changed = change(layer.Shapes[shapeIndex]) ?? throw new InvalidOperationException("The change must return a shape.");
        if (changed.Id != shapeId)
        {
            throw new InvalidOperationException("A shape's id cannot be changed.");
        }

        return this with { Layers = Layers.SetItem(layerIndex, layer with { Shapes = layer.Shapes.SetItem(shapeIndex, changed) }) };
    }

    /// <summary>Returns a document with the shapes added above the existing shapes of the layer, in the given order. One pass, however many shapes.</summary>
    /// <exception cref="KeyNotFoundException">No layer has this id.</exception>
    /// <exception cref="ArgumentException">A shape has an id that already exists in the document.</exception>
    public Document AddShapes(Guid layerId, IEnumerable<Shape> shapes)
    {
        ArgumentNullException.ThrowIfNull(shapes);

        var index = IndexOfLayer(layerId);
        var layer = Layers[index];
        var added = layer.Shapes.AddRange(shapes);
        return added.Length == layer.Shapes.Length ? this : this with { Layers = Layers.SetItem(index, layer with { Shapes = added }) };
    }

    /// <summary>
    /// Returns a document in which every shape whose id is a key of <paramref name="replacements"/> is replaced by its value.
    /// Their places in the stack stay. One pass over the document, however many shapes change.
    /// </summary>
    /// <exception cref="KeyNotFoundException">A key is not the id of a shape.</exception>
    /// <exception cref="InvalidOperationException">A replacement has another id than its key.</exception>
    public Document UpdateShapes(IReadOnlyDictionary<Guid, Shape> replacements)
    {
        ArgumentNullException.ThrowIfNull(replacements);

        if (replacements.Count == 0)
        {
            return this;
        }

        var found = 0;
        var layers = Layers.ToBuilder();
        for (var layerIndex = 0; layerIndex < layers.Count; layerIndex++)
        {
            var shapes = layers[layerIndex].Shapes;
            ImmutableArray<Shape>.Builder? builder = null;
            for (var i = 0; i < shapes.Length; i++)
            {
                if (!replacements.TryGetValue(shapes[i].Id, out var replacement))
                {
                    continue;
                }

                if (replacement is null || replacement.Id != shapes[i].Id)
                {
                    throw new InvalidOperationException("A replacement must be a shape with the same id.");
                }

                builder ??= shapes.ToBuilder();
                builder[i] = replacement;
                found++;
            }

            if (builder is not null)
            {
                layers[layerIndex] = layers[layerIndex] with { Shapes = builder.MoveToImmutable() };
            }
        }

        if (found != replacements.Count)
        {
            throw new KeyNotFoundException("Some ids are not shapes of the document.");
        }

        return this with { Layers = layers.MoveToImmutable() };
    }

    /// <summary>Returns a document without the shapes with these ids. One pass over the document.</summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    public Document RemoveShapes(IReadOnlySet<Guid> shapeIds)
    {
        ArgumentNullException.ThrowIfNull(shapeIds);

        if (shapeIds.Count == 0)
        {
            return this;
        }

        var removed = 0;
        var layers = Layers.ToBuilder();
        for (var layerIndex = 0; layerIndex < layers.Count; layerIndex++)
        {
            var kept = layers[layerIndex].Shapes.Where(shape => !shapeIds.Contains(shape.Id)).ToImmutableArray();
            if (kept.Length != layers[layerIndex].Shapes.Length)
            {
                removed += layers[layerIndex].Shapes.Length - kept.Length;
                layers[layerIndex] = layers[layerIndex] with { Shapes = kept };
            }
        }

        if (removed != shapeIds.Count)
        {
            throw new KeyNotFoundException("Some ids are not shapes of the document.");
        }

        return this with { Layers = layers.MoveToImmutable() };
    }

    /// <summary>Returns a document without the shape.</summary>
    /// <exception cref="KeyNotFoundException">No shape has this id.</exception>
    public Document RemoveShape(Guid shapeId)
    {
        var (layerIndex, shapeIndex) = IndexOfShape(shapeId);
        var layer = Layers[layerIndex];

        return this with { Layers = Layers.SetItem(layerIndex, layer with { Shapes = layer.Shapes.RemoveAt(shapeIndex) }) };
    }

    /// <summary>Finds a shape and the layer that holds it. Returns <c>null</c> if no shape has this id.</summary>
    public ShapeLocation? FindShape(Guid shapeId)
    {
        foreach (var layer in Layers)
        {
            foreach (var shape in layer.Shapes)
            {
                if (shape.Id == shapeId)
                {
                    return new ShapeLocation(layer, shape);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the shapes whose geometry contains the point, topmost first. Shapes of hidden layers are skipped.
    /// Shapes of locked layers are included, tools decide what to do with them. The stroke width is ignored.
    /// </summary>
    public IReadOnlyList<Shape> HitTest(double x, double y)
    {
        var hits = new List<Shape>();
        for (var i = Layers.Length - 1; i >= 0; i--)
        {
            var layer = Layers[i];
            if (!layer.IsVisible)
            {
                continue;
            }

            for (var j = layer.Shapes.Length - 1; j >= 0; j--)
            {
                if (layer.Shapes[j].Contains(x, y))
                {
                    hits.Add(layer.Shapes[j]);
                }
            }
        }

        return hits;
    }

    private (int LayerIndex, int ShapeIndex) IndexOfShape(Guid shapeId)
    {
        for (var layerIndex = 0; layerIndex < Layers.Length; layerIndex++)
        {
            var shapes = Layers[layerIndex].Shapes;
            for (var shapeIndex = 0; shapeIndex < shapes.Length; shapeIndex++)
            {
                if (shapes[shapeIndex].Id == shapeId)
                {
                    return (layerIndex, shapeIndex);
                }
            }
        }

        throw new KeyNotFoundException($"No shape with id {shapeId}.");
    }

    private int IndexOfLayer(Guid layerId)
    {
        for (var i = 0; i < Layers.Length; i++)
        {
            if (Layers[i].Id == layerId)
            {
                return i;
            }
        }

        throw new KeyNotFoundException($"No layer with id {layerId}.");
    }

    private static ImmutableArray<Layer> Validated(ImmutableArray<Layer> layers)
    {
        if (layers.IsDefaultOrEmpty)
        {
            throw new ArgumentException("A document needs at least one layer.", nameof(Layers));
        }

        var layerIds = new HashSet<Guid>();
        var shapeIds = new HashSet<Guid>();
        foreach (var layer in layers)
        {
            if (layer is null)
            {
                throw new ArgumentException("A layer must not be null.", nameof(Layers));
            }

            if (!layerIds.Add(layer.Id))
            {
                throw new ArgumentException($"Layer id {layer.Id} is used more than once.", nameof(Layers));
            }

            foreach (var shape in layer.Shapes)
            {
                if (shape is null)
                {
                    throw new ArgumentException("A shape must not be null.", nameof(Layers));
                }

                if (!shapeIds.Add(shape.Id))
                {
                    throw new ArgumentException($"Shape id {shape.Id} is used more than once.", nameof(Layers));
                }
            }
        }

        return layers;
    }
}
