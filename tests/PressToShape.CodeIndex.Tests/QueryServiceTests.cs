using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Query;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class QueryServiceTests
{
    private const string Source = """
        namespace N;
        /// <summary>Does the work.</summary>
        public interface IService { void Run(int x); string Title { get; } }
        public class Base : IService { public virtual void Run(int x) { } public string Title => ""; }
        public class Derived : Base { public override void Run(int x) { } }
        public class Consumer
        {
            private readonly IService _service;
            public Consumer(IService service) { _service = service; service.Run(1); }
        }
        public class Helper { public void Go(Base b) { b.Run(2); } public void Run() { } }
        public interface IExtended : IService { }
        """;

    private sealed class FakeSource(params ProjectCompilation[] projects) : ICompilationSource
    {
        public Task<LoadResult> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new LoadResult(projects, []));
    }

    private sealed class CountingSource(IIndexFileSource inner) : IIndexFileSource
    {
        public List<string> Reads { get; } = [];

        public byte[]? Read(string relativePath)
        {
            Reads.Add(relativePath);
            return inner.Read(relativePath);
        }

        public IReadOnlyList<string> ListFiles() => inner.ListFiles();
    }

    private static InMemoryFileSink BuildIndex()
    {
        var generated = new IndexGenerator(new FakeSource(TestCompilation.Project("Core", Source)), TestCompilation.RepoRoot).GenerateAsync(CancellationToken.None).GetAwaiter().GetResult();
        var sink = new InMemoryFileSink();
        IndexGenerator.Write(generated.Data, sink);
        return sink;
    }

    private static QueryService Service(IIndexFileSource? source = null) => new(new IndexReader(source ?? BuildIndex()));

    private static List<string> Ids(IReadOnlyList<QueryHit> hits) => hits.Select(h => h.Id).ToList();

    [Fact]
    public void Search_ExactMatches_AreOrderedById()
    {
        var hits = Service().Search("Run");

        Assert.Equal(
            ["M:N.Base.Run(System.Int32)", "M:N.Derived.Run(System.Int32)", "M:N.Helper.Run", "M:N.IService.Run(System.Int32)"],
            hits.Select(h => h.Id));
        Assert.All(hits, h => Assert.Equal("Method", h.SymbolType));
    }

    [Fact]
    public void Search_ExactBeforePrefixBeforeSubstring_AndCaseInsensitiveAfterExact()
    {
        var service = Service();

        var serv = service.Search("Serv");
        var consumer = service.Search("consumer");
        var run = service.Search("un").Select(h => h.Name).Distinct().ToList();

        Assert.Equal("IService", serv[0].Name);
        Assert.Equal("Consumer", consumer[0].Name);
        Assert.Equal("Run", run[0]);
    }

    [Fact]
    public void Search_TermWithDot_AlsoMatchesIdsAndRespectsLimit()
    {
        var service = Service();

        Assert.Contains(service.Search("N.Consumer"), h => h.Id == "T:N.Consumer");
        Assert.Single(service.Search("Run", limit: 1));
        Assert.Empty(service.Search("DoesNotExistAnywhere"));
    }

    [Fact]
    public void Show_UniqueType_ReturnsSymbolWithDocsAndRelationships()
    {
        var result = Service().Show("IService");

        Assert.Equal(QueryStatus.Found, result.Status);
        var shown = result.Value!;
        Assert.Equal("T:N.IService", shown.Symbol!.Id);
        Assert.Equal("Does the work.", shown.Symbol.Doc.Summary);
        Assert.Contains(shown.Incoming, h => h.Id == "T:N.Base" && h.RelationType == "Implements" && h.Confidence == Confidence.Verified);
        Assert.Contains(shown.Incoming, h => h.Id == "T:N.Consumer" && h.RelationType == "InjectedDependency");
    }

    [Fact]
    public void Show_ById_ResolvesAMember()
    {
        var result = Service().Show("M:N.Base.Run(System.Int32)");

        Assert.Equal(QueryStatus.Found, result.Status);
        Assert.Equal("void Run(int x)", result.Value!.Symbol!.Signature);
        Assert.Contains(result.Value.Incoming, h => h.Id == "M:N.Helper.Go(N.Base)" && h.RelationType == "Calls" && h.Location.StartsWith("src/Core/File0.cs:", StringComparison.Ordinal));
    }

    [Fact]
    public void Show_TypeWithSameShortNameAsFullName_ResolvesWithoutPrefix()
    {
        Assert.Equal("T:N.Consumer", Service().Show("N.Consumer").Value!.Symbol!.Id);
    }

    [Fact]
    public void Show_AmbiguousName_ListsCandidatesAndNoValue()
    {
        var result = Service().Show("Run");

        Assert.Equal(QueryStatus.Ambiguous, result.Status);
        Assert.Null(result.Value);
        Assert.Equal(4, result.Candidates.Count);
    }

    [Fact]
    public void Show_UnknownName_IsNotFound()
    {
        var result = Service().Show("Nope");

        Assert.Equal(QueryStatus.NotFound, result.Status);
        Assert.Empty(result.Candidates);
    }

    [Fact]
    public void Implementations_Interface_ListsVerifiedImplementersAndExtendingInterfaces()
    {
        var hits = Service().Implementations("IService").Value!;

        Assert.Equal(["T:N.Base", "T:N.Derived", "T:N.IExtended"], Ids(hits).Where(i => i.StartsWith("T:", StringComparison.Ordinal)));
        Assert.All(hits.Where(h => h.Id.StartsWith("T:", StringComparison.Ordinal)), h => Assert.Equal(Confidence.Verified, h.Confidence));
    }

    [Fact]
    public void Implementations_InterfaceMethod_ListsMatchingMembersAsInferred()
    {
        var hits = Service().Implementations("M:N.IService.Run(System.Int32)").Value!;

        Assert.Equal(["M:N.Base.Run(System.Int32)", "M:N.Derived.Run(System.Int32)"], Ids(hits));
        Assert.All(hits, h =>
        {
            Assert.Equal(Confidence.Inferred, h.Confidence);
            Assert.False(string.IsNullOrEmpty(h.Reason));
        });
    }

    [Fact]
    public void Derived_Class_ListsDirectSubtypes()
    {
        Assert.Equal(["T:N.Derived"], Ids(Service().Derived("Base").Value!));
        Assert.Empty(Service().Derived("Derived").Value!);
    }

    [Fact]
    public void References_Method_ListsVerifiedCallsAndInferredCallsThroughTheInterface()
    {
        var hits = Service().References("M:N.Base.Run(System.Int32)").Value!;

        var verified = hits.Where(h => h.Confidence == Confidence.Verified).ToList();
        var inferred = hits.Where(h => h.Confidence == Confidence.Inferred).ToList();
        Assert.Equal(["M:N.Helper.Go(N.Base)"], Ids(verified));
        Assert.Contains(inferred, h => h.Id == "M:N.Consumer.#ctor(N.IService)" && h.Reason!.Contains("M:N.IService.Run(System.Int32)", StringComparison.Ordinal));
    }

    [Fact]
    public void References_OverrideMethod_AlsoCountsCallsOfTheBaseMember()
    {
        var hits = Service().References("M:N.Derived.Run(System.Int32)").Value!;

        Assert.DoesNotContain(hits, h => h.Confidence == Confidence.Verified);
        Assert.Contains(hits, h => h.Id == "M:N.Helper.Go(N.Base)" && h.Reason!.Contains("overrides", StringComparison.Ordinal));
        Assert.All(hits, h => Assert.Equal(Confidence.Inferred, h.Confidence));
    }

    [Fact]
    public void References_Type_ListsSignatureAndCreationReferences()
    {
        var hits = Service().References("IService").Value!;

        Assert.Contains(hits, h => h.Id == "F:N.Consumer._service" && h.RelationType == "References");
        Assert.Contains(hits, h => h.Id == "M:N.Consumer.#ctor(N.IService)");
    }

    [Fact]
    public void Dependencies_Type_ListsConstructorDependencies()
    {
        var hits = Service().Dependencies("Consumer").Value!;

        var hit = Assert.Single(hits);
        Assert.Equal("T:N.IService", hit.Id);
        Assert.Equal("InjectedDependency", hit.RelationType);
        Assert.Equal(Confidence.Verified, hit.Confidence);
    }

    [Fact]
    public void Queries_UnknownSymbol_AreNotFoundForEveryQuery()
    {
        var service = Service();

        Assert.Equal(QueryStatus.NotFound, service.Implementations("Nope").Status);
        Assert.Equal(QueryStatus.NotFound, service.Derived("Nope").Status);
        Assert.Equal(QueryStatus.NotFound, service.References("Nope").Status);
        Assert.Equal(QueryStatus.NotFound, service.Dependencies("Nope").Status);
    }

    [Fact]
    public void Search_ReadsOnlyTheManifestAndTheNamesTable()
    {
        var counting = new CountingSource(BuildIndex());

        Service(counting).Search("Consumer");

        Assert.Equal(["manifest.json", "names.tsv"], counting.Reads.Distinct().OrderBy(r => r, StringComparer.Ordinal));
    }

    [Fact]
    public void Show_ReadsOneSymbolFileNotTheWholeIndex()
    {
        var counting = new CountingSource(BuildIndex());

        Service(counting).Show("IService");

        Assert.Single(counting.Reads.Distinct(), r => r.StartsWith("symbols/", StringComparison.Ordinal));
    }

    [Fact]
    public void Reader_MissingIndex_SaysHowToCreateIt()
    {
        var ex = Assert.Throws<IndexToolException>(() => Service(new InMemoryFileSink()).Search("x"));

        Assert.Contains("generate", ex.Message, StringComparison.Ordinal);
        Assert.Contains("manifest.json", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Reader_OtherSchemaVersion_SaysHowToRegenerate()
    {
        var sink = BuildIndex();
        var manifest = Encoding.UTF8.GetString(sink.Files["manifest.json"]).Replace($"\"SchemaVersion\": {IndexSchema.Version}", "\"SchemaVersion\": 99", StringComparison.Ordinal);
        sink.Write("manifest.json", Encoding.UTF8.GetBytes(manifest));

        var ex = Assert.Throws<IndexToolException>(() => Service(sink).Search("x"));

        Assert.Contains("schema version 99", ex.Message, StringComparison.Ordinal);
        Assert.Contains(IndexHints.Regenerate, ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Reader_DamagedNamesTable_IsReportedNotCrashed()
    {
        var sink = BuildIndex();
        sink.Write("names.tsv", Encoding.UTF8.GetBytes("only\ttwo\n"));

        var ex = Assert.Throws<IndexToolException>(() => Service(sink).Search("x"));

        Assert.Contains("names.tsv is damaged", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Reader_InvalidJson_IsReportedNotCrashed()
    {
        var sink = BuildIndex();
        sink.Write("manifest.json", Encoding.UTF8.GetBytes("{ not json"));

        Assert.Throws<IndexToolException>(() => Service(sink).Search("x"));
    }

    [Fact]
    public void DirectoryFileSource_MissingFile_ReturnsNullAndRejectsOutsidePaths()
    {
        var source = new DirectoryFileSource(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ptsh-no-such-" + Guid.NewGuid().ToString("N")));

        Assert.Null(source.Read("manifest.json"));
        Assert.Throws<ArgumentException>(() => source.Read("../x"));
    }
}
