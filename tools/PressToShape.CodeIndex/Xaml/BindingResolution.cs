using Microsoft.CodeAnalysis;

namespace PressToShape.CodeIndex.Xaml;

internal enum BindingStatus
{
    Verified,
    Unresolved,
}

/// <summary>
/// The result of resolving a binding path. <c>PropertyId</c> is the id of the last property or field (null for a path that is the data context itself).
/// <c>TargetTypeId</c> is the id of its type when that type is declared in the solution. <c>TargetType</c> is the symbol, for further steps.
/// An unresolved result has no ids and says why. <c>SegmentNotFound</c> is true when a path segment definitely does not exist on its type
/// (as opposed to a form that is merely not modelled), which is the case that a compiled binding reports as a build error.
/// </summary>
internal sealed record BindingResolution(
    BindingStatus Status,
    string? PropertyId,
    string? TargetTypeId,
    bool IsCommand,
    string? Reason,
    ITypeSymbol? TargetType,
    bool SegmentNotFound = false)
{
    public static BindingResolution Unresolved(string reason) => new(BindingStatus.Unresolved, null, null, false, reason, null);
}
