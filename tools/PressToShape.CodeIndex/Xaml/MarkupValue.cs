namespace PressToShape.CodeIndex.Xaml;

/// <summary>An argument of a markup extension: plain text or another markup extension.</summary>
internal abstract record MarkupValue;

/// <summary>Plain text, with quotes and escapes already removed.</summary>
internal sealed record MarkupText(string Value) : MarkupValue;

/// <summary>A nested markup extension such as <c>{StaticResource Key}</c> inside a binding.</summary>
internal sealed record MarkupNested(MarkupExtension Extension) : MarkupValue;
