using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Chain;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Query;
using PressToShape.CodeIndex.Xaml;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class ChainViewTests
{
    private sealed class FakeProjects(params ProjectCompilation[] projects) : ICompilationSource
    {
        public Task<LoadResult> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new LoadResult(projects, []));
    }

    internal static ChainBuilder Sample()
    {
        var sink = MvvmSampleData.Index();
        return new ChainBuilder(new IndexReader(sink));
    }

    internal static IEnumerable<ChainNode> All(ChainNode node)
    {
        yield return node;
        foreach (var descendant in node.Children.SelectMany(All))
        {
            yield return descendant;
        }
    }

    internal static ChainNode Find(ChainNode root, string labelStart) =>
        All(root).First(n => n.Label.StartsWith(labelStart, StringComparison.Ordinal));

    private static ChainNode ShellRoot(int depth = 1)
    {
        var result = Sample().Build("ShellView", depth);
        Assert.Equal(ChainStatus.Found, result.Status);
        return result.Root!;
    }

    [Fact]
    public void Build_View_HasTheViewItsXamlFileAndItsViewModelWithEvidence()
    {
        var root = ShellRoot();

        Assert.Equal(("View ShellView", "T:App.Views.ShellView"), (root.Label, root.Id));
        var xaml = Find(root, "XAML file");
        Assert.Equal(("X:src/App/Views/ShellView.axaml", Confidence.Verified, "x:Class"), (xaml.Id, xaml.Confidence, xaml.Reason));
        var viewModel = root.Children.Single(c => c.Label.StartsWith("View model", StringComparison.Ordinal));
        Assert.Equal(("View model ShellViewModel", "T:App.ViewModels.ShellViewModel", Confidence.Verified, "x:DataType"), (viewModel.Label, viewModel.Id, viewModel.Confidence, viewModel.Reason));
    }

    [Fact]
    public void Build_View_ListsBindingsWithHandlersOfCommands()
    {
        var bindings = Find(ShellRoot(), "Bindings");

        Assert.Equal("Bindings (6)", bindings.Label);
        var save = bindings.Children.Single(c => c.Label.StartsWith("Button.Command = Binding SaveCommand", StringComparison.Ordinal));
        Assert.Equal((Confidence.Verified, "x:DataType vm:ShellViewModel"), (save.Confidence, save.Reason));
        Assert.StartsWith("src/App/Views/ShellView.axaml:", save.Location, StringComparison.Ordinal);
        var handler = Assert.Single(save.Children);
        Assert.Equal(("Handler Save", "M:App.ViewModels.ShellViewModel.Save", Confidence.Verified, "[RelayCommand]"), (handler.Label, handler.Id, handler.Confidence, handler.Reason));
    }

    [Fact]
    public void Build_View_MethodGroupHandlerAndUnknownHandler()
    {
        var bindings = Find(ShellRoot(), "Bindings");

        var zoom = bindings.Children.Single(c => c.Label.Contains("ZoomCommand", StringComparison.Ordinal)).Children.Single();
        var apply = bindings.Children.Single(c => c.Label.Contains("ApplyCommand", StringComparison.Ordinal)).Children.Single();

        Assert.Equal(("Handler ZoomIn", "new RelayCommand(method group)"), (zoom.Label, zoom.Reason));
        Assert.Equal("Handler not statically known", apply.Label);
        Assert.StartsWith("not statically known", apply.Reason, StringComparison.Ordinal);
        Assert.Null(apply.Id);
    }

    [Fact]
    public void Build_View_ListsServicesWithImplementationsAndCreationSites()
    {
        var services = Find(ShellRoot(), "Services");

        Assert.Equal("Services (2)", services.Label);
        var store = services.Children.Single(c => c.Label.StartsWith("Service IFileStore", StringComparison.Ordinal));
        Assert.Equal("Service IFileStore (interface)", store.Label);
        var implementation = Assert.Single(store.Children);
        Assert.Equal(("Implemented by LocalFileStore", Confidence.Verified), (implementation.Label, implementation.Confidence));
        var created = Assert.Single(implementation.Children);
        Assert.Equal(("Created in MyApp.OnFrameworkInitializationCompleted", Confidence.Verified), (created.Label, created.Confidence));
        Assert.StartsWith("src/App/File1.cs:", created.Location, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_View_ListsModelsOfProjectsWithoutViews()
    {
        var models = Find(ShellRoot(), "Models");

        var document = Assert.Single(models.Children);
        Assert.Equal(("T:Core.Document", Confidence.Verified), (document.Id, document.Confidence));
        Assert.StartsWith("Model Document (Core, ", document.Label, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_View_ListsChildViewsWithVerifiedAndInferredLinks()
    {
        var children = Find(ShellRoot(), "Child views");

        Assert.Equal("Child views (2)", children.Label);
        var panel = children.Children.Single(c => c.Label == "Child view PanelView");
        var detail = children.Children.Single(c => c.Label == "Child view DetailView");
        Assert.Equal(Confidence.Verified, panel.Confidence);
        Assert.StartsWith("DataContext binding at src/App/Views/ShellView.axaml:", panel.Reason, StringComparison.Ordinal);
        Assert.Equal(Confidence.Inferred, detail.Confidence);
        Assert.Contains("ViewLocator", detail.Reason, StringComparison.Ordinal);
        Assert.Equal("View model DetailViewModel", Assert.Single(detail.Children).Label);
    }

    [Fact]
    public void Build_View_ListsResourcesConvertersAndUnresolvedThemeKeys()
    {
        var look = Find(ShellRoot(), "Look and feel");

        Assert.Equal("Look and feel (4)", look.Label);
        var padding = look.Children.Single(c => c.Label == "StaticResource PanelPadding");
        Assert.Equal((Confidence.Verified, "ReferencesResource"), (padding.Confidence, padding.Reason));
        Assert.StartsWith("src/App/App.axaml:", padding.Location, StringComparison.Ordinal);
        var window = look.Children.Single(c => c.Label == "DynamicResource WindowBrush");
        Assert.Equal(2, window.Children.Count);
        var accent = look.Children.Single(c => c.Label == "DynamicResource SystemAccentColor");
        Assert.Equal(Confidence.Inferred, accent.Confidence);
        Assert.Contains("not defined in this solution", accent.Reason, StringComparison.Ordinal);
        var converter = look.Children.Single(c => c.Label.StartsWith("Converter", StringComparison.Ordinal));
        Assert.Equal(("Converter FlagConverter", "T:App.Converters.FlagConverter"), (converter.Label, converter.Id));
    }

    [Fact]
    public void Build_View_ListsTestsOfTheViewModelAndSaysNoneForTheView()
    {
        var root = ShellRoot();

        var viewModelTests = Find(root.Children.Single(c => c.Label.StartsWith("View model", StringComparison.Ordinal)), "Tests");
        Assert.Equal("Test class ShellViewModelTests", Assert.Single(viewModelTests.Children).Label);
        Assert.Equal("Tests (none)", root.Children.Last().Label);
    }

    [Fact]
    public void Build_ViewWithTwoLinksToTheSameViewModel_UsesTheVerifiedOneAndNotesTheOther()
    {
        var root = Sample().Build("DetailView").Root!;

        var viewModel = root.Children.Single(c => c.Label.StartsWith("View model", StringComparison.Ordinal));

        Assert.Equal((Confidence.Verified, "x:DataType"), (viewModel.Confidence, viewModel.Reason));
        var also = viewModel.Children.Single(c => c.Label == "Also linked");
        Assert.Equal(Confidence.Inferred, also.Confidence);
        Assert.Contains("ViewLocator", also.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_ByXamlFileId_GivesTheSameChain()
    {
        var byClass = Sample().Build("ShellView").Root!;
        var byFile = Sample().Build("X:src/App/Views/ShellView.axaml").Root!;

        Assert.Equal(JsonSerializer.Serialize(byClass), JsonSerializer.Serialize(byFile));
    }

    [Fact]
    public void Build_XamlFileWithoutAClass_IsNotSupported()
    {
        var result = Sample().Build("X:src/App/App.axaml");

        Assert.Equal(ChainStatus.NotSupported, result.Status);
        Assert.Contains("chain accepts a view", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_UnknownAndAmbiguousNames_BehaveLikeShow()
    {
        var chain = Sample();

        var unknown = chain.Build("Nope");
        var ambiguous = chain.Build("Save");

        Assert.Equal(ChainStatus.NotFound, unknown.Status);
        Assert.Contains("Try: find Nope", unknown.Message, StringComparison.Ordinal);
        Assert.Equal(ChainStatus.Ambiguous, ambiguous.Status);
        Assert.True(ambiguous.Candidates.Count >= 2);
    }

    [Fact]
    public void Build_DepthOne_DoesNotExpandChildViewsAndDepthTwoDoes()
    {
        var shallow = Find(ShellRoot(1), "Child views");
        var deep = Find(ShellRoot(2), "Child views");

        Assert.DoesNotContain(All(shallow), n => n.Label.StartsWith("XAML file", StringComparison.Ordinal));
        Assert.Contains(All(deep), n => n.Label == "XAML file src/App/Views/PanelView.axaml");
    }

    [Fact]
    public void Build_DepthIsLimited()
    {
        var chain = Sample();

        Assert.Equal(JsonSerializer.Serialize(chain.Build("ShellView", 5).Root), JsonSerializer.Serialize(chain.Build("ShellView", 99).Root));
        Assert.Equal(JsonSerializer.Serialize(chain.Build("ShellView", 1).Root), JsonSerializer.Serialize(chain.Build("ShellView", -3).Root));
    }

    [Fact]
    public async Task Build_ViewsThatHostEachOther_StopAtTheSecondVisit()
    {
        const string Source = """
            namespace App.ViewModels { public class AVm { public BVm B { get; } = new(); } public class BVm { public AVm A { get; } = new(); } }
            namespace App.Views { public partial class AView : Avalonia.Controls.UserControl { } public partial class BView : Avalonia.Controls.UserControl { } }
            """;
        const string Header = "xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:vm=\"using:App.ViewModels\" xmlns:views=\"using:App.Views\"";
        var xaml = new InMemoryXamlFileSource()
            .Add("src/App/A.axaml", $"<UserControl {Header} x:Class=\"App.Views.AView\" x:DataType=\"vm:AVm\"><views:BView DataContext=\"{{Binding B}}\" /></UserControl>")
            .Add("src/App/B.axaml", $"<UserControl {Header} x:Class=\"App.Views.BView\" x:DataType=\"vm:BVm\"><views:AView DataContext=\"{{Binding A}}\" /></UserControl>");
        var data = (await new IndexGenerator(new FakeProjects(XamlTestHost.AppProject("App", Source)), TestCompilation.RepoRoot, xaml).GenerateAsync(CancellationToken.None)).Data;
        var sink = new InMemoryFileSink();
        IndexGenerator.Write(data, sink);

        var root = new ChainBuilder(new IndexReader(sink)).Build("AView", 5).Root!;

        var labels = All(root).Where(n => n.Label.StartsWith("Child view ", StringComparison.Ordinal)).Select(n => n.Label).ToList();
        Assert.Equal(["Child view BView", "Child view AView"], labels);
    }

    [Fact]
    public void Build_Twice_GivesTheSameTreeAndTheViewHasNoChildViewsWhenItHostsNone()
    {
        var chain = Sample();

        Assert.Equal(JsonSerializer.Serialize(chain.Build("ShellView").Root), JsonSerializer.Serialize(chain.Build("ShellView").Root));
        Assert.Equal("Child views (none)", Find(chain.Build("PanelView").Root!, "Child views").Label);
    }
}
