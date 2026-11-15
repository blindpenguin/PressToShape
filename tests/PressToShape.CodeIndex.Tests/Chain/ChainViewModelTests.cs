using System;
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

public class ChainViewModelTests
{
    private sealed class FakeProjects(params ProjectCompilation[] projects) : ICompilationSource
    {
        public Task<LoadResult> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new LoadResult(projects, []));
    }

    private static ChainNode Root(string query)
    {
        var result = ChainViewTests.Sample().Build(query);
        Assert.Equal(ChainStatus.Found, result.Status);
        return result.Root!;
    }

    private static ChainNode Find(ChainNode root, string label) => ChainViewTests.Find(root, label);

    [Fact]
    public void Build_ViewModel_ListsTheViewsThatUseItWithEvidence()
    {
        var root = Root("ShellViewModel");

        Assert.Equal(("View model ShellViewModel", "T:App.ViewModels.ShellViewModel"), (root.Label, root.Id));
        var views = Find(root, "Views using it");
        var view = Assert.Single(views.Children);
        Assert.Equal(("View ShellView", Confidence.Verified, "x:DataType"), (view.Label, view.Confidence, view.Reason));
    }

    [Fact]
    public void Build_ViewModelWithAVerifiedAndAnInferredLink_ListsBoth()
    {
        var views = Find(Root("DetailViewModel"), "Views using it");

        Assert.Equal("Views using it (2)", views.Label);
        Assert.Equal([Confidence.Verified, Confidence.Inferred], views.Children.Select(c => c.Confidence!.Value).OrderBy(c => c));
        Assert.Contains(views.Children, c => c.Confidence == Confidence.Inferred && c.Reason!.Contains("ViewLocator", StringComparison.Ordinal));
    }

    [Fact]
    public void Build_ViewModel_ListsCommandsWithHandlersAndTheXamlPlacesThatBindThem()
    {
        var commands = Find(Root("ShellViewModel"), "Commands");

        Assert.Equal("Commands (3)", commands.Label);
        Assert.Equal(["Command ApplyCommand", "Command SaveCommand", "Command ZoomCommand"], commands.Children.Select(c => c.Label));
        var save = commands.Children.Single(c => c.Label == "Command SaveCommand");
        Assert.Equal("Handler Save", save.Children[0].Label);
        var bound = save.Children.Single(c => c.Label.StartsWith("Bound at", StringComparison.Ordinal));
        Assert.StartsWith("src/App/Views/ShellView.axaml:", bound.Location, StringComparison.Ordinal);
        Assert.Equal(Confidence.Verified, bound.Confidence);
        var apply = commands.Children.Single(c => c.Label == "Command ApplyCommand");
        Assert.Equal("Handler not statically known", apply.Children[0].Label);
    }

    [Fact]
    public void Build_ViewModel_HasServicesModelsAndTests()
    {
        var root = Root("ShellViewModel");

        Assert.Equal("Services (2)", Find(root, "Services").Label);
        Assert.Equal("Models (1)", Find(root, "Models").Label);
        Assert.Equal("Test class ShellViewModelTests", Find(root, "Tests").Children.Single().Label);
    }

    [Fact]
    public void Build_GeneratedCommand_ShowsHandlerBindingsAndNoTests()
    {
        var root = Root("P:App.ViewModels.ShellViewModel.SaveCommand");

        Assert.Equal(("Command ShellViewModel.SaveCommand", "P:App.ViewModels.ShellViewModel.SaveCommand"), (root.Label, root.Id));
        var handler = root.Children[0];
        Assert.Equal(("Handler Save", "M:App.ViewModels.ShellViewModel.Save", Confidence.Verified, "[RelayCommand]"), (handler.Label, handler.Id, handler.Confidence, handler.Reason));
        var bound = Find(root, "Bound at");
        Assert.Equal("Bound at (1)", bound.Label);
        Assert.Equal("ShellView.axaml Button[1] @Command", bound.Children.Single().Label);
        Assert.Equal("Tests (none)", root.Children[^1].Label);
    }

    [Fact]
    public void Build_MethodGroupCommandAndParameterCommand_ShowTheirHandlers()
    {
        var zoom = Root("ZoomCommand");
        var apply = Root("ApplyCommand");

        Assert.Equal(("Handler ZoomIn", "new RelayCommand(method group)"), (zoom.Children[0].Label, zoom.Children[0].Reason));
        Assert.Equal("Handler not statically known", apply.Children[0].Label);
        Assert.Null(apply.Children[0].Id);
        Assert.StartsWith("not statically known", apply.Children[0].Reason, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Build_CommandBoundByAButtonAndAKeyBinding_ListsBothPlaces()
    {
        const string Source = """
            namespace App.ViewModels
            {
                public class Vm
                {
                    public System.Windows.Input.ICommand Go { get; } = new CommunityToolkit.Mvvm.Input.RelayCommand(GoNow);
                    public static void GoNow() { }
                }
            }
            namespace App.Views { public partial class V : Avalonia.Controls.UserControl { } }
            """;
        var xaml = new InMemoryXamlFileSource().Add(
            "src/App/V.axaml",
            "<UserControl xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:vm=\"using:App.ViewModels\" x:Class=\"App.Views.V\" x:DataType=\"vm:Vm\">\n"
            + "<UserControl.KeyBindings><KeyBinding Gesture=\"Ctrl+G\" Command=\"{Binding Go}\" /></UserControl.KeyBindings>\n"
            + "<Button Command=\"{Binding Go}\" />\n</UserControl>");
        var app = XamlTestHost.AppProject("App", MvvmSampleData.ToolkitStubs, Source);
        var data = (await new IndexGenerator(new FakeProjects(app), TestCompilation.RepoRoot, xaml).GenerateAsync(CancellationToken.None)).Data;
        var sink = new InMemoryFileSink();
        IndexGenerator.Write(data, sink);

        var result = new ChainBuilder(new IndexReader(sink)).Build("P:App.ViewModels.Vm.Go");

        var bound = Find(result.Root!, "Bound at");
        Assert.Equal("Bound at (2)", bound.Label);
        Assert.Equal("Handler GoNow", result.Root!.Children[0].Label);
    }

    [Fact]
    public void Build_SymbolsThatAreNotViewsViewModelsOrCommands_AreNotSupported()
    {
        var chain = ChainViewTests.Sample();

        foreach (var query in new[] { "Title", "IFileStore", "OrphanViewModel", "Ruler", "X:src/App/App.axaml" })
        {
            var result = chain.Build(query);

            Assert.Equal(ChainStatus.NotSupported, result.Status);
            Assert.Null(result.Root);
            Assert.Contains("chain accepts a view", result.Message, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Build_NotSupportedMessage_NamesTheSymbolType()
    {
        var result = ChainViewTests.Sample().Build("Title");

        Assert.StartsWith("'Title' is a Property.", result.Message, StringComparison.Ordinal);
    }
}
