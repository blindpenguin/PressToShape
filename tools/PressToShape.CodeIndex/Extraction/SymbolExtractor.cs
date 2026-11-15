using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Extraction;

/// <summary>Reads the source symbols of a compilation into index records, using the semantic model only.</summary>
internal sealed class SymbolExtractor
{
    private readonly string _repoRoot;

    public SymbolExtractor(string repoRoot)
    {
        ArgumentNullException.ThrowIfNull(repoRoot);
        _repoRoot = repoRoot;
    }

    /// <summary>Returns every declared symbol of the project, sorted by id. Partial types appear once, at their first declaration.</summary>
    public IReadOnlyList<SymbolRecord> Extract(ProjectCompilation project, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);

        var records = new List<SymbolRecord>();
        WalkNamespace(project, project.Compilation.Assembly.GlobalNamespace, records, cancellationToken);
        records.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
        return records;
    }

    private void WalkNamespace(ProjectCompilation project, INamespaceSymbol ns, List<SymbolRecord> records, CancellationToken ct)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            AddType(project, type, records, ct);
        }

        foreach (var child in ns.GetNamespaceMembers())
        {
            var before = records.Count;
            WalkNamespace(project, child, records, ct);
            if (records.Count > before)
            {
                AddSymbol(project, child, SymbolType.Namespace, records);
            }
        }
    }

    private void AddType(ProjectCompilation project, INamedTypeSymbol type, List<SymbolRecord> records, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (type.IsImplicitlyDeclared || type.IsAnonymousType)
        {
            return;
        }

        AddSymbol(project, type, TypeSymbolType(type), records);
        foreach (var member in type.GetMembers())
        {
            if (member is INamedTypeSymbol nested)
            {
                AddType(project, nested, records, ct);
            }
            else if (!member.IsImplicitlyDeclared && MemberSymbolType(member) is { } memberType)
            {
                AddSymbol(project, member, memberType, records);
            }
        }
    }

    private static SymbolType TypeSymbolType(INamedTypeSymbol type) => type.TypeKind switch
    {
        TypeKind.Interface => SymbolType.Interface,
        TypeKind.Struct => type.IsRecord ? SymbolType.Record : SymbolType.Struct,
        TypeKind.Enum => SymbolType.Enum,
        TypeKind.Delegate => SymbolType.Delegate,
        _ => type.IsRecord ? SymbolType.Record : SymbolType.Class,
    };

    private static SymbolType? MemberSymbolType(ISymbol member) => member switch
    {
        IMethodSymbol { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor } => SymbolType.Constructor,
        IMethodSymbol { MethodKind: MethodKind.Ordinary or MethodKind.UserDefinedOperator or MethodKind.Conversion } => SymbolType.Method,
        IPropertySymbol => SymbolType.Property,
        IFieldSymbol => SymbolType.Field,
        IEventSymbol => SymbolType.Event,
        _ => null,
    };

    private void AddSymbol(ProjectCompilation project, ISymbol symbol, SymbolType symbolType, List<SymbolRecord> records)
    {
        var id = symbol.GetDocumentationCommentId();
        // A hand-written declaration wins over generated parts, so partial types point at real source.
        var reference = symbol.DeclaringSyntaxReferences
            .OrderBy(r => project.IsGenerated(r.SyntaxTree))
            .ThenBy(r => r.SyntaxTree.FilePath, StringComparer.Ordinal)
            .ThenBy(r => r.Span.Start)
            .FirstOrDefault();
        if (id is null || reference is null)
        {
            return;
        }

        var isGenerated = project.IsGenerated(reference.SyntaxTree);

        var span = reference.SyntaxTree.GetLineSpan(reference.Span);
        var parameters = ParametersOf(symbol);
        var namedType = symbol as INamedTypeSymbol;

        records.Add(new SymbolRecord(
            id,
            NameOf(symbol),
            symbol.ToDisplayString(SymbolFormats.Qualified),
            symbolType,
            SymbolFormats.Accessibility(symbol),
            SymbolFormats.Modifiers(symbol),
            ContainingIdOf(symbol),
            symbol is INamespaceSymbol self ? self.ToDisplayString() : symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            project.Name,
            SourcePaths.Of(project, reference.SyntaxTree, _repoRoot),
            span.StartLinePosition.Line + 1,
            span.EndLinePosition.Line + 1,
            SignatureOf(symbol, symbolType),
            parameters.Select(p => new ParameterRecord(p.Name, p.Type.ToDisplayString(SymbolFormats.TypeName))).ToList(),
            TypeNameOf(symbol),
            symbol.GetAttributes().Select(ToAttributeRecord).ToList(),
            namedType is null ? null : BaseTypeOf(namedType),
            namedType is null ? [] : namedType.Interfaces.Select(i => i.ToDisplayString(SymbolFormats.TypeName)).OrderBy(n => n, StringComparer.Ordinal).ToList(),
            isGenerated,
            isGenerated ? GeneratedMemberLinker.FindSource(symbol, project.IsGenerated) : null,
            DocCommentReader.Read(symbol, project.Compilation)));
    }

    private static string NameOf(ISymbol symbol) => symbol is IMethodSymbol { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor }
        ? symbol.ContainingType.Name
        : symbol.Name;

    private static string? ContainingIdOf(ISymbol symbol) => symbol switch
    {
        INamespaceSymbol ns => ns.ContainingNamespace is { IsGlobalNamespace: false } parent ? parent.GetDocumentationCommentId() : null,
        _ => symbol.ContainingType?.GetDocumentationCommentId()
             ?? (symbol.ContainingNamespace is { IsGlobalNamespace: false } ns ? ns.GetDocumentationCommentId() : null),
    };

    private static IReadOnlyList<IParameterSymbol> ParametersOf(ISymbol symbol) => symbol switch
    {
        IMethodSymbol method => method.Parameters,
        IPropertySymbol property => property.Parameters,
        INamedTypeSymbol { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke } => invoke.Parameters,
        _ => [],
    };

    private static string? TypeNameOf(ISymbol symbol) => symbol switch
    {
        IMethodSymbol { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor } => null,
        IMethodSymbol method => method.ReturnType.ToDisplayString(SymbolFormats.TypeName),
        IPropertySymbol property => property.Type.ToDisplayString(SymbolFormats.TypeName),
        IFieldSymbol field => field.Type.ToDisplayString(SymbolFormats.TypeName),
        IEventSymbol evt => evt.Type.ToDisplayString(SymbolFormats.TypeName),
        INamedTypeSymbol { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke } => invoke.ReturnType.ToDisplayString(SymbolFormats.TypeName),
        _ => null,
    };

    private static string? SignatureOf(ISymbol symbol, SymbolType symbolType) => symbolType switch
    {
        SymbolType.Namespace => null,
        SymbolType.Class or SymbolType.Interface or SymbolType.Record or SymbolType.Struct or SymbolType.Enum => null,
        _ => symbol.ToDisplayString(SymbolFormats.Signature),
    };

    private static string? BaseTypeOf(INamedTypeSymbol type) =>
        type.TypeKind is TypeKind.Class && type.BaseType is { SpecialType: not SpecialType.System_Object } baseType
            ? baseType.ToDisplayString(SymbolFormats.TypeName)
            : null;

    private static AttributeRecord ToAttributeRecord(AttributeData attribute)
    {
        var name = attribute.AttributeClass?.Name ?? "?";
        if (name.EndsWith("Attribute", StringComparison.Ordinal) && name.Length > "Attribute".Length)
        {
            name = name[..^"Attribute".Length];
        }

        return new AttributeRecord(name, SymbolFormats.AttributeArguments(attribute));
    }
}
