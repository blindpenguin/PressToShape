namespace PressToShape.CodeIndex.Xaml.Model;

/// <summary>A ControlTemplate or DataTemplate with its target (<c>TargetType</c> or <c>DataType</c>).</summary>
internal sealed record XamlTemplateRecord(string Id, string TemplateType, string? Target, string? TargetId, string? Key, int Line);
