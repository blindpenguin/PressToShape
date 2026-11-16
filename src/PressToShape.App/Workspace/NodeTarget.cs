using System.Collections.Generic;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>A path with selected nodes: the editable path (a rectangle or ellipse already converted, not stored) and the indices of its selected nodes.</summary>
/// <param name="Path">The path before its rotation is applied, in the frame of <see cref="PathNodeEditing"/>.</param>
/// <param name="Indices">The selected nodes, sorted, each one existing.</param>
internal sealed record NodeTarget(PathShape Path, IReadOnlyList<int> Indices);
