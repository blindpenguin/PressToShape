using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Chain;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Query;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class IndexAnalysisTests
{
    private const string Shell = "T:App.ViewModels.ShellViewModel";

    private sealed class FakeProjects(params ProjectCompilation[] projects) : ICompilationSource
    {
        public Task<LoadResult> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new LoadResult(projects, []));
    }

    /// <summary>The analysis over both views of the same data, so every test runs against the model view and the file view.</summary>
    private static IEnumerable<IndexAnalysis> Analyses(IndexData data)
    {
        var sink = new InMemoryFileSink();
        IndexGenerator.Write(data, sink);
        yield return new IndexAnalysis(new ModelIndexView(data));
        yield return new IndexAnalysis(new ReaderIndexView(new IndexReader(sink)));
    }

    private static IEnumerable<IndexAnalysis> Sample() => Analyses(MvvmSampleData.Data());

    private static IEnumerable<IndexAnalysis> Small(string appSource, params ProjectCompilation[] others)
    {
        var app = XamlTestHost.AppProject("App", MvvmSampleData.ToolkitStubs, appSource);
        var projects = others.Prepend(app).ToArray();
        var data = new IndexGenerator(new FakeProjects(projects), TestCompilation.RepoRoot, new InMemoryXamlFileSource()).GenerateAsync(CancellationToken.None).GetAwaiter().GetResult().Data;
        return Analyses(data);
    }

    [Fact]
    public void ViewModelsOf_View_GivesEvidenceAndConfidence()
    {
        foreach (var analysis in Sample())
        {
            var shell = Assert.Single(analysis.ViewModelsOf("T:App.Views.ShellView"));
            Assert.Equal((Shell, Confidence.Verified, "x:DataType"), (shell.ViewModelId, shell.Confidence, shell.Reason));

            var detail = analysis.ViewModelsOf("T:App.Views.DetailView");
            Assert.Equal([Confidence.Verified, Confidence.Inferred], detail.Select(l => l.Confidence));
            Assert.Contains("ViewLocator", detail[1].Reason, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ViewsOf_ViewModel_GivesTheViewsAndNoneForAnOrphan()
    {
        foreach (var analysis in Sample())
        {
            Assert.Equal(["T:App.Views.ShellView"], analysis.ViewsOf(Shell).Select(l => l.ViewId));
            Assert.Empty(analysis.ViewsOf("T:App.ViewModels.OrphanViewModel"));
        }
    }

    [Fact]
    public void Services_ListInterfacesWithImplementationsAndCreationSitesWithoutTests()
    {
        foreach (var analysis in Sample())
        {
            var services = analysis.Services(Shell);

            Assert.Equal(["IClock", "IFileStore"], services.Select(s => s.Name));
            var store = services.Single(s => s.Name == "IFileStore");
            Assert.True(store.IsInterface);
            Assert.Equal("Stores files.", store.Summary);
            var local = Assert.Single(store.Implementations);
            Assert.Equal("LocalFileStore", local.Name);
            var site = Assert.Single(local.CreatedAt);
            Assert.Equal("MyApp.OnFrameworkInitializationCompleted", site.FromName);
            Assert.StartsWith("src/App/File1.cs:", site.Location, StringComparison.Ordinal);
            var clock = services.Single(s => s.Name == "IClock").Implementations.Single();
            Assert.Equal(["MyApp.Initialize", "MyApp.OnFrameworkInitializationCompleted"], clock.CreatedAt.Select(p => p.FromName).OrderBy(n => n, StringComparer.Ordinal));
        }
    }

    [Fact]
    public void Services_InterfaceWithoutImplementation_HasNoImplementationsAndAClassDependencyListsItsCreationSites()
    {
        const string Source = """
            namespace App
            {
                public interface IMissing { }
                public class Helper { }
                public class Vm { public Vm(IMissing missing, Helper helper) { } }
                public class Starter { public Vm Make() => new Vm(null!, new Helper()); }
            }
            """;

        foreach (var analysis in Small(Source))
        {
            var services = analysis.Services("T:App.Vm");

            var missing = services.Single(s => s.Name == "IMissing");
            Assert.Empty(missing.Implementations);
            var helper = services.Single(s => s.Name == "Helper");
            Assert.False(helper.IsInterface);
            Assert.Equal(["Starter.Make"], helper.CreatedAt.Select(p => p.FromName));
        }
    }

    [Fact]
    public void Models_AreReferencedTypesOfProjectsWithoutViewsAndNotTheServices()
    {
        foreach (var analysis in Sample())
        {
            var model = Assert.Single(analysis.Models(Shell));

            Assert.Equal(("Document", "Core", "T:Core.Document"), (model.Name, model.Project, model.TypeId));
            Assert.True(model.References >= 2);
        }
    }

    [Fact]
    public void Models_TypeOfTheSameProjectIsNotAModel()
    {
        const string Source = "namespace App { public class Local { } public class Vm { public Local Item { get; } = new(); } }";

        foreach (var analysis in Small(Source))
        {
            Assert.Empty(analysis.Models("T:App.Vm"));
        }
    }

    [Fact]
    public void Commands_ListGeneratedMethodGroupAndBoundCommandsWithTheirHandlers()
    {
        foreach (var analysis in Sample())
        {
            var commands = analysis.Commands(Shell);

            Assert.Equal(["ApplyCommand", "SaveCommand", "ZoomCommand"], commands.Select(c => c.Name));
            var save = commands.Single(c => c.Name == "SaveCommand");
            Assert.Equal(("M:App.ViewModels.ShellViewModel.Save", Confidence.Verified, "[RelayCommand]"), (save.Handler.MethodId, save.Handler.Confidence, save.Handler.Reason));
            var zoom = commands.Single(c => c.Name == "ZoomCommand");
            Assert.Equal(("M:App.ViewModels.ShellViewModel.ZoomIn", "new RelayCommand(method group)"), (zoom.Handler.MethodId, zoom.Handler.Reason));
            var apply = commands.Single(c => c.Name == "ApplyCommand");
            Assert.False(apply.Handler.IsKnown);
            Assert.StartsWith("not statically known", apply.Handler.Reason, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Commands_AreBoundAtTheXamlPlaces()
    {
        foreach (var analysis in Sample())
        {
            var save = analysis.CommandOf("P:App.ViewModels.ShellViewModel.SaveCommand")!;

            var place = Assert.Single(save.BoundAt);
            Assert.StartsWith("src/App/Views/ShellView.axaml:", place.Location, StringComparison.Ordinal);
            Assert.Equal("ShellView.axaml Button[1] @Command", place.FromName);
        }
    }

    [Fact]
    public void IsCommand_IsTrueForCommandsAndFalseForOtherMembers()
    {
        foreach (var analysis in Sample())
        {
            Assert.True(analysis.IsCommand("P:App.ViewModels.ShellViewModel.SaveCommand"));
            Assert.True(analysis.IsCommand("P:App.ViewModels.ShellViewModel.ApplyCommand"));
            Assert.False(analysis.IsCommand("P:App.ViewModels.ShellViewModel.Title"));
            Assert.False(analysis.IsCommand("T:App.ViewModels.ShellViewModel"));
            Assert.Null(analysis.CommandOf("P:No.Such.Command"));
        }
    }

    [Fact]
    public void HandlerOf_UnknownId_IsNotStaticallyKnown()
    {
        foreach (var analysis in Sample())
        {
            Assert.False(analysis.HandlerOf("P:Nothing.Here").IsKnown);
        }
    }

    [Fact]
    public void TestsFor_GroupsPerTestClassAndIgnoresClassesWithoutTestMethods()
    {
        foreach (var analysis in Sample())
        {
            var vmTests = analysis.TestsFor([Shell]);
            var viewModelTest = Assert.Single(vmTests);
            Assert.Equal("ShellViewModelTests", viewModelTest.Name);
            Assert.True(viewModelTest.References >= 1);
            Assert.StartsWith("src/App.Tests/File0.cs", viewModelTest.File, StringComparison.Ordinal);

            Assert.Equal(["StoreTests"], analysis.TestsFor(["T:App.Services.IFileStore"]).Select(t => t.Name));
            Assert.Empty(analysis.TestsFor(["T:App.ViewModels.PanelViewModel"]));
        }
    }

    [Fact]
    public void Services_IgnoreFakesAndHelpersOfTestProjects()
    {
        foreach (var analysis in Sample())
        {
            var store = analysis.Services(Shell).Single(s => s.Name == "IFileStore");

            Assert.Equal(["LocalFileStore"], store.Implementations.Select(i => i.Name));
            Assert.DoesNotContain(analysis.CreationSites("T:App.Services.SystemClock"), p => p.FromName.StartsWith("ClockFactory", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void CreationSites_ExcludeTestsAndCoverImplicitAndExplicitConstructors()
    {
        foreach (var analysis in Sample())
        {
            Assert.Equal(["MyApp.OnFrameworkInitializationCompleted"], analysis.CreationSites("T:App.Services.LocalFileStore").Select(p => p.FromName));
            Assert.Equal(2, analysis.CreationSites("T:App.Services.SystemClock").Count);
        }
    }

    [Fact]
    public void NameOf_UsesTheSymbolNameOrDescribesXamlIds()
    {
        foreach (var analysis in Sample())
        {
            Assert.Equal("ShellViewModel", analysis.NameOf(Shell));
            Assert.Equal("ShellView.axaml", analysis.NameOf("X:src/App/Views/ShellView.axaml"));
        }
    }
}
