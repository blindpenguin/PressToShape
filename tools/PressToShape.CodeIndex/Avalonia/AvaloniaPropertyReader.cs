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
/// Reads the Avalonia properties and routed events that a control declares as static fields. Types are recognized by their full names and the registration call is
/// read from the operation tree, so names, default values and routing strategies are the ones in the code. Nothing is taken from the field name unless the call has no constant name.
/// </summary>
internal static class AvaloniaPropertyReader
{
    private const string AvaloniaNamespace = "Avalonia";
    private const string InteractivityNamespace = "Avalonia.Interactivity";

    public static IReadOnlyList<AvaloniaPropertyFact> Properties(INamedTypeSymbol control, Compilation compilation)
    {
        var facts = new List<AvaloniaPropertyFact>();
        foreach (var field in control.GetMembers().OfType<IFieldSymbol>().Where(f => f.IsStatic && f.Type is INamedTypeSymbol))
        {
            var type = (INamedTypeSymbol)field.Type;
            var propertyType = PropertyTypeOf(type);
            if (propertyType is null || field.GetDocumentationCommentId() is not { } fieldId)
            {
                continue;
            }

            var registration = RegistrationOf(field, compilation);
            var name = ConstantName(registration) ?? WithoutSuffix(field.Name, "Property");
            var valueType = propertyType == "Direct" ? type.TypeArguments.LastOrDefault() : type.TypeArguments.FirstOrDefault();
            facts.Add(new AvaloniaPropertyFact(
                fieldId,
                name,
                propertyType,
                valueType?.ToDisplayString(SymbolFormats.TypeName) ?? "?",
                ArgumentText(registration, "defaultValue"),
                HasWrapper(control, name, propertyType),
                LineOf(field)));
        }

        return facts.OrderBy(f => f.Name, StringComparer.Ordinal).ThenBy(f => f.FieldId, StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyList<RoutedEventFact> Events(INamedTypeSymbol control, Compilation compilation)
    {
        var facts = new List<RoutedEventFact>();
        foreach (var field in control.GetMembers().OfType<IFieldSymbol>().Where(f => f.IsStatic && f.Type is INamedTypeSymbol))
        {
            var type = (INamedTypeSymbol)field.Type;
            if (type.OriginalDefinition.Name != "RoutedEvent" || type.ContainingNamespace.ToDisplayString() != InteractivityNamespace
                || field.GetDocumentationCommentId() is not { } fieldId)
            {
                continue;
            }

            var registration = RegistrationOf(field, compilation);
            facts.Add(new RoutedEventFact(
                fieldId,
                ConstantName(registration) ?? WithoutSuffix(field.Name, "Event"),
                type.TypeArguments.FirstOrDefault()?.ToDisplayString(SymbolFormats.TypeName),
                ArgumentText(registration, "routingStrategies"),
                LineOf(field)));
        }

        return facts.OrderBy(f => f.Name, StringComparer.Ordinal).ThenBy(f => f.FieldId, StringComparer.Ordinal).ToList();
    }

    private static string? PropertyTypeOf(INamedTypeSymbol type)
    {
        if (type.ContainingNamespace.ToDisplayString() != AvaloniaNamespace)
        {
            return null;
        }

        return type.OriginalDefinition.Name switch
        {
            "StyledProperty" => "Styled",
            "AttachedProperty" => "Attached",
            "DirectProperty" => "Direct",
            _ => null,
        };
    }

    /// <summary>The invocation of <c>Register</c>, <c>RegisterAttached</c>, <c>RegisterDirect</c> or <c>AddOwner</c> in the field initializer, or null.</summary>
    private static IInvocationOperation? RegistrationOf(IFieldSymbol field, Compilation compilation)
    {
        var declarator = field.DeclaringSyntaxReferences.Select(r => r.GetSyntax()).OfType<VariableDeclaratorSyntax>().FirstOrDefault(d => d.Initializer is not null);
        if (declarator is null)
        {
            return null;
        }

        var model = compilation.GetSemanticModel(declarator.SyntaxTree);
        return model.GetOperation(declarator.Initializer!.Value) is { } operation
            ? operation.DescendantsAndSelf().OfType<IInvocationOperation>().FirstOrDefault(i => i.TargetMethod.Name.StartsWith("Register", StringComparison.Ordinal) || i.TargetMethod.Name == "AddOwner")
            : null;
    }

    private static string? ConstantName(IInvocationOperation? registration) =>
        registration is { Arguments.Length: > 0 } && registration.Arguments[0].Value.ConstantValue is { HasValue: true, Value: string name } && name.Length > 0 ? name : null;

    private static string? ArgumentText(IInvocationOperation? registration, string parameterName)
    {
        var argument = registration?.Arguments.FirstOrDefault(a => a.Parameter?.Name == parameterName && !a.IsImplicit);
        return argument?.Value.Syntax.ToString();
    }

    private static bool HasWrapper(INamedTypeSymbol control, string name, string propertyType) => propertyType == "Attached"
        ? control.GetMembers("Get" + name).OfType<IMethodSymbol>().Any(m => m.IsStatic)
        : control.GetMembers(name).OfType<IPropertySymbol>().Any(p => !p.IsStatic);

    private static string WithoutSuffix(string name, string suffix) =>
        name.EndsWith(suffix, StringComparison.Ordinal) && name.Length > suffix.Length ? name[..^suffix.Length] : name;

    private static int LineOf(ISymbol symbol) =>
        symbol.DeclaringSyntaxReferences.FirstOrDefault() is { } reference ? reference.SyntaxTree.GetLineSpan(reference.Span).StartLinePosition.Line + 1 : 0;
}
