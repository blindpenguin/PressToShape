namespace PressToShape.CodeIndex.Query;

/// <summary>One line of <c>names.tsv</c>.</summary>
internal sealed record NameEntry(string Name, string Id, string IndexFile, string SymbolType, string Source);
