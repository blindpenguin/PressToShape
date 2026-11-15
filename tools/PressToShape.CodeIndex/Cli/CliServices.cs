using System;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Output;

namespace PressToShape.CodeIndex.Cli;

/// <summary>What the command line needs from the outside world. Tests replace all three, so no process, solution or disk is involved.</summary>
internal sealed record CliServices(
    Func<string, string> FindRepoRoot,
    Func<string, string, ICompilationSource> CreateCompilationSource,
    Func<string, IIndexFolder> CreateIndexFolder)
{
    public static CliServices Default { get; } = new(
        RepoRoot.Find,
        (solutionPath, repoRoot) => new SolutionLoader(solutionPath, repoRoot),
        indexDirectory => new DiskIndexFolder(indexDirectory));
}
