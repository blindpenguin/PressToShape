# PressToShape.Core.Shapes

Project: PressToShape.Core

## Appearance (record)

`public sealed record Appearance : IEquatable<Appearance>`

Source: `src/PressToShape.Core/Shapes/Appearance.cs:4-35`

How a shape looks: solid fill, solid stroke and opacity. Values are validated on creation and on with.

**Members**

- `public Appearance Default { get; }` - The appearance of a new shape: a blue fill, a dark gray stroke of width 1 and full opacity.
- `public Color? Fill { get; init; }` - Fill color, or null for no fill.
- `public Color? Stroke { get; init; }` - Stroke color, or null for no stroke.
- `public double StrokeWidth { get; init; }` - Stroke width in document units. Finite and not negative. Zero is allowed.
- `public double Opacity { get; init; }` - Opacity from 0 (invisible) to 1 (opaque).

## Color (record)

`public readonly record Color : IEquatable<Color>`

Source: `src/PressToShape.Core/Shapes/Color.cs:8-12`

An sRGB color with 8 bits per channel. Alpha 255 is fully opaque.

**Members**

- `public Color(byte R, byte G, byte B, byte A)` - An sRGB color with 8 bits per channel. Alpha 255 is fully opaque.
- `public byte A { get; init; }` - Alpha channel, 0 (transparent) to 255 (opaque).
- `public byte B { get; init; }` - Blue channel, 0 to 255.
- `public byte G { get; init; }` - Green channel, 0 to 255.
- `public byte R { get; init; }` - Red channel, 0 to 255.
- `public Color FromRgb(byte r, byte g, byte b)` - Creates a fully opaque color from red, green and blue.

## EllipseShape (record)

`public sealed record EllipseShape : Shape, IEquatable<EllipseShape>`

Source: `src/PressToShape.Core/Shapes/EllipseShape.cs:4-28`

An axis-aligned ellipse that fills its bounds.

**Members**

- `protected bool ContainsLocal(double x, double y)` - Whether the point lies inside the shape's geometry before any rotation is applied.

**Relationships**

- Inherits `PressToShape.Core.Shapes.Shape`

## GroupShape (record)

`public sealed record GroupShape : Shape, IEquatable<GroupShape>`

Source: `src/PressToShape.Core/Shapes/GroupShape.cs:13-77`

A group of shapes that is selected, moved and styled as one. Children keep their own document coordinates, so a group has no transform. Its box is the union of the rotated bounds of its children, always upright (PressToShape.Core.Shapes.Shape.Rotation is 0), and follows PressToShape.Core.Shapes.GroupShape.Children by itself. Do not set the box, the rotation or the appearance of a group: change its children. Immutable: change it with with.

**Members**

- `public ImmutableArray<Shape> Children { get; init; }` - The children in stacking order, bottom first. At least one, none null, nested groups allowed.
- `protected bool ContainsLocal(double x, double y)` - Whether the point lies on a visible child. A hidden child is not hit.

**Relationships**

- Inherits `PressToShape.Core.Shapes.Shape`
- Injected into `PressToShape.App.Tests.CanvasInteractionGroupTests.Scene`
- Injected into `PressToShape.Core.Documents.ShapeLocation`

## RectangleShape (record)

`public sealed record RectangleShape : Shape, IEquatable<RectangleShape>`

Source: `src/PressToShape.Core/Shapes/RectangleShape.cs:4-9`

An axis-aligned rectangle.

**Members**

- `protected bool ContainsLocal(double x, double y)` - Whether the point lies inside the shape's geometry before any rotation is applied.

**Relationships**

- Inherits `PressToShape.Core.Shapes.Shape`
- Injected into `PressToShape.App.Tests.CanvasInteractionGroupTests.Scene`

## Shape (record)

`public abstract record Shape : IEquatable<Shape>`

Source: `src/PressToShape.Core/Shapes/Shape.cs:12-188`

A drawable shape with a position, a size and an appearance. Immutable: change it with with. Values are validated on creation and on with.

**Members**

- `private protected Shape()` - _(no documentation)_
- `public Guid Id { get; init; }` - Identifier, unique within a document. Stays the same when the shape is edited.
- `public double X { get; init; }` - Left edge in document units.
- `public double Y { get; init; }` - Top edge in document units.
- `public double Width { get; init; }` - Width, finite and not negative. Zero is allowed.
- `public double Height { get; init; }` - Height, finite and not negative. Zero is allowed.
- `public double Rotation { get; init; }` - Rotation in degrees around the center of the box. Positive angles turn clockwise on the screen (y grows downward). Finite. Stored normalized to at least 0 and less than 360. The default is 0.
- `public Appearance Appearance { get; init; }` - Fill, stroke and opacity.
- `public string Name { get; init; }` - The name the user gave the object. Not null. Empty means the object has no name of its own, and the app shows a default text.
- `public bool IsVisible { get; init; }` - Whether the object is shown. A hidden object is not drawn, not hit and not exported, and neither are the objects inside a hidden group.
- `public bool IsLocked { get; init; }` - Whether the object is locked. Tools decide what a lock means, the model still reports hits.
- `public IReadOnlyList<Shape> Leaves()` - The shape itself if it is not a group, otherwise all shapes below the group that are not groups, bottom first.
- `public Shape MovedCopy(Func<Guid> newId, double dx, double dy)` - A copy of the shape, and of everything below it if it is a group, each with a new id from newId and moved by the distance. For a group the children get their ids first, then the group.
- `public Bounds Bounds { get; }` - The unturned box of the shape. The stroke width and the rotation are not included.
- `public double CenterX { get; }` - Horizontal center of the box. The shape turns around it.
- `public double CenterY { get; }` - Vertical center of the box. The shape turns around it.
- `public Bounds RotatedBounds { get; }` - The axis-aligned bounds of the turned box. Equal to PressToShape.Core.Shapes.Shape.Bounds without a rotation.
- `public bool Contains(double x, double y)` - Whether the point lies inside the turned shape's geometry or on its edge. The stroke is ignored.
- `protected bool ContainsLocal(double x, double y)` - Whether the point lies inside the shape's geometry before any rotation is applied.

**Relationships**

- Inherited by `PressToShape.Core.Shapes.EllipseShape`
- Inherited by `PressToShape.Core.Shapes.GroupShape`
- Inherited by `PressToShape.Core.Shapes.RectangleShape`
- Injected into `PressToShape.App.Workspace.SelectionQueries.Node`
- Injected into `PressToShape.Core.Documents.ShapeLocation`
- Injected into `PressToShape.Core.Documents.ShapeTree.Found`
