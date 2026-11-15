using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class IndexGeneratorTests
{
    private sealed class FakeSource(IReadOnlyList<ProjectCompilation> projects, params LoadProblem[] problems) : ICompilationSource
    {
        public Task<LoadResult> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new LoadResult(projects, problems));
    }

    private static (ProjectCompilation Core, ProjectCompilation App) TwoProjects()
    {
        var core = TestCompilation.Project("Core", "namespace Core; public interface IService { void Run(); }");
        var app = TestCompilation.Project("App", "namespace App; public class Impl : Core.IService { public void Run() { } }");
        app = app with { Compilation = app.Compilation.AddReferences(core.Compilation.ToMetadataReference()) };
        return (core, app);
    }

    private static Task<GenerateResult> Generate(ICompilationSource source) =>
        new IndexGenerator(source, TestCompilation.RepoRoot).GenerateAsync(CancellationToken.None);

    [Fact]
    public async Task GenerateAsync_TwoProjects_ExtractsSymbolsPerProjectSortedByName()
    {
        var (core, app) = TwoProjects();

        var result = await Generate(new FakeSource([app, core]));

        Assert.Equal(["App", "Core"], result.Data.Projects.Select(p => p.Name));
        Assert.Contains(result.Data.Projects[0].Symbols, s => s.Id == "T:App.Impl");
        Assert.Contains(result.Data.Projects[1].Symbols, s => s.Id == "T:Core.IService");
        Assert.Equal("src/Core/Core.csproj", result.Data.Projects[1].Path);
        Assert.False(result.HasErrors);
    }

    [Fact]
    public async Task GenerateAsync_EdgeAcrossProjects_IsRecordedInTheProjectThatHasTheSource()
    {
        var (core, app) = TwoProjects();

        var result = await Generate(new FakeSource([core, app]));

        var appEdges = result.Data.Projects.Single(p => p.Name == "App").Relationships;
        Assert.Contains(appEdges, e => e is { Type: RelationshipType.Implements, FromId: "T:App.Impl", ToId: "T:Core.IService" });
        Assert.DoesNotContain(result.Data.Projects.Single(p => p.Name == "Core").Relationships, e => e.Type == RelationshipType.Implements);
    }

    [Fact]
    public async Task GenerateAsync_ErrorProblem_SetsHasErrorsAndKeepsTheOtherProjects()
    {
        var (core, _) = TwoProjects();
        var problem = new LoadProblem("Broken", "3 compile errors", false);

        var result = await Generate(new FakeSource([core], problem));

        Assert.True(result.HasErrors);
        Assert.Equal(["Core"], result.Data.Projects.Select(p => p.Name));
        Assert.Equal([problem], result.Data.Warnings);
    }

    [Fact]
    public async Task GenerateAsync_OnlyWarningProblems_DoNotSetHasErrors()
    {
        var (core, _) = TwoProjects();
        var warning = new LoadProblem("PressToShape.Android", "workload missing", true);

        var result = await Generate(new FakeSource([core], warning));

        Assert.False(result.HasErrors);
        Assert.Equal([warning], result.Data.Warnings);
    }

    [Fact]
    public async Task Write_TwoRuns_ProduceIdenticalFilesIncludingTheProblemsInTheManifest()
    {
        var (core, app) = TwoProjects();
        var warning = new LoadProblem("PressToShape.Android", "workload missing", true);

        var first = new InMemoryFileSink();
        var second = new InMemoryFileSink();
        IndexGenerator.Write((await Generate(new FakeSource([core, app], warning))).Data, first);
        IndexGenerator.Write((await Generate(new FakeSource([app, core], warning))).Data, second);

        Assert.Equal(first.Files.Keys, second.Files.Keys);
        Assert.All(first.Files, pair => Assert.Equal(pair.Value, second.Files[pair.Key]));
        var manifest = JsonSerializer.Deserialize<IndexManifest>(first.Files["manifest.json"], JsonIndexWriter.Options)!;
        Assert.Equal("PressToShape.Android", Assert.Single(manifest.Warnings).Project);
        Assert.Contains("markdown/README.md", first.Files.Keys);
    }

    [Fact]
    public async Task Write_NoProjects_StillWritesAManifestAndEmptyNames()
    {
        var sink = new InMemoryFileSink();

        IndexGenerator.Write((await Generate(new FakeSource([]))).Data, sink);

        Assert.Equal(0, JsonSerializer.Deserialize<IndexManifest>(sink.Files["manifest.json"], JsonIndexWriter.Options)!.SymbolCount);
        Assert.Empty(sink.Files["names.tsv"]);
    }

    [Fact]
    public async Task GenerateAsync_Cancelled_Throws()
    {
        var (core, _) = TwoProjects();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<System.OperationCanceledException>(() => new IndexGenerator(new FakeSource([core]), TestCompilation.RepoRoot).GenerateAsync(cts.Token));
    }
}

public class LoadPolicyTests
{
    [Theory]
    [InlineData("src/PressToShape.Core/PressToShape.Core.csproj", true)]
    [InlineData("tests/PressToShape.Core.Tests/PressToShape.Core.Tests.csproj", true)]
    [InlineData("tools/PressToShape.CodeIndex/PressToShape.CodeIndex.csproj", false)]
    [InlineData("", false)]
    public void IsIndexed_OnlySrcAndTests(string path, bool expected) => Assert.Equal(expected, LoadPolicy.IsIndexed(path));

    [Theory]
    [InlineData("PressToShape.Android", true)]
    [InlineData("PressToShape.App", false)]
    public void IsWarning_OnlyAndroid(string project, bool expected) => Assert.Equal(expected, LoadPolicy.IsWarning(project));
}

public class PathScrubberTests
{
    [Fact]
    public void Scrub_RepoRootAndHome_AreReplaced()
    {
        var text = "Cannot open /work/repo/src/App/App.csproj and /home/someone/.nuget/x";

        var result = PathScrubber.Scrub(text, "/work/repo/", "/home/someone");

        Assert.Equal("Cannot open <repo>/src/App/App.csproj and ~/.nuget/x", result);
        Assert.DoesNotContain("someone", result, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Scrub_WindowsStyleRoot_IsReplaced()
    {
        Assert.Equal("<repo>\\src\\a.cs", PathScrubber.Scrub("C:\\work\\repo\\src\\a.cs", "C:/work/repo", "/h"));
    }

    [Fact]
    public void Scrub_NoHomeDirectory_LeavesOtherText()
    {
        Assert.Equal("plain", PathScrubber.Scrub("plain", "/repo", string.Empty));
    }
}
