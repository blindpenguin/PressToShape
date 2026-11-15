using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Output;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class AvaloniaPagesTests
{
    private static string Page(string name, bool reverse = false) =>
        Encoding.UTF8.GetString(MvvmSampleData.Index(reverse).Files["markdown/avalonia/" + name]);

    [Fact]
    public void Views_ListsViewsWithViewModelsEvidenceAndCounts()
    {
        var page = Page("views.md");

        Assert.Contains("### ShellView", page, StringComparison.Ordinal);
        Assert.Contains("- view model `ShellViewModel` (`T:App.ViewModels.ShellViewModel`), verified: x:DataType", page, StringComparison.Ordinal);
        Assert.Contains("3 commands, 1 test classes", page, StringComparison.Ordinal);
        Assert.Contains("XAML: `src/App/Views/ShellView.axaml`", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Views_MarksInferredLinksWithTheirReason()
    {
        var page = Page("views.md");

        var line = page.Split('\n').First(l => l.Contains("`DetailViewModel`", StringComparison.Ordinal) && l.Contains("inferred", StringComparison.Ordinal));
        Assert.Contains("ViewLocator", line, StringComparison.Ordinal);
    }

    private sealed class FakeProjects(params ProjectCompilation[] projects) : ICompilationSource
    {
        public Task<LoadResult> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new LoadResult(projects, []));
    }

    [Fact]
    public async Task Views_DoNotListTheApplicationClass()
    {
        const string Source = "namespace App { public partial class MyApp : Avalonia.Application { } }";
        var xaml = new InMemoryXamlFileSource().Add(
            "src/App/App.axaml",
            "<Application xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" x:Class=\"App.MyApp\"></Application>");
        var data = (await new IndexGenerator(new FakeProjects(XamlTestHost.AppProject("App", Source)), TestCompilation.RepoRoot, xaml).GenerateAsync(CancellationToken.None)).Data;
        var sink = new InMemoryFileSink();
        IndexGenerator.Write(data, sink);

        var page = Encoding.UTF8.GetString(sink.Files["markdown/avalonia/views.md"]);

        Assert.DoesNotContain("MyApp", page, StringComparison.Ordinal);
        Assert.Contains("## Views\n\nnone found\n", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Views_ListsViewModelsWithoutAViewAndSaysNoneForViewsWithoutAViewModel()
    {
        var page = Page("views.md");

        Assert.Contains("## View models without a view\n", page, StringComparison.Ordinal);
        Assert.Contains("`OrphanViewModel` (`T:App.ViewModels.OrphanViewModel`)", page, StringComparison.Ordinal);
        Assert.DoesNotContain("`PanelViewModel` (`T:App.ViewModels.PanelViewModel`) at", page, StringComparison.Ordinal);
        Assert.EndsWith("## Views without a view model\n\nnone found\n", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Controls_ListsPropertiesAndSaysNoneFoundForEvents()
    {
        var page = Page("controls.md");

        Assert.Contains("## Ruler", page, StringComparison.Ordinal);
        Assert.Contains("A ruler along the canvas.", page, StringComparison.Ordinal);
        Assert.Contains("- `Zoom` (Styled, `double`, default 100)", page, StringComparison.Ordinal);
        Assert.Contains("**Events**\n\nnone found\n", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Converters_ShowUsesAndMarkAnUnusedConverter()
    {
        var page = Page("converters.md");

        var flag = page[page.IndexOf("## FlagConverter", StringComparison.Ordinal)..page.IndexOf("## UnusedConverter", StringComparison.Ordinal)];
        Assert.Contains("Formats flags.", flag, StringComparison.Ordinal);
        Assert.Contains("- used at src/App/Views/ShellView.axaml:", flag, StringComparison.Ordinal);
        Assert.EndsWith("not used\n", page, StringComparison.Ordinal);
        Assert.Contains("_(no documentation)_", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Readme_LinksTheAvaloniaPages()
    {
        var readme = Encoding.UTF8.GetString(MvvmSampleData.Index().Files[IndexLayout.MarkdownReadmePath]);

        Assert.Contains("- [Views and view models](avalonia/views.md)", readme, StringComparison.Ordinal);
        Assert.Contains("- [Value converters](avalonia/converters.md)", readme, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("views.md")]
    [InlineData("controls.md")]
    [InlineData("converters.md")]
    public void Pages_AreDeterministicAndFreeOfAbsolutePathsAndTheWordKind(string name)
    {
        var page = Page(name);

        Assert.Equal(page, Page(name, reverse: true));
        Assert.Equal(page, Page(name));
        Assert.DoesNotContain("/home/", page, StringComparison.Ordinal);
        Assert.DoesNotContain("kind", page, StringComparison.OrdinalIgnoreCase);
        Assert.True(page.Split('\n').Length < 400);
    }

    [Fact]
    public void Pages_WithoutFacts_SayNoneFound()
    {
        var sink = new InMemoryFileSink();
        AvaloniaPagesWriterAccess.Write(new PressToShape.CodeIndex.Model.IndexData([], []), sink);

        Assert.Contains("none found", Encoding.UTF8.GetString(sink.Files["markdown/avalonia/controls.md"]), StringComparison.Ordinal);
        Assert.Contains("none found", Encoding.UTF8.GetString(sink.Files["markdown/avalonia/converters.md"]), StringComparison.Ordinal);
        Assert.Contains("none found", Encoding.UTF8.GetString(sink.Files["markdown/avalonia/views.md"]), StringComparison.Ordinal);
    }
}

internal static class AvaloniaPagesWriterAccess
{
    public static void Write(PressToShape.CodeIndex.Model.IndexData data, IIndexFileSink sink) =>
        PressToShape.CodeIndex.Output.Avalonia.AvaloniaPagesWriter.Write(data, sink);
}
