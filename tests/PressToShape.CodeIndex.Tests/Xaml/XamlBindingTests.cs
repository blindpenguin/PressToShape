using System;
using System.Linq;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml;
using PressToShape.CodeIndex.Xaml.Model;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class XamlBindingTests
{
    private const string Source = """
        namespace App.ViewModels
        {
            public class Child { public string Name { get; } = ""; public int Value { get; } }
            public class Item { public string Label { get; } = ""; public System.Windows.Input.ICommand SelectCommand { get; } = null!; }
            public class ShellViewModel
            {
                public string Title { get; } = "";
                public bool IsOn { get; }
                public Child Stroke { get; } = new();
                public System.Windows.Input.ICommand SaveCommand { get; } = null!;
                public System.Collections.Generic.List<Item> Items { get; } = [];
                public string[] Names { get; } = [];
            }
        }
        namespace App
        {
            public class GestureTextConverter { }
            public class BrushConverter { public static readonly BrushConverter Instance = new(); }
            public class NotAConverterHere { }
        }
        """;

    private const string Path = "src/App/Views/ShellView.axaml";

    private static string View(string body, string dataType = "vm:ShellViewModel") =>
        $"<UserControl {XamlTestHost.Header} xmlns:vm=\"using:App.ViewModels\" xmlns:local=\"using:App\" x:Class=\"App.Views.ShellView\" {(dataType.Length > 0 ? $"x:DataType=\"{dataType}\"" : string.Empty)}>\n{body}\n</UserControl>";

    private static XamlResolution Resolve(string body, string dataType = "vm:ShellViewModel", params (string Path, string Xml)[] more) =>
        XamlTestHost.Create(Source + "\nnamespace App.Views { public partial class ShellView : Avalonia.Controls.UserControl { } }", [(Path, View(body, dataType)), .. more]).Resolve(Path);

    private static XamlBindingRecord BindingOn(XamlResolution resolution, string property) => Assert.Single(resolution.File.Bindings, b => b.Property == property);

    private static RelationshipRecord[] Edges(XamlResolution resolution, RelationshipType type) => resolution.Relationships.Where(e => e.Type == type).ToArray();

    [Fact]
    public void Collect_SimpleBinding_IsVerifiedWithAnEdgeToTheProperty()
    {
        var resolution = Resolve("<TextBlock Text=\"{Binding Title}\" />");

        var binding = BindingOn(resolution, "Text");
        var edge = Assert.Single(Edges(resolution, RelationshipType.BindsTo));

        Assert.Equal("P:App.ViewModels.ShellViewModel.Title", binding.TargetId);
        Assert.Equal("Binding", binding.BindingType);
        Assert.Equal("Title", binding.Path);
        Assert.Equal(Confidence.Verified, binding.Confidence);
        Assert.Equal("x:DataType vm:ShellViewModel", binding.Reason);
        Assert.False(binding.IsCommand);
        Assert.Equal((binding.Id, binding.TargetId), (edge.FromId, edge.ToId));
        Assert.Equal(Confidence.Verified, edge.Confidence);
        Assert.Equal(Path, edge.File);
        Assert.Equal(2, edge.Line);
        Assert.Equal("X:src/App/Views/ShellView.axaml#/UserControl/TextBlock[1]@Text", binding.Id);
    }

    [Fact]
    public void Collect_NestedPath_ResolvesAndReportsTheTypeId()
    {
        var binding = BindingOn(Resolve("<TextBlock Text=\"{Binding Stroke.Name}\" /><TextBlock Tag=\"{Binding Stroke}\" />"), "Text");

        Assert.Equal("P:App.ViewModels.Child.Name", binding.TargetId);
    }

    [Fact]
    public void Collect_PathToASolutionClass_RecordsTheTargetTypeId()
    {
        var binding = BindingOn(Resolve("<TextBlock Tag=\"{Binding Stroke}\" />"), "Tag");

        Assert.Equal("T:App.ViewModels.Child", binding.TargetTypeId);
    }

    [Fact]
    public void Collect_CommandBinding_IsMarkedAsCommandAndKeyBindingsCountToo()
    {
        var resolution = Resolve("<Button Command=\"{Binding SaveCommand}\" /><UserControl.KeyBindings><KeyBinding Gesture=\"Ctrl+S\" Command=\"{Binding SaveCommand}\" /></UserControl.KeyBindings>");

        var commands = resolution.File.Bindings.Where(b => b.Property == "Command").ToList();

        Assert.Equal(2, commands.Count);
        Assert.All(commands, c => Assert.True(c.IsCommand));
        Assert.All(commands, c => Assert.Equal("P:App.ViewModels.ShellViewModel.SaveCommand", c.TargetId));
        Assert.Equal(2, Edges(resolution, RelationshipType.BindsTo).Length);
    }

    [Fact]
    public void Collect_DataTemplateDataType_ScopesTheBindingsInside()
    {
        var resolution = Resolve("<ItemsControl><ItemsControl.ItemTemplate><DataTemplate DataType=\"vm:Item\"><Button Content=\"{Binding Label}\" Command=\"{Binding SelectCommand}\" /></DataTemplate></ItemsControl.ItemTemplate></ItemsControl>");

        Assert.Equal("P:App.ViewModels.Item.Label", BindingOn(resolution, "Content").TargetId);
        Assert.True(BindingOn(resolution, "Command").IsCommand);
        Assert.Equal("x:DataType vm:Item", BindingOn(resolution, "Content").Reason);
    }

    [Fact]
    public void Collect_XDataTypeOnATemplate_ScopesTheBindingsInside()
    {
        var resolution = Resolve("<DataTemplate x:DataType=\"vm:Item\"><TextBlock Text=\"{Binding Label}\" /></DataTemplate>");

        Assert.Equal("P:App.ViewModels.Item.Label", BindingOn(resolution, "Text").TargetId);
    }

    [Fact]
    public void Collect_NoDataType_IsUnresolvedWithTheReasonAndNoEdge()
    {
        var resolution = Resolve("<TextBlock Text=\"{Binding Title}\" />", dataType: string.Empty);

        var binding = BindingOn(resolution, "Text");

        Assert.Null(binding.TargetId);
        Assert.Equal(Confidence.Inferred, binding.Confidence);
        Assert.Equal("no x:DataType", binding.Reason);
        Assert.Empty(Edges(resolution, RelationshipType.BindsTo));
        Assert.DoesNotContain(resolution.File.Findings, f => f.StartsWith("Binding", StringComparison.Ordinal));
    }

    [Fact]
    public void Collect_UnresolvableDataType_SaysSo()
    {
        var binding = BindingOn(Resolve("<TextBlock Text=\"{Binding Title}\" />", dataType: "vm:Missing"), "Text");

        Assert.Contains("'vm:Missing' could not be resolved", binding.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Collect_MissingSegmentWithADataType_IsUnresolvedAndAFinding()
    {
        var resolution = Resolve("<TextBlock Text=\"{Binding Stroke.Nope}\" />");

        var binding = BindingOn(resolution, "Text");

        Assert.Null(binding.TargetId);
        Assert.Equal("path segment 'Nope' not found on 'App.ViewModels.Child'", binding.Reason);
        Assert.Contains(resolution.File.Findings, f => f.Contains("Binding 'Stroke.Nope' at line 2 does not resolve", StringComparison.Ordinal));
        Assert.Empty(Edges(resolution, RelationshipType.BindsTo));
    }

    [Fact]
    public void Collect_UnmodelledPathForm_IsUnresolvedButNotAFinding()
    {
        var resolution = Resolve("<TextBlock Text=\"{Binding Names[0]}\" />");

        var binding = BindingOn(resolution, "Text");

        Assert.Null(binding.TargetId);
        Assert.Contains("indexer", binding.Reason, StringComparison.Ordinal);
        Assert.Empty(resolution.File.Findings);
    }

    [Fact]
    public void Collect_ElementName_ResolvesAgainstTheNamedElementType()
    {
        var xaml = "<ToggleButton x:Name=\"toggle\" /><TextBlock Text=\"{Binding IsChecked, ElementName=toggle}\" /><TextBlock Tag=\"{Binding #toggle.IsChecked}\" />";
        var resolution = Resolve(xaml);

        Assert.Null(BindingOn(resolution, "Text").Source is null ? "no source" : null);
        Assert.Equal("ElementName=toggle", BindingOn(resolution, "Text").Source);
        Assert.Equal("ElementName=toggle", BindingOn(resolution, "Tag").Source);
        Assert.Equal("P:Avalonia.Controls.ToggleButton.IsChecked", BindingOn(resolution, "Text").TargetId);
        Assert.Equal("P:Avalonia.Controls.ToggleButton.IsChecked", BindingOn(resolution, "Tag").TargetId);
    }

    [Fact]
    public void Collect_ElementNameThatDoesNotExist_IsUnresolved()
    {
        var binding = BindingOn(Resolve("<TextBlock Text=\"{Binding Text, ElementName=ghost}\" />"), "Text");

        Assert.Null(binding.TargetId);
        Assert.Equal("no element named 'ghost' in this file", binding.Reason);
    }

    [Fact]
    public void Collect_RelativeSourceAncestorType_ResolvesAgainstThatType()
    {
        var resolution = Resolve("<StackPanel><TextBlock Text=\"{Binding Title, RelativeSource={RelativeSource AncestorType=vm:ShellViewModel}}\" /><TextBlock Tag=\"{Binding Width, RelativeSource={RelativeSource Self}}\" /></StackPanel>");

        Assert.Equal("P:App.ViewModels.ShellViewModel.Title", BindingOn(resolution, "Text").TargetId);
        Assert.StartsWith("RelativeSource", BindingOn(resolution, "Text").Source, StringComparison.Ordinal);
        Assert.Contains("not found on 'Avalonia.Controls.TextBlock'", BindingOn(resolution, "Tag").Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Collect_ParentSourceWithAType_ResolvesAndWithoutOneDoesNot()
    {
        var resolution = Resolve("<TextBlock Text=\"{Binding $parent[vm:ShellViewModel].Title}\" /><TextBlock Tag=\"{Binding $parent.Title}\" />");

        Assert.Equal("P:App.ViewModels.ShellViewModel.Title", BindingOn(resolution, "Text").TargetId);
        Assert.Null(BindingOn(resolution, "Tag").TargetId);
    }

    [Fact]
    public void Collect_ReflectionBinding_IsOnlyInferredEvenWhenThePathMatches()
    {
        var resolution = Resolve("<TextBlock Text=\"{ReflectionBinding Title}\" />");

        var binding = BindingOn(resolution, "Text");
        var edge = Assert.Single(Edges(resolution, RelationshipType.BindsTo));

        Assert.Equal("ReflectionBinding", binding.BindingType);
        Assert.Equal("P:App.ViewModels.ShellViewModel.Title", binding.TargetId);
        Assert.Equal(Confidence.Inferred, binding.Confidence);
        Assert.Equal(Confidence.Inferred, edge.Confidence);
        Assert.Contains("resolved at run time", binding.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Collect_TemplateBinding_IsUnresolvedWithAReason()
    {
        var resolution = Resolve("<Button Background=\"{TemplateBinding Background}\" />");

        var binding = BindingOn(resolution, "Background");

        Assert.Equal("TemplateBinding", binding.BindingType);
        Assert.Null(binding.TargetId);
        Assert.Contains("templated control", binding.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Collect_SourceBinding_IsUnresolved()
    {
        var binding = BindingOn(Resolve("<TextBlock Text=\"{Binding Title, Source={StaticResource Other}}\" />"), "Text");

        Assert.Null(binding.TargetId);
        Assert.Equal("Source", binding.Source);
    }

    [Fact]
    public void Collect_SetterValueBinding_IsAttributedToTheSetterProperty()
    {
        var resolution = Resolve("<UserControl.Styles><Style Selector=\"TextBlock\"><Setter Property=\"Text\" Value=\"{Binding Title}\" /></Style></UserControl.Styles>");

        Assert.Equal("P:App.ViewModels.ShellViewModel.Title", BindingOn(resolution, "Text").TargetId);
        Assert.DoesNotContain(resolution.File.Bindings, b => b.Property == "Value");
    }

    [Fact]
    public void Collect_DataContextBinding_IsRecordedLikeAnyBinding()
    {
        var resolution = Resolve("<TextBlock DataContext=\"{Binding Stroke}\" />");

        Assert.Equal("P:App.ViewModels.ShellViewModel.Stroke", BindingOn(resolution, "DataContext").TargetId);
    }

    [Fact]
    public void Collect_ModeAndPathNamedArguments_AreRecorded()
    {
        var binding = BindingOn(Resolve("<TextBlock Text=\"{Binding Path=Title, Mode=OneWay}\" />"), "Text");

        Assert.Equal("Title", binding.Path);
        Assert.Equal("OneWay", binding.Mode);
    }

    [Fact]
    public void Collect_DotPath_ResolvesToTheDataContextWithoutAProperty()
    {
        var binding = BindingOn(Resolve("<TextBlock Text=\"{Binding .}\" />"), "Text");

        Assert.Null(binding.TargetId);
        Assert.Equal("T:App.ViewModels.ShellViewModel", binding.TargetTypeId);
        Assert.Equal(Confidence.Verified, binding.Confidence);
    }

    [Fact]
    public void Collect_LocalizedTextMarkup_IsNotABinding()
    {
        var resolution = Resolve("<TextBlock Text=\"{loc:T 'Layers'}\" />");

        Assert.Empty(resolution.File.Bindings);
        Assert.Empty(resolution.File.ResourceRefs);
        Assert.Empty(resolution.File.Findings);
    }

    [Fact]
    public void Collect_MarkupThatCannotBeRead_IsAFinding()
    {
        var resolution = Resolve("<TextBlock Text=\"{Binding Title\" />");

        Assert.Contains(resolution.File.Findings, f => f.Contains("could not be read", StringComparison.Ordinal));
        Assert.Empty(resolution.File.Bindings);
    }

    [Fact]
    public void Collect_ConverterByStaticResource_LinksTheConverterClass()
    {
        var resolution = Resolve("<UserControl.Resources><local:GestureTextConverter x:Key=\"GestureText\" /></UserControl.Resources><TextBlock Text=\"{Binding Title, Converter={StaticResource GestureText}}\" />");

        var edge = Assert.Single(Edges(resolution, RelationshipType.UsesConverter));

        Assert.Equal("T:App.GestureTextConverter", edge.ToId);
        Assert.Equal(BindingOn(resolution, "Text").Id, edge.FromId);
        Assert.Equal(Confidence.Verified, edge.Confidence);
        Assert.Contains("defined at src/App/Views/ShellView.axaml:", edge.Reason, StringComparison.Ordinal);
        Assert.Equal("StaticResource GestureText", BindingOn(resolution, "Text").Converter);
    }

    [Fact]
    public void Collect_ConverterByXStatic_LinksTheClassWhenTheMemberExists()
    {
        var resolution = Resolve("<TextBlock Text=\"{Binding Title, Converter={x:Static local:BrushConverter.Instance}}\" /><TextBlock Tag=\"{Binding Title, Converter={x:Static local:BrushConverter.Missing}}\" />");

        var edge = Assert.Single(Edges(resolution, RelationshipType.UsesConverter));

        Assert.Equal("T:App.BrushConverter", edge.ToId);
        Assert.Contains(resolution.File.Findings, f => f.Contains("x:Static local:BrushConverter.Missing", StringComparison.Ordinal));
    }

    [Fact]
    public void Collect_ConverterResourceThatIsNotASolutionClass_IsAFinding()
    {
        var resolution = Resolve("<UserControl.Resources><SolidColorBrush x:Key=\"B\" /></UserControl.Resources><TextBlock Text=\"{Binding Title, Converter={StaticResource B}}\" /><TextBlock Tag=\"{Binding Title, Converter={StaticResource Nowhere}}\" />");

        Assert.Empty(Edges(resolution, RelationshipType.UsesConverter));
        Assert.Contains(resolution.File.Findings, f => f.Contains("StaticResource B", StringComparison.Ordinal) && f.Contains("not a class of this solution", StringComparison.Ordinal));
        Assert.Contains(resolution.File.Findings, f => f.Contains("StaticResource Nowhere", StringComparison.Ordinal) && f.Contains("not defined in this solution", StringComparison.Ordinal));
    }

    [Fact]
    public void Collect_ResourceReferenceFound_IsVerifiedWithAnEdge()
    {
        var resolution = Resolve("<UserControl.Resources><Thickness x:Key=\"Pad\">8</Thickness></UserControl.Resources><DockPanel Margin=\"{StaticResource Pad}\" />");

        var reference = Assert.Single(resolution.File.ResourceRefs);
        var edge = Assert.Single(Edges(resolution, RelationshipType.ReferencesResource));

        Assert.Equal(("Pad", "StaticResource", "Margin"), (reference.Key, reference.RefType, reference.Property));
        Assert.Equal(["R:src/App/Views/ShellView.axaml#/UserControl/UserControl.Resources[1]/Pad"], reference.TargetIds);
        Assert.Equal(Confidence.Verified, reference.Confidence);
        Assert.Null(reference.Reason);
        Assert.Equal((reference.Id, reference.TargetIds[0]), (edge.FromId, edge.ToId));
        Assert.Equal("StaticResource Pad", edge.Reason);
    }

    [Fact]
    public void Collect_ResourceReferenceNotFound_IsRecordedWithTheThemeReasonAndNoEdge()
    {
        var resolution = Resolve("<TextBlock Foreground=\"{DynamicResource SystemAccentColor}\" />");

        var reference = Assert.Single(resolution.File.ResourceRefs);

        Assert.Equal("DynamicResource", reference.RefType);
        Assert.Empty(reference.TargetIds);
        Assert.Equal(Confidence.Inferred, reference.Confidence);
        Assert.Equal("not defined in this solution (may come from the Avalonia theme)", reference.Reason);
        Assert.Empty(Edges(resolution, RelationshipType.ReferencesResource));
    }

    [Fact]
    public void Collect_ResourceReferenceKeyedByAType_IsRecordedWithTheTypeKeyAndNoFinding()
    {
        var resolution = Resolve("<UserControl.Styles><ControlTheme x:Key=\"T\" TargetType=\"MenuItem\" BasedOn=\"{StaticResource {x:Type MenuItem}}\" /></UserControl.Styles>");

        var reference = Assert.Single(resolution.File.ResourceRefs);

        Assert.Equal("{x:Type MenuItem}", reference.Key);
        Assert.Equal("BasedOn", reference.Property);
        Assert.Empty(reference.TargetIds);
        Assert.Equal("not defined in this solution (may come from the Avalonia theme)", reference.Reason);
        Assert.Empty(resolution.File.Findings);
    }

    [Fact]
    public void Collect_ResourceReferenceKeyedByATypeThatIsDefinedLocally_IsFound()
    {
        var resolution = Resolve("<UserControl.Resources><ControlTheme x:Key=\"{x:Type Button}\" TargetType=\"Button\" /></UserControl.Resources><Button Theme=\"{StaticResource {x:Type Button}}\" />");

        var reference = Assert.Single(resolution.File.ResourceRefs);

        Assert.Single(reference.TargetIds);
        Assert.Equal(Confidence.Verified, reference.Confidence);
    }

    [Fact]
    public void Collect_ResourceReferenceInASetter_IsAttributedToTheSetterProperty()
    {
        var resolution = Resolve("<UserControl.Styles><Style Selector=\"Button\"><Setter Property=\"Foreground\" Value=\"{DynamicResource TextBrush}\" /></Style></UserControl.Styles>");

        Assert.Equal("Foreground", Assert.Single(resolution.File.ResourceRefs).Property);
    }

    [Fact]
    public void Collect_ResourceInBothThemeVariants_GivesOneEdgePerVariant()
    {
        var theme = $"<ResourceDictionary {XamlTestHost.Header}><ResourceDictionary.ThemeDictionaries><ResourceDictionary x:Key=\"Dark\"><SolidColorBrush x:Key=\"Back\" /></ResourceDictionary><ResourceDictionary x:Key=\"Light\"><SolidColorBrush x:Key=\"Back\" /></ResourceDictionary></ResourceDictionary.ThemeDictionaries></ResourceDictionary>";
        var app = $"<Application {XamlTestHost.Header}><Application.Resources><ResourceDictionary><ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"avares://App/Styles/Theme.axaml\" /></ResourceDictionary.MergedDictionaries></ResourceDictionary></Application.Resources></Application>";
        var resolution = Resolve("<TextBlock Background=\"{DynamicResource Back}\" />", "vm:ShellViewModel", ("src/App/App.axaml", app), ("src/App/Styles/Theme.axaml", theme));

        var edges = Edges(resolution, RelationshipType.ReferencesResource);

        Assert.Equal(2, edges.Length);
        Assert.Contains(edges, e => e.Reason == "DynamicResource Back (Dark)");
        Assert.Contains(edges, e => e.Reason == "DynamicResource Back (Light)");
        Assert.Equal(2, Assert.Single(resolution.File.ResourceRefs).TargetIds.Count);
    }

    [Fact]
    public void Collect_Output_IsDeterministic()
    {
        const string Body = "<TextBlock Text=\"{Binding Title}\" Foreground=\"{DynamicResource A}\" /><Button Command=\"{Binding SaveCommand}\" />";

        Assert.Equal(
            Resolve(Body).Relationships.Select(XamlTestHost.Edge),
            Resolve(Body).Relationships.Select(XamlTestHost.Edge));
    }
}
