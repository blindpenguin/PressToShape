namespace PressToShape.CodeIndex.Xaml.Model;

/// <summary>One element of a XAML file. <c>Property</c> is set for a property element such as <c>UserControl.Resources</c>.</summary>
internal sealed record XamlElementRecord(string Id, string? ParentId, string Type, string? TypeId, string? Name, int Line, string? Property);
