using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PressToShape.CodeIndex.Extraction;

namespace PressToShape.CodeIndex.Tests;

/// <summary>Builds small compilations in memory, so tests need no solution and no MSBuild.</summary>
internal static class TestCompilation
{
    private static readonly Lazy<IReadOnlyList<MetadataReference>> References = new(LoadReferences);

    /// <summary>A fake repository root. Nothing is read from or written to it.</summary>
    public static string RepoRoot { get; } = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "repo"));

    public static ProjectCompilation Project(string name, params string[] sources)
    {
        var options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Diagnose);
        var trees = sources.Select((text, i) =>
            CSharpSyntaxTree.ParseText(text, options, Path.Combine(RepoRoot, "src", name, $"File{i}.cs")));
        var compilation = CSharpCompilation.Create(
            name,
            trees,
            References.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        return new ProjectCompilation(name, $"src/{name}/{name}.csproj", compilation);
    }

    /// <summary>Runs a generator over the project and returns the project with the generated trees marked.</summary>
    public static ProjectCompilation WithGenerator(ProjectCompilation project, IIncrementalGenerator generator)
    {
        var driver = CSharpGeneratorDriver.Create(generator).RunGeneratorsAndUpdateCompilation(project.Compilation, out var updated, out _);
        var generated = driver.GetRunResult().GeneratedTrees.ToHashSet();
        return project with { Compilation = updated, GeneratedTrees = generated };
    }

    public static ProjectCompilation Project(params string[] sources) => Project("Test", sources);

    private static IReadOnlyList<MetadataReference> LoadReferences()
    {
        var trusted = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty;
        return trusted.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(p => Path.GetFileName(p) is "System.Private.CoreLib.dll" or "System.Runtime.dll" or "System.Collections.dll"
                or "System.Linq.dll" or "System.ComponentModel.dll" or "System.ObjectModel.dll" or "netstandard.dll")
            .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
            .ToList();
    }
}
