using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Chain;

/// <summary>The view over the result of a run, used when the pages are written.</summary>
internal sealed class ModelIndexView : IIndexView
{
    private readonly Dictionary<string, SymbolRecord> _symbols = new(StringComparer.Ordinal);
    private readonly ILookup<string, SymbolRecord> _members;
    private readonly Dictionary<string, XamlFileRecord> _xamlById = new(StringComparer.Ordinal);

    public ModelIndexView(IndexData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var projects = data.Projects.OrderBy(p => p.Name, StringComparer.Ordinal).ToList();
        foreach (var symbol in projects.SelectMany(p => p.Symbols))
        {
            _symbols.TryAdd(symbol.Id, symbol);
        }

        _members = _symbols.Values.Where(s => s.ContainingId is not null).ToLookup(s => s.ContainingId!, StringComparer.Ordinal);
        Relationships = projects.SelectMany(p => p.Relationships).ToList();
        XamlFiles = projects.SelectMany(p => p.Xaml).OrderBy(f => f.Id, StringComparer.Ordinal).ToList();
        foreach (var file in XamlFiles)
        {
            _xamlById.TryAdd(file.Id, file);
        }

        Projects = projects.Select(p => new ProjectInfo(p.Name, p.Xaml.Count)).ToList();
    }

    public IReadOnlyList<RelationshipRecord> Relationships { get; }

    public IReadOnlyList<XamlFileRecord> XamlFiles { get; }

    public IReadOnlyList<ProjectInfo> Projects { get; }

    public SymbolRecord? SymbolOf(string id) => _symbols.GetValueOrDefault(id);

    public IReadOnlyList<SymbolRecord> MembersOf(string typeId) => _members[typeId].OrderBy(s => s.Id, StringComparer.Ordinal).ToList();

    public XamlFileRecord? XamlFileOf(string id) => _xamlById.GetValueOrDefault(id);
}
