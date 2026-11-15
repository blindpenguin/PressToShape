namespace PressToShape.CodeIndex.Xaml.Model;

/// <summary>A resource definition (an entry with a key). <c>Variant</c> is the theme variant (Light, Dark) when the entry is in a theme dictionary.</summary>
internal sealed record XamlResourceRecord(string Id, string Key, string Type, string? TypeId, string? Variant, int Line);
