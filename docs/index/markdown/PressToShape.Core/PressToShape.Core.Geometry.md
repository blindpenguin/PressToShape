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

**Relationships**

- Injected into `PressToShape.App.Workspace.PathPointTargets.NearSegment`
- Injected into `PressToShape.App.Workspace.SmartBox`
- Injected into `PressToShape.App.Workspace.SmartSize`

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

**Relationships**

- Injected into `PressToShape.App.Workspace.PathPointTargets.NearSegment`

## FlatPoint (record)

`public readonly record FlatPoint : IEquatable<FlatPoint>`

Source: `src/PressToShape.Core/Geometry/FlatPoint.cs:12-12`

A point of a flattened outline together with where it came from: the curve point at T of a segment. The tags let a later step (the boolean operations) find out which pieces of the result are unchanged pieces of an original curve.

**Members**

- `public FlatPoint(double X, double Y, int Operand, int Segment, double T)` - A point of a flattened outline together with where it came from: the curve point at T of a segment. The tags let a later step (the boolean operations) find out which pieces of the result are unchanged pieces of an original curve.
- `public int Operand { get; init; }` - The index of the operand (the shape) the point comes from.
- `public int Segment { get; init; }` - The index of the segment, counted across all sub-paths of the operand.
- `public double T { get; init; }` - The position on the segment, from 0 (its start) to 1 (its end).
- `public double X { get; init; }` - The x position in document units.
- `public double Y { get; init; }` - The y position in document units.

## Flattening (class)

`public static class Flattening`

Source: `src/PressToShape.Core/Geometry/Flattening.cs:7-77`

Turns the cubic segments of a sub-path into a polyline whose points know where on which segment they lie. Deterministic.

**Members**

- `public IReadOnlyList<FlatPoint> Flatten(IReadOnlyList<Cubic> segments, bool isClosed, int operand, int firstSegment, double tolerance)` - The points of one sub-path as a polyline. A straight segment (a cubic whose handle points lie on its end points) gives only its start point. A curve is cut into equal steps of t, as many as keep the curve within tolerance of the polyline. Every segment gives its points from t = 0 up to, but not including, the end, because the end is the start of the next segment. Only the last segment of an open sub-path also gives its end point, so a closed sub-path does not repeat its first point.
