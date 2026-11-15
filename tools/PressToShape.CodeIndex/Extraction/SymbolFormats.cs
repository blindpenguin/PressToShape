using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PressToShape.CodeIndex.Extraction;

/// <summary>Display formats and small helpers that turn Roslyn symbols into index text.</summary>
internal static class SymbolFormats
{
    /// <summary>Types and members as shown to a reader: no namespaces, special type names, nullable marks, parameters with defaults.</summary>
    public static SymbolDisplayFormat Signature { get; } = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance,
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeType | SymbolDisplayMemberOptions.IncludeRef,
        delegateStyle: SymbolDisplayDelegateStyle.NameAndSignature,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName
            | SymbolDisplayParameterOptions.IncludeDefaultValue | SymbolDisplayParameterOptions.IncludeParamsRefOut
            | SymbolDisplayParameterOptions.IncludeExtensionThis,
        propertyStyle: SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            | SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName);

    /// <summary>A type name as written in a signature, without namespaces.</summary>
    public static SymbolDisplayFormat TypeName { get; } = Signature.WithMemberOptions(SymbolDisplayMemberOptions.None);

    /// <summary>Fully qualified names with type parameters, without parameter lists.</summary>
    public static SymbolDisplayFormat Qualified { get; } = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public static string Accessibility(ISymbol symbol) => symbol.DeclaredAccessibility switch
    {
        Microsoft.CodeAnalysis.Accessibility.Public => "public",
        Microsoft.CodeAnalysis.Accessibility.Internal => "internal",
        Microsoft.CodeAnalysis.Accessibility.Private => "private",
        Microsoft.CodeAnalysis.Accessibility.Protected => "protected",
        Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal => "protected internal",
        Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal => "private protected",
        _ => symbol is INamespaceSymbol ? "public" : "private",
    };

    public static IReadOnlyList<string> Modifiers(ISymbol symbol)
    {
        var modifiers = new List<string>();

        // Interfaces and their members are abstract by definition, so the word would only be noise.
        var impliedAbstract = symbol is INamedTypeSymbol { TypeKind: TypeKind.Interface } || symbol.ContainingType is { TypeKind: TypeKind.Interface };
        if (symbol.IsStatic)
        {
            modifiers.Add("static");
        }

        // Roslyn reports a static class as both abstract and sealed, so those are only meaningful for non-static symbols.
        if (symbol.IsAbstract && !symbol.IsStatic && !impliedAbstract)
        {
            modifiers.Add("abstract");
        }

        if (symbol.IsSealed && !symbol.IsStatic && symbol is not INamedTypeSymbol { TypeKind: TypeKind.Enum or TypeKind.Struct or TypeKind.Delegate })
        {
            modifiers.Add("sealed");
        }

        if (symbol.IsVirtual && !impliedAbstract)
        {
            modifiers.Add("virtual");
        }

        if (symbol.IsOverride)
        {
            modifiers.Add("override");
        }

        if (symbol.IsExtern)
        {
            modifiers.Add("extern");
        }

        switch (symbol)
        {
            case IFieldSymbol field:
                AddIf(modifiers, field.IsConst, "const");
                AddIf(modifiers, field.IsReadOnly, "readonly");
                AddIf(modifiers, field.IsRequired, "required");
                break;
            case IMethodSymbol method:
                AddIf(modifiers, method.IsAsync, "async");
                AddIf(modifiers, method.IsReadOnly, "readonly");
                break;
            case IPropertySymbol property:
                AddIf(modifiers, property.IsRequired, "required");
                AddIf(modifiers, property.IsReadOnly && property.SetMethod is null && property.GetMethod is null, "readonly");
                break;
            case INamedTypeSymbol type:
                AddIf(modifiers, type.IsReadOnly && type.TypeKind == TypeKind.Struct, "readonly");
                AddIf(modifiers, IsPartial(type), "partial");
                break;
        }

        modifiers.Sort(System.StringComparer.Ordinal);
        return modifiers;
    }

    /// <summary>True when the type or one of its base types has the given full name, for example <c>Avalonia.Controls.Control</c>.</summary>
    public static bool DerivesFrom(ITypeSymbol type, string fullName)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == fullName)
            {
                return true;
            }
        }

        return false;
    }

    public static IReadOnlyList<string> AttributeArguments(AttributeData attribute) =>
        attribute.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax { ArgumentList: { } list }
            ? list.Arguments.Select(a => a.ToString()).ToList()
            : [];

    private static bool IsPartial(INamedTypeSymbol type) =>
        type.DeclaringSyntaxReferences.Any(r => r.GetSyntax() is BaseTypeDeclarationSyntax { Modifiers: var m } && m.Any(t => t.Text == "partial"));

    private static void AddIf(List<string> modifiers, bool condition, string value)
    {
        if (condition)
        {
            modifiers.Add(value);
        }
    }
}
