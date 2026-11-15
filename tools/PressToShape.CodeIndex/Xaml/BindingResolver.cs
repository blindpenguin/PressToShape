using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Extraction;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>
/// Resolves binding paths against C# types, property by property. A path is verified only when every segment is a property or field that Roslyn finds.
/// Indexers, casts, attached properties and stream operators are not modelled and give an unresolved result with a reason.
/// </summary>
internal sealed class BindingResolver
{
    private const string CommandInterface = "System.Windows.Input.ICommand";

    private readonly IReadOnlySet<string> _knownIds;

    public BindingResolver(IReadOnlySet<string> knownIds)
    {
        ArgumentNullException.ThrowIfNull(knownIds);
        _knownIds = knownIds;
    }

    /// <summary>Resolves a path against the data type of the binding (<c>x:DataType</c>).</summary>
    public BindingResolution Resolve(ITypeSymbol? dataType, string? path)
    {
        var parsed = BindingPathParser.Parse(path);
        if (parsed.Source != BindingSource.DataContext)
        {
            return BindingResolution.Unresolved(parsed.Source switch
            {
                BindingSource.Self => "the source $self has no known type here",
                BindingSource.Parent => $"the source $parent[{parsed.SourceText}] has no known type here",
                _ => $"the source #{parsed.SourceText} has no known type here",
            });
        }

        return dataType is null ? BindingResolution.Unresolved("no x:DataType") : ResolveSegments(dataType, parsed.Segments, parsed.Unsupported);
    }

    /// <summary>Resolves the segments of a path from a known start type, for example the type of an <c>ElementName</c> or <c>RelativeSource</c>.</summary>
    public BindingResolution ResolveSegments(ITypeSymbol start, IReadOnlyList<string> segments, string? unsupported = null)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(segments);

        var current = start;
        ISymbol? last = null;
        var walked = new List<string>();
        foreach (var segment in segments)
        {
            var member = FindMember(current, segment);
            if (member is null)
            {
                return new BindingResolution(BindingStatus.Unresolved, null, null, false, NotFound(current, segment), null, SegmentNotFound: true);
            }

            last = member;
            walked.Add(segment);
            current = Unwrap(MemberType(member));
        }

        if (unsupported is not null)
        {
            var where = walked.Count == 0 ? "at the start" : $"after '{string.Join('.', walked)}'";
            return BindingResolution.Unresolved($"{where} the path has {unsupported}, which is not modelled");
        }

        if (last is null)
        {
            return new BindingResolution(BindingStatus.Verified, null, TypeId(start), false, null, start);
        }

        return new BindingResolution(
            BindingStatus.Verified, last.OriginalDefinition.GetDocumentationCommentId(), TypeId(current), IsCommand(MemberType(last)), null, current);
    }

    private static ISymbol? FindMember(ITypeSymbol type, string name)
    {
        var candidates = new List<ITypeSymbol>();
        for (var current = type; current is not null; current = current.BaseType)
        {
            candidates.Add(current);
        }

        if (type.TypeKind == TypeKind.Interface)
        {
            candidates.AddRange(type.AllInterfaces);
        }

        foreach (var candidate in candidates)
        {
            var member = candidate.GetMembers(name).FirstOrDefault(m => m is IPropertySymbol { IsIndexer: false } or IFieldSymbol && m.DeclaredAccessibility != Accessibility.Private && !m.IsImplicitlyDeclared);
            if (member is not null)
            {
                return member;
            }
        }

        return null;
    }

    private static string NotFound(ITypeSymbol type, string segment)
    {
        var typeName = type.ToDisplayString(SymbolFormats.Qualified);
        var isMethod = type.GetMembers(segment).Any(m => m is IMethodSymbol);
        return isMethod
            ? $"'{segment}' on '{typeName}' is a method, and method bindings are not modelled"
            : $"path segment '{segment}' not found on '{typeName}'";
    }

    private static ITypeSymbol MemberType(ISymbol member) => member switch
    {
        IPropertySymbol property => property.Type,
        IFieldSymbol field => field.Type,
        _ => throw new ArgumentException("A property or a field was expected.", nameof(member)),
    };

    /// <summary>Nullable value types continue with the underlying type.</summary>
    private static ITypeSymbol Unwrap(ITypeSymbol type) =>
        type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable ? nullable.TypeArguments[0] : type;

    private static bool IsCommand(ITypeSymbol type) =>
        type.ToDisplayString() == CommandInterface || type.AllInterfaces.Any(i => i.ToDisplayString() == CommandInterface);

    private string? TypeId(ITypeSymbol type)
    {
        var id = type.OriginalDefinition.GetDocumentationCommentId();
        return id is not null && _knownIds.Contains(id) ? id : null;
    }
}
