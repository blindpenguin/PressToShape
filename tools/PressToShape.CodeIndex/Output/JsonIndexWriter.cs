using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Output;

/// <summary>Writes the manifest, the per-namespace symbol files and the per-project relationship files. The output depends only on the data, not on the order it arrives in.</summary>
internal static class JsonIndexWriter
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        WriteIndented = true,
        NewLine = "\n",
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static void Write(IndexData data, IIndexFileSink sink)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(sink);

        var projects = data.Projects.OrderBy(p => p.Name, StringComparer.Ordinal).ToList();
        foreach (var project in projects)
        {
            WriteSymbols(project, sink);
            WriteRelationships(project, sink);
        }

        WriteManifest(projects, data.Warnings, sink);
        NamesTableWriter.Write(projects, sink);
    }

    private static void WriteSymbols(ProjectIndex project, IIndexFileSink sink)
    {
        foreach (var group in project.Symbols.GroupBy(s => s.Namespace).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var file = new SymbolsFile(IndexSchema.Version, project.Name, group.Key, group.OrderBy(s => s.Id, StringComparer.Ordinal).ToList());
            sink.Write(IndexLayout.SymbolsPath(project.Name, group.Key), Serialize(file));
        }
    }

    private static void WriteRelationships(ProjectIndex project, IIndexFileSink sink)
    {
        var edges = project.Relationships
            .OrderBy(e => e.Type)
            .ThenBy(e => e.FromId, StringComparer.Ordinal)
            .ThenBy(e => e.ToId, StringComparer.Ordinal)
            .ThenBy(e => e.File, StringComparer.Ordinal)
            .ThenBy(e => e.Line)
            .ToList();
        sink.Write(IndexLayout.RelationshipsPath(project.Name), Serialize(new RelationshipsFile(IndexSchema.Version, project.Name, edges)));
    }

    private static void WriteManifest(List<ProjectIndex> projects, IReadOnlyList<LoadProblem> warnings, IIndexFileSink sink)
    {
        var manifest = new IndexManifest(
            IndexSchema.Version,
            IndexSchema.ToolVersion,
            projects.Select(p => new ManifestProject(p.Name, p.Path, p.Symbols.Count, p.Relationships.Count)).ToList(),
            projects.Sum(p => p.Symbols.Count),
            projects.Sum(p => p.Relationships.Count),
            warnings.OrderBy(w => w.Project, StringComparer.Ordinal).ThenBy(w => w.Reason, StringComparer.Ordinal).ToList());
        sink.Write(IndexLayout.ManifestPath, Serialize(manifest));
    }

    private static byte[] Serialize<T>(T value)
    {
        // A trailing newline keeps the files friendly to text tools and diffs.
        var json = JsonSerializer.Serialize(value, Options) + "\n";
        return new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(json);
    }
}

/// <summary>The content of one <c>symbols/&lt;project&gt;/&lt;namespace&gt;.json</c> file.</summary>
internal sealed record SymbolsFile(int SchemaVersion, string Project, string Namespace, IReadOnlyList<SymbolRecord> Symbols);

/// <summary>The content of one <c>relationships/&lt;project&gt;.json</c> file.</summary>
internal sealed record RelationshipsFile(int SchemaVersion, string Project, IReadOnlyList<RelationshipRecord> Relationships);
