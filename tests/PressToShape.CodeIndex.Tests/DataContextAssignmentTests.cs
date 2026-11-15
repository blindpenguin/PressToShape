using System.Linq;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Relationships;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class DataContextAssignmentTests
{
    private const string Stubs = """
        namespace Avalonia.Controls
        {
            public class Control { public object? DataContext { get; set; } }
            public class UserControl : Control { }
            public class Window : Control { }
        }
        namespace App.ViewModels
        {
            public interface IMainViewModel { }
            public class MainViewModel : IMainViewModel { }
            public class ShellViewModel { }
        }
        namespace App.Views
        {
            public class MainView : Avalonia.Controls.UserControl { }
            public class MainWindow : Avalonia.Controls.Window { }
            public class NotAView { public object? DataContext { get; set; } }
        }
        """;

    private static RelationshipRecord[] Links(string code)
    {
        var project = TestCompilation.Project("App", Stubs, code);
        var known = new SymbolExtractor(TestCompilation.RepoRoot).Extract(project).Select(s => s.Id).ToHashSet();
        return new RelationshipBuilder(known, TestCompilation.RepoRoot).Build(project)
            .Where(e => e.Type == RelationshipType.UsesViewModel)
            .ToArray();
    }

    private static string Wrap(string body) => $"namespace App {{ using App.ViewModels; using App.Views; public class Startup {{ public void Run() {{ {body} }} }} }}";

    [Fact]
    public void Build_ObjectInitializer_GivesAVerifiedLinkWithFileAndLine()
    {
        var link = Assert.Single(Links(Wrap("var view = new MainWindow { DataContext = new ShellViewModel() };")));

        Assert.Equal("T:App.Views.MainWindow", link.FromId);
        Assert.Equal("T:App.ViewModels.ShellViewModel", link.ToId);
        Assert.Equal(Confidence.Verified, link.Confidence);
        Assert.Equal("DataContext assignment", link.Reason);
        Assert.Equal("src/App/File1.cs", link.File);
        Assert.Equal(1, link.Line);
    }

    [Fact]
    public void Build_PlainAssignment_GivesALink()
    {
        var link = Assert.Single(Links(Wrap("var view = new MainView(); view.DataContext = new MainViewModel();")));

        Assert.Equal(("T:App.Views.MainView", "T:App.ViewModels.MainViewModel"), (link.FromId, link.ToId));
    }

    [Fact]
    public void Build_ValueThroughAVariable_UsesItsStaticType()
    {
        var link = Assert.Single(Links(Wrap("var vm = new ShellViewModel(); var view = new MainView(); view.DataContext = vm;")));

        Assert.Equal("T:App.ViewModels.ShellViewModel", link.ToId);
    }

    [Fact]
    public void Build_ValueOfASolutionInterfaceType_LinksToTheInterface()
    {
        var link = Assert.Single(Links(Wrap("IMainViewModel vm = new MainViewModel(); var view = new MainView(); view.DataContext = vm;")));

        Assert.Equal("T:App.ViewModels.IMainViewModel", link.ToId);
    }

    [Fact]
    public void Build_ValueOfTypeObject_GivesNoLink()
    {
        Assert.Empty(Links(Wrap("object vm = new ShellViewModel(); var view = new MainView(); view.DataContext = vm;")));
    }

    [Fact]
    public void Build_FrameworkValueType_GivesNoLink()
    {
        Assert.Empty(Links(Wrap("var view = new MainView { DataContext = new System.Text.StringBuilder() };")));
    }

    [Fact]
    public void Build_TargetThatIsNotAControl_GivesNoLink()
    {
        Assert.Empty(Links(Wrap("var other = new NotAView { DataContext = new ShellViewModel() };")));
    }

    [Fact]
    public void Build_OtherPropertyNamedLikeADataContext_GivesNoLink()
    {
        var code = "namespace App { using App.ViewModels; public class Odd : Avalonia.Controls.Control { public object? Tag { get; set; } public void Run() { Tag = new ShellViewModel(); } } }";

        Assert.Empty(Links(code));
    }

    [Fact]
    public void Build_AssignmentInsideTheViewClass_UsesTheViewClass()
    {
        var code = "namespace App { using App.ViewModels; public class Custom : Avalonia.Controls.UserControl { public Custom() { DataContext = new ShellViewModel(); } } }";

        var link = Assert.Single(Links(code));

        Assert.Equal(("T:App.Custom", "T:App.ViewModels.ShellViewModel"), (link.FromId, link.ToId));
    }

    [Fact]
    public void Build_AssignmentInALambda_IsFound()
    {
        var link = Assert.Single(Links(Wrap("System.Func<MainView> make = () => new MainView { DataContext = new MainViewModel() };")));

        Assert.Equal("T:App.Views.MainView", link.FromId);
    }

    [Fact]
    public void Build_SeveralAssignments_AllAppearOnceAndInOrder()
    {
        var links = Links(Wrap("var a = new MainView { DataContext = new MainViewModel() };\nvar b = new MainWindow { DataContext = new ShellViewModel() };\nvar c = new MainWindow { DataContext = new ShellViewModel() };"));

        Assert.Equal(3, links.Length);
        Assert.Equal([1, 2, 3], links.Select(l => l.Line).OrderBy(l => l));
    }

    [Fact]
    public void Build_NullValue_GivesNoLink()
    {
        Assert.Empty(Links(Wrap("var view = new MainView { DataContext = null };")));
    }
}
