using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace PressToShape.CodeIndex.Extraction;

/// <summary>A loaded project: its name, its repo-relative project file, its compilation and the syntax trees that source generators produced.</summary>
internal sealed record ProjectCompilation(
    string Name,
    string ProjectPath,
    Compilation Compilation,
    IReadOnlySet<SyntaxTree>? GeneratedTrees = null)
{
    public bool IsGenerated(SyntaxTree tree) => GeneratedTrees?.Contains(tree) ?? false;
}
