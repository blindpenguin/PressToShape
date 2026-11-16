using System.Collections.Generic;

namespace PressToShape.App.Workspace;

/// <summary>What the canvas shows for the node tools, in screen pixels of the canvas area: every node of the selected shapes, with the handles that are shown.</summary>
/// <param name="Nodes">The nodes of all editable selected shapes, shape by shape in drawing order and in node order.</param>
internal sealed record ScreenNodeOverlay(IReadOnlyList<ScreenOverlayNode> Nodes);
