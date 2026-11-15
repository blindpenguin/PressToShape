using System.Collections.Generic;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>The XAML files and relationships of one project.</summary>
internal sealed record XamlProjectData(IReadOnlyList<XamlFileRecord> Files, IReadOnlyList<RelationshipRecord> Relationships)
{
    public static XamlProjectData Empty { get; } = new([], []);
}

/// <summary>The XAML part of an analysis: data per project name, and the files that could not be read.</summary>
internal sealed record XamlIndexResult(IReadOnlyDictionary<string, XamlProjectData> Projects, IReadOnlyList<LoadProblem> Problems)
{
    public static XamlIndexResult Empty { get; } = new(new Dictionary<string, XamlProjectData>(), []);

    public XamlProjectData For(string project) => Projects.TryGetValue(project, out var data) ? data : XamlProjectData.Empty;
}
