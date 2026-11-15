using System.Collections.Generic;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>A parsed markup extension, for example <c>{Binding Path, Mode=OneWay}</c>: the name, the positional arguments and the named arguments.</summary>
internal sealed record MarkupExtension(string Name, IReadOnlyList<MarkupValue> Positional, IReadOnlyDictionary<string, MarkupValue> Named)
{
    /// <summary>The text of the named argument, or null when it is missing or not plain text.</summary>
    public string? NamedText(string name) => Named.TryGetValue(name, out var value) ? (value as MarkupText)?.Value : null;

    /// <summary>The nested extension of the named argument, or null.</summary>
    public MarkupExtension? NamedExtension(string name) => Named.TryGetValue(name, out var value) ? (value as MarkupNested)?.Extension : null;

    /// <summary>The first positional argument as text, or null.</summary>
    public string? FirstPositionalText => Positional.Count > 0 ? (Positional[0] as MarkupText)?.Value : null;
}
