using System;
using System.IO;

namespace PressToShape.CodeIndex;

/// <summary>Finds the repository root: the folder holding <c>PressToShape.sln</c>.</summary>
internal static class RepoRoot
{
    public const string SolutionFileName = "PressToShape.sln";

    public static string Find(string startDirectory) => Find(startDirectory, File.Exists);

    public static string Find(string startDirectory, Func<string, bool> fileExists)
    {
        ArgumentNullException.ThrowIfNull(startDirectory);
        ArgumentNullException.ThrowIfNull(fileExists);

        var directory = Path.GetFullPath(startDirectory);
        while (true)
        {
            if (fileExists(Path.Combine(directory, SolutionFileName)))
            {
                return directory;
            }

            var parent = Path.GetDirectoryName(directory);
            if (parent is null)
            {
                throw new InvalidOperationException(
                    SolutionFileName + " not found above " + startDirectory + ". Run the tool inside the repository or pass --solution.");
            }

            directory = parent;
        }
    }
}
