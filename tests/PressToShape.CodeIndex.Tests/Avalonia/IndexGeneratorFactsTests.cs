using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class IndexGeneratorFactsTests
{
    private const string CommandStubs = """
        namespace CommunityToolkit.Mvvm.Input
        {
            [System.AttributeUsage(System.AttributeTargets.Method)] public sealed class RelayCommandAttribute : System.Attribute { }
            public abstract class CommandBase : System.Windows.Input.ICommand
            {
                public event System.EventHandler? CanExecuteChanged;
                public bool CanExecute(object? parameter) => true;
                public void Execute(object? parameter) { }
            }
            public class RelayCommand(System.Action execute) : CommandBase { }
        }
        """;

    private const string AppSource = """
        namespace App
        {
            public class Ruler : Avalonia.Controls.Control
            {
                public static readonly Avalonia.StyledProperty<int> LevelProperty = Avalonia.AvaloniaProperty.Register<Ruler, int>(nameof(Level), 1);
                public int Level { get; set; }
            }

            public partial class ShellView : Avalonia.Controls.UserControl { }

            public partial class Vm
            {
                [CommunityToolkit.Mvvm.Input.RelayCommand] private void Save() { }
                public System.Windows.Input.ICommand Open { get; }
                public Vm() { Open = new CommunityToolkit.Mvvm.Input.RelayCommand(OpenFile); }
                void OpenFile() { }
            }

            public class MyApp : Avalonia.Application { public override void Initialize() { var vm = new Vm(); } }
        }
        """;

    private const string LibSource = "namespace Lib { public class FlagConverter : Avalonia.Data.Converters.IValueConverter { public object? Convert(object? v, System.Type t, object? p, System.Globalization.CultureInfo c) => v; } }";

    private sealed class FakeSource(params ProjectCompilation[] projects) : ICompilationSource
    {
        public Task<LoadResult> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new LoadResult(projects, []));
    }

    private static ProjectCompilation App()
    {
        var project = XamlTestHost.AppProject("App", CommandStubs, AppSource);
        return TestCompilation.WithGenerator(project, new FixedSourceGenerator("Gen.g.cs",
            "namespace App { partial class Vm { public System.Windows.Input.ICommand SaveCommand { get; } = null!; } }"));
    }

    private static Task<GenerateResult> Generate(IXamlFileSource? xaml, params ProjectCompilation[] projects) =>
        new IndexGenerator(new FakeSource(projects), TestCompilation.RepoRoot, xaml).GenerateAsync(CancellationToken.None);

    [Fact]
    public async Task GenerateAsync_MergesFactsOfAllProjectsSorted()
    {
        var result = await Generate(null, App(), XamlTestHost.AppProject("Lib", LibSource));

        var facts = result.Data.Avalonia!;

        Assert.Equal(["T:App.Ruler", "T:App.ShellView"], facts.Controls.Select(c => c.Id));
        Assert.Equal(["T:Lib.FlagConverter"], facts.Converters.Select(c => c.Id));
        Assert.Equal(["T:App.MyApp"], facts.Applications.Select(a => a.Id));
    }

    [Fact]
    public async Task GenerateAsync_ViewWithAXamlFile_IsNotACustomControl()
    {
        var xaml = new InMemoryXamlFileSource().Add("src/App/Views/ShellView.axaml",
            "<UserControl xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" x:Class=\"App.ShellView\" />");

        var result = await Generate(xaml, App(), XamlTestHost.AppProject("Lib", LibSource));

        Assert.Equal(["T:App.Ruler"], result.Data.Avalonia!.Controls.Select(c => c.Id));
    }

    [Fact]
    public async Task GenerateAsync_GeneratedCommandAndMethodGroupCommand_GetExecutesMethodEdgesInTheOwningProject()
    {
        var result = await Generate(null, App(), XamlTestHost.AppProject("Lib", LibSource));

        var handlers = result.Data.Projects.Single(p => p.Name == "App").Relationships.Where(e => e.Type == RelationshipType.ExecutesMethod).ToList();

        Assert.Equal(["P:App.Vm.Open->M:App.Vm.OpenFile", "P:App.Vm.SaveCommand->M:App.Vm.Save"], handlers.Select(e => $"{e.FromId}->{e.ToId}").OrderBy(x => x, System.StringComparer.Ordinal));
        Assert.Equal(["[RelayCommand]", "new RelayCommand(method group)"], handlers.Select(e => e.Reason).OrderBy(x => x, System.StringComparer.Ordinal));
        Assert.DoesNotContain(result.Data.Projects.Single(p => p.Name == "Lib").Relationships, e => e.Type == RelationshipType.ExecutesMethod);
    }

    [Fact]
    public async Task GenerateAsync_ProjectWithoutAvaloniaTypes_HasEmptyFacts()
    {
        var result = await Generate(null, XamlTestHost.AppProject("Lib", "namespace Lib { public class Plain { } }"));

        Assert.Empty(result.Data.Avalonia!.Controls);
        Assert.Empty(result.Data.Avalonia.Converters);
        Assert.Empty(result.Data.Avalonia.Applications);
    }

    [Fact]
    public async Task GenerateAsync_ShuffledProjects_GiveTheSameFactsAndEdges()
    {
        var first = await Generate(null, App(), XamlTestHost.AppProject("Lib", LibSource));
        var second = await Generate(null, XamlTestHost.AppProject("Lib", LibSource), App());

        Assert.Equal(first.Data.Avalonia!.Controls.Select(c => c.Id), second.Data.Avalonia!.Controls.Select(c => c.Id));
        Assert.Equal(
            first.Data.Projects.SelectMany(p => p.Relationships).Where(e => e.Type == RelationshipType.ExecutesMethod).Select(e => e.FromId).OrderBy(x => x, System.StringComparer.Ordinal),
            second.Data.Projects.SelectMany(p => p.Relationships).Where(e => e.Type == RelationshipType.ExecutesMethod).Select(e => e.FromId).OrderBy(x => x, System.StringComparer.Ordinal));
    }

    [Fact]
    public async Task GenerateAsync_FactsAreNotWrittenAsJson()
    {
        var result = await Generate(null, App(), XamlTestHost.AppProject("Lib", LibSource));
        var sink = new PressToShape.CodeIndex.Output.InMemoryFileSink();

        IndexGenerator.Write(result.Data, sink);

        Assert.DoesNotContain(sink.Files.Keys, k => k.Contains("avalonia", System.StringComparison.OrdinalIgnoreCase) && k.EndsWith(".json", System.StringComparison.Ordinal));
    }
}
