using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Query;

namespace PressToShape.CodeIndex.Cli;

/// <summary>Writes query results as compact text or as JSON.</summary>
internal sealed class CliPrinter
{
    private readonly TextWriter _out;
    private readonly bool _json;

    public CliPrinter(TextWriter output, bool json)
    {
        _out = output;
        _json = json;
    }

    public void Line(string text) => _out.Write(text + "\n");

    public void Search(string term, IReadOnlyList<SearchHit> hits)
    {
        if (_json)
        {
            Json(hits);
            return;
        }

        if (hits.Count == 0)
        {
            Line($"No symbol matches '{term}'.");
        }

        foreach (var hit in hits)
        {
            Line($"{hit.Name} [{hit.SymbolType}] {hit.Source} {hit.Id}");
        }
    }

    /// <summary>Prints the result when it was found. Otherwise prints the candidates and returns false.</summary>
    public bool Resolved<T>(string query, QueryResult<T> result, Action<T> printFound)
    {
        if (result.Status == QueryStatus.Found)
        {
            printFound(result.Value!);
            return true;
        }

        if (_json)
        {
            Json(new { Status = result.Status.ToString(), result.Candidates });
        }
        else if (result.Status == QueryStatus.NotFound)
        {
            Line($"No symbol named '{query}'. Try: find {query}");
        }
        else
        {
            Line($"'{query}' is ambiguous. Use one of these ids:");
            foreach (var candidate in result.Candidates)
            {
                Line($"{candidate.Id} [{candidate.SymbolType}] {candidate.Source}");
            }
        }

        return false;
    }

    public void Hits(IReadOnlyList<QueryHit> hits)
    {
        if (_json)
        {
            Json(hits);
            return;
        }

        if (hits.Count == 0)
        {
            Line("(none)");
        }

        foreach (var hit in hits)
        {
            var reason = hit.Reason is null ? string.Empty : $" - {hit.Reason}";
            Line($"{hit.Name} [{hit.RelationType}, {hit.Confidence.ToString().ToLowerInvariant()}] {hit.Location} {hit.Id}{reason}");
        }
    }

    public void Show(ShowResult result)
    {
        if (_json)
        {
            Json(result);
            return;
        }

        if (result.Symbol is not { } symbol)
        {
            // XAML entities without a C# class (elements, resources, styles, templates, files without x:Class).
            XamlPrinter.Print(result.Xaml!, Line);
            PrintHits("Outgoing", result.Outgoing);
            PrintHits("Incoming", result.Incoming);
            return;
        }

        Line($"{symbol.Id} [{symbol.SymbolType}] {symbol.File}:{symbol.StartLine}-{symbol.EndLine}{(symbol.IsGenerated ? " (generated)" : string.Empty)}");
        Line($"{symbol.Accessibility}{(symbol.Modifiers.Count > 0 ? " " + string.Join(' ', symbol.Modifiers) : string.Empty)} {symbol.Signature ?? symbol.QualifiedName}".TrimEnd());
        if (symbol.BaseType is not null || symbol.Interfaces.Count > 0)
        {
            Line("Bases: " + string.Join(", ", symbol.Interfaces.Prepend(symbol.BaseType).Where(b => b is not null)));
        }

        if (symbol.GeneratedFrom is not null)
        {
            Line("Generated from: " + symbol.GeneratedFrom);
        }

        Line(symbol.Doc.HasDocumentation && symbol.Doc.Summary is { } summary ? "Summary: " + summary : "(no documentation)");
        PrintDocDetails(symbol.Doc);
        if (result.Xaml is { } xaml)
        {
            XamlPrinter.Print(xaml, Line);
        }

        PrintHits("Outgoing", result.Outgoing);
        PrintHits("Incoming", result.Incoming);
    }

    private void PrintDocDetails(DocRecord doc)
    {
        if (doc.Remarks is not null)
        {
            Line("Remarks: " + doc.Remarks.Replace("\n\n", " ", StringComparison.Ordinal));
        }

        foreach (var (name, text) in doc.Params)
        {
            Line($"Param {name}: {text}");
        }

        if (doc.Returns is not null)
        {
            Line("Returns: " + doc.Returns);
        }

        foreach (var exception in doc.Exceptions)
        {
            Line($"Throws {exception.Type}: {exception.Description}");
        }
    }

    private void PrintHits(string title, IReadOnlyList<QueryHit> hits)
    {
        if (hits.Count == 0)
        {
            return;
        }

        Line(title + ":");
        foreach (var hit in hits)
        {
            var reason = hit.Reason is null ? string.Empty : $" - {hit.Reason}";
            Line($"  {hit.RelationType} [{hit.Confidence.ToString().ToLowerInvariant()}] {hit.Name} {hit.Location} {hit.Id}{reason}".TrimEnd());
        }
    }

    private void Json<T>(T value) => Line(JsonSerializer.Serialize(value, JsonIndexWriter.Options));
}
