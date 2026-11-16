using System;
using System.Collections.Generic;

namespace PressToShape.App.Workspace;

/// <summary>Turns a <see cref="ScreenPath"/> into drawing steps, so the canvas control only replays them and the rules are tested without a UI.</summary>
internal static class PathOutline
{
    /// <summary>
    /// The steps that draw the path, moved by the origin (the top left corner of the shape's box on the screen). A segment without
    /// handles is a line. A closed path ends with the segment from the last node back to the first (a line or a curve) and a
    /// <see cref="PathCommandType.Close"/>. An open path has no close step.
    /// </summary>
    public static IReadOnlyList<PathCommand> Commands(ScreenPath path, double originX, double originY)
    {
        ArgumentNullException.ThrowIfNull(path);

        var nodes = path.Nodes;
        var commands = new List<PathCommand>(nodes.Length + 2)
        {
            new(PathCommandType.Move, originX + nodes[0].X, originY + nodes[0].Y),
        };

        var segmentCount = path.IsClosed ? nodes.Length : nodes.Length - 1;
        for (var i = 0; i < segmentCount; i++)
        {
            var from = nodes[i];
            var to = nodes[(i + 1) % nodes.Length];
            if (!from.HasOut && !to.HasIn)
            {
                // A closed path draws its last line when it closes, so the line is only written for the other segments.
                if (!(path.IsClosed && i == nodes.Length - 1))
                {
                    commands.Add(new PathCommand(PathCommandType.Line, originX + to.X, originY + to.Y));
                }

                continue;
            }

            commands.Add(new PathCommand(
                PathCommandType.Cubic,
                originX + to.X,
                originY + to.Y,
                originX + (from.OutX ?? from.X),
                originY + (from.OutY ?? from.Y),
                originX + (to.InX ?? to.X),
                originY + (to.InY ?? to.Y)));
        }

        if (path.IsClosed)
        {
            commands.Add(new PathCommand(PathCommandType.Close));
        }

        return commands;
    }
}
