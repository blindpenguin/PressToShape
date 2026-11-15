# PressToShape.Core.Documents

Project: PressToShape.Core

## Document (record)

`public sealed record Document : IEquatable<Document>`

Source: `src/PressToShape.Core/Documents/Document.cs:13-370`

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
- `public Document RemoveLayer(Guid layerId)` - Returns a document without the layer and its shapes.
- `public Document UpdateLayer(Guid layerId, Func<Layer, Layer> change)` - Returns a document in which the layer is replaced by the result of change.
- `public Document AddShape(Guid layerId, Shape shape)` - Returns a document with the shape added above the existing shapes of the layer.
- `public Document UpdateShape(Guid shapeId, Func<Shape, Shape> change)` - Returns a document in which the shape is replaced by the result of change. Its place in the stack stays.
- `public Document AddShapes(Guid layerId, IEnumerable<Shape> shapes)` - Returns a document with the shapes added above the existing shapes of the layer, in the given order. One pass, however many shapes.
- `public Document UpdateShapes(IReadOnlyDictionary<Guid, Shape> replacements)` - Returns a document in which every shape whose id is a key of replacements is replaced by its value. Their places in the stack stay. One pass over the document, however many shapes change.
- `public Document RemoveShapes(IReadOnlySet<Guid> shapeIds)` - Returns a document without the shapes with these ids. One pass over the document.
- `public Document RemoveShape(Guid shapeId)` - Returns a document without the shape.
- `public ShapeLocation? FindShape(Guid shapeId)` - Finds a shape and the layer that holds it. Returns null if no shape has this id.
- `public IReadOnlyList<Shape> HitTest(double x, double y)` - Returns the shapes whose geometry contains the point, topmost first. Shapes of hidden layers are skipped. Shapes of locked layers are included, tools decide what to do with them. The stroke width is ignored.

**Relationships**

- Injected into `PressToShape.App.Workspace.EditorState`
- Injected into `PressToShape.Core.History.DocumentHistory`
- Injected into `PressToShape.Core.IO.ProjectFileResult`

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

- Injected into `PressToShape.App.ViewModels.Editor.LayerItemViewModel`
- Injected into `PressToShape.Core.Documents.ShapeLocation`

## ShapeLocation (record)

`public sealed record ShapeLocation : IEquatable<ShapeLocation>`

Source: `src/PressToShape.Core/Documents/ShapeLocation.cs:8-8`

The result of looking up a shape: the shape and the layer that holds it.

**Members**

- `public ShapeLocation(Layer Layer, Shape Shape)` - The result of looking up a shape: the shape and the layer that holds it.
- `public Layer Layer { get; init; }` - The layer that holds the shape.
- `public Shape Shape { get; init; }` - The shape.

**Relationships**

- Depends on (constructor) `PressToShape.Core.Documents.Layer`
- Depends on (constructor) `PressToShape.Core.Shapes.Shape`
