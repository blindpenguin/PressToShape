using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Xaml.Model;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class XamlJsonWriterTests
{
    private const string ShellFile = "xaml/App/Views.ShellView.axaml.json";

    private static string Text(InMemoryFileSink sink, string path) => Encoding.UTF8.GetString(sink.Files[path]);

    [Fact]
    public void Write_OneJsonFilePerXamlFileUnderTheProjectFolder()
    {
        var files = XamlSampleData.Index().Files.Keys.Where(k => k.StartsWith("xaml/", StringComparison.Ordinal)).ToList();

        Assert.Equal(["xaml/App/App.axaml.json", ShellFile], files);
    }

    [Fact]
    public void Write_TwoRunsAndShuffledProjects_GiveIdenticalBytes()
    {
        var first = XamlSampleData.Index();
        var second = XamlSampleData.Index(reverse: true);

        Assert.Equal(first.Files.Keys, second.Files.Keys);
        Assert.All(first.Files, pair => Assert.Equal(pair.Value, second.Files[pair.Key]));
    }

    [Fact]
    public void Write_FileReadsBackAsTheRecordAndHasTheSchemaVersion()
    {
        var data = XamlSampleData.Data();
        var sink = new InMemoryFileSink();
        JsonIndexWriter.Write(data, sink);
        var original = data.Projects.Single(p => p.Name == "App").Xaml.Single(f => f.File == "src/App/Views/ShellView.axaml");

        var text = Text(sink, ShellFile);
        var back = JsonSerializer.Deserialize<XamlFileRecord>(text, JsonIndexWriter.Options)!;

        Assert.StartsWith($"{{\"SchemaVersion\":{IndexSchema.Version},\"Id\":\"X:src/App/Views/ShellView.axaml\"", text, StringComparison.Ordinal);
        Assert.Equal(original.Id, back.Id);
        Assert.Equal(original.Elements.Count, back.Elements.Count);
        Assert.Equal(original.Bindings.Select(b => (b.Id, b.TargetId)), back.Bindings.Select(b => (b.Id, b.TargetId)));
        Assert.Equal(original.ResourceRefs.Count, back.ResourceRefs.Count);
        Assert.Equal(original.Styles.Select(s => s.Id), back.Styles.Select(s => s.Id));
        Assert.Equal(original.Templates.Select(t => t.Id), back.Templates.Select(t => t.Id));
        Assert.Equal(original.Handlers.Select(h => h.TargetId), back.Handlers.Select(h => h.TargetId));
        Assert.Equal(original.Namespaces, back.Namespaces);
    }

    [Fact]
    public void Write_EveryListItemIsOnItsOwnValidJsonLine()
    {
        var lines = Text(XamlSampleData.Index(), ShellFile).TrimEnd('\n').Split('\n');

        var records = lines.Where(l => l.StartsWith('{') && l.Contains("\"Id\"", StringComparison.Ordinal) && !l.Contains("SchemaVersion", StringComparison.Ordinal)).ToList();

        Assert.NotEmpty(records);
        foreach (var line in records)
        {
            using var document = JsonDocument.Parse(line.TrimEnd(','));
            Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
        }

        Assert.Contains(lines, l => l.StartsWith(",\"Elements\":[", StringComparison.Ordinal) || l.Contains(",\"Elements\":[", StringComparison.Ordinal));
    }

    [Fact]
    public void Write_ListsAreSortedByLineAndEmptyListsAreWrittenAsEmptyArrays()
    {
        var text = Text(XamlSampleData.Index(), "xaml/App/App.axaml.json");

        Assert.Contains("\"Bindings\":[]", text, StringComparison.Ordinal);
        Assert.Contains("\"Handlers\":[]", text, StringComparison.Ordinal);
        var shell = JsonSerializer.Deserialize<XamlFileRecord>(Text(XamlSampleData.Index(), ShellFile), JsonIndexWriter.Options)!;
        Assert.Equal(shell.Elements.Select(e => e.Line).OrderBy(l => l), shell.Elements.Select(e => e.Line));
    }

    [Fact]
    public void Write_AnyXamlFile_HasNoAbsolutePathsNoCarriageReturnsAndNoBom()
    {
        foreach (var (path, bytes) in XamlSampleData.Index().Files.Where(f => f.Key.StartsWith("xaml/", StringComparison.Ordinal)))
        {
            var text = Encoding.UTF8.GetString(bytes);

            Assert.DoesNotContain(TestCompilation.RepoRoot, text, StringComparison.Ordinal);
            Assert.DoesNotContain("\r", text, StringComparison.Ordinal);
            Assert.False(bytes[0] == 0xEF, path + " has a BOM");
            Assert.EndsWith("}\n", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Write_NamesTable_HasEntriesForViewsElementsResourcesStylesAndTemplates()
    {
        var lines = Text(XamlSampleData.Index(), "names.tsv").Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Split('\t')).ToList();

        var view = Assert.Single(lines, l => l[1] == "X:src/App/Views/ShellView.axaml");
        Assert.Equal(["ShellView", "X:src/App/Views/ShellView.axaml", ShellFile, "View", "src/App/Views/ShellView.axaml:1"], view);
        Assert.Contains(lines, l => l[0] == "TitleText" && l[3] == "Element" && l[4] == "src/App/Views/ShellView.axaml:5");
        Assert.Contains(lines, l => l[0] == "Gesture" && l[3] == "Resource" && l[2] == ShellFile);
        Assert.Contains(lines, l => l[0] == "Button.icon" && l[3] == "Style");
        Assert.Contains(lines, l => l[0] == "RowTemplate" && l[3] == "Template");
        Assert.Equal(2, lines.Count(l => l[0] == "WindowBrush" && l[3] == "Resource"));
        Assert.All(lines, l => Assert.Equal(5, l.Length));
    }

    [Fact]
    public void Write_NamesTable_StaysSortedAndKeepsCSharpEntries()
    {
        var lines = Text(XamlSampleData.Index(), "names.tsv").Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Split('\t')).ToList();

        Assert.Equal(lines.OrderBy(l => l[0], StringComparer.Ordinal).ThenBy(l => l[1], StringComparer.Ordinal).Select(l => l[1]), lines.Select(l => l[1]));
        Assert.Contains(lines, l => l[1] == "T:App.Views.ShellView" && l[3] == "Class");
    }

    [Fact]
    public void Write_Manifest_CountsXamlFilesPerProject()
    {
        var manifest = JsonSerializer.Deserialize<IndexManifest>(XamlSampleData.Index().Files["manifest.json"], JsonIndexWriter.Options)!;

        Assert.Equal(2, manifest.Projects.Single(p => p.Name == "App").XamlFileCount);
        Assert.Equal(0, manifest.Projects.Single(p => p.Name == "Lib").XamlFileCount);
        Assert.Equal(IndexSchema.Version, manifest.SchemaVersion);
    }

    [Theory]
    [InlineData("App", "src/App", "src/App/Views/Shell View.axaml", "xaml/App/Views.Shell_View.axaml.json")]
    [InlineData("App", "src/App", "src/App/App.axaml", "xaml/App/App.axaml.json")]
    [InlineData("My App", "src/My App", "src/My App/A/B.axaml", "xaml/My_App/A.B.axaml.json")]
    [InlineData("App", "", "x/y.axaml", "xaml/App/x.y.axaml.json")]
    [InlineData("App", "src/App", "tests/Other/z.axaml", "xaml/App/tests.Other.z.axaml.json")]
    public void XamlPath_IsRelativeToTheProjectFolderAndSafe(string project, string directory, string file, string expected)
    {
        var path = IndexLayout.XamlPath(project, directory, file);

        Assert.Equal(expected, path);
        Assert.Equal(path, IndexPath.Normalize(path));
    }

    [Fact]
    public void Write_ProjectWithoutXaml_WritesNoXamlFiles()
    {
        var data = new IndexData([new ProjectIndex("P", "src/P/P.csproj", [], [])], []);
        var sink = new InMemoryFileSink();

        JsonIndexWriter.Write(data, sink);

        Assert.DoesNotContain(sink.Files.Keys, k => k.StartsWith("xaml/", StringComparison.Ordinal));
        Assert.Empty(sink.Files["names.tsv"]);
    }
}
