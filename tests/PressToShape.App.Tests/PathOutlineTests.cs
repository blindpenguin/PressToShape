using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class PathOutlineTests
{
    private static ScreenPath Path(bool closed, params ScreenPathNode[] nodes) => new([new ScreenSubPath([.. nodes], closed)], FillRule.NonZero);

    [Fact]
    public void Commands_OpenLines_MoveThenLinesAndNoClose()
    {
        var path = Path(false, new ScreenPathNode(0, 0), new ScreenPathNode(10, 0), new ScreenPathNode(10, 10));

        var commands = PathOutline.Commands(path, 0, 0);

        Assert.Equal(
            [PathCommandType.Move, PathCommandType.Line, PathCommandType.Line],
            commands.Select(c => c.Type));
        Assert.Equal(10, commands[2].X);
        Assert.Equal(10, commands[2].Y);
    }

    [Fact]
    public void Commands_ClosedLines_EndsWithCloseAndNoRedundantClosingLine()
    {
        var path = Path(true, new ScreenPathNode(0, 0), new ScreenPathNode(10, 0), new ScreenPathNode(10, 10));

        var commands = PathOutline.Commands(path, 0, 0);

        // Close draws the line from the last node back to the first.
        Assert.Equal(
            [PathCommandType.Move, PathCommandType.Line, PathCommandType.Line, PathCommandType.Close],
            commands.Select(c => c.Type));
    }

    [Fact]
    public void Commands_SegmentWithHandles_IsACubicWithTheHandlePoints()
    {
        var path = Path(false, new ScreenPathNode(0, 0, OutX: 0, OutY: 5), new ScreenPathNode(10, 10, InX: 5, InY: 10));

        var commands = PathOutline.Commands(path, 0, 0);

        Assert.Equal(PathCommandType.Cubic, commands[1].Type);
        Assert.Equal((0, 5), (commands[1].Control1X, commands[1].Control1Y));
        Assert.Equal((5, 10), (commands[1].Control2X, commands[1].Control2Y));
        Assert.Equal((10, 10), (commands[1].X, commands[1].Y));
    }

    [Fact]
    public void Commands_OnlyOneHandle_UsesTheAnchorForTheMissingOne()
    {
        var path = Path(false, new ScreenPathNode(0, 0, OutX: 0, OutY: 8), new ScreenPathNode(10, 10));

        var cubic = PathOutline.Commands(path, 0, 0)[1];

        Assert.Equal(PathCommandType.Cubic, cubic.Type);
        Assert.Equal((10, 10), (cubic.Control2X, cubic.Control2Y));
    }

    [Fact]
    public void Commands_ClosedPathWithACurvedClosingSegment_WritesTheCurveBeforeClose()
    {
        var path = Path(true, new ScreenPathNode(0, 0), new ScreenPathNode(10, 0), new ScreenPathNode(10, 10, OutX: 0, OutY: 10));

        var commands = PathOutline.Commands(path, 0, 0);

        Assert.Equal(
            [PathCommandType.Move, PathCommandType.Line, PathCommandType.Line, PathCommandType.Cubic, PathCommandType.Close],
            commands.Select(c => c.Type));
        Assert.Equal((0, 0), (commands[3].X, commands[3].Y));
        Assert.Equal((0, 10), (commands[3].Control1X, commands[3].Control1Y));
    }

    [Fact]
    public void Commands_Origin_MovesEveryPoint()
    {
        var path = Path(false, new ScreenPathNode(1, 2, OutX: 3, OutY: 4), new ScreenPathNode(5, 6, InX: 7, InY: 8));

        var commands = PathOutline.Commands(path, 100, 200);

        Assert.Equal((101, 202), (commands[0].X, commands[0].Y));
        Assert.Equal((105, 206), (commands[1].X, commands[1].Y));
        Assert.Equal((103, 204), (commands[1].Control1X, commands[1].Control1Y));
        Assert.Equal((107, 208), (commands[1].Control2X, commands[1].Control2Y));
    }

    [Fact]
    public void Commands_NullPath_Throws()
    {
        Assert.Throws<System.ArgumentNullException>(() => PathOutline.Commands(null!, 0, 0));
    }

    [Fact]
    public void ScreenPathNode_HasInAndHasOut_NeedBothNumbers()
    {
        Assert.False(new ScreenPathNode(0, 0).HasIn);
        Assert.True(new ScreenPathNode(0, 0, InX: 1, InY: 1).HasIn);
        Assert.True(new ScreenPathNode(0, 0, OutX: 1, OutY: 1).HasOut);
        Assert.False(new ScreenPathNode(0, 0, OutX: 1).HasOut);
    }
}
