using System.Collections.Generic;

namespace PressToShape.CodeIndex.Model;

/// <summary>The XML documentation of a symbol as written. Undocumented symbols have no text.</summary>
internal sealed record DocRecord(
    bool HasDocumentation,
    string? Summary,
    string? Remarks,
    IReadOnlyDictionary<string, string> Params,
    string? Returns,
    IReadOnlyList<ExceptionDoc> Exceptions,
    string? InheritedFrom)
{
    /// <summary>The documentation of a symbol that has none.</summary>
    public static DocRecord None { get; } = new(false, null, null, new Dictionary<string, string>(), null, [], null);
}
