using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Xaml;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class ResourceResolverTests
{
    private const string Header = "xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"";

    private static readonly Dictionary<string, string> Assemblies = new() { ["App"] = "src/App", ["Lib"] = "src/Lib" };

    private static XamlDocument Doc(string path, string xml, string project = "App")
    {
        var result = XamlReader.Read(path, project, xml);
        Assert.Null(result.Problem);
        return result.Document!;
    }

    private static (ResourceResolver Resolver, XamlWorld World) Create(params XamlDocument[] documents)
    {
        var world = new XamlWorld(documents, Assemblies);
        return (new ResourceResolver(world), world);
    }

    private static XamlNode Node(XamlDocument document, string typeText, int skip = 0) =>
        document.Nodes.Where(n => n.TypeText == typeText && !n.IsPropertyElement).ElementAt(skip);

    private static string[] Ids(ResourceLookup lookup) => lookup.Hits.Select(h => h.Id).ToArray();

    private static readonly string Application = $"""
        <Application {Header}>
            <Application.Resources>
                <ResourceDictionary>
                    <ResourceDictionary.MergedDictionaries>
                        <ResourceInclude Source="avares://App/Styles/Theme.axaml" />
                    </ResourceDictionary.MergedDictionaries>
                    <SolidColorBrush x:Key="AppBrush">Red</SolidColorBrush>
                </ResourceDictionary>
            </Application.Resources>
            <Application.Styles>
                <FluentTheme />
                <StyleInclude Source="avares://App/Styles/Controls.axaml" />
            </Application.Styles>
        </Application>
        """;

    private static readonly string Theme = $"""
        <ResourceDictionary {Header}>
            <ResourceDictionary.ThemeDictionaries>
                <ResourceDictionary x:Key="Dark"><SolidColorBrush x:Key="WindowBrush">Black</SolidColorBrush><x:Double x:Key="OnlyDark">1</x:Double></ResourceDictionary>
                <ResourceDictionary x:Key="Light"><SolidColorBrush x:Key="WindowBrush">White</SolidColorBrush></ResourceDictionary>
            </ResourceDictionary.ThemeDictionaries>
            <Thickness x:Key="PanelPadding">8</Thickness>
        </ResourceDictionary>
        """;

    private static readonly string Controls = """
        <Styles __HEADER__>
            <Styles.Resources>
                <ControlTheme x:Key="TabTheme" TargetType="TabItem"><Setter Property="Foreground" Value="{DynamicResource TextBrush}" /></ControlTheme>
                <SolidColorBrush x:Key="TextBrush">Gray</SolidColorBrush>
            </Styles.Resources>
        </Styles>
        """.Replace("__HEADER__", Header, System.StringComparison.Ordinal);

    [Fact]
    public void Resolve_ElementScope_FindsTheDefinitionOnTheElement()
    {
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><Button><Button.Resources><SolidColorBrush x:Key=\"Local\">Red</SolidColorBrush></Button.Resources><TextBlock Foreground=\"{{StaticResource Local}}\" /></Button></UserControl>");
        var (resolver, _) = Create(view);

        var lookup = resolver.Resolve(view, Node(view, "TextBlock"), "Local");

        Assert.True(lookup.IsFound);
        Assert.Equal(["R:src/App/V.axaml#/UserControl/Button[1]/Button.Resources[1]/Local"], Ids(lookup));
        Assert.Null(lookup.Reason);
    }

    [Fact]
    public void Resolve_AncestorScope_FindsTheNearestAncestorDefinition()
    {
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><Grid><Grid.Resources><x:Double x:Key=\"Size\">1</x:Double></Grid.Resources><StackPanel><Button /></StackPanel></Grid></UserControl>");
        var (resolver, _) = Create(view);

        var lookup = resolver.Resolve(view, Node(view, "Button"), "Size");

        Assert.Equal(["R:src/App/V.axaml#/UserControl/Grid[1]/Grid.Resources[1]/Size"], Ids(lookup));
    }

    [Fact]
    public void Resolve_ViewRootWithResourceDictionaryChild_IsFound()
    {
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><UserControl.Resources><ResourceDictionary><x:Double x:Key=\"Size\">1</x:Double></ResourceDictionary></UserControl.Resources><Button /></UserControl>");
        var (resolver, _) = Create(view);

        var lookup = resolver.Resolve(view, Node(view, "Button"), "Size");

        Assert.Equal(["R:src/App/V.axaml#/UserControl/UserControl.Resources[1]/ResourceDictionary[1]/Size"], Ids(lookup));
    }

    [Fact]
    public void Resolve_ApplicationResources_AreUsedWhenNoElementDefinesTheKey()
    {
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><Button /></UserControl>");
        var app = Doc("src/App/App.axaml", Application);
        var (resolver, _) = Create(view, app);

        var lookup = resolver.Resolve(view, Node(view, "Button"), "AppBrush");

        Assert.Equal(["R:src/App/App.axaml#/Application/Application.Resources[1]/ResourceDictionary[1]/AppBrush"], Ids(lookup));
    }

    [Fact]
    public void Resolve_MergedDictionary_IsFoundThroughTheInclude()
    {
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><Button /></UserControl>");
        var (resolver, _) = Create(view, Doc("src/App/App.axaml", Application), Doc("src/App/Styles/Theme.axaml", Theme));

        var lookup = resolver.Resolve(view, Node(view, "Button"), "PanelPadding");

        Assert.Equal(["R:src/App/Styles/Theme.axaml#/ResourceDictionary/PanelPadding"], Ids(lookup));
    }

    [Fact]
    public void Resolve_KeyInBothThemeVariants_GivesOneHitPerVariant()
    {
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><Button /></UserControl>");
        var (resolver, _) = Create(view, Doc("src/App/App.axaml", Application), Doc("src/App/Styles/Theme.axaml", Theme));

        var lookup = resolver.Resolve(view, Node(view, "Button"), "WindowBrush");

        Assert.Equal(
            ["R:src/App/Styles/Theme.axaml#/ResourceDictionary/WindowBrush@Dark", "R:src/App/Styles/Theme.axaml#/ResourceDictionary/WindowBrush@Light"],
            Ids(lookup).OrderBy(i => i, System.StringComparer.Ordinal));
        Assert.Equal(["Dark", "Light"], lookup.Hits.Select(h => h.Variant).OrderBy(v => v, System.StringComparer.Ordinal));
    }

    [Fact]
    public void Resolve_KeyInOneVariantOnly_GivesOneHit()
    {
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><Button /></UserControl>");
        var (resolver, _) = Create(view, Doc("src/App/App.axaml", Application), Doc("src/App/Styles/Theme.axaml", Theme));

        Assert.Equal(["R:src/App/Styles/Theme.axaml#/ResourceDictionary/OnlyDark@Dark"], Ids(resolver.Resolve(view, Node(view, "Button"), "OnlyDark")));
    }

    [Fact]
    public void Resolve_MergedDictionaries_LaterOnesWin()
    {
        var app = Doc("src/App/App.axaml", $"""
            <Application {Header}><Application.Resources><ResourceDictionary><ResourceDictionary.MergedDictionaries>
              <ResourceInclude Source="avares://App/A.axaml" /><ResourceInclude Source="avares://App/B.axaml" />
            </ResourceDictionary.MergedDictionaries></ResourceDictionary></Application.Resources></Application>
            """);
        var first = Doc("src/App/A.axaml", $"<ResourceDictionary {Header}><x:Double x:Key=\"K\">1</x:Double></ResourceDictionary>");
        var second = Doc("src/App/B.axaml", $"<ResourceDictionary {Header}><x:Double x:Key=\"K\">2</x:Double></ResourceDictionary>");
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><Button /></UserControl>");
        var (resolver, _) = Create(view, app, first, second);

        Assert.Equal(["R:src/App/B.axaml#/ResourceDictionary/K"], Ids(resolver.Resolve(view, Node(view, "Button"), "K")));
    }

    [Fact]
    public void Resolve_OwnEntryBeatsMergedEntryInTheSameDictionary()
    {
        var app = Doc("src/App/App.axaml", $"""
            <Application {Header}><Application.Resources><ResourceDictionary>
              <ResourceDictionary.MergedDictionaries><ResourceInclude Source="avares://App/A.axaml" /></ResourceDictionary.MergedDictionaries>
              <x:Double x:Key="K">9</x:Double>
            </ResourceDictionary></Application.Resources></Application>
            """);
        var merged = Doc("src/App/A.axaml", $"<ResourceDictionary {Header}><x:Double x:Key=\"K\">1</x:Double></ResourceDictionary>");
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><Button /></UserControl>");
        var (resolver, _) = Create(view, app, merged);

        Assert.Equal(["R:src/App/App.axaml#/Application/Application.Resources[1]/ResourceDictionary[1]/K"], Ids(resolver.Resolve(view, Node(view, "Button"), "K")));
    }

    [Fact]
    public void Resolve_StyleIncludeResources_AreSearchedAfterTheApplicationResources()
    {
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><Button /></UserControl>");
        var (resolver, _) = Create(view, Doc("src/App/App.axaml", Application), Doc("src/App/Styles/Controls.axaml", Controls));

        var lookup = resolver.Resolve(view, Node(view, "Button"), "TabTheme");

        Assert.Equal(["R:src/App/Styles/Controls.axaml#/Styles/Styles.Resources[1]/TabTheme"], Ids(lookup));
    }

    [Fact]
    public void Resolve_ReferenceInsideAStyleFile_FindsASiblingDefinition()
    {
        var controls = Doc("src/App/Styles/Controls.axaml", Controls);
        var (resolver, _) = Create(controls);

        var setter = Node(controls, "Setter");

        Assert.Equal(["R:src/App/Styles/Controls.axaml#/Styles/Styles.Resources[1]/TextBrush"], Ids(resolver.Resolve(controls, setter, "TextBrush")));
    }

    [Fact]
    public void Resolve_IncludeAcrossProjects_UsesTheAssemblyMap()
    {
        var app = Doc("src/App/App.axaml", $"<Application {Header}><Application.Resources><ResourceDictionary><ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"avares://Lib/Res/Colors.axaml\" /></ResourceDictionary.MergedDictionaries></ResourceDictionary></Application.Resources></Application>");
        var colors = Doc("src/Lib/Res/Colors.axaml", $"<ResourceDictionary {Header}><SolidColorBrush x:Key=\"LibBrush\">Red</SolidColorBrush></ResourceDictionary>", "Lib");
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><Button /></UserControl>");
        var (resolver, _) = Create(view, app, colors);

        Assert.Equal(["R:src/Lib/Res/Colors.axaml#/ResourceDictionary/LibBrush"], Ids(resolver.Resolve(view, Node(view, "Button"), "LibBrush")));
    }

    [Fact]
    public void ResolveInclude_RelativeAndAbsoluteInAssemblyPaths_AreResolved()
    {
        var view = Doc("src/App/Views/V.axaml", $"<UserControl {Header} />");
        var sibling = Doc("src/App/Views/Sibling.axaml", $"<ResourceDictionary {Header} />");
        var parent = Doc("src/App/Styles/S.axaml", $"<ResourceDictionary {Header} />");
        var (_, world) = Create(view, sibling, parent);

        Assert.Same(sibling, world.ResolveInclude(view, "Sibling.axaml"));
        Assert.Same(sibling, world.ResolveInclude(view, "./Sibling.axaml"));
        Assert.Same(parent, world.ResolveInclude(view, "../Styles/S.axaml"));
        Assert.Same(parent, world.ResolveInclude(view, "/Styles/S.axaml"));
        Assert.Same(parent, world.ResolveInclude(view, "avares://App/Styles/S.axaml"));
    }

    [Theory]
    [InlineData("avares://Unknown/x.axaml")]
    [InlineData("avares://App/Missing.axaml")]
    [InlineData("https://example.com/x.axaml")]
    [InlineData("../../../../../outside.axaml")]
    [InlineData("")]
    public void ResolveInclude_OutsideTheSolution_IsNull(string source)
    {
        var view = Doc("src/App/Views/V.axaml", $"<UserControl {Header} />");
        var (_, world) = Create(view);

        Assert.Null(world.ResolveInclude(view, source));
    }

    [Fact]
    public void Resolve_NearestWins_ElementBeforeApplication()
    {
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><UserControl.Resources><SolidColorBrush x:Key=\"AppBrush\">Blue</SolidColorBrush></UserControl.Resources><Button /></UserControl>");
        var (resolver, _) = Create(view, Doc("src/App/App.axaml", Application));

        Assert.Equal(["R:src/App/V.axaml#/UserControl/UserControl.Resources[1]/AppBrush"], Ids(resolver.Resolve(view, Node(view, "Button"), "AppBrush")));
    }

    [Fact]
    public void Resolve_UnknownKey_IsNotFoundWithTheThemeReason()
    {
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><Button /></UserControl>");
        var (resolver, _) = Create(view, Doc("src/App/App.axaml", Application));

        var lookup = resolver.Resolve(view, Node(view, "Button"), "SystemAccentColor");

        Assert.False(lookup.IsFound);
        Assert.Equal("not defined in this solution (may come from the Avalonia theme)", lookup.Reason);
    }

    [Fact]
    public void Resolve_NoApplicationDocument_StillSearchesTheViewScopes()
    {
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><UserControl.Resources><x:Double x:Key=\"K\">1</x:Double></UserControl.Resources><Button /></UserControl>");
        var (resolver, _) = Create(view);

        Assert.True(resolver.Resolve(view, Node(view, "Button"), "K").IsFound);
        Assert.False(resolver.Resolve(view, Node(view, "Button"), "Other").IsFound);
    }

    [Fact]
    public void Resolve_IncludeCycle_TerminatesWithoutAHit()
    {
        var a = Doc("src/App/A.axaml", $"<ResourceDictionary {Header}><ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"avares://App/B.axaml\" /></ResourceDictionary.MergedDictionaries></ResourceDictionary>");
        var b = Doc("src/App/B.axaml", $"<ResourceDictionary {Header}><ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"avares://App/A.axaml\" /></ResourceDictionary.MergedDictionaries></ResourceDictionary>");
        var app = Doc("src/App/App.axaml", $"<Application {Header}><Application.Resources><ResourceDictionary><ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"avares://App/A.axaml\" /></ResourceDictionary.MergedDictionaries></ResourceDictionary></Application.Resources></Application>");
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><Button /></UserControl>");
        var (resolver, _) = Create(view, app, a, b);

        Assert.False(resolver.Resolve(view, Node(view, "Button"), "Missing").IsFound);
    }

    [Fact]
    public void Resolve_IncludeOutsideTheSolution_IsIgnored()
    {
        var app = Doc("src/App/App.axaml", $"<Application {Header}><Application.Resources><ResourceDictionary><ResourceDictionary.MergedDictionaries><ResourceInclude Source=\"avares://Avalonia.Themes.Fluent/Accents/Base.xaml\" /></ResourceDictionary.MergedDictionaries><x:Double x:Key=\"K\">1</x:Double></ResourceDictionary></Application.Resources></Application>");
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><Button /></UserControl>");
        var (resolver, _) = Create(view, app);

        Assert.True(resolver.Resolve(view, Node(view, "Button"), "K").IsFound);
        Assert.False(resolver.Resolve(view, Node(view, "Button"), "FromFluent").IsFound);
    }

    [Fact]
    public void Definitions_ListsEveryEntryWithIdsAndVariants()
    {
        var theme = Doc("src/App/Styles/Theme.axaml", Theme);
        var view = Doc("src/App/V.axaml", $"<UserControl {Header}><UserControl.Resources><x:Double x:Key=\"Size\">1</x:Double></UserControl.Resources></UserControl>");

        var themeEntries = ResourceDictionaries.Definitions(theme);
        var viewEntries = ResourceDictionaries.Definitions(view);

        Assert.Equal(
            ["OnlyDark", "PanelPadding", "WindowBrush", "WindowBrush"],
            themeEntries.Select(e => e.Key).OrderBy(k => k, System.StringComparer.Ordinal));
        Assert.Contains(themeEntries, e => e.Id == "R:src/App/Styles/Theme.axaml#/ResourceDictionary/WindowBrush@Light" && e.Variant == "Light");
        Assert.Contains(themeEntries, e => e.Id == "R:src/App/Styles/Theme.axaml#/ResourceDictionary/PanelPadding" && e.Variant is null);
        Assert.Equal(["R:src/App/V.axaml#/UserControl/UserControl.Resources[1]/Size"], viewEntries.Select(e => e.Id));
    }

    [Fact]
    public void IncludesOf_ListsStyleAndResourceIncludesWithTargets()
    {
        var app = Doc("src/App/App.axaml", Application);
        var theme = Doc("src/App/Styles/Theme.axaml", Theme);
        var (_, world) = Create(app, theme);

        var includes = world.IncludesOf(app);

        Assert.Equal(["avares://App/Styles/Theme.axaml", "avares://App/Styles/Controls.axaml"], includes.Select(i => i.Source));
        Assert.Same(theme, includes[0].Target);
        Assert.Null(includes[1].Target);
    }

    [Fact]
    public void ApplicationFor_PrefersTheApplicationOfTheSameProject()
    {
        var appA = Doc("src/App/App.axaml", $"<Application {Header} />");
        var appB = Doc("src/Lib/App.axaml", $"<Application {Header} />", "Lib");
        var libView = Doc("src/Lib/V.axaml", $"<UserControl {Header} />", "Lib");
        var (_, world) = Create(appA, appB, libView);

        Assert.Same(appB, world.ApplicationFor(libView));
        Assert.Same(appA, world.ApplicationFor(appA));
    }
}
