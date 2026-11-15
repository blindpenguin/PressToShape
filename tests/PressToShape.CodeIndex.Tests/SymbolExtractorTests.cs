using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class SymbolExtractorTests
{
    private static IReadOnlyList<SymbolRecord> Extract(params string[] sources) =>
        new SymbolExtractor(TestCompilation.RepoRoot).Extract(TestCompilation.Project(sources));

    private static SymbolRecord Get(IReadOnlyList<SymbolRecord> symbols, string id) =>
        symbols.Single(s => s.Id == id);

    [Fact]
    public void Extract_EachSymbolType_IsRecognized()
    {
        var symbols = Extract("""
            namespace N;
            public class C { public int F; public event System.Action? E; public int P { get; set; } public C() { } public void M() { } }
            public interface I { }
            public struct S { }
            public enum En { A }
            public delegate void D(int x);
            public record R(int X);
            """);

        Assert.Equal(SymbolType.Namespace, Get(symbols, "N:N").SymbolType);
        Assert.Equal(SymbolType.Class, Get(symbols, "T:N.C").SymbolType);
        Assert.Equal(SymbolType.Interface, Get(symbols, "T:N.I").SymbolType);
        Assert.Equal(SymbolType.Struct, Get(symbols, "T:N.S").SymbolType);
        Assert.Equal(SymbolType.Enum, Get(symbols, "T:N.En").SymbolType);
        Assert.Equal(SymbolType.Delegate, Get(symbols, "T:N.D").SymbolType);
        Assert.Equal(SymbolType.Record, Get(symbols, "T:N.R").SymbolType);
        Assert.Equal(SymbolType.Field, Get(symbols, "F:N.C.F").SymbolType);
        Assert.Equal(SymbolType.Event, Get(symbols, "E:N.C.E").SymbolType);
        Assert.Equal(SymbolType.Property, Get(symbols, "P:N.C.P").SymbolType);
        Assert.Equal(SymbolType.Constructor, Get(symbols, "M:N.C.#ctor").SymbolType);
        Assert.Equal(SymbolType.Method, Get(symbols, "M:N.C.M").SymbolType);
        Assert.Equal(SymbolType.Field, Get(symbols, "F:N.En.A").SymbolType);
    }

    [Fact]
    public void Extract_Method_RecordsSignatureParametersAndReturnType()
    {
        var symbols = Extract("namespace N; public class C { public static string? M(int count, System.Collections.Generic.List<string> names, bool flag = false) => null; }");

        var method = Get(symbols, "M:N.C.M(System.Int32,System.Collections.Generic.List{System.String},System.Boolean)");

        Assert.Equal("M", method.Name);
        Assert.Equal("N.C.M", method.QualifiedName);
        Assert.Equal("string? M(int count, List<string> names, bool flag = false)", method.Signature);
        Assert.Equal("string?", method.TypeName);
        Assert.Equal(["count:int", "names:List<string>", "flag:bool"], method.Parameters.Select(p => p.Name + ":" + p.Type));
        Assert.Equal("public", method.Accessibility);
        Assert.Equal(["static"], method.Modifiers);
        Assert.Equal("T:N.C", method.ContainingId);
        Assert.Equal("N", method.Namespace);
    }

    [Fact]
    public void Extract_Property_RecordsTypeAndSignature()
    {
        var symbols = Extract("namespace N; public class C { public int Count { get; private set; } public string this[int i] => \"\"; }");

        var property = Get(symbols, "P:N.C.Count");
        var indexer = Get(symbols, "P:N.C.Item(System.Int32)");

        Assert.Equal("int", property.TypeName);
        Assert.Contains("Count", property.Signature, System.StringComparison.Ordinal);
        Assert.Single(indexer.Parameters);
    }

    [Fact]
    public void Extract_Delegate_TakesParametersFromInvoke()
    {
        var symbols = Extract("namespace N; public delegate string D(int x, bool y);");

        var delegateSymbol = Get(symbols, "T:N.D");

        Assert.Equal(["x", "y"], delegateSymbol.Parameters.Select(p => p.Name));
        Assert.Equal("string", delegateSymbol.TypeName);
    }

    [Fact]
    public void Extract_Modifiers_AreSortedAndStaticClassIsNotAbstractSealed()
    {
        var symbols = Extract("""
            namespace N;
            public static class Util { }
            public abstract class A { public abstract void M(); public virtual void V() { } }
            public sealed class B : A { public override void M() { } public readonly int X = 1; public const int K = 2; }
            """);

        Assert.Equal(["static"], Get(symbols, "T:N.Util").Modifiers);
        Assert.Equal(["abstract"], Get(symbols, "T:N.A").Modifiers);
        Assert.Equal(["abstract"], Get(symbols, "M:N.A.M").Modifiers);
        Assert.Equal(["virtual"], Get(symbols, "M:N.A.V").Modifiers);
        Assert.Equal(["sealed"], Get(symbols, "T:N.B").Modifiers);
        Assert.Equal(["override"], Get(symbols, "M:N.B.M").Modifiers);
        Assert.Equal(["readonly"], Get(symbols, "F:N.B.X").Modifiers);
        Assert.Equal(["const", "static"], Get(symbols, "F:N.B.K").Modifiers);
    }

    [Fact]
    public void Extract_InterfaceAndItsMembers_AreNotMarkedAbstract()
    {
        var symbols = Extract("namespace N; public interface I { void M(); int P { get; } }");

        Assert.Empty(Get(symbols, "T:N.I").Modifiers);
        Assert.Empty(Get(symbols, "M:N.I.M").Modifiers);
        Assert.Empty(Get(symbols, "P:N.I.P").Modifiers);
    }

    [Fact]
    public void Extract_Accessibility_UsesCSharpWords()
    {
        var symbols = Extract("""
            namespace N;
            internal class C
            {
                private int a; protected int b; protected internal int c; private protected int d; public int e;
            }
            """);

        Assert.Equal("internal", Get(symbols, "T:N.C").Accessibility);
        Assert.Equal("private", Get(symbols, "F:N.C.a").Accessibility);
        Assert.Equal("protected", Get(symbols, "F:N.C.b").Accessibility);
        Assert.Equal("protected internal", Get(symbols, "F:N.C.c").Accessibility);
        Assert.Equal("private protected", Get(symbols, "F:N.C.d").Accessibility);
        Assert.Equal("public", Get(symbols, "F:N.C.e").Accessibility);
    }

    [Fact]
    public void Extract_BaseTypeAndInterfaces_AreRecorded()
    {
        var symbols = Extract("""
            namespace N;
            public interface I1 { }
            public interface I2 { }
            public class Base { }
            public class Derived : Base, I2, I1 { }
            public class Plain { }
            """);

        var derived = Get(symbols, "T:N.Derived");

        Assert.Equal("Base", derived.BaseType);
        Assert.Equal(["I1", "I2"], derived.Interfaces);
        Assert.Null(Get(symbols, "T:N.Plain").BaseType);
    }

    [Fact]
    public void Extract_Attributes_RecordNameAndArguments()
    {
        var symbols = Extract("""
            namespace N;
            [System.Obsolete("old", true)]
            public class C { [System.Diagnostics.CodeAnalysis.AllowNull] public string P { get; set; } = ""; }
            """);

        var attribute = Assert.Single(Get(symbols, "T:N.C").Attributes);

        Assert.Equal("Obsolete", attribute.Name);
        Assert.Equal(["\"old\"", "true"], attribute.Arguments);
        Assert.Equal("AllowNull", Assert.Single(Get(symbols, "P:N.C.P").Attributes).Name);
    }

    [Fact]
    public void Extract_NestedAndGenericTypes_AreLinkedToTheirContainer()
    {
        var symbols = Extract("namespace N; public class Outer<T> { public class Inner { } }");

        var inner = Get(symbols, "T:N.Outer`1.Inner");

        Assert.Equal("T:N.Outer`1", inner.ContainingId);
        Assert.Equal("N.Outer<T>.Inner", inner.QualifiedName);
        Assert.Equal("N", inner.Namespace);
    }

    [Fact]
    public void Extract_PartialType_AppearsOnceAtItsFirstDeclaration()
    {
        var symbols = Extract(
            "namespace N; public partial class P { public void A() { } }",
            "namespace N; public partial class P { public void B() { } }");

        var type = Get(symbols, "T:N.P");

        Assert.Equal("src/Test/File0.cs", type.File);
        Assert.Contains("partial", type.Modifiers);
        Assert.Equal(1, symbols.Count(s => s.Id == "T:N.P"));
        Assert.Equal("src/Test/File1.cs", Get(symbols, "M:N.P.B").File);
    }

    [Fact]
    public void Extract_PrimaryConstructor_IsListedWithItsParameters()
    {
        var symbols = Extract("namespace N; public class C(int a, string b) { }");

        var ctor = Get(symbols, "M:N.C.#ctor(System.Int32,System.String)");

        Assert.Equal("C", ctor.Name);
        Assert.Equal(["a", "b"], ctor.Parameters.Select(p => p.Name));
    }

    [Fact]
    public void Extract_ImplicitMembers_AreSkipped()
    {
        var symbols = Extract("namespace N; public class C { public int P { get; set; } public record R(int X); }");

        Assert.DoesNotContain(symbols, s => s.Id.StartsWith("M:N.C.get_", System.StringComparison.Ordinal));
        Assert.DoesNotContain(symbols, s => s.Id.Contains("<P>k__BackingField", System.StringComparison.Ordinal));
        Assert.DoesNotContain(symbols, s => s.Id.Contains("EqualityContract", System.StringComparison.Ordinal));
    }

    [Fact]
    public void Extract_Locations_AreRepoRelativeWithForwardSlashesAndLineRanges()
    {
        var symbols = Extract("namespace N;\n\npublic class C\n{\n    public void M()\n    {\n    }\n}\n");

        var type = Get(symbols, "T:N.C");
        var method = Get(symbols, "M:N.C.M");

        Assert.Equal("src/Test/File0.cs", type.File);
        Assert.Equal("Test", type.Project);
        Assert.Equal((3, 8), (type.StartLine, type.EndLine));
        Assert.Equal((5, 7), (method.StartLine, method.EndLine));
        Assert.DoesNotContain(symbols, s => s.File.Contains('\\') || System.IO.Path.IsPathRooted(s.File));
    }

    [Fact]
    public void Extract_Namespaces_AreListedWithParents()
    {
        var symbols = Extract("namespace A.B.C { public class X { } }");

        Assert.Equal("N:A.B", Get(symbols, "N:A.B.C").ContainingId);
        Assert.Equal("N:A", Get(symbols, "N:A.B").ContainingId);
        Assert.Null(Get(symbols, "N:A").ContainingId);
        Assert.Equal("A.B.C", Get(symbols, "N:A.B.C").Namespace);
    }

    [Fact]
    public void Extract_Output_IsSortedAndStable()
    {
        var first = Extract("namespace N; public class B { public void Z() { } public void A() { } } public class A { }");
        var second = Extract("namespace N; public class B { public void Z() { } public void A() { } } public class A { }");

        Assert.Equal(first.Select(s => s.Id), second.Select(s => s.Id));
        Assert.Equal(first.Select(s => s.Id).OrderBy(i => i, System.StringComparer.Ordinal), first.Select(s => s.Id));
    }
}
