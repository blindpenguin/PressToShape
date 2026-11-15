using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>Either the parsed document or the problem that made the file unreadable.</summary>
internal sealed record XamlReadResult(XamlDocument? Document, LoadProblem? Problem);
