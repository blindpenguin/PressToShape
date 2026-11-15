using System;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class SelectionGeometryTests
{
    private static SelectionFrame Frame(double rotation = 0, double width = 100, double height = 60) => new(200, 100, width, height, rotation, true);

    private static void AssertPoint((double X, double Y) actual, double x, double y)
    {
        Assert.Equal(x, actual.X, 6);
        Assert.Equal(y, actual.Y, 6);
    }

    [Fact]
    public void FrameOf_NoShapes_IsNull()
    {
        Assert.Null(SelectionGeometry.FrameOf([]));
    }

    [Fact]
    public void FrameOf_OneShape_IsItsOwnBoxAndRotation()
    {
        var frame = SelectionGeometry.FrameOf([new RectangleShape { X = 10, Y = 20, Width = 100, Height = 40, Rotation = 30 }]);

        Assert.NotNull(frame);
        Assert.Equal(new SelectionFrame(60, 40, 100, 40, 30, true), frame);
    }

    [Fact]
    public void FrameOf_SeveralShapes_IsTheUprightUnionOfTheRotatedBounds()
    {
        var frame = SelectionGeometry.FrameOf(
        [
            new RectangleShape { X = 0, Y = 0, Width = 10, Height = 10 },
            new RectangleShape { X = 100, Y = 50, Width = 100, Height = 20, Rotation = 90 }, // turned: x 140..160, y 10..110
        ]);

        Assert.NotNull(frame);
        Assert.False(frame.IsSingle);
        Assert.Equal(0, frame.Rotation);
        Assert.Equal(160, frame.Width, 6);
        Assert.Equal(110, frame.Height, 6);
        Assert.Equal(80, frame.CenterX, 6);
        Assert.Equal(55, frame.CenterY, 6);
    }

    [Fact]
    public void HandlePosition_Upright_AtZoom100()
    {
        var expected = new (HandleType Handle, double X, double Y)[]
        {
            (HandleType.TopLeft, 150, 70), (HandleType.Top, 200, 70), (HandleType.TopRight, 250, 70), (HandleType.Right, 250, 100),
            (HandleType.BottomRight, 250, 130), (HandleType.Bottom, 200, 130), (HandleType.BottomLeft, 150, 130), (HandleType.Left, 150, 100),
            (HandleType.Rotate, 200, 46),
        };

        foreach (var (handle, x, y) in expected)
        {
            AssertPoint(SelectionGeometry.HandlePosition(Frame(), handle, 100), x, y);
        }
    }

    [Fact]
    public void HandlePosition_RotateHandleDistanceIsConstantOnTheScreen()
    {
        foreach (var zoom in new[] { 25.0, 100, 400 })
        {
            var (_, y) = SelectionGeometry.HandlePosition(Frame(), HandleType.Rotate, zoom);
            Assert.Equal(70 - 24 / (zoom / 100), y, 6);
        }
    }

    [Fact]
    public void HandlePosition_Rotated90_TurnsClockwise()
    {
        // The top handle of a frame turned 90 degrees clockwise is on the right.
        AssertPoint(SelectionGeometry.HandlePosition(Frame(90), HandleType.Top, 100), 230, 100);
        AssertPoint(SelectionGeometry.HandlePosition(Frame(90), HandleType.Rotate, 100), 254, 100);
        AssertPoint(SelectionGeometry.HandlePosition(Frame(90), HandleType.TopLeft, 100), 230, 50);
    }

    [Fact]
    public void HandlePosition_Rotated30_StaysOnTheTurnedFrame()
    {
        var (x, y) = SelectionGeometry.HandlePosition(Frame(30), HandleType.Right, 100);

        Assert.Equal(200 + 50 * Math.Cos(Math.PI / 6), x, 6);
        Assert.Equal(100 + 50 * Math.Sin(Math.PI / 6), y, 6);
    }

    [Theory]
    [InlineData(25)]
    [InlineData(100)]
    [InlineData(400)]
    public void HitHandle_UsesAGrabSizeThatIsConstantOnTheScreen(double zoom)
    {
        var frame = Frame(width: 1000, height: 600);
        var (hx, hy) = SelectionGeometry.HandlePosition(frame, HandleType.BottomRight, zoom);
        var inside = 4.9 / (zoom / 100);
        var outside = 5.1 / (zoom / 100);

        Assert.Equal(HandleType.BottomRight, SelectionGeometry.HitHandle(frame, hx + inside, hy - inside, zoom));
        Assert.Null(SelectionGeometry.HitHandle(frame, hx + outside, hy, zoom));
        Assert.Null(SelectionGeometry.HitHandle(frame, hx, hy + outside, zoom));
    }

    [Fact]
    public void HitHandle_AwayFromAllHandles_IsNull()
    {
        Assert.Null(SelectionGeometry.HitHandle(Frame(), 200, 100, 100));
        Assert.Null(SelectionGeometry.HitHandle(Frame(), 0, 0, 100));
    }

    [Fact]
    public void HitHandle_RotateHandleIsOutsideTheFrame()
    {
        Assert.Equal(HandleType.Rotate, SelectionGeometry.HitHandle(Frame(), 200, 46, 100));
        Assert.Equal(HandleType.Rotate, SelectionGeometry.HitHandle(Frame(), 203, 49, 100));
    }

    [Fact]
    public void HitHandle_RotatedFrame_FollowsTheTurnedHandles()
    {
        var frame = Frame(90);

        Assert.Equal(HandleType.Top, SelectionGeometry.HitHandle(frame, 230, 100, 100));
        Assert.Null(SelectionGeometry.HitHandle(frame, 200, 70, 100)); // where Top would be unturned
    }

    [Fact]
    public void HitHandle_EveryHandleIsReachableByItsOwnCenter_EvenOnATinyFrame()
    {
        foreach (var rotation in new[] { 0.0, 30, 90 })
        {
            var frame = new SelectionFrame(50, 50, 2, 2, rotation, true);
            foreach (var handle in Enum.GetValues<HandleType>())
            {
                var (x, y) = SelectionGeometry.HandlePosition(frame, handle, 100);

                Assert.Equal(handle, SelectionGeometry.HitHandle(frame, x, y, 100));
            }
        }
    }

    [Fact]
    public void HitHandle_EqualDistance_PrefersCornersOverEdges()
    {
        // A frame of width 0 puts TopLeft and TopRight on the same point as Top: the corner wins.
        var frame = new SelectionFrame(50, 50, 0, 40, 0, true);

        var hit = SelectionGeometry.HitHandle(frame, 50, 30, 100);

        Assert.Equal(HandleType.TopLeft, hit);
    }

    [Fact]
    public void HandleDirectionDegrees_IsTheTurnedDirection()
    {
        var cases = new (HandleType Handle, double Rotation, double Expected)[]
        {
            (HandleType.Right, 0, 0), (HandleType.BottomRight, 0, 45), (HandleType.Bottom, 0, 90), (HandleType.Left, 0, 180),
            (HandleType.Top, 0, 270), (HandleType.Rotate, 0, 270), (HandleType.Top, 90, 0), (HandleType.Right, 350, 350), (HandleType.Left, 270, 90),
        };

        foreach (var (handle, rotation, expected) in cases)
        {
            Assert.Equal(expected, SelectionGeometry.HandleDirectionDegrees(handle, rotation), 6);
        }
    }

    [Fact]
    public void NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => SelectionGeometry.FrameOf(null!));
        Assert.Throws<ArgumentNullException>(() => SelectionGeometry.HandlePosition(null!, HandleType.Top, 100));
        Assert.Throws<ArgumentNullException>(() => SelectionGeometry.HitHandle(null!, 0, 0, 100));
    }
}
