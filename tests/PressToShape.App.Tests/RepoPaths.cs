using System;
using System.IO;

namespace PressToShape.App.Tests;

/// <summary>Finds files in the repository, relative to the solution root, without hardcoded absolute paths.</summary>
internal static class RepoPaths
{
    public static string Root { get; } = FindSolutionRoot();

    public static string Combine(string relativePath) => Path.Combine(Root, relativePath);

    private static string FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "PressToShape.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("PressToShape.sln not found above " + AppContext.BaseDirectory);
    }
}
