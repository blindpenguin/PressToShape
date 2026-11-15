using System.Collections.Generic;

namespace PressToShape.CodeIndex.Xaml.Model;

/// <summary>A style. The selector is kept as text and is not evaluated.</summary>
internal sealed record XamlStyleRecord(string Id, string Selector, string? Key, IReadOnlyList<string> Setters, IReadOnlyList<string> ResourceKeys, int Line);
