using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Relationships;

namespace PressToShape.CodeIndex;

/// <summary>The result of an analysis: the data, and whether a project failed (a failure other than a warning).</summary>
internal sealed record GenerateResult(IndexData Data, bool HasErrors);

/// <summary>Runs extraction and relationship building over every loaded project. Writing is a separate step, so <c>check</c> can compare without touching the disk.</summary>
internal sealed class IndexGenerator
{
    private readonly ICompilationSource _source;
    private readonly string _repoRoot;

    public IndexGenerator(ICompilationSource source, string repoRoot)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(repoRoot);
        _source = source;
        _repoRoot = repoRoot;
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

        var projects = extracted
            .Select(e => new ProjectIndex(e.Project.Name, e.Project.ProjectPath, e.Symbols, builder.Build(e.Project, cancellationToken)))
            .ToList();

        var data = new IndexData(projects, loaded.Problems);
        return new GenerateResult(data, loaded.Problems.Any(p => !p.IsWarning));
    }

    /// <summary>Writes the JSON and Markdown files of the data to the sink.</summary>
    public static void Write(IndexData data, IIndexFileSink sink)
    {
        JsonIndexWriter.Write(data, sink);
        MarkdownWriter.Write(data, sink);
    }
}
