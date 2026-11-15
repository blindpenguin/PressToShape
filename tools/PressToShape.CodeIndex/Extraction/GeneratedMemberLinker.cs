using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PressToShape.CodeIndex.Extraction;

/// <summary>Finds the hand-written member behind a member that CommunityToolkit.Mvvm generated. Attributes are matched by full name, so the tool needs no reference to the toolkit.</summary>
internal static class GeneratedMemberLinker
{
    private const string ObservablePropertyAttribute = "CommunityToolkit.Mvvm.ComponentModel.ObservablePropertyAttribute";
    private const string RelayCommandAttribute = "CommunityToolkit.Mvvm.Input.RelayCommandAttribute";

    /// <summary>Returns the id of the field marked <c>[ObservableProperty]</c> or the method marked <c>[RelayCommand]</c> that produced the generated property, or null when none matches.</summary>
    public static string? FindSource(ISymbol generated, Func<SyntaxTree, bool> isGeneratedTree)
    {
        if (generated is not IPropertySymbol property || property.ContainingType is not { } type)
        {
            return null;
        }

        return type.GetMembers()
            .Where(m => m.DeclaringSyntaxReferences.Any(r => !isGeneratedTree(r.SyntaxTree)))
            .Where(m => m switch
            {
                IFieldSymbol field => HasAttribute(field, ObservablePropertyAttribute) && ObservablePropertyName(field.Name) == property.Name,
                IMethodSymbol method => HasAttribute(method, RelayCommandAttribute) && CommandName(method.Name) == property.Name,
                _ => false,
            })
            .Select(m => m.GetDocumentationCommentId())
            .OrderBy(id => id, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static bool HasAttribute(ISymbol symbol, string fullName) =>
        symbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == fullName);

    /// <summary>The toolkit strips a leading <c>_</c> or <c>m_</c> and capitalizes the first letter.</summary>
    private static string ObservablePropertyName(string fieldName)
    {
        var name = fieldName.StartsWith("m_", StringComparison.Ordinal) ? fieldName[2..] : fieldName.TrimStart('_');
        return name.Length == 0 ? name : char.ToUpperInvariant(name[0]) + name[1..];
    }

    /// <summary>The toolkit drops an <c>Async</c> suffix and appends <c>Command</c>.</summary>
    private static string CommandName(string methodName)
    {
        var name = methodName.EndsWith("Async", StringComparison.Ordinal) && methodName.Length > "Async".Length ? methodName[..^"Async".Length] : methodName;
        return name + "Command";
    }
}
