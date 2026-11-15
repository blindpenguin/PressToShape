using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Relationships;

/// <summary>Finds references, calls and DI registrations by walking the operation trees of member bodies and initializers. Only statically resolved targets are recorded.</summary>
internal sealed class OperationRelationshipWalker
{
    private static readonly string[] RegistrationPrefixes = ["AddSingleton", "AddScoped", "AddTransient", "AddKeyedSingleton", "AddKeyedScoped", "AddKeyedTransient"];

    private readonly IReadOnlySet<string> _knownIds;
    private readonly string _repoRoot;

    public OperationRelationshipWalker(IReadOnlySet<string> knownIds, string repoRoot)
    {
        _knownIds = knownIds;
        _repoRoot = repoRoot;
    }

    public void Walk(ProjectCompilation project, ISet<RelationshipRecord> edges, CancellationToken cancellationToken)
    {
        foreach (var tree in project.Compilation.SyntaxTrees)
        {
            var model = project.Compilation.GetSemanticModel(tree);
            foreach (var node in tree.GetRoot(cancellationToken).DescendantNodes())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var context = new Context(project, tree, model, edges);
                VisitMember(node, context, cancellationToken);
            }
        }
    }

    private void VisitMember(SyntaxNode node, Context context, CancellationToken ct)
    {
        switch (node)
        {
            case BaseMethodDeclarationSyntax method:
                VisitBodies(OwnerOf(context.Model.GetDeclaredSymbol(method, ct)), node, context, ct, true,
                    method.Body, method.ExpressionBody?.Expression, (method as ConstructorDeclarationSyntax)?.Initializer);
                break;
            case PropertyDeclarationSyntax property:
                VisitBodies(OwnerOf(context.Model.GetDeclaredSymbol(property, ct)), node, context, ct, true,
                    property.ExpressionBody?.Expression, property.Initializer?.Value);
                break;
            case IndexerDeclarationSyntax indexer:
                VisitBodies(OwnerOf(context.Model.GetDeclaredSymbol(indexer, ct)), node, context, ct, true, indexer.ExpressionBody?.Expression);
                break;
            case AccessorDeclarationSyntax accessor:
                VisitBodies(OwnerOf(context.Model.GetDeclaredSymbol(accessor, ct)), node, context, ct, false, accessor.Body, accessor.ExpressionBody?.Expression);
                break;
            case VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax or EventFieldDeclarationSyntax } declarator:
                VisitBodies(OwnerOf(context.Model.GetDeclaredSymbol(declarator, ct)), node, context, ct, true, declarator.Initializer?.Value);
                break;
        }
    }

    private void VisitBodies(ISymbol? owner, SyntaxNode member, Context context, CancellationToken ct, bool signature, params SyntaxNode?[] roots)
    {
        var fromId = owner?.OriginalDefinition.GetDocumentationCommentId();
        if (owner is null || fromId is null || !_knownIds.Contains(fromId))
        {
            return;
        }

        if (signature)
        {
            AddSignatureTypes(owner, fromId, member, context);
        }

        foreach (var root in roots.Where(r => r is not null))
        {
            ct.ThrowIfCancellationRequested();
            if (context.Model.GetOperation(root!, ct) is not { } operation)
            {
                continue;
            }

            foreach (var current in operation.DescendantsAndSelf())
            {
                VisitOperation(current, fromId, context);
            }
        }
    }

    private void VisitOperation(IOperation operation, string fromId, Context context)
    {
        switch (operation)
        {
            case IInvocationOperation invocation:
                AddCall(invocation.TargetMethod, fromId, operation.Syntax, context);
                AddRegistration(invocation, fromId, context);
                break;
            case IObjectCreationOperation creation:
                if (creation.Constructor is { } constructor)
                {
                    AddCall(constructor, fromId, operation.Syntax, context);
                }

                AddTypeReferences(creation.Type, fromId, operation.Syntax, context);
                break;
            case ISimpleAssignmentOperation assignment:
                AddDataContextAssignment(assignment, context);
                break;
            case IFieldReferenceOperation field:
                AddReference(field.Field, fromId, operation.Syntax, context);
                break;
            case IPropertyReferenceOperation property:
                AddReference(property.Property, fromId, operation.Syntax, context);
                break;
            case IEventReferenceOperation evt:
                AddReference(evt.Event, fromId, operation.Syntax, context);
                break;
            case IMethodReferenceOperation method:
                AddReference(method.Method, fromId, operation.Syntax, context);
                break;
            case ITypeOfOperation typeOf:
                AddTypeReferences(typeOf.TypeOperand, fromId, operation.Syntax, context);
                break;
            case IVariableDeclaratorOperation declarator:
                AddTypeReferences(declarator.Symbol.Type, fromId, operation.Syntax, context);
                break;
        }
    }

    private void AddDataContextAssignment(ISimpleAssignmentOperation assignment, Context context)
    {
        if (DataContextAssignment.Match(assignment, _knownIds) is not var (viewId, viewModelId))
        {
            return;
        }

        var (file, line) = Locate(assignment.Syntax, context);
        context.Edges.Add(new RelationshipRecord(RelationshipType.UsesViewModel, viewId, viewModelId, file, line, Confidence.Verified, "DataContext assignment"));
    }

    private void AddSignatureTypes(ISymbol owner, string fromId, SyntaxNode member, Context context)
    {
        switch (owner)
        {
            case IMethodSymbol method:
                AddTypeReferences(method.ReturnType, fromId, member, context);
                foreach (var parameter in method.Parameters)
                {
                    AddTypeReferences(parameter.Type, fromId, member, context);
                }

                break;
            case IPropertySymbol property:
                AddTypeReferences(property.Type, fromId, member, context);
                foreach (var parameter in property.Parameters)
                {
                    AddTypeReferences(parameter.Type, fromId, member, context);
                }

                break;
            case IFieldSymbol field:
                AddTypeReferences(field.Type, fromId, member, context);
                break;
            case IEventSymbol evt:
                AddTypeReferences(evt.Type, fromId, member, context);
                break;
        }
    }

    private void AddCall(IMethodSymbol target, string fromId, SyntaxNode at, Context context) =>
        AddEdge(RelationshipType.Calls, fromId, (target.ReducedFrom ?? target).OriginalDefinition, at, context, null);

    private void AddReference(ISymbol target, string fromId, SyntaxNode at, Context context) =>
        AddEdge(RelationshipType.References, fromId, (target is IMethodSymbol { ReducedFrom: { } reduced } ? reduced : target).OriginalDefinition, at, context, null);

    /// <summary>Records the type and every type argument, for example <c>List&lt;Foo&gt;</c> references <c>Foo</c>.</summary>
    private void AddTypeReferences(ITypeSymbol? type, string fromId, SyntaxNode at, Context context)
    {
        switch (type)
        {
            case IArrayTypeSymbol array:
                AddTypeReferences(array.ElementType, fromId, at, context);
                break;
            case INamedTypeSymbol named:
                AddReference(named, fromId, at, context);
                foreach (var argument in named.TypeArguments)
                {
                    AddTypeReferences(argument, fromId, at, context);
                }

                break;
        }
    }

    private void AddRegistration(IInvocationOperation invocation, string fromId, Context context)
    {
        var method = invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod;
        if (method.ContainingNamespace.ToDisplayString() != "Microsoft.Extensions.DependencyInjection"
            || !RegistrationPrefixes.Any(p => method.Name.StartsWith(p, StringComparison.Ordinal))
            || invocation.TargetMethod.TypeArguments.IsEmpty)
        {
            return;
        }

        var service = invocation.TargetMethod.TypeArguments[0].OriginalDefinition.GetDocumentationCommentId();
        var implementation = invocation.TargetMethod.TypeArguments[^1].OriginalDefinition.GetDocumentationCommentId();
        if (service is null || implementation is null || !_knownIds.Contains(service) || !_knownIds.Contains(implementation))
        {
            return;
        }

        var lifetime = method.Name.Replace("AddKeyed", string.Empty, StringComparison.Ordinal).Replace("Add", string.Empty, StringComparison.Ordinal);
        var (file, line) = Locate(invocation.Syntax, context);
        context.Edges.Add(new RelationshipRecord(RelationshipType.Registers, service, implementation, file, line, Confidence.Verified, lifetime + ", registered in " + fromId));
    }

    private void AddEdge(RelationshipType type, string fromId, ISymbol target, SyntaxNode at, Context context, string? reason)
    {
        var toId = target.GetDocumentationCommentId();
        if (toId is null || toId == fromId || !_knownIds.Contains(toId))
        {
            return;
        }

        var (file, line) = Locate(at, context);
        context.Edges.Add(new RelationshipRecord(type, fromId, toId, file, line, Confidence.Verified, reason));
    }

    private (string File, int Line) Locate(SyntaxNode node, Context context) =>
        (SourcePaths.Of(context.Project, context.Tree, _repoRoot), context.Tree.GetLineSpan(node.Span).StartLinePosition.Line + 1);

    /// <summary>Accessors belong to their property or event, so edges from their bodies start at the property.</summary>
    private static ISymbol? OwnerOf(ISymbol? symbol) => symbol is IMethodSymbol { AssociatedSymbol: { } associated } ? associated : symbol;

    private sealed record Context(ProjectCompilation Project, SyntaxTree Tree, SemanticModel Model, ISet<RelationshipRecord> Edges);
}
