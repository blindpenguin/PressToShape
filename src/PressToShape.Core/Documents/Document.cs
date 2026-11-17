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
    private readonly ImmutableArray<Color> _palette = [];

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
        init => _layers = DocumentValidation.Validated(value);
    }

    /// <summary>The swatches saved with the document, in the order the user added them. No colour appears twice.</summary>
    public ImmutableArray<Color> Palette
    {
        get => _palette;
        init => _palette = value.IsDefault ? [] : value.Distinct().ToImmutableArray();
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

    /// <summary>Returns a document with the colour added at the end of the palette. Returns this document if the palette has the colour.</summary>
    public Document AddSwatch(Color color) =>
        _palette.Contains(color) ? this : this with { Palette = _palette.Add(color) };

    /// <summary>Returns a document without the swatch at this index.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is not a swatch of the palette.</exception>
    public Document RemoveSwatchAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _palette.Length);

        return this with { Palette = _palette.RemoveAt(index) };
    }

    /// <summary>Returns a document with the layer added above all existing layers.</summary>
    /// <exception cref="ArgumentException">A layer or shape in <paramref name="layer"/> has an id that already exists.</exception>
    public Document AddLayer(Layer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);

        return this with { Layers = Layers.Add(layer) };
    }

    /// <summary>Returns a document with the layer inserted so that it has this index (0 is the bottom, the layer count is the top).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is below 0 or above the layer count.</exception>
    /// <exception cref="ArgumentException">A layer or shape in <paramref name="layer"/> has an id that already exists.</exception>
    public Document InsertLayer(int index, Layer layer) => LayerOperations.Insert(this, index, layer);

    /// <summary>Returns a document in which the layer has this index (0 is the bottom). Returns this document if it has it already.</summary>
    /// <exception cref="KeyNotFoundException">No layer has this id.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The index is not the index of a layer.</exception>
    public Document MoveLayerToIndex(Guid layerId, int index) => LayerOperations.MoveToIndex(this, layerId, index);

    /// <summary>Returns a document in which the layer is directly above <paramref name="targetId"/>. Returns this document if it is there already.</summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a layer.</exception>
    /// <exception cref="InvalidOperationException">The ids are the same.</exception>
    public Document MoveLayerAbove(Guid layerId, Guid targetId) => LayerOperations.MoveAbove(this, layerId, targetId);

    /// <summary>
    /// Returns a document with a copy of the layer directly above it: same flags, a copy of every shape and group below it, and new ids
    /// for the copy and everything in it.
    /// </summary>
    /// <param name="layerId">The layer to copy.</param>
    /// <param name="newLayerId">The id of the copy.</param>
    /// <param name="name">The name of the copy. Core holds no default text, so the caller provides it.</param>
    /// <param name="newShapeId">Creates the id of each copied shape and group.</param>
    /// <exception cref="KeyNotFoundException">No layer has this id.</exception>
    /// <exception cref="ArgumentException">An id of the copy exists in the document already.</exception>
    public Document DuplicateLayer(Guid layerId, Guid newLayerId, string name, Func<Guid> newShapeId) =>
        LayerOperations.Duplicate(this, layerId, newLayerId, name, newShapeId);

    /// <summary>
    /// Returns a document in which the layer's shapes are on top of the layer below and the layer is gone. The lower layer keeps its id, name and flags.
    /// </summary>
    /// <exception cref="KeyNotFoundException">No layer has this id.</exception>
    /// <exception cref="InvalidOperationException">The layer is the bottom layer.</exception>
    public Document MergeLayerDown(Guid layerId) => LayerOperations.MergeDown(this, layerId);

    /// <summary>
    /// Returns a document in which the shapes, nested ones included, are top-level shapes at the top of the layer, in their old document
    /// order. A group that loses its last child goes with it. Returns this document if <paramref name="ids"/> is empty.
    /// </summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape, or no layer has the layer id.</exception>
    public Document MoveShapesToLayer(IReadOnlySet<Guid> ids, Guid layerId) => LayerOperations.MoveShapesToLayer(this, ids, layerId);

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

        var found = ShapeTree.Find(Layers, shapeId) ?? throw new KeyNotFoundException($"No shape with id {shapeId}.");
        var changed = change(found.Shape) ?? throw new InvalidOperationException("The change must return a shape.");
        if (changed.Id != shapeId)
        {
            throw new InvalidOperationException("A shape's id cannot be changed.");
        }

        return UpdateShapes(new Dictionary<Guid, Shape> { [shapeId] = changed });
    }

    /// <summary>
    /// Returns a document with the shapes added above the existing shapes of a layer or of a group, in the given order.
    /// The box of a group and of its enclosing groups follows. One pass, however many shapes.
    /// </summary>
    /// <param name="containerId">The id of a layer or of a group.</param>
    /// <param name="shapes">The shapes to add.</param>
    /// <exception cref="KeyNotFoundException">No layer or group has this id.</exception>
    /// <exception cref="ArgumentException">A shape has an id that already exists in the document.</exception>
    public Document AddShapes(Guid containerId, IEnumerable<Shape> shapes)
    {
        ArgumentNullException.ThrowIfNull(shapes);

        var layerIndex = FindLayerIndex(containerId);
        if (layerIndex >= 0)
        {
            var layer = Layers[layerIndex];
            var added = layer.Shapes.AddRange(shapes);
            return added.Length == layer.Shapes.Length ? this : this with { Layers = Layers.SetItem(layerIndex, layer with { Shapes = added }) };
        }

        var found = ShapeTree.Find(Layers, containerId);
        if (found?.Shape is not GroupShape group)
        {
            throw new KeyNotFoundException($"No layer or group with id {containerId}.");
        }

        var children = group.Children.AddRange(shapes);
        return children.Length == group.Children.Length
            ? this
            : UpdateShapes(new Dictionary<Guid, Shape> { [containerId] = group with { Children = children } });
    }

    /// <summary>
    /// Returns a document in which every shape or group whose id is a key of <paramref name="replacements"/> is replaced by its value.
    /// Their places in the stack stay, and the boxes of the groups around them follow. One pass over the document, however many
    /// shapes change.
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
            var shapes = ShapeTree.Replace(layers[layerIndex].Shapes, replacements, ref found);
            if (shapes != layers[layerIndex].Shapes)
            {
                layers[layerIndex] = layers[layerIndex] with { Shapes = shapes };
            }
        }

        if (found != replacements.Count)
        {
            throw new KeyNotFoundException("Some ids are not shapes of the document.");
        }

        foreach (var (id, replacement) in replacements)
        {
            if (replacement is null || replacement.Id != id)
            {
                throw new InvalidOperationException("A replacement must be a shape with the same id.");
            }
        }

        return this with { Layers = layers.MoveToImmutable() };
    }

    /// <summary>
    /// Returns a document without the shapes with these ids, wherever they are. A group that loses its last child goes with it.
    /// One pass over the document.
    /// </summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    public Document RemoveShapes(IReadOnlySet<Guid> shapeIds)
    {
        ArgumentNullException.ThrowIfNull(shapeIds);

        if (shapeIds.Count == 0)
        {
            return this;
        }

        var matched = 0;
        var layers = Layers.ToBuilder();
        for (var layerIndex = 0; layerIndex < layers.Count; layerIndex++)
        {
            var shapes = ShapeTree.Remove(layers[layerIndex].Shapes, shapeIds, ref matched);
            if (shapes != layers[layerIndex].Shapes)
            {
                layers[layerIndex] = layers[layerIndex] with { Shapes = shapes };
            }
        }

        if (matched != shapeIds.Count)
        {
            throw new KeyNotFoundException("Some ids are not shapes of the document.");
        }

        return this with { Layers = layers.MoveToImmutable() };
    }

    /// <summary>Returns a document without the shape.</summary>
    /// <exception cref="KeyNotFoundException">No shape has this id.</exception>
    public Document RemoveShape(Guid shapeId) => RemoveShapes(new HashSet<Guid> { shapeId });

    /// <summary>Finds a shape or group, wherever it is, with its layer and its closest group. Returns <c>null</c> if no shape has this id.</summary>
    public ShapeLocation? FindShape(Guid shapeId)
    {
        if (ShapeTree.Find(Layers, shapeId) is not { } found)
        {
            return null;
        }

        var layer = Layers[found.LayerIndex];
        var (visible, locked) = ShapeTree.EffectiveState(found);
        return new ShapeLocation(layer, found.Shape, found.Parent, visible && layer.IsVisible, locked || layer.IsLocked);
    }

    /// <summary>
    /// Returns the shapes whose geometry contains the point, topmost first. A group is never returned, its shapes are. Shapes of hidden
    /// layers, hidden shapes and the shapes of hidden groups are skipped. Locked shapes and the shapes of locked layers and groups are
    /// included, tools decide what to do with them. The stroke width is ignored.
    /// </summary>
    public IReadOnlyList<Shape> HitTest(double x, double y)
    {
        var hits = new List<Shape>();
        for (var i = Layers.Length - 1; i >= 0; i--)
        {
            if (Layers[i].IsVisible)
            {
                ShapeTree.HitTest(Layers[i].Shapes, x, y, hits);
            }
        }

        return hits;
    }

    /// <summary>
    /// Returns a document in which the shapes are replaced by one new group with the id <paramref name="groupId"/>. The group takes the stack
    /// position of the topmost shape, and the shapes keep their order.
    /// </summary>
    /// <exception cref="ArgumentException">Fewer than two ids, or an id twice, or <paramref name="groupId"/> is used already.</exception>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    /// <exception cref="InvalidOperationException">The shapes are not in the same layer and group.</exception>
    public Document Group(IReadOnlyList<Guid> ids, Guid groupId) => GroupOperations.Group(this, ids, groupId);

    /// <summary>
    /// Returns a document in which each group is replaced by its children, in place, one level. The group's own name and flags are dropped.
    /// </summary>
    /// <exception cref="ArgumentException">An id is the id of a shape that is not a group.</exception>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    public Document Ungroup(IReadOnlySet<Guid> groupIds) => GroupOperations.Ungroup(this, groupIds);

    /// <summary>
    /// Returns a document in which the shapes are moved in the stacking order of their siblings (the shapes of the same layer or group).
    /// Shapes with different parents are arranged each among their own siblings, and the moved shapes keep their order. No shape changes its
    /// parent. Returns this document if nothing changes.
    /// </summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    public Document Arrange(IReadOnlySet<Guid> ids, ArrangeType type) => ArrangeOperations.Arrange(this, ids, type);

    /// <summary>Returns a document in which the shape is directly above <paramref name="targetId"/>. Returns this document if it is there already.</summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    /// <exception cref="InvalidOperationException">The shapes have different parents, or are the same shape.</exception>
    public Document MoveAbove(Guid id, Guid targetId) => ArrangeOperations.MoveAbove(this, id, targetId);

    /// <summary>
    /// Returns a document in which the shapes are hidden (<paramref name="hidden"/> true) or shown. A group's flag is its own, its children
    /// keep theirs. Returns this document if nothing changes.
    /// </summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    public Document SetHidden(IReadOnlySet<Guid> ids, bool hidden) => FlagOperations.SetHidden(this, ids, hidden);

    /// <summary>Returns a document in which the shapes are locked or unlocked. A group's flag is its own. Returns this document if nothing changes.</summary>
    /// <exception cref="KeyNotFoundException">An id is not the id of a shape.</exception>
    public Document SetLocked(IReadOnlySet<Guid> ids, bool locked) => FlagOperations.SetLocked(this, ids, locked);

    /// <summary>Returns a document in which every shape and group, nested ones included, is shown. Layers are not changed.</summary>
    public Document ShowAll() => FlagOperations.ShowAll(this);

    /// <summary>Returns a document in which every shape and group, nested ones included, is unlocked. Layers are not changed.</summary>
    public Document UnlockAll() => FlagOperations.UnlockAll(this);

    /// <summary>Returns a document in which the shape has this name. An empty or blank name means "no name". Returns this document if the name is the same.</summary>
    /// <exception cref="KeyNotFoundException">No shape has this id.</exception>
    public Document Rename(Guid id, string name) => FlagOperations.Rename(this, id, name);

    private int IndexOfLayer(Guid layerId)
    {
        var index = FindLayerIndex(layerId);
        return index >= 0 ? index : throw new KeyNotFoundException($"No layer with id {layerId}.");
    }

    // The index of the layer, or -1 if no layer has this id.
    private int FindLayerIndex(Guid layerId)
    {
        for (var i = 0; i < Layers.Length; i++)
        {
            if (Layers[i].Id == layerId)
            {
                return i;
            }
        }

        return -1;
    }
}
