using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Xaml;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class XamlTypeResolverTests
{
    private const string FakeAvalonia = """
        [assembly: Avalonia.Metadata.XmlnsDefinition("https://github.com/avaloniaui", "Fake.Controls")]
        [assembly: Avalonia.Metadata.XmlnsDefinition("https://github.com/avaloniaui", "Fake.Primitives")]
        namespace Avalonia.Metadata
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace) : System.Attribute { }
        }
        namespace Fake.Controls { public class Button { } public class Grid { } public class UserControl { } }
        namespace Fake.Primitives { public class ToggleButton { } }
        """;

    private const string AppSource = """
        namespace App.Views { public class MenuBarView { } public class Generic<T> { } }
        namespace App.Views.Nested { public class Deep { } }
        namespace Other { public class Thing { } }
        """;

    private const string Xaml = """
        <UserControl xmlns="https://github.com/avaloniaui"
                     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                     xmlns:views="using:App.Views"
                     xmlns:nested="clr-namespace:App.Views.Nested;assembly=App"
                     xmlns:other="clr-namespace:Other"
                     xmlns:bad="using:Nope">
            <Button />
        </UserControl>
        """;

    private static (XamlTypeResolver Resolver, XamlDocument Document) Create()
    {
        var fake = TestCompilation.Project("Fake.Avalonia", FakeAvalonia);
        var app = TestCompilation.Project("App", AppSource);
        app = app with { Compilation = app.Compilation.AddReferences(fake.Compilation.ToMetadataReference()) };
        var known = new SymbolExtractor(TestCompilation.RepoRoot).Extract(app).Select(s => s.Id).ToHashSet();
        var document = XamlReader.Read("src/App/V.axaml", "App", Xaml).Document!;
        return (new XamlTypeResolver(app.Compilation, known), document);
    }

    private static ResolvedType Resolve(string typeText)
    {
        var (resolver, document) = Create();
        return resolver.Resolve(document, document.Nodes[1], typeText);
    }

    [Fact]
    public void Resolve_UsingNamespace_FindsTheSolutionType()
    {
        var type = Resolve("views:MenuBarView");

        Assert.True(type.IsResolved);
        Assert.Equal("App.Views.MenuBarView", type.Display);
        Assert.Equal("T:App.Views.MenuBarView", type.SolutionId);
    }

    [Fact]
    public void Resolve_ClrNamespaceWithAssembly_IgnoresTheAssemblyPart()
    {
        Assert.Equal("T:App.Views.Nested.Deep", Resolve("nested:Deep").SolutionId);
        Assert.Equal("T:Other.Thing", Resolve("other:Thing").SolutionId);
    }

    [Fact]
    public void Resolve_DefaultNamespace_UsesXmlnsDefinitionAttributesOfTheReferencedAssembly()
    {
        var button = Resolve("Button");
        var toggle = Resolve("ToggleButton");

        Assert.True(button.IsResolved);
        Assert.Equal("Fake.Controls.Button", button.Display);
        Assert.Equal("Fake.Primitives.ToggleButton", toggle.Display);
    }

    [Fact]
    public void Resolve_PackageType_HasNoSolutionIdAndKeepsItsName()
    {
        var button = Resolve("Button");

        Assert.NotNull(button.Symbol);
        Assert.Null(button.SolutionId);
    }

    [Fact]
    public void Resolve_UnknownPrefix_IsNotResolvedAndKeepsTheText()
    {
        var type = Resolve("nope:Thing");

        Assert.False(type.IsResolved);
        Assert.Equal("nope:Thing", type.Display);
        Assert.Null(type.SolutionId);
    }

    [Theory]
    [InlineData("views:DoesNotExist")]
    [InlineData("bad:Thing")]
    [InlineData("DoesNotExist")]
    [InlineData("views:")]
    [InlineData("")]
    public void Resolve_UnknownTypeOrNamespace_IsNotResolved(string text)
    {
        var type = Resolve(text);

        Assert.False(type.IsResolved);
        Assert.Equal(text, type.Display);
    }

    [Fact]
    public void Resolve_GenericTypeNameWithoutArity_IsNotResolved()
    {
        Assert.False(Resolve("views:Generic").IsResolved);
    }

    [Fact]
    public void Resolve_XamlLanguageNamespaceType_IsNotResolved()
    {
        Assert.False(Resolve("x:Static").IsResolved);
    }

    [Fact]
    public void Resolve_XmlnsDefinitionInTheSolutionItself_IsUsed()
    {
        var app = TestCompilation.Project("App", "[assembly: Avalonia.Metadata.XmlnsDefinition(\"urn:mine\", \"App.Views\")]\nnamespace Avalonia.Metadata { public sealed class XmlnsDefinitionAttribute(string a, string b) : System.Attribute { } }\nnamespace App.Views { public class MenuBarView { } }");
        var known = new SymbolExtractor(TestCompilation.RepoRoot).Extract(app).Select(s => s.Id).ToHashSet();
        var document = XamlReader.Read("src/App/V.axaml", "App", "<Root xmlns=\"urn:mine\"><MenuBarView /></Root>").Document!;

        var type = new XamlTypeResolver(app.Compilation, known).Resolve(document, document.Nodes[1], "MenuBarView");

        Assert.Equal("T:App.Views.MenuBarView", type.SolutionId);
    }

    [Fact]
    public void ResolveMember_AttachedPropertyAndStaticMember_SplitOwnerAndMember()
    {
        var (resolver, document) = Create();
        var node = document.Nodes[1];

        var attached = resolver.ResolveMember(document, node, "Grid.Row");
        var staticMember = resolver.ResolveMember(document, node, "views:MenuBarView.Instance");
        var noMember = resolver.ResolveMember(document, node, "views:MenuBarView");

        Assert.Equal("Fake.Controls.Grid", attached.Owner.Display);
        Assert.Equal("Row", attached.Member);
        Assert.Equal("T:App.Views.MenuBarView", staticMember.Owner.SolutionId);
        Assert.Equal("Instance", staticMember.Member);
        Assert.Null(noMember.Member);
        Assert.True(noMember.Owner.IsResolved);
    }

    [Fact]
    public void ResolveMember_UnknownOwner_IsNotResolvedButKeepsTheMember()
    {
        var (resolver, document) = Create();

        var result = resolver.ResolveMember(document, document.Nodes[1], "nope:Thing.Prop");

        Assert.False(result.Owner.IsResolved);
        Assert.Equal("Prop", result.Member);
    }

    [Fact]
    public void Resolve_NodeWithItsOwnNamespaceDeclaration_UsesIt()
    {
        var (resolver, _) = Create();
        var document = XamlReader.Read("a.axaml", "App", "<Root xmlns=\"https://github.com/avaloniaui\"><Grid xmlns:late=\"using:Other\"><late:Thing /></Grid></Root>").Document!;

        Assert.Equal("T:Other.Thing", resolver.Resolve(document, document.Nodes[2], "late:Thing").SolutionId);
        Assert.False(resolver.Resolve(document, document.Nodes[0], "late:Thing").IsResolved);
    }
}
