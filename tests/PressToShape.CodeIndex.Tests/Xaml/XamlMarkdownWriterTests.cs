using System;
using System.Linq;
using System.Text;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Xaml.Model;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class XamlMarkdownWriterTests
{
    private const string ShellPage = "markdown/App/xaml/Views.ShellView.axaml.md";

    private static string Page(string path = ShellPage) => Encoding.UTF8.GetString(XamlSampleData.Index().Files[path]);

    [Fact]
    public void Write_OnePagePerXamlFileUnderTheXamlFolder()
    {
        var pages = XamlSampleData.Index().Files.Keys.Where(k => k.Contains("/xaml/", StringComparison.Ordinal)).Where(k => k.StartsWith("markdown/", StringComparison.Ordinal)).ToList();

        Assert.Equal(["markdown/App/xaml/App.axaml.md", ShellPage], pages);
    }

    [Fact]
    public void Write_Header_NamesFileRootAndClass()
    {
        var page = Page();

        Assert.StartsWith("# ShellView.axaml (XAML)", page, StringComparison.Ordinal);
        Assert.Contains("File: `src/App/Views/ShellView.axaml`. Root: `UserControl`.", page, StringComparison.Ordinal);
        Assert.Contains("Class: `App.Views.ShellView`.", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_ViewModelLinks_ShowEvidenceAndConfidence()
    {
        var page = Page();

        Assert.Contains("## View model links", page, StringComparison.Ordinal);
        Assert.Contains("`T:App.ViewModels.ShellViewModel` (verified): x:DataType (src/App/Views/ShellView.axaml:1)", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_InferredViewLink_IsMarkedInferredWithTheViewLocatorReason()
    {
        var data = XamlSampleData.Data();
        var inferred = new RelationshipRecord(
            RelationshipType.UsesViewModel, "T:App.Views.ShellView", "T:App.ViewModels.PanelViewModel", "src/App/Views/ShellView.axaml", 9, Confidence.Inferred,
            "ViewLocator (registered in Application.DataTemplates at src/App/App.axaml:2) picks views by name");
        var withLink = data with { Projects = data.Projects.Select(p => p.Name == "App" ? p with { Relationships = p.Relationships.Append(inferred).ToList() } : p).ToList() };
        var sink = new InMemoryFileSink();

        IndexGenerator.Write(withLink, sink);

        Assert.Contains("`T:App.ViewModels.PanelViewModel` (inferred): ViewLocator (registered in Application.DataTemplates", Encoding.UTF8.GetString(sink.Files[ShellPage]), StringComparison.Ordinal);
    }

    [Fact]
    public void Write_Bindings_ShowTargetsConvertersCommandsAndUnresolvedReasons()
    {
        var page = Page();

        Assert.Contains("- line 5: TitleText.Text = Binding `Title` -> `P:App.ViewModels.ShellViewModel.Title` (verified), converter StaticResource Gesture", page, StringComparison.Ordinal);
        Assert.Contains("Button.Command = Binding `SaveCommand` -> `P:App.ViewModels.ShellViewModel.SaveCommand` (verified), command", page, StringComparison.Ordinal);
        Assert.Contains("ContentControl.Content = Binding `Panel` -> `P:App.ViewModels.ShellViewModel.Panel` (verified)", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_UnresolvedBinding_SaysWhy()
    {
        var data = XamlSampleData.Data();
        var broken = data with
        {
            Projects = data.Projects.Select(p => p with
            {
                XamlFiles = p.Xaml.Select(f => f with
                {
                    Bindings = f.Bindings.Select(b => b.Path == "Title" ? b with { TargetId = null, Confidence = Confidence.Inferred, Reason = "no x:DataType" } : b).ToList(),
                }).ToList(),
            }).ToList(),
        };
        var sink = new InMemoryFileSink();

        IndexGenerator.Write(broken, sink);

        Assert.Contains("Binding `Title` -> unresolved: no x:DataType", Encoding.UTF8.GetString(sink.Files[ShellPage]), StringComparison.Ordinal);
    }

    [Fact]
    public void Write_ResourceReferences_ShowTargetsAndTheNotFoundReason()
    {
        var page = Page();

        Assert.Contains("StackPanel.Background = DynamicResource `WindowBrush` -> `R:src/App/App.axaml#/Application/Application.Resources[1]/ResourceDictionary[1]/WindowBrush@Dark`, `R:src/App/App.axaml#/Application/Application.Resources[1]/ResourceDictionary[1]/WindowBrush@Light` (verified)", page, StringComparison.Ordinal);
        Assert.Contains("Padding = StaticResource `PanelPadding` ->", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_Definitions_ListResourcesStylesTemplatesAndHandlers()
    {
        var page = Page();

        Assert.Contains("## Resources defined", page, StringComparison.Ordinal);
        Assert.Contains("`Gesture` (local:GestureConverter)", page, StringComparison.Ordinal);
        Assert.Contains("## Styles", page, StringComparison.Ordinal);
        Assert.Contains("`Button.icon`, sets Padding, uses PanelPadding", page, StringComparison.Ordinal);
        Assert.Contains("## Templates", page, StringComparison.Ordinal);
        Assert.Contains("DataTemplate `RowTemplate` for `vm:PanelViewModel` (`T:App.ViewModels.PanelViewModel`)", page, StringComparison.Ordinal);
        Assert.Contains("## Event handlers", page, StringComparison.Ordinal);
        Assert.Contains("Button.Click = `OnSaveClick` -> `M:App.Views.ShellView.OnSaveClick(System.Object,System.EventArgs)` (verified)", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_ThemeVariantResources_AreMarked()
    {
        var page = Page("markdown/App/xaml/App.axaml.md");

        Assert.Contains("`WindowBrush` (SolidColorBrush), theme Dark", page, StringComparison.Ordinal);
        Assert.Contains("`WindowBrush` (SolidColorBrush), theme Light", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_ReadmeLinksTheXamlPages()
    {
        var readme = Encoding.UTF8.GetString(XamlSampleData.Index().Files["markdown/README.md"]);

        Assert.Contains("- [ShellView.axaml](App/xaml/Views.ShellView.axaml.md) (XAML, 3 bindings)", readme, StringComparison.Ordinal);
        Assert.Contains("- [App.axaml](App/xaml/App.axaml.md) (XAML, 0 bindings)", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_Pages_AreDeterministicAndHaveNoAbsolutePathsOrCarriageReturns()
    {
        var first = XamlSampleData.Index();
        var second = XamlSampleData.Index(reverse: true);

        foreach (var (path, bytes) in first.Files.Where(f => f.Key.StartsWith("markdown/", StringComparison.Ordinal)))
        {
            Assert.Equal(bytes, second.Files[path]);
            var text = Encoding.UTF8.GetString(bytes);
            Assert.DoesNotContain(TestCompilation.RepoRoot, text, StringComparison.Ordinal);
            Assert.DoesNotContain("\r", text, StringComparison.Ordinal);
            Assert.DoesNotContain("kind", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Write_Findings_AreListedWhenPresent()
    {
        var data = XamlSampleData.Data();
        var withFinding = data with
        {
            Projects = data.Projects.Select(p => p with { XamlFiles = p.Xaml.Select(f => f with { Findings = ["a finding about line 4"] }).ToList() }).ToList(),
        };
        var sink = new InMemoryFileSink();

        IndexGenerator.Write(withFinding, sink);

        var page = Encoding.UTF8.GetString(sink.Files[ShellPage]);
        Assert.Contains("## Findings", page, StringComparison.Ordinal);
        Assert.Contains("- a finding about line 4", page, StringComparison.Ordinal);
        Assert.DoesNotContain("## Findings", Page(), StringComparison.Ordinal);
    }

    [Fact]
    public void XamlMarkdownPath_FollowsTheJsonName()
    {
        Assert.Equal("markdown/App/xaml/Views.ShellView.axaml.md", IndexLayout.XamlMarkdownPath("App", "src/App", "src/App/Views/ShellView.axaml"));
        Assert.Equal("markdown/My_App/xaml/A.B.axaml.md", IndexLayout.XamlMarkdownPath("My App", "src/My App", "src/My App/A/B.axaml"));
    }
}
