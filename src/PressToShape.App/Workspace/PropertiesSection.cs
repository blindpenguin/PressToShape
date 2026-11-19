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

    /// <summary>The boolean operations and Combine as icon buttons. Shown by the tools that select several objects, and only while two or more are selected.</summary>
    PathOperations = 128,

    /// <summary>The font, size, style, alignment and spacing of texts. Shown by the Text tool, and by the selection tools while a text is selected.</summary>
    Text = 256,
}
