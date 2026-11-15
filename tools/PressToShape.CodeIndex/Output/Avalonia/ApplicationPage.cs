using System;
using System.Linq;
using System.Text;
using PressToShape.CodeIndex.Avalonia.Model;
using PressToShape.CodeIndex.Chain;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Output.Avalonia;

/// <summary>The application class and what its lifecycle methods create, the view locator convention, explicit data templates and content hosts. It claims nothing about navigation.</summary>
internal static class ApplicationPage
{
    public static string Render(AvaloniaFacts facts, IndexAnalysis analysis)
    {
        var view = analysis.View;
        var text = new StringBuilder("# Application\n\n");

        text.Append("## Application class\n\n");
        if (facts.Applications.Count == 0)
        {
            text.Append("none found\n");
        }

        foreach (var app in facts.Applications.OrderBy(a => a.Id, StringComparer.Ordinal))
        {
            text.Append($"### {app.Name}\n\n`{app.Id}` in {app.File}.\n\n");
            foreach (var method in app.Overrides.OrderBy(m => m.Line))
            {
                text.Append($"- `{method.Name}` at line {method.Line}: creates {Created(method.Created)}\n");
                foreach (var branch in method.Branches)
                {
                    text.Append($"  - when the lifetime is `{branch.LifetimeType}` (line {branch.Line}): creates {Created(branch.Created)}\n");
                }
            }

            text.Append('\n');
        }

        text.Append("## View locator convention\n\n");
        var inferred = view.Relationships
            .Where(e => e.Type == RelationshipType.UsesViewModel && e.Confidence == Confidence.Inferred && e.Reason is not null && e.Reason.StartsWith("ViewLocator (", StringComparison.Ordinal))
            .ToList();
        var convention = inferred.Select(e => ConventionOf(e.Reason!)).Distinct(StringComparer.Ordinal).OrderBy(r => r, StringComparer.Ordinal).ToList();
        text.Append(convention.Count == 0 ? "none recognized: no inferred view links were made from a name convention\n" : string.Concat(convention.Select(c => $"- {c}\n")));

        text.Append("\n## Explicit data templates\n\n");
        var templates = view.XamlFiles
            .SelectMany(f => f.Templates.Where(t => t.TemplateType == "DataTemplate" && t.Target is not null).Select(t => (File: f.File, Template: t)))
            .OrderBy(x => x.File, StringComparer.Ordinal)
            .ThenBy(x => x.Template.Line)
            .ToList();
        text.Append(templates.Count == 0 ? "none found\n" : string.Concat(templates.Select(x => $"- `{x.Template.Target}` at {x.File}:{x.Template.Line}\n")));

        text.Append("\n## Content hosts\n\n");
        text.Append("XAML places that show a view model through a content control, so the view is chosen at run time. These links are `inferred`.\n\n");
        var hosts = inferred.Where(e => e.File is not null).OrderBy(e => e.File, StringComparer.Ordinal).ThenBy(e => e.Line).ThenBy(e => e.FromId, StringComparer.Ordinal).ToList();
        text.Append(hosts.Count == 0 ? "none found\n" : string.Concat(hosts.Select(e =>
            $"- {e.File}:{e.Line} shows `{analysis.NameOf(e.ToId)}` as `{analysis.NameOf(e.FromId)}` (inferred)\n")));

        text.Append("\n## Navigation\n\n");
        text.Append("no navigation service found: the index has no data about navigation, so nothing is claimed about how views are switched.\n");
        return text.ToString();
    }

    /// <summary>The part of an inferred link reason that states the convention, without the example of the one link.</summary>
    private static string ConventionOf(string reason)
    {
        var cut = reason.IndexOf(", so ", StringComparison.Ordinal);
        return cut > 0 ? reason[..cut] : reason;
    }

    private static string Created(System.Collections.Generic.IReadOnlyList<CreatedFact> created) =>
        created.Count == 0 ? "nothing from this solution" : string.Join(", ", created.Select(c => $"`{c.TypeName}` (line {c.Line})"));
}
