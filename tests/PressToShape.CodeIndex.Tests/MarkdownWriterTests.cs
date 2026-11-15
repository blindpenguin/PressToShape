using System;
using System.Linq;
using System.Text;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class MarkdownWriterTests
{
    private static InMemoryFileSink Write(IndexData data)
    {
        var sink = new InMemoryFileSink();
        MarkdownWriter.Write(data, sink);
        return sink;
    }

    private static string Text(InMemoryFileSink sink, string path) => Encoding.UTF8.GetString(sink.Files[path]);

    [Fact]
    public void Write_TwoRunsAndShuffledInput_ProduceIdenticalBytes()
    {
        var first = Write(JsonIndexWriterTests.BuildData());
        var second = Write(JsonIndexWriterTests.BuildData(reverse: true));

        Assert.Equal(first.Files.Keys, second.Files.Keys);
        Assert.All(first.Files, pair => Assert.Equal(pair.Value, second.Files[pair.Key]));
    }

    [Fact]
    public void Write_Layout_HasReadmeAndOnePagePerNamespaceWithTypes()
    {
        var sink = Write(JsonIndexWriterTests.BuildData());

        Assert.Equal(
            ["markdown/App/App.md", "markdown/Core/A.B.md", "markdown/Core/A.md", "markdown/README.md"],
            sink.Files.Keys.OrderBy(k => k, StringComparer.Ordinal));
        var readme = Text(sink, "markdown/README.md");
        Assert.Contains("[A.B](Core/A.B.md)", readme, StringComparison.Ordinal);
        Assert.Contains("Android: workload missing", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_TypePage_ShowsHeaderSourceSummaryMembersAndRelationships()
    {
        var page = Text(Write(JsonIndexWriterTests.BuildData()), "markdown/Core/A.B.md");

        Assert.Contains("## IService (interface)", page, StringComparison.Ordinal);
        Assert.Contains("`public interface IService`", page, StringComparison.Ordinal);
        Assert.Contains("The service.", page, StringComparison.Ordinal);
        Assert.Contains("`public class Impl : IService`", page, StringComparison.Ordinal);
        Assert.Contains("- `public void Run()`", page, StringComparison.Ordinal);
        Assert.Contains("Implements `A.B.IService`", page, StringComparison.Ordinal);
        Assert.Contains("Implemented by `A.B.Impl`", page, StringComparison.Ordinal);
        Assert.Contains("Depends on (constructor) `A.B.IService`", page, StringComparison.Ordinal);
        Assert.Contains("Source: `src/Core/File0.cs:", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_UndocumentedSymbols_AreMarkedWithoutInventedText()
    {
        var page = Text(Write(JsonIndexWriterTests.BuildData()), "markdown/Core/A.B.md");

        Assert.Contains("- `public int Count { get; set; }` - _(no documentation)_", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_PrivateMembersAndTypes_AreLeftOutButInternalOnesStay()
    {
        var project = TestCompilation.Project("P", "namespace N; internal class Shown { private int _hidden; internal int Visible; private class Nested { } } public class Other { }");
        var symbols = new Extraction.SymbolExtractor(TestCompilation.RepoRoot).Extract(project);
        var page = Text(Write(new IndexData([new ProjectIndex("P", "src/P/P.csproj", symbols, [])], [])), "markdown/P/N.md");

        Assert.Contains("## Shown (class)", page, StringComparison.Ordinal);
        Assert.Contains("internal int Visible", page, StringComparison.Ordinal);
        Assert.DoesNotContain("_hidden", page, StringComparison.Ordinal);
        Assert.DoesNotContain("Nested", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_InferredEdges_AreMarked()
    {
        var data = JsonIndexWriterTests.BuildData();
        var core = data.Projects.Single(p => p.Name == "Core");
        var inferred = new RelationshipRecord(RelationshipType.Implements, "T:A.B.Consumer", "T:A.B.IService", null, null, Confidence.Inferred, "resolved by reflection");
        data = data with { Projects = data.Projects.Select(p => p == core ? p with { Relationships = p.Relationships.Append(inferred).ToList() } : p).ToList() };

        var page = Text(Write(data), "markdown/Core/A.B.md");

        Assert.Contains("(inferred: resolved by reflection)", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_ChangedSummaryInTheModel_ChangesThePage()
    {
        var data = JsonIndexWriterTests.BuildData();
        var changed = data with
        {
            Projects = data.Projects.Select(p => p with
            {
                Symbols = p.Symbols.Select(s => s.Id == "T:A.B.IService" ? s with { Doc = s.Doc with { Summary = "A different summary." } } : s).ToList(),
            }).ToList(),
        };

        var page = Text(Write(changed), "markdown/Core/A.B.md");

        Assert.Contains("A different summary.", page, StringComparison.Ordinal);
        Assert.DoesNotContain("The service.", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_AnyPage_HasNoAbsolutePathsNoCarriageReturnsNoBomAndAnotherWordForKind()
    {
        foreach (var (path, bytes) in Write(JsonIndexWriterTests.BuildData()).Files)
        {
            var text = Encoding.UTF8.GetString(bytes);

            Assert.DoesNotContain(TestCompilation.RepoRoot, text, StringComparison.Ordinal);
            Assert.DoesNotContain("\r", text, StringComparison.Ordinal);
            Assert.DoesNotContain("kind", text, StringComparison.OrdinalIgnoreCase);
            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF, path + " has a BOM");
            Assert.EndsWith("\n", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Write_GeneratedMember_ShowsItsOrigin()
    {
        var data = new IndexData(
        [
            new ProjectIndex("P", "src/P/P.csproj",
            [
                Type("T:N.VM", "VM"),
                Member("P:N.VM.Title", "Title", "string Title { get; set; }", "T:N.VM") with { IsGenerated = true, GeneratedFrom = "F:N.VM._title" },
            ], []),
        ], []);

        var page = Text(Write(data), "markdown/P/N.md");

        Assert.Contains("_(generated from `F:N.VM._title`)_", page, StringComparison.Ordinal);
    }

    private static SymbolRecord Type(string id, string name) => new(
        id, name, "N." + name, SymbolType.Class, "public", [], "N:N", "N", "P", "src/P/File0.cs", 1, 5, null, [], null, [], null, [], false, null, DocRecord.None);

    private static SymbolRecord Member(string id, string name, string signature, string containing) => new(
        id, name, "N.VM." + name, SymbolType.Property, "public", [], containing, "N", "P", "src/P/File0.cs", 2, 2, signature, [], "string", [], null, [], false, null, DocRecord.None);
}
