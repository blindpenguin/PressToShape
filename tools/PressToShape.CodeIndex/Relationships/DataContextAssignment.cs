using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using PressToShape.CodeIndex.Extraction;

namespace PressToShape.CodeIndex.Relationships;

/// <summary>Recognizes <c>view.DataContext = viewModel</c>, also inside object initializers, when the view derives from <c>Avalonia.Controls.Control</c> and the value has a static type of the solution.</summary>
internal static class DataContextAssignment
{
    private const string ControlType = "Avalonia.Controls.Control";

    /// <summary>The ids of the view class and of the static type of the assigned value, or null when the assignment is not such a link.</summary>
    public static (string ViewId, string ViewModelId)? Match(ISimpleAssignmentOperation assignment, IReadOnlySet<string> knownIds)
    {
        if (Unwrap(assignment.Target) is not IPropertyReferenceOperation { Property.Name: "DataContext" } property
            || property.Instance?.Type is not INamedTypeSymbol view
            || !SymbolFormats.DerivesFrom(view, ControlType)
            || Unwrap(assignment.Value)?.Type is not INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Interface } viewModel)
        {
            return null;
        }

        var viewId = view.OriginalDefinition.GetDocumentationCommentId();
        var viewModelId = viewModel.OriginalDefinition.GetDocumentationCommentId();
        return viewId is not null && viewModelId is not null && knownIds.Contains(viewId) && knownIds.Contains(viewModelId)
            ? (viewId, viewModelId)
            : null;
    }

    private static IOperation? Unwrap(IOperation? operation)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        return operation;
    }
}
