using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Query;

/// <summary>Finds members that a call can reach without naming them: the interface members a method implements and the base members it overrides. This is matched by name and parameter types, so every result is only <c>inferred</c>.</summary>
internal sealed class MemberMatcher
{
    private readonly IndexReader _reader;

    public MemberMatcher(IndexReader reader) => _reader = reader;

    /// <summary>Members that the symbol implements or overrides, with the reason.</summary>
    public IReadOnlyList<(SymbolRecord Member, string Reason)> ImplementedBy(SymbolRecord symbol)
    {
        var result = new List<(SymbolRecord, string)>();
        if (symbol.ContainingId is not { } typeId || !IsMember(symbol))
        {
            return result;
        }

        foreach (var interfaceId in TargetsOf(typeId, RelationshipType.Implements))
        {
            foreach (var member in _reader.MembersOf(interfaceId).Where(m => Matches(m, symbol)))
            {
                result.Add((member, $"{symbol.Name} has the same name and parameters as the interface member {member.QualifiedName}"));
            }
        }

        if (symbol.Modifiers.Contains("override"))
        {
            var visited = new HashSet<string>(StringComparer.Ordinal) { typeId };
            for (var next = TargetsOf(typeId, RelationshipType.Inherits).FirstOrDefault(); next is not null && visited.Add(next); next = TargetsOf(next, RelationshipType.Inherits).FirstOrDefault())
            {
                foreach (var member in _reader.MembersOf(next).Where(m => Matches(m, symbol)))
                {
                    result.Add((member, $"{symbol.Name} overrides {member.QualifiedName}"));
                }
            }
        }

        return result;
    }

    /// <summary>Members of types that implement the interface (or derive from the base) of the given member, matched by name and parameters.</summary>
    public IReadOnlyList<(SymbolRecord Member, string Reason)> Implementers(SymbolRecord member)
    {
        var result = new List<(SymbolRecord, string)>();
        if (member.ContainingId is not { } typeId || !IsMember(member))
        {
            return result;
        }

        var types = _reader.Relationships
            .Where(r => r.ToId == typeId && r.Type is RelationshipType.Implements or RelationshipType.Inherits)
            .Select(r => r.FromId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal);
        foreach (var implementingType in types)
        {
            foreach (var candidate in _reader.MembersOf(implementingType).Where(m => Matches(member, m)))
            {
                result.Add((candidate, $"{candidate.QualifiedName} has the same name and parameters as {member.QualifiedName}, and its type implements or derives from {typeId}"));
            }
        }

        return result;
    }

    private IEnumerable<string> TargetsOf(string typeId, RelationshipType type) =>
        _reader.Relationships.Where(r => r.FromId == typeId && r.Type == type).Select(r => r.ToId).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal);

    private static bool IsMember(SymbolRecord s) => s.SymbolType is SymbolType.Method or SymbolType.Property or SymbolType.Event;

    private static bool Matches(SymbolRecord declared, SymbolRecord candidate) =>
        declared.SymbolType == candidate.SymbolType
        && declared.Name == candidate.Name
        && declared.Parameters.Select(p => p.Type).SequenceEqual(candidate.Parameters.Select(p => p.Type));
}
