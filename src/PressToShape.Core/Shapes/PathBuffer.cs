using System;
using System.Collections.Generic;
using System.Linq;

namespace PressToShape.Core.Shapes;

/// <summary>
/// The nodes of a <see cref="PathShape"/> in document coordinates as lists that an edit can change, one list per sub-path. A node is
/// addressed by its flat index (see <see cref="PathShape.Locate"/>). <see cref="Build"/> makes the edited path.
/// </summary>
internal sealed class PathBuffer
{
    private readonly PathShape _path;
    private readonly List<List<DocumentNode>> _nodes;
    private readonly List<bool> _closed;

    private PathBuffer(PathShape path)
    {
        _path = path;
        var subPaths = path.ToDocumentSubPaths();
        _nodes = subPaths.Select(subPath => new List<DocumentNode>(subPath.Nodes)).ToList();
        _closed = subPaths.Select(subPath => subPath.IsClosed).ToList();
    }

    /// <summary>The number of sub-paths.</summary>
    public int SubPathCount => _nodes.Count;

    /// <summary>A buffer with the nodes of the path.</summary>
    public static PathBuffer From(PathShape path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return new PathBuffer(path);
    }

    /// <summary>The nodes of a sub-path. The list can be changed.</summary>
    public List<DocumentNode> Nodes(int subPath) => _nodes[subPath];

    /// <summary>Whether a sub-path is closed.</summary>
    public bool IsClosed(int subPath) => _closed[subPath];

    /// <summary>The node with the flat index.</summary>
    public DocumentNode this[int flatIndex]
    {
        get
        {
            var (subPath, node) = _path.Locate(flatIndex);
            return _nodes[subPath][node];
        }

        set
        {
            var (subPath, node) = _path.Locate(flatIndex);
            _nodes[subPath][node] = value;
        }
    }

    /// <summary>
    /// The edited path, built from the current nodes with <see cref="PathShape.Rebuilt(IReadOnlyList{DocumentSubPath}, FillRule)"/>. A sub-path with
    /// fewer than two nodes is dropped. Returns <c>null</c> if no sub-path is left.
    /// </summary>
    public PathShape? Build()
    {
        var subPaths = new List<DocumentSubPath>(_nodes.Count);
        for (var i = 0; i < _nodes.Count; i++)
        {
            if (_nodes[i].Count >= 2)
            {
                subPaths.Add(new DocumentSubPath(_nodes[i], _closed[i]));
            }
        }

        return subPaths.Count == 0 ? null : _path.Rebuilt(subPaths, _path.FillRule);
    }
}
