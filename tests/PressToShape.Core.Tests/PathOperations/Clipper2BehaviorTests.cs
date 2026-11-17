using System;
using System.Linq;
using System.Reflection;
using Clipper2Lib;
using Xunit;
using ClipperFillRule = Clipper2Lib.FillRule;

namespace PressToShape.Core.Tests.PathOperations;

/// <summary>
/// Documents how the Clipper2 package behaves for the boolean operations (spec boolean-operations, task 2).
/// Finding about Z: see <see cref="PointD_HasNoZValue_SoSourceTagsAreMatchedByPosition"/>. The NuGet build of 1.5.4 is compiled without
/// USINGZ, so a point has no Z value and there is no Z callback. The engine therefore matches result points to the flattened source
/// points and source edges by position (the fallback of the design).
/// </summary>
public class Clipper2BehaviorTests
{
    private const int Precision = 4;

    private static PathsD Square(double x, double y, double size) =>
        [new PathD([new PointD(x, y), new PointD(x + size, y), new PointD(x + size, y + size), new PointD(x, y + size)])];

    private static double TotalArea(PathsD paths) => paths.Sum(Clipper.Area);

    [Fact]
    public void PointD_HasNoZValue_SoSourceTagsAreMatchedByPosition()
    {
        var fields = typeof(PointD).GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name);
        var properties = typeof(PointD).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name);

        Assert.DoesNotContain(fields.Concat(properties), name => string.Equals(name, "z", StringComparison.OrdinalIgnoreCase));
        Assert.Null(typeof(ClipperD).GetMethod("set_ZCallback") ?? typeof(ClipperD).GetProperty("ZCallback")?.SetMethod);
    }

    [Fact]
    public void Union_OfOverlappingSquares_GivesOneOutlineWithTheUnionArea()
    {
        var result = Clipper.Union(Square(0, 0, 10), Square(5, 5, 10), ClipperFillRule.NonZero, Precision);

        Assert.Single(result);
        Assert.Equal(175, Math.Abs(TotalArea(result)), 6);
    }

    [Fact]
    public void Difference_OfSquareInsideSquare_GivesOuterAndHoleWithOppositeWinding()
    {
        var result = Clipper.Difference(Square(0, 0, 10), Square(3, 3, 4), ClipperFillRule.NonZero, Precision);

        Assert.Equal(2, result.Count);
        Assert.Equal(84, Math.Abs(TotalArea(result)), 6);
        Assert.NotEqual(Math.Sign(Clipper.Area(result[0])), Math.Sign(Clipper.Area(result[1])));
    }

    [Fact]
    public void Intersect_OfOverlappingSquares_GivesTheOverlap()
    {
        var result = Clipper.Intersect(Square(0, 0, 10), Square(5, 5, 10), ClipperFillRule.NonZero, Precision);

        Assert.Single(result);
        Assert.Equal(25, Math.Abs(TotalArea(result)), 6);
    }

    [Fact]
    public void Xor_OfOverlappingSquares_RemovesTheOverlap()
    {
        var result = Clipper.Xor(Square(0, 0, 10), Square(5, 5, 10), ClipperFillRule.NonZero, Precision);

        Assert.Equal(150, result.Sum(path => Math.Abs(Clipper.Area(path))), 6);
    }

    [Fact]
    public void Intersect_OfDisjointSquares_IsEmpty()
    {
        var result = Clipper.Intersect(Square(0, 0, 10), Square(20, 0, 10), ClipperFillRule.NonZero, Precision);

        Assert.Empty(result);
    }

    [Fact]
    public void Union_OfDisjointSquares_GivesTwoOutlines()
    {
        var result = Clipper.Union(Square(0, 0, 10), Square(20, 0, 10), ClipperFillRule.NonZero, Precision);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Union_OfSquaresSharingAnEdge_MergesThem()
    {
        var result = Clipper.Union(Square(0, 0, 10), Square(10, 0, 10), ClipperFillRule.NonZero, Precision);

        Assert.Single(result);
        Assert.Equal(200, Math.Abs(TotalArea(result)), 6);
    }

    [Fact]
    public void Difference_OfCoveredSquare_IsEmpty()
    {
        var result = Clipper.Difference(Square(2, 2, 4), Square(0, 0, 10), ClipperFillRule.NonZero, Precision);

        Assert.Empty(result);
    }

    [Fact]
    public void Union_RunTwice_GivesTheSameResult()
    {
        var first = Clipper.Union(Square(0, 0, 10), Square(5, 5, 10), ClipperFillRule.NonZero, Precision);
        var second = Clipper.Union(Square(0, 0, 10), Square(5, 5, 10), ClipperFillRule.NonZero, Precision);

        Assert.Equal(first.Select(p => p.ToList()), second.Select(p => p.ToList()));
    }
}
