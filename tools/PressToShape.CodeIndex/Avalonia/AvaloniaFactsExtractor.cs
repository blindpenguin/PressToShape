using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Avalonia.Model;
using PressToShape.CodeIndex.Extraction;

namespace PressToShape.CodeIndex.Avalonia;

/// <summary>Reads the Avalonia facts of one project from its compilation: custom controls with their Avalonia properties and routed events, converters, and the application class.</summary>
internal sealed class AvaloniaFactsExtractor
{
    private const string ControlType = "Avalonia.Controls.Control";
    private const string ApplicationType = "Avalonia.Application";
    private const string ConverterNamespace = "Avalonia.Data.Converters";

    private readonly string _repoRoot;

    public AvaloniaFactsExtractor(string repoRoot)
    {
        ArgumentNullException.ThrowIfNull(repoRoot);
        _repoRoot = repoRoot;
    }

    /// <param name="project">The project and its compilation.</param>
    /// <param name="knownIds">The ids of the symbols of the whole solution.</param>
    /// <param name="xamlClassIds">The ids of classes that have a XAML file. They are views, not custom controls.</param>
    public AvaloniaFacts Extract(ProjectCompilation project, IReadOnlySet<string> knownIds, IReadOnlySet<string> xamlClassIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(knownIds);
        ArgumentNullException.ThrowIfNull(xamlClassIds);

        var types = SourceTypes.In(project.Compilation.Assembly.GlobalNamespace)
            .Where(t => t.TypeKind == TypeKind.Class && !t.IsImplicitlyDeclared && t.GetDocumentationCommentId() is { } id && knownIds.Contains(id))
            .ToList();

        var controls = new List<ControlFact>();
        foreach (var type in types)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsCustomControl(type, xamlClassIds))
            {
                controls.Add(ToControl(type, project));
            }
        }

        var converters = types.Where(IsConverter).Select(t => ToConverter(t, project)).OrderBy(c => c.Id, StringComparer.Ordinal).ToList();
        var applications = types
            .Where(t => t.BaseType is { } baseType && SymbolFormats.DerivesFrom(baseType, ApplicationType))
            .Select(t => ToApplication(t, project, knownIds))
            .OrderBy(a => a.Id, StringComparer.Ordinal)
            .ToList();

        return new AvaloniaFacts(controls.OrderBy(c => c.Id, StringComparer.Ordinal).ToList(), converters, applications);
    }

    private static bool IsCustomControl(INamedTypeSymbol type, IReadOnlySet<string> xamlClassIds) =>
        type.BaseType is { } baseType
        && SymbolFormats.DerivesFrom(baseType, ControlType)
        && !xamlClassIds.Contains(type.GetDocumentationCommentId()!);

    private static bool IsConverter(INamedTypeSymbol type) => ConverterInterfaces(type).Count > 0;

    private static List<string> ConverterInterfaces(INamedTypeSymbol type) =>
        type.AllInterfaces
            .Where(i => i.ContainingNamespace.ToDisplayString() == ConverterNamespace && i.Name is "IValueConverter" or "IMultiValueConverter")
            .Select(i => i.Name)
            .Distinct()
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

    private ConverterFact ToConverter(INamedTypeSymbol type, ProjectCompilation project)
    {
        var (file, line) = Location(type, project);
        return new ConverterFact(type.GetDocumentationCommentId()!, type.Name, file, line, DocCommentReader.Read(type, project.Compilation).Summary, ConverterInterfaces(type));
    }

    private ApplicationFact ToApplication(INamedTypeSymbol type, ProjectCompilation project, IReadOnlySet<string> knownIds)
    {
        var (file, _) = Location(type, project);
        return new ApplicationFact(type.GetDocumentationCommentId()!, type.Name, file, ApplicationFactsReader.Overrides(type, project, knownIds));
    }

    private (string File, int Line) Location(INamedTypeSymbol type, ProjectCompilation project)
    {
        var reference = type.DeclaringSyntaxReferences.OrderBy(r => r.SyntaxTree.FilePath, StringComparer.Ordinal).ThenBy(r => r.Span.Start).First();
        return (SourcePaths.Of(project, reference.SyntaxTree, _repoRoot), reference.SyntaxTree.GetLineSpan(reference.Span).StartLinePosition.Line + 1);
    }

    private ControlFact ToControl(INamedTypeSymbol type, ProjectCompilation project)
    {
        var reference = type.DeclaringSyntaxReferences.OrderBy(r => r.SyntaxTree.FilePath, StringComparer.Ordinal).ThenBy(r => r.Span.Start).First();
        return new ControlFact(
            type.GetDocumentationCommentId()!,
            type.Name,
            type.BaseType?.ToDisplayString(SymbolFormats.TypeName),
            SourcePaths.Of(project, reference.SyntaxTree, _repoRoot),
            reference.SyntaxTree.GetLineSpan(reference.Span).StartLinePosition.Line + 1,
            DocCommentReader.Read(type, project.Compilation).Summary,
            AvaloniaPropertyReader.Properties(type, project.Compilation),
            AvaloniaPropertyReader.Events(type, project.Compilation));
    }
}
