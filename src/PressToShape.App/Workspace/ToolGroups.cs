using System.Collections.Generic;
using System.Linq;

namespace PressToShape.App.Workspace;

/// <summary>The icons of the tablet tool column, in order. Text and Eyedropper are groups of one tool.</summary>
internal static class ToolGroups
{
    public static IReadOnlyList<ToolGroupDefinition> All { get; } =
    [
        new([Tool.Select, Tool.DirectSelect]),
        new([Tool.Pen, Tool.Node]),
        new([Tool.Text]),
        new([Tool.Rectangle, Tool.Ellipse]),
        new([Tool.Eyedropper]),
        new([Tool.Hand, Tool.Zoom]),
    ];

    /// <summary>The group a tool belongs to.</summary>
    public static ToolGroupDefinition Of(Tool tool) => All.First(g => g.Tools.Contains(tool));
}
