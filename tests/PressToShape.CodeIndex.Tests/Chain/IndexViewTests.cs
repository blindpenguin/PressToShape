using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Chain;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Query;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class IndexViewTests
{
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

    private static (IIndexView Model, IIndexView Reader, IndexData Data) Both()
    {
        var data = MvvmSampleData.Data();
        var sink = new InMemoryFileSink();
        IndexGenerator.Write(data, sink);
        return (new ModelIndexView(data), new ReaderIndexView(new IndexReader(sink)), data);
    }

    private static string Edge(RelationshipRecord e) => $"{e.Type}|{e.FromId}|{e.ToId}|{e.File}|{e.Line}|{e.Confidence}|{e.Reason}";

    [Fact]
    public void Parity_Projects_AreTheSame()
    {
        var (model, reader, _) = Both();

        Assert.Equal(model.Projects, reader.Projects);
        Assert.Equal(4, reader.Projects.Single(p => p.Name == "App").XamlFileCount);
    }

    [Fact]
    public void Parity_Relationships_AreTheSameSet()
    {
        var (model, reader, _) = Both();

        Assert.Equal(model.Relationships.Select(Edge).OrderBy(x => x, StringComparer.Ordinal), reader.Relationships.Select(Edge).OrderBy(x => x, StringComparer.Ordinal));
    }

    [Fact]
    public void Parity_EverySymbolExceptNamespaces_IsFoundWithTheSameRecord()
    {
        var (model, reader, data) = Both();

        foreach (var symbol in data.Projects.SelectMany(p => p.Symbols).Where(s => s.SymbolType != SymbolType.Namespace))
        {
            var viaReader = reader.SymbolOf(symbol.Id);
            Assert.NotNull(viaReader);
            Assert.Equal((symbol.Id, symbol.File, symbol.StartLine, symbol.Signature), (viaReader.Id, viaReader.File, viaReader.StartLine, viaReader.Signature));
            Assert.Equal(symbol.Id, model.SymbolOf(symbol.Id)!.Id);
        }
    }

    [Fact]
    public void Parity_MembersOfEveryType_AreTheSame()
    {
        var (model, reader, data) = Both();

        foreach (var type in data.Projects.SelectMany(p => p.Symbols).Where(s => s.SymbolType is SymbolType.Class or SymbolType.Interface))
        {
            Assert.Equal(model.MembersOf(type.Id).Select(m => m.Id), reader.MembersOf(type.Id).Select(m => m.Id));
        }
    }

    [Fact]
    public void Parity_XamlFiles_AreTheSame()
    {
        var (model, reader, _) = Both();

        Assert.Equal(model.XamlFiles.Select(f => f.Id), reader.XamlFiles.Select(f => f.Id));
        Assert.Equal(
            model.XamlFileOf("X:src/App/Views/ShellView.axaml")!.Bindings.Select(b => (b.Id, b.TargetId)),
            reader.XamlFileOf("X:src/App/Views/ShellView.axaml")!.Bindings.Select(b => (b.Id, b.TargetId)));
    }

    [Fact]
    public void ReaderView_ConstructorSymbol_IsFoundThroughItsType()
    {
        var (_, reader, _) = Both();

        var ctor = reader.SymbolOf("M:App.ViewModels.ShellViewModel.#ctor(App.Services.IFileStore,App.Services.IClock,System.Action)");

        Assert.NotNull(ctor);
        Assert.Equal(SymbolType.Constructor, ctor.SymbolType);
    }

    [Fact]
    public void Views_UnknownIds_GiveNullOrEmpty()
    {
        var (model, reader, _) = Both();

        foreach (var view in new[] { model, reader })
        {
            Assert.Null(view.SymbolOf("T:No.Such.Type"));
            Assert.Empty(view.MembersOf("T:No.Such.Type"));
            Assert.Null(view.XamlFileOf("X:no/such.axaml"));
            Assert.Null(view.XamlFileOf("T:App.ViewModels.ShellViewModel"));
        }
    }

    [Fact]
    public void ReaderView_ReadsOnlyWhatItNeeds()
    {
        var sink = new InMemoryFileSink();
        IndexGenerator.Write(MvvmSampleData.Data(), sink);
        var counting = new CountingSource(sink);
        var view = new ReaderIndexView(new IndexReader(counting));

        _ = view.SymbolOf("T:App.ViewModels.ShellViewModel");

        Assert.DoesNotContain(counting.Reads, r => r.StartsWith("xaml/", StringComparison.Ordinal));
        Assert.DoesNotContain(counting.Reads, r => r.StartsWith("relationships/", StringComparison.Ordinal));
        Assert.Single(counting.Reads.Distinct(), r => r.StartsWith("symbols/", StringComparison.Ordinal));
    }

    [Fact]
    public void ReaderView_SingleXamlFile_DoesNotReadTheOthers()
    {
        var sink = new InMemoryFileSink();
        IndexGenerator.Write(MvvmSampleData.Data(), sink);
        var counting = new CountingSource(sink);
        var view = new ReaderIndexView(new IndexReader(counting));

        _ = view.XamlFileOf("X:src/App/Views/PanelView.axaml");

        Assert.Single(counting.Reads.Distinct(), r => r.StartsWith("xaml/", StringComparison.Ordinal));
    }

    [Fact]
    public void ModelView_Output_IsDeterministicForShuffledProjects()
    {
        var first = new ModelIndexView(MvvmSampleData.Data());
        var second = new ModelIndexView(MvvmSampleData.Data(reverse: true));

        Assert.Equal(first.XamlFiles.Select(f => f.Id), second.XamlFiles.Select(f => f.Id));
        Assert.Equal(first.Projects, second.Projects);
        Assert.Equal(first.MembersOf("T:App.ViewModels.ShellViewModel").Select(s => s.Id), second.MembersOf("T:App.ViewModels.ShellViewModel").Select(s => s.Id));
    }
}
