using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class SelectionTransformsTests
{
    private static readonly HandleType[] ResizeHandles = Enum.GetValues<HandleType>().Where(h => h != HandleType.Rotate).ToArray();

    private static RectangleShape Rect(double x = 100, double y = 100, double w = 200, double h = 100, double rotation = 0) =>
        new() { X = x, Y = y, Width = w, Height = h, Rotation = rotation, Appearance = Appearance.Default with { StrokeWidth = 4, Opacity = 0.5 } };

    private static (IReadOnlyList<Shape> Shapes, SelectionFrame Frame) Single(double rotation = 0)
    {
        IReadOnlyList<Shape> shapes = [Rect(rotation: rotation)];
        return (shapes, SelectionGeometry.FrameOf(shapes)!);
    }

    private static (double X, double Y) Local(SelectionFrame frame, double x, double y)
    {
        var radians = -frame.Rotation * Math.PI / 180;
        var dx = x - frame.CenterX;
        var dy = y - frame.CenterY;
        return (dx * Math.Cos(radians) - dy * Math.Sin(radians), dx * Math.Sin(radians) + dy * Math.Cos(radians));
    }

    private static (double X, double Y) Corner(Shape shape, int dx, int dy)
    {
        var frame = SelectionGeometry.FrameOf([shape])!;
        var radians = frame.Rotation * Math.PI / 180;
        var lx = dx * shape.Width / 2;
        var ly = dy * shape.Height / 2;
        return (shape.CenterX + lx * Math.Cos(radians) - ly * Math.Sin(radians), shape.CenterY + lx * Math.Sin(radians) + ly * Math.Cos(radians));
    }

    private static void Near((double X, double Y) actual, (double X, double Y) expected)
    {
        Assert.Equal(expected.X, actual.X, 6);
        Assert.Equal(expected.Y, actual.Y, 6);
    }

    // ---- resize, single shape ----

    [Fact]
    public void Resize_Upright_EveryHandleKeepsTheOppositePointAndFollowsThePointer()
    {
        foreach (var handle in ResizeHandles)
        {
            var (shapes, frame) = Single();
            var (dx, dy) = SelectionGeometry.Direction(handle);
            var (hx, hy) = SelectionGeometry.HandlePosition(frame, handle, 100);

            var result = SelectionTransforms.Resize(shapes, frame, handle, hx + dx * 30, hy + dy * 20, false, false)[0];

            Near(Corner(result, -dx, -dy), Corner(shapes[0], -dx, -dy));
            Assert.Equal(200 + (dx != 0 ? 30 : 0), result.Width, 6);
            Assert.Equal(100 + (dy != 0 ? 20 : 0), result.Height, 6);
        }
    }

    [Theory]
    [InlineData(30)]
    [InlineData(90)]
    [InlineData(200)]
    public void Resize_Rotated_OppositePointStaysFixedAndSizeFollowsThePointerInTheTurnedFrame(double rotation)
    {
        foreach (var handle in ResizeHandles)
        {
            var (shapes, frame) = Single(rotation);
            var (dx, dy) = SelectionGeometry.Direction(handle);
            var (hx, hy) = SelectionGeometry.HandlePosition(frame, handle, 100);
            var radians = rotation * Math.PI / 180;
            // Drag 25 units outward along the turned x axis and 15 along the turned y axis.
            var px = hx + 25 * dx * Math.Cos(radians) - 15 * dy * Math.Sin(radians);
            var py = hy + 25 * dx * Math.Sin(radians) + 15 * dy * Math.Cos(radians);

            var result = SelectionTransforms.Resize(shapes, frame, handle, px, py, false, false)[0];

            Near(Corner(result, -dx, -dy), Corner(shapes[0], -dx, -dy));
            Assert.Equal(200 + (dx != 0 ? 25 : 0), result.Width, 6);
            Assert.Equal(100 + (dy != 0 ? 15 : 0), result.Height, 6);
            Assert.Equal(shapes[0].Rotation, result.Rotation, 6);
        }
    }

    [Fact]
    public void Resize_KeepProportions_CornerKeepsTheRatio()
    {
        var (shapes, frame) = Single();

        var result = SelectionTransforms.Resize(shapes, frame, HandleType.BottomRight, 400, 250, keepProportions: true, fromCenter: false)[0];

        Assert.Equal(2, result.Width / result.Height, 6);
        Assert.Equal(100, result.X, 6);
        Assert.Equal(100, result.Y, 6);
        Assert.Equal(300, result.Width, 6); // the larger of the two scales (1.5 and 1.5 -> 300 x 150)
    }

    [Fact]
    public void Resize_KeepProportions_EdgeScalesTheOtherSideAroundItsCenter()
    {
        var (shapes, frame) = Single();

        var result = SelectionTransforms.Resize(shapes, frame, HandleType.Right, 400, 500, keepProportions: true, fromCenter: false)[0];

        Assert.Equal(300, result.Width, 6);
        Assert.Equal(150, result.Height, 6);
        Assert.Equal(100, result.X, 6);
        Assert.Equal(75, result.Y, 6);
    }

    [Fact]
    public void Resize_FromCenter_KeepsTheCenter()
    {
        var (shapes, frame) = Single();

        var result = SelectionTransforms.Resize(shapes, frame, HandleType.BottomRight, 350, 200, false, fromCenter: true)[0];

        Assert.Equal(frame.CenterX, result.CenterX, 6);
        Assert.Equal(frame.CenterY, result.CenterY, 6);
        Assert.Equal(300, result.Width, 6);
        Assert.Equal(100, result.Height, 6);
    }

    [Fact]
    public void Resize_PastTheOppositeEdge_Flips()
    {
        var (shapes, frame) = Single();

        var result = SelectionTransforms.Resize(shapes, frame, HandleType.Right, 50, 150, false, false)[0];

        Assert.Equal(50, result.X, 6);
        Assert.Equal(50, result.Width, 6);
        Assert.Equal(100, result.Y, 6);
        Assert.Equal(100, result.Height, 6);
    }

    [Fact]
    public void Resize_ToTheOppositeEdge_GivesZeroSize_NotNegative()
    {
        var (shapes, frame) = Single();

        var result = SelectionTransforms.Resize(shapes, frame, HandleType.Right, 100, 150, false, false)[0];

        Assert.Equal(0, result.Width);
        Assert.Equal(100, result.X, 6);
    }

    [Fact]
    public void Resize_LeavesStrokeAndOpacityAlone()
    {
        var (shapes, frame) = Single(30);

        var result = SelectionTransforms.Resize(shapes, frame, HandleType.TopLeft, 0, 0, false, false)[0];

        Assert.Equal(4, result.Appearance.StrokeWidth);
        Assert.Equal(0.5, result.Appearance.Opacity);
        Assert.Equal(shapes[0].Id, result.Id);
    }

    [Fact]
    public void Resize_RotateHandleOrNonFiniteInput_ReturnsTheSameList()
    {
        var (shapes, frame) = Single();

        Assert.Same(shapes, SelectionTransforms.Resize(shapes, frame, HandleType.Rotate, 1, 1, false, false));
        Assert.Same(shapes, SelectionTransforms.Resize(shapes, frame, HandleType.Right, double.NaN, 1, false, false));
        Assert.Same(shapes, SelectionTransforms.Resize(shapes, frame, HandleType.Right, 1, double.PositiveInfinity, false, false));
        IReadOnlyList<Shape> huge = [Rect(x: double.MaxValue)];
        Assert.Same(huge, SelectionTransforms.Move(huge, double.MaxValue, 0)); // overflows to infinity
    }

    [Fact]
    public void Resize_DoesNotChangeTheInput()
    {
        var (shapes, frame) = Single();
        var before = shapes[0];

        SelectionTransforms.Resize(shapes, frame, HandleType.Right, 500, 500, false, false);

        Assert.Equal(before, shapes[0]);
    }

    // ---- resize, group ----

    private static (IReadOnlyList<Shape> Shapes, SelectionFrame Frame) Group(double rotationOfSecond = 0)
    {
        IReadOnlyList<Shape> shapes =
        [
            Rect(0, 0, 100, 100),
            Rect(200, 100, 100, 100, rotation: rotationOfSecond),
        ];
        return (shapes, SelectionGeometry.FrameOf(shapes)!);
    }

    [Fact]
    public void Resize_Group_ScalesPositionsAndSizesTogether_AndKeepsStrokes()
    {
        var (shapes, frame) = Group(); // frame 0..300 x 0..200

        var result = SelectionTransforms.Resize(shapes, frame, HandleType.BottomRight, 600, 200, false, false);

        Assert.Equal((0, 0, 200, 100), (result[0].X, result[0].Y, result[0].Width, result[0].Height));
        Assert.Equal((400, 100, 200, 100), (result[1].X, result[1].Y, result[1].Width, result[1].Height));
        Assert.All(result, s => Assert.Equal(4, s.Appearance.StrokeWidth));
        Assert.Equal(shapes.Select(s => s.Id), result.Select(s => s.Id));
    }

    [Fact]
    public void Resize_Group_FromCenter_StaysCentered()
    {
        var (shapes, frame) = Group();

        var result = SelectionTransforms.Resize(shapes, frame, HandleType.Right, 450, 0, false, fromCenter: true);
        var after = SelectionGeometry.FrameOf(result)!;

        Assert.Equal(frame.CenterX, after.CenterX, 6);
        Assert.Equal(frame.CenterY, after.CenterY, 6);
        Assert.Equal(600, after.Width, 6);
    }

    [Fact]
    public void Resize_Group_FlipsPastTheOppositeEdge()
    {
        var (shapes, frame) = Group();

        var result = SelectionTransforms.Resize(shapes, frame, HandleType.Right, -300, 100, false, false);

        Assert.Equal(-100, result[0].X, 6);
        Assert.Equal(100, result[0].Width, 6);
        Assert.Equal(-300, result[1].X, 6);
        Assert.Equal(100, result[1].Width, 6);
    }

    [Fact]
    public void Resize_GroupWithARotatedShape_ScalesUniformly()
    {
        var (shapes, frame) = Group(rotationOfSecond: 30);

        var result = SelectionTransforms.Resize(shapes, frame, HandleType.Right, frame.CenterX + frame.Width, 0, false, false);

        // The Right edge handle only widens by the factor; the height follows by the same factor.
        var factor = result[0].Width / shapes[0].Width;
        Assert.Equal(factor, result[0].Height / shapes[0].Height, 6);
        Assert.Equal(factor, result[1].Width / shapes[1].Width, 6);
        Assert.Equal(factor, result[1].Height / shapes[1].Height, 6);
        Assert.Equal(30, result[1].Rotation, 6);
    }

    // ---- rotate ----

    [Fact]
    public void Rotate_Single_TheHandleFollowsThePointer()
    {
        var (shapes, frame) = Single();
        var (sx, sy) = SelectionGeometry.HandlePosition(frame, HandleType.Rotate, 100);

        // Pointer straight to the right of the center: the top handle is now on the right: 90 degrees.
        var result = SelectionTransforms.Rotate(shapes, frame, sx, sy, frame.CenterX + 500, frame.CenterY, snap: false)[0];

        Assert.Equal(90, result.Rotation, 6);
        Assert.Equal(frame.CenterX, result.CenterX, 6);
        Assert.Equal(frame.CenterY, result.CenterY, 6);
    }

    [Fact]
    public void Rotate_Single_FromANonZeroStartAngle_AddsTheMovement()
    {
        var (shapes, frame) = Single(40);
        var (sx, sy) = SelectionGeometry.HandlePosition(frame, HandleType.Rotate, 100);

        var result = SelectionTransforms.Rotate(shapes, frame, sx, sy, frame.CenterX, frame.CenterY + 100, snap: false)[0];

        Assert.Equal(180, result.Rotation, 6);
    }

    [Fact]
    public void Rotate_Single_SnapsToFifteenDegrees()
    {
        var (shapes, frame) = Single();
        var (sx, sy) = SelectionGeometry.HandlePosition(frame, HandleType.Rotate, 100);
        var radians = 37 * Math.PI / 180;
        var px = frame.CenterX + 100 * Math.Sin(radians);
        var py = frame.CenterY - 100 * Math.Cos(radians);

        var free = SelectionTransforms.Rotate(shapes, frame, sx, sy, px, py, snap: false)[0];
        var snapped = SelectionTransforms.Rotate(shapes, frame, sx, sy, px, py, snap: true)[0];

        Assert.Equal(37, free.Rotation, 6);
        Assert.Equal(30, snapped.Rotation, 6);
    }

    [Fact]
    public void Rotate_Group_TurnsPositionsAroundTheFrameCenter_AndAddsToEachRotation()
    {
        var (shapes, frame) = Group(rotationOfSecond: 10);
        var (sx, sy) = SelectionGeometry.HandlePosition(frame, HandleType.Rotate, 100);

        var result = SelectionTransforms.Rotate(shapes, frame, sx, sy, frame.CenterX + 500, frame.CenterY, snap: false);

        Assert.Equal(90, result[0].Rotation, 6);
        Assert.Equal(100, result[1].Rotation, 6);
        // A quarter turn clockwise: a shape left of the center ends up above it.
        Assert.True(result[0].CenterX > shapes[0].CenterX - 1 && result[0].CenterY < frame.CenterY);
        var distanceBefore = Math.Sqrt(Math.Pow(shapes[0].CenterX - frame.CenterX, 2) + Math.Pow(shapes[0].CenterY - frame.CenterY, 2));
        var distanceAfter = Math.Sqrt(Math.Pow(result[0].CenterX - frame.CenterX, 2) + Math.Pow(result[0].CenterY - frame.CenterY, 2));
        Assert.Equal(distanceBefore, distanceAfter, 6);
    }

    [Fact]
    public void Rotate_Group_SnapsTheMovement()
    {
        var (shapes, frame) = Group();
        var (sx, sy) = SelectionGeometry.HandlePosition(frame, HandleType.Rotate, 100);
        var radians = 22 * Math.PI / 180;

        var result = SelectionTransforms.Rotate(shapes, frame, sx, sy, frame.CenterX + 100 * Math.Sin(radians), frame.CenterY - 100 * Math.Cos(radians), snap: true);

        Assert.Equal(15, result[0].Rotation, 6);
    }

    [Fact]
    public void Rotate_PointerAtTheCenterOrNonFinite_ReturnsTheSameList()
    {
        var (shapes, frame) = Single();

        Assert.Same(shapes, SelectionTransforms.Rotate(shapes, frame, 0, 0, frame.CenterX, frame.CenterY, false));
        Assert.Same(shapes, SelectionTransforms.Rotate(shapes, frame, 0, 0, double.NaN, 1, false));
    }

    // ---- move and setters ----

    [Fact]
    public void Move_MovesEveryShape_AndIgnoresNonFiniteDistances()
    {
        var (shapes, _) = Group();

        var moved = SelectionTransforms.Move(shapes, 5, -7);

        Assert.Equal(new[] { 5.0, 205 }, moved.Select(s => s.X));
        Assert.Equal(new[] { -7.0, 93 }, moved.Select(s => s.Y));
        Assert.Same(shapes, SelectionTransforms.Move(shapes, double.NaN, 0));
    }

    [Fact]
    public void SetPosition_Single_MovesTheUnturnedBoxToXY()
    {
        var (shapes, frame) = Single(45);

        var result = SelectionTransforms.SetPosition(shapes, frame, 10, 20)[0];

        Assert.Equal(10, result.X, 6);
        Assert.Equal(20, result.Y, 6);
        Assert.Equal(45, result.Rotation, 6);
    }

    [Fact]
    public void SetPosition_OnlyOneCoordinate_KeepsTheOther()
    {
        var (shapes, frame) = Single();

        var result = SelectionTransforms.SetPosition(shapes, frame, null, 20)[0];

        Assert.Equal(100, result.X, 6);
        Assert.Equal(20, result.Y, 6);
    }

    [Fact]
    public void SetPosition_Group_MovesTheBoxAroundThemAndKeepsTheLayout()
    {
        var (shapes, frame) = Group();

        var result = SelectionTransforms.SetPosition(shapes, frame, 50, 60);
        var after = SelectionGeometry.FrameOf(result)!;

        Assert.Equal(50, after.CenterX - after.Width / 2, 6);
        Assert.Equal(60, after.CenterY - after.Height / 2, 6);
        Assert.Equal(200, result[1].X - result[0].X, 6);
    }

    [Fact]
    public void SetSize_Single_KeepsTheTopLeftCorner()
    {
        var (shapes, frame) = Single();

        var result = SelectionTransforms.SetSize(shapes, frame, 50, null, false)[0];

        Assert.Equal((100, 100, 50, 100), (result.X, result.Y, result.Width, result.Height));
    }

    [Fact]
    public void SetSize_Single_Rotated_KeepsTheTopLeftCornerOfTheTurnedBox()
    {
        var (shapes, frame) = Single(90);
        var before = Corner(shapes[0], -1, -1);

        var result = SelectionTransforms.SetSize(shapes, frame, 100, 40, false)[0];

        Near(Corner(result, -1, -1), before);
        Assert.Equal(100, result.Width, 6);
        Assert.Equal(40, result.Height, 6);
    }

    [Fact]
    public void SetSize_KeepProportions_UsesTheRatioOfTheFrame()
    {
        var (shapes, frame) = Single();

        var byWidth = SelectionTransforms.SetSize(shapes, frame, 100, null, true)[0];
        var byHeight = SelectionTransforms.SetSize(shapes, frame, null, 200, true)[0];

        Assert.Equal((100, 50), (byWidth.Width, byWidth.Height));
        Assert.Equal((400, 200), (byHeight.Width, byHeight.Height));
    }

    [Fact]
    public void SetSize_Group_ScalesAboutTheTopLeftOfTheFrame()
    {
        var (shapes, frame) = Group();

        var result = SelectionTransforms.SetSize(shapes, frame, 600, null, false);

        Assert.Equal((0, 0, 200, 100), (result[0].X, result[0].Y, result[0].Width, result[0].Height));
        Assert.Equal((400, 100, 200, 100), (result[1].X, result[1].Y, result[1].Width, result[1].Height));
    }

    [Fact]
    public void SetSize_ZeroNegativeAndNonFinite()
    {
        var (shapes, frame) = Single();

        Assert.Equal(0, SelectionTransforms.SetSize(shapes, frame, 0, null, false)[0].Width);
        Assert.Equal(0, SelectionTransforms.SetSize(shapes, frame, -5, null, false)[0].Width);
        Assert.Same(shapes, SelectionTransforms.SetSize(shapes, frame, double.NaN, null, false));
    }

    [Fact]
    public void SetSize_FromZeroWidth_CanGrowASingleShape()
    {
        IReadOnlyList<Shape> shapes = [Rect(w: 0)];
        var frame = SelectionGeometry.FrameOf(shapes)!;

        var result = SelectionTransforms.SetSize(shapes, frame, 80, null, false)[0];

        Assert.Equal(80, result.Width);
        Assert.Equal(100, result.X, 6);
    }

    [Fact]
    public void SetRotation_Single_SetsTheAngle_Normalized()
    {
        var (shapes, frame) = Single();

        Assert.Equal(30, SelectionTransforms.SetRotation(shapes, frame, 30)[0].Rotation, 6);
        Assert.Equal(350, SelectionTransforms.SetRotation(shapes, frame, -10)[0].Rotation, 6);
        Assert.Equal(0, SelectionTransforms.SetRotation(shapes, frame, 360)[0].Rotation, 6);
        Assert.Same(shapes, SelectionTransforms.SetRotation(shapes, frame, double.NaN));
    }

    [Fact]
    public void SetRotation_Group_TurnsTheGroupByTheAngle()
    {
        var (shapes, frame) = Group();

        var result = SelectionTransforms.SetRotation(shapes, frame, 90);

        Assert.Equal(new[] { 90.0, 90 }, result.Select(s => s.Rotation));
        Assert.Equal(frame.CenterX, SelectionGeometry.FrameOf(result)!.CenterX, 6);
    }

    [Fact]
    public void ResetRotation_ClearsEveryShapesRotationAroundItsOwnCenter()
    {
        IReadOnlyList<Shape> shapes = [Rect(rotation: 40), Rect(x: 500, rotation: 200)];

        var result = SelectionTransforms.ResetRotation(shapes);

        Assert.All(result, s => Assert.Equal(0, s.Rotation));
        Assert.Equal(shapes.Select(s => s.CenterX), result.Select(s => s.CenterX));
    }

    // ---- robustness ----

    [Fact]
    public void RandomOperations_KeepEveryNumberFiniteAndSizesNotNegative()
    {
        var random = new Random(12345);
        var handles = Enum.GetValues<HandleType>();
        for (var run = 0; run < 300; run++)
        {
            IReadOnlyList<Shape> shapes = Enumerable.Range(0, random.Next(1, 4))
                .Select(_ => (Shape)Rect(random.NextDouble() * 500, random.NextDouble() * 500, random.NextDouble() * 300, random.NextDouble() * 300, random.Next(0, 360)))
                .ToList();
            var frame = SelectionGeometry.FrameOf(shapes)!;
            var handle = handles[random.Next(handles.Length)];
            var px = (random.NextDouble() - 0.5) * 2000;
            var py = (random.NextDouble() - 0.5) * 2000;

            var results = new[]
            {
                SelectionTransforms.Resize(shapes, frame, handle, px, py, random.Next(2) == 0, random.Next(2) == 0),
                SelectionTransforms.Rotate(shapes, frame, 1, 1, px, py, random.Next(2) == 0),
                SelectionTransforms.SetSize(shapes, frame, random.NextDouble() * 400 - 50, random.NextDouble() * 400 - 50, random.Next(2) == 0),
                SelectionTransforms.SetRotation(shapes, frame, (random.NextDouble() - 0.5) * 1000),
                SelectionTransforms.SetPosition(shapes, frame, px, py),
            };

            foreach (var result in results)
            {
                Assert.Equal(shapes.Count, result.Count);
                Assert.All(result, s =>
                {
                    Assert.True(double.IsFinite(s.X) && double.IsFinite(s.Y) && double.IsFinite(s.Width) && double.IsFinite(s.Height));
                    Assert.True(s.Width >= 0 && s.Height >= 0);
                    Assert.InRange(s.Rotation, 0, 360);
                });
                Assert.Equal(shapes.Select(s => s.Id), result.Select(s => s.Id));
                Assert.All(result, s => Assert.Equal(0.5, s.Appearance.Opacity));
            }
        }
    }

    [Fact]
    public void NullArguments_Throw()
    {
        var (shapes, frame) = Single();

        Assert.Throws<ArgumentNullException>(() => SelectionTransforms.Resize(null!, frame, HandleType.Right, 0, 0, false, false));
        Assert.Throws<ArgumentNullException>(() => SelectionTransforms.Resize(shapes, null!, HandleType.Right, 0, 0, false, false));
        Assert.Throws<ArgumentNullException>(() => SelectionTransforms.Move(null!, 0, 0));
        Assert.Throws<ArgumentNullException>(() => SelectionTransforms.ResetRotation(null!));
    }
}
