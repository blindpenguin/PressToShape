using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Relationships;

/// <summary>Links command properties that CommunityToolkit.Mvvm generated from a <c>[RelayCommand]</c> method to that method.</summary>
internal static class CommandHandlerLinker
{
    public const string GeneratedReason = "[RelayCommand]";

    public static IReadOnlyList<RelationshipRecord> FromGenerated(IReadOnlyList<SymbolRecord> symbols)
    {
        ArgumentNullException.ThrowIfNull(symbols);

        var methods = symbols.Where(s => s.SymbolType == SymbolType.Method).ToDictionary(s => s.Id, StringComparer.Ordinal);
        return symbols
            .Where(s => s.SymbolType == SymbolType.Property && s.GeneratedFrom is { } from && methods.ContainsKey(from))
            .Select(s =>
            {
                var method = methods[s.GeneratedFrom!];
                return new RelationshipRecord(RelationshipType.ExecutesMethod, s.Id, method.Id, method.File, method.StartLine, Confidence.Verified, GeneratedReason);
            })
            .OrderBy(e => e.FromId, StringComparer.Ordinal)
            .ThenBy(e => e.ToId, StringComparer.Ordinal)
            .ToList();
    }
}
