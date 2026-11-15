using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Xaml.Model;

/// <summary>An event handler attribute, with the method it resolved to (verified) or the reason why not.</summary>
internal sealed record XamlHandlerRecord(string Id, string ElementId, string Event, string Handler, string? TargetId, Confidence Confidence, string? Reason, int Line);
