using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Output;

/// <summary>Writes <c>names.tsv</c>, a flat list for grep. Columns: name, id, index file, symbol type, source path and line. XAML entries have the types View, Element, Resource, Style and Template.</summary>
internal static class NamesTableWriter
{
    public static void Write(IReadOnlyList<ProjectIndex> projects, IIndexFileSink sink)
    {
        var entries = projects
            .SelectMany(p => p.Symbols)
            .Where(s => s.SymbolType is not SymbolType.Namespace and not SymbolType.Constructor)
            .Select(s => (s.Name, s.Id, Line: string.Join('\t', Clean(s.Name), Clean(s.Id), IndexLayout.SymbolsPath(s.Project, s.Namespace), s.SymbolType, $"{s.File}:{s.StartLine}")))
            .Concat(projects.SelectMany(XamlEntries));
        var lines = entries
            .OrderBy(e => e.Name, StringComparer.Ordinal)
            .ThenBy(e => e.Id, StringComparer.Ordinal)
            .Select(e => e.Line);

        var text = new StringBuilder();
        foreach (var line in lines)
        {
            text.Append(line).Append('\n');
        }

        sink.Write(IndexLayout.NamesPath, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(text.ToString()));
    }

    /// <summary>
    /// Entries for the XAML of a project: the file (type <c>View</c>, named like the file without its extension), named elements, resources, styles and templates.
    /// The index file column is the XAML JSON file.
    /// </summary>
    private static IEnumerable<(string Name, string Id, string Line)> XamlEntries(ProjectIndex project)
    {
        var directory = XamlJsonWriter.DirectoryOf(project.Path);
        foreach (var file in project.Xaml)
        {
            var indexFile = IndexLayout.XamlPath(project.Name, directory, file.File);
            yield return Entry(FileNameWithoutExtension(file.File), file.Id, indexFile, "View", file.File, 1);

            foreach (var element in file.Elements.Where(e => e.Name is not null && e.Property is null))
            {
                yield return Entry(element.Name!, element.Id, indexFile, "Element", file.File, element.Line);
            }

            foreach (var resource in file.Resources)
            {
                yield return Entry(resource.Key, resource.Id, indexFile, "Resource", file.File, resource.Line);
            }

            foreach (var style in file.Styles)
            {
                yield return Entry(style.Key ?? style.Selector, style.Id, indexFile, "Style", file.File, style.Line);
            }

            foreach (var template in file.Templates)
            {
                yield return Entry(template.Key ?? template.Target ?? template.TemplateType, template.Id, indexFile, "Template", file.File, template.Line);
            }
        }
    }

    private static (string Name, string Id, string Line) Entry(string name, string id, string indexFile, string symbolType, string file, int line) =>
        (name, id, string.Join('\t', Clean(name), Clean(id), indexFile, symbolType, $"{file}:{line}"));

    private static string FileNameWithoutExtension(string path)
    {
        var name = path[(path.LastIndexOf('/') + 1)..];
        var dot = name.LastIndexOf('.');
        return dot > 0 ? name[..dot] : name;
    }

    private static string Clean(string value) => value.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');
}
