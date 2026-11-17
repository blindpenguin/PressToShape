# PressToShape.Core.Documents

Project: PressToShape.Core

## ArrangeOperations (class)

`internal static class ArrangeOperations`

Source: `src/PressToShape.Core/Documents/ArrangeOperations.cs:10-126`

Changes the stacking order of shapes among their siblings (the shapes of one layer or of one group). Nothing changes its parent.

**Members**

- `public Document Arrange(Document document, IReadOnlySet<Guid> ids, ArrangeType type)` - Moves the shapes. Shapes with different parents are arranged each among their own siblings. The selected siblings keep their order among themselves. Returns the same document if no shape changes its place.
- `public Document MoveAbove(Document document, Guid id, Guid targetId)` - Puts the shape directly above another shape that has the same parent. Returns the same document if it is there already.

## ArrangeType (enum)

`public enum ArrangeType`

Source: `src/PressToShape.Core/Documents/ArrangeType.cs:4-17`

How PressToShape.Core.Documents.Document.Arrange changes the stacking order of shapes among their siblings.

**Members**

- `public ToFront` - Above all siblings.
- `public Forward` - One place up, past the next sibling that is not moved.
- `public Backward` - One place down, past the next sibling that is not moved.
- `public ToBack` - Below all siblings.

## Document (record)

`public sealed record Document : IEquatable<Document>`

Source: `src/PressToShape.Core/Documents/Document.cs:13-407`

A drawing: an artboard and a stack of layers. Immutable: every edit method returns a new document and leaves this one unchanged. Unchanged layers are shared between the two. A document always has at least one layer.

**Members**

- `public double ArtboardWidth { get; init; }` - Artboard width in document units. Finite and greater than zero.
- `public double ArtboardHeight { get; init; }` - Artboard height in document units. Finite and greater than zero.
- `public ImmutableArray<Layer> Layers { get; init; }` - The layers in stacking order, bottom first. At least one layer, and layer and shape ids are unique in the document.
- `public ImmutableArray<Color> Palette { get; init; }` - The swatches saved with the document, in the order the user added them. No colour appears twice.
- `public Document Create(string firstLayerName)` - Creates a 1200 x 800 document with one visible, unlocked layer.
- `public Document WithArtboardSize(double width, double height)` - Returns a document with a new artboard size.
- `public Document AddSwatch(Color color)` - Returns a document with the colour added at the end of the palette. Returns this document if the palette has the colour.
- `public Document RemoveSwatchAt(int index)` - Returns a document without the swatch at this index.
- `public Document AddLayer(Layer layer)` - Returns a document with the layer added above all existing layers.
- `public Document InsertLayer(int index, Layer layer)` - Returns a document with the layer inserted so that it has this index (0 is the bottom, the layer count is the top).
- `public Document MoveLayerToIndex(Guid layerId, int index)` - Returns a document in which the layer has this index (0 is the bottom). Returns this document if it has it already.
- `public Document MoveLayerAbove(Guid layerId, Guid targetId)` - Returns a document in which the layer is directly above targetId. Returns this document if it is there already.
- `public Document DuplicateLayer(Guid layerId, Guid newLayerId, string name, Func<Guid> newShapeId)` - Returns a document with a copy of the layer directly above it: same flags, a copy of every shape and group below it, and new ids for the copy and everything in it.
- `public Document MergeLayerDown(Guid layerId)` - Returns a document in which the layer's shapes are on top of the layer below and the layer is gone. The lower layer keeps its id, name and flags.
- `public Document MoveShapesToLayer(IReadOnlySet<Guid> ids, Guid layerId)` - Returns a document in which the shapes, nested ones included, are top-level shapes at the top of the layer, in their old document order. A group that loses its last child goes with it. Returns this document if ids is empty.
- `public Document RemoveLayer(Guid layerId)` - Returns a document without the layer and its shapes.
- `public Document UpdateLayer(Guid layerId, Func<Layer, Layer> change)` - Returns a document in which the layer is replaced by the result of change.
- `public Document AddShape(Guid layerId, Shape shape)` - Returns a document with the shape added above the existing shapes of the layer.
- `public Document UpdateShape(Guid shapeId, Func<Shape, Shape> change)` - Returns a document in which the shape is replaced by the result of change. Its place in the stack stays.
- `public Document AddShapes(Guid containerId, IEnumerable<Shape> shapes)` - Returns a document with the shapes added above the existing shapes of a layer or of a group, in the given order. The box of a group and of its enclosing groups follows. One pass, however many shapes.
- `public Document UpdateShapes(IReadOnlyDictionary<Guid, Shape> replacements)` - Returns a document in which every shape or group whose id is a key of replacements is replaced by its value. Their places in the stack stay, and the boxes of the groups around them follow. One pass over the document, however many shapes change.
- `public Document RemoveShapes(IReadOnlySet<Guid> shapeIds)` - Returns a document without the shapes with these ids, wherever they are. A group that loses its last child goes with it. One pass over the document.
- `public Document RemoveShape(Guid shapeId)` - Returns a document without the shape.
- `public ShapeLocation? FindShape(Guid shapeId)` - Finds a shape or group, wherever it is, with its layer and its closest group. Returns null if no shape has this id.
- `public IReadOnlyList<Shape> HitTest(double x, double y)` - Returns the shapes whose geometry contains the point, topmost first. A group is never returned, its shapes are. Shapes of hidden layers, hidden shapes and the shapes of hidden groups are skipped. Locked shapes and the shapes of locked layers and groups are included, tools decide what to do with them. The stroke width is ignored.
- `public Document Group(IReadOnlyList<Guid> ids, Guid groupId)` - Returns a document in which the shapes are replaced by one new group with the id groupId. The group takes the stack position of the topmost shape, and the shapes keep their order.
- `public Document Ungroup(IReadOnlySet<Guid> groupIds)` - Returns a document in which each group is replaced by its children, in place, one level. The group's own name and flags are dropped.
- `public Document Arrange(IReadOnlySet<Guid> ids, ArrangeType type)` - Returns a document in which the shapes are moved in the stacking order of their siblings (the shapes of the same layer or group). Shapes with different parents are arranged each among their own siblings, and the moved shapes keep their order. No shape changes its parent. Returns this document if nothing changes.
- `public Document MoveAbove(Guid id, Guid targetId)` - Returns a document in which the shape is directly above targetId. Returns this document if it is there already.
- `public Document SetHidden(IReadOnlySet<Guid> ids, bool hidden)` - Returns a document in which the shapes are hidden (hidden true) or shown. A group's flag is its own, its children keep theirs. Returns this document if nothing changes.
- `public Document SetLocked(IReadOnlySet<Guid> ids, bool locked)` - Returns a document in which the shapes are locked or unlocked. A group's flag is its own. Returns this document if nothing changes.
- `public Document ShowAll()` - Returns a document in which every shape and group, nested ones included, is shown. Layers are not changed.
- `public Document UnlockAll()` - Returns a document in which every shape and group, nested ones included, is unlocked. Layers are not changed.
- `public Document Rename(Guid id, string name)` - Returns a document in which the shape has this name. An empty or blank name means "no name". Returns this document if the name is the same.

**Relationships**

- Injected into `PressToShape.App.Workspace.EditorState`
- Injected into `PressToShape.Core.History.DocumentHistory`
- Injected into `PressToShape.Core.IO.ProjectFileResult`
- Injected into `PressToShape.Core.Tests.Documents.LayerOperationsTests.Fixture`

## DocumentValidation (class)

`internal static class DocumentValidation`

Source: `src/PressToShape.Core/Documents/DocumentValidation.cs:9-78`

The rules a document's layers must follow. A broken value never becomes a document.

**Members**

- `public ImmutableArray<Layer> Validated(ImmutableArray<Layer> layers)` - Returns the layers if they are valid: at least one layer, no null layer or shape, layer ids unique, shape and group ids unique over the whole tree, groups nested at most PressToShape.Core.Documents.ShapeTree.MaxDepth levels, and every group with the box and the rotation that follow from its children.

## FlagOperations (class)

`internal static class FlagOperations`

Source: `src/PressToShape.Core/Documents/FlagOperations.cs:10-78`

Hide, lock and rename shapes and groups. A group's flags are its own: its children keep theirs. Layers are not touched.

**Members**

- `public Document SetHidden(Document document, IReadOnlySet<Guid> ids, bool hidden)` - Shows or hides the shapes. Returns the same document if nothing changes.
- `public Document SetLocked(Document document, IReadOnlySet<Guid> ids, bool locked)` - Locks or unlocks the shapes. Returns the same document if nothing changes.
- `public Document ShowAll(Document document)` - Shows every shape and group, nested ones included. Returns the same document if everything is shown.
- `public Document UnlockAll(Document document)` - Unlocks every shape and group, nested ones included. Returns the same document if nothing is locked.
- `public Document Rename(Document document, Guid id, string name)` - Gives the shape a name. A name that is empty or only spaces becomes the empty string, which means "no name". Returns the same document if the name is the same.

## GroupOperations (class)

`internal static class GroupOperations`

Source: `src/PressToShape.Core/Documents/GroupOperations.cs:10-140`

Group and ungroup on a document. Documents are validated when they are made, so their groups are at most PressToShape.Core.Documents.ShapeTree.MaxDepth deep.

**Members**

- `public Document Group(Document document, IReadOnlyList<Guid> ids, Guid groupId)` - Replaces the shapes with one new group. The group takes the stack position of the topmost shape, and the shapes keep their order.
- `public Document Ungroup(Document document, IReadOnlySet<Guid> groupIds)` - Replaces each group by its children, in place and in order. One level for each group: a group inside a group that is not in groupIds stays a group. The groups' own names and flags are dropped.
- `internal Document WithSiblings(Document document, int layerIndex, GroupShape? parent, ImmutableArray<Shape> siblings)` - _(no documentation)_

## Layer (record)

`public sealed record Layer : IEquatable<Layer>`

Source: `src/PressToShape.Core/Documents/Layer.cs:11-45`

A named group of shapes that can be hidden or locked. Immutable: change it with with. Records compare PressToShape.Core.Documents.Layer.Shapes by reference, not by content.

**Members**

- `public Guid Id { get; init; }` - Identifier, unique within a document.
- `public string Name { get; init; }` - The layer's name. Not null, but may be empty. Core holds no default name, the caller provides it.
- `public bool IsVisible { get; init; }` - Whether the layer is shown. Hidden layers are not hit by PressToShape.Core.Documents.Document.HitTest.
- `public bool IsLocked { get; init; }` - Whether the layer is locked. Tools decide what a lock means, the model still reports hits.
- `public ImmutableArray<Shape> Shapes { get; init; }` - The shapes in stacking order, bottom first. Never the uninitialized default array.

**Relationships**

- Injected into `PressToShape.Core.Documents.ShapeLocation`
- Injected into `PressToShape.Core.Tests.Documents.LayerOperationsTests.Fixture`

## LayerOperations (class)

`internal static class LayerOperations`

Source: `src/PressToShape.Core/Documents/LayerOperations.cs:10-165`

Changes to the list of layers of a document: insert and reorder. Layers are listed bottom first.

**Members**

- `public Document Insert(Document document, int index, Layer layer)` - Inserts the layer so that it has this index afterwards (0 is the bottom, the layer count is the top).
- `public Document MoveToIndex(Document document, Guid layerId, int index)` - Moves the layer so that it has this index afterwards. Returns the same document if it has it already.
- `public Document MoveAbove(Document document, Guid layerId, Guid targetId)` - Puts the layer directly above the target layer. Returns the same document if it is there already.
- `public Document Duplicate(Document document, Guid layerId, Guid newLayerId, string name, Func<Guid> newShapeId)` - Inserts a copy of the layer directly above it, with the same flags and a copy of every shape below it. The copy, and each shape and group in it, gets a new id.
- `public Document MergeDown(Document document, Guid layerId)` - Moves the shapes of the layer onto the top of the layer below and removes the layer. The lower layer keeps its id, name and flags.
- `public Document MoveShapesToLayer(Document document, IReadOnlySet<Guid> ids, Guid layerId)` - Moves the shapes onto the top of the layer. Shapes inside groups leave their group, and a group that loses its last child goes with it. The shapes keep their order in the document: lower layers first, then stack order. A shape and its own group move together.

## ShapeLocation (record)

`public sealed record ShapeLocation : IEquatable<ShapeLocation>`

Source: `src/PressToShape.Core/Documents/ShapeLocation.cs:11-16`

The result of looking up a shape or group: the shape, the layer that holds it and the group it is in.

**Members**

- `public ShapeLocation(Layer Layer, Shape Shape, GroupShape? Parent = null, bool IsEffectivelyVisible = true, bool IsEffectivelyLocked = false)` - The result of looking up a shape or group: the shape, the layer that holds it and the group it is in.
- `public Layer Layer { get; init; }` - The layer that holds the shape.
- `public Shape Shape { get; init; }` - The shape or group.
- `public GroupShape? Parent { get; init; }` - The closest group around the shape, or null if it is on the top level of the layer.
- `public bool IsEffectivelyVisible { get; init; }` - Whether the shape is shown: it, every group around it and its layer are visible.
- `public bool IsEffectivelyLocked { get; init; }` - Whether the shape is locked: it, a group around it or its layer is locked.

**Relationships**

- Depends on (constructor) `PressToShape.Core.Documents.Layer`
- Depends on (constructor) `PressToShape.Core.Shapes.GroupShape`
- Depends on (constructor) `PressToShape.Core.Shapes.Shape`

## ShapeTree (class)

`internal static class ShapeTree`

Source: `src/PressToShape.Core/Documents/ShapeTree.cs:14-348`

The one place that walks nested shapes. All results are new values, the input is never changed. A group's box follows its children by itself (see PressToShape.Core.Shapes.GroupShape), so every method that changes children also updates the boxes of all ancestors. Every walk stops with an System.InvalidOperationException when groups are nested deeper than PressToShape.Core.Documents.ShapeTree.MaxDepth.

**Members**

- `public int MaxDepth` - The deepest nesting of groups that the model accepts.
- `public IReadOnlyList<Shape> Leaves(Shape shape)` - The shape itself if it is not a group, otherwise all non-group shapes below it, bottom first.
- `public Found? Find(ImmutableArray<Layer> layers, Guid id)` - Finds a shape (or group) by id in the layers. Returns null if no shape has this id.
- `public ImmutableArray<Shape> Replace(ImmutableArray<Shape> shapes, IReadOnlyDictionary<Guid, Shape> replacements, ref int replaced)` - Replaces every shape whose id is a key of replacements, anywhere in the list. Ancestors are rebuilt, so their boxes are right. Shapes inside a replaced shape are not visited. Counts the replaced shapes in replaced.
- `public ImmutableArray<Shape> Map(ImmutableArray<Shape> shapes, Func<Shape, Shape> change)` - Applies change to every shape and group in the list, children before the group around them. A change that returns its input leaves it alone. Returns the same array if nothing changes.
- `public ImmutableArray<Shape> Remove(ImmutableArray<Shape> shapes, IReadOnlySet<Guid> ids, ref int matched)` - Removes the shapes with these ids, anywhere in the list. A group that loses its last child is removed too, and the boxes of the other ancestors are updated. Counts in matched how many of the ids were found, descendants of a removed group included.
- `public void HitTest(ImmutableArray<Shape> shapes, double x, double y, List<Shape> hits)` - Adds the shapes at the point to hits, topmost first. Groups are entered, never returned. Hidden shapes and hidden groups are skipped.
- `public Shape Reidentify(Shape shape, Func<Guid> newId)` - A copy of the shape and everything below it, each with a new id from newId.
- `public Shape Translate(Shape shape, double dx, double dy)` - A copy of the shape moved by the distance. A group moves all its leaves.
- `public int Depth(Shape shape)` - The number of group levels below and including this shape: 0 for a plain shape, 1 for a group of plain shapes.
- `public (bool IsVisible, bool IsLocked) EffectiveState(Found found)` - Whether the shape is shown and whether it is locked, counting the enclosing groups: hidden or locked if the shape or any enclosing group is hidden or locked. Layers are not included.

### ShapeTree.Found (record)

`public readonly record ShapeTree.Found : IEquatable<Found>`

Source: `src/PressToShape.Core/Documents/ShapeTree.cs:23-27`

Where a shape was found: the layer, the enclosing groups from the outermost to the innermost, and the shape.

**Members**

- `public Found(int LayerIndex, ImmutableArray<GroupShape> Parents, Shape Shape)` - Where a shape was found: the layer, the enclosing groups from the outermost to the innermost, and the shape.
- `public int LayerIndex { get; init; }` - Index of the layer in the document.
- `public ImmutableArray<GroupShape> Parents { get; init; }` - The enclosing groups, outermost first. Empty for a shape on the top level of the layer.
- `public Shape Shape { get; init; }` - The shape.
- `public GroupShape? Parent { get; }` - The closest enclosing group, or null on the top level.

**Relationships**

- Depends on (constructor) `PressToShape.Core.Shapes.Shape`
