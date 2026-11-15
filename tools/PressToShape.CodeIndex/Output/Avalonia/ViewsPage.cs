using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PressToShape.CodeIndex.Chain;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Output.Avalonia;

/// <summary>The MVVM map: every view with its view models, evidence and counts, then the view models without a view and the views without a view model.</summary>
internal static class ViewsPage
{
    public static string Render(IndexAnalysis analysis)
    {
        var view = analysis.View;
        var text = new StringBuilder("# Views and view models\n\n");
        text.Append("Links come from `x:DataType`, DataContext bindings and assignments, and the ViewLocator convention. `inferred` links are not proven, the reason is given.\n");

        var viewIds = view.Relationships
            .Where(e => e.Type is RelationshipType.CodeBehind or RelationshipType.UsesViewModel)
            .Select(e => e.Type == RelationshipType.CodeBehind ? e.ToId : e.FromId)
            .Where(id => view.SymbolOf(id) is not null)
            .Distinct(StringComparer.Ordinal)
            .Where(id => !IsApplication(view, id))
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();

        text.Append("\n## Views\n\n");
        if (viewIds.Count == 0)
        {
            text.Append("none found\n");
        }

        foreach (var id in viewIds)
        {
            AppendView(text, analysis, id);
        }

        AppendUnused(text, analysis);
        return text.ToString();
    }

    /// <summary>The application class has a XAML file too, but it is not a view (the application page covers it).</summary>
    private static bool IsApplication(IIndexView view, string classId) =>
        view.XamlFiles.Any(f => f.ClassId == classId && f.Root == "Application");

    private static void AppendView(StringBuilder text, IndexAnalysis analysis, string viewId)
    {
        var view = analysis.View;
        var symbol = view.SymbolOf(viewId)!;
        var xaml = view.Relationships.Where(e => e.Type == RelationshipType.CodeBehind && e.ToId == viewId).Select(e => view.XamlFileOf(e.FromId)).FirstOrDefault(f => f is not null);
        text.Append("### ").Append(symbol.Name).Append("\n\n");
        text.Append("`").Append(viewId).Append("` at ").Append(symbol.File).Append(':').Append(symbol.StartLine).Append(". ");
        text.Append(xaml is null ? "No XAML file." : $"XAML: `{xaml.File}`, {xaml.Bindings.Count} bindings.").Append('\n');
        text.Append(PageText.Summary(symbol.Doc)).Append("\n\n");

        var links = analysis.ViewModelsOf(viewId);
        if (links.Count == 0)
        {
            text.Append("- no view model found\n");
        }

        foreach (var link in links)
        {
            var name = analysis.NameOf(link.ViewModelId);
            var commands = analysis.Commands(link.ViewModelId);
            var tests = analysis.TestsFor([link.ViewModelId]);
            var where = link.File is null ? string.Empty : $" ({link.File}:{link.Line})";
            text.Append($"- view model `{name}` (`{link.ViewModelId}`), {PageText.Mark(link.Confidence)}: {link.Reason}{where}; {commands.Count} commands, {tests.Count} test classes\n");
        }

        text.Append('\n');
    }

    private static void AppendUnused(StringBuilder text, IndexAnalysis analysis)
    {
        var view = analysis.View;
        var used = view.Relationships.Where(e => e.Type == RelationshipType.UsesViewModel).Select(e => e.ToId).ToHashSet(StringComparer.Ordinal);
        var bases = view.Relationships
            .Where(e => e.Type == RelationshipType.Inherits && used.Contains(e.FromId))
            .Select(e => e.ToId)
            .ToHashSet(StringComparer.Ordinal);
        var unused = view.Relationships
            .Where(e => e.Type == RelationshipType.Inherits && bases.Contains(e.ToId) && !used.Contains(e.FromId) && !bases.Contains(e.FromId))
            .Select(e => e.FromId)
            .Distinct(StringComparer.Ordinal)
            .Where(id => view.SymbolOf(id) is not null)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();

        text.Append("## View models without a view\n\n");
        text.Append("Types that derive from a base class of a linked view model but that no view links to. A view may still create them in code.\n\n");
        AppendList(text, unused.Select(id => $"`{analysis.NameOf(id)}` (`{id}`) at {Place(view.SymbolOf(id)!)}"));

        var viewsWithout = view.XamlFiles
            .Where(f => f.ClassId is not null && f.Root != "Application" && analysis.ViewModelsOf(f.ClassId).Count == 0 && !used.Contains(f.ClassId))
            .OrderBy(f => f.Id, StringComparer.Ordinal)
            .Select(f => $"`{f.File}` (`{f.ClassId}`)");
        text.Append("\n## Views without a view model\n\n");
        AppendList(text, viewsWithout);
    }

    private static string Place(SymbolRecord symbol) => $"{symbol.File}:{symbol.StartLine}";

    private static void AppendList(StringBuilder text, IEnumerable<string> items)
    {
        var list = items.ToList();
        if (list.Count == 0)
        {
            text.Append("none found\n");
        }

        foreach (var item in list)
        {
            text.Append("- ").Append(item).Append('\n');
        }
    }
}
