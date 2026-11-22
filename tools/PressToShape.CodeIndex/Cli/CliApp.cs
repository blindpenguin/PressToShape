using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Chain;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Query;

namespace PressToShape.CodeIndex.Cli;

/// <summary>Runs one command. It takes its console writers and services as parameters, so tests need no process.</summary>
internal sealed class CliApp
{
    public const int Success = 0;
    public const int Failure = 1;
    public const int UsageError = 2;

    private const string SolutionFileName = RepoRoot.SolutionFileName;
    private const string IndexFolder = "docs/index";

    private static readonly HashSet<string> QueryCommands = ["find", "show", "impls", "derived", "refs", "deps", "chain"];

    private readonly TextWriter _out;
    private readonly TextWriter _err;
    private readonly string _startDirectory;
    private readonly CliServices _services;

    public CliApp(TextWriter output, TextWriter error, string startDirectory, CliServices services)
    {
        _out = output;
        _err = error;
        _startDirectory = startDirectory;
        _services = services;
    }

    public async Task<int> RunAsync(IReadOnlyList<string> args, CancellationToken cancellationToken)
    {
        var options = CliOptions.Parse(args);
        if (options.Error is not null)
        {
            return Usage(options.Error);
        }

        if (options.Help)
        {
            _out.Write(CliText.Usage + "\n");
            return Success;
        }

        if (options.Command is null)
        {
            return Usage("No command given.");
        }

        var isQuery = QueryCommands.Contains(options.Command);
        if (options.Command is not ("generate" or "check") && !isQuery)
        {
            return Usage($"Unknown command: {options.Command}");
        }

        if (isQuery && options.Argument is null)
        {
            return Usage($"The command {options.Command} needs a name, an id or a search term.");
        }

        if (!isQuery && options.Argument is not null)
        {
            return Usage($"Unexpected argument: {options.Argument}");
        }

        var scrubRoot = _startDirectory;
        try
        {
            var (solutionPath, repoRoot) = ResolvePaths(options);
            scrubRoot = repoRoot;
            var folder = _services.CreateIndexFolder(Path.Combine(repoRoot, IndexFolder.Replace('/', Path.DirectorySeparatorChar)));
            return options.Command switch
            {
                "generate" => await GenerateAsync(solutionPath, repoRoot, folder, cancellationToken),
                "check" => await CheckAsync(solutionPath, repoRoot, folder, cancellationToken),
                _ => Query(options, folder),
            };
        }
        catch (IndexToolException ex)
        {
            _err.Write(ex.Message + "\n");
            return Failure;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            // A file the user can fix (permissions, a locked or missing folder). Other exceptions are bugs and keep their stack trace.
            _err.Write($"A file problem stopped the command: {PathScrubber.Scrub(ex.Message, scrubRoot)}\n");
            return Failure;
        }
    }

    private (string SolutionPath, string RepoRoot) ResolvePaths(CliOptions options)
    {
        if (options.Solution is not null)
        {
            var solution = Path.GetFullPath(options.Solution, _startDirectory);
            return (solution, Path.GetDirectoryName(solution) ?? _startDirectory);
        }

        var root = _services.FindRepoRoot(_startDirectory);
        return (Path.Combine(root, SolutionFileName), root);
    }

    private async Task<int> GenerateAsync(string solutionPath, string repoRoot, IIndexFolder folder, CancellationToken ct)
    {
        var result = await Analyse(solutionPath, repoRoot, ct);
        if (result.HasErrors)
        {
            // A partly updated index would be worse than none, and removing files of the failed project would delete good data.
            _err.Write($"Nothing was written, because a project failed. Fix it and run: {IndexHints.Regenerate}\n");
            return Failure;
        }

        // The whole index is collected in memory first. If two source files map to one index file, this throws before any file of the existing index is touched.
        var fresh = BuildInMemoryIndex(result.Data);
        foreach (var (path, bytes) in fresh.Files)
        {
            folder.Write(path, bytes);
        }

        var removed = folder.RemoveStale();

        var projects = result.Data.Projects;
        var xamlFiles = projects.SelectMany(p => p.Xaml).ToList();
        _out.Write($"Indexed {projects.Count} projects, {projects.Sum(p => p.Symbols.Count)} symbols, {projects.Sum(p => p.Relationships.Count)} relationships and {xamlFiles.Count} XAML files into {IndexFolder}.\n");
        foreach (var finding in xamlFiles.OrderBy(f => f.File, StringComparer.Ordinal).SelectMany(f => f.Findings.Select(text => (f.File, text))))
        {
            _err.Write($"Finding: {finding.File}: {finding.text}\n");
        }

        if (removed.Count > 0)
        {
            _out.Write($"Removed {removed.Count} files that are no longer produced.\n");
        }

        return Success;
    }

    private static InMemoryFileSink BuildInMemoryIndex(IndexData data)
    {
        var fresh = new InMemoryFileSink();
        IndexGenerator.Write(data, fresh);
        return fresh;
    }

    private async Task<int> CheckAsync(string solutionPath, string repoRoot, IIndexFolder folder, CancellationToken ct)
    {
        var result = await Analyse(solutionPath, repoRoot, ct);
        if (result.HasErrors)
        {
            _err.Write("The index cannot be checked, because a project failed.\n");
            return Failure;
        }

        var fresh = BuildInMemoryIndex(result.Data);

        var differences = new List<string>();
        foreach (var (path, bytes) in fresh.Files)
        {
            var existing = folder.Read(path);
            if (existing is null)
            {
                differences.Add("missing: " + path);
            }
            else if (!existing.AsSpan().SequenceEqual(bytes))
            {
                differences.Add("changed: " + path);
            }
        }

        differences.AddRange(folder.ListFiles().Where(f => !fresh.Files.ContainsKey(f)).Select(f => "stale: " + f));

        if (differences.Count == 0)
        {
            _out.Write("The index is up to date.\n");
            return Success;
        }

        _out.Write($"The index in {IndexFolder} is out of date ({differences.Count} files differ). Regenerate it with: {IndexHints.Regenerate}\n");
        foreach (var difference in differences.OrderBy(d => d, StringComparer.Ordinal))
        {
            _out.Write(difference + "\n");
        }

        return Failure;
    }

    private async Task<GenerateResult> Analyse(string solutionPath, string repoRoot, CancellationToken ct)
    {
        var source = _services.CreateCompilationSource(solutionPath, repoRoot);
        var result = await new IndexGenerator(source, repoRoot, _services.CreateXamlFileSource?.Invoke(repoRoot)).GenerateAsync(ct);
        foreach (var problem in result.Data.Warnings.OrderBy(p => p.Project, StringComparer.Ordinal))
        {
            _err.Write($"{(problem.IsWarning ? "Warning" : "Error")}: {problem.Project}: {problem.Reason}\n");
        }

        return result;
    }

    private int Query(CliOptions options, IIndexFolder folder)
    {
        var query = new QueryService(new IndexReader(folder));
        var printer = new CliPrinter(_out, options.Json);
        var argument = options.Argument!;

        if (options.Command == "find")
        {
            printer.Search(argument, query.Search(argument));
            return Success;
        }

        if (options.Command == "chain")
        {
            return ChainPrinter.Print(new ChainBuilder(new IndexReader(folder)).Build(argument, options.Depth), _out, options.Json) ? Success : Failure;
        }

        var found = options.Command switch
        {
            "show" => printer.Resolved(argument, query.Show(argument), printer.Show),
            "impls" => printer.Resolved(argument, query.Implementations(argument), printer.Hits),
            "derived" => printer.Resolved(argument, query.Derived(argument), printer.Hits),
            "refs" => printer.Resolved(argument, query.References(argument), printer.Hits),
            _ => printer.Resolved(argument, query.Dependencies(argument), printer.Hits),
        };
        return found ? Success : Failure;
    }

    private int Usage(string message)
    {
        _err.Write(message + "\n\n" + CliText.Usage + "\n");
        return UsageError;
    }
}
