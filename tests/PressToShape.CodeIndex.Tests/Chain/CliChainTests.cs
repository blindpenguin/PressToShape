using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Cli;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class CliChainTests
{
    private sealed class FakeProjects(params ProjectCompilation[] projects) : ICompilationSource
    {
        public Task<LoadResult> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new LoadResult(projects, []));
    }

    private sealed record Harness(StringWriter Out, StringWriter Err, Func<string[], Task<int>> Run);

    private static async Task<Harness> Start(bool generate = true)
    {
        var folder = new MemoryIndexFolder();
        var output = new StringWriter();
        var error = new StringWriter();
        var services = new CliServices(
            _ => TestCompilation.RepoRoot,
            (_, _) => new FakeProjects(MvvmSampleData.Projects()),
            _ => folder,
            _ => MvvmSampleData.Files());
        Task<int> Run(string[] args) => new CliApp(output, error, TestCompilation.RepoRoot, services).RunAsync(args, CancellationToken.None);

        if (generate)
        {
            Assert.Equal(CliApp.Success, await Run(["generate"]));
            output.GetStringBuilder().Clear();
            error.GetStringBuilder().Clear();
        }

        return new Harness(output, error, Run);
    }

    [Fact]
    public async Task Chain_View_PrintsAnIndentedTreeWithConfidenceAndLocations()
    {
        var h = await Start();

        var code = await h.Run(["chain", "ShellView"]);

        var lines = h.Out.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(CliApp.Success, code);
        Assert.StartsWith("View ShellView  src/App/File1.cs:", lines[0], StringComparison.Ordinal);
        Assert.EndsWith("T:App.Views.ShellView", lines[0], StringComparison.Ordinal);
        Assert.Contains("  XAML file src/App/Views/ShellView.axaml [verified]  src/App/Views/ShellView.axaml:1 - x:Class", lines);
        Assert.Contains(lines, l => l.StartsWith("  View model ShellViewModel [verified]", StringComparison.Ordinal) && l.EndsWith(" - x:DataType", StringComparison.Ordinal));
        Assert.Contains(lines, l => l.StartsWith("      Button.Command = Binding SaveCommand [verified]", StringComparison.Ordinal));
        Assert.Contains(lines, l => l.StartsWith("        Handler Save [verified]", StringComparison.Ordinal) && l.EndsWith("- [RelayCommand]", StringComparison.Ordinal));
        Assert.Contains(lines, l => l.TrimStart().StartsWith("Handler not statically known - not statically known:", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Chain_View_ShowsInferredLinksWithTheirReason()
    {
        var h = await Start();

        await h.Run(["chain", "ShellView"]);

        var line = Array.Find(h.Out.ToString().Split('\n'), l => l.Contains("Child view DetailView", StringComparison.Ordinal))!;
        Assert.Contains("[inferred]", line, StringComparison.Ordinal);
        Assert.Contains("ViewLocator (registered in Application.DataTemplates", line, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Chain_ViewModelAndCommand_PrintTheirTrees()
    {
        var h = await Start();

        await h.Run(["chain", "ShellViewModel"]);
        var viewModel = h.Out.ToString();
        h.Out.GetStringBuilder().Clear();
        await h.Run(["chain", "SaveCommand"]);

        Assert.StartsWith("View model ShellViewModel", viewModel, StringComparison.Ordinal);
        Assert.Contains("  Views using it (1)", viewModel, StringComparison.Ordinal);
        Assert.StartsWith("Command ShellViewModel.SaveCommand", h.Out.ToString(), StringComparison.Ordinal);
        Assert.Contains("  Bound at (1)", h.Out.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Chain_Json_IsAValidTreeWithTheSameSections()
    {
        var h = await Start();

        var code = await h.Run(["chain", "ShellView", "--json"]);

        using var document = JsonDocument.Parse(h.Out.ToString());
        Assert.Equal(CliApp.Success, code);
        Assert.Equal("View ShellView", document.RootElement.GetProperty("Label").GetString());
        Assert.Equal("T:App.Views.ShellView", document.RootElement.GetProperty("Id").GetString());
        Assert.True(document.RootElement.GetProperty("Children").GetArrayLength() >= 3);
    }

    [Fact]
    public async Task Chain_AmbiguousUnknownAndNotSupported_ExitWithOne()
    {
        var h = await Start();

        var ambiguous = await h.Run(["chain", "Save"]);
        var ambiguousText = h.Out.ToString();
        h.Out.GetStringBuilder().Clear();
        var unknown = await h.Run(["chain", "Nope"]);
        var unknownText = h.Out.ToString();
        h.Out.GetStringBuilder().Clear();
        var unsupported = await h.Run(["chain", "Title"]);

        Assert.Equal((CliApp.Failure, CliApp.Failure, CliApp.Failure), (ambiguous, unknown, unsupported));
        Assert.Contains("'Save' is ambiguous", ambiguousText, StringComparison.Ordinal);
        Assert.Contains("M:App.ViewModels.ShellViewModel.Save", ambiguousText, StringComparison.Ordinal);
        Assert.Contains("No symbol named 'Nope'. Try: find Nope", unknownText, StringComparison.Ordinal);
        Assert.Contains("chain accepts a view", h.Out.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Chain_JsonForAProblem_IsValidJsonWithTheStatus()
    {
        var h = await Start();

        var code = await h.Run(["chain", "Nope", "--json"]);

        using var document = JsonDocument.Parse(h.Out.ToString());
        Assert.Equal(CliApp.Failure, code);
        Assert.Equal("NotFound", document.RootElement.GetProperty("Status").GetString());
    }

    [Theory]
    [InlineData("--depth")]
    [InlineData("--depth", "0")]
    [InlineData("--depth", "6")]
    [InlineData("--depth", "two")]
    [InlineData("--depth", "-1")]
    public async Task Chain_BadDepth_IsAUsageError(params string[] depth)
    {
        var h = await Start();

        var code = await h.Run(["chain", "ShellView", .. depth]);

        Assert.Equal(CliApp.UsageError, code);
        Assert.Contains("--depth needs a number from 1 to 5", h.Err.ToString(), StringComparison.Ordinal);
        Assert.Empty(h.Out.ToString());
    }

    [Fact]
    public async Task Chain_Depth_ExpandsChildViews()
    {
        var h = await Start();

        await h.Run(["chain", "ShellView"]);
        var shallow = h.Out.ToString();
        h.Out.GetStringBuilder().Clear();
        var code = await h.Run(["chain", "ShellView", "--depth", "2"]);

        Assert.Equal(CliApp.Success, code);
        Assert.DoesNotContain("XAML file src/App/Views/PanelView.axaml", shallow, StringComparison.Ordinal);
        Assert.Contains("XAML file src/App/Views/PanelView.axaml", h.Out.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Chain_WithoutAnArgument_IsAUsageError()
    {
        var h = await Start();

        Assert.Equal(CliApp.UsageError, await h.Run(["chain"]));
        Assert.Contains("needs a name", h.Err.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Chain_WithoutAnIndex_SaysHowToCreateIt()
    {
        var h = await Start(generate: false);

        var code = await h.Run(["chain", "ShellView"]);

        Assert.Equal(CliApp.Failure, code);
        Assert.Contains("generate", h.Err.ToString(), StringComparison.Ordinal);
        Assert.Empty(h.Out.ToString());
    }

    [Fact]
    public async Task Help_ListsChainAndTheDepthOption()
    {
        var h = await Start();

        await h.Run(["--help"]);

        Assert.Contains("chain <name|id>", h.Out.ToString(), StringComparison.Ordinal);
        Assert.Contains("--depth <1-5>", h.Out.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Chain_DoesNotCompileTheSolution()
    {
        var folder = new MemoryIndexFolder();
        var compiled = 0;
        var services = new CliServices(
            _ => TestCompilation.RepoRoot,
            (_, _) =>
            {
                compiled++;
                return new FakeProjects(MvvmSampleData.Projects());
            },
            _ => folder,
            _ => MvvmSampleData.Files());
        var app = new CliApp(new StringWriter(), new StringWriter(), TestCompilation.RepoRoot, services);
        await app.RunAsync(["generate"], CancellationToken.None);
        var afterGenerate = compiled;

        await app.RunAsync(["chain", "ShellView"], CancellationToken.None);
        await app.RunAsync(["chain", "ShellViewModel", "--depth", "3"], CancellationToken.None);

        Assert.Equal(afterGenerate, compiled);
    }
}
