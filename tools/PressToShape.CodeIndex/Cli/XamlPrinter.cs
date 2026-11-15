using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Query;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Cli;

/// <summary>Prints the XAML part of <c>show</c> as compact text. A view prints everything of its file. Other entities print their own records and what belongs to them.</summary>
internal static class XamlPrinter
{
    public static void Print(XamlShow xaml, Action<string> line)
    {
        var file = xaml.File;
        line($"XAML: {file.File} [{xaml.EntityType}] {xaml.EntityId}");
        switch (xaml.EntityType)
        {
            case "View":
                PrintView(file, line);
                break;
            case "Element":
                PrintElement(file, xaml.EntityId, line);
                break;
            case "Resource":
                foreach (var resource in file.Resources.Where(r => r.Id == xaml.EntityId))
                {
                    line($"  resource '{resource.Key}' ({resource.Type}) line {resource.Line}{(resource.Variant is null ? string.Empty : ", theme " + resource.Variant)}{TypeId(resource.TypeId)}");
                }

                break;
            case "Style":
                foreach (var style in file.Styles.Where(s => s.Id == xaml.EntityId))
                {
                    line($"  style '{style.Selector}'{(style.Key is null ? string.Empty : " key " + style.Key)} line {style.Line}; sets {Join(style.Setters)}; uses {Join(style.ResourceKeys)}");
                }

                break;
            case "Template":
                foreach (var template in file.Templates.Where(t => t.Id == xaml.EntityId))
                {
                    line($"  {template.TemplateType}{(template.Key is null ? string.Empty : " " + template.Key)} line {template.Line}; target {template.Target ?? "(none)"}{TypeId(template.TargetId)}");
                }

                break;
        }

        foreach (var finding in file.Findings)
        {
            line($"  finding: {finding}");
        }
    }

    private static void PrintView(XamlFileRecord file, Action<string> line)
    {
        line($"  root {file.Root}; class {file.Class ?? "(none)"}{TypeId(file.ClassId)}; data type {file.DataType ?? "(none)"}{TypeId(file.DataTypeId)}");
        PrintBindings(file.Bindings, "Bindings", line);
        PrintReferences(file.ResourceRefs, line);
        PrintHandlers(file.Handlers, line);
        Section(file.Resources.Select(r => $"line {r.Line}: '{r.Key}' ({r.Type}){(r.Variant is null ? string.Empty : ", theme " + r.Variant)}").ToList(), "Resources defined", line);
        Section(file.Styles.Select(s => $"line {s.Line}: '{s.Selector}'{(s.Key is null ? string.Empty : " key " + s.Key)}").ToList(), "Styles", line);
        Section(file.Templates.Select(t => $"line {t.Line}: {t.TemplateType}{(t.Key is null ? string.Empty : " " + t.Key)} for {t.Target ?? "(none)"}").ToList(), "Templates", line);
    }

    private static void PrintElement(XamlFileRecord file, string elementId, Action<string> line)
    {
        if (file.Elements.FirstOrDefault(e => e.Id == elementId) is { } element)
        {
            line($"  element {element.Type}{(element.Name is null ? string.Empty : " '" + element.Name + "'")} line {element.Line}{TypeId(element.TypeId)}");
        }

        PrintBindings(file.Bindings.Where(b => b.ElementId == elementId).ToList(), "Bindings", line);
        PrintReferences(file.ResourceRefs.Where(r => r.ElementId == elementId).ToList(), line);
        PrintHandlers(file.Handlers.Where(h => h.ElementId == elementId).ToList(), line);
    }

    private static void PrintBindings(IReadOnlyList<XamlBindingRecord> bindings, string title, Action<string> line)
    {
        Section(bindings.Select(b =>
        {
            var target = b.TargetId is null
                ? $"unresolved ({b.Reason})"
                : $"{b.TargetId} ({b.Confidence.ToString().ToLowerInvariant()}{(b.Confidence == Confidence.Inferred ? ": " + b.Reason : string.Empty)})";
            return $"line {b.Line}: {b.Property} = {b.BindingType} '{b.Path}' -> {target}{(b.IsCommand ? ", command" : string.Empty)}{(b.Converter is null ? string.Empty : ", converter " + b.Converter)}";
        }).ToList(), title, line);
    }

    private static void PrintReferences(IReadOnlyList<XamlResourceRefRecord> references, Action<string> line) =>
        Section(references.Select(r => $"line {r.Line}: {r.Property} = {r.RefType} '{r.Key}' -> {(r.TargetIds.Count == 0 ? $"unresolved ({r.Reason})" : string.Join(", ", r.TargetIds) + " (verified)")}").ToList(), "Resource references", line);

    private static void PrintHandlers(IReadOnlyList<XamlHandlerRecord> handlers, Action<string> line) =>
        Section(handlers.Select(h => $"line {h.Line}: {h.Event} = {h.Handler} -> {(h.TargetId is null ? $"unresolved ({h.Reason})" : h.TargetId + " (verified)")}").ToList(), "Event handlers", line);

    private static void Section(IReadOnlyList<string> entries, string title, Action<string> line)
    {
        if (entries.Count == 0)
        {
            return;
        }

        line($"  {title} ({entries.Count}):");
        foreach (var entry in entries)
        {
            line("    " + entry);
        }
    }

    private static string TypeId(string? id) => id is null ? string.Empty : $" [{id}]";

    private static string Join(IReadOnlyList<string> values) => values.Count == 0 ? "(none)" : string.Join(", ", values);
}
