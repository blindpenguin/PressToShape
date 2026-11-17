using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.IO;

public static partial class ProjectFile
{
    /// <summary>The most nodes one path in a file may have, over all its sub-paths together.</summary>
    public const int MaxPathNodes = 100_000;

    private const string EvenOddText = "evenodd";

    private static PathShape? BuildPath(ShapeDto dto, Guid id, Appearance appearance, string name, bool visible, bool locked, out ProjectFileResult? failure)
    {
        failure = null;
        if (dto.SubPaths is not null && (dto.Nodes is not null || dto.Closed is not null))
        {
            failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, "A path has both nodes and sub-paths.");
            return null;
        }

        List<SubPathDto?> sources;
        if (dto.SubPaths is not null)
        {
            if (dto.SubPaths.Count < 2 || dto.SubPaths.Count > PathShape.MaxSubPaths)
            {
                failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, $"A compound path needs from 2 to {PathShape.MaxSubPaths} sub-paths, this one has {dto.SubPaths.Count}.");
                return null;
            }

            sources = dto.SubPaths;
        }
        else
        {
            if (dto.Nodes is null)
            {
                failure = ProjectFileResult.Failure(ProjectFileError.NotProjectFile, "A path misses its nodes.");
                return null;
            }

            sources = [new SubPathDto { Nodes = dto.Nodes, Closed = dto.Closed }];
        }

        if (dto.FillRule is not null && dto.FillRule != EvenOddText)
        {
            failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, "A path has an unknown fill rule: " + dto.FillRule);
            return null;
        }

        var total = 0;
        var subPaths = ImmutableArray.CreateBuilder<SubPath>(sources.Count);
        foreach (var source in sources)
        {
            if (source?.Nodes is null)
            {
                failure = ProjectFileResult.Failure(ProjectFileError.NotProjectFile, "A path misses its nodes.");
                return null;
            }

            total += source.Nodes.Count;
            if (source.Nodes.Count < 2 || total > MaxPathNodes)
            {
                failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, $"A sub-path needs at least 2 nodes and a path at most {MaxPathNodes} nodes in all.");
                return null;
            }

            var nodes = ImmutableArray.CreateBuilder<PathNode>(source.Nodes.Count);
            foreach (var nodeDto in source.Nodes)
            {
                if (nodeDto is not { X: { } x, Y: { } y })
                {
                    failure = ProjectFileResult.Failure(ProjectFileError.NotProjectFile, "A path node misses an entry.");
                    return null;
                }

                // A half handle or a number that is not finite makes the node constructor throw, which Read turns into InvalidValue.
                var plain = new PathNode(x, y, nodeDto.InX, nodeDto.InY, nodeDto.OutX, nodeDto.OutY);
                if (!TryReadNodeType(nodeDto.Type, plain, out var type))
                {
                    failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, "A path node has an unknown type: " + nodeDto.Type);
                    return null;
                }

                nodes.Add(new PathNode(x, y, nodeDto.InX, nodeDto.InY, nodeDto.OutX, nodeDto.OutY, type));
            }

            subPaths.Add(new SubPath(nodes.MoveToImmutable(), source.Closed ?? false));
        }

        return new PathShape
        {
            Id = id,
            X = dto.X!.Value,
            Y = dto.Y!.Value,
            Width = dto.Width!.Value,
            Height = dto.Height!.Value,
            SubPaths = subPaths.MoveToImmutable(),
            FillRule = dto.FillRule is null ? FillRule.NonZero : FillRule.EvenOdd,
            Appearance = appearance,
            Rotation = dto.Rotation ?? 0,
            Name = name,
            IsVisible = visible,
            IsLocked = locked,
        };
    }

    // One sub-path is written as "nodes" and "closed" like a path always was, so such files stay the same. Only a compound path writes "subPaths".
    private static void AddPathData(ShapeDto dto, PathShape path)
    {
        if (path.SubPaths.Length == 1)
        {
            dto.Nodes = NodeDtos(path.SubPaths[0]);
            dto.Closed = path.SubPaths[0].IsClosed ? true : null;
        }
        else
        {
            dto.SubPaths = new List<SubPathDto?>(path.SubPaths.Select(subPath => (SubPathDto?)new SubPathDto
            {
                Nodes = NodeDtos(subPath),
                Closed = subPath.IsClosed ? true : null,
            }));
        }

        dto.FillRule = path.FillRule == FillRule.EvenOdd ? EvenOddText : null;
    }

    private static List<NodeDto?> NodeDtos(SubPath subPath) =>
        new(subPath.Nodes.Select(node => (NodeDto?)new NodeDto
        {
            X = node.X,
            Y = node.Y,
            InX = node.InX,
            InY = node.InY,
            OutX = node.OutX,
            OutY = node.OutY,
            Type = node.Type == NodeTypes.Derive(node) ? null : NodeTypeText(node.Type),
        }));

    // A missing type is the one the handles give (files from before the node types have none). Any other text must be a known type.
    private static bool TryReadNodeType(string? text, PathNode plain, out NodeType type)
    {
        type = NodeTypes.Derive(plain);
        switch (text)
        {
            case null:
                return true;
            case "corner":
                type = NodeType.Corner;
                return true;
            case "smooth":
                type = NodeType.Smooth;
                return true;
            case "symmetric":
                type = NodeType.Symmetric;
                return true;
            default:
                return false;
        }
    }

    private static string NodeTypeText(NodeType type) => type switch
    {
        NodeType.Smooth => "smooth",
        NodeType.Symmetric => "symmetric",
        _ => "corner",
    };
}
