using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Output;

/// <summary>Writes <c>names.tsv</c>, a flat list for grep. Columns: name, id, index file, symbol type, source path and line.</summary>
internal static class NamesTableWriter
{
    public static void Write(IReadOnlyList<ProjectIndex> projects, IIndexFileSink sink)
    {
        var lines = projects
            .SelectMany(p => p.Symbols)
            .Where(s => s.SymbolType is not SymbolType.Namespace and not SymbolType.Constructor)
            .OrderBy(s => s.Name, StringComparer.Ordinal)
            .ThenBy(s => s.Id, StringComparer.Ordinal)
            .Select(s => string.Join('\t', Clean(s.Name), Clean(s.Id), IndexLayout.SymbolsPath(s.Project, s.Namespace), s.SymbolType, $"{s.File}:{s.StartLine}"));

        var text = new StringBuilder();
        foreach (var line in lines)
        {
            text.Append(line).Append('\n');
        }

        sink.Write(IndexLayout.NamesPath, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(text.ToString()));
    }

    private static string Clean(string value) => value.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');
}
