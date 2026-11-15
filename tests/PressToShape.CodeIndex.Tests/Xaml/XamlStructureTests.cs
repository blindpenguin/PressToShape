using System;
using System.Linq;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class XamlStructureTests
{
    private const string Source = """
        namespace App.Controls { public class Ruler : Avalonia.Controls.Control { public event System.EventHandler? Measured; } }
        namespace App { public class FlagConverter { } }
        namespace App.Views
        {
            public partial class MainView : Avalonia.Controls.UserControl
            {
                public void OnFillNoneClick(object? sender, System.EventArgs e) { }
                public void OnTwice(object? sender, System.EventArgs e) { }
                public void OnTwice(object? sender) { }
                public void OnLoaded(object? sender, System.EventArgs e) { }
                public void OnMeasured(object? sender, System.EventArgs e) { }
            }
            public partial class NoCodeBehindView { }
        }
        """;

    private const string Path = "src/App/Views/MainView.axaml";

    private static string View(string body, string extra = "") =>
        $"<UserControl {XamlTestHost.Header} xmlns:ctl=\"using:App.Controls\" xmlns:local=\"using:App\" x:Class=\"App.Views.MainView\" {extra}>\n{body}\n</UserControl>";

    private static XamlResolution Resolve(string body, params (string Path, string Xml)[] more) =>
        XamlTestHost.Create(Source, [(Path, View(body)), .. more]).Resolve(Path);

    private static RelationshipRecord[] Edges(XamlResolution resolution, RelationshipType type) => resolution.Relationships.Where(e => e.Type == type).ToArray();

    [Fact]
    public void Collect_ResourceDefinitions_HaveIdsKeysTypesAndLines()
    {
        var resolution = Resolve("<UserControl.Resources><Thickness x:Key=\"Pad\">8</Thickness><local:FlagConverter x:Key=\"Flag\" /><TextBlock /></UserControl.Resources>");

        var resources = resolution.File.Resources;

        Assert.Equal(["Flag", "Pad"], resources.Select(r => r.Key).OrderBy(k => k, StringComparer.Ordinal));
        var flag = resources.Single(r => r.Key == "Flag");
        Assert.Equal("R:src/App/Views/MainView.axaml#/UserControl/UserControl.Resources[1]/Flag", flag.Id);
        Assert.Equal("local:FlagConverter", flag.Type);
        Assert.Equal("T:App.FlagConverter", flag.TypeId);
        Assert.Null(flag.Variant);
        Assert.Equal(2, flag.Line);
        Assert.Null(resources.Single(r => r.Key == "Pad").TypeId);
    }

    [Fact]
    public void Collect_ThemeVariantResources_KeepTheirVariant()
    {
        var theme = $"<ResourceDictionary {XamlTestHost.Header}><ResourceDictionary.ThemeDictionaries><ResourceDictionary x:Key=\"Dark\"><SolidColorBrush x:Key=\"B\" /></ResourceDictionary><ResourceDictionary x:Key=\"Light\"><SolidColorBrush x:Key=\"B\" /></ResourceDictionary></ResourceDictionary.ThemeDictionaries><Thickness x:Key=\"P\" /></ResourceDictionary>";
        var host = XamlTestHost.Create(Source, ("src/App/Styles/Theme.axaml", theme));

        var resources = host.Resolve("src/App/Styles/Theme.axaml").File.Resources;

        Assert.Equal(["B@Dark", "B@Light", "P@"], resources.Select(r => $"{r.Key}@{r.Variant}").OrderBy(k => k, StringComparer.Ordinal));
    }

    [Fact]
    public void Collect_Style_RecordsSelectorKeySettersAndReferencedResourceKeys()
    {
        var resolution = Resolve("<UserControl.Styles><Style Selector=\"ListBoxItem\" x:Key=\"Item\"><Setter Property=\"MinHeight\" Value=\"30\" /><Setter Property=\"Foreground\" Value=\"{DynamicResource TextBrush}\" /><Setter Property=\"Padding\" Value=\"{StaticResource Pad}\" /></Style></UserControl.Styles>");

        var style = Assert.Single(resolution.File.Styles);

        Assert.Equal("ListBoxItem", style.Selector);
        Assert.Equal("Item", style.Key);
        Assert.Equal(["Foreground", "MinHeight", "Padding"], style.Setters);
        Assert.Equal(["Pad", "TextBrush"], style.ResourceKeys);
        Assert.Equal("S:src/App/Views/MainView.axaml#Item@2", style.Id);
        Assert.Equal(2, style.Line);
    }

    [Fact]
    public void Collect_NestedStyleAndStyleWithoutSelector_GetTheirOwnRecords()
    {
        var resolution = Resolve("<UserControl.Styles><Style Selector=\"Button\"><Setter Property=\"A\" Value=\"1\" /><Style Selector=\"^:pointerover\"><Setter Property=\"B\" Value=\"2\" /></Style></Style><Style><Setter Property=\"C\" Value=\"3\" /></Style></UserControl.Styles>");

        var styles = resolution.File.Styles;

        Assert.Equal(3, styles.Count);
        Assert.Equal(["A"], styles.Single(s => s.Selector == "Button").Setters);
        Assert.Equal(["B"], styles.Single(s => s.Selector == "^:pointerover").Setters);
        Assert.Equal(["C"], styles.Single(s => s.Selector == "(no selector)").Setters);
    }

    [Fact]
    public void Collect_Templates_RecordTypeTargetAndKey()
    {
        var resolution = Resolve("<UserControl.Resources><DataTemplate x:Key=\"Row\" DataType=\"local:FlagConverter\"><TextBlock /></DataTemplate><ControlTemplate TargetType=\"Button\"><Grid /></ControlTemplate><DataTemplate x:DataType=\"local:Missing\"><TextBlock /></DataTemplate></UserControl.Resources>");

        var templates = resolution.File.Templates.OrderBy(t => t.Id, StringComparer.Ordinal).ToList();

        Assert.Equal(3, templates.Count);
        var row = templates.Single(t => t.Key == "Row");
        Assert.Equal(("DataTemplate", "local:FlagConverter", "T:App.FlagConverter"), (row.TemplateType, row.Target, row.TargetId));
        var control = templates.Single(t => t.TemplateType == "ControlTemplate");
        Assert.Equal("Button", control.Target);
        Assert.Null(control.TargetId);
        Assert.Null(control.Key);
        Assert.Null(templates.Single(t => t.Target == "local:Missing").TargetId);
    }

    [Fact]
    public void Collect_HandlerWithOneMatchingMethod_IsVerified()
    {
        var resolution = Resolve("<Button Click=\"OnFillNoneClick\" />");

        var handler = Assert.Single(resolution.File.Handlers);
        var edge = Assert.Single(Edges(resolution, RelationshipType.HandledBy));

        Assert.Equal(("Click", "OnFillNoneClick"), (handler.Event, handler.Handler));
        Assert.Equal("M:App.Views.MainView.OnFillNoneClick(System.Object,System.EventArgs)", handler.TargetId);
        Assert.Equal(Confidence.Verified, handler.Confidence);
        Assert.Null(handler.Reason);
        Assert.Equal((handler.Id, handler.TargetId), (edge.FromId, edge.ToId));
        Assert.Equal("X:src/App/Views/MainView.axaml#/UserControl/Button[1]@Click", handler.Id);
    }

    [Fact]
    public void Collect_EventFromABaseClassAndACustomControl_IsRecognized()
    {
        var resolution = Resolve("<Button Loaded=\"OnLoaded\" /><ctl:Ruler Measured=\"OnMeasured\" />");

        Assert.Equal(["Loaded", "Measured"], resolution.File.Handlers.Select(h => h.Event).OrderBy(e => e, StringComparer.Ordinal));
        Assert.All(resolution.File.Handlers, h => Assert.Equal(Confidence.Verified, h.Confidence));
    }

    [Fact]
    public void Collect_HandlerWithoutAMethod_IsUnresolvedWithAReason()
    {
        var resolution = Resolve("<Button Click=\"OnGhost\" />");

        var handler = Assert.Single(resolution.File.Handlers);

        Assert.Null(handler.TargetId);
        Assert.Equal(Confidence.Inferred, handler.Confidence);
        Assert.Equal("no method 'OnGhost' in App.Views.MainView", handler.Reason);
        Assert.Empty(Edges(resolution, RelationshipType.HandledBy));
    }

    [Fact]
    public void Collect_HandlerWithTwoOverloads_IsAmbiguousAndUnresolved()
    {
        var handler = Assert.Single(Resolve("<Button Click=\"OnTwice\" />").File.Handlers);

        Assert.Null(handler.TargetId);
        Assert.Contains("2 methods named 'OnTwice'", handler.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Collect_HandlerWithoutACodeBehindClass_IsUnresolved()
    {
        var xml = $"<UserControl {XamlTestHost.Header}><Button Click=\"OnClick\" /></UserControl>";
        var host = XamlTestHost.Create(Source, ("src/App/Views/Loose.axaml", xml));

        var handler = Assert.Single(host.Resolve("src/App/Views/Loose.axaml").File.Handlers);

        Assert.Null(handler.TargetId);
        Assert.Contains("no x:Class code-behind class", handler.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void Collect_IdentifierAttributesThatAreNotEvents_AreNotHandlers()
    {
        var resolution = Resolve("<Button Orientation=\"Horizontal\" Name=\"Go\" Content=\"Hello\" />");

        Assert.Empty(resolution.File.Handlers);
    }

    [Fact]
    public void Collect_SolutionClassesAsElements_GetUsesTypeEdgesAndAvaloniaTypesDoNot()
    {
        var resolution = Resolve("<UserControl.Resources><local:FlagConverter x:Key=\"Flag\" /></UserControl.Resources><ctl:Ruler /><Button />");

        var edges = Edges(resolution, RelationshipType.UsesType);

        Assert.Equal(["T:App.Controls.Ruler", "T:App.FlagConverter"], edges.Select(e => e.ToId).OrderBy(i => i, StringComparer.Ordinal));
        Assert.All(edges, e => Assert.Equal(Confidence.Verified, e.Confidence));
        Assert.All(edges, e => Assert.StartsWith("X:src/App/Views/MainView.axaml#/UserControl/", e.FromId, StringComparison.Ordinal));
    }

    [Fact]
    public void Collect_Includes_LinkToFilesOfTheSolutionOnly()
    {
        var app = $"<Application {XamlTestHost.Header}><Application.Resources><ResourceDictionary><ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"avares://App/Styles/Theme.axaml\" /><ResourceInclude Source=\"avares://Fluent/Base.xaml\" /></ResourceDictionary.MergedDictionaries></ResourceDictionary></Application.Resources><Application.Styles><StyleInclude Source=\"avares://App/Styles/Controls.axaml\" /><StyleInclude Source=\"avares://App/Styles/Missing.axaml\" /></Application.Styles></Application>";
        var host = XamlTestHost.Create(Source,
            ("src/App/App.axaml", app),
            ("src/App/Styles/Theme.axaml", $"<ResourceDictionary {XamlTestHost.Header} />"),
            ("src/App/Styles/Controls.axaml", $"<Styles {XamlTestHost.Header} />"));

        var edges = Edges(host.Resolve("src/App/App.axaml"), RelationshipType.Includes);

        Assert.Equal(["X:src/App/Styles/Controls.axaml", "X:src/App/Styles/Theme.axaml"], edges.Select(e => e.ToId).OrderBy(i => i, StringComparer.Ordinal));
        Assert.All(edges, e => Assert.Equal("X:src/App/App.axaml", e.FromId));
        Assert.Contains(edges, e => e.Reason == "ResourceInclude avares://App/Styles/Theme.axaml");
    }

    [Fact]
    public void Collect_Output_IsDeterministicAndFindingsAreEmptyForCleanFiles()
    {
        const string Body = "<Button Click=\"OnFillNoneClick\" /><UserControl.Styles><Style Selector=\"Button\"><Setter Property=\"A\" Value=\"1\" /></Style></UserControl.Styles>";

        var first = Resolve(Body);
        var second = Resolve(Body);

        Assert.Equal(first.Relationships.Select(XamlTestHost.Edge), second.Relationships.Select(XamlTestHost.Edge));
        Assert.Equal(first.File.Styles.Select(s => s.Id), second.File.Styles.Select(s => s.Id));
        Assert.Empty(first.File.Findings);
    }
}
