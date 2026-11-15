using System.Collections.Generic;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Loading;

/// <summary>The projects that could be compiled, and the problems with the ones that could not.</summary>
internal sealed record LoadResult(IReadOnlyList<ProjectCompilation> Projects, IReadOnlyList<LoadProblem> Problems);
