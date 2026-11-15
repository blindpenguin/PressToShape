using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PressToShape.CodeIndex.Chain;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Output.Avalonia;

/// <summary>Resource dictionaries and style files, the include graph, and the keys that are used but not defined or defined but never used.</summary>
internal static class StylesPage
{
    public static string Render(IndexAnalysis analysis)
    {
        var view = analysis.View;
        var text = new StringBuilder("# Styles and resources\n\n");
        text.Append("Only XAML files of this solution are read. Keys that the framework or a theme defines show up as not defined here, and keys that override a framework key show up as never referenced, because the framework uses them without any XAML naming them.\n");

        var files = view.XamlFiles.Where(f => f.Resources.Count > 0 || f.Styles.Count > 0 || Includes(view, f).Count > 0).OrderBy(f => f.Id, StringComparer.Ordinal).ToList();
        text.Append("\n## Files\n\n");
        if (files.Count == 0)
        {
            text.Append("none found\n");
        }

        foreach (var file in files)
        {
            var variants = file.Resources.Select(r => r.Variant).Where(v => v is not null).Distinct().OrderBy(v => v, StringComparer.Ordinal).ToList();
            text.Append($"### {file.File}\n\n- {file.Resources.Count} resources, {file.Styles.Count} styles");
            text.Append(variants.Count == 0 ? string.Empty : ", variants " + string.Join(", ", variants)).Append('\n');
            foreach (var include in Includes(view, file))
            {
                text.Append("- includes `").Append(include.Path).Append("` (").Append(include.Reason).Append(")\n");
            }

            foreach (var style in file.Styles.OrderBy(s => s.Line))
            {
                text.Append("- style `").Append(style.Selector).Append("` at line ").Append(style.Line).Append(", ").Append(style.Setters.Count).Append(" setters\n");
            }

            text.Append('\n');
        }

        AppendGraph(text, analysis);
        AppendKeys(text, view);
        return text.ToString();
    }

    private sealed record Include(string Path, string? Reason);

    private static List<Include> Includes(IIndexView view, XamlFileRecord file) =>
        view.Relationships
            .Where(e => e.Type == RelationshipType.Includes && e.FromId == file.Id)
            .Select(e => new Include(e.ToId.StartsWith("X:", StringComparison.Ordinal) ? e.ToId[2..] : e.ToId, e.Reason))
            .OrderBy(i => i.Path, StringComparer.Ordinal)
            .ToList();

    private static void AppendGraph(StringBuilder text, IndexAnalysis analysis)
    {
        var view = analysis.View;
        text.Append("## Include graph from the application file\n\n");
        var roots = view.XamlFiles.Where(f => f.Root == "Application").OrderBy(f => f.Id, StringComparer.Ordinal).ToList();
        if (roots.Count == 0)
        {
            text.Append("none found: no application XAML file\n\n");
            return;
        }

        foreach (var root in roots)
        {
            text.Append("- ").Append(root.File).Append('\n');
            Walk(text, view, root, 1, [root.Id]);
        }

        text.Append('\n');
    }

    private static void Walk(StringBuilder text, IIndexView view, XamlFileRecord file, int level, HashSet<string> visited)
    {
        foreach (var include in Includes(view, file))
        {
            var id = "X:" + include.Path;
            var seen = !visited.Add(id);
            text.Append(new string(' ', level * 2)).Append("- ").Append(include.Path).Append(seen ? " (already listed)" : string.Empty).Append('\n');
            if (!seen && view.XamlFileOf(id) is { } next)
            {
                Walk(text, view, next, level + 1, visited);
            }
        }
    }

    private static void AppendKeys(StringBuilder text, IIndexView view)
    {
        var refs = view.XamlFiles.SelectMany(f => f.ResourceRefs).ToList();
        text.Append("## Keys referenced but not defined\n\n");
        var missing = refs.Where(r => r.TargetIds.Count == 0).GroupBy(r => r.Key, StringComparer.Ordinal).OrderBy(g => g.Key, StringComparer.Ordinal).ToList();
        text.Append(missing.Count == 0 ? "none found\n" : string.Concat(missing.Select(g => $"- `{g.Key}`, {g.Count()} uses\n")));

        var used = refs.SelectMany(r => r.TargetIds).ToHashSet(StringComparer.Ordinal);
        var unused = view.XamlFiles
            .SelectMany(f => f.Resources.Select(r => (File: f.File, Resource: r)))
            .Where(x => !used.Contains(x.Resource.Id))
            .OrderBy(x => x.File, StringComparer.Ordinal)
            .ThenBy(x => x.Resource.Line)
            .ToList();
        text.Append("\n## Keys defined but never referenced\n\n");
        text.Append(unused.Count == 0 ? "none found\n" : string.Concat(unused.Select(x =>
            $"- `{x.Resource.Key}` ({x.Resource.Type}{(x.Resource.Variant is null ? string.Empty : ", " + x.Resource.Variant)}) at {x.File}:{x.Resource.Line}\n")));
    }
}
