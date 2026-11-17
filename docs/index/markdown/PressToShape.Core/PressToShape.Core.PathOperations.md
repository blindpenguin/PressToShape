# PressToShape.Core.PathOperations

Project: PressToShape.Core

## BooleanEngine (class)

`public static class BooleanEngine`

Source: `src/PressToShape.Core/PathOperations/BooleanEngine.cs:18-95`

Combines the outlines of shapes with a boolean operation. The curves are flattened, the polygons are clipped with the Clipper2 package, and the result is turned back into nodes (see PressToShape.Core.PathOperations.CurveFitting), keeping unchanged pieces of the original curves. This is the only class that uses Clipper2, and its interface is plain geometry, so other features can use it as well. The same input always gives the same result.

**Members**

- `public int BackgroundNodeLimit` - The total node count of the operands above which a caller should run the operation off the UI thread. From the benchmark PathOperationBenchmarks (spec boolean-operations, task 19): a union of two wavy outlines took 72 ms at 40,000 nodes in total and 1,030 ms at 200,000, so it reaches about 200 ms, the budget of the requirements, at about 70,000.
- `public double Tolerance` - How far the flattened polygons may be from the curves, in document units.
- `public IReadOnlyList<DocumentSubPath> Compute(PathOperationType type, IReadOnlyList<Outline> operands, CancellationToken cancellationToken = default(CancellationToken))` - Combines the operands from the bottom one up: the result so far is combined with the next operand, so a difference cuts every upper operand out of the bottom one, and an exclusion keeps the areas that an odd number of operands cover.

## CurveFitting (class)

`internal static class CurveFitting`

Source: `src/PressToShape.Core/PathOperations/CurveFitting.cs:14-149`

Turns the polygons of a clipped result back into nodes. An edge of the polygon that is a piece of a flattened original curve becomes that piece of the original curve again (a cubic cut out of it), and neighboring edges on the same curve become one cubic. Every other edge is a straight line, which is within the tolerance of the exact result because the polygon is.

**Members**

- `public DocumentSubPath Fit(IReadOnlyList<(double X, double Y)> polygon, SourceCurves sources)` - The closed sub-path of a polygon.

## Outline (record)

`public sealed record Outline : IEquatable<Outline>`

Source: `src/PressToShape.Core/PathOperations/Outline.cs:12-12`

The outline of one operand of a boolean operation: its sub-paths in document coordinates, with the rotation already applied, and the fill rule that tells what is inside. An open sub-path counts as closed by a straight line. Plain geometry, no shape or document.

**Members**

- `public Outline(IReadOnlyList<DocumentSubPath> SubPaths, FillRule FillRule)` - The outline of one operand of a boolean operation: its sub-paths in document coordinates, with the rotation already applied, and the fill rule that tells what is inside. An open sub-path counts as closed by a straight line. Plain geometry, no shape or document.
- `public FillRule FillRule { get; init; }` - How the inside is decided across the sub-paths.
- `public IReadOnlyList<DocumentSubPath> SubPaths { get; init; }` - The sub-paths, at least one.

**Relationships**

- Injected into `PressToShape.Core.Documents.PathShapeOperations.Operand`

## PathOperationType (enum)

`public enum PathOperationType`

Source: `src/PressToShape.Core/PathOperations/PathOperationType.cs:4-17`

The boolean operation that combines the outlines of shapes.

**Members**

- `public Union` - The area covered by any operand.
- `public Difference` - The area of the bottom operand that no other operand covers.
- `public Intersection` - The area covered by every operand.
- `public Exclusion` - The area covered by an odd number of operands.

## SourceCurves (class)

`internal sealed class SourceCurves`

Source: `src/PressToShape.Core/PathOperations/SourceCurves.cs:12-146`

The curves of the operands of a boolean operation, flattened, with an index by position. After clipping, a point of the result is looked up here to find out whether it lies on an original curve segment and at which t, so unchanged curve pieces of the result can be written as the original curve again. Straight segments are not indexed, because they need no curve.

**Members**

- `public Cubic? Curve(int operand, int segment)` - The cubic of a curved segment, or null if the segment is straight or unknown.
- `public void Add(int operand, int firstSegment, IReadOnlyList<Cubic> segments, IReadOnlyList<FlatPoint> points)` - Adds one sub-path: its segments (see PressToShape.Core.Geometry.Flattening.Flatten) and the flattened points of them.
- `public IReadOnlyList<(int Operand, int Segment, double T)> Matches(double x, double y)` - Where a point lies on the flattened curves: for every curved segment that has an edge within reach of the point, the segment and the t of the nearest such point of the edge.
