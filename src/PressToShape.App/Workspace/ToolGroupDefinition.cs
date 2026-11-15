using System.Collections.Generic;

namespace PressToShape.App.Workspace;

/// <summary>Tools that share one icon in the tablet tool column. The first tool is the one the group starts with.</summary>
internal sealed record ToolGroupDefinition(IReadOnlyList<Tool> Tools)
{
    /// <summary>The tool the group has before any other one was chosen.</summary>
    public Tool DefaultTool => Tools[0];

    /// <summary>Whether the group has more than one tool, so it needs a mark and a flyout.</summary>
    public bool IsGroup => Tools.Count > 1;
}
