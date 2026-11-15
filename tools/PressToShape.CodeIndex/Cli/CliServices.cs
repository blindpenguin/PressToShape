using System;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Xaml;

namespace PressToShape.CodeIndex.Cli;

/// <summary>What the command line needs from the outside world. Tests replace them, so no process, solution or disk is involved.</summary>
internal sealed record CliServices(
    Func<string, string> FindRepoRoot,
    Func<string, string, ICompilationSource> CreateCompilationSource,
    Func<string, IIndexFolder> CreateIndexFolder,
    Func<string, IXamlFileSource>? CreateXamlFileSource = null)
{
    public static CliServices Default { get; } = new(
        RepoRoot.Find,
        (solutionPath, repoRoot) => new SolutionLoader(solutionPath, repoRoot),
        indexDirectory => new DiskIndexFolder(indexDirectory),
        repoRoot => new DiskXamlFileSource(repoRoot));
}
