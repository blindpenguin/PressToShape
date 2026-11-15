using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Output;

/// <summary>One Markdown page per XAML file, rendered only from the index data. Verified, inferred and unresolved items are marked.</summary>
internal static class XamlMarkdownWriter
{
    public static void Write(IReadOnlyList<ProjectIndex> projects, IIndexFileSink sink)
    {
        var edges = projects.SelectMany(p => p.Relationships).ToList();
        foreach (var project in projects)
        {
            var directory = XamlJsonWriter.DirectoryOf(project.Path);
            foreach (var file in project.Xaml.OrderBy(f => f.Id, StringComparer.Ordinal))
            {
                sink.Write(IndexLayout.XamlMarkdownPath(project.Name, directory, file.File), Encode(Render(file, edges)));
            }
        }
    }

    /// <summary>The README lines that link the XAML pages of a project.</summary>
    public static IEnumerable<string> ReadmeLines(ProjectIndex project)
    {
        var directory = XamlJsonWriter.DirectoryOf(project.Path);
        foreach (var file in project.Xaml.OrderBy(f => f.Id, StringComparer.Ordinal))
        {
            var link = IndexLayout.XamlMarkdownPath(project.Name, directory, file.File)["markdown/".Length..];
            yield return $"- [{file.File[(file.File.LastIndexOf('/') + 1)..]}]({link}) (XAML, {file.Bindings.Count} bindings)";
        }
    }

    private static string Render(XamlFileRecord file, List<RelationshipRecord> edges)
    {
        var elements = file.Elements.ToDictionary(e => e.Id, StringComparer.Ordinal);
        var text = new StringBuilder();
        text.Append("# ").Append(file.File[(file.File.LastIndexOf('/') + 1)..]).Append(" (XAML)\n\n");
        text.Append("Project: ").Append(file.Project).Append(". File: `").Append(file.File).Append("`. Root: `").Append(file.Root).Append("`.\n");
        text.Append(file.Class is null ? "No `x:Class`.\n" : $"Class: `{file.Class}`{(file.ClassId is null ? " (not in this solution)" : string.Empty)}.\n");

        AppendLinks(text, file, edges);
        AppendBindings(text, file, elements);
        AppendResourceRefs(text, file, elements);
        AppendDefinitions(text, file);
        AppendHandlers(text, file, elements);

        if (file.Findings.Count > 0)
        {
            text.Append("\n## Findings\n\n");
            foreach (var finding in file.Findings)
            {
                text.Append("- ").Append(finding).Append('\n');
            }
        }

        return text.ToString();
    }

    private static void AppendLinks(StringBuilder text, XamlFileRecord file, List<RelationshipRecord> edges)
    {
        var links = new List<string>();
        if (file.ClassId is not null)
        {
            links.AddRange(edges
                .Where(e => e.FromId == file.ClassId && e.Type == RelationshipType.UsesViewModel)
                .Select(e => $"`{e.ToId}` ({Mark(e.Confidence)}): {e.Reason}" + (e.File is null ? string.Empty : $" ({e.File}:{e.Line})")));
            links.AddRange(edges
                .Where(e => e.ToId == file.ClassId && e.Type == RelationshipType.UsesViewModel)
                .Select(e => $"used as the view of `{e.FromId}` ({Mark(e.Confidence)}): {e.Reason}"));
        }

        if (links.Count == 0)
        {
            return;
        }

        text.Append("\n## View model links\n\n");
        foreach (var link in links.Distinct().OrderBy(l => l, StringComparer.Ordinal))
        {
            text.Append("- ").Append(link).Append('\n');
        }
    }

    private static void AppendBindings(StringBuilder text, XamlFileRecord file, Dictionary<string, XamlElementRecord> elements)
    {
        if (file.Bindings.Count == 0)
        {
            return;
        }

        text.Append("\n## Bindings\n\n");
        foreach (var binding in file.Bindings.OrderBy(b => b.Line).ThenBy(b => b.Id, StringComparer.Ordinal))
        {
            var target = binding.TargetId is null
                ? $"unresolved: {binding.Reason}"
                : $"`{binding.TargetId}` ({Mark(binding.Confidence)}{(binding.Confidence == Confidence.Inferred ? ": " + binding.Reason : string.Empty)})";
            var command = binding.IsCommand ? ", command" : string.Empty;
            var converter = binding.Converter is null ? string.Empty : $", converter {binding.Converter}";
            var source = binding.Source is null ? string.Empty : $", source {binding.Source}";
            text.Append($"- line {binding.Line}: {ElementName(elements, binding.ElementId)}.{binding.Property} = {binding.BindingType} `{binding.Path}` -> {target}{command}{converter}{source}\n");
        }
    }

    private static void AppendResourceRefs(StringBuilder text, XamlFileRecord file, Dictionary<string, XamlElementRecord> elements)
    {
        if (file.ResourceRefs.Count == 0)
        {
            return;
        }

        text.Append("\n## Resource references\n\n");
        foreach (var reference in file.ResourceRefs.OrderBy(r => r.Line).ThenBy(r => r.Id, StringComparer.Ordinal))
        {
            var target = reference.TargetIds.Count == 0 ? $"unresolved: {reference.Reason}" : string.Join(", ", reference.TargetIds.Select(t => $"`{t}`")) + " (verified)";
            text.Append($"- line {reference.Line}: {ElementName(elements, reference.ElementId)}.{reference.Property} = {reference.RefType} `{reference.Key}` -> {target}\n");
        }
    }

    private static void AppendDefinitions(StringBuilder text, XamlFileRecord file)
    {
        if (file.Resources.Count > 0)
        {
            text.Append("\n## Resources defined\n\n");
            foreach (var resource in file.Resources.OrderBy(r => r.Line).ThenBy(r => r.Id, StringComparer.Ordinal))
            {
                text.Append($"- line {resource.Line}: `{resource.Key}` ({resource.Type}){(resource.Variant is null ? string.Empty : $", theme {resource.Variant}")}\n");
            }
        }

        if (file.Styles.Count > 0)
        {
            text.Append("\n## Styles\n\n");
            foreach (var style in file.Styles.OrderBy(s => s.Line).ThenBy(s => s.Id, StringComparer.Ordinal))
            {
                var setters = style.Setters.Count == 0 ? string.Empty : $", sets {string.Join(", ", style.Setters)}";
                var keys = style.ResourceKeys.Count == 0 ? string.Empty : $", uses {string.Join(", ", style.ResourceKeys)}";
                text.Append($"- line {style.Line}: `{style.Selector}`{(style.Key is null ? string.Empty : $" (key {style.Key})")}{setters}{keys}\n");
            }
        }

        if (file.Templates.Count > 0)
        {
            text.Append("\n## Templates\n\n");
            foreach (var template in file.Templates.OrderBy(t => t.Line).ThenBy(t => t.Id, StringComparer.Ordinal))
            {
                text.Append($"- line {template.Line}: {template.TemplateType}{(template.Key is null ? string.Empty : $" `{template.Key}`")}{(template.Target is null ? string.Empty : $" for `{template.Target}`")}{(template.TargetId is null ? string.Empty : $" (`{template.TargetId}`)")}\n");
            }
        }
    }

    private static void AppendHandlers(StringBuilder text, XamlFileRecord file, Dictionary<string, XamlElementRecord> elements)
    {
        if (file.Handlers.Count == 0)
        {
            return;
        }

        text.Append("\n## Event handlers\n\n");
        foreach (var handler in file.Handlers.OrderBy(h => h.Line).ThenBy(h => h.Id, StringComparer.Ordinal))
        {
            var target = handler.TargetId is null ? $"unresolved: {handler.Reason}" : $"`{handler.TargetId}` ({Mark(handler.Confidence)})";
            text.Append($"- line {handler.Line}: {ElementName(elements, handler.ElementId)}.{handler.Event} = `{handler.Handler}` -> {target}\n");
        }
    }

    private static string ElementName(Dictionary<string, XamlElementRecord> elements, string id) =>
        elements.TryGetValue(id, out var element) ? element.Name ?? element.Type : id;

    private static string Mark(Confidence confidence) => confidence == Confidence.Verified ? "verified" : "inferred";

    private static byte[] Encode(string text) => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(text);
}
