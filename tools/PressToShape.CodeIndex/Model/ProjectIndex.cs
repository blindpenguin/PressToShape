using System.Collections.Generic;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Model;

/// <summary>Everything the index holds about one project.</summary>
internal sealed record ProjectIndex(
    string Name,
    string Path,
    IReadOnlyList<SymbolRecord> Symbols,
    IReadOnlyList<RelationshipRecord> Relationships,
    IReadOnlyList<XamlFileRecord>? XamlFiles = null)
{
    /// <summary>The XAML files of the project, never null.</summary>
    public IReadOnlyList<XamlFileRecord> Xaml => XamlFiles ?? [];
}
