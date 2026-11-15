using System;
using System.Linq;

namespace PressToShape.CodeIndex.Output;

/// <summary>Checks that a path stays inside the index folder.</summary>
internal static class IndexPath
{
    /// <summary>Returns the path with <c>/</c> separators, or throws when it is empty, rooted or leaves the folder.</summary>
    public static string Normalize(string relativePath)
    {
        ArgumentNullException.ThrowIfNull(relativePath);

        var path = relativePath.Replace('\\', '/');
        var segments = path.Split('/');
        if (path.Length == 0 || path.StartsWith('/') || (path.Length > 1 && path[1] == ':')
            || segments.Any(s => s is "" or "." or ".."))
        {
            throw new ArgumentException("The index path must be relative and stay inside the index folder: " + relativePath, nameof(relativePath));
        }

        return path;
    }
}
