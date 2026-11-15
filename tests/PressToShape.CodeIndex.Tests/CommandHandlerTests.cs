using System.Linq;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Relationships;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class CommandHandlerTests
{
    private const string Stubs = """
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
            public class RelayCommand<T>(System.Action<T> execute) : CommandBase { }
            public class AsyncRelayCommand(System.Func<System.Threading.Tasks.Task> execute) : CommandBase { }
            public class AsyncRelayCommand<T>(System.Func<T, System.Threading.Tasks.Task> execute) : CommandBase { }
        }
        """;

    private static RelationshipRecord[] Handlers(string code)
    {
        var project = TestCompilation.Project("App", Stubs, code);
        var known = new SymbolExtractor(TestCompilation.RepoRoot).Extract(project).Select(s => s.Id).ToHashSet();
        return new RelationshipBuilder(known, TestCompilation.RepoRoot).Build(project).Where(e => e.Type == RelationshipType.ExecutesMethod).ToArray();
    }

    private static string Vm(string members) =>
        $"namespace App {{ using CommunityToolkit.Mvvm.Input; using System.Windows.Input; public class Vm {{ {members} }} }}";

    [Fact]
    public void Build_MethodGroupAssignedInAConstructor_GivesAVerifiedEdgeWithFileAndLine()
    {
        var edge = Assert.Single(Handlers(Vm("public ICommand SaveCommand { get; } public Vm() { SaveCommand = new RelayCommand(Save); } void Save() { }")));

        Assert.Equal(("P:App.Vm.SaveCommand", "M:App.Vm.Save"), (edge.FromId, edge.ToId));
        Assert.Equal(Confidence.Verified, edge.Confidence);
        Assert.Equal("new RelayCommand(method group)", edge.Reason);
        Assert.Equal("src/App/File1.cs", edge.File);
        Assert.Equal(1, edge.Line);
    }

    [Fact]
    public void Build_FieldAndPropertyInitializers_GiveEdges()
    {
        var edges = Handlers(Vm("public readonly ICommand Open = new RelayCommand(OpenFile); public ICommand Close { get; } = new RelayCommand(CloseFile); static void OpenFile() { } static void CloseFile() { }"));

        Assert.Equal(["F:App.Vm.Open->M:App.Vm.OpenFile", "P:App.Vm.Close->M:App.Vm.CloseFile"], edges.Select(e => $"{e.FromId}->{e.ToId}").OrderBy(x => x, System.StringComparer.Ordinal));
    }

    [Fact]
    public void Build_AsyncAndGenericForms_GiveEdges()
    {
        var edges = Handlers(Vm("""
            public ICommand A { get; } = new AsyncRelayCommand(RunAsync);
            public ICommand B { get; } = new RelayCommand<int>(Handle);
            public ICommand C { get; } = new AsyncRelayCommand<string>(HandleAsync);
            static System.Threading.Tasks.Task RunAsync() => System.Threading.Tasks.Task.CompletedTask;
            static void Handle(int x) { }
            static System.Threading.Tasks.Task HandleAsync(string s) => System.Threading.Tasks.Task.CompletedTask;
            """));

        Assert.Equal(["P:App.Vm.A->M:App.Vm.RunAsync", "P:App.Vm.B->M:App.Vm.Handle(System.Int32)", "P:App.Vm.C->M:App.Vm.HandleAsync(System.String)"], edges.Select(e => $"{e.FromId}->{e.ToId}").OrderBy(x => x, System.StringComparer.Ordinal));
    }

    [Fact]
    public void Build_Lambda_GivesNoEdge()
    {
        Assert.Empty(Handlers(Vm("public ICommand A { get; } = new RelayCommand(() => Save()); void Save() { }")));
    }

    [Fact]
    public void Build_ParameterAndLocal_GiveNoEdge()
    {
        var code = Vm("public ICommand A { get; } public ICommand B { get; } public Vm(System.Action apply) { A = new RelayCommand(apply); System.Action local = Save; B = new RelayCommand(local); } void Save() { }");

        Assert.Empty(Handlers(code));
    }

    [Fact]
    public void Build_MethodOfAnotherAssembly_GivesNoEdge()
    {
        Assert.Empty(Handlers(Vm("public ICommand A { get; } = new RelayCommand(System.GC.Collect);")));
    }

    [Fact]
    public void Build_AssignmentToALocal_GivesNoEdge()
    {
        Assert.Empty(Handlers(Vm("public void Make() { ICommand cmd = null!; cmd = new RelayCommand(Save); } void Save() { }")));
    }

    [Fact]
    public void Build_AnotherCommandType_GivesNoEdge()
    {
        var code = "namespace App { public class MyCommand(System.Action a) { } public class Vm { public object A { get; } = new MyCommand(Save); static void Save() { } } }";

        Assert.Empty(Handlers(code));
    }

    [Fact]
    public void Build_TwoCommandsInOneConstructor_GiveTwoEdges()
    {
        var edges = Handlers(Vm("public ICommand A { get; } public ICommand B { get; } public Vm() { A = new RelayCommand(One); B = new RelayCommand(Two); } void One() { } void Two() { }"));

        Assert.Equal(2, edges.Length);
        Assert.Equal(edges.Select(e => e.FromId).Distinct().Count(), edges.Length);
    }

    [Fact]
    public void FromGenerated_PropertyGeneratedFromARelayCommandMethod_GivesAVerifiedEdgeAtTheMethod()
    {
        var project = TestCompilation.WithGenerator(
            TestCompilation.Project("App", Stubs, "namespace App { public partial class Vm { [CommunityToolkit.Mvvm.Input.RelayCommand] private void Save() { } [CommunityToolkit.Mvvm.Input.RelayCommand] private System.Threading.Tasks.Task LoadAsync() => System.Threading.Tasks.Task.CompletedTask; } }"),
            new FixedSourceGenerator("Gen.g.cs", "namespace App { partial class Vm { public System.Windows.Input.ICommand SaveCommand { get; } = null!; public System.Windows.Input.ICommand LoadCommand { get; } = null!; } }"));
        var symbols = new SymbolExtractor(TestCompilation.RepoRoot).Extract(project);

        var edges = CommandHandlerLinker.FromGenerated(symbols);

        Assert.Equal(["P:App.Vm.LoadCommand->M:App.Vm.LoadAsync", "P:App.Vm.SaveCommand->M:App.Vm.Save"], edges.Select(e => $"{e.FromId}->{e.ToId}"));
        Assert.All(edges, e => Assert.Equal((Confidence.Verified, "[RelayCommand]", "src/App/File1.cs"), (e.Confidence, e.Reason, e.File)));
    }

    [Fact]
    public void FromGenerated_ObservablePropertyFieldAndHandWrittenMembers_GiveNoEdge()
    {
        var project = TestCompilation.Project("App", "namespace App { public class Vm { public int X { get; set; } private void Save() { } } }");
        var symbols = new SymbolExtractor(TestCompilation.RepoRoot).Extract(project);

        Assert.Empty(CommandHandlerLinker.FromGenerated(symbols));
        Assert.Throws<System.ArgumentNullException>(() => CommandHandlerLinker.FromGenerated(null!));
    }
}
