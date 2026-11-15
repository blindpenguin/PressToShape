using System.Collections.Generic;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Chain;

/// <summary>
/// Read access to the index for the analysis. The pages use a view over the data of a run, the <c>chain</c> command uses a view over the committed files.
/// Everything that interprets the data is written once against this interface, so pages and chains cannot disagree.
/// </summary>
internal interface IIndexView
{
    /// <summary>All relationships of all projects.</summary>
    IReadOnlyList<RelationshipRecord> Relationships { get; }

    /// <summary>All XAML files of all projects.</summary>
    IReadOnlyList<XamlFileRecord> XamlFiles { get; }

    IReadOnlyList<ProjectInfo> Projects { get; }

    /// <summary>The symbol with the id, or null. Namespaces are not available through every view.</summary>
    SymbolRecord? SymbolOf(string id);

    /// <summary>The members and nested types of a type.</summary>
    IReadOnlyList<SymbolRecord> MembersOf(string typeId);

    /// <summary>The XAML file with the id (<c>X:path</c>), or null.</summary>
    XamlFileRecord? XamlFileOf(string id);
}
