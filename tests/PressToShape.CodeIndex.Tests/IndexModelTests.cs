using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PressToShape.CodeIndex.Model;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class IndexModelTests
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };

    [Fact]
    public void SymbolRecord_RoundTrip_KeepsAllValues()
    {
        var symbol = new SymbolRecord(
            "M:N.C.M(System.Int32)", "M", "N.C.M", SymbolType.Method, "public", ["static"], "T:N.C", "N", "P", "src/P/C.cs", 3, 9,
            "void M(int x)", [new ParameterRecord("x", "int")], "void", [new AttributeRecord("Obsolete", ["\"x\""])],
            null, [], true, "F:N.C._f",
            new DocRecord(true, "Does it.", null, new Dictionary<string, string> { ["x"] = "The x." }, null, [new ExceptionDoc("System.ArgumentException", "Bad.")], null));

        var json = JsonSerializer.Serialize(symbol, Options);
        var back = JsonSerializer.Deserialize<SymbolRecord>(json, Options);

        Assert.NotNull(back);
        Assert.Equal(json, JsonSerializer.Serialize(back, Options));
        Assert.Contains("\"SymbolType\":\"Method\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void RelationshipRecord_RoundTrip_WritesEnumsAsNames()
    {
        var edge = new RelationshipRecord(RelationshipType.InjectedDependency, "T:A", "T:B", "src/A.cs", 12, Confidence.Inferred, "reflection");

        var json = JsonSerializer.Serialize(edge, Options);
        var back = JsonSerializer.Deserialize<RelationshipRecord>(json, Options);

        Assert.Equal(edge, back);
        Assert.Contains("\"Type\":\"InjectedDependency\"", json, StringComparison.Ordinal);
        Assert.Contains("\"Confidence\":\"Inferred\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void IndexManifest_RoundTrip_KeepsProjectsAndWarnings()
    {
        var manifest = new IndexManifest(
            IndexSchema.Version, "1.0.0", [new ManifestProject("Core", "src/Core", 5, 2)], 5, 2, [new LoadProblem("Android", "workload missing", true)]);

        var json = JsonSerializer.Serialize(manifest, Options);
        var back = JsonSerializer.Deserialize<IndexManifest>(json, Options);

        Assert.NotNull(back);
        Assert.Equal(json, JsonSerializer.Serialize(back, Options));
        Assert.Equal(IndexSchema.Version, back.SchemaVersion);
    }

    [Fact]
    public void DocRecord_None_HasNoText()
    {
        Assert.False(DocRecord.None.HasDocumentation);
        Assert.Null(DocRecord.None.Summary);
        Assert.Empty(DocRecord.None.Params);
    }

    [Fact]
    public void RepoRoot_Find_ReturnsFirstFolderWithSolution()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "repo"));
        var deep = Path.Combine(root, "a", "b");

        var found = RepoRoot.Find(deep, path => path == Path.Combine(root, RepoRoot.SolutionFileName));

        Assert.Equal(root, found);
    }

    [Fact]
    public void RepoRoot_Find_NoSolution_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => RepoRoot.Find(Path.GetTempPath(), _ => false));

        Assert.Contains("--solution", ex.Message, StringComparison.Ordinal);
    }
}
