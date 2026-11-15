using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    /// <summary>Used for one record per line in the large files. Same names and escaping as <see cref="Options"/>, without indentation.</summary>
    private static JsonSerializerOptions LineOptions { get; } = new(Options) { WriteIndented = false };

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
            var symbols = group.OrderBy(s => s.Id, StringComparer.Ordinal).ToList();
            sink.Write(IndexLayout.SymbolsPath(project.Name, group.Key), SerializeLines(project.Name, group.Key, nameof(SymbolsFile.Symbols), symbols));
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
        sink.Write(IndexLayout.RelationshipsPath(project.Name), SerializeLines(project.Name, null, nameof(RelationshipsFile.Relationships), edges));
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

    /// <summary>
    /// Writes a file of the shape <c>{ SchemaVersion, Project, [Namespace], &lt;list&gt; }</c> with each list item on its own line.
    /// This keeps the file small and makes a git diff show only the changed records.
    /// </summary>
    private static byte[] SerializeLines<T>(string project, string? ns, string listName, IReadOnlyList<T> items)
    {
        var text = new StringBuilder();
        text.Append("{\"SchemaVersion\":").Append(IndexSchema.Version);
        text.Append(",\"Project\":").Append(JsonSerializer.Serialize(project, LineOptions));
        if (ns is not null)
        {
            text.Append(",\"Namespace\":").Append(JsonSerializer.Serialize(ns, LineOptions));
        }

        text.Append(",\"").Append(listName).Append("\":[");
        for (var i = 0; i < items.Count; i++)
        {
            text.Append('\n').Append(JsonSerializer.Serialize(items[i], LineOptions));
            if (i < items.Count - 1)
            {
                text.Append(',');
            }
        }

        text.Append(items.Count > 0 ? "\n]}\n" : "]}\n");
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(text.ToString());
    }

    private static byte[] Serialize<T>(T value)
    {
        // A trailing newline keeps the files friendly to text tools and diffs.
        var json = JsonSerializer.Serialize(value, Options) + "\n";
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(json);
    }
}

/// <summary>The content of one <c>symbols/&lt;project&gt;/&lt;namespace&gt;.json</c> file.</summary>
internal sealed record SymbolsFile(int SchemaVersion, string Project, string Namespace, IReadOnlyList<SymbolRecord> Symbols);

/// <summary>The content of one <c>relationships/&lt;project&gt;.json</c> file.</summary>
internal sealed record RelationshipsFile(int SchemaVersion, string Project, IReadOnlyList<RelationshipRecord> Relationships);
