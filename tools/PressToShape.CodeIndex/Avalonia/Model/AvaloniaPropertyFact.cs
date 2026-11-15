namespace PressToShape.CodeIndex.Avalonia.Model;

/// <summary>An Avalonia property of a control. <c>PropertyType</c> is Styled, Attached or Direct. <c>Default</c> is the default value as written in the registration call.</summary>
internal sealed record AvaloniaPropertyFact(string FieldId, string Name, string PropertyType, string ValueType, string? Default, bool HasClrWrapper, int Line);
