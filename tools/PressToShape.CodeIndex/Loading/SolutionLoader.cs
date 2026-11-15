using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Loading;

/// <summary>Opens the solution with MSBuild, like an IDE does, so references and source generators are resolved. This is the only unit that knows about MSBuild.</summary>
internal sealed class SolutionLoader : ICompilationSource
{
    private const int MaxMessagesPerProject = 3;

    private readonly string _solutionPath;
    private readonly string _repoRoot;

    public SolutionLoader(string solutionPath, string repoRoot)
    {
        ArgumentException.ThrowIfNullOrEmpty(solutionPath);
        ArgumentException.ThrowIfNullOrEmpty(repoRoot);
        _solutionPath = solutionPath;
        _repoRoot = repoRoot;
    }

    public async Task<LoadResult> LoadAsync(CancellationToken cancellationToken)
    {
        RegisterMsBuild();

        var problems = new List<LoadProblem>();
        var projects = new List<ProjectCompilation>();
        using var workspace = MSBuildWorkspace.Create();
        workspace.RegisterWorkspaceFailedHandler(e =>
        {
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
            {
                problems.Add(new LoadProblem("(workspace)", Scrub(e.Diagnostic.Message), true));
            }
        });

        Solution solution;
        try
        {
            solution = await workspace.OpenSolutionAsync(_solutionPath, cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
        {
            throw new IndexToolException($"Could not open the solution: {Scrub(ex.Message)} Run `dotnet restore` first and check that the .NET SDK is installed.", ex);
        }

        foreach (var project in solution.Projects.Where(p => p.Language == LanguageNames.CSharp).OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = RelativeProjectPath(project);
            if (!LoadPolicy.IsIndexed(relativePath))
            {
                continue;
            }

            var loaded = await LoadProjectAsync(project, relativePath, problems, cancellationToken);
            if (loaded is not null)
            {
                projects.Add(loaded);
            }
        }

        return new LoadResult(projects, problems);
    }

    private async Task<ProjectCompilation?> LoadProjectAsync(Project project, string relativePath, List<LoadProblem> problems, CancellationToken ct)
    {
        var isWarning = LoadPolicy.IsWarning(project.Name);
        var compilation = await project.GetCompilationAsync(ct);
        if (compilation is null)
        {
            problems.Add(new LoadProblem(project.Name, "The project could not be compiled. Run `dotnet restore` and check the SDK and workloads.", isWarning));
            return null;
        }

        var errors = compilation.GetDiagnostics(ct).Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        if (errors.Count > 0)
        {
            var messages = errors.Take(MaxMessagesPerProject).Select(d => $"{d.Id}: {d.GetMessage()}");
            problems.Add(new LoadProblem(project.Name, Scrub($"{errors.Count} compile errors, first: {string.Join(" | ", messages)}"), isWarning));
            return null;
        }

        return new ProjectCompilation(project.Name, relativePath, compilation, await GeneratedTreesAsync(project, compilation, ct));
    }

    private static async Task<IReadOnlySet<SyntaxTree>> GeneratedTreesAsync(Project project, Compilation compilation, CancellationToken ct)
    {
        var paths = new HashSet<string>(StringComparer.Ordinal);
        var trees = new HashSet<SyntaxTree>();
        foreach (var document in await project.GetSourceGeneratedDocumentsAsync(ct))
        {
            if (document.FilePath is { } path)
            {
                paths.Add(path);
            }

            if (await document.GetSyntaxTreeAsync(ct) is { } tree)
            {
                trees.Add(tree);
            }
        }

        foreach (var tree in compilation.SyntaxTrees.Where(t => paths.Contains(t.FilePath)))
        {
            trees.Add(tree);
        }

        return trees;
    }

    private string RelativeProjectPath(Project project) =>
        project.FilePath is null ? string.Empty : Path.GetRelativePath(_repoRoot, project.FilePath).Replace('\\', '/');

    private string Scrub(string text) => PathScrubber.Scrub(text, _repoRoot);

    private static void RegisterMsBuild()
    {
        if (MSBuildLocator.IsRegistered)
        {
            return;
        }

        try
        {
            MSBuildLocator.RegisterDefaults();
        }
        catch (InvalidOperationException ex)
        {
            throw new IndexToolException("No .NET SDK with MSBuild was found. Install the .NET 10 SDK and try again.", ex);
        }
    }
}
