using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class SnapperExtrasTests
{
    private static readonly SnapSettings ObjectsOnly = new(false, true, 10);
    private static readonly SnapSettings GridOnly = new(true, false, 10);
    private static readonly SnapSettings Off = new(false, false, 10);

    private static readonly SmartMarker Marker = new(SmartMarkerType.EqualGap, SnapAxis.X, 0, 10, 5, 10);

    private static SnapTargets Targets(Document document, bool includeGuides = true) =>
        SnapTargets.Collect(document, new HashSet<Guid>(), includeGuides);

    private static Document BoxDocument(double x, double y)
    {
        var document = Document.Create("L");
        return document.AddShape(document.Layers[0].Id, new RectangleShape { X = x, Y = y, Width = 10, Height = 10 });
    }

    private static SnapCandidate Extra(SnapAxis axis, double offset) => new(axis, offset, [Marker]);

    [Fact]
    public void Extra_BeatsAFartherEdge_AndCarriesItsMarkers()
    {
        // The edge target at x 105 is 5 away from 100, the extra is 2 away.
        var targets = Targets(BoxDocument(105, 5000));

        var result = Snapper.Snap([100], [], targets, ObjectsOnly, 100, extras: [Extra(SnapAxis.X, 2)]);

        Assert.Equal(2, result.OffsetX, 9);
        Assert.Equal([Marker], result.Markers);
        Assert.Empty(result.Guides);
    }

    [Fact]
    public void Edge_BeatsAFartherExtra()
    {
        var targets = Targets(BoxDocument(103, 5000));

        var result = Snapper.Snap([100], [], targets, ObjectsOnly, 100, extras: [Extra(SnapAxis.X, 5)]);

        Assert.Equal(3, result.OffsetX, 9);
        Assert.Empty(result.Markers);
        Assert.NotEmpty(result.Guides);
    }

    [Fact]
    public void Extra_BeatsAGridLine_EvenWhenTheGridLineIsNearer()
    {
        var result = Snapper.Snap([101], [], null, GridOnly, 100, extras: [Extra(SnapAxis.X, 4)]);

        Assert.Equal(4, result.OffsetX, 9);
    }

    [Fact]
    public void Extra_WorksWithSnapToObjectsOff()
    {
        var result = Snapper.Snap([100], [], null, Off, 100, extras: [Extra(SnapAxis.X, 3)]);

        Assert.Equal(3, result.OffsetX, 9);
    }

    [Fact]
    public void Extra_ActsOnItsOwnAxisOnly_AndOutsideToleranceOrNaNIsIgnored()
    {
        var result = Snapper.Snap([100], [], null, Off, 100, extras: [Extra(SnapAxis.Y, 3)]);
        Assert.Equal((0.0, 3.0), (result.OffsetX, result.OffsetY));
        Assert.Same(SnapResult.None, Snapper.Snap([100], [], null, Off, 100, extras: [Extra(SnapAxis.X, 9)]));
        Assert.Same(SnapResult.None, Snapper.Snap([100], [], null, Off, 100, extras: [Extra(SnapAxis.X, double.NaN)]));
    }

    [Fact]
    public void Extra_TheSmallestOfSeveral_Wins()
    {
        var result = Snapper.Snap([100], [], null, Off, 100, extras: [Extra(SnapAxis.X, 5), Extra(SnapAxis.X, -2), Extra(SnapAxis.X, 4)]);

        Assert.Equal(-2, result.OffsetX, 9);
    }

    [Fact]
    public void Extra_ToleranceScalesWithZoom()
    {
        Assert.Same(SnapResult.None, Snapper.Snap([100], [], null, Off, 400, extras: [Extra(SnapAxis.X, 3)]));
        Assert.Equal(3, Snapper.Snap([100], [], null, Off, 200, extras: [Extra(SnapAxis.X, 3)]).OffsetX, 9);
    }

    [Fact]
    public void Extra_AxesAreIndependent()
    {
        var result = Snapper.Snap([100], [200], null, Off, 100, extras: [Extra(SnapAxis.X, 3), Extra(SnapAxis.Y, -4)]);

        Assert.Equal((3.0, -4.0), (result.OffsetX, result.OffsetY));
    }

    [Fact]
    public void Guide_IsATarget_ItsIdIsActive_AndItGetsNoGuideLine()
    {
        var guide = new Guide { Orientation = GuideOrientation.Vertical, Position = 105 };
        var document = Document.Create("L").AddGuide(guide);

        var result = Snapper.Snap([100], [], Targets(document), ObjectsOnly, 100);

        Assert.Equal(5, result.OffsetX, 9);
        Assert.Equal([guide.Id], result.ActiveGuideIds);
        Assert.Empty(result.Guides);
    }

    [Fact]
    public void Guide_Horizontal_SnapsOnTheYAxis()
    {
        var guide = new Guide { Orientation = GuideOrientation.Horizontal, Position = 52 };
        var document = Document.Create("L").AddGuide(guide);

        var result = Snapper.Snap([], [50], Targets(document), ObjectsOnly, 100);

        Assert.Equal(2, result.OffsetY, 9);
        Assert.Equal([guide.Id], result.ActiveGuideIds);
    }

    [Fact]
    public void Guide_WinsATieAgainstAnObjectEdge()
    {
        var guide = new Guide { Orientation = GuideOrientation.Vertical, Position = 105 };
        var document = BoxDocument(105, 5000).AddGuide(guide);

        var result = Snapper.Snap([100], [], Targets(document), ObjectsOnly, 100);

        Assert.Equal([guide.Id], result.ActiveGuideIds);
        Assert.Equal(5, result.OffsetX, 9);
    }

    [Fact]
    public void Guide_NearerThanAnEdge_Wins_AndFartherLoses()
    {
        var guide = new Guide { Orientation = GuideOrientation.Vertical, Position = 107 };
        var near = BoxDocument(103, 5000).AddGuide(guide);

        var result = Snapper.Snap([100], [], Targets(near), ObjectsOnly, 100);

        Assert.Equal(3, result.OffsetX, 9);
        Assert.Empty(result.ActiveGuideIds);
    }

    [Fact]
    public void Guides_NotIncluded_AreNoTargets()
    {
        var document = Document.Create("L").AddGuide(new Guide { Orientation = GuideOrientation.Vertical, Position = 105 });

        var result = Snapper.Snap([100], [], Targets(document, includeGuides: false), ObjectsOnly, 100);

        Assert.Same(SnapResult.None, result);
    }

    [Fact]
    public void Guide_WithSnapToObjectsOff_DoesNotSnap()
    {
        var document = Document.Create("L").AddGuide(new Guide { Orientation = GuideOrientation.Vertical, Position = 105 });

        Assert.Same(SnapResult.None, Snapper.Snap([100], [], Targets(document), Off, 100));
    }

    [Fact]
    public void Guide_BeatsAGridLine()
    {
        var guide = new Guide { Orientation = GuideOrientation.Vertical, Position = 105 };
        var document = Document.Create("L").AddGuide(guide);

        var result = Snapper.Snap([101], [], Targets(document), new SnapSettings(true, true, 10), 100);

        Assert.Equal([guide.Id], result.ActiveGuideIds);
        Assert.Equal(4, result.OffsetX, 9);
    }

    [Fact]
    public void WithoutExtras_ResultsAreAsBefore()
    {
        var targets = Targets(BoxDocument(105, 5000), includeGuides: false);

        var result = Snapper.Snap([100], [], targets, ObjectsOnly, 100);

        Assert.Equal(5, result.OffsetX, 9);
        Assert.Empty(result.Markers);
        Assert.Empty(result.ActiveGuideIds);
        Assert.NotEmpty(result.Guides);
    }

    [Fact]
    public void Collect_GuidesGoToTheAxisOfTheirOrientation()
    {
        var document = Document.Create("L")
            .AddGuide(new Guide { Orientation = GuideOrientation.Vertical, Position = 11 })
            .AddGuide(new Guide { Orientation = GuideOrientation.Horizontal, Position = 22 });

        var targets = SnapTargets.Collect(document, new HashSet<Guid>(), includeGuides: true);

        Assert.Contains(targets.X, t => t.Type == SnapTargetType.Guide && t.Value == 11 && t.Id is not null);
        Assert.Contains(targets.Y, t => t.Type == SnapTargetType.Guide && t.Value == 22);
        Assert.DoesNotContain(targets.X, t => t.Type == SnapTargetType.Guide && t.Value == 22);
    }

    [Fact]
    public void TryFindNearest_AGuideAtTheSameValueAsAnEdge_IsReturned()
    {
        var document = BoxDocument(105, 5000).AddGuide(new Guide { Orientation = GuideOrientation.Vertical, Position = 105 });

        Assert.True(Targets(document).TryFindNearest(SnapAxis.X, 104, 5, out var nearest));

        Assert.Equal(SnapTargetType.Guide, nearest.Type);
    }
}
