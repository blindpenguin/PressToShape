using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PressToShape.CodeIndex.Extraction;

/// <summary>Enumerates the types declared in source, including nested types, in a stable order.</summary>
internal static class SourceTypes
{
    public static IEnumerable<INamedTypeSymbol> In(INamespaceSymbol ns)
    {
        foreach (var type in ns.GetTypeMembers().SelectMany(WithNested))
        {
            yield return type;
        }

        foreach (var child in ns.GetNamespaceMembers().SelectMany(In))
        {
            yield return child;
        }
    }

    private static IEnumerable<INamedTypeSymbol> WithNested(INamedTypeSymbol type)
    {
        yield return type;
        foreach (var nested in type.GetTypeMembers().SelectMany(WithNested))
        {
            yield return nested;
        }
    }
}
