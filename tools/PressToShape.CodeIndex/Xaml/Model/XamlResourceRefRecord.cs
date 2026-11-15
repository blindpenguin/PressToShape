using System.Collections.Generic;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Xaml.Model;

/// <summary>A StaticResource or DynamicResource use. <c>TargetIds</c> is empty when the key is not defined in the solution.</summary>
internal sealed record XamlResourceRefRecord(
    string Id,
    string ElementId,
    string Property,
    string Key,
    string RefType,
    IReadOnlyList<string> TargetIds,
    Confidence Confidence,
    string? Reason,
    int Line);
