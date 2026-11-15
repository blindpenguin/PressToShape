using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using PressToShape.CodeIndex.Xaml;
using PressToShape.CodeIndex.Xaml.Model;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class XamlModelTests
{
    private const string Path = "src/App/Views/LayersView.axaml";

    private static XamlFileRecord Sample()
    {
        var elementId = XamlIds.Element(Path, "/UserControl/Button[1]");
        var bindingId = XamlIds.Binding(elementId, "Command");
        return new XamlFileRecord(
            XamlIds.File(Path), "App", Path, "UserControl", "App.Views.LayersView", "T:App.Views.LayersView", "vm:LayersViewModel", "T:App.LayersViewModel",
            new Dictionary<string, string> { ["vm"] = "using:App" },
            [new XamlElementRecord(elementId, XamlIds.Element(Path, "/UserControl"), "Button", null, "AddButton", 12, null)],
            [new XamlBindingRecord(bindingId, elementId, "Command", "Binding", "AddLayerCommand", null, null, null, "P:App.LayersViewModel.AddLayerCommand", "T:CommunityToolkit.IRelayCommand", true, Confidence.Verified, null, 12)],
            [new XamlResourceRecord(XamlIds.Resource(Path, "UserControl.Resources", "PanelPadding", "Dark"), "PanelPadding", "Thickness", null, "Dark", 5)],
            [new XamlResourceRefRecord(bindingId + "#ref", elementId, "Margin", "PanelPadding", "StaticResource", ["R:x#/PanelPadding@Light", "R:x#/PanelPadding@Dark"], Confidence.Verified, null, 20)],
            [new XamlStyleRecord(XamlIds.Style(Path, "ListBoxItem", 9), "ListBoxItem", null, ["MinHeight", "Padding"], ["PanelPadding"], 9)],
            [new XamlTemplateRecord("X:t", "DataTemplate", "vm:LayerItemViewModel", "T:App.LayerItemViewModel", null, 30)],
            [new XamlHandlerRecord("X:h", elementId, "Click", "OnClick", null, Confidence.Inferred, "no such method", 14)],
            ["a finding"]);
    }

    [Fact]
    public void XamlFileRecord_RoundTrip_KeepsEveryList()
    {
        var record = Sample();

        var json = JsonSerializer.Serialize(record, JsonIndexWriter.Options);
        var back = JsonSerializer.Deserialize<XamlFileRecord>(json, JsonIndexWriter.Options)!;

        Assert.Equal(json, JsonSerializer.Serialize(back, JsonIndexWriter.Options));
        Assert.Single(back.Bindings);
        Assert.True(back.Bindings[0].IsCommand);
        Assert.Equal(["R:x#/PanelPadding@Light", "R:x#/PanelPadding@Dark"], back.ResourceRefs[0].TargetIds);
        Assert.Equal(Confidence.Inferred, back.Handlers[0].Confidence);
        Assert.Equal("using:App", back.Namespaces["vm"]);
        Assert.Contains("\"Confidence\":\"Verified\"", JsonSerializer.Serialize(record.Bindings[0], JsonIndexWriter.Options).Replace(" ", string.Empty), System.StringComparison.Ordinal);
    }

    [Fact]
    public void RelationshipType_XamlValues_AreAppendedAfterTheEarlierOnes()
    {
        var names = System.Enum.GetNames<RelationshipType>();

        Assert.Equal(["Inherits", "Implements", "InjectedDependency", "References", "Calls", "Registers"], names.Take(6));
        Assert.Equal(["UsesViewModel", "CodeBehind", "BindsTo", "ReferencesResource", "UsesConverter", "HandledBy", "UsesType", "Includes", "ExecutesMethod"], names.Skip(6));
        var edge = new RelationshipRecord(RelationshipType.BindsTo, "X:a", "P:b", "a.axaml", 3, Confidence.Verified, "x:DataType");
        Assert.Contains("\"Type\":\"BindsTo\"", JsonSerializer.Serialize(edge, JsonIndexWriter.Options).Replace(" ", string.Empty), System.StringComparison.Ordinal);
    }

    [Fact]
    public void IndexSchema_Version_IsThree() => Assert.Equal(3, IndexSchema.Version);

    [Fact]
    public void ManifestProject_WithoutXamlCount_DefaultsToZero()
    {
        var back = JsonSerializer.Deserialize<ManifestProject>("{\"Name\":\"P\",\"Path\":\"p\",\"SymbolCount\":1,\"RelationshipCount\":2}", JsonIndexWriter.Options)!;

        Assert.Equal(0, back.XamlFileCount);
        Assert.Equal(3, new ManifestProject("P", "p", 1, 2, 3).XamlFileCount);
    }

    [Fact]
    public void ProjectIndex_WithoutXaml_HasAnEmptyList() =>
        Assert.Empty(new ProjectIndex("P", "p", [], []).Xaml);

    [Fact]
    public void Ids_AreBuiltFromRepoRelativePathsOnly()
    {
        var element = XamlIds.Element(Path, "/UserControl/Grid[2]/Button");

        Assert.Equal("X:src/App/Views/LayersView.axaml", XamlIds.File(Path));
        Assert.Equal("X:src/App/Views/LayersView.axaml#/UserControl/Grid[2]/Button", element);
        Assert.Equal(element + "@Command", XamlIds.Binding(element, "Command"));
        Assert.Equal("R:src/App/Styles/Theme.axaml#ResourceDictionary/PanelPadding@Light", XamlIds.Resource("src/App/Styles/Theme.axaml", "ResourceDictionary", "PanelPadding", "Light"));
        Assert.Equal("R:a.axaml#Resources/Key", XamlIds.Resource("a.axaml", "Resources", "Key", null));
        Assert.Equal("S:a.axaml#Button:pointerover /template/ Border@40", XamlIds.Style("a.axaml", "Button:pointerover /template/ Border", 40));
    }

    [Theory]
    [InlineData("T:App.Foo", false)]
    [InlineData("M:App.Foo.Bar(System.Int32)", false)]
    [InlineData("P:App.Foo.Bar", false)]
    [InlineData("N:App", false)]
    [InlineData("X:a.axaml", true)]
    [InlineData("X:a.axaml#/Root/B[1]@Command", true)]
    [InlineData("R:a.axaml#R/Key@Dark", true)]
    [InlineData("S:a.axaml#Button@3", true)]
    [InlineData("", false)]
    public void IsXaml_SeparatesXamlIdsFromRoslynIds(string id, bool expected) => Assert.Equal(expected, XamlIds.IsXaml(id));

    [Theory]
    [InlineData("X:src/App/Views/LayersView.axaml", "LayersView.axaml")]
    [InlineData("X:src/App/Views/LayersView.axaml#/UserControl/DockPanel/Button[1]", "LayersView.axaml Button[1]")]
    [InlineData("X:src/App/Views/LayersView.axaml#/UserControl/Button[1]@Command", "LayersView.axaml Button[1] @Command")]
    [InlineData("R:src/App/Styles/Theme.axaml#ResourceDictionary/PanelPadding@Dark", "Theme.axaml resource 'PanelPadding' (Dark)")]
    [InlineData("R:src/App/Styles/Theme.axaml#ResourceDictionary/PanelPadding", "Theme.axaml resource 'PanelPadding'")]
    [InlineData("S:src/App/Styles/Controls.axaml#Button.icon:pointerover /template/ Border@40", "Controls.axaml style 'Button.icon:pointerover /template/ Border' (line 40)")]
    [InlineData("T:App.Foo", "T:App.Foo")]
    public void Describe_GivesShortReadableText(string id, string expected) => Assert.Equal(expected, XamlIds.Describe(id));
}
