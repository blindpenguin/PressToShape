using System.Collections.Generic;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Query;

internal enum QueryStatus
{
    Found,
    Ambiguous,
    NotFound,
}

/// <summary>The answer to a query: the value when the name was resolved, otherwise the candidates that matched.</summary>
internal sealed record QueryResult<T>(QueryStatus Status, T? Value, IReadOnlyList<NameEntry> Candidates);

/// <summary>A search match.</summary>
internal sealed record SearchHit(string Name, string Id, string SymbolType, string Source);

/// <summary>A symbol that relates to the queried one. Location is the place of the relationship (a call site) or the symbol's own source.</summary>
internal sealed record QueryHit(string Id, string Name, string Location, string RelationType, Confidence Confidence, string? Reason);

/// <summary>
/// One symbol or XAML entity with the relationships that start or end at it. <c>Symbol</c> is null for XAML entities that have no C# class (elements, resources, styles).
/// <c>Xaml</c> is set for a view class and for XAML entities.
/// </summary>
internal sealed record ShowResult(SymbolRecord? Symbol, XamlShow? Xaml, IReadOnlyList<QueryHit> Outgoing, IReadOnlyList<QueryHit> Incoming);
