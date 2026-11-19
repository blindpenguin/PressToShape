using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.IO;

public class LayerRoundTripTests
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";
    private static readonly XNamespace Ink = "http://www.inkscape.org/namespaces/inkscape";

    private static RectangleShape Rect(double x) => new() { X = x, Y = 0, Width = 10, Height = 10 };

    // Three layers, bottom first: "Sky" (hidden, locked), "Hills" (with a nested group) and "Sun" (empty). They are made in this
    // order and then moved, so the stored order differs from the order in which the layers were created.
    private static Document Build()
    {
        var group = new GroupShape { Name = "Trees", Children = [Rect(10), new GroupShape { Children = [Rect(20), Rect(30)] }] };
        var document = Document.Create("Sun")
            .AddLayer(new Layer { Name = "Hills", Shapes = [Rect(0), group] })
            .AddLayer(new Layer { Name = "Sky", IsVisible = false, IsLocked = true, Shapes = [Rect(40)] });
        return document.MoveLayerToIndex(document.Layers[2].Id, 0);
    }

    private static Document RoundTrip(Document document)
    {
        var result = ProjectFile.Read(ProjectFile.Write(document));
        Assert.True(result.Succeeded, result.Detail);
        return result.Document!;
    }

    private static XElement[] ExportedLayers(Document document)
    {
        var svg = XDocument.Parse(new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(SvgExport.ToUtf8(document, new FakeTextMeasurer())));
        return [.. svg.Root!.Elements(Svg + "g")];
    }

    [Fact]
    public void WriteThenRead_LayersInChangedOrder_KeepCountOrderNamesAndFlags()
    {
        var document = Build();

        var loaded = RoundTrip(document);

        Assert.Equal(["Sky", "Sun", "Hills"], loaded.Layers.Select(l => l.Name));
        Assert.Equal(document.Layers.Select(l => l.Id), loaded.Layers.Select(l => l.Id));
        Assert.Equal([false, true, true], loaded.Layers.Select(l => l.IsVisible));
        Assert.Equal([true, false, false], loaded.Layers.Select(l => l.IsLocked));
    }

    [Fact]
    public void WriteThenRead_LayersWithShapesAndGroups_KeepContents()
    {
        var document = Build();

        var loaded = RoundTrip(document);

        var hills = loaded.Layers[2];
        Assert.Equal(document.Layers[2].Shapes.Select(s => s.Id), hills.Shapes.Select(s => s.Id));
        var trees = Assert.IsType<GroupShape>(hills.Shapes[1]);
        Assert.Equal("Trees", trees.Name);
        Assert.IsType<GroupShape>(trees.Children[1]);
        Assert.Empty(loaded.Layers[1].Shapes);
        Assert.Single(loaded.Layers[0].Shapes);
    }

    [Fact]
    public void WriteThenRead_AfterLayerCommands_KeepsTheResult()
    {
        var document = Build();
        var duplicated = document.DuplicateLayer(document.Layers[2].Id, Guid.NewGuid(), "Hills copy", Guid.NewGuid);
        var merged = duplicated.MergeLayerDown(duplicated.Layers[2].Id);

        var loaded = RoundTrip(merged);

        Assert.Equal(merged.Layers.Select(l => l.Name), loaded.Layers.Select(l => l.Name));
        Assert.Equal(merged.Layers.Select(l => l.Shapes.Length), loaded.Layers.Select(l => l.Shapes.Length));
    }

    [Fact]
    public void ToUtf8_LayersInChangedOrder_ExportsVisibleLayersInStackingOrder()
    {
        var layers = ExportedLayers(Build());

        Assert.Equal(["Sun", "Hills"], layers.Select(l => (string?)l.Attribute(Ink + "label")));
        Assert.Empty(layers[0].Elements());
        Assert.Equal(2, layers[1].Elements().Count());
    }

    [Fact]
    public void ToUtf8_AfterMoveLayerAbove_FollowsTheNewOrder()
    {
        var document = Build();
        var moved = document.MoveLayerAbove(document.Layers[0].Id, document.Layers[2].Id);

        var layers = ExportedLayers(moved);

        Assert.Equal(["Sun", "Hills"], layers.Select(l => (string?)l.Attribute(Ink + "label")));
        Assert.Equal(["Sun", "Hills", "Sky"], moved.Layers.Select(l => l.Name));
    }
}
