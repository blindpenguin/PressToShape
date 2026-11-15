using System.Collections.Generic;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>A parsed XAML file.</summary>
internal sealed class XamlDocument
{
    public XamlDocument(string path, string project, XamlNode root, IReadOnlyList<XamlNode> nodes, IReadOnlyList<string> findings)
    {
        Path = path;
        Project = project;
        Root = root;
        Nodes = nodes;
        Findings = findings;
    }

    /// <summary>The repo-relative path with <c>/</c>.</summary>
    public string Path { get; }

    public string Project { get; }

    public XamlNode Root { get; }

    /// <summary>All elements in document order.</summary>
    public IReadOnlyList<XamlNode> Nodes { get; }

    /// <summary>Warnings found while reading, for example duplicate names.</summary>
    public IReadOnlyList<string> Findings { get; }

    public string RootType => Root.TypeText;

    public string? XClass => Root.AttributeValue("x:Class");

    /// <summary>Finds the namespace of a prefix, looking at the node and then at its ancestors. The empty prefix is the default namespace.</summary>
    public string? ResolvePrefix(XamlNode node, string prefix)
    {
        for (var current = node; current is not null; current = current.Parent)
        {
            if (current.Namespaces.TryGetValue(prefix, out var uri))
            {
                return uri;
            }
        }

        return null;
    }
}
