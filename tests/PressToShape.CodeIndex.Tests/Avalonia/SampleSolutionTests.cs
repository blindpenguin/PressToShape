using System.Linq;
using PressToShape.CodeIndex.Model;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class SampleSolutionTests
{
    [Fact]
    public void Generate_Sample_HasNoErrorsAndThreeProjects()
    {
        var result = MvvmSampleData.Generate();

        Assert.False(result.HasErrors);
        Assert.Equal(["App", "App.Tests", "Core"], result.Data.Projects.Select(p => p.Name));
        Assert.Equal(4, result.Data.Projects.Single(p => p.Name == "App").Xaml.Count);
        Assert.Empty(result.Data.Projects.Single(p => p.Name == "Core").Xaml);
    }

    [Fact]
    public void Generate_Sample_HasTheAvaloniaFactsThePagesNeed()
    {
        var facts = MvvmSampleData.Data().Avalonia!;

        Assert.Equal(["T:App.Controls.Ruler"], facts.Controls.Select(c => c.Id));
        Assert.Equal(["T:App.Converters.FlagConverter", "T:App.Converters.UnusedConverter"], facts.Converters.Select(c => c.Id));
        var application = Assert.Single(facts.Applications);
        Assert.Equal("T:App.MyApp", application.Id);
        Assert.Equal(["IClassicDesktopStyleApplicationLifetime", "ISingleViewApplicationLifetime"], application.Overrides.Single(o => o.Name == "OnFrameworkInitializationCompleted").Branches.Select(b => b.LifetimeType));
    }

    [Fact]
    public void Generate_Sample_HasHandlerEdgesForGeneratedAndMethodGroupCommandsOnly()
    {
        var handlers = MvvmSampleData.Data().Projects.SelectMany(p => p.Relationships).Where(e => e.Type == RelationshipType.ExecutesMethod).Select(e => $"{e.FromId}->{e.ToId}").OrderBy(x => x, System.StringComparer.Ordinal).ToList();

        Assert.Equal(["P:App.ViewModels.ShellViewModel.SaveCommand->M:App.ViewModels.ShellViewModel.Save", "P:App.ViewModels.ShellViewModel.ZoomCommand->M:App.ViewModels.ShellViewModel.ZoomIn"], handlers);
    }

    [Fact]
    public void Generate_Sample_HasVerifiedAndInferredViewModelLinks()
    {
        var links = MvvmSampleData.Data().Projects.SelectMany(p => p.Relationships).Where(e => e.Type == RelationshipType.UsesViewModel).ToList();

        Assert.Contains(links, e => e.FromId == "T:App.Views.ShellView" && e.ToId == "T:App.ViewModels.ShellViewModel" && e.Confidence == Confidence.Verified && e.Reason == "x:DataType");
        Assert.Contains(links, e => e.FromId == "T:App.Views.PanelView" && e.ToId == "T:App.ViewModels.PanelViewModel" && e.Reason!.StartsWith("DataContext binding", System.StringComparison.Ordinal));
        Assert.Contains(links, e => e.FromId == "T:App.Views.DetailView" && e.ToId == "T:App.ViewModels.DetailViewModel" && e.Confidence == Confidence.Inferred && e.Reason!.Contains("ViewLocator", System.StringComparison.Ordinal));
        Assert.DoesNotContain(links, e => e.ToId == "T:App.ViewModels.OrphanViewModel");
    }

    [Fact]
    public void Generate_Sample_HasTestClassesThatReferenceTheViewModelAndTheService()
    {
        var edges = MvvmSampleData.Data().Projects.Single(p => p.Name == "App.Tests").Relationships;

        Assert.Contains(edges, e => e.Type == RelationshipType.Calls && e.ToId.StartsWith("M:App.ViewModels.ShellViewModel.#ctor", System.StringComparison.Ordinal) && e.FromId.Contains("ShellViewModelTests", System.StringComparison.Ordinal));
        Assert.Contains(edges, e => e.Type == RelationshipType.Calls && e.ToId == "M:App.Services.IFileStore.Save(System.String)" && e.FromId.Contains("StoreTests", System.StringComparison.Ordinal));
    }

    [Fact]
    public void Generate_Sample_IsDeterministicForShuffledProjects()
    {
        var first = MvvmSampleData.Index();
        var second = MvvmSampleData.Index(reverse: true);

        Assert.Equal(first.Files.Keys, second.Files.Keys);
        Assert.All(first.Files, pair => Assert.Equal(pair.Value, second.Files[pair.Key]));
    }
}
