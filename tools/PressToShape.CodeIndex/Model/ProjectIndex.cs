using System.Collections.Generic;

namespace PressToShape.CodeIndex.Model;

/// <summary>Everything the index holds about one project.</summary>
internal sealed record ProjectIndex(
    string Name,
    string Path,
    IReadOnlyList<SymbolRecord> Symbols,
    IReadOnlyList<RelationshipRecord> Relationships);
