using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class SelectionOverlayGroupTests
{
    private static readonly RectangleShape A = new() { X = 10, Y = 20, Width = 100, Height = 40 };
    private static readonly RectangleShape B = new() { X = 200, Y = 100, Width = 50, Height = 50, Rotation = 30 };
    private static readonly GroupShape Group = new() { Children = [A, B] };

    private static Document Doc()
    {
        var document = Document.Create("x");
        return document.AddShapes(document.Layers[0].Id, [Group]);
    }

    [Fact]
    public void SelectedGroup_HasOneUprightOutlineAndAnUprightFrameWithHandles()
    {
        var overlay = SelectionOverlay.Build(Doc(), [Group.Id], Tool.Select, 100, 0, 0);

        var outline = Assert.Single(overlay.Outlines);
        Assert.Equal(Group.X, outline.Points.Min(p => p.X), 6);
        Assert.Equal(Group.X + Group.Width, outline.Points.Max(p => p.X), 6);
        Assert.NotNull(overlay.Box);
        Assert.Equal(9, overlay.Handles.Count);
    }

    [Fact]
    public void HiddenSelectedGroup_IsNotPartOfTheOverlay()
    {
        var document = Doc().SetHidden(new[] { Group.Id }.ToHashSet(), true);

        var overlay = SelectionOverlay.Build(document, [Group.Id], Tool.Select, 100, 0, 0);

        Assert.Empty(overlay.Outlines);
    }

    [Fact]
    public void EnteredGroup_HasAnOutlineEvenWithoutSelection()
    {
        var overlay = SelectionOverlay.Build(Doc(), [], Tool.Select, 200, 50, 70, enteredGroupId: Group.Id);

        Assert.NotNull(overlay.EnteredGroup);
        Assert.Equal(50 + Group.X * 2, overlay.EnteredGroup.Points.Min(p => p.X), 6);
        Assert.Empty(overlay.Outlines);
    }

    [Fact]
    public void EnteredGroup_IsPartOfTheOverlayForOtherToolsToo()
    {
        var overlay = SelectionOverlay.Build(Doc(), [A.Id], Tool.Rectangle, 100, 0, 0, enteredGroupId: Group.Id);

        Assert.NotNull(overlay.EnteredGroup);
        Assert.Single(overlay.Outlines);
    }

    [Fact]
    public void NoEnteredGroupOrUnknownId_HasNoEnteredOutline()
    {
        Assert.Null(SelectionOverlay.Build(Doc(), [Group.Id], Tool.Select, 100, 0, 0).EnteredGroup);
        Assert.Null(SelectionOverlay.Build(Doc(), [], Tool.Select, 100, 0, 0, enteredGroupId: Guid.NewGuid()).EnteredGroup);
        Assert.Null(SelectionOverlay.Build(Doc(), [], Tool.Select, 100, 0, 0, enteredGroupId: A.Id).EnteredGroup);
    }
}
