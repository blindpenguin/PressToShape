using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Query;

/// <summary>The XAML part of a <c>show</c> result: the XAML file and the entity that was asked for (the view itself, an element, a resource, a style or a template).</summary>
internal sealed record XamlShow(XamlFileRecord File, string EntityId, string EntityType);
