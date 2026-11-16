using System.Collections.Immutable;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// The outline of a path ready to draw: every node is already in screen pixels, relative to the top left corner of the
/// <see cref="ScreenShape"/> that holds it. A segment is a curve if the node it starts at has an out handle or the node it ends at
/// has an in handle, otherwise a line.
/// </summary>
/// <param name="Nodes">The nodes in drawing order, at least two.</param>
/// <param name="IsClosed">Whether the last node connects back to the first one.</param>
/// <param name="FillRule">How the inside is decided where the path crosses itself.</param>
internal sealed record ScreenPath(ImmutableArray<ScreenPathNode> Nodes, bool IsClosed, FillRule FillRule);
