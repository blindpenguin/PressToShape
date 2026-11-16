using System.Linq;
using System.Text;
using System.Xml.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class SvgExportGroupTests
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";
    private static readonly XNamespace Ink = "http://www.inkscape.org/namespaces/inkscape";

    private static XElement Layer(params Shape[] shapes)
    {
        var document = Document.Create("Layer 1");
        document = document.AddShapes(document.Layers[0].Id, shapes);
        var svg = XDocument.Parse(new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(SvgExport.ToUtf8(document)));
        return svg.Root!.Elements(Svg + "g").Single();
    }

    private static RectangleShape Rect(double x, double rotation = 0) => new() { X = x, Y = 0, Width = 10, Height = 10, Rotation = rotation };

    private static GroupShape Group(params Shape[] children) => new() { Children = [.. children] };

    [Fact]
    public void ToUtf8_Group_BecomesGWithChildrenInOrder()
    {
        var layer = Layer(Rect(0), Group(Rect(10), Rect(20)), Rect(30));

        var elements = layer.Elements().ToList();

        Assert.Equal(3, elements.Count);
        Assert.Equal(Svg + "rect", elements[0].Name);
        Assert.Equal(Svg + "g", elements[1].Name);
        Assert.Equal(["10", "20"], elements[1].Elements(Svg + "rect").Select(e => (string?)e.Attribute("x")));
        Assert.Equal(Svg + "rect", elements[2].Name);
    }

    [Fact]
    public void ToUtf8_NestedGroups_Nest()
    {
        var layer = Layer(Group(Group(Rect(0)), Rect(20)));

        var outer = layer.Elements(Svg + "g").Single();

        Assert.Single(outer.Elements(Svg + "g").Single().Elements(Svg + "rect"));
        Assert.Single(outer.Elements(Svg + "rect"));
    }

    [Fact]
    public void ToUtf8_NamedGroup_HasInkscapeLabel_UnnamedHasNone()
    {
        var layer = Layer(Group(Rect(0)) with { Name = "Wheels & <tyres>" }, Group(Rect(20)));

        var groups = layer.Elements(Svg + "g").ToList();

        Assert.Equal("Wheels & <tyres>", (string?)groups[0].Attribute(Ink + "label"));
        Assert.Null(groups[1].Attribute(Ink + "label"));
        Assert.Null(groups[0].Attribute("transform"));
    }

    [Fact]
    public void ToUtf8_HiddenLeaf_IsLeftOut()
    {
        var layer = Layer(Rect(0) with { IsVisible = false }, Rect(20));

        Assert.Equal("20", (string?)Assert.Single(layer.Elements()).Attribute("x"));
    }

    [Fact]
    public void ToUtf8_HiddenGroup_IsLeftOutWithItsChildren()
    {
        var layer = Layer(Group(Rect(0), Rect(10)) with { IsVisible = false }, Rect(40));

        Assert.Equal("40", (string?)Assert.Single(layer.Elements()).Attribute("x"));
    }

    [Fact]
    public void ToUtf8_HiddenChildInVisibleGroup_IsLeftOutOnly()
    {
        var layer = Layer(Group(Rect(0) with { IsVisible = false }, Rect(10)));

        var group = layer.Elements(Svg + "g").Single();

        Assert.Equal("10", (string?)Assert.Single(group.Elements()).Attribute("x"));
    }

    [Fact]
    public void ToUtf8_LockedShapes_AreExported()
    {
        var layer = Layer(Rect(0) with { IsLocked = true }, Group(Rect(20)) with { IsLocked = true });

        Assert.Equal(2, layer.Elements().Count());
    }

    [Fact]
    public void ToUtf8_RotatedChildInGroup_KeepsItsOwnTransform()
    {
        var layer = Layer(Group(Rect(0, rotation: 30)));

        var rect = layer.Elements(Svg + "g").Single().Elements(Svg + "rect").Single();

        Assert.Equal("rotate(30 5 5)", (string?)rect.Attribute("transform"));
    }

    [Fact]
    public void ToUtf8_GroupWithOnlyHiddenChildren_IsAnEmptyG()
    {
        var layer = Layer(Group(Rect(0) with { IsVisible = false }));

        Assert.Empty(layer.Elements(Svg + "g").Single().Elements());
    }
}
