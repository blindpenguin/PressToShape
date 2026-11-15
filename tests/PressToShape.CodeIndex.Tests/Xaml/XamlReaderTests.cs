using System;
using System.IO;
using System.Linq;
using PressToShape.CodeIndex.Xaml;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public sealed class XamlReaderTests : IDisposable
{
    private const string Sample = """
        <UserControl xmlns="https://github.com/avaloniaui"
                     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                     xmlns:vm="using:App.ViewModels"
                     xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                     d:DesignWidth="300"
                     x:Class="App.Views.LayersView"
                     x:DataType="vm:LayersViewModel">
            <!-- a comment -->
            <UserControl.Resources>
                <Thickness x:Key="PanelPadding">8</Thickness>
            </UserControl.Resources>
            <DockPanel Margin="{StaticResource PanelPadding}">
                <Button x:Name="AddButton" Width="200" Command="{Binding AddLayerCommand}" Click="OnAddClick" />
                <Button Name="Second" Orientation="Horizontal" />
            </DockPanel>
        </UserControl>
        """;

    private readonly string _root = Path.Combine(Path.GetTempPath(), "ptsh-xaml-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static XamlDocument Read(string text, string path = "src/App/Views/LayersView.axaml")
    {
        var result = XamlReader.Read(path, "App", text);
        Assert.Null(result.Problem);
        return result.Document!;
    }

    [Fact]
    public void Read_Sample_BuildsHierarchyWithParentsAndPaths()
    {
        var document = Read(Sample);

        Assert.Equal("UserControl", document.RootType);
        Assert.Equal("App.Views.LayersView", document.XClass);
        Assert.Equal(
            ["/UserControl", "/UserControl/UserControl.Resources[1]", "/UserControl/UserControl.Resources[1]/Thickness[1]", "/UserControl/DockPanel[1]", "/UserControl/DockPanel[1]/Button[1]", "/UserControl/DockPanel[1]/Button[2]"],
            document.Nodes.Select(n => n.Path));
        Assert.Equal(Enumerable.Range(0, 6), document.Nodes.Select(n => n.Index));
        var second = document.Nodes[5];
        Assert.Equal("DockPanel", second.Parent!.TypeText);
        Assert.Null(document.Root.Parent);
        Assert.Equal(["Resources", "DockPanel"], document.Root.Children.Select(c => c.PropertyName ?? c.TypeText));
    }

    [Fact]
    public void Read_PropertyElement_HasOwnerAndProperty()
    {
        var resources = Read(Sample).Nodes[1];

        Assert.True(resources.IsPropertyElement);
        Assert.Equal("UserControl", resources.TypeText);
        Assert.Equal("Resources", resources.PropertyName);
        Assert.False(Read(Sample).Nodes[3].IsPropertyElement);
    }

    [Fact]
    public void Read_Names_AreTakenFromXNameAndName()
    {
        var document = Read(Sample);

        Assert.Equal("AddButton", document.Nodes[4].Name);
        Assert.Equal("Second", document.Nodes[5].Name);
        Assert.Null(document.Nodes[3].Name);
    }

    [Fact]
    public void Read_Lines_AreOneBased()
    {
        var document = Read(Sample);

        Assert.Equal(1, document.Root.Line);
        Assert.Equal(9, document.Nodes[1].Line);
        Assert.Equal(13, document.Nodes[4].Line);
        Assert.Equal(13, document.Nodes[4].Attribute("Command")!.Line);
    }

    [Fact]
    public void Read_Attributes_KeepMarkupMetadataAndHandlerCandidatesOnly()
    {
        var button = Read(Sample).Nodes[4];

        Assert.Equal(["x:Name", "Command", "Click"], button.Attributes.Select(a => a.Name));
        Assert.Equal(XamlAttributeRole.Metadata, button.Attribute("x:Name")!.Role);
        Assert.Equal(XamlAttributeRole.MarkupExtension, button.Attribute("Command")!.Role);
        Assert.Equal("Binding", button.Attribute("Command")!.Parsed!.Name);
        Assert.Equal(XamlAttributeRole.HandlerCandidate, button.Attribute("Click")!.Role);
        Assert.Null(button.Attribute("Width"));
    }

    [Fact]
    public void Read_RootLiterals_AreDroppedExceptMetadata()
    {
        var root = Read(Sample).Root;

        Assert.Equal(["x:Class", "x:DataType"], root.Attributes.Select(a => a.Name));
        Assert.Null(root.Attribute("d:DesignWidth"));
    }

    [Fact]
    public void Read_IdentifierLiteralWithoutPrefixIsACandidateAndNumbersAreNot()
    {
        var second = Read(Sample).Nodes[5];

        Assert.Equal(["Name", "Orientation"], second.Attributes.Select(a => a.Name));
        Assert.Equal(XamlAttributeRole.HandlerCandidate, second.Attribute("Orientation")!.Role);
    }

    [Fact]
    public void Read_AttachedPropertyAttributes_AreNotHandlerCandidates()
    {
        var document = Read("<Grid xmlns=\"https://github.com/avaloniaui\"><Button Grid.Row=\"Auto\" /></Grid>");

        Assert.Empty(document.Nodes[1].Attributes);
    }

    [Fact]
    public void Read_NamespaceDeclarations_AreOnTheNodeAndResolveThroughAncestors()
    {
        var document = Read("""
            <Window xmlns="https://github.com/avaloniaui" xmlns:a="using:One">
                <Grid xmlns:b="using:Two"><Button /></Grid>
            </Window>
            """);

        var button = document.Nodes[2];

        Assert.Equal("using:One", document.ResolvePrefix(button, "a"));
        Assert.Equal("using:Two", document.ResolvePrefix(button, "b"));
        Assert.Equal("https://github.com/avaloniaui", document.ResolvePrefix(button, string.Empty));
        Assert.Null(document.ResolvePrefix(document.Root, "b"));
        Assert.Null(document.ResolvePrefix(button, "zzz"));
        Assert.DoesNotContain(document.Root.Attributes, a => a.Name.StartsWith("xmlns", StringComparison.Ordinal));
    }

    [Fact]
    public void Read_PrefixedElements_KeepTheirPrefix()
    {
        var document = Read("<UserControl xmlns=\"https://github.com/avaloniaui\" xmlns:desktop=\"using:A\"><desktop:MenuBarView DataContext=\"{Binding MenuBar}\" /></UserControl>");

        var view = document.Nodes[1];

        Assert.Equal("desktop:MenuBarView", view.TypeText);
        Assert.Equal("/UserControl/desktop:MenuBarView[1]", view.Path);
        Assert.Equal("MenuBar", view.Attribute("DataContext")!.Parsed!.FirstPositionalText);
    }

    [Fact]
    public void Read_MarkupThatCannotBeParsed_IsKeptWithoutParsedValue()
    {
        var document = Read("<Grid xmlns=\"https://github.com/avaloniaui\"><Button Command=\"{Binding Foo\" /></Grid>");

        var attribute = document.Nodes[1].Attribute("Command")!;

        Assert.Equal(XamlAttributeRole.MarkupExtension, attribute.Role);
        Assert.Null(attribute.Parsed);
    }

    [Fact]
    public void Read_EscapedBraces_AreALiteralNotMarkup()
    {
        var document = Read("<Grid xmlns=\"https://github.com/avaloniaui\"><TextBlock Text=\"{}{0} items\" /></Grid>");

        Assert.Empty(document.Nodes[1].Attributes);
    }

    [Fact]
    public void Read_DuplicateNames_GiveAFinding()
    {
        var document = Read("<Grid xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"><Button x:Name=\"A\" /><Button x:Name=\"A\" /><Button x:Name=\"B\" /></Grid>");

        var finding = Assert.Single(document.Findings);

        Assert.Contains("'A' is declared 2 times", finding, StringComparison.Ordinal);
        Assert.Equal(["A", "A", "B"], document.Nodes.Where(n => n.Name is not null).Select(n => n.Name));
    }

    [Fact]
    public void Read_CommentsAndWhitespace_AreIgnored()
    {
        var document = Read("<!-- top --><Grid xmlns=\"https://github.com/avaloniaui\">\n  <!-- inner -->\n</Grid>");

        Assert.Single(document.Nodes);
        Assert.Empty(document.Findings);
    }

    [Fact]
    public void Read_ByteOrderMark_IsAccepted()
    {
        Assert.Equal("Grid", Read("﻿<Grid xmlns=\"https://github.com/avaloniaui\" />").RootType);
    }

    [Theory]
    [InlineData("<Grid>")]
    [InlineData("<Grid></Button>")]
    [InlineData("not xml at all")]
    [InlineData("")]
    [InlineData("<Grid a=\"1\" a=\"2\" />")]
    [InlineData("<a:Grid />")]
    public void Read_MalformedXml_GivesAProblemWithFileAndNoDocument(string text)
    {
        var result = XamlReader.Read("src/App/Views/Bad.axaml", "App", text);

        Assert.Null(result.Document);
        Assert.NotNull(result.Problem);
        Assert.False(result.Problem.IsWarning);
        Assert.Equal("App", result.Problem.Project);
        Assert.StartsWith("src/App/Views/Bad.axaml", result.Problem.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_MalformedXml_ReportsTheLine()
    {
        var result = XamlReader.Read("a.axaml", "App", "<Grid>\n  <Button>\n</Grid>");

        Assert.Contains("a.axaml:3", result.Problem!.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_DtdWithExternalEntity_IsRejectedAndNothingIsRead()
    {
        var marker = Path.Combine(_root, "secret.txt");
        Directory.CreateDirectory(_root);
        File.WriteAllText(marker, "SECRET");
        var xml = $"<!DOCTYPE x [<!ENTITY e SYSTEM \"{new Uri(marker).AbsoluteUri}\">]><Grid xmlns=\"https://github.com/avaloniaui\" Tag=\"&e;\" />";

        var result = XamlReader.Read("a.axaml", "App", xml);

        Assert.Null(result.Document);
        Assert.NotNull(result.Problem);
        Assert.DoesNotContain("SECRET", result.Problem.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_EntityExpansionBomb_IsRejected()
    {
        const string Bomb = "<!DOCTYPE lolz [<!ENTITY a \"aaaaaaaaaa\"><!ENTITY b \"&a;&a;&a;&a;&a;&a;&a;&a;&a;&a;\"><!ENTITY c \"&b;&b;&b;&b;&b;&b;&b;&b;&b;&b;\">]><Grid Tag=\"&c;\" />";

        var result = XamlReader.Read("a.axaml", "App", Bomb);

        Assert.NotNull(result.Problem);
    }

    [Fact]
    public void Read_DeepNesting_WorksUpToTheLimitAndFailsBeyond()
    {
        static string Nested(int levels) => string.Concat(Enumerable.Repeat("<A>", levels)) + string.Concat(Enumerable.Repeat("</A>", levels));

        Assert.Equal(100, Read(Nested(100)).Nodes.Count);
        var tooDeep = XamlReader.Read("a.axaml", "App", Nested(400));
        Assert.Null(tooDeep.Document);
        Assert.Contains("nested", tooDeep.Problem!.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_SameInput_GivesSameDocument()
    {
        var first = Read(Sample).Nodes.Select(n => (n.Path, n.Line, n.Name)).ToList();
        var second = Read(Sample).Nodes.Select(n => (n.Path, n.Line, n.Name)).ToList();

        Assert.Equal(first, second);
    }

    [Fact]
    public void InMemoryFileSource_List_ReturnsSortedPathsUnderTheProject()
    {
        var source = new InMemoryXamlFileSource().Add("src/B/z.axaml", "<A/>").Add("src/B/a.axaml", "<A/>").Add("src/C/c.axaml", "<A/>");

        Assert.Equal(["src/B/a.axaml", "src/B/z.axaml"], source.List("src/B"));
        Assert.Equal("<A/>", source.Read("src/C/c.axaml"));
    }

    [Fact]
    public void DiskFileSource_List_IsSortedRelativeAndSkipsBinAndObj()
    {
        Directory.CreateDirectory(Path.Combine(_root, "src", "App", "Views"));
        Directory.CreateDirectory(Path.Combine(_root, "src", "App", "obj", "Debug"));
        Directory.CreateDirectory(Path.Combine(_root, "src", "App", "bin"));
        Directory.CreateDirectory(Path.Combine(_root, "src", "Other"));
        foreach (var file in new[] { "src/App/Views/B.axaml", "src/App/A.axaml", "src/App/obj/Debug/Gen.axaml", "src/App/bin/X.axaml", "src/Other/O.axaml", "src/App/Views/readme.txt" })
        {
            File.WriteAllText(Path.Combine(_root, file.Replace('/', Path.DirectorySeparatorChar)), "<A/>");
        }

        var source = new DiskXamlFileSource(_root);

        Assert.Equal(["src/App/A.axaml", "src/App/Views/B.axaml"], source.List("src/App"));
        Assert.Equal("<A/>", source.Read("src/Other/O.axaml"));
        Assert.Empty(source.List("src/Missing"));
    }

    [Theory]
    [InlineData("../outside.axaml")]
    [InlineData("/etc/passwd")]
    [InlineData("a/../../b.axaml")]
    public void DiskFileSource_PathsOutsideTheRepo_AreRejected(string path)
    {
        var source = new DiskXamlFileSource(_root);

        Assert.Throws<ArgumentException>(() => source.Read(path));
        Assert.Throws<ArgumentException>(() => source.List(path));
    }
}
