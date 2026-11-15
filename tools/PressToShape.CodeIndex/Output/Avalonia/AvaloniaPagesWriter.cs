using System;
using System.Collections.Generic;
using PressToShape.CodeIndex.Avalonia.Model;
using PressToShape.CodeIndex.Chain;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Output.Avalonia;

/// <summary>Writes the Avalonia pages under <c>markdown/avalonia/</c>. They are rendered only from the index data and its facts, through the same analysis as the <c>chain</c> command.</summary>
internal static class AvaloniaPagesWriter
{
    public const string Folder = "markdown/avalonia/";

    /// <summary>The page file names and their README titles, in README order.</summary>
    public static IReadOnlyList<(string File, string Title)> Pages { get; } =
    [
        ("views.md", "Views and view models"),
        ("controls.md", "Custom controls"),
        ("converters.md", "Value converters"),
        ("styles-and-resources.md", "Styles and resources"),
        ("services.md", "Services"),
        ("application.md", "Application"),
    ];

    public static void Write(IndexData data, IIndexFileSink sink)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(sink);

        var facts = data.Avalonia ?? AvaloniaFacts.Empty;
        var analysis = new IndexAnalysis(new ModelIndexView(data));
        sink.Write(Folder + "views.md", PageText.Encode(ViewsPage.Render(analysis)));
        sink.Write(Folder + "controls.md", PageText.Encode(ControlsPage.Render(facts)));
        sink.Write(Folder + "converters.md", PageText.Encode(ConvertersPage.Render(facts, analysis)));
        sink.Write(Folder + "styles-and-resources.md", PageText.Encode(StylesPage.Render(analysis)));
        sink.Write(Folder + "services.md", PageText.Encode(ServicesPage.Render(analysis)));
        sink.Write(Folder + "application.md", PageText.Encode(ApplicationPage.Render(facts, analysis)));
    }
}
