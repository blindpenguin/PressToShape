using System;
using System.Collections.Immutable;

namespace PressToShape.Core.Shapes;

/// <summary>
/// One run of connected nodes of a <see cref="PathShape"/>, open or closed. The nodes are stored the way the path stores them, as fractions of
/// the box of the whole path (see <see cref="PathNode"/>). Immutable.
/// </summary>
public sealed record SubPath
{
    /// <summary>Creates a sub-path.</summary>
    /// <param name="nodes">The nodes in drawing order, at least two.</param>
    /// <param name="isClosed">Whether the last node connects back to the first one.</param>
    /// <exception cref="ArgumentException">There are fewer than two nodes.</exception>
    public SubPath(ImmutableArray<PathNode> nodes, bool isClosed = false)
    {
        if (nodes.IsDefaultOrEmpty || nodes.Length < 2)
        {
            throw new ArgumentException("A sub-path needs at least two nodes.", nameof(nodes));
        }

        Nodes = nodes;
        IsClosed = isClosed;
    }

    /// <summary>The nodes in drawing order, at least two.</summary>
    public ImmutableArray<PathNode> Nodes { get; }

    /// <summary>Whether the last node connects back to the first one.</summary>
    public bool IsClosed { get; }

    /// <summary>The number of segments: the nodes minus 1 when open, the nodes when closed.</summary>
    public int SegmentCount => IsClosed ? Nodes.Length : Nodes.Length - 1;
}
