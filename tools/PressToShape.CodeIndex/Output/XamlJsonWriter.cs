using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Output;

/// <summary>
/// Writes one file per XAML file below <c>xaml/&lt;Project&gt;/</c>. The header and the lists have the names of <see cref="XamlFileRecord"/>, so the file reads back as that record.
/// Each list item is on its own line, which keeps the files small and the git diffs short.
/// </summary>
internal static class XamlJsonWriter
{
    private static readonly JsonSerializerOptions LineOptions = new(JsonIndexWriter.Options) { WriteIndented = false };

    public static void Write(IReadOnlyList<ProjectIndex> projects, IIndexFileSink sink)
    {
        foreach (var project in projects)
        {
            var directory = DirectoryOf(project.Path);
            foreach (var file in project.Xaml.OrderBy(f => f.Id, StringComparer.Ordinal))
            {
                sink.Write(IndexLayout.XamlPath(project.Name, directory, file.File), Serialize(file));
            }
        }
    }

    internal static string DirectoryOf(string path)
    {
        var slash = path.LastIndexOf('/');
        return slash < 0 ? string.Empty : path[..slash];
    }

    private static byte[] Serialize(XamlFileRecord file)
    {
        var header = JsonSerializer.Serialize(
            new Header(
                IndexSchema.Version,
                file.Id,
                file.Project,
                file.File,
                file.Root,
                file.Class,
                file.ClassId,
                file.DataType,
                file.DataTypeId,
                new SortedDictionary<string, string>(file.Namespaces.ToDictionary(p => p.Key, p => p.Value), StringComparer.Ordinal),
                file.Findings),
            LineOptions);

        var text = new StringBuilder(header[..^1]);
        AppendList(text, nameof(XamlFileRecord.Elements), file.Elements.OrderBy(e => e.Line).ThenBy(e => e.Id, StringComparer.Ordinal).ToList());
        AppendList(text, nameof(XamlFileRecord.Bindings), file.Bindings.OrderBy(b => b.Line).ThenBy(b => b.Id, StringComparer.Ordinal).ToList());
        AppendList(text, nameof(XamlFileRecord.Resources), file.Resources.OrderBy(r => r.Line).ThenBy(r => r.Id, StringComparer.Ordinal).ToList());
        AppendList(text, nameof(XamlFileRecord.ResourceRefs), file.ResourceRefs.OrderBy(r => r.Line).ThenBy(r => r.Id, StringComparer.Ordinal).ToList());
        AppendList(text, nameof(XamlFileRecord.Styles), file.Styles.OrderBy(s => s.Line).ThenBy(s => s.Id, StringComparer.Ordinal).ToList());
        AppendList(text, nameof(XamlFileRecord.Templates), file.Templates.OrderBy(t => t.Line).ThenBy(t => t.Id, StringComparer.Ordinal).ToList());
        AppendList(text, nameof(XamlFileRecord.Handlers), file.Handlers.OrderBy(h => h.Line).ThenBy(h => h.Id, StringComparer.Ordinal).ToList());
        text.Append("}\n");
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(text.ToString());
    }

    private static void AppendList<T>(StringBuilder text, string name, IReadOnlyList<T> items)
    {
        text.Append(",\"").Append(name).Append("\":[");
        for (var i = 0; i < items.Count; i++)
        {
            text.Append('\n').Append(JsonSerializer.Serialize(items[i], LineOptions));
            if (i < items.Count - 1)
            {
                text.Append(',');
            }
        }

        text.Append(items.Count > 0 ? "\n]" : "]");
    }

    private sealed record Header(
        int SchemaVersion,
        string Id,
        string Project,
        string File,
        string Root,
        string? Class,
        string? ClassId,
        string? DataType,
        string? DataTypeId,
        IReadOnlyDictionary<string, string> Namespaces,
        IReadOnlyList<string> Findings);
}
