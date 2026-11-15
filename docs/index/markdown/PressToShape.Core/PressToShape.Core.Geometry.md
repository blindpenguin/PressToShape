# PressToShape.Core.Geometry

Project: PressToShape.Core

## Bounds (record)

`public readonly record Bounds : IEquatable<Bounds>`

Source: `src/PressToShape.Core/Geometry/Bounds.cs:8-8`

An axis-aligned rectangle in document units. Origin is the artboard's top-left corner, y grows downward.

**Members**

- `public Bounds(double X, double Y, double Width, double Height)` - An axis-aligned rectangle in document units. Origin is the artboard's top-left corner, y grows downward.
- `public double Height { get; init; }` - Height, not negative.
- `public double Width { get; init; }` - Width, not negative.
- `public double X { get; init; }` - Left edge.
- `public double Y { get; init; }` - Top edge.
