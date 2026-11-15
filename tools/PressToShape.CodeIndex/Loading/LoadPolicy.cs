using System;

namespace PressToShape.CodeIndex.Loading;

/// <summary>Decides which projects are analysed and which load failures only warn.</summary>
internal static class LoadPolicy
{
    private const string AndroidProject = "PressToShape.Android";

    /// <summary>Only projects under <c>src/</c> and <c>tests/</c> are indexed. The tool itself under <c>tools/</c> is not.</summary>
    public static bool IsIndexed(string repoRelativeProjectPath) =>
        repoRelativeProjectPath.StartsWith("src/", StringComparison.Ordinal) || repoRelativeProjectPath.StartsWith("tests/", StringComparison.Ordinal);

    /// <summary>The Android head needs the Android workload and JDK 21, so it often cannot load. Its failure is a warning and does not change the exit status (owner decision).</summary>
    public static bool IsWarning(string projectName) => projectName == AndroidProject;
}
