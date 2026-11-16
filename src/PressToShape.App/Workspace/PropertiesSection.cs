using System;

namespace PressToShape.App.Workspace;

/// <summary>The sections of the Properties tab. Combined as flags to say which ones a tool shows.</summary>
[Flags]
internal enum PropertiesSection
{
    None = 0,
    Transform = 1,
    Appearance = 2,
    Path = 4,
    Align = 8,
    QuickActions = 16,

    /// <summary>The nodes of the selected paths: position of a node and, for the Node tool, the type and segment buttons.</summary>
    Node = 32,

    /// <summary>The corner radii of the one selected rectangle. Shown by the tools that show the radius handles.</summary>
    Corners = 64,
}
