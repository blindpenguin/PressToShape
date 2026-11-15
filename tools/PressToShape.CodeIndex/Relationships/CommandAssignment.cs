using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace PressToShape.CodeIndex.Relationships;

/// <summary>
/// Recognizes <c>new RelayCommand(Method)</c> (and the async and generic forms of CommunityToolkit.Mvvm) where the argument is a method group.
/// Lambdas, parameters, locals and other expressions are not followed, because they would need guessing.
/// </summary>
internal static class CommandAssignment
{
    private static readonly HashSet<string> CommandTypes =
    [
        "CommunityToolkit.Mvvm.Input.RelayCommand",
        "CommunityToolkit.Mvvm.Input.RelayCommand<T>",
        "CommunityToolkit.Mvvm.Input.AsyncRelayCommand",
        "CommunityToolkit.Mvvm.Input.AsyncRelayCommand<T>",
    ];

    /// <summary>The id of the method of the solution that the command runs, or null.</summary>
    public static string? MethodIdOf(IOperation? value, IReadOnlySet<string> knownIds)
    {
        if (Unwrap(value) is not IObjectCreationOperation creation
            || creation.Type is null
            || !CommandTypes.Contains(creation.Type.OriginalDefinition.ToDisplayString())
            || creation.Arguments.Length == 0
            || Unwrap(creation.Arguments[0].Value) is not IDelegateCreationOperation { Target: IMethodReferenceOperation reference })
        {
            return null;
        }

        var id = reference.Method.OriginalDefinition.GetDocumentationCommentId();
        return id is not null && knownIds.Contains(id) ? id : null;
    }

    /// <summary>The property or field that an assignment writes to, or null for any other target.</summary>
    public static ISymbol? TargetMember(IOperation? target) => Unwrap(target) switch
    {
        IPropertyReferenceOperation property => property.Property,
        IFieldReferenceOperation field => field.Field,
        _ => null,
    };

    private static IOperation? Unwrap(IOperation? operation)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        return operation;
    }
}
