using System.Collections.Generic;

namespace PressToShape.CodeIndex.Model;

/// <summary>One symbol of the solution. The id is the Roslyn documentation comment id, which is stable and contains no paths.</summary>
internal sealed record SymbolRecord(
    string Id,
    string Name,
    string QualifiedName,
    SymbolType SymbolType,
    string Accessibility,
    IReadOnlyList<string> Modifiers,
    string? ContainingId,
    string Namespace,
    string Project,
    string File,
    int StartLine,
    int EndLine,
    string? Signature,
    IReadOnlyList<ParameterRecord> Parameters,
    string? TypeName,
    IReadOnlyList<AttributeRecord> Attributes,
    string? BaseType,
    IReadOnlyList<string> Interfaces,
    bool IsGenerated,
    string? GeneratedFrom,
    DocRecord Doc);
