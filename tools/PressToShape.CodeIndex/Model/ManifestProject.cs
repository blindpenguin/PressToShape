namespace PressToShape.CodeIndex.Model;

/// <summary>A project listed in the manifest, with its counts. <c>XamlFileCount</c> is the number of XAML files.</summary>
internal sealed record ManifestProject(string Name, string Path, int SymbolCount, int RelationshipCount, int XamlFileCount = 0);
