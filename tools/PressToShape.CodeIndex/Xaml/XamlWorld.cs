using System;
using System.Collections.Generic;
using System.Linq;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>All parsed XAML documents of the solution, with the include graph and the application document. Documents are looked up by repo-relative path.</summary>
internal sealed class XamlWorld
{
    private const string AvaresScheme = "avares://";

    private readonly Dictionary<string, XamlDocument> _byPath;
    private readonly IReadOnlyDictionary<string, string> _assemblyDirectories;

    /// <param name="documents">Every document of every indexed project.</param>
    /// <param name="assemblyDirectories">Assembly name to repo-relative project folder, for <c>avares://Assembly/path</c> includes.</param>
    public XamlWorld(IEnumerable<XamlDocument> documents, IReadOnlyDictionary<string, string> assemblyDirectories)
    {
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(assemblyDirectories);
        Documents = documents.OrderBy(d => d.Path, StringComparer.Ordinal).ToList();
        _byPath = Documents.ToDictionary(d => d.Path, StringComparer.Ordinal);
        _assemblyDirectories = assemblyDirectories;
    }

    public IReadOnlyList<XamlDocument> Documents { get; }

    public XamlDocument? Find(string path) => _byPath.GetValueOrDefault(path);

    /// <summary>The application document: the one in the same project folder as the document, otherwise the first one by path.</summary>
    public XamlDocument? ApplicationFor(XamlDocument document)
    {
        var applications = Documents.Where(d => XamlNames.IsType(d.Root, "Application")).ToList();
        var folder = ProjectDirectoryOf(document);
        return applications.FirstOrDefault(a => ProjectDirectoryOf(a) == folder) ?? applications.FirstOrDefault();
    }

    /// <summary>Resolves an include <c>Source</c> to a document of the solution, or null when it points elsewhere (a package, a missing file).</summary>
    public XamlDocument? ResolveInclude(XamlDocument from, string source)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(source);

        var text = source.Trim();
        string? path;
        if (text.StartsWith(AvaresScheme, StringComparison.Ordinal))
        {
            var rest = text[AvaresScheme.Length..];
            var slash = rest.IndexOf('/', StringComparison.Ordinal);
            path = slash < 0 || !_assemblyDirectories.TryGetValue(rest[..slash], out var directory) ? null : Combine(directory, rest[(slash + 1)..]);
        }
        else if (text.Contains("://", StringComparison.Ordinal))
        {
            path = null;
        }
        else if (text.StartsWith('/'))
        {
            path = Combine(ProjectDirectoryOf(from) ?? string.Empty, text[1..]);
        }
        else
        {
            path = Combine(DirectoryOf(from.Path), text);
        }

        return path is null ? null : Find(path);
    }

    /// <summary>The <c>StyleInclude</c>, <c>ResourceInclude</c> and <c>MergeResourceInclude</c> elements of a document with their resolved target (null when outside the solution).</summary>
    public IReadOnlyList<(XamlNode Node, string Source, XamlDocument? Target)> IncludesOf(XamlDocument document) =>
        document.Nodes
            .Where(n => !n.IsPropertyElement && XamlNames.Local(n.TypeText) is "StyleInclude" or "ResourceInclude" or "MergeResourceInclude")
            .Select(n => (Node: n, Source: n.AttributeValue("Source")))
            .Where(x => x.Source is not null)
            .Select(x => (x.Node, x.Source!, ResolveInclude(document, x.Source!)))
            .ToList();

    private string? ProjectDirectoryOf(XamlDocument document) =>
        _assemblyDirectories.Values
            .Where(d => document.Path.StartsWith(d + "/", StringComparison.Ordinal))
            .OrderByDescending(d => d.Length)
            .FirstOrDefault();

    private static string DirectoryOf(string path)
    {
        var slash = path.LastIndexOf('/');
        return slash < 0 ? string.Empty : path[..slash];
    }

    /// <summary>Joins a folder and a relative path and removes <c>.</c> and <c>..</c> parts. Returns null when the path leaves the repository.</summary>
    private static string? Combine(string directory, string relative)
    {
        var parts = new List<string>(directory.Split('/', StringSplitOptions.RemoveEmptyEntries));
        foreach (var part in relative.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part == ".")
            {
                continue;
            }

            if (part == "..")
            {
                if (parts.Count == 0)
                {
                    return null;
                }

                parts.RemoveAt(parts.Count - 1);
            }
            else
            {
                parts.Add(part);
            }
        }

        return string.Join('/', parts);
    }
}
