using System.Collections.Generic;

namespace PressToShape.CodeIndex.Xaml.Model;

/// <summary>Everything the index holds about one XAML file.</summary>
internal sealed record XamlFileRecord(
    string Id,
    string Project,
    string File,
    string Root,
    string? Class,
    string? ClassId,
    string? DataType,
    string? DataTypeId,
    IReadOnlyDictionary<string, string> Namespaces,
    IReadOnlyList<XamlElementRecord> Elements,
    IReadOnlyList<XamlBindingRecord> Bindings,
    IReadOnlyList<XamlResourceRecord> Resources,
    IReadOnlyList<XamlResourceRefRecord> ResourceRefs,
    IReadOnlyList<XamlStyleRecord> Styles,
    IReadOnlyList<XamlTemplateRecord> Templates,
    IReadOnlyList<XamlHandlerRecord> Handlers,
    IReadOnlyList<string> Findings);
