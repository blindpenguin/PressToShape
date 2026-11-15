using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Xaml;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class BindingResolverTests
{
    private const string Source = """
        namespace App;
        public interface IHasTitle { string Title { get; } }
        public class Base { public string BaseProp { get; set; } = ""; }
        public class Child { public int Value { get; set; } public Child? Next { get; set; } }
        public class Vm : Base, IHasTitle
        {
            public string Name { get; set; } = "";
            public Child Stroke { get; set; } = new();
            public Child? Optional { get; set; }
            public int? MaybeNumber { get; set; }
            public bool IsEnabled { get; set; }
            public string Title => "";
            public int[] Items { get; } = [];
            public System.Collections.Generic.List<Child> Children { get; } = [];
            public System.Threading.Tasks.Task<string> Pending { get; } = null!;
            public System.Windows.Input.ICommand SaveCommand { get; } = null!;
            public int Field;
            private string Secret { get; set; } = "";
            public void DoIt() { }
            public string this[int index] => "";
        }
        public interface IShell : IHasTitle { Vm Current { get; } }
        public partial class Generated { }
        """;

    private static (BindingResolver Resolver, INamedTypeSymbol Vm, Compilation Compilation) Create()
    {
        var project = TestCompilation.Project("App", Source);
        project = TestCompilation.WithGenerator(project, new FixedSourceGenerator("Gen.g.cs",
            "namespace App { partial class Generated { public System.Windows.Input.ICommand SaveCommand { get; } = null!; public bool Flag { get; } } }"));
        var known = new SymbolExtractor(TestCompilation.RepoRoot).Extract(project).Select(s => s.Id).ToHashSet();
        return (new BindingResolver(known), project.Compilation.GetTypeByMetadataName("App.Vm")!, project.Compilation);
    }

    private static BindingResolution Resolve(string? path)
    {
        var (resolver, vm, _) = Create();
        return resolver.Resolve(vm, path);
    }

    [Fact]
    public void Resolve_SimpleProperty_IsVerifiedWithThePropertyId()
    {
        var result = Resolve("Name");

        Assert.Equal(BindingStatus.Verified, result.Status);
        Assert.Equal("P:App.Vm.Name", result.PropertyId);
        Assert.Null(result.TargetTypeId);
        Assert.False(result.IsCommand);
        Assert.Null(result.Reason);
    }

    [Fact]
    public void Resolve_NestedPath_FollowsMemberTypesAndReportsTheSolutionTypeId()
    {
        var result = Resolve("Stroke.Value");
        var type = Resolve("Stroke");

        Assert.Equal("P:App.Child.Value", result.PropertyId);
        Assert.Equal("T:App.Child", type.TargetTypeId);
        Assert.Equal("Child", type.TargetType!.Name);
    }

    [Fact]
    public void Resolve_NullableReferenceAndNullableValueTypes_ContinueWithTheUnderlyingType()
    {
        Assert.Equal("P:App.Child.Value", Resolve("Optional.Value").PropertyId);
        Assert.Equal("P:App.Child.Value", Resolve("Optional.Next.Value").PropertyId);
        Assert.Equal(BindingStatus.Verified, Resolve("MaybeNumber").Status);
    }

    [Fact]
    public void Resolve_InheritedMember_IsFoundOnTheBaseType()
    {
        Assert.Equal("P:App.Base.BaseProp", Resolve("BaseProp").PropertyId);
    }

    [Fact]
    public void Resolve_InterfaceMember_IsFoundThroughBaseInterfaces()
    {
        var (resolver, _, compilation) = Create();
        var shell = compilation.GetTypeByMetadataName("App.IShell")!;

        Assert.Equal("P:App.IHasTitle.Title", resolver.Resolve(shell, "Title").PropertyId);
        Assert.Equal("P:App.Vm.Title", resolver.Resolve(shell, "Current.Title").PropertyId);
    }

    [Fact]
    public void Resolve_Field_IsAllowed()
    {
        Assert.Equal("F:App.Vm.Field", Resolve("Field").PropertyId);
    }

    [Fact]
    public void Resolve_CollectionProperty_ContinuesWithTheCollectionMembers()
    {
        Assert.Equal(BindingStatus.Verified, Resolve("Children.Count").Status);
        Assert.Equal(BindingStatus.Verified, Resolve("Items.Length").Status);
    }

    [Fact]
    public void Resolve_CommandProperty_IsMarkedAsCommand()
    {
        var result = Resolve("SaveCommand");

        Assert.True(result.IsCommand);
        Assert.Equal("P:App.Vm.SaveCommand", result.PropertyId);
        Assert.False(Resolve("Name").IsCommand);
    }

    [Fact]
    public void Resolve_GeneratedCommandProperty_IsFound()
    {
        var (resolver, _, compilation) = Create();
        var generated = compilation.GetTypeByMetadataName("App.Generated")!;

        var result = resolver.Resolve(generated, "SaveCommand");

        Assert.Equal("P:App.Generated.SaveCommand", result.PropertyId);
        Assert.True(result.IsCommand);
        Assert.Equal(BindingStatus.Verified, resolver.Resolve(generated, "Flag").Status);
    }

    [Theory]
    [InlineData("!IsEnabled")]
    [InlineData("!!IsEnabled")]
    [InlineData("! IsEnabled")]
    public void Resolve_Negation_IsIgnoredForTheTarget(string path)
    {
        var result = Resolve(path);

        Assert.Equal("P:App.Vm.IsEnabled", result.PropertyId);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Resolve_DataContextItself_IsVerifiedWithoutAProperty(string? path)
    {
        var result = Resolve(path);

        Assert.Equal(BindingStatus.Verified, result.Status);
        Assert.Null(result.PropertyId);
        Assert.Equal("T:App.Vm", result.TargetTypeId);
    }

    [Theory]
    [InlineData("Items[0]", "an indexer [0]")]
    [InlineData("Items[0].Length", "an indexer [0].Length")]
    [InlineData("Pending^", "the stream operator ^")]
    [InlineData("(local:Attached.Prop)", "an attached property or cast (local:Attached.Prop)")]
    [InlineData("(vm:Child)Stroke", "an attached property or cast (vm:Child)Stroke")]
    [InlineData("Name.(Grid.Row)", "an attached property or cast (Grid.Row)")]
    [InlineData("Name Foo", "the unexpected text ' Foo'")]
    public void Resolve_UnmodelledForms_AreUnresolvedWithAReasonAndNeverVerified(string path, string expectedPart)
    {
        var result = Resolve(path);

        Assert.Equal(BindingStatus.Unresolved, result.Status);
        Assert.Null(result.PropertyId);
        Assert.Contains(expectedPart, result.Reason, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_UnmodelledFormAfterResolvedSegments_SaysHowFarItGot()
    {
        var result = Resolve("Stroke.Next.Value[2]");

        Assert.Contains("after 'Stroke.Next.Value'", result.Reason, System.StringComparison.Ordinal);
        Assert.Equal(BindingStatus.Unresolved, Resolve("Name[0]").Status);
    }

    [Fact]
    public void Resolve_MissingSegment_NamesTheSegmentAndTheType()
    {
        var result = Resolve("Stroke.Nope");

        Assert.Equal(BindingStatus.Unresolved, result.Status);
        Assert.Equal("path segment 'Nope' not found on 'App.Child'", result.Reason);
    }

    [Fact]
    public void Resolve_PrivateMemberAndIndexerAndMethod_AreNotBindable()
    {
        Assert.Contains("not found", Resolve("Secret").Reason, System.StringComparison.Ordinal);
        Assert.Contains("not found", Resolve("Item").Reason, System.StringComparison.Ordinal);
        Assert.Contains("is a method", Resolve("DoIt").Reason, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_NoDataType_IsUnresolvedWithTheReason()
    {
        var (resolver, _, _) = Create();

        var result = resolver.Resolve(null, "Name");

        Assert.Equal("no x:DataType", result.Reason);
        Assert.Equal(BindingStatus.Unresolved, result.Status);
    }

    [Theory]
    [InlineData("$parent[ItemsControl].Foo")]
    [InlineData("$parent.Foo")]
    [InlineData("$self.Width")]
    [InlineData("#box.Text")]
    public void Resolve_OtherSources_AreUnresolvedWithoutAKnownType(string path)
    {
        var result = Resolve(path);

        Assert.Equal(BindingStatus.Unresolved, result.Status);
        Assert.Contains("has no known type here", result.Reason, System.StringComparison.Ordinal);
    }

    [Fact]
    public void ResolveSegments_FromAKnownStartType_IsVerified()
    {
        var (resolver, _, compilation) = Create();
        var child = compilation.GetTypeByMetadataName("App.Child")!;

        var result = resolver.ResolveSegments(child, BindingPathParser.Parse("#box.Next.Value").Segments);

        Assert.Equal("P:App.Child.Value", result.PropertyId);
    }

    [Fact]
    public void ResolveSegments_NoSegmentsAndNoUnsupported_IsTheStartTypeItself()
    {
        var (resolver, _, compilation) = Create();
        var child = compilation.GetTypeByMetadataName("App.Child")!;

        var result = resolver.ResolveSegments(child, []);

        Assert.Null(result.PropertyId);
        Assert.Equal("T:App.Child", result.TargetTypeId);
    }

    [Theory]
    [InlineData("$self.Width", "Self", null, "Width")]
    [InlineData("$parent[ItemsControl,2].DataContext.Foo", "Parent", "ItemsControl,2", "DataContext.Foo")]
    [InlineData("$parent.Foo", "Parent", null, "Foo")]
    [InlineData("#box.Text", "ElementName", "box", "Text")]
    [InlineData("A.B", "DataContext", null, "A.B")]
    public void Parse_Sources_AreRecognized(string path, string source, string? sourceText, string segments)
    {
        var parsed = BindingPathParser.Parse(path);

        Assert.Equal(source, parsed.Source.ToString());
        Assert.Equal(sourceText, parsed.SourceText);
        Assert.Equal(segments, string.Join('.', parsed.Segments));
        Assert.Null(parsed.Unsupported);
    }

    [Fact]
    public void Parse_LeadingBangs_AreCounted() => Assert.Equal(2, BindingPathParser.Parse("!!A").Negations);

    [Theory]
    [InlineData("$parent[Unclosed.Foo")]
    [InlineData("a b")]
    [InlineData("..")]
    [InlineData("@@@")]
    public void Parse_BadInput_NeverThrowsAndReportsUnsupported(string path)
    {
        var parsed = BindingPathParser.Parse(path);

        Assert.NotNull(parsed.Unsupported);
    }
}
