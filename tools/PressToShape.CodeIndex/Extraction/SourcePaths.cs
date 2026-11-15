using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace PressToShape.CodeIndex.Extraction;

/// <summary>Turns syntax tree paths into index paths: repo-relative with <c>/</c>, or <c>generated/&lt;hint&gt;</c> for generator output.</summary>
internal static class SourcePaths
{
    public static string Of(ProjectCompilation project, SyntaxTree tree, string repoRoot)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(tree);
        ArgumentNullException.ThrowIfNull(repoRoot);

        var path = tree.FilePath;
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        // Generated sources have no real file, so only the generator's hint name is kept. This also keeps machine paths out of the index.
        return project.IsGenerated(tree)
            ? "generated/" + path.Replace('\\', '/').Split('/')[^1]
            : Path.GetRelativePath(repoRoot, path).Replace('\\', '/');
    }
}
