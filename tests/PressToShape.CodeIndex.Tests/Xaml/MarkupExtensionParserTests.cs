using System;
using System.Linq;
using PressToShape.CodeIndex.Xaml;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class MarkupExtensionParserTests
{
    private static MarkupExtension Parse(string text)
    {
        Assert.True(MarkupExtensionParser.TryParse(text, out var extension), "should parse: " + text);
        return extension!;
    }

    private static string[] Texts(MarkupExtension e) => e.Positional.Select(p => p is MarkupText text ? text.Value : "<nested>").ToArray();

    [Fact]
    public void TryParse_BindingWithPositionalPath_ReadsNameAndPath()
    {
        var e = Parse("{Binding AddLayerCommand}");

        Assert.Equal("Binding", e.Name);
        Assert.Equal(["AddLayerCommand"], Texts(e));
        Assert.Empty(e.Named);
        Assert.Equal("AddLayerCommand", e.FirstPositionalText);
    }

    [Fact]
    public void TryParse_BindingWithoutArguments_HasNone()
    {
        var e = Parse("{Binding}");

        Assert.Equal("Binding", e.Name);
        Assert.Empty(e.Positional);
        Assert.Null(e.FirstPositionalText);
    }

    [Fact]
    public void TryParse_PositionalAndNamedArguments_AreSeparated()
    {
        var e = Parse("{Binding Stroke.IsEnabled, Mode=OneWay, StringFormat='{}{0:F1} px'}");

        Assert.Equal(["Stroke.IsEnabled"], Texts(e));
        Assert.Equal("OneWay", e.NamedText("Mode"));
        Assert.Equal("{}{0:F1} px", e.NamedText("StringFormat"));
    }

    [Fact]
    public void TryParse_NamedPathOnly_ReadsIt()
    {
        var e = Parse("{Binding Path=Foo.Bar}");

        Assert.Equal("Foo.Bar", e.NamedText("Path"));
        Assert.Empty(e.Positional);
    }

    [Fact]
    public void TryParse_NestedExtension_IsKept()
    {
        var e = Parse("{Binding IsChecked, Converter={StaticResource CheckableToggleTypeConverter}}");

        var converter = e.NamedExtension("Converter")!;
        Assert.Equal("StaticResource", converter.Name);
        Assert.Equal("CheckableToggleTypeConverter", converter.FirstPositionalText);
        Assert.Null(e.NamedText("Converter"));
    }

    [Fact]
    public void TryParse_TwoLevelsOfNesting_Works()
    {
        var e = Parse("{Binding Foo, RelativeSource={RelativeSource AncestorType=vm:Shell, Mode=FindAncestor}}");

        var relative = e.NamedExtension("RelativeSource")!;
        Assert.Equal("RelativeSource", relative.Name);
        Assert.Equal("vm:Shell", relative.NamedText("AncestorType"));
        Assert.Equal("FindAncestor", relative.NamedText("Mode"));
    }

    [Fact]
    public void TryParse_StaticWithPrefixedMember_ReadsTheMemberText()
    {
        var e = Parse("{x:Static local:CoreColorBrushConverter.Instance}");

        Assert.Equal("x:Static", e.Name);
        Assert.Equal("local:CoreColorBrushConverter.Instance", e.FirstPositionalText);
    }

    [Theory]
    [InlineData("{loc:T 'Add layer'}", "Add layer")]
    [InlineData("{loc:T \"Add layer\"}", "Add layer")]
    [InlineData("{loc:T 'Hello, world'}", "Hello, world")]
    [InlineData("{loc:T 'It\\'s here'}", "It's here")]
    public void TryParse_QuotedText_KeepsCommasAndEscapes(string text, string expected)
    {
        var e = Parse(text);

        Assert.Equal("loc:T", e.Name);
        Assert.Equal(expected, e.FirstPositionalText);
    }

    [Theory]
    [InlineData("{StaticResource PanelPadding}", "StaticResource")]
    [InlineData("{DynamicResource WindowBackgroundBrush}", "DynamicResource")]
    [InlineData("{TemplateBinding Background}", "TemplateBinding")]
    [InlineData("{CompiledBinding Name}", "CompiledBinding")]
    [InlineData("{ReflectionBinding Name}", "ReflectionBinding")]
    public void TryParse_OtherExtensions_ReadNameAndKey(string text, string name)
    {
        var e = Parse(text);

        Assert.Equal(name, e.Name);
        Assert.Single(e.Positional);
    }

    [Fact]
    public void TryParse_ParentPathWithCommaInBrackets_DoesNotSplit()
    {
        var e = Parse("{Binding $parent[ItemsControl,2].DataContext.Foo, Mode=TwoWay}");

        Assert.Equal(["$parent[ItemsControl,2].DataContext.Foo"], Texts(e));
        Assert.Equal("TwoWay", e.NamedText("Mode"));
    }

    [Theory]
    [InlineData("{Binding (local:Attached.Prop)}", "(local:Attached.Prop)")]
    [InlineData("{Binding Items[0].Name}", "Items[0].Name")]
    [InlineData("{Binding !IsEnabled}", "!IsEnabled")]
    [InlineData("{Binding .}", ".")]
    [InlineData("{Binding Items[Key=1].Name}", "Items[Key=1].Name")]
    public void TryParse_SpecialPathForms_AreKeptAsText(string text, string path)
    {
        Assert.Equal(path, Parse(text).FirstPositionalText);
    }

    [Fact]
    public void TryParse_ElementNameAndWhitespaceVariations_Work()
    {
        var e = Parse("  {  Binding   Text ,ElementName = box,  Mode=TwoWay  }  ");

        Assert.Equal("Binding", e.Name);
        Assert.Equal("Text", e.FirstPositionalText);
        Assert.Equal("box", e.NamedText("ElementName"));
        Assert.Equal("TwoWay", e.NamedText("Mode"));
    }

    [Fact]
    public void TryParse_TrailingComma_IsTolerated()
    {
        Assert.Equal("Name", Parse("{Binding Name, }").FirstPositionalText);
    }

    [Theory]
    [InlineData("{}Literal text")]
    [InlineData("{}")]
    [InlineData("plain text")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{")]
    [InlineData("}")]
    [InlineData("{Binding Foo")]
    [InlineData("{Binding Foo}}")]
    [InlineData("{Binding Foo} trailing")]
    [InlineData("{Binding Foo, Converter={StaticResource X}")]
    [InlineData("{Binding 'unterminated}")]
    [InlineData("{Binding Foo Bar=1}")]
    [InlineData("{Binding =1}")]
    [InlineData("{Binding {A} {B}}")]
    [InlineData("{ }")]
    [InlineData("{, }")]
    public void TryParse_BadOrNonExtensionInput_ReturnsFalseWithoutThrowing(string text)
    {
        Assert.False(MarkupExtensionParser.TryParse(text, out var extension));
        Assert.Null(extension);
    }

    [Fact]
    public void TryParse_Null_ReturnsFalse() => Assert.False(MarkupExtensionParser.TryParse(null, out _));

    [Fact]
    public void TryParse_UnknownName_StillParses() => Assert.Equal("MyThing", Parse("{MyThing a, b=c}").Name);

    [Fact]
    public void TryParse_NestingAtTheLimit_WorksAndBeyondTheLimit_Fails()
    {
        static string Nested(int levels) => string.Concat(Enumerable.Repeat("{A x=", levels)) + "v" + new string('}', levels);

        Assert.True(MarkupExtensionParser.TryParse(Nested(10), out _));
        Assert.False(MarkupExtensionParser.TryParse(Nested(40), out _));
        Assert.False(MarkupExtensionParser.TryParse(Nested(5000), out _));
    }

    [Fact]
    public void TryParse_VeryLongInput_ReturnsFalse()
    {
        Assert.False(MarkupExtensionParser.TryParse("{Binding " + new string('a', 20_000) + "}", out _));
        Assert.True(MarkupExtensionParser.TryParse("{Binding " + new string('a', 5_000) + "}", out _));
    }

    [Fact]
    public void TryParse_ManyArguments_AreAllKept()
    {
        var args = string.Join(", ", Enumerable.Range(0, 200).Select(i => $"K{i}=V{i}"));

        var e = Parse("{Binding " + args + "}");

        Assert.Equal(200, e.Named.Count);
        Assert.Equal("V199", e.NamedText("K199"));
    }

    [Fact]
    public void TryParse_Garbage_NeverThrows()
    {
        var random = new Random(42);
        const string Alphabet = "{}[](),='\"\\ abcXx:.$!#";
        for (var i = 0; i < 3000; i++)
        {
            var length = random.Next(0, 40);
            var chars = Enumerable.Range(0, length).Select(_ => Alphabet[random.Next(Alphabet.Length)]).ToArray();

            var ex = Record.Exception(() => MarkupExtensionParser.TryParse(new string(chars), out _));

            Assert.Null(ex);
        }
    }
}
