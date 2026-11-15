using System.Linq;

namespace PressToShape.CodeIndex.Output;

/// <summary>The names of the index files, shared by the writers and the reader.</summary>
internal static class IndexLayout
{
    public const string ManifestPath = "manifest.json";
    public const string NamesPath = "names.tsv";
    public const string MarkdownReadmePath = "markdown/README.md";

    public static string SymbolsPath(string project, string ns) => $"symbols/{FileName(project)}/{NamespaceFileName(ns)}.json";

    public static string RelationshipsPath(string project) => $"relationships/{FileName(project)}.json";

    public static string MarkdownPath(string project, string ns) => $"markdown/{FileName(project)}/{NamespaceFileName(ns)}.md";

    private static string NamespaceFileName(string ns) => ns.Length == 0 ? "_global" : FileName(ns);

    /// <summary>Keeps letters, digits, dot, dash and underscore. Everything else becomes an underscore, and a name of only dots is replaced too, so names never leave their folder.</summary>
    private static string FileName(string name)
    {
        var clean = new string(name.Select(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_' ? c : '_').ToArray());
        return clean.All(c => c == '.') ? new string('_', clean.Length) : clean;
    }
}
