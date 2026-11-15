using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Relationships;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class RelationshipBuilderTests
{
    private static IReadOnlyList<RelationshipRecord> Build(params string[] sources)
    {
        var project = TestCompilation.Project(sources);
        return BuildFor(project, project);
    }

    private static IReadOnlyList<RelationshipRecord> BuildFor(ProjectCompilation target, params ProjectCompilation[] solution)
    {
        var extractor = new SymbolExtractor(TestCompilation.RepoRoot);
        var known = solution.SelectMany(p => extractor.Extract(p)).Select(s => s.Id).ToHashSet();
        // These tests cover the structural edges. References, calls and registrations are tested in OperationRelationshipTests.
        return new RelationshipBuilder(known, TestCompilation.RepoRoot).Build(target)
            .Where(e => e.Type is RelationshipType.Inherits or RelationshipType.Implements or RelationshipType.InjectedDependency)
            .ToList();
    }

    private static string Describe(RelationshipRecord edge) => $"{edge.Type}:{edge.FromId}->{edge.ToId}";

    [Fact]
    public void Build_ClassChain_RecordsInheritsForEachLevel()
    {
        var edges = Build("namespace N; public class A { } public class B : A { } public class C : B { }");

        Assert.Equal(["Inherits:T:N.B->T:N.A", "Inherits:T:N.C->T:N.B"], edges.Select(Describe));
        Assert.All(edges, e => Assert.Equal(Confidence.Verified, e.Confidence));
    }

    [Fact]
    public void Build_InterfaceImplementation_RecordsImplementsIncludingInheritedInterfaces()
    {
        var edges = Build("""
            namespace N;
            public interface IBase { }
            public interface IThing : IBase { }
            public class Impl : IThing { }
            public class Child : Impl { }
            """);

        var described = edges.Select(Describe).ToList();

        Assert.Contains("Inherits:T:N.IThing->T:N.IBase", described);
        Assert.Contains("Implements:T:N.Impl->T:N.IThing", described);
        Assert.Contains("Implements:T:N.Impl->T:N.IBase", described);
        Assert.Contains("Inherits:T:N.Child->T:N.Impl", described);
    }

    [Fact]
    public void Build_FrameworkTypes_AreNeverTargets()
    {
        var edges = Build("namespace N; public class C : System.Collections.Generic.List<int>, System.IDisposable { public void Dispose() { } public C(System.Text.StringBuilder b) { } }");

        Assert.Empty(edges);
    }

    [Fact]
    public void Build_ConstructorParameters_RecordInjectedDependencies()
    {
        var edges = Build("""
            namespace N;
            public interface IService { }
            public class Helper { }
            public enum Mode { A }
            public class Consumer(IService service, Helper helper, Mode mode, string name, int count) { }
            """);

        Assert.Equal(["InjectedDependency:T:N.Consumer->T:N.Helper", "InjectedDependency:T:N.Consumer->T:N.IService"], edges.Select(Describe));
    }

    [Fact]
    public void Build_ExplicitConstructor_RecordsInjectedDependency()
    {
        var edges = Build("namespace N; public interface IService { } public class Consumer { public Consumer(IService s) { } }");

        var edge = Assert.Single(edges);

        Assert.Equal(RelationshipType.InjectedDependency, edge.Type);
        Assert.Equal("src/Test/File0.cs", edge.File);
        Assert.Equal(1, edge.Line);
    }

    [Fact]
    public void Build_TwoConstructorsSameParameter_GiveOneEdge()
    {
        var edges = Build("namespace N; public interface IService { } public class C { public C(IService s) { } public C(IService s, int x) { } }");

        Assert.Single(edges);
    }

    [Fact]
    public void Build_GenericBaseAndParameter_UseTheOriginalDefinition()
    {
        var edges = Build("""
            namespace N;
            public class Base<T> { }
            public interface IRepo<T> { }
            public class Derived : Base<int> { public Derived(IRepo<string> repo) { } }
            """);

        Assert.Equal(["Inherits:T:N.Derived->T:N.Base`1", "InjectedDependency:T:N.Derived->T:N.IRepo`1"], edges.Select(Describe).OrderBy(d => d, System.StringComparer.Ordinal));
    }

    [Fact]
    public void Build_RecordCopyConstructorAndSelfReference_AreIgnored()
    {
        var edges = Build("namespace N; public record R(int X); public class Node { public Node(Node parent) { } }");

        Assert.Empty(edges);
    }

    [Fact]
    public void Build_NestedTypes_AreIncluded()
    {
        var edges = Build("namespace N; public interface I { } public class Outer { public class Inner : I { } }");

        Assert.Equal(["Implements:T:N.Outer.Inner->T:N.I"], edges.Select(Describe));
    }

    [Fact]
    public void Build_TargetInAnotherProject_IsRecorded()
    {
        var core = TestCompilation.Project("Core", "namespace Core; public interface IService { }");
        var app = TestCompilation.Project("App", "namespace App; public class Impl : Core.IService { public Impl(Core.IService other) { } }");
        app = app with { Compilation = app.Compilation.AddReferences(core.Compilation.ToMetadataReference()) };

        var edges = BuildFor(app, core, app);

        Assert.Equal(["Implements:T:App.Impl->T:Core.IService", "InjectedDependency:T:App.Impl->T:Core.IService"], edges.Select(Describe));
    }

    [Fact]
    public void Build_Output_IsSortedAndStable()
    {
        const string source = "namespace N; public interface I { } public interface J { } public class B : I, J { } public class A : J, I { }";

        var first = Build(source).Select(Describe).ToList();
        var second = Build(source).Select(Describe).ToList();

        Assert.Equal(first, second);
        Assert.Equal("Implements:T:N.A->T:N.I", first[0]);
    }
}
