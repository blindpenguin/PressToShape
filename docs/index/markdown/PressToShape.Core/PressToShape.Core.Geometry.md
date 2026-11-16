# PressToShape.Core.Geometry

Project: PressToShape.Core

## Bezier (class)

`public static class Bezier`

Source: `src/PressToShape.Core/Geometry/Bezier.cs:7-192`

The curve math for cubic bezier segments. Bounds, hit tests, display, export and point insertion all use it.

**Members**

- `public (double X, double Y) Point(Cubic curve, double t)` - The point of the curve at t, from 0 (start) to 1 (end).
- `public (Cubic Left, Cubic Right) Split(Cubic curve, double t)` - Splits the curve at t into two curves that together draw the same shape (de Casteljau).
- `public Bounds ExtremaBounds(Cubic curve)` - The tightest box around the curve itself. Handle points that lie off the curve do not count, but the extremes between the end points do.
- `public IReadOnlyList<(double X, double Y)> Flatten(Cubic curve, double tolerance)` - Approximates the curve with a polyline whose points are at most about tolerance away from the curve. The result starts with the start point and ends with the end point.
- `public double NearestParameter(Cubic curve, double x, double y)` - The parameter of the point of the curve that is closest to the given point.

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

## Cubic (record)

`public readonly record Cubic : IEquatable<Cubic>`

Source: `src/PressToShape.Core/Geometry/Cubic.cs:8-8`

A cubic bezier segment in document units: from P0 to P3, shaped by two handle points. A line is a cubic whose handle points lie on its end points.

**Members**

- `public Cubic((double X, double Y) P0, (double X, double Y) P1, (double X, double Y) P2, (double X, double Y) P3)` - A cubic bezier segment in document units: from P0 to P3, shaped by two handle points. A line is a cubic whose handle points lie on its end points.
- `public (double X, double Y) P0 { get; init; }` - Start point.
- `public (double X, double Y) P1 { get; init; }` - Handle point of the start.
- `public (double X, double Y) P2 { get; init; }` - Handle point of the end.
- `public (double X, double Y) P3 { get; init; }` - End point.
