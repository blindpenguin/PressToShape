using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasSceneRoundedTests
{
    private static ScreenShape Build(Shape shape, double zoomPercent = 100) => CanvasScene.BuildShape(shape, zoomPercent, 0, 0);

    [Fact]
    public void BuildShape_SquareRectangle_HasNoCorners()
    {
        var screen = Build(new RectangleShape { Width = 100, Height = 50 });

        Assert.Null(screen.Corners);
    }

    [Fact]
    public void BuildShape_RoundedRectangle_ScalesTheRadiiByTheZoom()
    {
        var screen = Build(new RectangleShape { Width = 100, Height = 50, Radii = new CornerRadii(1, 2, 3, 4) }, 200);

        Assert.Equal(new CornerRadii(2, 4, 6, 8), screen.Corners);
        Assert.Equal(200, screen.Width);
    }

    [Fact]
    public void BuildShape_RadiusAboveTheLimit_IsPassedLimited()
    {
        var screen = Build(new RectangleShape { Width = 100, Height = 50, Radii = CornerRadii.Uniform(500) }, 50);

        Assert.Equal(CornerRadii.Uniform(12.5), screen.Corners);
    }

    [Fact]
    public void BuildShape_FlatRectangle_HasNoCorners()
    {
        var screen = Build(new RectangleShape { Width = 100, Height = 0, Radii = CornerRadii.Uniform(5) });

        Assert.Null(screen.Corners);
    }

    [Fact]
    public void BuildShape_EllipseAndPath_HaveNoCorners()
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]);

        Assert.Null(Build(new EllipseShape { Width = 10, Height = 10 }).Corners);
        Assert.Null(Build(path).Corners);
    }

    [Fact]
    public void BuildShape_RotatedRoundedRectangle_KeepsTheRotationAndTheRadii()
    {
        var screen = Build(new RectangleShape { Width = 100, Height = 50, Rotation = 30, Radii = CornerRadii.Uniform(10) });

        Assert.Equal(30, screen.RotationDegrees);
        Assert.Equal(CornerRadii.Uniform(10), screen.Corners);
    }
}
