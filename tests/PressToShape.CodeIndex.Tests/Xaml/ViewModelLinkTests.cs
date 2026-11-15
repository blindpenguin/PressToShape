using System.Linq;
using PressToShape.CodeIndex.Model;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class ViewModelLinkTests
{
    private const string Source = """
        namespace App.ViewModels
        {
            public class ViewModelBase { }
            public class ShellViewModel : ViewModelBase
            {
                public MenuViewModel Menu { get; } = new();
                public PanelViewModel Panel { get; } = new();
                public OrphanViewModel Orphan { get; } = new();
                public System.Collections.Generic.List<ItemViewModel> Items { get; } = [];
                public string Title { get; } = "";
            }
            public class MenuViewModel : ViewModelBase { }
            public class PanelViewModel : ViewModelBase { }
            public class OrphanViewModel : ViewModelBase { }
            public class ItemViewModel : ViewModelBase { }
            public class LookalikeViewModel : ViewModelBase { }
        }
        namespace App.Views
        {
            public partial class ShellView : Avalonia.Controls.UserControl { }
            public partial class MenuView : Avalonia.Controls.UserControl { }
            public partial class PanelView : Avalonia.Controls.UserControl { }
            public partial class ItemView : Avalonia.Controls.UserControl { }
            public partial class LookalikeView : Avalonia.Controls.UserControl { }
            public partial class UnrelatedView : Avalonia.Controls.UserControl { }
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

                public bool Match(object? data) => data is App.ViewModels.ViewModelBase;
            }
        }
        """;

    private static string App => $"<Application {XamlTestHost.Header} xmlns:local=\"using:App\"><Application.DataTemplates><local:ViewLocator /></Application.DataTemplates></Application>";

    private static string View(string body, string className = "App.Views.ShellView", string dataType = "vm:ShellViewModel", string extraAttributes = "") =>
        $"<UserControl {XamlTestHost.Header} xmlns:vm=\"using:App.ViewModels\" xmlns:views=\"using:App.Views\" x:Class=\"{className}\" {(dataType.Length > 0 ? $"x:DataType=\"{dataType}\"" : string.Empty)} {extraAttributes}>\n{body}\n</UserControl>";

    private static XamlTestHost Host(string shell, params (string Path, string Xml)[] more) =>
        XamlTestHost.Create(Source, [("src/App/App.axaml", App), ("src/App/Views/ShellView.axaml", shell), .. more]);

    private static string[] Edges(XamlTestHost host, string path = "src/App/Views/ShellView.axaml") =>
        host.Resolve(path).Relationships.Select(XamlTestHost.Edge).ToArray();

    [Fact]
    public void Resolve_XClass_GivesACodeBehindLinkFromTheFileToTheClass()
    {
        var host = Host(View(string.Empty));

        var edge = Assert.Single(host.Resolve("src/App/Views/ShellView.axaml").Relationships, e => e.Type == RelationshipType.CodeBehind);

        Assert.Equal("X:src/App/Views/ShellView.axaml", edge.FromId);
        Assert.Equal("T:App.Views.ShellView", edge.ToId);
        Assert.Equal(Confidence.Verified, edge.Confidence);
        Assert.Equal("x:Class", edge.Reason);
        Assert.Equal(1, edge.Line);
    }

    [Fact]
    public void Resolve_FileRecord_HasClassDataTypeAndIds()
    {
        var file = Host(View(string.Empty)).Resolve("src/App/Views/ShellView.axaml").File;

        Assert.Equal("X:src/App/Views/ShellView.axaml", file.Id);
        Assert.Equal("UserControl", file.Root);
        Assert.Equal("App.Views.ShellView", file.Class);
        Assert.Equal("T:App.Views.ShellView", file.ClassId);
        Assert.Equal("vm:ShellViewModel", file.DataType);
        Assert.Equal("T:App.ViewModels.ShellViewModel", file.DataTypeId);
        Assert.Equal("using:App.ViewModels", file.Namespaces["vm"]);
        Assert.Empty(file.Findings);
    }

    [Fact]
    public void Resolve_Elements_HaveIdsParentsTypesAndNames()
    {
        var file = Host(View("<StackPanel><views:MenuView x:Name=\"MenuHost\" /></StackPanel>")).Resolve("src/App/Views/ShellView.axaml").File;

        Assert.Equal(["X:src/App/Views/ShellView.axaml#/UserControl", "X:src/App/Views/ShellView.axaml#/UserControl/StackPanel[1]", "X:src/App/Views/ShellView.axaml#/UserControl/StackPanel[1]/views:MenuView[1]"], file.Elements.Select(e => e.Id));
        var menu = file.Elements[2];
        Assert.Equal(file.Elements[1].Id, menu.ParentId);
        Assert.Equal("T:App.Views.MenuView", menu.TypeId);
        Assert.Equal("MenuHost", menu.Name);
        Assert.Null(file.Elements[0].ParentId);
        Assert.Null(file.Elements[1].TypeId);
    }

    [Fact]
    public void Resolve_XDataType_IsAVerifiedViewModelLink()
    {
        var edge = Host(View(string.Empty)).Resolve("src/App/Views/ShellView.axaml").Relationships.Single(e => e.Type == RelationshipType.UsesViewModel);

        Assert.Equal(("T:App.Views.ShellView", "T:App.ViewModels.ShellViewModel"), (edge.FromId, edge.ToId));
        Assert.Equal(Confidence.Verified, edge.Confidence);
        Assert.Equal("x:DataType", edge.Reason);
    }

    [Fact]
    public void Resolve_DataContextBindingOnAChildView_IsAVerifiedLink()
    {
        var host = Host(View("<views:MenuView DataContext=\"{Binding Menu}\" />\n<views:PanelView DataContext=\"{Binding Panel}\" />"));

        var edges = Edges(host);

        Assert.Contains("UsesViewModel:T:App.Views.MenuView->T:App.ViewModels.MenuViewModel [Verified]", edges);
        Assert.Contains("UsesViewModel:T:App.Views.PanelView->T:App.ViewModels.PanelViewModel [Verified]", edges);
        var menu = host.Resolve("src/App/Views/ShellView.axaml").Relationships.Single(e => e.ToId == "T:App.ViewModels.MenuViewModel");
        Assert.StartsWith("DataContext binding at src/App/Views/ShellView.axaml:", menu.Reason, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_DataContextBindingThatDoesNotResolve_GivesNoLink()
    {
        var edges = Edges(Host(View("<views:MenuView DataContext=\"{Binding Nope}\" />\n<views:PanelView DataContext=\"{Binding Title}\" />")));

        Assert.DoesNotContain(edges, e => e.StartsWith("UsesViewModel", System.StringComparison.Ordinal) && e.Contains("MenuView", System.StringComparison.Ordinal));
        Assert.DoesNotContain(edges, e => e.StartsWith("UsesViewModel", System.StringComparison.Ordinal) && e.Contains("PanelView", System.StringComparison.Ordinal));
    }

    [Fact]
    public void Resolve_DataContextBindingWithoutADataType_GivesNoLink()
    {
        var edges = Edges(Host(View("<views:MenuView DataContext=\"{Binding Menu}\" />", dataType: string.Empty)));

        Assert.DoesNotContain(edges, e => e.Contains("MenuViewModel", System.StringComparison.Ordinal));
    }

    [Fact]
    public void Resolve_DataContextBindingOnANonViewElement_GivesNoLink()
    {
        var edges = Edges(Host(View("<StackPanel DataContext=\"{Binding Menu}\" />")));

        Assert.DoesNotContain(edges, e => e.Contains("MenuViewModel", System.StringComparison.Ordinal));
    }

    [Fact]
    public void Resolve_DesignTimeDataContext_IsInferredAndSaysSo()
    {
        var xml = View("<Design.DataContext><vm:MenuViewModel /></Design.DataContext>");

        var edge = Host(xml).Resolve("src/App/Views/ShellView.axaml").Relationships.Single(e => e.Type == RelationshipType.UsesViewModel && e.ToId == "T:App.ViewModels.MenuViewModel");

        Assert.Equal(RelationshipType.UsesViewModel, edge.Type);
        Assert.Equal("T:App.Views.ShellView", edge.FromId);
        Assert.Equal(Confidence.Inferred, edge.Confidence);
        Assert.Contains("design-time only", edge.Reason, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_DesignInstanceAttribute_IsAlsoDesignTimeOnly()
    {
        var xml = View(string.Empty, extraAttributes: "d:DataContext=\"{d:DesignInstance vm:PanelViewModel}\"");

        var edge = Host(xml).Resolve("src/App/Views/ShellView.axaml").Relationships.Single(e => e.ToId == "T:App.ViewModels.PanelViewModel");

        Assert.Equal(Confidence.Inferred, edge.Confidence);
        Assert.Contains("design-time only", edge.Reason, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_DataTemplateDataType_IsAVerifiedUsesTypeLink()
    {
        var xml = View("<UserControl.Resources><DataTemplate x:Key=\"T\" DataType=\"vm:ItemViewModel\"><TextBlock /></DataTemplate></UserControl.Resources>");

        var edge = Host(xml).Resolve("src/App/Views/ShellView.axaml").Relationships.Single(e => e.Type == RelationshipType.UsesType);

        Assert.Equal("X:src/App/Views/ShellView.axaml#/UserControl/UserControl.Resources[1]/DataTemplate[1]", edge.FromId);
        Assert.Equal("T:App.ViewModels.ItemViewModel", edge.ToId);
        Assert.Equal(Confidence.Verified, edge.Confidence);
        Assert.Equal("DataTemplate DataType", edge.Reason);
    }

    [Fact]
    public void Resolve_ContentControlContentBinding_GivesAnInferredViewLocatorLink()
    {
        var host = Host(View("<ContentControl Content=\"{Binding Panel}\" />"));

        var edge = host.Resolve("src/App/Views/ShellView.axaml").Relationships.Single(e => e.FromId == "T:App.Views.PanelView");

        Assert.Equal("T:App.ViewModels.PanelViewModel", edge.ToId);
        Assert.Equal(RelationshipType.UsesViewModel, edge.Type);
        Assert.Equal(Confidence.Inferred, edge.Confidence);
        Assert.Contains("ViewLocator (registered in Application.DataTemplates at src/App/App.axaml:", edge.Reason, System.StringComparison.Ordinal);
        Assert.Contains("'ViewModel' is replaced by 'View'", edge.Reason, System.StringComparison.Ordinal);
        Assert.Contains("ContentControl Content binding at src/App/Views/ShellView.axaml:", edge.Reason, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_ItemsControlWithoutATemplate_LinksTheElementTypeThroughTheLocator()
    {
        var edges = Edges(Host(View("<ItemsControl ItemsSource=\"{Binding Items}\" />")));

        Assert.Contains("UsesViewModel:T:App.Views.ItemView->T:App.ViewModels.ItemViewModel [Inferred]", edges);
    }

    [Fact]
    public void Resolve_ItemsControlWithAnItemTemplate_GetsNoInferredLink()
    {
        var withAttribute = Edges(Host(View("<ItemsControl ItemsSource=\"{Binding Items}\" ItemTemplate=\"{StaticResource T}\" />")));
        var withProperty = Edges(Host(View("<ItemsControl ItemsSource=\"{Binding Items}\"><ItemsControl.ItemTemplate><DataTemplate /></ItemsControl.ItemTemplate></ItemsControl>")));

        Assert.DoesNotContain(withAttribute, e => e.Contains("ItemView", System.StringComparison.Ordinal));
        Assert.DoesNotContain(withProperty, e => e.Contains("ItemView", System.StringComparison.Ordinal));
    }

    [Fact]
    public void Resolve_ContentControlWithAContentTemplate_GetsNoInferredLink()
    {
        var edges = Edges(Host(View("<ContentControl Content=\"{Binding Panel}\" ContentTemplate=\"{StaticResource T}\" />")));

        Assert.DoesNotContain(edges, e => e.Contains("PanelView", System.StringComparison.Ordinal));
    }

    [Fact]
    public void Resolve_ViewModelWithoutAViewByTheConvention_GetsNoLink()
    {
        var edges = Edges(Host(View("<ContentControl Content=\"{Binding Orphan}\" />")));

        Assert.DoesNotContain(edges, e => e.Contains("OrphanViewModel", System.StringComparison.Ordinal));
    }

    [Fact]
    public void Resolve_NoRecognizedConvention_GivesNoInferredLinksAndNoError()
    {
        var host = XamlTestHost.Create(Source.Replace("Replace(\"ViewModel\", \"View\", System.StringComparison.Ordinal)", "ToString()", System.StringComparison.Ordinal),
            ("src/App/App.axaml", App), ("src/App/Views/ShellView.axaml", View("<ContentControl Content=\"{Binding Panel}\" />")));

        Assert.Null(host.Convention);
        Assert.DoesNotContain(Edges(host), e => e.Contains("Inferred", System.StringComparison.Ordinal));
    }

    [Fact]
    public void Resolve_MatchingNamesWithoutEvidence_GiveNoLink()
    {
        // LookalikeView and LookalikeViewModel exist, but nothing in the XAML connects them.
        var host = Host(View("<TextBlock />"), ("src/App/Views/LookalikeView.axaml", View(string.Empty, "App.Views.LookalikeView", string.Empty)));

        var all = Edges(host, "src/App/Views/ShellView.axaml").Concat(Edges(host, "src/App/Views/LookalikeView.axaml"));

        Assert.DoesNotContain(all, e => e.Contains("LookalikeViewModel", System.StringComparison.Ordinal));
    }

    [Fact]
    public void Resolve_MissingXClassClass_GivesAFindingAndNoCodeBehindLink()
    {
        var host = Host(View(string.Empty), ("src/App/Views/Ghost.axaml", View(string.Empty, "App.Views.Ghost", string.Empty)));

        var resolution = host.Resolve("src/App/Views/Ghost.axaml");

        Assert.DoesNotContain(resolution.Relationships, e => e.Type == RelationshipType.CodeBehind);
        Assert.Contains("x:Class 'App.Views.Ghost' is not a class of this solution (line 1).", resolution.File.Findings);
        Assert.Null(resolution.File.ClassId);
    }

    [Fact]
    public void Resolve_DuplicateNames_AreCopiedAsFindings()
    {
        var host = Host(View("<TextBlock x:Name=\"A\" /><TextBlock x:Name=\"A\" />"));

        Assert.Contains(host.Resolve("src/App/Views/ShellView.axaml").File.Findings, f => f.Contains("'A' is declared 2 times", System.StringComparison.Ordinal));
    }

    [Fact]
    public void Resolve_SameInputTwice_GivesTheSameEdgesInTheSameOrder()
    {
        var xml = View("<views:MenuView DataContext=\"{Binding Menu}\" /><ContentControl Content=\"{Binding Panel}\" />");

        Assert.Equal(Edges(Host(xml)), Edges(Host(xml)));
    }
}
