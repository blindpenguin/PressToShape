using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Query;

/// <summary>Answers questions about the index. Every result carries its confidence, and anything that is not proven by the compiler is marked inferred.</summary>
internal sealed class QueryService
{
    /// <summary>The relationship types that count as a use of the target: C# references and calls, and the XAML places that bind, reference, handle, use or include it.</summary>
    private static readonly RelationshipType[] UsageTypes =
    [
        RelationshipType.References,
        RelationshipType.Calls,
        RelationshipType.Registers,
        RelationshipType.UsesViewModel,
        RelationshipType.CodeBehind,
        RelationshipType.BindsTo,
        RelationshipType.ReferencesResource,
        RelationshipType.UsesConverter,
        RelationshipType.HandledBy,
        RelationshipType.UsesType,
        RelationshipType.Includes,
    ];

    private readonly IndexReader _reader;
    private readonly MemberMatcher _matcher;
    private readonly XamlQueries _xaml;
    private readonly NameResolver _resolver;

    public QueryService(IndexReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        _reader = reader;
        _matcher = new MemberMatcher(reader);
        _xaml = new XamlQueries(reader);
        _resolver = new NameResolver(reader);
    }

    /// <summary>Finds symbols by name. Exact matches come first, then prefix, then substring. A term with a dot or colon also matches ids.</summary>
    public IReadOnlyList<SearchHit> Search(string term, int limit = 50)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(term);

        var idSearch = term.Contains('.', StringComparison.Ordinal) || term.Contains(':', StringComparison.Ordinal);
        return _reader.Names
            .Select(n => (Entry: n, Rank: Rank(n, term, idSearch)))
            .Where(x => x.Rank >= 0)
            .OrderBy(x => x.Rank)
            .ThenBy(x => x.Entry.Name, StringComparer.Ordinal)
            .ThenBy(x => x.Entry.Id, StringComparer.Ordinal)
            .Take(limit)
            .Select(x => new SearchHit(x.Entry.Name, x.Entry.Id, x.Entry.SymbolType, x.Entry.Source))
            .ToList();
    }

    /// <summary>A C# symbol (with the XAML file of a view class) or a XAML entity, with the relationships that start or end at it.</summary>
    public QueryResult<ShowResult> Show(string query) => Resolve(query, entry =>
    {
        var edges = _reader.Relationships;
        var startsAt = XamlQueries.StartsAt(entry);
        var outgoing = edges.Where(startsAt).Select(e => ToHit(e, e.ToId)).OrderBy(h => h.RelationType, StringComparer.Ordinal).ThenBy(h => h.Id, StringComparer.Ordinal).ToList();
        var incoming = edges.Where(e => e.ToId == entry.Id).Select(e => ToHit(e, e.FromId)).OrderBy(h => h.RelationType, StringComparer.Ordinal).ThenBy(h => h.Id, StringComparer.Ordinal).ToList();

        if (XamlQueries.IsXamlEntry(entry))
        {
            var xaml = _xaml.ForEntry(entry);
            var viewClass = entry.SymbolType == "View" && xaml.File.ClassId is { } classId ? _reader.SymbolOf(classId) : null;
            return new ShowResult(viewClass, xaml, outgoing, incoming);
        }

        var symbol = _reader.SymbolOf(entry.Id) ?? throw new IndexToolException($"The symbol {entry.Id} is listed in names.tsv but missing in {entry.IndexFile}. Regenerate the index with: {IndexHints.Regenerate}");
        return new ShowResult(symbol, _xaml.ViewOfClass(symbol.Id), outgoing, incoming);
    });

    /// <summary>Types that implement an interface (verified). For an interface member, members with the same name and parameters in implementing types (inferred).</summary>
    public QueryResult<IReadOnlyList<QueryHit>> Implementations(string query) => Resolve(query, entry =>
    {
        var hits = new List<QueryHit>();
        hits.AddRange(_reader.Relationships
            .Where(e => e.ToId == entry.Id && (e.Type == RelationshipType.Implements || (e.Type == RelationshipType.Inherits && entry.SymbolType == "Interface")))
            .Select(e => ToHit(e, e.FromId)));
        if (_reader.SymbolOf(entry.Id) is { } symbol)
        {
            hits.AddRange(_matcher.Implementers(symbol).Select(m => SymbolHit(m.Member, "Implements", m.Reason)));
        }

        return Sort(hits);
    });

    /// <summary>Types that derive directly from a type.</summary>
    public QueryResult<IReadOnlyList<QueryHit>> Derived(string query) => Resolve(query, entry =>
        Sort(_reader.Relationships.Where(e => e.ToId == entry.Id && e.Type == RelationshipType.Inherits).Select(e => ToHit(e, e.FromId))));

    /// <summary>Places that reference, call or register a symbol. Calls to the interface or base member that the symbol implements or overrides are added as inferred.</summary>
    public QueryResult<IReadOnlyList<QueryHit>> References(string query) => Resolve(query, entry =>
    {
        var usageTypes = UsageTypes;
        var hits = _reader.Relationships.Where(e => e.ToId == entry.Id && usageTypes.Contains(e.Type)).Select(e => ToHit(e, e.FromId)).ToList();
        if (_reader.SymbolOf(entry.Id) is { } symbol)
        {
            foreach (var (member, reason) in _matcher.ImplementedBy(symbol))
            {
                hits.AddRange(_reader.Relationships
                    .Where(e => e.ToId == member.Id && usageTypes.Contains(e.Type))
                    .Select(e => ToHit(e, e.FromId) with { Confidence = Confidence.Inferred, Reason = $"{reason}. The call goes through {member.Id}." }));
            }
        }

        return Sort(hits);
    });

    /// <summary>The types and services a type takes in its constructors.</summary>
    public QueryResult<IReadOnlyList<QueryHit>> Dependencies(string query) => Resolve(query, entry =>
        Sort(_reader.Relationships.Where(e => e.FromId == entry.Id && e.Type == RelationshipType.InjectedDependency).Select(e => ToHit(e, e.ToId))));

    private QueryResult<T> Resolve<T>(string query, Func<NameEntry, T> answer)
    {
        var resolution = _resolver.Resolve(query);
        return resolution.Status switch
        {
            QueryStatus.Found => new QueryResult<T>(QueryStatus.Found, answer(resolution.Matches[0]), resolution.Matches),
            QueryStatus.Ambiguous => new QueryResult<T>(QueryStatus.Ambiguous, default, resolution.Matches),
            _ => new QueryResult<T>(QueryStatus.NotFound, default, []),
        };
    }

    private static IReadOnlyList<QueryHit> Sort(IEnumerable<QueryHit> hits) => hits
        .DistinctBy(h => (h.Id, h.Location, h.RelationType, h.Confidence))
        .OrderBy(h => h.Confidence)
        .ThenBy(h => h.Id, StringComparer.Ordinal)
        .ThenBy(h => h.Location, StringComparer.Ordinal)
        .ToList();

    private QueryHit ToHit(RelationshipRecord edge, string otherId) =>
        new(otherId, NameOf(otherId), edge.File is null ? string.Empty : edge.Line is { } line ? $"{edge.File}:{line}" : edge.File, edge.Type.ToString(), edge.Confidence, edge.Reason);

    private QueryHit SymbolHit(SymbolRecord symbol, string relationType, string reason) =>
        new(symbol.Id, symbol.Name, $"{symbol.File}:{symbol.StartLine}", relationType, Confidence.Inferred, reason);

    private string NameOf(string id) => _xaml.NameOf(id);

    private static int Rank(NameEntry n, string term, bool idSearch)
    {
        if (string.Equals(n.Name, term, StringComparison.Ordinal))
        {
            return 0;
        }

        if (string.Equals(n.Name, term, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (n.Name.StartsWith(term, StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (n.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
        {
            return 3;
        }

        return idSearch && n.Id.Contains(term, StringComparison.OrdinalIgnoreCase) ? 4 : -1;
    }
}
