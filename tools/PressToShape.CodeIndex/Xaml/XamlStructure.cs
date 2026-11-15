using System.Collections.Generic;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>The structure facts of a file: resource definitions, styles, templates and event handlers.</summary>
internal sealed record XamlStructure(
    IReadOnlyList<XamlResourceRecord> Resources,
    IReadOnlyList<XamlStyleRecord> Styles,
    IReadOnlyList<XamlTemplateRecord> Templates,
    IReadOnlyList<XamlHandlerRecord> Handlers);
