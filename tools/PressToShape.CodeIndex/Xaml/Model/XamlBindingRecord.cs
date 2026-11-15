using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Xaml.Model;

/// <summary>A binding on an element property. <c>TargetId</c> is set only when the path was resolved against the data type (verified).</summary>
internal sealed record XamlBindingRecord(
    string Id,
    string ElementId,
    string Property,
    string BindingType,
    string? Path,
    string? Converter,
    string? Mode,
    string? Source,
    string? TargetId,
    string? TargetTypeId,
    bool IsCommand,
    Confidence Confidence,
    string? Reason,
    int Line);
