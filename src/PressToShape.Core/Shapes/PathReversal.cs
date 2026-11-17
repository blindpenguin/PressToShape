using System;
using System.Collections.Generic;

namespace PressToShape.Core.Shapes;

/// <summary>Reverses the direction of a <see cref="PathShape"/> without changing how it looks.</summary>
public static class PathReversal
{
    /// <summary>
    /// The path with every sub-path running the other way. An open sub-path gets its node order reversed. A closed sub-path keeps its first node
    /// first and the other nodes run backwards, so the start point does not move. In both cases the in and out handle of every node are swapped.
    /// Each sub-path stays at its place in the list. The shape is the same, every node stays at the same place on the screen (also for a rotated
    /// path), and id, appearance, name, flags, rotation and fill rule are kept. Reversing twice gives the same nodes. The input is not changed.
    /// </summary>
    public static PathShape Reverse(PathShape path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var reversed = new List<DocumentSubPath>(path.SubPaths.Length);
        foreach (var subPath in path.ToDocumentSubPaths())
        {
            reversed.Add(new DocumentSubPath(Reversed(subPath), subPath.IsClosed));
        }

        return path.Rebuilt(reversed, path.FillRule);
    }

    private static List<DocumentNode> Reversed(DocumentSubPath subPath)
    {
        var nodes = subPath.Nodes;
        var result = new List<DocumentNode>(nodes.Count);
        if (subPath.IsClosed)
        {
            result.Add(Swapped(nodes[0]));
            for (var i = nodes.Count - 1; i >= 1; i--)
            {
                result.Add(Swapped(nodes[i]));
            }
        }
        else
        {
            for (var i = nodes.Count - 1; i >= 0; i--)
            {
                result.Add(Swapped(nodes[i]));
            }
        }

        return result;
    }

    private static DocumentNode Swapped(DocumentNode node) =>
        new(node.X, node.Y, node.OutX, node.OutY, node.InX, node.InY, node.Type);
}
