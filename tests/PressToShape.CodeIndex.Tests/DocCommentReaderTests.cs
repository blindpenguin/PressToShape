using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class DocCommentReaderTests
{
    private static SymbolRecord Get(string id, params string[] sources) =>
        new SymbolExtractor(TestCompilation.RepoRoot).Extract(TestCompilation.Project(sources)).Single(s => s.Id == id);

    [Fact]
    public void Read_FullComment_StoresEverySection()
    {
        var method = Get("M:N.C.M(System.Int32)", """
            namespace N;
            public class C
            {
                /// <summary>Does the thing.</summary>
                /// <remarks>Be careful.</remarks>
                /// <param name="x">The x value.</param>
                /// <returns>Nothing useful.</returns>
                /// <exception cref="System.ArgumentException">When x is bad.</exception>
                public int M(int x) => x;
            }
            """);

        var doc = method.Doc;

        Assert.True(doc.HasDocumentation);
        Assert.Equal("Does the thing.", doc.Summary);
        Assert.Equal("Be careful.", doc.Remarks);
        Assert.Equal("The x value.", doc.Params["x"]);
        Assert.Equal("Nothing useful.", doc.Returns);
        var exception = Assert.Single(doc.Exceptions);
        Assert.Equal("System.ArgumentException", exception.Type);
        Assert.Equal("When x is bad.", exception.Description);
        Assert.Null(doc.InheritedFrom);
    }

    [Fact]
    public void Read_NoComment_IsUndocumentedWithoutText()
    {
        var method = Get("M:N.C.M", "namespace N; public class C { public void M() { } }");

        Assert.False(method.Doc.HasDocumentation);
        Assert.Null(method.Doc.Summary);
        Assert.Empty(method.Doc.Params);
        Assert.Empty(method.Doc.Exceptions);
    }

    [Fact]
    public void Read_OnlySummary_LeavesOtherSectionsEmpty()
    {
        var type = Get("T:N.C", "namespace N;\n/// <summary>Just a summary.</summary>\npublic class C { }");

        Assert.True(type.Doc.HasDocumentation);
        Assert.Null(type.Doc.Remarks);
        Assert.Null(type.Doc.Returns);
    }

    [Fact]
    public void Read_EmptySummary_CountsAsUndocumented()
    {
        var type = Get("T:N.C", "namespace N;\n/// <summary></summary>\npublic class C { }");

        Assert.False(type.Doc.HasDocumentation);
    }

    [Fact]
    public void Read_InheritDocOnOverride_UsesBaseTextAndNamesSource()
    {
        var method = Get("M:N.D.M", """
            namespace N;
            public class B { /// <summary>Base summary.</summary>
                public virtual void M() { } }
            public class D : B { /// <inheritdoc/>
                public override void M() { } }
            """);

        Assert.True(method.Doc.HasDocumentation);
        Assert.Equal("Base summary.", method.Doc.Summary);
        Assert.Equal("M:N.B.M", method.Doc.InheritedFrom);
    }

    [Fact]
    public void Read_InheritDocOnInterfaceImplementation_UsesInterfaceText()
    {
        var method = Get("M:N.C.Run", """
            namespace N;
            public interface I { /// <summary>Runs it.</summary>
                void Run(); }
            public class C : I { /// <inheritdoc/>
                public void Run() { } }
            """);

        Assert.Equal("Runs it.", method.Doc.Summary);
        Assert.Equal("M:N.I.Run", method.Doc.InheritedFrom);
    }

    [Fact]
    public void Read_InheritDocWithOwnSummary_KeepsOwnTextAndFillsTheRest()
    {
        var method = Get("M:N.D.M(System.Int32)", """
            namespace N;
            public class B { /// <summary>Base summary.</summary>
                /// <param name="x">Base x.</param>
                public virtual void M(int x) { } }
            public class D : B { /// <summary>Own summary.</summary>
                /// <inheritdoc/>
                public override void M(int x) { } }
            """);

        Assert.Equal("Own summary.", method.Doc.Summary);
        Assert.Equal("Base x.", method.Doc.Params["x"]);
        Assert.Equal("M:N.B.M(System.Int32)", method.Doc.InheritedFrom);
    }

    [Fact]
    public void Read_InheritDocChain_PointsAtTheSymbolThatHasTheText()
    {
        var method = Get("M:N.E.M", """
            namespace N;
            public class A { /// <summary>Root text.</summary>
                public virtual void M() { } }
            public class B : A { /// <inheritdoc/>
                public override void M() { } }
            public class E : B { /// <inheritdoc/>
                public override void M() { } }
            """);

        Assert.Equal("Root text.", method.Doc.Summary);
        Assert.Equal("M:N.A.M", method.Doc.InheritedFrom);
    }

    [Fact]
    public void Read_InheritDocWithoutDocumentedSource_StaysUndocumented()
    {
        var method = Get("M:N.D.M", """
            namespace N;
            public class B { public virtual void M() { } }
            public class D : B { /// <inheritdoc/>
                public override void M() { } }
            """);

        Assert.False(method.Doc.HasDocumentation);
        Assert.Null(method.Doc.InheritedFrom);
    }

    [Fact]
    public void Read_InheritDocWithCref_UsesThatSymbol()
    {
        var method = Get("M:N.C.M", """
            namespace N;
            public class Other { /// <summary>From other.</summary>
                public void Source() { } }
            public class C { /// <inheritdoc cref="Other.Source"/>
                public void M() { } }
            """);

        Assert.Equal("From other.", method.Doc.Summary);
        Assert.Equal("M:N.Other.Source", method.Doc.InheritedFrom);
    }

    [Fact]
    public void Read_InlineTags_AreShownAsPlainText()
    {
        var method = Get("M:N.C.M(System.Int32)", """
            namespace N;
            public class C
            {
                /// <summary>Uses <see cref="C"/>, <paramref name="x"/> and <see langword="null"/>
                /// with <c>code</c>.</summary>
                /// <remarks><para>First paragraph.</para><para>Second   paragraph.</para></remarks>
                public void M(int x) { }
            }
            """);

        Assert.Equal("Uses N.C, x and null with code.", method.Doc.Summary);
        Assert.Equal("First paragraph.\n\nSecond paragraph.", method.Doc.Remarks);
    }

    [Fact]
    public void Read_MalformedXml_DoesNotThrowAndIsUndocumented()
    {
        var type = Get("T:N.C", "namespace N;\n/// <summary>Broken <b></summary>\npublic class C { }");

        Assert.False(type.Doc.Params.Any());
        Assert.NotNull(type.Doc);
    }

    [Fact]
    public void Read_ParamsAreKeptInSourceOrder()
    {
        var method = Get("M:N.C.M(System.Int32,System.Int32)", """
            namespace N;
            public class C
            {
                /// <summary>S.</summary>
                /// <param name="b">B.</param>
                /// <param name="a">A.</param>
                public void M(int b, int a) { }
            }
            """);

        Assert.Equal(new List<string> { "b", "a" }, method.Doc.Params.Keys.ToList());
    }
}
