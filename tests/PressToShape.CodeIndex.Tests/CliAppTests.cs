using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Cli;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class CliAppTests
{
    private const string Source = """
        namespace N;
        /// <summary>Does the work.</summary>
        public interface IService { void Run(); }
        public class Impl : IService { public void Run() { } }
        public class Consumer { public Consumer(IService service) { service.Run(); } }
        public class Twin { } 
        public class Other { public class Twin { } }
        """;

    private sealed class FakeFolder : IIndexFolder
    {
        public SortedDictionary<string, byte[]> Files { get; } = new(StringComparer.Ordinal);

        public List<string> Writes { get; } = [];

        public int RemoveStaleCalls { get; private set; }

        /// <summary>Thrown by <see cref="Read"/> and <see cref="ListFiles"/>, to test a folder that cannot be read.</summary>
        public Exception? ReadFailure { get; set; }

        /// <summary>Thrown by <see cref="Write"/>, to test a folder that cannot be written.</summary>
        public Exception? WriteFailure { get; set; }

        private readonly HashSet<string> _written = [];

        public void Write(string relativePath, byte[] content)
        {
            if (WriteFailure is not null)
            {
                throw WriteFailure;
            }

            Files[relativePath] = content;
            Writes.Add(relativePath);
            _written.Add(relativePath);
        }

        public byte[]? Read(string relativePath) => ReadFailure is null ? Files.GetValueOrDefault(relativePath) : throw ReadFailure;

        public IReadOnlyList<string> ListFiles() => ReadFailure is null ? Files.Keys.ToList() : throw ReadFailure;

        public IReadOnlyList<string> RemoveStale()
        {
            RemoveStaleCalls++;
            var stale = Files.Keys.Where(k => !_written.Contains(k)).ToList();
            foreach (var key in stale)
            {
                Files.Remove(key);
            }

            return stale;
        }
    }

    private sealed class FakeSource(IReadOnlyList<ProjectCompilation> projects, IReadOnlyList<LoadProblem> problems) : ICompilationSource
    {
        public Task<LoadResult> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new LoadResult(projects, problems));
    }

    private sealed class Harness
    {
        public FakeFolder Folder { get; init; } = new();

        public StringWriter Out { get; } = new();

        public StringWriter Err { get; } = new();

        public List<(string Solution, string Root)> SourceRequests { get; } = [];

        public IReadOnlyList<LoadProblem> Problems { get; init; } = [];

        public string Code { get; init; } = Source;

        public async Task<int> Run(params string[] args)
        {
            var services = new CliServices(
                _ => TestCompilation.RepoRoot,
                (solution, root) =>
                {
                    SourceRequests.Add((solution, root));
                    return new FakeSource([TestCompilation.Project("Core", Code)], Problems);
                },
                _ => Folder);
            return await new CliApp(Out, Err, TestCompilation.RepoRoot, services).RunAsync(args, CancellationToken.None);
        }

        public string[] OutLines => Out.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }

    private static async Task<Harness> Generated(Harness? harness = null)
    {
        harness ??= new Harness();
        Assert.Equal(CliApp.Success, await harness.Run("generate"));
        harness.Out.GetStringBuilder().Clear();
        harness.Err.GetStringBuilder().Clear();
        return harness;
    }

    [Fact]
    public async Task Help_ListsEveryCommandAndOptionAndExitsWithSuccess()
    {
        var harness = new Harness();

        var code = await harness.Run("--help");

        Assert.Equal(CliApp.Success, code);
        foreach (var word in new[] { "generate", "check", "find", "show", "impls", "derived", "refs", "deps", "--json", "--solution", "--help" })
        {
            Assert.Contains(word, harness.Out.ToString(), StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData]
    [InlineData("frobnicate")]
    [InlineData("find")]
    [InlineData("generate", "extra")]
    [InlineData("find", "a", "b")]
    [InlineData("generate", "--nope")]
    [InlineData("generate", "--solution")]
    public async Task UsageErrors_ExitWithTwoAndPrintUsageToStderr(params string[] args)
    {
        var harness = new Harness();

        var code = await harness.Run(args);

        Assert.Equal(CliApp.UsageError, code);
        Assert.Contains("Usage:", harness.Err.ToString(), StringComparison.Ordinal);
        Assert.Empty(harness.Out.ToString());
        Assert.Empty(harness.Folder.Writes);
    }

    [Fact]
    public async Task Generate_WritesTheIndexReportsCountsAndRemovesStaleFiles()
    {
        var harness = new Harness();
        harness.Folder.Files["old/removed.json"] = [1];

        var code = await harness.Run("generate");

        Assert.Equal(CliApp.Success, code);
        Assert.Contains("manifest.json", harness.Folder.Files.Keys);
        Assert.Contains("names.tsv", harness.Folder.Files.Keys);
        Assert.DoesNotContain("old/removed.json", harness.Folder.Files.Keys);
        Assert.Contains("Indexed 1 projects", harness.Out.ToString(), StringComparison.Ordinal);
        Assert.Contains("Removed 1 files", harness.Out.ToString(), StringComparison.Ordinal);
        Assert.Equal(1, harness.Folder.RemoveStaleCalls);
    }

    [Fact]
    public async Task Generate_ProjectError_WritesNothingKeepsOldFilesAndFails()
    {
        var harness = new Harness { Problems = [new LoadProblem("Broken", "3 compile errors", false)] };
        harness.Folder.Files["manifest.json"] = [1];

        var code = await harness.Run("generate");

        Assert.Equal(CliApp.Failure, code);
        Assert.Empty(harness.Folder.Writes);
        Assert.Equal(0, harness.Folder.RemoveStaleCalls);
        Assert.Equal([1], harness.Folder.Files["manifest.json"]);
        Assert.Contains("Error: Broken: 3 compile errors", harness.Err.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Generate_AndroidWarning_StillSucceedsAndIsPrinted()
    {
        var harness = new Harness { Problems = [new LoadProblem("PressToShape.Android", "workload missing", true)] };

        var code = await harness.Run("generate");

        Assert.Equal(CliApp.Success, code);
        Assert.Contains("Warning: PressToShape.Android: workload missing", harness.Err.ToString(), StringComparison.Ordinal);
        var manifest = JsonSerializer.Deserialize<IndexManifest>(harness.Folder.Files["manifest.json"], JsonIndexWriter.Options)!;
        Assert.Single(manifest.Warnings);
    }

    [Fact]
    public async Task Generate_SolutionOption_IsResolvedAgainstTheStartFolder()
    {
        var harness = new Harness();

        await harness.Run("generate", "--solution", Path.Combine("sub", "My.sln"));

        var request = Assert.Single(harness.SourceRequests);
        Assert.Equal(Path.Combine(TestCompilation.RepoRoot, "sub", "My.sln"), request.Solution);
        Assert.Equal(Path.Combine(TestCompilation.RepoRoot, "sub"), request.Root);
    }

    [Fact]
    public async Task Generate_Default_UsesTheSolutionAtTheRepoRoot()
    {
        var harness = new Harness();

        await harness.Run("generate");

        Assert.Equal((Path.Combine(TestCompilation.RepoRoot, "PressToShape.sln"), TestCompilation.RepoRoot), Assert.Single(harness.SourceRequests));
    }

    [Fact]
    public async Task Check_FreshIndex_ReportsUpToDateAndWritesNothing()
    {
        var harness = await Generated();
        var writesBefore = harness.Folder.Writes.Count;

        var code = await harness.Run("check");

        Assert.Equal(CliApp.Success, code);
        Assert.Contains("up to date", harness.Out.ToString(), StringComparison.Ordinal);
        Assert.Equal(writesBefore, harness.Folder.Writes.Count);
    }

    [Fact]
    public async Task Check_ChangedMissingAndStaleFiles_AreListed()
    {
        var harness = await Generated();
        harness.Folder.Files["names.tsv"] = Encoding.UTF8.GetBytes("changed\n");
        harness.Folder.Files.Remove("manifest.json");
        harness.Folder.Files["symbols/Old/Gone.json"] = [1];
        var writesBefore = harness.Folder.Writes.Count;

        var code = await harness.Run("check");

        Assert.Equal(CliApp.Failure, code);
        Assert.Contains("changed: names.tsv", harness.OutLines);
        Assert.Contains("missing: manifest.json", harness.OutLines);
        Assert.Contains("stale: symbols/Old/Gone.json", harness.OutLines);
        Assert.Contains("generate", harness.Out.ToString(), StringComparison.Ordinal);
        Assert.Equal(writesBefore, harness.Folder.Writes.Count);
        Assert.Equal(1, harness.Folder.RemoveStaleCalls);
    }

    [Fact]
    public async Task Check_ProjectError_FailsWithoutComparing()
    {
        var harness = new Harness { Problems = [new LoadProblem("Broken", "boom", false)] };

        var code = await harness.Run("check");

        Assert.Equal(CliApp.Failure, code);
        Assert.Contains("cannot be checked", harness.Err.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Find_PrintsOneLinePerHit()
    {
        var harness = await Generated();

        var code = await harness.Run("find", "Consumer");

        Assert.Equal(CliApp.Success, code);
        Assert.Contains(harness.OutLines, l => l.StartsWith("Consumer [Class] src/Core/File0.cs:", StringComparison.Ordinal) && l.EndsWith("T:N.Consumer", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Find_NoMatch_SaysSoAndSucceeds()
    {
        var harness = await Generated();

        var code = await harness.Run("find", "Zzz");

        Assert.Equal(CliApp.Success, code);
        Assert.Contains("No symbol matches 'Zzz'", harness.Out.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Show_PrintsSignatureDocsAndRelationships()
    {
        var harness = await Generated();

        var code = await harness.Run("show", "IService");

        Assert.Equal(CliApp.Success, code);
        var text = harness.Out.ToString();
        Assert.Contains("T:N.IService [Interface]", text, StringComparison.Ordinal);
        Assert.Contains("Summary: Does the work.", text, StringComparison.Ordinal);
        Assert.Contains("Incoming:", text, StringComparison.Ordinal);
        Assert.Contains("Implements [verified] Impl", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Show_AmbiguousName_ListsIdsAndFails()
    {
        var harness = await Generated();

        var code = await harness.Run("show", "Twin");

        Assert.Equal(CliApp.Failure, code);
        Assert.Contains("'Twin' is ambiguous", harness.Out.ToString(), StringComparison.Ordinal);
        Assert.Contains(harness.OutLines, l => l.StartsWith("T:N.Other.Twin ", StringComparison.Ordinal));
        Assert.Contains(harness.OutLines, l => l.StartsWith("T:N.Twin ", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Show_Unknown_SuggestsFindAndFails()
    {
        var harness = await Generated();

        var code = await harness.Run("show", "Nope");

        Assert.Equal(CliApp.Failure, code);
        Assert.Contains("No symbol named 'Nope'. Try: find Nope", harness.Out.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("impls", "IService", "Impl [Implements, verified]")]
    [InlineData("derived", "IService", "(none)")]
    [InlineData("refs", "IService", "[References, verified]")]
    [InlineData("deps", "Consumer", "IService [InjectedDependency, verified]")]
    public async Task QueryCommands_PrintTheirHits(string command, string argument, string expected)
    {
        var harness = await Generated();

        var code = await harness.Run(command, argument);

        Assert.Equal(CliApp.Success, code);
        Assert.Contains(expected, harness.Out.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Json_Find_ProducesValidJsonArray()
    {
        var harness = await Generated();

        await harness.Run("find", "Consumer", "--json");

        using var document = JsonDocument.Parse(harness.Out.ToString());
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        Assert.Equal("T:N.Consumer", document.RootElement[0].GetProperty("Id").GetString());
    }

    [Fact]
    public async Task Json_ShowRefsAndAmbiguous_AreValidJson()
    {
        var harness = await Generated();

        await harness.Run("show", "IService", "--json");
        using (var show = JsonDocument.Parse(harness.Out.ToString()))
        {
            Assert.Equal("T:N.IService", show.RootElement.GetProperty("Symbol").GetProperty("Id").GetString());
        }

        harness.Out.GetStringBuilder().Clear();
        await harness.Run("refs", "IService", "--json");
        using (var refs = JsonDocument.Parse(harness.Out.ToString()))
        {
            Assert.Equal(JsonValueKind.Array, refs.RootElement.ValueKind);
        }

        harness.Out.GetStringBuilder().Clear();
        var code = await harness.Run("show", "Twin", "--json");
        using var ambiguous = JsonDocument.Parse(harness.Out.ToString());
        Assert.Equal(CliApp.Failure, code);
        Assert.Equal("Ambiguous", ambiguous.RootElement.GetProperty("Status").GetString());
        Assert.Equal(2, ambiguous.RootElement.GetProperty("Candidates").GetArrayLength());
    }

    [Fact]
    public async Task Query_WithoutIndex_SaysHowToCreateItAndFails()
    {
        var harness = new Harness();

        var code = await harness.Run("find", "x");

        Assert.Equal(CliApp.Failure, code);
        Assert.Contains("generate", harness.Err.ToString(), StringComparison.Ordinal);
        Assert.Empty(harness.Out.ToString());
    }

    [Fact]
    public async Task Query_DoesNotCompileTheSolution()
    {
        var harness = await Generated();
        harness.SourceRequests.Clear();

        await harness.Run("find", "Consumer");
        await harness.Run("show", "Consumer");
        await harness.Run("refs", "IService");

        Assert.Empty(harness.SourceRequests);
    }

    // Two namespaces that differ only in case give the index files symbols/Core/Foo.json and symbols/Core/foo.json: one file on Windows and macOS.
    private const string CollidingSource = """
        namespace Foo { public class A { } }
        namespace foo { public class B { } }
        """;

    [Fact]
    public async Task Generate_TwoIndexFilesCollide_FailsAndLeavesTheExistingIndexAlone()
    {
        var harness = new Harness { Code = CollidingSource };
        harness.Folder.Files["symbols/Core/Old.json"] = Encoding.UTF8.GetBytes("old");

        var code = await harness.Run("generate");

        Assert.Equal(CliApp.Failure, code);
        Assert.Contains("Two source files produce the same index file", harness.Err.ToString(), StringComparison.Ordinal);
        Assert.Contains("symbols/Core/", harness.Err.ToString(), StringComparison.Ordinal);
        Assert.Empty(harness.Folder.Writes);
        Assert.Equal(0, harness.Folder.RemoveStaleCalls);
        Assert.Equal(["symbols/Core/Old.json"], harness.Folder.Files.Keys);
    }

    [Fact]
    public async Task Check_TwoIndexFilesCollide_FailsWithTheSameMessage()
    {
        var harness = new Harness { Code = CollidingSource };

        var code = await harness.Run("check");

        Assert.Equal(CliApp.Failure, code);
        Assert.Contains("Two source files produce the same index file", harness.Err.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Generate_NoCollision_WritesTheSameFilesAsBefore()
    {
        var harness = new Harness();

        var code = await harness.Run("generate");

        Assert.Equal(CliApp.Success, code);
        Assert.Equal(harness.Folder.Files.Keys, harness.Folder.Writes.Order(StringComparer.Ordinal));
        Assert.Contains("manifest.json", harness.Folder.Files.Keys);
        Assert.Equal(1, harness.Folder.RemoveStaleCalls);
    }

    private static string DeniedMessage => $"Access to the path '{TestCompilation.RepoRoot}/docs/index/names.tsv' is denied.";

    private static void AssertFriendlyFileError(Harness harness, int code)
    {
        var error = harness.Err.ToString();
        Assert.Equal(CliApp.Failure, code);
        Assert.StartsWith("A file problem stopped the command: ", error, StringComparison.Ordinal);
        Assert.Single(error.Split('\n', StringSplitOptions.RemoveEmptyEntries));
        Assert.DoesNotContain(" at ", error, StringComparison.Ordinal);
        Assert.DoesNotContain(TestCompilation.RepoRoot, error, StringComparison.Ordinal);
        Assert.Contains("<repo>", error, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("find", "Consumer")]
    [InlineData("show", "IService")]
    [InlineData("refs", "IService")]
    public async Task Query_FolderCannotBeRead_PrintsAFriendlyMessage(string command, string argument)
    {
        var harness = await Generated();
        harness.Folder.ReadFailure = new UnauthorizedAccessException(DeniedMessage);

        var code = await harness.Run(command, argument);

        AssertFriendlyFileError(harness, code);
    }

    [Fact]
    public async Task Check_FolderCannotBeRead_PrintsAFriendlyMessage()
    {
        var harness = await Generated();
        harness.Folder.ReadFailure = new IOException(DeniedMessage);

        var code = await harness.Run("check");

        AssertFriendlyFileError(harness, code);
    }

    [Fact]
    public async Task Generate_FolderCannotBeWritten_PrintsAFriendlyMessage()
    {
        var harness = new Harness();
        harness.Folder.WriteFailure = new IOException(DeniedMessage);

        var code = await harness.Run("generate");

        AssertFriendlyFileError(harness, code);
    }

    [Fact]
    public async Task Query_SecurityProblem_PrintsAFriendlyMessage()
    {
        var harness = await Generated();
        harness.Folder.ReadFailure = new System.Security.SecurityException(DeniedMessage);

        var code = await harness.Run("find", "Consumer");

        AssertFriendlyFileError(harness, code);
    }

    [Fact]
    public async Task Query_UnexpectedException_StillEscapes()
    {
        var harness = await Generated();
        harness.Folder.ReadFailure = new InvalidOperationException("a bug");

        await Assert.ThrowsAsync<InvalidOperationException>(() => harness.Run("find", "Consumer"));
    }

    [Fact]
    public async Task Query_Cancelled_StillEscapes()
    {
        var harness = await Generated();
        harness.Folder.ReadFailure = new OperationCanceledException();

        await Assert.ThrowsAsync<OperationCanceledException>(() => harness.Run("find", "Consumer"));
    }
}
