using System;
using System.Collections.Generic;

namespace PressToShape.Core.Shapes;

/// <summary>
/// One run of connected nodes in document units, in the frame of the path before its rotation. Used to build and edit a
/// <see cref="PathShape"/>, which stores its sub-paths as fractions of its box (<see cref="SubPath"/>). Immutable by convention: the list is not changed.
/// </summary>
public sealed record DocumentSubPath
{
    /// <summary>Creates a sub-path.</summary>
    /// <param name="nodes">The nodes in drawing order, at least two.</param>
    /// <param name="isClosed">Whether the last node connects back to the first one.</param>
    /// <exception cref="ArgumentException">There are fewer than two nodes.</exception>
    public DocumentSubPath(IReadOnlyList<DocumentNode> nodes, bool isClosed = false)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        if (nodes.Count < 2)
        {
            throw new ArgumentException("A path needs at least two nodes.", nameof(nodes));
        }

        Nodes = nodes;
        IsClosed = isClosed;
    }

    /// <summary>The nodes in drawing order, at least two.</summary>
    public IReadOnlyList<DocumentNode> Nodes { get; }

    /// <summary>Whether the last node connects back to the first one.</summary>
    public bool IsClosed { get; }
}
