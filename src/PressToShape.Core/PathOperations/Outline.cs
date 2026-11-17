using System.Collections.Generic;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.PathOperations;

/// <summary>
/// The outline of one operand of a boolean operation: its sub-paths in document coordinates, with the rotation already applied, and the
/// fill rule that tells what is inside. An open sub-path counts as closed by a straight line. Plain geometry, no shape or document.
/// </summary>
/// <param name="SubPaths">The sub-paths, at least one.</param>
/// <param name="FillRule">How the inside is decided across the sub-paths.</param>
public sealed record Outline(IReadOnlyList<DocumentSubPath> SubPaths, FillRule FillRule);
