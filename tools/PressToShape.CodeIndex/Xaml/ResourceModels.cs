using System.Collections.Generic;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>A resource definition: an element with an <c>x:Key</c> in a resource dictionary. <c>Variant</c> is the theme variant (Light, Dark) or null.</summary>
internal sealed record ResourceEntry(string Id, string Key, XamlDocument Document, XamlNode Node, string ScopePath, string? Variant);

/// <summary>The outcome of a resource lookup: the nearest definitions (several when theme variants exist), or the reason that none was found.</summary>
internal sealed record ResourceLookup(IReadOnlyList<ResourceEntry> Hits, string? Reason)
{
    public const string NotInSolution = "not defined in this solution (may come from the Avalonia theme)";

    public bool IsFound => Hits.Count > 0;

    public static ResourceLookup NotFound() => new([], NotInSolution);
}
