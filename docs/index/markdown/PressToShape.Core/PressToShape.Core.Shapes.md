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

## Corner (enum)

`public enum Corner`

Source: `src/PressToShape.Core/Shapes/Corner.cs:4-17`

The four corners of a rectangle, clockwise from the top left.

**Members**

- `public TopLeft` - The corner at the left end of the top edge.
- `public TopRight` - The corner at the right end of the top edge.
- `public BottomRight` - The corner at the right end of the bottom edge.
- `public BottomLeft` - The corner at the left end of the bottom edge.

## CornerRadii (record)

`public readonly record CornerRadii : IEquatable<CornerRadii>`

Source: `src/PressToShape.Core/Shapes/CornerRadii.cs:13-60`

The radius of each corner of a rectangle, in document units. The struct only holds the values: PressToShape.Core.Shapes.RectangleShape checks that they are finite and not negative. PressToShape.Core.Shapes.CornerRadii.Limited gives the radii that are really used for a box of a given size.

**Members**

- `public CornerRadii(double TopLeft, double TopRight, double BottomRight, double BottomLeft)` - The radius of each corner of a rectangle, in document units. The struct only holds the values: PressToShape.Core.Shapes.RectangleShape checks that they are finite and not negative. PressToShape.Core.Shapes.CornerRadii.Limited gives the radii that are really used for a box of a given size.
- `public double BottomLeft { get; init; }` - Radius of the bottom left corner.
- `public double BottomRight { get; init; }` - Radius of the bottom right corner.
- `public double TopLeft { get; init; }` - Radius of the top left corner.
- `public double TopRight { get; init; }` - Radius of the top right corner.
- `public CornerRadii Zero { get; }` - All four radii are 0: a rectangle with square corners.
- `public CornerRadii Uniform(double radius)` - All four corners have the same radius.
- `public bool IsZero { get; }` - Whether all four radii are 0.
- `public bool IsUniform { get; }` - Whether all four radii are equal.
- `public double this[Corner corner] { get; }` - The radius of one corner.
- `public CornerRadii With(Corner corner, double radius)` - A copy with the radius of one corner replaced.
- `public CornerRadii Limited(double width, double height)` - Each radius limited to at least 0 and at most half of the shorter side of a box of this size. A radius that is not a number becomes 0.

**Relationships**

- Injected into `PressToShape.App.Tests.PropertiesViewModelCornersTests.Fixture`

## DocumentNode (record)

`public readonly record DocumentNode : IEquatable<DocumentNode>`

Source: `src/PressToShape.Core/Shapes/DocumentNode.cs:7-55`

A point of a path in document units, in the frame of the path before its rotation. Used to build and edit a PressToShape.Core.Shapes.PathShape, which stores its nodes as fractions of its box (PressToShape.Core.Shapes.PathNode).

**Members**

- `public DocumentNode(double x, double y, double? inX = null, double? inY = null, double? outX = null, double? outY = null, NodeType type = Corner)` - Creates a node. Every number is finite, and a handle point is given with both numbers or with none.
- `public double X { get; }` - Anchor x in document units.
- `public double Y { get; }` - Anchor y in document units.
- `public double? InX { get; }` - In handle point x, or null together with PressToShape.Core.Shapes.DocumentNode.InY when there is no in handle.
- `public double? InY { get; }` - In handle point y.
- `public double? OutX { get; }` - Out handle point x, or null together with PressToShape.Core.Shapes.DocumentNode.OutY when there is no out handle.
- `public double? OutY { get; }` - Out handle point y.
- `public NodeType Type { get; }` - Whether the node has an in handle.
- `public bool HasIn { get; }` - _(no documentation)_
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

## NodeHandleSide (enum)

`public enum NodeHandleSide`

Source: `src/PressToShape.Core/Shapes/NodeHandleSide.cs:4-11`

Which of the two handles of a node: the one toward the previous node or the one toward the next node.

**Members**

- `public In` - The handle on the side of the previous node.
- `public Out` - The handle on the side of the next node.

## NodeType (enum)

`public enum NodeType`

Source: `src/PressToShape.Core/Shapes/NodeType.cs:7-17`

How the two handles of a node relate when one of them is edited. The type is a rule for editing: a model value that does not match its type (for example after a non-uniform resize) is not an error, the next handle edit enforces the type again.

**Members**

- `public Corner` - The handles move independently, and the node may have none, one or two handles.
- `public Smooth` - The two handles lie on one straight line through the anchor on opposite sides, each with its own length.
- `public Symmetric` - The two handles lie on one straight line through the anchor on opposite sides and have the same length.

## NodeTypes (class)

`public static class NodeTypes`

Source: `src/PressToShape.Core/Shapes/NodeTypes.cs:9-95`

The rules that connect a PressToShape.Core.Shapes.NodeType with the handles of a node. A node counts as having its handles on one line when the sine of the angle between them is below 1e-6, and as equally long when the lengths differ by less than 1e-6 of the longer one.

**Members**

- `public NodeType Derive(PathNode node)` - The type that matches the handles of the node, whatever type it stores: fewer than two handles give a corner, two handles on one line on opposite sides give symmetric if they are equally long and smooth otherwise, every other case gives a corner.
- `public NodeType Derive(DocumentNode node)` - The type that matches the handles of the node, whatever type it stores: fewer than two handles give a corner, two handles on one line on opposite sides give symmetric if they are equally long and smooth otherwise, every other case gives a corner.
- `public bool Satisfies(PathNode node)` - Whether the handles of the node match the type it stores. A corner always does, so does a node with at most one handle. A smooth node needs its handles on one line on opposite sides, a symmetric node also needs them equally long.
- `public bool Satisfies(DocumentNode node)` - Whether the handles of the node match the type it stores. A corner always does, so does a node with at most one handle. A smooth node needs its handles on one line on opposite sides, a symmetric node also needs them equally long.

## NodeValidation (class)

`internal static class NodeValidation`

Source: `src/PressToShape.Core/Shapes/NodeValidation.cs:6-34`

The checks shared by PressToShape.Core.Shapes.PathNode and PressToShape.Core.Shapes.DocumentNode, so both refuse the same values.

**Members**

- `public void Check(double x, double y, double? inX, double? inY, double? outX, double? outY, NodeType type = Corner)` - Checks the anchor, the two optional handle points (each given with both numbers or with none) and the node type.

## PathEditing (class)

`public static class PathEditing`

Source: `src/PressToShape.Core/Shapes/PathEditing.cs:8-118`

Operations that change the nodes of a PressToShape.Core.Shapes.PathShape: add a point, delete a point, continue an open path. Every result is a new path, the input is never changed.

**Members**

- `public PathShape InsertNode(PathShape path, int segmentIndex, double t)` - Adds a node on a segment without changing the curve. A segment is the stretch from node segmentIndex to the next node (the first node again for the last segment of a closed path).
- `public PathShape? RemoveNode(PathShape path, int index)` - Removes a node. The neighbors connect, each keeping its own handle, so the curve changes a little. Returns null if fewer than two nodes would be left, in which case the caller should delete the shape.
- `public IReadOnlyList<DocumentNode> Continue(PathShape path, bool atStart)` - The nodes of an open path in document coordinates, ordered so that the end to continue from is the last one: as they are for the end of the path, reversed for its start. Reversing swaps the in and out handle of every node.

## PathNode (record)

`public readonly record PathNode : IEquatable<PathNode>`

Source: `src/PressToShape.Core/Shapes/PathNode.cs:9-57`

A point of a PressToShape.Core.Shapes.PathShape as it is stored: the anchor and the optional handle points are fractions of the path's box, so 0 is the left or top edge and 1 the right or bottom edge. Handle points may lie outside 0..1. Resizing the box scales the path without touching the nodes. A segment is a curve if the node it starts at has an out handle or the node it ends at has an in handle, otherwise it is a line.

**Members**

- `public PathNode(double x, double y, double? inX = null, double? inY = null, double? outX = null, double? outY = null, NodeType type = Corner)` - Creates a node. Every number is finite, and a handle point is given with both numbers or with none.
- `public double X { get; }` - Anchor, fraction of the box width.
- `public double Y { get; }` - Anchor, fraction of the box height.
- `public double? InX { get; }` - In handle point, fraction of the box width. Null together with PressToShape.Core.Shapes.PathNode.InY when there is no in handle.
- `public double? InY { get; }` - In handle point, fraction of the box height.
- `public double? OutX { get; }` - Out handle point, fraction of the box width. Null together with PressToShape.Core.Shapes.PathNode.OutY when there is no out handle.
- `public double? OutY { get; }` - Out handle point, fraction of the box height.
- `public NodeType Type { get; }` - Whether the node has an in handle.
- `public bool HasIn { get; }` - _(no documentation)_
- `public bool HasOut { get; }` - Whether the node has an out handle.

## PathNodeEditing (class)

`public partial static class PathNodeEditing`

Source: `src/PressToShape.Core/Shapes/PathNodeEditing.Segments.cs:6-121`

The rules for editing the nodes and handles of a PressToShape.Core.Shapes.PathShape. Every method takes the path and positions in the frame of the path before its rotation (see PressToShape.Core.Shapes.PathShape.ToLocal), changes nothing in place and returns a new path built with PressToShape.Core.Shapes.PathShape.Rebuilt, so every node that was not moved keeps its place on the screen, also for a rotated path.

**Members**

- `public PathShape SetNodeType(PathShape path, IReadOnlyCollection<int> indices, NodeType type)` - Gives the nodes the type. A corner only changes the type, the handles stay. A smooth or symmetric node first needs its handles on one line on opposite sides: if it has an out handle, the direction of that handle wins and the in handle turns to the opposite side (with its own length for smooth, with the length of the out handle for symmetric), and if it only has an in handle the out handle follows it in the same way. A node without handles gets handles on the line from its previous to its next neighbor, a third of the distance to the neighbor on each side (for symmetric both as long as the shorter one). An end node of an open path only gets the handle toward its one neighbor. No handle is created toward a neighbor on the same spot.
- `public PathShape MoveNodes(PathShape path, IReadOnlyCollection<int> indices, double dx, double dy)` - Moves the nodes by the distance. Their handles move with them, so their types stay valid.
- `public PathShape BendSegment(PathShape path, int segmentIndex, double t, double dx, double dy)` - Bends a segment so that its point at t moves by the distance, by changing only the two handle points of the segment. A straight segment becomes a curve this way. Afterwards a node whose handles no longer match its type becomes a corner. For t from 0.15 to 0.85 the point moves exactly by the distance, nearer to an end only approximately.
- `public PathShape MoveHandle(PathShape path, int index, NodeHandleSide side, double x, double y, bool breakLink)` - Moves one handle to the position. The other handle follows the type of the node: a corner leaves it alone, a smooth node keeps it on the line through the anchor on the opposite side with its own length, a symmetric node mirrors it. With breakLink only this handle moves and the node becomes a corner. A handle moved onto its anchor is removed and the node becomes a corner.
- `public PathShape StraightenSegment(PathShape path, int segmentIndex)` - Makes a segment straight: the handles that belong to it are removed. A node that lost a handle becomes a corner. A segment that is already straight leaves the path as it is.
- `public PathShape CurveSegment(PathShape path, int segmentIndex)` - Makes a straight segment a curve with handles one third of its length from each end, on the line itself, so the picture does not change. A segment that is already curved (it has a handle) leaves the path as it is, and so does a segment of no length. A node whose handles no longer match its type becomes a corner.
- `public PathShape? RemoveNodes(PathShape path, IReadOnlyCollection<int> indices)` - Removes the nodes. The neighbors connect, each keeping its own handle, so the curve changes a little. Returns null if fewer than two nodes would remain, in which case the caller deletes the shape.
- `public (double X, double Y) NodePosition(PathShape path, int index)` - The position of the anchor of a node on the screen, with the rotation of the path applied.

## PathShape (record)

`public partial sealed record PathShape : Shape, IEquatable<PathShape>`

Source: `src/PressToShape.Core/Shapes/PathShape.Geometry.cs:7-160`

A path of line and cubic bezier segments, open or closed. The box (PressToShape.Core.Shapes.Shape.X, PressToShape.Core.Shapes.Shape.Y, PressToShape.Core.Shapes.Shape.Width, PressToShape.Core.Shapes.Shape.Height) is the tight bounds of the curve, and the nodes are stored as fractions of that box (see PressToShape.Core.Shapes.PathNode), so changing the box with with moves, resizes and (with the rotation) turns the whole path, like any other shape. Build one from document coordinates with PressToShape.Core.Shapes.PathShape.Create. Immutable: change it with with.

**Members**

- `public ImmutableArray<PathNode> Nodes { get; init; }` - The nodes in drawing order, at least two. The segment from the last node back to the first exists only if PressToShape.Core.Shapes.PathShape.IsClosed.
- `public PathShape Rebuilt(IReadOnlyList<DocumentNode> nodes, bool isClosed, FillRule fillRule)` - A path with the given nodes (document coordinates, in the frame of this path before the rotation) that keeps the id, the appearance, the name, the flags and the rotation. The box is recomputed from the nodes. Because the rotation turns the shape around the center of its box and the center moves when the box changes, the path is shifted so that every node stays at the same place on the screen as if the old box had been kept.
- `public bool IsClosed { get; init; }` - Whether the last node connects back to the first one.
- `public FillRule FillRule { get; init; }` - How the inside is decided where the path crosses itself. The default is PressToShape.Core.Shapes.FillRule.NonZero.
- `public PathShape Create(IReadOnlyList<DocumentNode> nodes, bool isClosed = false, FillRule fillRule = NonZero)` - Creates a path from nodes in document coordinates, in the frame before any rotation. The box becomes the tight bounds of the curve (the end points and the extremes of the curved segments, not the handle points). A handle point that no segment uses, such as the in handle of the first node of an open path, does not count. If one side of the box has size 0, every node stores 0 on that side.
- `public (double X, double Y) ToLocal(double x, double y)` - Turns a position on the screen into the frame of the path before its rotation (the frame PressToShape.Core.Shapes.PathShape.Create and PressToShape.Core.Shapes.PathShape.ToDocumentNodes use): the inverse of the turn around the center of the box.
- `public (double X, double Y) FromLocal(double x, double y)` - Turns a position in the frame of the path before its rotation into the position on the screen: the turn around the center of the box.
- `public (double Dx, double Dy) ToLocalDelta(double dx, double dy)` - Turns a distance on the screen into a distance in the frame of the path before its rotation.
- `protected bool ContainsLocal(double x, double y)` - Whether the point (before any rotation) hits the path. The inside counts for a closed path and for an open path with a fill, which is filled as if it were closed. A path that is open or has a stroke is also hit near its outline: within half the stroke width, but at least a few document units. The closing line of an open path is not part of the outline.
- `public IReadOnlyList<DocumentNode> ToDocumentNodes()` - The nodes in document coordinates, in the frame before the rotation. The inverse of PressToShape.Core.Shapes.PathShape.Create for the current box.
- `public IReadOnlyList<Cubic> Segments()` - The segments in document coordinates, in the frame before the rotation. A line is a cubic with its handles on its end points.

**Relationships**

- Inherits `PressToShape.Core.Shapes.Shape`
- Injected into `PressToShape.App.Tests.CanvasAreaViewModelNodeTests.Fixture`
- Injected into `PressToShape.App.Workspace.NodeTarget`

## RectangleShape (record)

`public sealed record RectangleShape : Shape, IEquatable<RectangleShape>`

Source: `src/PressToShape.Core/Shapes/RectangleShape.cs:4-64`

An axis-aligned rectangle, with square or rounded corners.

**Members**

- `public CornerRadii Radii { get; init; }` - The corner radii as they were set: each one finite and not negative, 0 for square corners (the default). They are not trimmed when the rectangle is resized, so a corner that was limited gets its radius back when the rectangle grows. Drawing, hit tests, export and the editing fields use PressToShape.Core.Shapes.RectangleShape.EffectiveRadii.
- `public CornerRadii EffectiveRadii { get; }` - The radii that are used: each stored radius limited to half of the shorter side of the box.
- `protected bool ContainsLocal(double x, double y)` - Whether the point lies inside the shape's geometry before any rotation is applied.

**Relationships**

- Inherits `PressToShape.Core.Shapes.Shape`
- Injected into `PressToShape.App.Tests.CanvasInteractionContextMenuTests.Scene`
- Injected into `PressToShape.App.Tests.CanvasInteractionGroupTests.Scene`

## RoundedCorner (record)

`internal readonly record RoundedCorner : IEquatable<RoundedCorner>`

Source: `src/PressToShape.Core/Shapes/RoundedRectangleGeometry.cs:13-13`

One corner of a rounded rectangle: its radius, the corner point of the box, and where the arc starts and ends when the outline is followed clockwise. For a radius of 0 the arc is empty and PressToShape.Core.Shapes.RoundedCorner.Start and PressToShape.Core.Shapes.RoundedCorner.End are the corner point.

**Members**

- `public RoundedCorner(double Radius, (double X, double Y) Point, (double X, double Y) Start, (double X, double Y) End)` - One corner of a rounded rectangle: its radius, the corner point of the box, and where the arc starts and ends when the outline is followed clockwise. For a radius of 0 the arc is empty and PressToShape.Core.Shapes.RoundedCorner.Start and PressToShape.Core.Shapes.RoundedCorner.End are the corner point.
- `public (double X, double Y) End { get; init; }` - Where the arc ends, going on along the next edge.
- `public (double X, double Y) Point { get; init; }` - The corner point of the box.
- `public double Radius { get; init; }` - The radius used, at least 0.
- `public (double X, double Y) Start { get; init; }` - Where the arc starts, coming along the previous edge.

## RoundedRectangleGeometry (class)

`internal static class RoundedRectangleGeometry`

Source: `src/PressToShape.Core/Shapes/RoundedRectangleGeometry.cs:19-34`

The outline of a rectangle with rounded corners, in the frame before any rotation. The conversion to a path and the SVG export both start from these points, so the two outlines are the same shape.

**Members**

- `public IReadOnlyList<RoundedCorner> Corners(double x, double y, double width, double height, CornerRadii effective)` - The four corners clockwise from the top left. Pass the effective radii (see PressToShape.Core.Shapes.CornerRadii.Limited).

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

## ShapeConversion (class)

`public static class ShapeConversion`

Source: `src/PressToShape.Core/Shapes/ShapeConversion.cs:6-87`

Turns shapes into paths without changing how they look, so they can be edited node by node.

**Members**

- `public PathShape? ToPath(Shape shape)` - A path that looks like the shape and keeps its id, name, flags, box, rotation and appearance. A rectangle becomes four corner nodes (closed), plus one more node for each rounded corner, which is two nodes joined by a curve. An ellipse becomes four symmetric nodes at the top, right, bottom and left with handles of 0.2761 of the width or the height, the usual circle approximation. Returns null for a path, a group and every other type, which need no conversion.
