using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Relationships;

/// <summary>Builds the relationships of one project. Targets must be symbols of this solution, so framework and package types never appear.</summary>
internal sealed class RelationshipBuilder
{
    private readonly IReadOnlySet<string> _knownIds;
    private readonly string _repoRoot;

    /// <param name="knownIds">The ids of every indexed symbol of the whole solution.</param>
    /// <param name="repoRoot">Used to make file paths repo-relative.</param>
    public RelationshipBuilder(IReadOnlySet<string> knownIds, string repoRoot)
    {
        ArgumentNullException.ThrowIfNull(knownIds);
        ArgumentNullException.ThrowIfNull(repoRoot);
        _knownIds = knownIds;
        _repoRoot = repoRoot;
    }

    public IReadOnlyList<RelationshipRecord> Build(ProjectCompilation project, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);

        var edges = new HashSet<RelationshipRecord>();
        foreach (var type in Extraction.SourceTypes.In(project.Compilation.Assembly.GlobalNamespace))
        {
            cancellationToken.ThrowIfCancellationRequested();
            AddStructural(project, type, edges);
        }

        new OperationRelationshipWalker(_knownIds, _repoRoot).Walk(project, edges, cancellationToken);

        return edges
            .OrderBy(e => e.Type)
            .ThenBy(e => e.FromId, StringComparer.Ordinal)
            .ThenBy(e => e.ToId, StringComparer.Ordinal)
            .ThenBy(e => e.File, StringComparer.Ordinal)
            .ThenBy(e => e.Line)
            .ToList();
    }

    private void AddStructural(ProjectCompilation project, INamedTypeSymbol type, HashSet<RelationshipRecord> edges)
    {
        var fromId = type.GetDocumentationCommentId();
        if (fromId is null || type.IsImplicitlyDeclared)
        {
            return;
        }

        var (file, line) = Locate(project, type.DeclaringSyntaxReferences);

        if (type.TypeKind == TypeKind.Interface)
        {
            foreach (var parent in type.Interfaces)
            {
                AddEdge(edges, RelationshipType.Inherits, fromId, parent, file, line);
            }
        }
        else
        {
            if (type.BaseType is { SpecialType: not SpecialType.System_Object and not SpecialType.System_ValueType and not SpecialType.System_Enum } baseType)
            {
                AddEdge(edges, RelationshipType.Inherits, fromId, baseType, file, line);
            }

            foreach (var iface in type.AllInterfaces)
            {
                AddEdge(edges, RelationshipType.Implements, fromId, iface, file, line);
            }
        }

        foreach (var constructor in type.InstanceConstructors.Where(c => !c.IsImplicitlyDeclared))
        {
            var (ctorFile, ctorLine) = Locate(project, constructor.DeclaringSyntaxReferences);
            foreach (var parameter in constructor.Parameters)
            {
                if (parameter.Type is INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Interface or TypeKind.Struct } parameterType
                    && !SymbolEqualityComparer.Default.Equals(parameterType.OriginalDefinition, type))
                {
                    AddEdge(edges, RelationshipType.InjectedDependency, fromId, parameterType, ctorFile, ctorLine);
                }
            }
        }
    }

    private void AddEdge(HashSet<RelationshipRecord> edges, RelationshipType type, string fromId, ITypeSymbol target, string? file, int? line)
    {
        var toId = target.OriginalDefinition.GetDocumentationCommentId();
        if (toId is not null && _knownIds.Contains(toId))
        {
            edges.Add(new RelationshipRecord(type, fromId, toId, file, line, Confidence.Verified, null));
        }
    }

    private (string? File, int? Line) Locate(ProjectCompilation project, IEnumerable<SyntaxReference> references)
    {
        var reference = references
            .OrderBy(r => project.IsGenerated(r.SyntaxTree))
            .ThenBy(r => r.SyntaxTree.FilePath, StringComparer.Ordinal)
            .ThenBy(r => r.Span.Start)
            .FirstOrDefault();
        if (reference is null)
        {
            return (null, null);
        }

        return (SourcePaths.Of(project, reference.SyntaxTree, _repoRoot), reference.SyntaxTree.GetLineSpan(reference.Span).StartLinePosition.Line + 1);
    }
}
