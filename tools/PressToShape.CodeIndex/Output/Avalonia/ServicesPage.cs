using System;
using System.Linq;
using System.Text;
using PressToShape.CodeIndex.Chain;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Output.Avalonia;

/// <summary>Types that are injected through constructors, with implementations, users and creation sites, and the container registrations when there are any.</summary>
internal static class ServicesPage
{
    public static string Render(IndexAnalysis analysis)
    {
        var view = analysis.View;
        var text = new StringBuilder("# Services\n\n");
        text.Append("Types that other types take as constructor parameters, and where they are provided.\n");

        var injected = view.Relationships
            .Where(e => e.Type == RelationshipType.InjectedDependency && view.SymbolOf(e.ToId) is not null && !analysis.IsTestSide(e.FromId) && !analysis.IsTestSide(e.ToId))
            .GroupBy(e => e.ToId, StringComparer.Ordinal)
            .OrderBy(g => analysis.NameOf(g.Key), StringComparer.Ordinal)
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .ToList();

        text.Append("\n## Injected types\n\n");
        if (injected.Count == 0)
        {
            text.Append("none found\n");
        }

        foreach (var group in injected)
        {
            var service = analysis.ServiceOf(group.Key);
            text.Append("### ").Append(service.Name).Append(service.IsInterface ? " (interface)" : " (class)").Append("\n\n");
            text.Append('`').Append(service.TypeId).Append("`. ").Append(PageText.Summary(service.Summary)).Append("\n\n");
            text.Append("- used by: ").Append(string.Join(", ", group.Select(e => $"`{analysis.NameOf(e.FromId)}`").Distinct().OrderBy(n => n, StringComparer.Ordinal))).Append('\n');
            if (service.IsInterface)
            {
                text.Append(service.Implementations.Count == 0 ? "- implementations: none found\n" : string.Concat(service.Implementations.Select(i =>
                    $"- implemented by `{i.Name}`, created at {Sites(i.CreatedAt)}\n")));
            }
            else
            {
                text.Append("- created at ").Append(Sites(service.CreatedAt)).Append('\n');
            }

            text.Append('\n');
        }

        AppendRegistrations(text, analysis);
        return text.ToString();
    }

    private static string Sites(System.Collections.Generic.IReadOnlyList<Place> places) =>
        places.Count == 0 ? "no creation site found" : string.Join(", ", places.Select(p => $"`{p.FromName}` ({p.Location})"));

    private static void AppendRegistrations(StringBuilder text, IndexAnalysis analysis)
    {
        var registrations = analysis.View.Relationships.Where(e => e.Type == RelationshipType.Registers).OrderBy(e => e.File, StringComparer.Ordinal).ThenBy(e => e.Line).ToList();
        text.Append("## Container registrations\n\n");
        if (registrations.Count == 0)
        {
            var creators = analysis.View.Relationships
                .Where(e => e.Type == RelationshipType.InjectedDependency && !analysis.IsTestSide(e.FromId))
                .SelectMany(e => analysis.ServiceOf(e.ToId).Implementations.SelectMany(i => i.CreatedAt).Concat(analysis.ServiceOf(e.ToId).CreatedAt))
                .Select(p => p.FromName)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToList();
            text.Append("no container registrations found; services are created by hand in ");
            text.Append(creators.Count == 0 ? "no member that the index found" : string.Join(", ", creators.Select(c => $"`{c}`"))).Append(".\n");
            return;
        }

        foreach (var edge in registrations)
        {
            text.Append($"- `{analysis.NameOf(edge.FromId)}` -> `{analysis.NameOf(edge.ToId)}` ({PageText.Mark(edge.Confidence)}): {edge.Reason} ({edge.File}:{edge.Line})\n");
        }
    }
}
