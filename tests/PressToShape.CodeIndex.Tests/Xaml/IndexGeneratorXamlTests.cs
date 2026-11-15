using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Xaml;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class IndexGeneratorXamlTests
{
    private const string AppSource = """
        namespace App.ViewModels { public class ShellViewModel { public string Title { get; } = ""; public System.Windows.Input.ICommand SaveCommand { get; } = null!; } public class PanelViewModel { } }
        namespace App.Views
        {
            public partial class ShellView : Avalonia.Controls.UserControl { }
            public partial class PanelView : Avalonia.Controls.UserControl { }
            public class Startup { public Avalonia.Controls.Window Make() => new Avalonia.Controls.Window { DataContext = new App.ViewModels.ShellViewModel() }; }
        }
        namespace App
        {
            public class ViewLocator : Avalonia.Controls.Templates.IDataTemplate
            {
                public object? Build(object? param)
                {
                    var name = param!.GetType().FullName!.Replace("ViewModel", "View", System.StringComparison.Ordinal);
                    var type = System.Type.GetType(name);
                    return type is null ? null : System.Activator.CreateInstance(type);
                }

                public bool Match(object? data) => true;
            }
        }
        """;

    private const string Header = XamlTestHost.Header;

    private static readonly string Application =
        $"<Application {Header} xmlns:local=\"using:App\"><Application.DataTemplates><local:ViewLocator /></Application.DataTemplates><Application.Resources><ResourceDictionary><ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"avares://Lib/Res/Colors.axaml\" /></ResourceDictionary.MergedDictionaries></ResourceDictionary></Application.Resources></Application>";

    private const string Shell = "<UserControl xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:vm=\"using:App.ViewModels\" x:Class=\"App.Views.ShellView\" x:DataType=\"vm:ShellViewModel\">\n<StackPanel Background=\"{DynamicResource LibBrush}\"><TextBlock Text=\"{Binding Title}\" /><ContentControl Content=\"{Binding Panel}\" /></StackPanel>\n</UserControl>";

    private sealed class FakeSource(params ProjectCompilation[] projects) : ICompilationSource
    {
        public Task<LoadResult> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new LoadResult(projects, []));
    }

    private static (ProjectCompilation App, ProjectCompilation Lib) Projects(string appSource = AppSource)
    {
        var app = TestCompilation.Project("App", XamlTestHost.Avalonia, appSource);
        var lib = TestCompilation.Project("Lib", "namespace Lib { public class Marker { } }");
        return (app, lib);
    }

    private static InMemoryXamlFileSource Files() => new InMemoryXamlFileSource()
        .Add("src/App/App.axaml", Application)
        .Add("src/App/Views/ShellView.axaml", Shell)
        .Add("src/Lib/Res/Colors.axaml", $"<ResourceDictionary {Header}><SolidColorBrush x:Key=\"LibBrush\" /></ResourceDictionary>");

    private static Task<GenerateResult> Generate(IXamlFileSource? xaml, params ProjectCompilation[] projects) =>
        new IndexGenerator(new FakeSource(projects), TestCompilation.RepoRoot, xaml).GenerateAsync(CancellationToken.None);

    [Fact]
    public async Task GenerateAsync_WithXaml_PutsFilesAndEdgesIntoTheOwningProject()
    {
        var (app, lib) = Projects();

        var result = await Generate(Files(), app, lib);

        var appIndex = result.Data.Projects.Single(p => p.Name == "App");
        Assert.False(result.HasErrors);
        Assert.Equal(["X:src/App/App.axaml", "X:src/App/Views/ShellView.axaml"], appIndex.Xaml.Select(f => f.Id));
        Assert.Equal(["X:src/Lib/Res/Colors.axaml"], result.Data.Projects.Single(p => p.Name == "Lib").Xaml.Select(f => f.Id));
        Assert.Contains(appIndex.Relationships, e => e.Type == RelationshipType.BindsTo && e.ToId == "P:App.ViewModels.ShellViewModel.Title");
        Assert.Contains(appIndex.Relationships, e => e.Type == RelationshipType.UsesViewModel && e.FromId == "T:App.Views.ShellView" && e.Reason == "x:DataType");
        Assert.Contains(appIndex.Relationships, e => e.Type == RelationshipType.CodeBehind && e.ToId == "T:App.Views.ShellView");
    }

    [Fact]
    public async Task GenerateAsync_IncludeAndResourceAcrossProjects_AreResolved()
    {
        var (app, lib) = Projects();

        var result = await Generate(Files(), app, lib);

        var edges = result.Data.Projects.Single(p => p.Name == "App").Relationships;
        Assert.Contains(edges, e => e.Type == RelationshipType.Includes && e.FromId == "X:src/App/App.axaml" && e.ToId == "X:src/Lib/Res/Colors.axaml");
        Assert.Contains(edges, e => e.Type == RelationshipType.ReferencesResource && e.ToId == "R:src/Lib/Res/Colors.axaml#/ResourceDictionary/LibBrush");
    }

    [Fact]
    public async Task GenerateAsync_CSharpDataContextAssignmentAndStructuralEdges_StillAppear()
    {
        var (app, lib) = Projects();

        var result = await Generate(Files(), app, lib);

        var edges = result.Data.Projects.Single(p => p.Name == "App").Relationships;
        Assert.Contains(edges, e => e.Type == RelationshipType.UsesViewModel && e.FromId == "T:Avalonia.Controls.Window" && e.Reason == "DataContext assignment" && e.ToId == "T:App.ViewModels.ShellViewModel");
        Assert.Contains(edges, e => e.Type == RelationshipType.Implements && e.FromId == "T:App.ViewLocator");
    }

    [Fact]
    public async Task GenerateAsync_BindingToAMissingProperty_IsUnresolvedAndMakesNoInferredLink()
    {
        var (app, lib) = Projects();

        var result = await Generate(Files(), app, lib);

        // ShellViewModel has no Panel property here, so no inferred link exists and nothing breaks.
        Assert.DoesNotContain(result.Data.Projects.Single(p => p.Name == "App").Relationships, e => e.Confidence == Confidence.Inferred && e.Type == RelationshipType.UsesViewModel);
        Assert.Contains(result.Data.Projects.Single(p => p.Name == "App").Xaml.Single(f => f.File.EndsWith("ShellView.axaml", StringComparison.Ordinal)).Bindings, b => b.Path == "Panel" && b.TargetId is null);
    }

    [Fact]
    public async Task GenerateAsync_MalformedXaml_IsAProjectErrorAndOtherFilesAreKept()
    {
        var (app, lib) = Projects();
        var source = Files().Add("src/App/Views/Broken.axaml", "<UserControl>\n<Grid>\n</UserControl>");

        var result = await Generate(source, app, lib);

        Assert.True(result.HasErrors);
        var problem = Assert.Single(result.Data.Warnings, p => !p.IsWarning);
        Assert.Equal("App", problem.Project);
        Assert.StartsWith("src/App/Views/Broken.axaml:", problem.Reason, StringComparison.Ordinal);
        Assert.Contains(result.Data.Projects.Single(p => p.Name == "App").Xaml, f => f.File == "src/App/Views/ShellView.axaml");
    }

    [Fact]
    public async Task GenerateAsync_WithoutAXamlSource_BehavesLikeBefore()
    {
        var (app, lib) = Projects();

        var result = await Generate(null, app, lib);

        Assert.All(result.Data.Projects, p => Assert.Empty(p.Xaml));
        Assert.DoesNotContain(result.Data.Projects.SelectMany(p => p.Relationships), e => e.Type is RelationshipType.BindsTo or RelationshipType.CodeBehind or RelationshipType.Includes);
    }

    [Fact]
    public async Task GenerateAsync_ProjectWithoutXamlFiles_IsUnchanged()
    {
        var (app, lib) = Projects();
        var withXaml = await Generate(Files(), app, lib);
        var withoutXaml = await Generate(null, app, lib);

        var libWith = withXaml.Data.Projects.Single(p => p.Name == "Lib");
        var libWithout = withoutXaml.Data.Projects.Single(p => p.Name == "Lib");

        Assert.Equal(libWithout.Symbols.Count, libWith.Symbols.Count);
        Assert.Equal(libWithout.Relationships.Count, libWith.Relationships.Count);
    }

    [Fact]
    public async Task GenerateAsync_NoXamlAtAll_GivesEmptyListsAndNoProblems()
    {
        var (app, lib) = Projects();

        var result = await Generate(new InMemoryXamlFileSource(), app, lib);

        Assert.False(result.HasErrors);
        Assert.Empty(result.Data.Warnings);
        Assert.All(result.Data.Projects, p => Assert.Empty(p.Xaml));
    }

    [Fact]
    public async Task GenerateAsync_RegisteredLocatorWithAnUnrecognizedConvention_AddsOneFindingToTheApplicationFile()
    {
        var source = AppSource.Replace("Replace(\"ViewModel\", \"View\", System.StringComparison.Ordinal)", "ToString()", StringComparison.Ordinal);
        var (app, lib) = Projects(source);

        var result = await Generate(Files(), app, lib);

        var application = result.Data.Projects.Single(p => p.Name == "App").Xaml.Single(f => f.File == "src/App/App.axaml");
        var finding = Assert.Single(application.Findings);
        Assert.Contains("ViewLocator convention was not recognized", finding, StringComparison.Ordinal);
        Assert.DoesNotContain(result.Data.Projects.Single(p => p.Name == "App").Xaml.Single(f => f.File.EndsWith("ShellView.axaml", StringComparison.Ordinal)).Findings, f => f.Contains("ViewLocator", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GenerateAsync_ApplicationWithoutALocator_HasNoConventionFinding()
    {
        var (app, lib) = Projects();
        var source = Files().Add("src/App/App.axaml", $"<Application {Header} />");

        var result = await Generate(source, app, lib);

        Assert.Empty(result.Data.Projects.Single(p => p.Name == "App").Xaml.Single(f => f.File == "src/App/App.axaml").Findings);
    }

    [Fact]
    public async Task GenerateAsync_Twice_GivesIdenticalXamlData()
    {
        var (app, lib) = Projects();

        var first = await Generate(Files(), app, lib);
        var second = await Generate(Files(), lib, app);

        static string Dump(GenerateResult r) => JsonSerializer.Serialize(
            r.Data.Projects.Select(p => (p.Name, Files: p.Xaml, Edges: p.Relationships.OrderBy(e => e.Type).ThenBy(e => e.FromId, StringComparer.Ordinal).ThenBy(e => e.ToId, StringComparer.Ordinal).ThenBy(e => e.Line))), JsonIndexWriter.Options);

        Assert.Equal(Dump(first), Dump(second));
    }

    [Fact]
    public async Task GenerateAsync_Output_HasNoAbsolutePaths()
    {
        var (app, lib) = Projects();

        var result = await Generate(Files(), app, lib);

        var json = JsonSerializer.Serialize(result.Data.Projects.SelectMany(p => p.Xaml), JsonIndexWriter.Options);
        Assert.DoesNotContain(TestCompilation.RepoRoot, json, StringComparison.Ordinal);
    }
}
