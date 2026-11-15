using System.Collections.Generic;

namespace PressToShape.CodeIndex.Model;

/// <summary>The complete result of one analysis run, ready to be written.</summary>
internal sealed record IndexData(IReadOnlyList<ProjectIndex> Projects, IReadOnlyList<LoadProblem> Warnings);
