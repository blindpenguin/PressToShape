namespace PressToShape.CodeIndex.Model;

/// <summary>A project listed in the manifest, with its counts.</summary>
internal sealed record ManifestProject(string Name, string Path, int SymbolCount, int RelationshipCount);
