using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.IO;

public static partial class ProjectFile
{
    /// <summary>The most nodes one path in a file may have.</summary>
    public const int MaxPathNodes = 100_000;

    private const string EvenOddText = "evenodd";

    private static PathShape? BuildPath(ShapeDto dto, Guid id, Appearance appearance, string name, bool visible, bool locked, out ProjectFileResult? failure)
    {
        failure = null;
        if (dto.Nodes is null)
        {
            failure = ProjectFileResult.Failure(ProjectFileError.NotProjectFile, "A path misses its nodes.");
            return null;
        }

        if (dto.Nodes.Count < 2 || dto.Nodes.Count > MaxPathNodes)
        {
            failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, $"A path needs from 2 to {MaxPathNodes} nodes, this one has {dto.Nodes.Count}.");
            return null;
        }

        if (dto.FillRule is not null && dto.FillRule != EvenOddText)
        {
            failure = ProjectFileResult.Failure(ProjectFileError.InvalidValue, "A path has an unknown fill rule: " + dto.FillRule);
            return null;
        }

        var nodes = ImmutableArray.CreateBuilder<PathNode>(dto.Nodes.Count);
        foreach (var nodeDto in dto.Nodes)
        {
            if (nodeDto is not { X: { } x, Y: { } y })
            {
                failure = ProjectFileResult.Failure(ProjectFileError.NotProjectFile, "A path node misses an entry.");
                return null;
            }

            // A half handle or a number that is not finite makes the node constructor throw, which Read turns into InvalidValue.
            nodes.Add(new PathNode(x, y, nodeDto.InX, nodeDto.InY, nodeDto.OutX, nodeDto.OutY));
        }

        return new PathShape
        {
            Id = id,
            X = dto.X!.Value,
            Y = dto.Y!.Value,
            Width = dto.Width!.Value,
            Height = dto.Height!.Value,
            Nodes = nodes.MoveToImmutable(),
            IsClosed = dto.Closed ?? false,
            FillRule = dto.FillRule is null ? FillRule.NonZero : FillRule.EvenOdd,
            Appearance = appearance,
            Rotation = dto.Rotation ?? 0,
            Name = name,
            IsVisible = visible,
            IsLocked = locked,
        };
    }

    private static void AddPathData(ShapeDto dto, PathShape path)
    {
        dto.Nodes = new List<NodeDto?>(path.Nodes.Select(node => new NodeDto
        {
            X = node.X,
            Y = node.Y,
            InX = node.InX,
            InY = node.InY,
            OutX = node.OutX,
            OutY = node.OutY,
        }));
        dto.Closed = path.IsClosed ? true : null;
        dto.FillRule = path.FillRule == FillRule.EvenOdd ? EvenOddText : null;
    }
}
