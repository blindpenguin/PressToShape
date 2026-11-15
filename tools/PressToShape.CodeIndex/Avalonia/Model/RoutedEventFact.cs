namespace PressToShape.CodeIndex.Avalonia.Model;

/// <summary>A routed event of a control. <c>Routing</c> is the routing strategy as written in the registration call, when there is one.</summary>
internal sealed record RoutedEventFact(string FieldId, string Name, string? ArgumentType, string? Routing, int Line);
