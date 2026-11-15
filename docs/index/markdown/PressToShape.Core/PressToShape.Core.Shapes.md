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

## RectangleShape (record)

`public sealed record RectangleShape : Shape, IEquatable<RectangleShape>`

Source: `src/PressToShape.Core/Shapes/RectangleShape.cs:4-9`

An axis-aligned rectangle.

**Members**

- `protected bool ContainsLocal(double x, double y)` - Whether the point lies inside the shape's geometry before any rotation is applied.

**Relationships**

- Inherits `PressToShape.Core.Shapes.Shape`

## Shape (record)

`public abstract record Shape : IEquatable<Shape>`

Source: `src/PressToShape.Core/Shapes/Shape.cs:10-158`

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
- `public Bounds Bounds { get; }` - The unturned box of the shape. The stroke width and the rotation are not included.
- `public double CenterX { get; }` - Horizontal center of the box. The shape turns around it.
- `public double CenterY { get; }` - Vertical center of the box. The shape turns around it.
- `public Bounds RotatedBounds { get; }` - The axis-aligned bounds of the turned box. Equal to PressToShape.Core.Shapes.Shape.Bounds without a rotation.
- `public bool Contains(double x, double y)` - Whether the point lies inside the turned shape's geometry or on its edge. The stroke is ignored.
- `protected bool ContainsLocal(double x, double y)` - Whether the point lies inside the shape's geometry before any rotation is applied.

**Relationships**

- Inherited by `PressToShape.Core.Shapes.EllipseShape`
- Inherited by `PressToShape.Core.Shapes.RectangleShape`
- Injected into `PressToShape.Core.Documents.ShapeLocation`
