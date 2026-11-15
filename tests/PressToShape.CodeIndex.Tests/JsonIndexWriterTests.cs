using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Relationships;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class JsonIndexWriterTests
{
    private const string Source = """
        namespace A.B
        {
            /// <summary>The service.</summary>
            public interface IService { void Run(); }
            public class Impl : IService { public void Run() { } public int Count { get; set; } }
            public class Consumer { public Consumer(IService s) { s.Run(); } }
        }
        namespace A { public class Root { } }
        """;

    internal static IndexData BuildData(bool reverse = false)
    {
        var project = TestCompilation.Project("Core", Source);
        var extractor = new SymbolExtractor(TestCompilation.RepoRoot);
        var symbols = extractor.Extract(project);
        var edges = new RelationshipBuilder(symbols.Select(s => s.Id).ToHashSet(), TestCompilation.RepoRoot).Build(project);
        var other = TestCompilation.Project("App", "namespace App { public class Main { } }");
        var otherSymbols = extractor.Extract(other);

        IReadOnlyList<ProjectIndex> projects =
        [
            new ProjectIndex("Core", "src/Core/Core.csproj", symbols, edges),
            new ProjectIndex("App", "src/App/App.csproj", otherSymbols, []),
        ];
        if (reverse)
        {
            projects = projects.Reverse().Select(p => p with { Symbols = p.Symbols.Reverse().ToList(), Relationships = p.Relationships.Reverse().ToList() }).ToList();
        }

        return new IndexData(projects, [new LoadProblem("Android", "workload missing", true)]);
    }

    private static InMemoryFileSink Write(IndexData data)
    {
        var sink = new InMemoryFileSink();
        JsonIndexWriter.Write(data, sink);
        return sink;
    }

    [Fact]
    public void Write_TwoRuns_ProduceIdenticalBytes()
    {
        var first = Write(BuildData());
        var second = Write(BuildData());

        Assert.Equal(first.Files.Keys, second.Files.Keys);
        Assert.All(first.Files, pair => Assert.Equal(pair.Value, second.Files[pair.Key]));
    }

    [Fact]
    public void Write_InputInAnotherOrder_ProducesIdenticalBytes()
    {
        var normal = Write(BuildData());
        var shuffled = Write(BuildData(reverse: true));

        Assert.Equal(normal.Files.Keys, shuffled.Files.Keys);
        Assert.All(normal.Files, pair => Assert.Equal(pair.Value, shuffled.Files[pair.Key]));
    }

    [Fact]
    public void Write_Layout_HasManifestNamesOneFilePerNamespaceAndRelationshipFilePerProject()
    {
        var files = Write(BuildData()).Files.Keys.ToList();

        Assert.Contains("manifest.json", files);
        Assert.Contains("names.tsv", files);
        Assert.Contains("symbols/Core/A.B.json", files);
        Assert.Contains("symbols/Core/A.json", files);
        Assert.Contains("symbols/App/App.json", files);
        Assert.Contains("relationships/Core.json", files);
        Assert.Contains("relationships/App.json", files);
    }

    [Fact]
    public void Write_Manifest_HasSchemaVersionToolVersionCountsAndWarnings()
    {
        var data = BuildData();
        var manifest = JsonSerializer.Deserialize<IndexManifest>(Write(data).Files["manifest.json"], JsonIndexWriter.Options)!;

        Assert.Equal(IndexSchema.Version, manifest.SchemaVersion);
        Assert.Equal(IndexSchema.ToolVersion, manifest.ToolVersion);
        Assert.Equal(["App", "Core"], manifest.Projects.Select(p => p.Name));
        Assert.Equal(data.Projects.Sum(p => p.Symbols.Count), manifest.SymbolCount);
        Assert.Equal(data.Projects.Sum(p => p.Relationships.Count), manifest.RelationshipCount);
        var warning = Assert.Single(manifest.Warnings);
        Assert.True(warning.IsWarning);
    }

    [Fact]
    public void Write_SymbolAndRelationshipFiles_CarrySchemaVersionAndSortedContent()
    {
        var files = Write(BuildData()).Files;

        var symbols = JsonSerializer.Deserialize<SymbolsFile>(files["symbols/Core/A.B.json"], JsonIndexWriter.Options)!;
        var relationships = JsonSerializer.Deserialize<RelationshipsFile>(files["relationships/Core.json"], JsonIndexWriter.Options)!;

        Assert.Equal(IndexSchema.Version, symbols.SchemaVersion);
        Assert.Equal("A.B", symbols.Namespace);
        Assert.Equal(symbols.Symbols.Select(s => s.Id).OrderBy(i => i, StringComparer.Ordinal), symbols.Symbols.Select(s => s.Id));
        Assert.All(symbols.Symbols, s => Assert.Equal("A.B", s.Namespace));
        Assert.Equal(IndexSchema.Version, relationships.SchemaVersion);
        Assert.NotEmpty(relationships.Relationships);
    }

    [Fact]
    public void Write_AnyFile_HasNoAbsolutePathsNoMachineNamesNoTimestampsNoBom()
    {
        var files = Write(BuildData()).Files;
        var machine = Environment.MachineName;
        var user = Environment.UserName;

        foreach (var (path, bytes) in files)
        {
            var text = Encoding.UTF8.GetString(bytes);
            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF, path + " has a BOM");
            Assert.DoesNotContain(TestCompilation.RepoRoot, text, StringComparison.Ordinal);
            Assert.DoesNotContain(System.IO.Path.GetTempPath(), text, StringComparison.Ordinal);
            Assert.DoesNotContain("\r", text, StringComparison.Ordinal);
            Assert.EndsWith("\n", text, StringComparison.Ordinal);
            Assert.False(machine.Length > 3 && text.Contains(machine, StringComparison.OrdinalIgnoreCase), path + " contains the machine name");
            Assert.False(user.Length > 3 && text.Contains(user, StringComparison.OrdinalIgnoreCase), path + " contains the user name");
        }
    }

    [Fact]
    public void Write_Enums_AreNamesAndHtmlCharactersStayReadable()
    {
        var core = Encoding.UTF8.GetString(Write(BuildData()).Files["symbols/Core/A.B.json"]);

        Assert.Contains("\"SymbolType\":\"Interface\"", core, StringComparison.Ordinal);
        Assert.DoesNotContain("\\u003C", core, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_NamesTable_HasOneLinePerTypeAndMemberWithoutNamespacesAndConstructors()
    {
        var text = Encoding.UTF8.GetString(Write(BuildData()).Files["names.tsv"]);
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var impl = lines.Single(l => l.StartsWith("Impl\t", StringComparison.Ordinal)).Split('\t');

        Assert.Equal("T:A.B.Impl", impl[1]);
        Assert.Equal("symbols/Core/A.B.json", impl[2]);
        Assert.Equal("Class", impl[3]);
        Assert.StartsWith("src/Core/File0.cs:", impl[4], StringComparison.Ordinal);
        Assert.DoesNotContain(lines, l => l.Split('\t')[3] is "Namespace" or "Constructor");
        Assert.Equal(lines.OrderBy(l => l.Split('\t')[0], StringComparer.Ordinal).ThenBy(l => l.Split('\t')[1], StringComparer.Ordinal), lines);
        Assert.All(lines, l => Assert.Equal(5, l.Split('\t').Length));
    }

    [Fact]
    public void Write_LargeFiles_HaveOneRecordPerLineAndStayValidJson()
    {
        var files = Write(BuildData()).Files;

        var symbolText = Encoding.UTF8.GetString(files["symbols/Core/A.B.json"]);
        var relationshipText = Encoding.UTF8.GetString(files["relationships/Core.json"]);
        var symbols = JsonSerializer.Deserialize<SymbolsFile>(symbolText, JsonIndexWriter.Options)!;
        var relationships = JsonSerializer.Deserialize<RelationshipsFile>(relationshipText, JsonIndexWriter.Options)!;

        Assert.Equal(symbols.Symbols.Count + 2, symbolText.TrimEnd('\n').Split('\n').Length);
        Assert.Equal(relationships.Relationships.Count + 2, relationshipText.TrimEnd('\n').Split('\n').Length);
        Assert.StartsWith("{\"SchemaVersion\":1,\"Project\":\"Core\",\"Namespace\":\"A.B\",\"Symbols\":[\n{", symbolText, StringComparison.Ordinal);
        Assert.EndsWith("\n]}\n", symbolText, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_EmptyList_StillWritesValidJson()
    {
        var data = new IndexData([new ProjectIndex("Empty", "src/Empty/Empty.csproj", [], [])], []);

        var text = Encoding.UTF8.GetString(Write(data).Files["relationships/Empty.json"]);

        Assert.Equal("{\"SchemaVersion\":1,\"Project\":\"Empty\",\"Relationships\":[]}\n", text);
        Assert.Empty(JsonSerializer.Deserialize<RelationshipsFile>(text, JsonIndexWriter.Options)!.Relationships);
    }

    [Theory]
    [InlineData("My Project", "", "symbols/My_Project/_global.json")]
    [InlineData("P", "A.B", "symbols/P/A.B.json")]
    [InlineData("../P", "N/S", "symbols/.._P/N_S.json")]
    [InlineData("..", ".", "symbols/__/_.json")]
    public void IndexLayout_SymbolsPath_KeepsNamesInsideTheirFolder(string project, string ns, string expected)
    {
        Assert.Equal(expected, IndexLayout.SymbolsPath(project, ns));
        Assert.Equal(expected, IndexPath.Normalize(expected));
    }
}
