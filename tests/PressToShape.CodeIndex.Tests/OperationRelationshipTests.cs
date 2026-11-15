using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Relationships;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class OperationRelationshipTests
{
    private const string DiStub = """
        namespace Microsoft.Extensions.DependencyInjection
        {
            public interface IServiceCollection { }
            public static class ServiceCollectionExtensions
            {
                public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services) => services;
                public static IServiceCollection AddTransient<TService>(this IServiceCollection services) => services;
            }
        }
        """;

    private static IReadOnlyList<RelationshipRecord> Build(params string[] sources)
    {
        var project = TestCompilation.Project(sources);
        var known = new SymbolExtractor(TestCompilation.RepoRoot).Extract(project).Select(s => s.Id).ToHashSet();
        return new RelationshipBuilder(known, TestCompilation.RepoRoot).Build(project);
    }

    private static List<string> Of(IEnumerable<RelationshipRecord> edges, RelationshipType type) =>
        edges.Where(e => e.Type == type).Select(e => $"{e.FromId}->{e.ToId}").ToList();

    [Fact]
    public void Build_DirectCall_RecordsCallWithLine()
    {
        var edges = Build("namespace N;\npublic class C\n{\n    public void A() { B(); }\n    public void B() { }\n}\n");

        var call = Assert.Single(edges, e => e.Type == RelationshipType.Calls);

        Assert.Equal("M:N.C.A", call.FromId);
        Assert.Equal("M:N.C.B", call.ToId);
        Assert.Equal("src/Test/File0.cs", call.File);
        Assert.Equal(4, call.Line);
        Assert.Equal(Confidence.Verified, call.Confidence);
    }

    [Fact]
    public void Build_CallThroughInterface_PointsAtTheInterfaceMethod()
    {
        var edges = Build("namespace N; public interface I { void Run(); } public class C { public void Go(I i) { i.Run(); } }");

        Assert.Contains("M:N.C.Go(N.I)->M:N.I.Run", Of(edges, RelationshipType.Calls));
    }

    [Fact]
    public void Build_GenericMethodAndExtensionMethod_UseTheDeclaration()
    {
        var edges = Build("""
            namespace N;
            public static class Ext { public static void Twice(this int x) { } public static T Id<T>(T v) => v; }
            public class C { public void M() { 1.Twice(); Ext.Id<string>("a"); } }
            """);

        var calls = Of(edges, RelationshipType.Calls);

        Assert.Contains("M:N.C.M->M:N.Ext.Twice(System.Int32)", calls);
        Assert.Contains("M:N.C.M->M:N.Ext.Id``1(``0)", calls);
    }

    [Fact]
    public void Build_MemberReferences_AreRecorded()
    {
        var edges = Build("""
            namespace N;
            public class C
            {
                public int Field; public int Prop { get; set; } public event System.Action? Evt; public void Target() { }
                public void M() { Field = 1; Prop = 2; Evt += Target; var d = (System.Action)Target; }
            }
            """);

        var references = Of(edges, RelationshipType.References);

        Assert.Contains("M:N.C.M->F:N.C.Field", references);
        Assert.Contains("M:N.C.M->P:N.C.Prop", references);
        Assert.Contains("M:N.C.M->E:N.C.Evt", references);
        Assert.Contains("M:N.C.M->M:N.C.Target", references);
    }

    [Fact]
    public void Build_ObjectCreation_RecordsConstructorCallAndTypeReference()
    {
        var edges = Build("namespace N; public class Made { public Made(int x) { } } public class C { public object M() => new Made(1); }");

        Assert.Contains("M:N.C.M->M:N.Made.#ctor(System.Int32)", Of(edges, RelationshipType.Calls));
        Assert.Contains("M:N.C.M->T:N.Made", Of(edges, RelationshipType.References));
    }

    [Fact]
    public void Build_SignatureTypes_AreReferences()
    {
        var edges = Build("""
            namespace N;
            public class Foo { } public class Bar { } public class Baz { } public class Qux { }
            public class C
            {
                private System.Collections.Generic.List<Foo> _items = new();
                public Bar Make(Baz[] items) => null!;
                public Qux Prop { get; set; } = null!;
            }
            """);

        var references = Of(edges, RelationshipType.References);

        Assert.Contains("F:N.C._items->T:N.Foo", references);
        Assert.Contains("M:N.C.Make(N.Baz[])->T:N.Bar", references);
        Assert.Contains("M:N.C.Make(N.Baz[])->T:N.Baz", references);
        Assert.Contains("P:N.C.Prop->T:N.Qux", references);
    }

    [Fact]
    public void Build_FrameworkTargets_AreNotRecorded()
    {
        var edges = Build("namespace N; public class C { public string M() { var sb = new System.Text.StringBuilder(); sb.Append(1); return sb.ToString(); } }");

        Assert.DoesNotContain(edges, e => e.ToId.StartsWith("M:System", System.StringComparison.Ordinal) || e.ToId.StartsWith("T:System", System.StringComparison.Ordinal));
    }

    [Fact]
    public void Build_DynamicAndDelegateCalls_AreNotRecordedAsCalls()
    {
        var edges = Build("""
            namespace N;
            public class Target { public void Hidden() { } }
            public class C
            {
                public void Dyn(dynamic d) { d.Hidden(); }
                public void Del(System.Action action) { action(); }
            }
            """);

        Assert.DoesNotContain(edges, e => e.Type == RelationshipType.Calls);
    }

    [Fact]
    public void Build_LambdaAndLocalFunction_AreAttributedToTheEnclosingMember()
    {
        var edges = Build("""
            namespace N;
            public class C
            {
                public void Target() { }
                public void M()
                {
                    System.Action a = () => Target();
                    void Local() { Target(); }
                    Local();
                }
            }
            """);

        var calls = Of(edges, RelationshipType.Calls);

        Assert.Equal(2, calls.Count(c => c == "M:N.C.M->M:N.C.Target"));
    }

    [Fact]
    public void Build_AccessorsInitializersAndConstructorChaining_AreAttributed()
    {
        var edges = Build("""
            namespace N;
            public class C
            {
                public static int Make() => 1;
                public static void Hook() { }
                public int Prop { get { return Make(); } }
                public int Auto { get; } = Make();
                public int Arrow => Make();
                private int _field = Make();
                public C() : this(Make()) { }
                public C(int x) { Hook(); }
            }
            """);

        var calls = Of(edges, RelationshipType.Calls);

        Assert.Contains("P:N.C.Prop->M:N.C.Make", calls);
        Assert.Contains("P:N.C.Auto->M:N.C.Make", calls);
        Assert.Contains("P:N.C.Arrow->M:N.C.Make", calls);
        Assert.Contains("F:N.C._field->M:N.C.Make", calls);
        Assert.Contains("M:N.C.#ctor->M:N.C.Make", calls);
        Assert.Contains("M:N.C.#ctor->M:N.C.#ctor(System.Int32)", calls);
        Assert.Contains("M:N.C.#ctor(System.Int32)->M:N.C.Hook", calls);
    }

    [Fact]
    public void Build_NoContainer_NoRegistrationIsInvented()
    {
        var edges = Build("namespace N; public interface IFoo { } public class Foo : IFoo { } public class Root { public Root(IFoo foo) { } }");

        Assert.Empty(Of(edges, RelationshipType.Registers));
    }

    [Fact]
    public void Build_ContainerRegistration_RecordsServiceToImplementation()
    {
        var edges = Build(DiStub, """
            using Microsoft.Extensions.DependencyInjection;
            namespace N;
            public interface IFoo { } public class Foo : IFoo { } public interface IBar { }
            public static class Startup
            {
                public static void Configure(IServiceCollection services)
                {
                    services.AddSingleton<IFoo, Foo>();
                    services.AddTransient<IBar>();
                }
            }
            """);

        var registrations = edges.Where(e => e.Type == RelationshipType.Registers).ToList();

        Assert.Equal(["T:N.IBar->T:N.IBar", "T:N.IFoo->T:N.Foo"], registrations.Select(e => $"{e.FromId}->{e.ToId}").OrderBy(x => x, System.StringComparer.Ordinal));
        Assert.Contains("Singleton, registered in M:N.Startup.Configure(Microsoft.Extensions.DependencyInjection.IServiceCollection)", registrations.Select(e => e.Reason));
        Assert.All(registrations, e => Assert.Equal(Confidence.Verified, e.Confidence));
    }

    [Fact]
    public void Build_AllEdges_AreVerifiedAndSorted()
    {
        var edges = Build("namespace N; public class C { public void A() { B(); } public void B() { A(); } }");

        Assert.All(edges, e => Assert.Equal(Confidence.Verified, e.Confidence));
        Assert.Equal(edges.OrderBy(e => e.Type).ThenBy(e => e.FromId, System.StringComparer.Ordinal).ThenBy(e => e.ToId, System.StringComparer.Ordinal).ThenBy(e => e.File, System.StringComparer.Ordinal).ThenBy(e => e.Line), edges);
    }
}
