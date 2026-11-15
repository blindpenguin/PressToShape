using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PressToShape.CodeIndex.Avalonia.Model;
using PressToShape.CodeIndex.Extraction;

namespace PressToShape.CodeIndex.Avalonia;

/// <summary>
/// Reads the lifecycle of an application class: for <c>Initialize</c> and every override, the classes of the solution that it creates, and the creations inside
/// lifetime branches (the true part of a conditional that tests <c>ApplicationLifetime</c> against a type that implements <c>IApplicationLifetime</c>).
/// </summary>
internal static class ApplicationFactsReader
{
    private const string LifetimeInterface = "Avalonia.Controls.ApplicationLifetimes.IApplicationLifetime";

    public static IReadOnlyList<LifecycleFact> Overrides(INamedTypeSymbol application, ProjectCompilation project, IReadOnlySet<string> knownIds)
    {
        var facts = new List<LifecycleFact>();
        foreach (var method in application.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Ordinary && (m.IsOverride || m.Name == "Initialize")))
        {
            if (method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not MethodDeclarationSyntax syntax
                || project.Compilation.GetSemanticModel(syntax.SyntaxTree).GetOperation(syntax) is not { } body
                || method.GetDocumentationCommentId() is not { } id)
            {
                continue;
            }

            facts.Add(Read(id, method.Name, syntax, body, knownIds));
        }

        return facts.OrderBy(f => f.Line).ThenBy(f => f.MethodId, StringComparer.Ordinal).ToList();
    }

    private static LifecycleFact Read(string id, string name, MethodDeclarationSyntax syntax, IOperation body, IReadOnlySet<string> knownIds)
    {
        var branches = new List<LifetimeBranchFact>();
        var inBranches = new HashSet<IOperation>();
        foreach (var conditional in body.DescendantsAndSelf().OfType<IConditionalOperation>())
        {
            if (TestedLifetime(conditional.Condition) is not { } lifetime)
            {
                continue;
            }

            var inside = CreationsOf(conditional.WhenTrue, knownIds);
            inside.ForEach(c => inBranches.Add(c.Operation));
            branches.Add(new LifetimeBranchFact(lifetime.Name, LineOf(conditional.Syntax), ToFacts(inside)));
        }

        var outside = CreationsOf(body, knownIds).Where(c => !inBranches.Contains(c.Operation)).ToList();
        return new LifecycleFact(id, name, LineOf(syntax), ToFacts(outside), branches.OrderBy(b => b.Line).ToList());
    }

    private static List<(IObjectCreationOperation Operation, CreatedFact Fact)> CreationsOf(IOperation root, IReadOnlySet<string> knownIds) =>
        root.DescendantsAndSelf()
            .OfType<IObjectCreationOperation>()
            .Where(c => c.Type is INamedTypeSymbol type && type.OriginalDefinition.GetDocumentationCommentId() is { } id && knownIds.Contains(id))
            .Select(c =>
            {
                var type = (INamedTypeSymbol)c.Type!;
                return (c, new CreatedFact(type.OriginalDefinition.GetDocumentationCommentId()!, type.Name, LineOf(c.Syntax)));
            })
            .ToList();

    private static List<CreatedFact> ToFacts(List<(IObjectCreationOperation Operation, CreatedFact Fact)> creations) =>
        creations.Select(c => c.Fact).Distinct().OrderBy(c => c.Line).ThenBy(c => c.TypeId, StringComparer.Ordinal).ToList();

    private static INamedTypeSymbol? TestedLifetime(IOperation condition)
    {
        foreach (var operation in condition.DescendantsAndSelf())
        {
            var tested = operation switch
            {
                IIsPatternOperation { Pattern: IDeclarationPatternOperation declaration } => declaration.MatchedType,
                IIsPatternOperation { Pattern: ITypePatternOperation typePattern } => typePattern.MatchedType,
                IIsTypeOperation isType => isType.TypeOperand,
                _ => null,
            };

            if (tested is INamedTypeSymbol type && IsLifetime(type))
            {
                return type;
            }
        }

        return null;
    }

    private static bool IsLifetime(INamedTypeSymbol type) =>
        type.ToDisplayString() == LifetimeInterface || type.AllInterfaces.Any(i => i.ToDisplayString() == LifetimeInterface);

    private static int LineOf(SyntaxNode node) => node.SyntaxTree.GetLineSpan(node.Span).StartLinePosition.Line + 1;
}
