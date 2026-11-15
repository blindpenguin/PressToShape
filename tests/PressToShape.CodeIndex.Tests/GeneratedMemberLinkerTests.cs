using System.Linq;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class GeneratedMemberLinkerTests
{
    private const string Stubs = """
        namespace CommunityToolkit.Mvvm.ComponentModel { [System.AttributeUsage(System.AttributeTargets.Field)] public sealed class ObservablePropertyAttribute : System.Attribute { } }
        namespace CommunityToolkit.Mvvm.Input { [System.AttributeUsage(System.AttributeTargets.Method)] public sealed class RelayCommandAttribute : System.Attribute { } }
        """;

    private static System.Collections.Generic.IReadOnlyList<SymbolRecord> Extract(string source, string generatedSource)
    {
        var project = TestCompilation.WithGenerator(TestCompilation.Project("Test", Stubs, source), new FixedSourceGenerator("Generated.g.cs", generatedSource));
        return new SymbolExtractor(TestCompilation.RepoRoot).Extract(project);
    }

    [Fact]
    public void Extract_GeneratedProperty_IsMarkedAndLinkedToTheObservableField()
    {
        var symbols = Extract(
            "namespace N; public partial class VM { [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty] private string _title = \"\"; }",
            "namespace N { partial class VM { public string Title { get; set; } } }");

        var property = symbols.Single(s => s.Id == "P:N.VM.Title");

        Assert.True(property.IsGenerated);
        Assert.Equal("F:N.VM._title", property.GeneratedFrom);
        Assert.Equal("generated/Generated.g.cs", property.File);
        Assert.False(symbols.Single(s => s.Id == "T:N.VM").IsGenerated);
    }

    [Fact]
    public void Extract_PartialPropertyWithGeneratedImplementation_StaysHandWrittenAndKeepsItsAttribute()
    {
        var symbols = Extract(
            "namespace N; public partial class VM { [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty] public partial string Title { get; set; } }",
            "namespace N { partial class VM { public partial string Title { get => \"\"; set { } } } }");

        var property = symbols.Single(s => s.Id == "P:N.VM.Title");

        Assert.False(property.IsGenerated);
        Assert.Null(property.GeneratedFrom);
        Assert.Equal("src/Test/File1.cs", property.File);
        Assert.Equal("ObservableProperty", Assert.Single(property.Attributes).Name);
    }

    [Fact]
    public void Extract_GeneratedCommand_IsLinkedToTheRelayCommandMethod()
    {
        var symbols = Extract(
            "namespace N; public partial class VM { [CommunityToolkit.Mvvm.Input.RelayCommand] private System.Threading.Tasks.Task SaveAsync() => System.Threading.Tasks.Task.CompletedTask; [CommunityToolkit.Mvvm.Input.RelayCommand] private void Open() { } }",
            "namespace N { partial class VM { public object SaveCommand { get; } public object OpenCommand { get; } } }");

        Assert.Equal("M:N.VM.SaveAsync", symbols.Single(s => s.Id == "P:N.VM.SaveCommand").GeneratedFrom);
        Assert.Equal("M:N.VM.Open", symbols.Single(s => s.Id == "P:N.VM.OpenCommand").GeneratedFrom);
    }

    [Fact]
    public void Extract_FieldWithPrefix_MapsToCapitalizedPropertyName()
    {
        var symbols = Extract(
            "namespace N; public partial class VM { [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty] private int m_count; }",
            "namespace N { partial class VM { public int Count { get; set; } } }");

        Assert.Equal("F:N.VM.m_count", symbols.Single(s => s.Id == "P:N.VM.Count").GeneratedFrom);
    }

    [Fact]
    public void Extract_GeneratedMemberWithoutMatchingSource_HasNoOrigin()
    {
        var symbols = Extract(
            "namespace N; public partial class VM { private int _other; }",
            "namespace N { partial class VM { public int Orphan { get; set; } } }");

        var property = symbols.Single(s => s.Id == "P:N.VM.Orphan");

        Assert.True(property.IsGenerated);
        Assert.Null(property.GeneratedFrom);
    }

    [Fact]
    public void Extract_HandWrittenMember_IsNotGenerated()
    {
        var symbols = Extract(
            "namespace N; public partial class VM { public int Own { get; set; } }",
            "namespace N { partial class VM { public int Other { get; set; } } }");

        var own = symbols.Single(s => s.Id == "P:N.VM.Own");

        Assert.False(own.IsGenerated);
        Assert.Null(own.GeneratedFrom);
        Assert.StartsWith("src/", own.File, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Extract_PartialTypeWithGeneratedPart_PointsAtHandWrittenFile()
    {
        var symbols = Extract(
            "namespace N; public partial class VM { }",
            "namespace N { partial class VM { public int Other { get; set; } } }");

        Assert.Equal("src/Test/File1.cs", symbols.Single(s => s.Id == "T:N.VM").File);
    }

    [Fact]
    public void Extract_GeneratedMembers_NeverContainMachinePaths()
    {
        var symbols = Extract(
            "namespace N; public partial class VM { }",
            "namespace N { partial class VM { public int Other { get; set; } } }");

        Assert.DoesNotContain(symbols, s => System.IO.Path.IsPathRooted(s.File) || s.File.Contains("..", System.StringComparison.Ordinal));
    }
}

internal sealed class FixedSourceGenerator(string hintName, string source) : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context) =>
        context.RegisterPostInitializationOutput(c => c.AddSource(hintName, source));
}
