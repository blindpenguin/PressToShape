namespace PressToShape.CodeIndex.Xaml;

/// <summary>Why the reader kept an attribute. Plain literal values (sizes, colors, texts) are not kept.</summary>
internal enum XamlAttributeRole
{
    /// <summary>The value is a markup extension such as <c>{Binding Name}</c>.</summary>
    MarkupExtension,

    /// <summary>A literal that carries structure: <c>x:Class</c>, <c>x:DataType</c>, <c>DataType</c>, <c>TargetType</c>, <c>Selector</c>, <c>x:Key</c>, <c>x:Name</c>, <c>Source</c>, <c>Property</c>.</summary>
    Metadata,

    /// <summary>A value that is a plain identifier, so it could name an event handler. The resolver decides with Roslyn whether the attribute is an event.</summary>
    HandlerCandidate,
}
