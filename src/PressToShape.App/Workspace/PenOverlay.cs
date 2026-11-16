using System.Collections.Generic;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>What the canvas shows on top of an unfinished path: its nodes with their handles, in document coordinates.</summary>
/// <param name="Nodes">The nodes drawn so far, in order.</param>
/// <param name="HighlightFirst">Whether the first node is highlighted because a click would close the path.</param>
internal sealed record PenOverlay(IReadOnlyList<DocumentNode> Nodes, bool HighlightFirst);
