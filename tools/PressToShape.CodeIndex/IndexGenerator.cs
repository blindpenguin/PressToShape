using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Avalonia;
using PressToShape.CodeIndex.Avalonia.Model;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Relationships;
using PressToShape.CodeIndex.Xaml;

namespace PressToShape.CodeIndex;

/// <summary>The result of an analysis: the data, and whether a project failed (a failure other than a warning).</summary>
internal sealed record GenerateResult(IndexData Data, bool HasErrors);

/// <summary>Runs extraction and relationship building over every loaded project. Writing is a separate step, so <c>check</c> can compare without touching the disk.</summary>
internal sealed class IndexGenerator
{
    private readonly ICompilationSource _source;
    private readonly string _repoRoot;
    private readonly IXamlFileSource? _xamlSource;

    /// <param name="xamlSource">Where the XAML files come from. Null means that XAML is not indexed.</param>
    public IndexGenerator(ICompilationSource source, string repoRoot, IXamlFileSource? xamlSource = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(repoRoot);
        _source = source;
        _repoRoot = repoRoot;
        _xamlSource = xamlSource;
    }

    public async Task<GenerateResult> GenerateAsync(CancellationToken cancellationToken)
    {
        var loaded = await _source.LoadAsync(cancellationToken);
        var extractor = new SymbolExtractor(_repoRoot);

        var extracted = loaded.Projects
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .Select(p => (Project: p, Symbols: extractor.Extract(p, cancellationToken)))
            .ToList();

        // Edges may point into other projects, so the ids of the whole solution are collected first.
        var knownIds = extracted.SelectMany(e => e.Symbols).Select(s => s.Id).ToHashSet(StringComparer.Ordinal);
        var builder = new RelationshipBuilder(knownIds, _repoRoot);

        var xaml = _xamlSource is null
            ? XamlIndexResult.Empty
            : new XamlIndexer(_xamlSource).Index(loaded.Projects.ToList(), knownIds, cancellationToken);

        var xamlClassIds = xaml.Projects.Values.SelectMany(p => p.Files).Select(f => f.ClassId).OfType<string>().ToHashSet(StringComparer.Ordinal);
        var factsExtractor = new AvaloniaFactsExtractor(_repoRoot);
        var facts = new List<AvaloniaFacts>();

        var projects = extracted
            .Select(e =>
            {
                var xamlData = xaml.For(e.Project.Name);
                var edges = builder.Build(e.Project, cancellationToken)
                    .Concat(xamlData.Relationships)
                    .Concat(CommandHandlerLinker.FromGenerated(e.Symbols))
                    .ToList();
                facts.Add(factsExtractor.Extract(e.Project, knownIds, xamlClassIds, cancellationToken));
                return new ProjectIndex(e.Project.Name, e.Project.ProjectPath, e.Symbols, edges, xamlData.Files);
            })
            .ToList();

        var problems = loaded.Problems.Concat(xaml.Problems).ToList();
        return new GenerateResult(new IndexData(projects, problems, Merge(facts)), problems.Any(p => !p.IsWarning));
    }

    private static AvaloniaFacts Merge(IReadOnlyList<AvaloniaFacts> parts) => new(
        parts.SelectMany(p => p.Controls).OrderBy(c => c.Id, StringComparer.Ordinal).ToList(),
        parts.SelectMany(p => p.Converters).OrderBy(c => c.Id, StringComparer.Ordinal).ToList(),
        parts.SelectMany(p => p.Applications).OrderBy(a => a.Id, StringComparer.Ordinal).ToList());

    /// <summary>Writes the JSON and Markdown files of the data to the sink.</summary>
    public static void Write(IndexData data, IIndexFileSink sink)
    {
        JsonIndexWriter.Write(data, sink);
        MarkdownWriter.Write(data, sink);
    }
}
