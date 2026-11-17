using System.Collections.Immutable;

namespace PressToShape.App.Workspace;

/// <summary>One sub-path of a <see cref="ScreenPath"/>, with its nodes already in screen pixels.</summary>
/// <param name="Nodes">The nodes in drawing order, at least two.</param>
/// <param name="IsClosed">Whether the last node connects back to the first one.</param>
internal sealed record ScreenSubPath(ImmutableArray<ScreenPathNode> Nodes, bool IsClosed);
