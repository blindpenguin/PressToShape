using System.Linq;

namespace PressToShape.CodeIndex.Output;

/// <summary>The names of the index files, shared by the writers and the reader.</summary>
internal static class IndexLayout
{
    public const string ManifestPath = "manifest.json";
    public const string NamesPath = "names.tsv";
    public const string MarkdownReadmePath = "markdown/README.md";

    public static string SymbolsPath(string project, string ns) => $"symbols/{FileName(project)}/{NamespaceFileName(ns)}.json";

    /// <summary>The file of one XAML file: <c>xaml/&lt;Project&gt;/&lt;path inside the project with / as .&gt;.json</c>.</summary>
    public static string XamlPath(string project, string projectDirectory, string file)
    {
        var relative = projectDirectory.Length > 0 && file.StartsWith(projectDirectory + "/", System.StringComparison.Ordinal) ? file[(projectDirectory.Length + 1)..] : file;
        return $"xaml/{FileName(project)}/{FileName(relative.Replace('/', '.'))}.json";
    }

    /// <summary>The Markdown page of one XAML file: <c>markdown/&lt;Project&gt;/xaml/&lt;same name as the JSON file&gt;.md</c>.</summary>
    public static string XamlMarkdownPath(string project, string projectDirectory, string file)
    {
        var json = XamlPath(project, projectDirectory, file);
        return $"markdown/{FileName(project)}/xaml/{json[(json.LastIndexOf('/') + 1)..^".json".Length]}.md";
    }

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
