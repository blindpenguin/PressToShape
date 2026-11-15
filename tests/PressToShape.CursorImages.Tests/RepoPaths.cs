using System;
using System.IO;

namespace PressToShape.CursorImages.Tests;

internal static class RepoPaths
{
    /// <summary>The repository root: the folder above the test output that holds PressToShape.sln.</summary>
    public static string Root { get; } = FindRoot();

    public static string Combine(string relativePath) => Path.Combine(Root, relativePath);

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "PressToShape.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("PressToShape.sln was not found above the test output.");
    }
}
