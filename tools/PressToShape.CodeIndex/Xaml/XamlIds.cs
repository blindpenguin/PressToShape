using System;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>
/// Ids of XAML entities. They contain only repo-relative paths. The prefixes (<c>X:</c> file, element and binding, <c>R:</c> resource, <c>S:</c> style)
/// differ from the Roslyn prefixes (<c>T:</c>, <c>M:</c>, <c>P:</c>, <c>F:</c>, <c>E:</c>, <c>N:</c>).
/// Element paths such as <c>/UserControl/Grid/Button[1]</c> change when same-type siblings are inserted before an element. Names stay searchable.
/// </summary>
internal static class XamlIds
{
    public static string File(string path) => "X:" + path;

    public static string Element(string path, string elementPath) => $"X:{path}#{elementPath}";

    public static string Binding(string elementId, string property) => $"{elementId}@{property}";

    public static string Resource(string path, string scope, string key, string? variant) =>
        $"R:{path}#{scope}/{key}" + (variant is null ? string.Empty : "@" + variant);

    public static string Style(string path, string selectorOrKey, int line) => $"S:{path}#{selectorOrKey}@{line}";

    public static bool IsXaml(string id) => id.StartsWith("X:", StringComparison.Ordinal) || id.StartsWith("R:", StringComparison.Ordinal) || id.StartsWith("S:", StringComparison.Ordinal);

    /// <summary>A short readable text for an id, for example <c>LayersView.axaml Button[1] @Command</c>. Other ids are returned unchanged.</summary>
    public static string Describe(string id)
    {
        if (!IsXaml(id))
        {
            return id;
        }

        var body = id[2..];
        var hash = body.IndexOf('#', StringComparison.Ordinal);
        var file = LastSegment(hash < 0 ? body : body[..hash]);
        if (hash < 0)
        {
            return file;
        }

        var fragment = body[(hash + 1)..];
        return id[0] switch
        {
            'X' => DescribeElement(file, fragment),
            'R' => DescribeResource(file, fragment),
            _ => DescribeStyle(file, fragment),
        };
    }

    private static string DescribeElement(string file, string fragment)
    {
        var at = fragment.LastIndexOf('@');
        var elementPath = at < 0 ? fragment : fragment[..at];
        var property = at < 0 ? string.Empty : " @" + fragment[(at + 1)..];
        return $"{file} {LastSegment(elementPath)}{property}";
    }

    private static string DescribeResource(string file, string fragment)
    {
        var at = fragment.LastIndexOf('@');
        var variant = at < 0 ? string.Empty : $" ({fragment[(at + 1)..]})";
        var keyPart = at < 0 ? fragment : fragment[..at];
        var slash = keyPart.LastIndexOf('/');
        return $"{file} resource '{(slash < 0 ? keyPart : keyPart[(slash + 1)..])}'{variant}";
    }

    private static string DescribeStyle(string file, string fragment)
    {
        var at = fragment.LastIndexOf('@');
        return at < 0 ? $"{file} style '{fragment}'" : $"{file} style '{fragment[..at]}' (line {fragment[(at + 1)..]})";
    }

    private static string LastSegment(string path)
    {
        var slash = path.LastIndexOf('/');
        return slash < 0 ? path : path[(slash + 1)..];
    }
}
