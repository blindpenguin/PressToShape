using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Query;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class XamlQueryTests
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

    private static QueryService Service(IIndexFileSource? source = null) => new(new IndexReader(source ?? XamlSampleData.Index()));

    [Fact]
    public void Search_FindsXamlEntriesTogetherWithCSharpSymbols()
    {
        var hits = Service().Search("ShellView");

        Assert.Contains(hits, h => h.Id == "T:App.Views.ShellView" && h.SymbolType == "Class");
        Assert.Contains(hits, h => h.Id == "X:src/App/Views/ShellView.axaml" && h.SymbolType == "View");
        Assert.Equal(2, hits.Count(h => h.Name == "ShellView"));
    }

    [Theory]
    [InlineData("TitleText", "Element")]
    [InlineData("Gesture", "Resource")]
    [InlineData("RowTemplate", "Template")]
    [InlineData("Button.icon", "Style")]
    public void Search_FindsNamedElementsResourcesStylesAndTemplates(string term, string symbolType)
    {
        // A keyed template is both a resource definition and a template, so a name can have two entries.
        var hit = Assert.Single(Service().Search(term), h => h.Name == term && h.SymbolType == symbolType);

        Assert.StartsWith("src/App/Views/ShellView.axaml:", hit.Source, StringComparison.Ordinal);
    }

    [Fact]
    public void Show_ViewClass_IncludesItsXamlFileAndTheViewModelLinks()
    {
        var shown = Service().Show("ShellView").Value!;

        Assert.Equal("T:App.Views.ShellView", shown.Symbol!.Id);
        Assert.Equal("X:src/App/Views/ShellView.axaml", shown.Xaml!.File.Id);
        Assert.Equal("View", shown.Xaml.EntityType);
        Assert.Contains(shown.Incoming, h => h.RelationType == "CodeBehind" && h.Id == "X:src/App/Views/ShellView.axaml" && h.Confidence == Confidence.Verified);
        Assert.Contains(shown.Outgoing, h => h.RelationType == "UsesViewModel" && h.Id == "T:App.ViewModels.ShellViewModel" && h.Reason == "x:DataType");
        Assert.Equal(3, shown.Xaml.File.Bindings.Count);
    }

    [Fact]
    public void Show_ViewById_ReturnsTheXamlFileAndTheViewClass()
    {
        var shown = Service().Show("X:src/App/Views/ShellView.axaml").Value!;

        Assert.Equal("T:App.Views.ShellView", shown.Symbol!.Id);
        Assert.Equal("View", shown.Xaml!.EntityType);
        Assert.Equal("App.Views.ShellView", shown.Xaml.File.Class);
        Assert.Contains(shown.Outgoing, h => h.RelationType == "CodeBehind");
    }

    [Fact]
    public void Show_XamlWithoutAClass_HasNoSymbol()
    {
        var shown = Service().Show("X:src/App/App.axaml").Value!;

        Assert.Null(shown.Symbol);
        Assert.Equal("X:src/App/App.axaml", shown.Xaml!.File.Id);
    }

    [Fact]
    public void Show_NamedElement_ListsItsBindingsAndHandlersAsOutgoing()
    {
        var shown = Service().Show("TitleText").Value!;

        Assert.Null(shown.Symbol);
        Assert.Equal("Element", shown.Xaml!.EntityType);
        Assert.Contains(shown.Outgoing, h => h.RelationType == "BindsTo" && h.Id == "P:App.ViewModels.ShellViewModel.Title");
        Assert.Contains(shown.Outgoing, h => h.RelationType == "UsesConverter" && h.Id == "T:App.GestureConverter");
    }

    [Fact]
    public void Show_Resource_ListsTheReferencesToItAsIncoming()
    {
        var shown = Service().Show("PanelPadding").Value!;

        Assert.Equal("Resource", shown.Xaml!.EntityType);
        Assert.Contains(shown.Incoming, h => h.RelationType == "ReferencesResource" && h.Location.StartsWith("src/App/Views/ShellView.axaml:", StringComparison.Ordinal));
    }

    [Fact]
    public void Show_ResourceInBothThemeVariants_IsAmbiguousWithOneCandidatePerVariant()
    {
        var result = Service().Show("WindowBrush");

        Assert.Equal(QueryStatus.Ambiguous, result.Status);
        Assert.Equal(2, result.Candidates.Count);
        Assert.All(result.Candidates, c => Assert.Equal("Resource", c.SymbolType));
        Assert.Contains(result.Candidates, c => c.Id.EndsWith("@Dark", StringComparison.Ordinal));
    }

    [Fact]
    public void References_ViewModelProperty_IncludesTheBindingWithFileAndLine()
    {
        var hits = Service().References("P:App.ViewModels.ShellViewModel.Title").Value!;

        var binding = Assert.Single(hits, h => h.RelationType == "BindsTo");

        Assert.Equal("ShellView.axaml TextBlock[1] @Text", binding.Name);
        Assert.StartsWith("src/App/Views/ShellView.axaml:", binding.Location, StringComparison.Ordinal);
        Assert.Equal(Confidence.Verified, binding.Confidence);
        Assert.Equal("X:src/App/Views/ShellView.axaml#/StackPanel[1]/TextBlock[1]@Text".Replace("/StackPanel", "/UserControl/StackPanel", StringComparison.Ordinal), binding.Id);
    }

    [Fact]
    public void References_Command_IncludesTheBinding()
    {
        var hits = Service().References("P:App.ViewModels.ShellViewModel.SaveCommand").Value!;

        Assert.Contains(hits, h => h.RelationType == "BindsTo" && h.Name.EndsWith("@Command", StringComparison.Ordinal));
    }

    [Fact]
    public void References_ConverterClass_IncludesTheBindingAndTheResourceElement()
    {
        var hits = Service().References("GestureConverter").Value!;

        Assert.Contains(hits, h => h.RelationType == "UsesConverter");
        Assert.Contains(hits, h => h.RelationType == "UsesType");
    }

    [Fact]
    public void References_EventHandlerMethod_IncludesTheXamlEvent()
    {
        var hits = Service().References("M:App.Views.ShellView.OnSaveClick(System.Object,System.EventArgs)").Value!;

        var hit = Assert.Single(hits, h => h.RelationType == "HandledBy");

        Assert.Equal("ShellView.axaml Button[1] @Click", hit.Name);
        Assert.Equal(Confidence.Verified, hit.Confidence);
    }

    [Fact]
    public void References_ViewModelClass_IncludesTheViewsThatUseIt()
    {
        var hits = Service().References("ShellViewModel").Value!;

        Assert.Contains(hits, h => h.RelationType == "UsesViewModel" && h.Id == "T:App.Views.ShellView" && h.Reason == "x:DataType");
        Assert.Contains(hits, h => h.RelationType == "UsesViewModel" && h.Id == "T:Avalonia.Controls.Window" && h.Reason == "DataContext assignment");
    }

    [Fact]
    public void References_ViewModelClassOfAnInferredLink_IsMarkedInferredWithTheReason()
    {
        var hits = Service().References("PanelViewModel").Value!;

        var inferred = Assert.Single(hits, h => h.Confidence == Confidence.Inferred);

        Assert.Equal("T:App.Views.PanelView", inferred.Id);
        Assert.Contains("ViewLocator (registered in Application.DataTemplates at src/App/App.axaml:", inferred.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void References_ViewClass_IncludesItsXamlFileThroughCodeBehind()
    {
        var hits = Service().References("T:App.Views.ShellView").Value!;

        Assert.Contains(hits, h => h.RelationType == "CodeBehind" && h.Id == "X:src/App/Views/ShellView.axaml");
    }

    [Fact]
    public void References_ResourceKey_IncludesTheXamlPlacesThatUseIt()
    {
        var hits = Service().References("PanelPadding").Value!;

        Assert.Contains(hits, h => h.RelationType == "ReferencesResource");
    }

    [Fact]
    public void Queries_ForXamlEntries_DoNotReadSymbolFiles()
    {
        var counting = new CountingSource(XamlSampleData.Index());

        Service(counting).Show("TitleText");

        Assert.DoesNotContain(counting.Reads, r => r.StartsWith("symbols/", StringComparison.Ordinal));
        Assert.Contains(counting.Reads, r => r.StartsWith("xaml/", StringComparison.Ordinal));
    }

    [Fact]
    public void Show_OldXamlFileSchema_SaysHowToRegenerate()
    {
        var index = XamlSampleData.Index();
        var path = index.Files.Keys.First(k => k.StartsWith("xaml/App/Views", StringComparison.Ordinal));
        var text = Encoding.UTF8.GetString(index.Files[path]).Replace($"\"SchemaVersion\":{IndexSchema.Version}", "\"SchemaVersion\":1", StringComparison.Ordinal);
        var sink = index.WithReplaced(path, Encoding.UTF8.GetBytes(text));

        var ex = Assert.Throws<IndexToolException>(() => Service(sink).Show("TitleText"));

        Assert.Contains("schema version 1", ex.Message, StringComparison.Ordinal);
        Assert.Contains(IndexHints.Regenerate, ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Show_DamagedXamlFile_IsReportedNotCrashed()
    {
        var index = XamlSampleData.Index();
        var sink = index.WithReplaced(index.Files.Keys.First(k => k.StartsWith("xaml/App/Views", StringComparison.Ordinal)), Encoding.UTF8.GetBytes("{ nope"));

        Assert.Throws<IndexToolException>(() => Service(sink).Show("TitleText"));
    }

    [Fact]
    public void Show_CSharpOnlyIndex_StillWorksWithoutXaml()
    {
        var sink = new InMemoryFileSink();
        JsonIndexWriter.Write(new IndexData([new ProjectIndex("P", "src/P/P.csproj", [], [])], []), sink);

        Assert.Equal(QueryStatus.NotFound, Service(sink).Show("Anything").Status);
    }
}
