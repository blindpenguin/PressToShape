namespace PressToShape.CodeIndex.Xaml;

/// <summary>A kept attribute. <c>Parsed</c> is set for markup extensions that the parser understood.</summary>
internal sealed record XamlAttribute(string Name, string Value, int Line, XamlAttributeRole Role, MarkupExtension? Parsed);
