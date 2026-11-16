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

## DocumentNode (record)

`public readonly record DocumentNode : IEquatable<DocumentNode>`

Source: `src/PressToShape.Core/Shapes/DocumentNode.cs:7-50`

A point of a path in document units, in the frame of the path before its rotation. Used to build and edit a PressToShape.Core.Shapes.PathShape, which stores its nodes as fractions of its box (PressToShape.Core.Shapes.PathNode).

**Members**

- `public DocumentNode(double x, double y, double? inX = null, double? inY = null, double? outX = null, double? outY = null)` - Creates a node. Every number is finite, and a handle point is given with both numbers or with none.
- `public double X { get; }` - Anchor x in document units.
- `public double Y { get; }` - Anchor y in document units.
- `public double? InX { get; }` - In handle point x, or null together with PressToShape.Core.Shapes.DocumentNode.InY when there is no in handle.
- `public double? InY { get; }` - In handle point y.
- `public double? OutX { get; }` - Out handle point x, or null together with PressToShape.Core.Shapes.DocumentNode.OutY when there is no out handle.
- `public double? OutY { get; }` - Out handle point y.
- `public bool HasIn { get; }` - Whether the node has an in handle.
- `public bool HasOut { get; }` - Whether the node has an out handle.

## EllipseShape (record)

`public sealed record EllipseShape : Shape, IEquatable<EllipseShape>`

Source: `src/PressToShape.Core/Shapes/EllipseShape.cs:4-28`

An axis-aligned ellipse that fills its bounds.

**Members**

- `protected bool ContainsLocal(double x, double y)` - Whether the point lies inside the shape's geometry before any rotation is applied.

**Relationships**

- Inherits `PressToShape.Core.Shapes.Shape`

## FillRule (enum)

`public enum FillRule`

Source: `src/PressToShape.Core/Shapes/FillRule.cs:4-11`

How the inside of a path is decided where it crosses itself.

**Members**

- `public NonZero` - A point is inside if the path winds around it at all (SVG nonzero).
- `public EvenOdd` - A point is inside if a ray from it crosses the path an odd number of times (SVG evenodd).

## GroupShape (record)

`public sealed record GroupShape : Shape, IEquatable<GroupShape>`

Source: `src/PressToShape.Core/Shapes/GroupShape.cs:13-77`

A group of shapes that is selected, moved and styled as one. Children keep their own document coordinates, so a group has no transform. Its box is the union of the rotated bounds of its children, always upright (PressToShape.Core.Shapes.Shape.Rotation is 0), and follows PressToShape.Core.Shapes.GroupShape.Children by itself. Do not set the box, the rotation or the appearance of a group: change its children. Immutable: change it with with.

**Members**

- `public ImmutableArray<Shape> Children { get; init; }` - The children in stacking order, bottom first. At least one, none null, nested groups allowed.
- `protected bool ContainsLocal(double x, double y)` - Whether the point lies on a visible child. A hidden child is not hit.

**Relationships**

- Inherits `PressToShape.Core.Shapes.Shape`
- Injected into `PressToShape.App.Tests.CanvasInteractionContextMenuTests.Scene`
- Injected into `PressToShape.App.Tests.CanvasInteractionGroupTests.Scene`
- Injected into `PressToShape.Core.Documents.ShapeLocation`

## NodeValidation (class)

`internal static class NodeValidation`

Source: `src/PressToShape.Core/Shapes/NodeValidation.cs:6-30`

The checks shared by PressToShape.Core.Shapes.PathNode and PressToShape.Core.Shapes.DocumentNode, so both refuse the same values.

**Members**

- `public void Check(double x, double y, double? inX, double? inY, double? outX, double? outY)` - Checks the anchor and the two optional handle points. A handle point is given with both numbers or with none.

## PathEditing (class)

`public static class PathEditing`

Source: `src/PressToShape.Core/Shapes/PathEditing.cs:8-111`

Operations that change the nodes of a PressToShape.Core.Shapes.PathShape: add a point, delete a point, continue an open path. Every result is a new path, the input is never changed.

**Members**

- `public PathShape InsertNode(PathShape path, int segmentIndex, double t)` - Adds a node on a segment without changing the curve. A segment is the stretch from node segmentIndex to the next node (the first node again for the last segment of a closed path).
- `public PathShape? RemoveNode(PathShape path, int index)` - Removes a node. The neighbors connect, each keeping its own handle, so the curve changes a little. Returns null if fewer than two nodes would be left, in which case the caller should delete the shape.
- `public IReadOnlyList<DocumentNode> Continue(PathShape path, bool atStart)` - The nodes of an open path in document coordinates, ordered so that the end to continue from is the last one: as they are for the end of the path, reversed for its start. Reversing swaps the in and out handle of every node.

## PathNode (record)

`public readonly record PathNode : IEquatable<PathNode>`

Source: `src/PressToShape.Core/Shapes/PathNode.cs:9-52`

A point of a PressToShape.Core.Shapes.PathShape as it is stored: the anchor and the optional handle points are fractions of the path's box, so 0 is the left or top edge and 1 the right or bottom edge. Handle points may lie outside 0..1. Resizing the box scales the path without touching the nodes. A segment is a curve if the node it starts at has an out handle or the node it ends at has an in handle, otherwise it is a line.

**Members**

- `public PathNode(double x, double y, double? inX = null, double? inY = null, double? outX = null, double? outY = null)` - Creates a node. Every number is finite, and a handle point is given with both numbers or with none.
- `public double X { get; }` - Anchor, fraction of the box width.
- `public double Y { get; }` - Anchor, fraction of the box height.
- `public double? InX { get; }` - In handle point, fraction of the box width. Null together with PressToShape.Core.Shapes.PathNode.InY when there is no in handle.
- `public double? InY { get; }` - In handle point, fraction of the box height.
- `public double? OutX { get; }` - Out handle point, fraction of the box width. Null together with PressToShape.Core.Shapes.PathNode.OutY when there is no out handle.
- `public double? OutY { get; }` - Out handle point, fraction of the box height.
- `public bool HasIn { get; }` - Whether the node has an in handle.
- `public bool HasOut { get; }` - Whether the node has an out handle.

## PathShape (record)

`public partial sealed record PathShape : Shape, IEquatable<PathShape>`

Source: `src/PressToShape.Core/Shapes/PathShape.Geometry.cs:7-132`

A path of line and cubic bezier segments, open or closed. The box (PressToShape.Core.Shapes.Shape.X, PressToShape.Core.Shapes.Shape.Y, PressToShape.Core.Shapes.Shape.Width, PressToShape.Core.Shapes.Shape.Height) is the tight bounds of the curve, and the nodes are stored as fractions of that box (see PressToShape.Core.Shapes.PathNode), so changing the box with with moves, resizes and (with the rotation) turns the whole path, like any other shape. Build one from document coordinates with PressToShape.Core.Shapes.PathShape.Create. Immutable: change it with with.

**Members**

- `public ImmutableArray<PathNode> Nodes { get; init; }` - The nodes in drawing order, at least two. The segment from the last node back to the first exists only if PressToShape.Core.Shapes.PathShape.IsClosed.
- `public PathShape Rebuilt(IReadOnlyList<DocumentNode> nodes, bool isClosed, FillRule fillRule)` - A path with the given nodes (document coordinates, in the frame of this path before the rotation) that keeps the id, the appearance, the name, the flags and the rotation. The box is recomputed from the nodes. Because the rotation turns the shape around the center of its box and the center moves when the box changes, the path is shifted so that every node stays at the same place on the screen as if the old box had been kept.
- `public bool IsClosed { get; init; }` - Whether the last node connects back to the first one.
- `public FillRule FillRule { get; init; }` - How the inside is decided where the path crosses itself. The default is PressToShape.Core.Shapes.FillRule.NonZero.
- `public PathShape Create(IReadOnlyList<DocumentNode> nodes, bool isClosed = false, FillRule fillRule = NonZero)` - Creates a path from nodes in document coordinates, in the frame before any rotation. The box becomes the tight bounds of the curve (the end points and the extremes of the curved segments, not the handle points). A handle point that no segment uses, such as the in handle of the first node of an open path, does not count. If one side of the box has size 0, every node stores 0 on that side.
- `protected bool ContainsLocal(double x, double y)` - Whether the point (before any rotation) hits the path. The inside counts for a closed path and for an open path with a fill, which is filled as if it were closed. A path that is open or has a stroke is also hit near its outline: within half the stroke width, but at least a few document units. The closing line of an open path is not part of the outline.
- `public IReadOnlyList<DocumentNode> ToDocumentNodes()` - The nodes in document coordinates, in the frame before the rotation. The inverse of PressToShape.Core.Shapes.PathShape.Create for the current box.
- `public IReadOnlyList<Cubic> Segments()` - The segments in document coordinates, in the frame before the rotation. A line is a cubic with its handles on its end points.

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
- Injected into `PressToShape.App.Tests.CanvasInteractionContextMenuTests.Scene`
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
- `private protected (double Sin, double Cos) SinCos(double degrees)` - _(no documentation)_

**Relationships**

- Inherited by `PressToShape.Core.Shapes.EllipseShape`
- Inherited by `PressToShape.Core.Shapes.GroupShape`
- Inherited by `PressToShape.Core.Shapes.PathShape`
- Inherited by `PressToShape.Core.Shapes.RectangleShape`
- Injected into `PressToShape.App.Workspace.SelectionQueries.Node`
- Injected into `PressToShape.Core.Documents.ShapeLocation`
- Injected into `PressToShape.Core.Documents.ShapeTree.Found`
